using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Services.Utils
{
    public class SignatureService : ISignatureService
    {
        private readonly IFileService _fileService;
        private readonly IMediaService _mediaService;

        public SignatureService(IMediaService mediaService)
        {
            _mediaService = mediaService;
            _fileService = DependencyService.Get<IFileService>();
        }

        public async Task<string> SaveSignatureAsync(Stream signatureStream)
        {
            if (signatureStream == null)
                return null;

            string filename;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await signatureStream.CopyToAsync(memoryStream).ConfigureAwait(false);
                filename = Guid.NewGuid().ToString("N") + ".png";
                _fileService.SaveFileToInternalStorage(memoryStream.ToArray(), filename, Constants.SignaturesDirectory);
            }

            return filename;
        }

        public async Task UploadSignaturesAsync(IEnumerable<Signature> signatures, MediaStorageTypeEnum mediaStorageType, int id)
        {
            foreach (Signature signature in signatures)
            {
                Stream stream = await _fileService.ReadFromInternalStorageAsBytesAsync(signature.SignatureImage, Constants.SignaturesDirectory).ConfigureAwait(false);

                string filename = Path.GetFileName(signature.SignatureImage);

                string signatureFilename = await _mediaService.UploadPictureAsync(stream, mediaStorageType, id, filename).ConfigureAwait(false);

                signature.SignatureImage = signatureFilename;
            }
        }

        public async Task UploadSignaturesAsync(IEnumerable<SignatureModel> signatures, MediaStorageTypeEnum mediaStorageType, int id)
        {
            foreach (SignatureModel signature in signatures)
            {
                if (!signature.IsLocal)
                    continue;

                Stream stream = await _fileService.ReadFromInternalStorageAsBytesAsync(signature.SignatureImage, Constants.SignaturesDirectory).ConfigureAwait(false);

                string filename = Path.GetFileName(signature.SignatureImage);

                string signatureFilename = await _mediaService.UploadPictureAsync(stream, mediaStorageType, id, filename).ConfigureAwait(false);

                signature.SignatureImage = signatureFilename;
                signature.IsLocal = false;
            }
        }
    }
}
