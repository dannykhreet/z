using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Models.Areas;
using EZGO.Maui.Core.Utils;
using System.Diagnostics;

namespace EZGO.Maui.Core.Services.Areas
{
    public class WorkAreaService : IWorkAreaService
    {
        private readonly IApiRequestHandler _apiRequestHandler;
        private readonly IRoleFunctionsWrapper _roleFunctionsWrapper;

        public WorkAreaService(IApiRequestHandler apiRequestHandler, IRoleFunctionsWrapper roleFunctionsWrapper)
        {
            _apiRequestHandler = apiRequestHandler;
            _roleFunctionsWrapper = roleFunctionsWrapper;
        }

        public async Task<List<WorkAreaModel>> GetWorkAreasAsync(bool refresh = false, bool isFromSyncService = false)
        {
            string allowed = _roleFunctionsWrapper.checkRoleForAllowedOnlyFlag(UserSettings.userSettingsPrefs.RoleType)
                                                 .ToString()
                                                 .ToLower();
            string uri = $"areas?maxlevel=10&allowedonly={allowed}";

            try
            {
                return await _apiRequestHandler.HandleListRequest<WorkAreaModel>(uri, refresh, isFromSyncService)
                                               .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WorkAreaService] Failed to get work areas: {ex}");
                return new List<WorkAreaModel>();
            }
        }

        public async Task<List<BasicWorkAreaModel>> GetBasicWorkAreasAsync(bool refresh = false)
        {
            try
            {
                var workAreas = await GetWorkAreasAsync(refresh).ConfigureAwait(false);
                workAreas = workAreas.OrderBy(x => x.Name).ToList();

                return workAreas.Select(model => ToBasicWorkArea(model)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WorkAreaService] Failed to get basic work areas: {ex}");
                return new List<BasicWorkAreaModel>();
            }
        }

        private BasicWorkAreaModel ToBasicWorkArea(WorkAreaModel sourceApi, BasicWorkAreaModel parent = null)
        {
            var basic = new BasicWorkAreaModel
            {
                Id = sourceApi.Id,
                Name = sourceApi.Name,
                FullDisplayName = sourceApi.FullDisplayName,
                Picture = sourceApi.Picture,
                Parent = parent,
            };

            basic.Children = sourceApi.Children?
                .Select(child => ToBasicWorkArea(child, basic))
                .OrderBy(child => child.Name)
                .Cast<Interfaces.Utils.ITreeDropdownFilterItem>()
                .ToList();

            return basic;
        }

        public List<BasicWorkAreaModel> GetFlattenedBasicWorkAreas(List<BasicWorkAreaModel> basicWorkAreas)
        {
            return Flatten(basicWorkAreas, AreaHasChildren).ToList();
        }

        private bool AreaHasChildren(BasicWorkAreaModel item) => item.Id != 0;

        private IEnumerable<BasicWorkAreaModel> Flatten(IEnumerable<Interfaces.Utils.ITreeDropdownFilterItem> input, Func<BasicWorkAreaModel, bool> predicate)
        {
            foreach (var node in input.Cast<BasicWorkAreaModel>())
            {
                if (predicate(node))
                    yield return node;

                if (node.Children != null)
                {
                    foreach (var child in Flatten(node.Children, predicate))
                        yield return child;
                }
            }
        }

        public async Task<BasicWorkAreaModel> GetWorkAreaAsync(int id)
        {
            var areas = await GetBasicWorkAreasAsync().ConfigureAwait(false);
            return FindRecursivelyAreaInListByParentId(areas, id);
        }

        private BasicWorkAreaModel FindRecursivelyAreaInListByParentId(List<BasicWorkAreaModel> areas, int id)
        {
            foreach (var item in areas)
            {
                if (item.Id == id)
                    return item;

                if (item.Children != null)
                {
                    var result = FindRecursivelyAreaInListByParentId(item.Children.Cast<BasicWorkAreaModel>().ToList(), id);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        public async Task<DateTime> GetNetworkTimeUtc()
        {
            return await Task.Run(() => NetworkTime.GetNetworkTimeUtc()).ConfigureAwait(false);
        }

        public async Task<DateTime> GetsServerTimeUtcAsync()
        {
            try
            {
                return await InternetHelper.GetServerTimeUtcAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WorkAreaService] Failed to get server time: {ex}");
                return DateTime.UtcNow;
            }
        }

        public void Dispose()
        {
            // nothing to dispose for now
        }
    }
}
