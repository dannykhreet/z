using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface ISignatureService
    {
        Task<string> SaveSignatureAsync(Stream signatureStream);

        Task UploadSignaturesAsync(IEnumerable<Signature> signatures, MediaStorageTypeEnum mediaStorageType, int id);
    }
}
