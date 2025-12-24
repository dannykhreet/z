using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models.TaskGeneration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface ITaskPlanningManager
    {
        Task<PlanningConfiguration> GetPlanningConfiguration(int companyId, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<int> SavePlanningConfiguration(int companyId, int userId, PlanningConfiguration planning);
        List<Exception> GetPossibleExceptions();
    }
}
