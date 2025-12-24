using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EZGO.Api.Models.Enumerations;
using Microsoft.AspNetCore.Http;

namespace EZGO.Api.Interfaces.Utils
{
    public interface IMediaUploader
    {
        Task<string> UploadFileAsync(IFormFile file, StorageTypeEnum storageType ,MediaTypeEnum mediaType, MediaStorageTypeEnum mediaStorageType, int? companyId = null, int? objectId = null, bool includeBaseUrlOnReturn = false);
        Task<string> CopyFileAsync(string sourceFileKey, MediaTypeEnum mediaType, MediaStorageTypeEnum mediaStorageType, int? companyId = null, int? objectId = null, bool includeBaseUrlOnReturn = false);
        Task<bool> DeleteFileFromS3Async(string keyName, MediaTypeEnum mediaType);
        Task<string> MoveFileToStructuredLocation(string currentFile, StorageTypeEnum storageType, MediaTypeEnum mediaType, MediaStorageTypeEnum mediaStorageType, int? companyId = null, int? objectId = null, bool includeBaseUrlOnReturn = false);
        Task<bool> CheckFileForUnstructured(string currentFile);
    }
}
