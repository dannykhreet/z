using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Services.Utils
{
    public class MediaService : IMediaService
    {
        private readonly IFileService _fileService;
        private readonly IMediaResizer _mediaResizer;

        private const int mediaHeight = 1080;
        private const int mediaWidth = 1920;

        public MediaService()
        {
            _fileService = DependencyService.Get<IFileService>();
            _mediaResizer = DependencyService.Get<IMediaResizer>();
        }

        private async Task UploadPictureAsync(byte[] array, MediaItem mediaItem, MediaStorageTypeEnum mediaStorageType, int id)
        {
            string filename = Path.GetFileName(mediaItem.PictureUrl);

            mediaItem.IsLocalFile = false;

            var pictureUrl = await UploadPictureAsync(array, mediaStorageType, id, filename).ConfigureAwait(false);

            mediaItem.PictureUrl = pictureUrl ?? throw new ArgumentNullException(pictureUrl, $"There was a problem with uploading media file: {filename}");
        }

        public async Task<string> UploadPictureAsync(Stream media, MediaStorageTypeEnum mediaStorageType, int id, string filename)
        {
            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StreamContent(media), "file", filename);

            HttpClient httpClient = Statics.RetrieveFromCacheHttpClient;
            httpClient.BaseAddress = new Uri(Statics.ApiUrl);

            string url = $"media/image/upload/{(int)mediaStorageType}/{id}";
            string mediaUrl = null;

            // await RetryWrapper.ExecuteAsync(async () =>
            // {
            HttpResponseMessage response = await httpClient.PostAsync(url, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            mediaUrl = await response.Content.ReadAsJsonAsync<string>().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(mediaUrl))
                throw new ArgumentNullException(mediaUrl, $"There was a problem with uploading media file: {filename}");

            if (mediaUrl.Contains("media/"))
            {
                mediaUrl = mediaUrl.Replace("media/", string.Empty);
            }

            if (mediaUrl == "Can not process uploaded file.")
            {
                //Crashes.TrackError(new Exception("Picture failed to process"), new Dictionary<string, string>()
                //{
                //    { "stream file name", $"{(media is FileStream mdf ? mdf.Name : string.Empty)}" },
                //    { "filename", filename }
                //});
            }
            //});
            return mediaUrl;
        }

        private async Task<string> UploadPictureAsync(byte[] media, MediaStorageTypeEnum mediaStorageType, int id, string filename)
        {
            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(media), "file", filename);

            HttpClient httpClient = Statics.RetrieveFromCacheHttpClient;
            httpClient.BaseAddress = new Uri(Statics.ApiUrl);

            string url = $"media/image/upload/{(int)mediaStorageType}/{id}";
            string mediaUrl = null;

            await RetryWrapper.ExecuteAsync(async () =>
            {
                HttpResponseMessage response = await httpClient.PostAsync(url, content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                mediaUrl = await response.Content.ReadAsJsonAsync<string>().ConfigureAwait(false);

                if (Config.CurrentEnv == Env.PRODUCTION)
                    mediaUrl = mediaUrl.Replace("media/", "");

                if (string.IsNullOrWhiteSpace(mediaUrl))
                    throw new ArgumentNullException(mediaUrl, $"There was a problem with uploading media file: {filename}");

                Debug.WriteLine(response.Content, "[Image Helper]\n\t");

                if (mediaUrl == "Can not process uploaded file.")
                {
                    //Crashes.TrackError(new Exception("Picture failed to process"), new Dictionary<string, string>()
                    //{
                    //    { "stream file name", $"{(media is byte[] mdf ? filename : string.Empty)}" },
                    //    { "filename", filename }
                    //});
                }
            }).ConfigureAwait(false);

            return mediaUrl;
        }

        private async Task<string> UploadFileAsync(Stream mediaStream, MediaStorageTypeEnum mediaStorageType, int id, string filename, MediaTypeEnum mediaType, bool includeBaseUrlOnReturn = true)
        {
            mediaStream.Seek(0, SeekOrigin.Begin);
            MultipartFormDataContent content = new MultipartFormDataContent
            {
                { new StreamContent(mediaStream), "file", filename }
            };

            HttpClient httpClient = Statics.RetrieveFromCacheHttpClient;

            httpClient.BaseAddress = new Uri(Statics.ApiUrl);

            string urlPart = mediaType.ToString().ToLower();
            string url = $"media/{urlPart}/upload/{(int)mediaStorageType}/{id}";

            if (includeBaseUrlOnReturn)
                url += "?includebaseurlonreturn=true";

            string mediaUrl = null;

            await RetryWrapper.ExecuteAsync(async () =>
            {
                HttpResponseMessage response = await httpClient.PostAsync(url, content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                mediaUrl = await response.Content.ReadAsJsonAsync<string>().ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(mediaUrl))
                    throw new ArgumentNullException(mediaUrl, "There was a problem with uploading media file");

                Debug.WriteLine(response.Content, "[Image Helper]\n\t");

                if (mediaUrl == "Can not process uploaded file.")
                {
                    //Crashes.TrackError(new Exception("File failed to process"), new Dictionary<string, string>()
                    //{
                    //    { "stream file name", $"{(mediaStream is FileStream mdf ? mdf.Name : string.Empty)}" },
                    //    { "filename", filename }
                    //});
                }

            }).ConfigureAwait(false);
            return mediaUrl;
        }

        public async Task UploadMediaItemAsync(MediaItem mediaItem, MediaStorageTypeEnum mediaStorageType, int id, bool hasWatermark = false)
        {
            try
            {
                if (mediaItem.MediaFile != null)
                {
                    if (mediaItem.IsVideo)
                    {
                        var videoStream = mediaItem.MediaFile.GetStream();

                        string videoFilename = Path.GetFileName(mediaItem.MediaFile.Path);

                        var videoUrl = await UploadFileAsync(videoStream, mediaStorageType, id, videoFilename, MediaTypeEnum.Video).ConfigureAwait(false);
                        mediaItem.VideoUrl = videoUrl ?? throw new ArgumentNullException(videoUrl, $"There was a problem with uploading media file: {videoFilename}");

                        string thumbnailFilename = Path.GetFileName(mediaItem.PictureUrl);

                        var thumbnailStream = await _fileService.ReadFromInternalStorageAsBytesAsync(thumbnailFilename, Constants.ThumbnailsDirectory).ConfigureAwait(false);

                        var bytes = await ConvertStringToByteArray(thumbnailStream).ConfigureAwait(false);
                        var resizedImage = _mediaResizer.ResizeImage(bytes, mediaWidth, mediaHeight);
                        await UploadPictureAsync(resizedImage, mediaItem, mediaStorageType, id).ConfigureAwait(false);
                        _fileService.DeleteFile(thumbnailFilename, Constants.ThumbnailsDirectory);
                    }
                    else if (mediaItem.IsFile)
                    {
                        var fileStream = mediaItem.MediaFile.GetStream();
                        string fileFilename = Path.GetFileName(mediaItem.MediaFile.Path);
                        var fileUrl = await UploadFileAsync(fileStream, mediaStorageType, id, fileFilename, MediaTypeEnum.Docs, false).ConfigureAwait(false);
                        mediaItem.FileUrl = fileUrl ?? throw new ArgumentNullException(fileUrl, $"There was a problem with uploading media file: {fileFilename}");
                    }
                    else
                    {
                        Stream stream;
                        string pictureWithWatermarkPath = Path.GetFileName(mediaItem.PictureUrl);
                        if (hasWatermark)
                        {
                            stream = await AsyncAwaiter.AwaitResultAsync(pictureWithWatermarkPath, async () =>
                            {
                                return await _fileService.ReadFromInternalStorageAsBytesAsync(pictureWithWatermarkPath, Constants.PictureProofsDirectory).ConfigureAwait(false);
                            }).ConfigureAwait(false);
                        }
                        else
                        {
                            try
                            {
                                stream = mediaItem.MediaFile.GetStreamWithImageRotatedForExternalStorage();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[RotationError] Falling back to GetStream(): {ex.Message}");
                                stream = mediaItem.MediaFile.GetStream();
                            }
                        }

                        var bytes = await ConvertStringToByteArray(stream).ConfigureAwait(false);
                        var resizedImage = _mediaResizer.ResizeImage(bytes, mediaWidth, mediaHeight);
                        await UploadPictureAsync(resizedImage, mediaItem, mediaStorageType, id).ConfigureAwait(false);

                        if (hasWatermark)
                            _fileService.DeleteFile(pictureWithWatermarkPath, Constants.PictureProofsDirectory);
                    }

                    if (mediaItem.ShouldRemoveAfterUpload)
                        _fileService.DeleteFile(mediaItem.MediaFile.Path);
                }
            }
            catch (Exception e)
            {
                mediaItem.IsLocalFile = true;
                //Crashes.TrackError(e);
                Debug.WriteLine($"[MediaUploadError:] {e.Message}");
                throw;
            }
        }

        private async Task<byte[]> ConvertStringToByteArray(Stream stream)
        {
            if (stream == null)
                return new byte[0];

            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin); // Reset stream position

            using MemoryStream sr = new MemoryStream();
            await stream.CopyToAsync(sr).ConfigureAwait(false);
            return sr.ToArray();
        }

        public async Task UploadMediaItemsAsync(IEnumerable<MediaItem> mediaItems, MediaStorageTypeEnum mediaStorageType, int id)
        {
            List<Task> tasks = new List<Task>();

            foreach (MediaItem mediaItem in mediaItems)
            {
                tasks.Add(UploadMediaItemAsync(mediaItem, mediaStorageType, id));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
