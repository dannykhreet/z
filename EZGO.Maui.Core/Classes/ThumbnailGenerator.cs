using System;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes
{
    public class ThumbnailGenerator : IThumbnailGenerator
    {
        private readonly IThumbnailService _thumbnailService;
        private readonly IFileService _fileService;

        public ThumbnailGenerator()
        {
            _thumbnailService = DependencyService.Get<IThumbnailService>();
            _fileService = DependencyService.Get<IFileService>();
        }

        public string GenerateThumbnail(string path)
        {
            string thumbnailFilename = string.Format(Constants.ThumbnailFilenameFormat, DateTimeHelper.Now.ToFileTime());

            byte[] thumbnailBytes = _thumbnailService.GenerateThumbnail(path);

            return _fileService.SaveFileToInternalStorage(thumbnailBytes, thumbnailFilename, Constants.ThumbnailsDirectory);
        }
    }
}
