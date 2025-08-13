using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes.Stages;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Tags;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Statuses;
using EZGO.Maui.Core.Models.Tags;
using Microsoft.IdentityModel.Tokens;
using Syncfusion.Maui.DataSource;

namespace EZGO.Maui.Core.Classes.Filters
{
    public interface IFilterControl
    {
        ObservableCollection<TagModel> SearchedTags { get; set; }
        List<TagModel> Tags { get; set; }
        void Filter(bool resetIfTheSame = true, bool useDataSource = true, bool reloadFileredList = true);
    }

    public class FilterControl<T, F> : NotifyPropertyChanged, IFilterControl, IDisposable where T : IItemFilter<F> where F : struct
    {
        private IReadOnlyList<T> basicList;
        private List<T> filteredList = new List<T>();

        public FilterModel SelectedFilter { get; set; }

        public List<FilterModel> FilterCollection { get; set; }

        public DataSource ListSource { get; set; } = new DataSource();

        public List<T> FilteredList
        {
            get => filteredList;
            set
            {
                filteredList = value;
                StagesControl?.SetFilteredItems(value);
            }
        }
        public List<T> FilteredSlideList { get; set; } = new List<T>();

        public F? StatusFilter { get; set; } = null;

        public List<F?> StatusFilters { get; set; } = new List<F?>();

        public IStatus<F> TaskStatusList { get; set; }

        public string SearchText { get; set; }

        public bool HasItems { get; private set; }

        public int ItemsCount { get; private set; }

        public int FilteredItemsCount => FilteredList.Count;

        public List<T> UnfilteredItems { get => basicList?.ToList(); }

        public ObservableCollection<TagModel> SearchedTags { get; set; } = new ObservableCollection<TagModel>();

        public List<TagModel> Tags { get; set; }

        public Func<T, IEnumerable<Tag>> NestedTagsAccessor { get; set; }

        private StagesControl StagesControl { get; set; }

        public FilterControl(List<T> list)
        {
            NestedTagsAccessor = (obj) =>
            {
                return obj.Tags ?? new List<Tag>();
            };

            SetUnfilteredItems(list);
            TaskStatusList = StatusFactory.CreateStatus<F>();
            _ = Task.Run(SetTags);
        }

        public void SetStagesControl(StagesControl stagesControl)
        {
            StagesControl = stagesControl;
        }

        public void SetSelectedFilter(string filterName = null, int id = 0) => SelectedFilter = FilterCollection?.FirstOrDefault(x => x?.Name == filterName && x?.Id == id);

        public void AddFilters(params FilterModel[] filters) => FilterCollection = filters?.ToList();

