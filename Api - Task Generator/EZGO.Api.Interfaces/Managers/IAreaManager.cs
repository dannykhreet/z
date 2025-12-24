using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    /// <summary>
    /// IAreaManager, Interface for use with the AreaManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface IAreaManager
    {
        Task<List<Area>> GetAreasAsync(int companyId, int maxLevel = 2, bool useTreeview = true, int? userId = null, AreaFilters? filters = null, string include = null);
        Task<Area> GetAreaAsync(int companyId, int areaId, ConnectionKind connectionKind = ConnectionKind.Reader, string include = null);
        Task<Dictionary<int, string>> GetAreaNamesAsync(int companyId, List<int> areaIds);
        Task<int> AddAreaAsync(int companyId, int userId, Area area);
        Task<bool> ChangeAreaAsync(int companyId, int userId, int areaId, Area area);
        Task<bool> SetAreaActiveAsync(int companyId, int userId, int areaId, bool isActive = true);
        Task<int> RemoveActionAssignedAreaAsync(int companyId, int userId, int areaId);
        Task<AreaActiveRelations> GetAreaHasActiveRelations(int companyId, int areaId);
        Task<AreaActiveRelations> GetAreaNumberActiveRelations(int companyId, int areaId);

        List<Exception> GetPossibleExceptions();
    }
}
