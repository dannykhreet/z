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
    public interface IAreaBasicManager
    {
        Task<List<AreaBasic>> GetAreasBasicByStartAreaAsync(int companyId, int areaId, FilterAreaTypeEnum areaFilterType, ConnectionKind connectionKind = ConnectionKind.Reader);

        List<Exception> GetPossibleExceptions();
    }
}
