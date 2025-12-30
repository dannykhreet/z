using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.FlattenDataManagers
{
    public interface IFlattenManager<T>
    {
        Task<bool> SaveFlattenData(int companyId, int userId, T flattenObject);
        Task<T> RetrieveFlattenData(int templateId, string version, int companyId);
        Task<T> RetrieveLatestFlattenData(int templateId, int companyId);
        Task<string> RetrieveLatestAvailableVersion(int templateId, int companyId);
        Task<string> RetrieveVersionForExistingObjectAsync(int objectId, int companyId);
        List<Exception> GetPossibleExceptions();
    }
}
