using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models.Versions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IVersionManager
    {
        Task<List<VersionApp>> GetVersionsAppAsync();
        Task<VersionApp> GetVersionAppAsync(int versionAppId, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<int> AddVersionAppAsync(VersionApp versionApp, int userId, int companyId);
        Task<bool> ChangeVersionAppAsync(VersionApp versionApp, int userId, int companyId);
        Task<bool> SetVersionActiveAsync(int userId, int companyId, int versionId, bool isActive = true);
        List<Exception> GetPossibleExceptions();
    }
}
