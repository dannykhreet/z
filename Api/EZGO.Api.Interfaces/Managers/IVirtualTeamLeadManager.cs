using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Stats;
using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IVirtualTeamLeadManager
    {
        Task<String> fetchShiftMessage();
        Task<String> generateShiftMessage();
        Task changeShift();
        Task<List<OptimizeData>> fetchOptimize(int week);
        Task<List<ReviewData>> fetchReview(int week);
        Task<Dictionary<string, int>> ComputePercentagesLandingPage(int companyId);
    }
}
