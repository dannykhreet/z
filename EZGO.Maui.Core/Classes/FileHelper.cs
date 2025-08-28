using System;
using EZGO.Maui.Core.Interfaces.File;

namespace EZGO.Maui.Core.Classes
{
    public class FileHelper
    {
        public static string GetAssetsPath(string fileName)
        {
            using var scope = App.Container.CreateScope();
            var fileService = scope.ServiceProvider.GetService<IFileService>();
            var result = fileService.GetAssetsPath() + fileName;
            result = result.Replace(" ", "\\ ");
            return result;
        }
    }
}