        public int CountItemsByStatus(F filter, IStatus<F> status)
        {
            int? itemsCount;

            if (SearchText == null)
            {
                itemsCount = basicList?.Count(x => x.FilterStatus.ToString() == filter.ToString());
            }
            else
            {
                itemsCount = basicList?.Count(x => x.FilterStatus.ToString() == filter.ToString() && x.Name != null && x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (itemsCount.HasValue)
            {
                status.SetItemsWithStatus(itemsCount.Value, filter);
                return itemsCount.Value;
            }
            return 0;
        }

        public void SetStatusPercentages(IStatus<F> status)
        {
            int? allItemsCount = basicList?.Count;
            if (allItemsCount == null)
                return;

            //var statuses = TaskStatusList.GetStatuses();

            foreach (var item in status?.StatusModels)
            {
                item.Percentage = (int)Math.Round((double)(item.ItemNumber * 100) / allItemsCount.Value, mode: MidpointRounding.AwayFromZero);
            }
        }

        public void SetStatusSelected(IStatus<F> status)
        {
            status?.StatusModels?.ForEach(x =>
            {
                if (StatusFilters?.Any(s => s.ToString() == x.Status.ToString()) ?? false)
                    x.IsSelected = true;
                else
                    x.IsSelected = false;
            });
        }

        public void RefreshStatusFilter(bool useDataSource = true, bool reloadFileredList = true)
        {
            if (StatusFilters.IsNullOrEmpty())
            {
                lock (FilteredList)
                {
                    if (useDataSource)
                    {
                        FilteredList = basicList?.ToList() ?? new();
                        SetStatusSelected(TaskStatusList);
                        FilterByDataSource();
                    }
                    else
                    {
                        FilterByList(reloadFileredList);
                    }
                }
            }
            else
            {
                FilterByList(reloadFileredList);
            }
        }

        public void Filter(bool resetIfTheSame = true, bool useDataSource = true, bool reloadFileredList = true)
        {
            Filter(StatusFilters, resetIfTheSame, useDataSource, reloadFileredList);
        }


        public void Filter(object obj, bool resetIfTheSame = true, bool useDataSource = true, bool reloadFileredList = true)
        {
            if (obj is null || basicList == null)
                return;

            if (obj is FilterModel filterModel)
            {
                SelectedFilter = filterModel;
            }
            else if (obj is List<F?> statusFilters)
            {
                if (statusFilters.IsNullOrEmpty())
                {
                    StatusFilters = new List<F?>();
                    StatusFilter = null;
                    FilteredList = basicList.ToList();
                }
                else
                {
                    StatusFilters = statusFilters;
                }

                SetStatusSelected(TaskStatusList);
            }
            else
            {
                var status = (F?)obj;

                var selectedStatus = StatusFilters?.FirstOrDefault(x => x.ToString() == status?.ToString());

                if (selectedStatus != null)
                    StatusFilters?.Remove(selectedStatus);
                else
                    StatusFilters?.Add(status);

                if (StatusFilter != null && status?.ToString() == StatusFilter?.ToString() && resetIfTheSame)
                {
                    StatusFilter = null;
                    ListSource.Filter = null;
                    FilteredList = basicList.ToList();
                }
                else
                {
                    StatusFilter = status;
                }

                SetStatusSelected(TaskStatusList);
            }

            if (useDataSource)
                FilterByDataSource();
            else
                FilterByList(reloadFileredList);
        }

        public void SetUnfilteredItems(List<T> unfilteredItems)
        {
            basicList = unfilteredItems;
            if (basicList != null)
            {
                FilteredList = basicList.ToList();
                if (ListSource != null)
                    ListSource.Source = basicList.ToList();
            }
            ItemsCount = basicList?.Count() ?? 0;

            SetHasItems();
        }

        public void AddFilteredItems(T first, T last, List<T> stageTemplates = null)
        {
            var filteredList = new LinkedList<T>(FilteredList);

            if (stageTemplates != null)
            {
                filteredList = new LinkedList<T>(stageTemplates);
            }

            if (first != null)
                filteredList.AddFirst(first);
            if (last != null)
                filteredList.AddLast(last);

            FilteredSlideList = filteredList.ToList();
        }

        private void FilterByDataSource()
        {
            var activeFilters = TaskStatusList?.StatusModels?.Where(x => x.IsSelected).ToList();

            Predicate<object> filterByActiveFilters = (obj) => activeFilters.IsNullOrEmpty() || (obj is T filterItem
                && activeFilters.Any(x => x.Status.ToString() == filterItem?.FilterStatus.ToString()));

            Predicate<object> filterBySearchText = (obj) => string.IsNullOrWhiteSpace(SearchText) || (obj is T filterItem
                && filterItem.Name != null && filterItem.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            Predicate<object> filterByActiveTags = (obj) => SearchedTags.IsNullOrEmpty() || (obj is T filterItem
                && NestedTagsAccessor(filterItem).Any(x => SearchedTags.Any(t => t.Id == x.Id)));

            Predicate<object> pre = (obj) => filterByActiveFilters(obj) && filterBySearchText(obj) && filterByActiveTags(obj);

            if (ListSource != null)
            {
                ListSource.Filter = pre;
                ListSource.RefreshFilter();
            }
            HasItems = ListSource?.Items?.Count > 0;
        }

        private void FilterByList(bool reloadFileredList = true)
        {
            var activeFilters = TaskStatusList?.StatusModels?.Where(x => x.IsSelected).ToList();

            Predicate<T> filterByActiveFilters = (obj) => activeFilters.IsNullOrEmpty() || (obj is T filterItem
                && activeFilters.Any(x => x.Status.ToString() == filterItem?.FilterStatus.ToString()));

            Predicate<T> filterBySearchText = (obj) => string.IsNullOrWhiteSpace(SearchText) || (obj is T filterItem
                && filterItem.Name != null && filterItem.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            Predicate<T> filterByActiveTags = (obj) => SearchedTags.IsNullOrEmpty() || (obj is T filterItem
                && NestedTagsAccessor(obj).Any(x => SearchedTags.Any(t => t.Id == x.Id)));

            Predicate<T> pre = (obj) => filterByActiveFilters(obj) && filterBySearchText(obj) && filterByActiveTags(obj);

            Func<T, bool> predicate = new(pre);

            if (pre != null)
            {
                FilteredList = reloadFileredList ? basicList?.Where(predicate).ToList() : FilteredList;
            }
            else
            {
                FilteredList = reloadFileredList ? basicList?.ToList() : FilteredList;
            }

            HasItems = FilteredList?.Count > 0;
        }

        public void Dispose()
        {
            basicList = null;
            SelectedFilter = null;
            FilterCollection?.Clear();
            FilterCollection = null;
            FilteredList?.Clear();
            FilteredList = null;
            TaskStatusList = null;
            ListSource = null;
        }

        internal void SetHasItems()
        {
            HasItems = FilteredList?.Count > 0;
        }

        private async Task SetTags()
        {
            using var scope = App.Container.CreateScope();
            var tagsService = scope.ServiceProvider.GetService<ITagsService>();
            var tags = await tagsService.GetTagModelsAsync(refresh: false);
            Tags = tags;
            OnPropertyChanged(nameof(Tags));
        }


    }
}
