using EZGO.Maui.Core.Models.Areas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Areas
{
    public interface IWorkAreaService : IDisposable
    {
        Task<List<WorkAreaModel>> GetWorkAreasAsync(bool refresh = false, bool isFromSyncService = false);

        Task<List<BasicWorkAreaModel>> GetBasicWorkAreasAsync(bool refresh = false);

        List<BasicWorkAreaModel> GetFlattenedBasicWorkAreas(List<BasicWorkAreaModel> basicWorkAreas);

        Task<BasicWorkAreaModel> GetWorkAreaAsync(int id);
        Task<DateTime> GetNetworkTimeUtc();

        Task<DateTime> GetsServerTimeUtcAsync();
    }
}
