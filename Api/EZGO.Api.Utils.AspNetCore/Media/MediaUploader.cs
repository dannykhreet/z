using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.Media
{
    /// <summary>
    /// MediaUploader; Media uploader is used for uploading media to the disk storage or s3 storage based on media type
    /// </summary>
    public class MediaUploader : IMediaUploader
    {
        private readonly ILogger _logger;
        private readonly IConfigurationHelper _confighelper;

        #region - constructor(s) -
        public MediaUploader(IConfigurationHelper configurationHelper, ILogger<MediaUploader> logger)
        {
            _logger = logger;
            _confighelper = configurationHelper;
        }
        #endregion

        /// <summary>
        /// UploadFileAsync; Upload a file, depending on type of storage the upload will be to S3 or Disk.
        /// </summary>
        /// <param name="file">File that is being uploaded.</param>
        /// <param name="storageType">StorageTypeEnum, determen the upload to S3 or Disk</param>
        /// <param name="mediaType">Type of media (video, image)</param>
        /// <param name="storageLocationPartType">Storage location type, based on the EZ object types (task, checklist etc). Will determan url build up.</param>
        /// <param name="companyId">CompanyId (DB:companies_company.id)</param>
        /// <param name="objectId">The object id, kan be id of checkist, audit, task etc.</param>
        /// <returns>Url part for storage or use later on. </returns>
        public async Task<string> UploadFileAsync(IFormFile file, StorageTypeEnum storageType, MediaTypeEnum mediaType, MediaStorageTypeEnum mediaStorageType, int? companyId = null, int? objectId = null, bool includeBaseUrlOnReturn = false)
        {
            if (file != null && file.Length > 0)
            {
                if (storageType == StorageTypeEnum.AwsS3)
                {
                    return await UploadS3Storage(file: file, mediaStorageType: mediaStorageType, mediaType: mediaType, companyId: companyId, objectId: objectId, includeBaseUrlOnReturn: includeBaseUrlOnReturn);
                }
                else if (storageType == StorageTypeEnum.AwsS3AndDisk)
                {
                    string output = "";
                    if (_confighelper.GetValueAsBool("AppSettings:EnableDiskUpload"))
                    {
                        output = await UploadDiskStorage(file: file, mediaStorageType: mediaStorageType, mediaType: mediaType, companyId: companyId, objectId: objectId, includeBaseUrlOnReturn: includeBaseUrlOnReturn);
                    }
                    if (_confighelper.GetValueAsBool("AppSettings:EnableImageAwsAndDiskUpload"))
                    {
                        var outputS3 = await UploadS3Storage(file: file, mediaStorageType: mediaStorageType, mediaType: mediaType, companyId: companyId, objectId: objectId, includeBaseUrlOnReturn: includeBaseUrlOnReturn, path: output);
                        if (string.IsNullOrEmpty(outputS3))
                        {
                            //write to log, for now load normal one
                        }
                        else
                        {
                            output = outputS3;
                        }
                    }

                    return output;
                }
                else
                {
                    return await UploadDiskStorage(file: file, mediaStorageType: mediaStorageType, mediaType: mediaType, companyId: companyId, objectId: objectId, includeBaseUrlOnReturn: includeBaseUrlOnReturn);
                }
            }
            return String.Empty;
        }

        public async Task<string> CopyFileAsync(string sourceFileKey, MediaTypeEnum mediaType, MediaStorageTypeEnum mediaStorageType, int? companyId = null, int? objectId = null, bool includeBaseUrlOnReturn = false)
        {
            if (!string.IsNullOrEmpty(sourceFileKey))
                return await CopyFileInS3Storage(sourceFileKey: sourceFileKey, mediaStorageType: mediaStorageType, mediaType: mediaType, includeBaseUrlOnReturn: includeBaseUrlOnReturn, companyId: companyId, objectId: objectId);
            
            return string.Empty;
        }

        /// <summary>
        /// MoveFileToStructures; Move file to correct structured location.
        /// </summary>
        /// <param name="file">File that is being uploaded.</param>
        /// <param name="storageType">StorageTypeEnum, determan the upload to S3 or Disk</param>
        /// <param name="mediaType">Type of media (video, image)</param>
        /// <param name="storageLocationPartType">Storage location type, based on the EZ object types (task, checklist etc). Will determan url build up.</param>
        /// <param name="companyId">CompanyId (DB:companies_company.id)</param>
        /// <param name="objectId">The object id, kan be id of checkist, audit, task etc.</param>
        /// <returns>Url part for storage or use later on. </returns>
        public async Task<string> MoveFileToStructuredLocation(string currentFile, StorageTypeEnum storageType, MediaTypeEnum mediaType, MediaStorageTypeEnum mediaStorageType, int? companyId = null, int? objectId = null, bool includeBaseUrlOnReturn = false)
        {
            if (storageType == StorageTypeEnum.Disk)
            {
                var fileName = Path.GetFileName(currentFile);
                var newFileLocation = GetAndPrepareFileLocation(fileName: fileName, mediaStorageType: mediaStorageType, companyId: companyId, objectId: objectId, useCreateDirectory: true);
                MoveFileOnDisk(fromFile: currentFile, toFile: newFileLocation);
                return newFileLocation;
            }
            await Task.CompletedTask;
            return string.Empty;
        }

        /// <summary>
        /// CheckFileForUnstructured; Check if file is not fully structured yet. If so it returns true.
        /// </summary>
        /// <param name="currentFile">File to be checked.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> CheckFileForUnstructured(string currentFile)
        {
            //check if file contains /0/; if so the file is saved at id 0 (when uploaded the id of the object was probably not available).
            if (currentFile.Contains("/0/"))
            {
                return true;
            }
            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// UploadDiskStorage; upload file to s3.
        /// </summary>
        /// <param name="file">File that is being uploaded.</param>
        /// <param name="mediaStorageType">Kind of file that is being uploaded, depending on type specific upload structure will be used.</param>
        /// <param name="includeBaseUrlOnReturn">Include the base URL when outputting the URL to the uploaded item.</param>
        /// <param name="companyId">CompanyId (DB:companies_company.id)</param>
        /// <param name="objectId">The object id, can be id of checklist, audit, task etc.</param>
        /// <returns>Url (string) to the uploaded file.</returns>
        private async Task<string> UploadS3Storage(IFormFile file, MediaStorageTypeEnum mediaStorageType, MediaTypeEnum mediaType, bool includeBaseUrlOnReturn, int? companyId = null, int? objectId = null, string path = "")
        {
            //clean path if needed
            if (path.StartsWith(_confighelper.GetValueAsString(MediaSettings.MEDIA_STORAGE_LOCATION_CONFIG_KEY)))
            {
                path = path.Replace(string.Concat(_confighelper.GetValueAsString(MediaSettings.MEDIA_STORAGE_LOCATION_CONFIG_KEY), "/"), "");
            }
            CleanPath(path);
            return await SaveFileToS3(file: file, mediaStorageType: mediaStorageType, mediaType: mediaType, companyId: companyId, objectId: objectId, includeBaseUrlOnReturn: includeBaseUrlOnReturn, path: path);
        }

        /// <summary>
        /// Copy file in S3 storage
        /// </summary>
        /// <param name="sourceFileKey">Key to the original file</param>
        /// <param name="mediaStorageType">What type of object the media is related to</param>
        /// <param name="mediaType">Kind of file that is being uploaded</param>
        /// <param name="includeBaseUrlOnReturn">Includes the base of the URL of the copy</param>
        /// <param name="companyId">The comompany id of the company for which the copy is being made</param>
        /// <param name="objectId">Id of the object the copy belongs to</param>
        /// <param name="path"></param>
        /// <returns>Key to the copy of the file</returns>
        private async Task<string> CopyFileInS3Storage(string sourceFileKey, MediaStorageTypeEnum mediaStorageType, MediaTypeEnum mediaType, bool includeBaseUrlOnReturn, int? companyId = null, int? objectId = null, string path = "")
        {
            //clean path if needed
            if (path.StartsWith(_confighelper.GetValueAsString(MediaSettings.MEDIA_STORAGE_LOCATION_CONFIG_KEY)))
            {
                path = path.Replace(string.Concat(_confighelper.GetValueAsString(MediaSettings.MEDIA_STORAGE_LOCATION_CONFIG_KEY), "/"), "");
            }
            CleanPath(path);
            return await CopyFileOnS3(sourceFileKey: sourceFileKey, mediaStorageType: mediaStorageType, mediaType: mediaType, companyId: companyId, objectId: objectId, includeBaseUrlOnReturn: includeBaseUrlOnReturn, path: path);
        }

        /// <summary>
        /// UploadDiskStorage; upload file to disk.
        /// </summary>
        /// <param name="file">File that is being uploaded.</param>
        /// <param name="mediaStorageType">Kind of file that is being uploaded, depending on type specific upload structure will be used.</param>
        /// <param name="includeBaseUrlOnReturn">Include the base URL when outputting the URL to the uploaded item.</param>
        /// <param name="companyId">CompanyId (DB:companies_company.id)</param>
        /// <param name="objectId">The object id, can be id of checklist, audit, task etc.</param>
        /// <returns>Url (string) to the uploaded file.</returns>
        private async Task<string> UploadDiskStorage(IFormFile file, MediaStorageTypeEnum mediaStorageType, MediaTypeEnum mediaType, bool includeBaseUrlOnReturn, int? companyId = null, int? objectId = null)
        {
            //TODO add checks and related items.
            return await SaveFileToDisk(file: file, mediaStorageType: mediaStorageType, companyId: companyId, objectId: objectId, includeBaseUrlOnReturn: includeBaseUrlOnReturn);
        }

        /// <summary>
        /// SaveFileToDisk; Saves a file to physical disk. File directory structure is based on the id's of the company, object and type of upload.
        /// </summary>
        /// <param name="file">File that is being uploaded.</param>
        /// <param name="mediaStorageType">StorageTypeEnum, determen the upload to S3 or Disk</param>
        /// <param name="companyId">CompanyId (DB:companies_company.id)</param>
        /// <param name="objectId">The object id, kan be id of checkist, audit, task etc.</param>
        /// <param name="includeBaseUrlOnReturn">include full url including the domain/host and https or only return the default. </param>
        /// <returns>Url after upload to the uploaded file.</returns>
        private async Task<string> SaveFileToDisk(IFormFile file, MediaStorageTypeEnum mediaStorageType, bool includeBaseUrlOnReturn, int? companyId = null, int? objectId = null)
        {
            var fullPath = GetAndPrepareFileLocation(file: file, mediaStorageType: mediaStorageType, companyId: companyId, objectId: objectId, useCreateDirectory: true, useMediaDiskStorageLocation: true);

            try
            {
                using (var stream = System.IO.File.Create(fullPath))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"Error occurred {nameof(MediaUploader)}.{nameof(SaveFileToDisk)}()");
            }

            return fullPath;
        }

        /// <summary>
        /// SaveFileToS3; Save file to S3 storage
        /// </summary>
        /// <param name="file">File that is being uploaded.</param>
        /// <param name="mediaStorageType">StorageTypeEnum, determen the upload to S3 or Disk</param>
        /// <param name="companyId">CompanyId (DB:companies_company.id)</param>
        /// <param name="objectId">The object id, kan be id of checkist, audit, task etc.</param>
        /// <param name="includeBaseUrlOnReturn">include full url including the domain/host and https or only return the default. </param>
        /// <returns>Url after upload to the uploaded file.</returns>
        private async Task<string> SaveFileToS3(IFormFile file, MediaStorageTypeEnum mediaStorageType, MediaTypeEnum mediaType, bool includeBaseUrlOnReturn, int? companyId = null, int? objectId = null, string path = "")
        {
            var fullPath = string.IsNullOrEmpty(path) ? GetAndPrepareFileLocation(file: file, mediaStorageType: mediaStorageType, companyId: companyId, objectId: objectId, useMediaDiskStorageLocation: false) : path;

            await using (var tempMemoryStream = new MemoryStream())
            {
                try
                {

                    var bucketName = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_BUCKETNAME_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_BUCKETNAME_CONFIG_KEY);
                    var accesskey = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_ACCESSKEY_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_ACCESSKEY_CONFIG_KEY);
                    var secretaccesskey = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_SECRETACCESSKEY_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_SECRETACCESSKEY_CONFIG_KEY);
                    var bucketRegionUrl = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_BUCKETREGIONURL_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_BUCKETREGIONURL_CONFIG_KEY);

                    if (_confighelper.GetValueAsBool(MediaSettings.ENABLE_MEDIA_UPLOAD_CONFIG_KEY))
                    {
                        _logger.LogInformation("Connection Information: {0} {1} {2} {3}", string.IsNullOrEmpty(bucketName) ? "EMPTY" : bucketName, string.IsNullOrEmpty(bucketRegionUrl) ? "EMPTY" : bucketRegionUrl, string.IsNullOrEmpty(accesskey) ? "EMPTY" : accesskey.Substring(0, 5), string.IsNullOrEmpty(secretaccesskey) ? "EMPTY" : secretaccesskey.Substring(0, 5));
                    }

                    //Client connects to bucket and serve data from and to : https://ezfactorytestmediastorage.s3.eu-central-1.amazonaws.com
                    using (var s3client = new AmazonS3Client(awsAccessKeyId: accesskey, awsSecretAccessKey: secretaccesskey, Amazon.RegionEndpoint.EUCentral1))
                    {

                        file.CopyTo(tempMemoryStream);

                        var fileTransferUtility = new TransferUtility(s3client);
                        var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                        {
                            BucketName = bucketName,
                            Key = fullPath,
                            InputStream = tempMemoryStream
                        };
                        await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);

                        return includeBaseUrlOnReturn ? string.Format("{0}{1}", bucketRegionUrl, fullPath) : fullPath;
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: $"Error occurred {nameof(MediaUploader)}.{nameof(SaveFileToS3)}(): {ex.Message}");
                }
            }

            return "";
        }

        /// <summary>
        /// MoveFileOnDisk; Moves a file from a location to another location on disk.
        /// </summary>
        /// <param name="fromFile">From location (file / path)</param>
        /// <param name="toFile">To location (file / path)</param>
        /// <returns>return true/false depending on outcome.</returns>
        private bool MoveFileOnDisk(string fromFile, string toFile)
        {
            try
            {
                if (File.Exists(fromFile))
                {
                    File.Move(fromFile, toFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"Error occurred {nameof(MediaUploader)}.{nameof(MoveFileOnDisk)}()");
                return false;
            }
            return true;
        }

        /// <summary>
        /// MoveFileOnS3; Moves a file from a location to another location on S3.
        /// </summary>
        /// <param name="fromFile">From location (file / path)</param>
        /// <param name="toFile">To location (file / path)</param>
        /// <returns>return true/false depending on outcome.</returns>
        private bool MoveFileOnS3(string fromFile, string toFile)
        {
            //TODO fill
            return true;
        }

        /// <summary>
        /// Copy a file on S3 storage.
        /// </summary>
        /// <param name="sourceFileKey">Key to the original file</param>
        /// <param name="mediaStorageType">Used to determine key name for copy</param>
        /// <param name="mediaType">Used to determine the bucket</param>
        /// <param name="includeBaseUrlOnReturn">Returns full url when true. Otherwise only the key to the copy will be returned.</param>
        /// <param name="companyId">Company id of the company for the copy. Used in key</param>
        /// <param name="objectId">Object id, used in key</param>
        /// <param name="path"></param>
        /// <returns>Path to copy of file. (only the key, or optionally the full url)</returns>
        private async Task<string> CopyFileOnS3(string sourceFileKey, MediaStorageTypeEnum mediaStorageType, MediaTypeEnum mediaType, bool includeBaseUrlOnReturn, int? companyId = null, int? objectId = null, string path = "")
        {
            string fileNameForCopy = $"{Guid.NewGuid()}{Path.GetExtension(sourceFileKey)}";
            var fullPathOfCopy = string.IsNullOrEmpty(path) ? GetAndPrepareFileLocation(fileName: fileNameForCopy, mediaStorageType: mediaStorageType, companyId: companyId, objectId: objectId, useMediaDiskStorageLocation: false) : path;

            try
            {
                var bucketName = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_BUCKETNAME_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_BUCKETNAME_CONFIG_KEY);
                var accesskey = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_ACCESSKEY_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_ACCESSKEY_CONFIG_KEY);
                var secretaccesskey = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_SECRETACCESSKEY_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_SECRETACCESSKEY_CONFIG_KEY);
                var bucketRegionUrl = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_BUCKETREGIONURL_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_BUCKETREGIONURL_CONFIG_KEY);

                //if path is already in name of the file, remove the path
                int index = sourceFileKey.IndexOf(bucketRegionUrl);
                if (index >= 0)
                {
                    sourceFileKey = sourceFileKey.Remove(index, bucketRegionUrl.Length);
                }

                if (_confighelper.GetValueAsBool(MediaSettings.ENABLE_MEDIA_UPLOAD_CONFIG_KEY))
                {
                    _logger.LogInformation("Connection Information: {0} {1} {2} {3}", string.IsNullOrEmpty(bucketName) ? "EMPTY" : bucketName, string.IsNullOrEmpty(bucketRegionUrl) ? "EMPTY" : bucketRegionUrl, string.IsNullOrEmpty(accesskey) ? "EMPTY" : accesskey.Substring(0, 5), string.IsNullOrEmpty(secretaccesskey) ? "EMPTY" : secretaccesskey.Substring(0, 5));
                }

                //Client connects to bucket and serve data from and to : https://ezfactorytestmediastorage.s3.eu-central-1.amazonaws.com
                using (var s3client = new AmazonS3Client(awsAccessKeyId: accesskey, awsSecretAccessKey: secretaccesskey, Amazon.RegionEndpoint.EUCentral1))
                {
                    CopyObjectRequest copyObjectRequest = new()
                    {
                        SourceBucket = bucketName,
                        SourceKey = sourceFileKey,
                        DestinationBucket = bucketName,
                        DestinationKey = fullPathOfCopy
                    };
                    CopyObjectResponse response = await s3client.CopyObjectAsync(copyObjectRequest);

                    return includeBaseUrlOnReturn ? string.Format("{0}{1}", bucketRegionUrl, fullPathOfCopy) : fullPathOfCopy;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"Error occurred {nameof(MediaUploader)}.{nameof(CopyFileOnS3)}(): {ex.Message}");
            }

            return "";
        }

        /// <summary>
        /// Deletes a file from S3 based on the key and media type
        /// </summary>
        /// <param name="keyName">Key of the file being deleted. (Filename without path).</param>
        /// <param name="mediaType">Type of media to be deleted</param>
        /// <returns>true if delete action was succesful</returns>
        public async Task<bool> DeleteFileFromS3Async(string keyName, MediaTypeEnum mediaType)
        {
            try
            {
                var bucketName = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_BUCKETNAME_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_BUCKETNAME_CONFIG_KEY);
                var accesskey = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_ACCESSKEY_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_ACCESSKEY_CONFIG_KEY);
                var secretaccesskey = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_SECRETACCESSKEY_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_SECRETACCESSKEY_CONFIG_KEY);
                var bucketRegionUrl = mediaType == MediaTypeEnum.Image || mediaType == MediaTypeEnum.Docs ? _confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_BUCKETREGIONURL_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_BUCKETREGIONURL_CONFIG_KEY);

                //if path is already in name of the file, remove the path
                int index = keyName.IndexOf(bucketRegionUrl);
                if (index >= 0)
                {
                    keyName = keyName.Remove(index, bucketRegionUrl.Length);
                }

                if (_confighelper.GetValueAsBool(MediaSettings.ENABLE_MEDIA_UPLOAD_CONFIG_KEY))
                {
                    _logger.LogInformation("Connection Information: {0} {1} {2} {3}", string.IsNullOrEmpty(bucketName) ? "EMPTY" : bucketName, string.IsNullOrEmpty(bucketRegionUrl) ? "EMPTY" : bucketRegionUrl, string.IsNullOrEmpty(accesskey) ? "EMPTY" : accesskey.Substring(0, 5), string.IsNullOrEmpty(secretaccesskey) ? "EMPTY" : secretaccesskey.Substring(0, 5));
                }
                using (var s3client = new AmazonS3Client(awsAccessKeyId: accesskey, awsSecretAccessKey: secretaccesskey, Amazon.RegionEndpoint.EUCentral1))
                {
                    var deleteObjectRequest = new DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = keyName
                    };

                    DeleteObjectResponse response = await s3client.DeleteObjectAsync(deleteObjectRequest);

                    if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"Error occurred {nameof(MediaUploader)}.{nameof(DeleteFileFromS3Async)}(): {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// GetAndPrepareFileLocation; Get file location and prepare the directory structure if needed.
        /// </summary>
        /// <param name="file">File that is being uploaded.</param>
        /// <param name="mediaStorageType">StorageTypeEnum, determen the upload to S3 or Disk</param>
        /// <param name="companyId">CompanyId (DB:companies_company.id)</param>
        /// <param name="objectId">The object id, kan be id of checkist, audit, task etc.</param>
        /// <param name="useCreateDirectory">Use create directory.</param>
        /// <returns>Full path of file including filename.</returns>
        private string GetAndPrepareFileLocation(IFormFile file, MediaStorageTypeEnum mediaStorageType, int? companyId = null, int? objectId = null, bool useCreateDirectory = false, bool useMediaDiskStorageLocation = true)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            return GetAndPrepareFileLocation(fileName: fileName, mediaStorageType: mediaStorageType, companyId: companyId, objectId: objectId, useCreateDirectory: useCreateDirectory, useMediaDiskStorageLocation: useMediaDiskStorageLocation);
        }

        /// <summary>
        /// GetAndPrepareFileLocation; Get file location and prepare the directory structure if needed.
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="mediaStorageType">StorageTypeEnum, determen the upload to S3 or Disk</param>
        /// <param name="companyId">CompanyId (DB:companies_company.id)</param>
        /// <param name="objectId">The object id, kan be id of checkist, audit, task etc.</param>
        /// <param name="useCreateDirectory">Use create directory.</param>
        /// <returns>Full path of file including filename.</returns>
        private string GetAndPrepareFileLocation(string fileName, MediaStorageTypeEnum mediaStorageType, int? companyId = null, int? objectId = null, bool useCreateDirectory = false, bool useMediaDiskStorageLocation = true)
        {
            var mediaLocation = _confighelper.GetValueAsString(MediaSettings.MEDIA_STORAGE_LOCATION_CONFIG_KEY);
            var directoryLocation = objectId.HasValue ?
                                    useMediaDiskStorageLocation ? $"{mediaLocation}/{companyId}/{mediaStorageType.ToStorageLocation()}/{objectId.Value}/" : $"{companyId}/{mediaStorageType.ToStorageLocation()}/{objectId.Value}/" :
                                    useMediaDiskStorageLocation ? $"{mediaLocation}/{companyId}/{mediaStorageType.ToStorageLocation()}/" : $"{companyId}/{mediaStorageType.ToStorageLocation()}/";

            if (useCreateDirectory) Directory.CreateDirectory(directoryLocation);

            var fullPath = string.Concat(directoryLocation, fileName);
            return fullPath;
        }

        private string CleanPath(string path)
        {
            if (path.StartsWith(_confighelper.GetValueAsString(MediaSettings.MEDIA_STORAGE_LOCATION_CONFIG_KEY)))
            {
                return path.Replace(string.Concat(_confighelper.GetValueAsString(MediaSettings.MEDIA_STORAGE_LOCATION_CONFIG_KEY), "/"), "");
            }

            return path;
        }

    }
}
