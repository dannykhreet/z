using Autofac;
using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.DateFormats;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Shifts;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Areas;
using EZGO.Maui.Core.Models.Shifts;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using EZGO.Maui.Core.ViewModels.Tasks.CompletedTasks;
using NodaTime;
using Syncfusion.Maui.Buttons;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class CompletedTaskViewModel : BaseViewModel
    {
        private List<CompletedTaskListItemViewModel> _Items;

        #region Public Properties 

        private AggregationTimeInterval? _CurrentInterval;
        public AggregationTimeInterval? CurrentInterval
        {
            get => _CurrentInterval;
            set
            {
                _CurrentInterval = value;
                OnPropertyChanged();

                // Check is the API objects are loaded in
                if (IsInitialized && !IsBusy)
                {
                    // Use Task.Run to exit this function as soon as possible and don't block the UI
                    Task.Run(UpdateIntervalSelectionAsync);
                }
            }
        }

        public DatePickerMode PickerMode { get; set; }

        public bool GoToDate => PickerMode == DatePickerMode.GoToDate;
        public bool DateRange => PickerMode == DatePickerMode.DateRange;
        public LocalDateTime PickedDate { get; set; } = DateTimeHelper.Now.Date.AddDays(-1);

        public LocalDateTime GoToDateDate => PickedDate;

        public string ListTitle { get; set; }

        public string PeriodText { get; set; }

        public string PeriodSubText { get; set; }

        public bool NextVisible { get; set; }

        public bool PreviousVisible { get; set; }

        public string SearchText { get; set; }

        public bool IsSearchBarVisible { get; set; }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                (FilterCommand as Command)?.ChangeCanExecute();
            }
        }

        public LocalDateTime FromDate { get; set; } = DateTimeHelper.Now.PlusDays(-2);

        public LocalDateTime ToDate { get; set; } = DateTimeHelper.Now.PlusDays(-1);

        public bool IsCalendarOpen { get; set; }

        public ObservableCollection<SfSegmentItem> SegmentItems { get; set; }

        public FilterControl<CompletedTaskListItemViewModel, TaskStatusEnum> TaskFilter { get; set; } = new FilterControl<CompletedTaskListItemViewModel, TaskStatusEnum>(null);

        public bool IsFromDeepLink { get; set; } = false;

        // In shift tab we only display go to date segment
        public int VisibleCalendarSegmentCount => (CurrentInterval == AggregationTimeInterval.Shift) ? 1 : 2;

        #endregion

        #region Commands

        public ICommand FilterCommand { get; private set; }

        public ICommand PreviousCommand => new Command(async () =>
        {
            IsBusy = true;
            switch (CurrentInterval)
            {
                case AggregationTimeInterval.Shift:
                    await PreviousShiftAsync();
                    break;

                case AggregationTimeInterval.Day:
                    await PreviousDayAsync();
                    break;

                case AggregationTimeInterval.Week:
                    await PreviousWeekAsync();
                    break;
            }
            IsBusy = false;
        });

        public ICommand NextCommand => new Command(async () =>
        {
            IsBusy = true;
            switch (CurrentInterval)
            {
                case AggregationTimeInterval.Shift:
                    await NextShiftAsync();
                    break;

                case AggregationTimeInterval.Day:
                    await NextDayAsync();
                    break;

                case AggregationTimeInterval.Week:
                    await NextWeekAsync();
                    break;
            }
            IsBusy = false;
        });

        public ICommand SearchTextChangedCommand => new Command(() =>
        {
            ExecuteLoadingAction(() => TaskFilter.Filter(TaskFilter.StatusFilters, useDataSource: false));
        }, CanExecuteCommands);

        public IRelayCommand CalendarCommand { get; private set; }

        public IRelayCommand CalendarOkCommand => new RelayCommand(() =>
        {
            if (PickerMode == DatePickerMode.DateRange && !ValidateDateRange())
            {
                DisplayDateRangeValidationPopup();
                return;
            }

            IsCalendarOpen = !IsCalendarOpen;
            PreviousVisible = false;
            Task.Run(async () =>
            {
                IsBusy = true;
                await UpdateCustomRangeAsync();
                IsBusy = false;
            });
        }, CanExecuteCommands);

        private async Task DisplayDateRangeValidationPopup()
        {
            Page page = NavigationService.GetCurrentPage();
            var translated = TranslateExtension.GetValueFromDictionary(LanguageConstants.maxDateRangeText);
            var text = string.Format(translated.ReplaceLanguageVariablesCumulative(), Constants.CompletedTasksCalendarMaxDateRange);
            string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCancel);
            await page.DisplayActionSheet(text, null, cancel);
        }

        public IRelayCommand CalendarCancelCommand => new RelayCommand(() =>
        {
            IsCalendarOpen = !IsCalendarOpen;
        }, CanExecuteCommands);

        public ICommand TaskSelectedCommand => new Command<Syncfusion.Maui.ListView.ItemTappedEventArgs>((args) =>
        {
            var task = args.DataItem as CompletedTaskListItemViewModel;
            if (task == null)
                return;

            ExecuteLoadingAction(async () =>
            {
                using var scope = App.Container.CreateScope();
                var taskInfoViewModel = scope.ServiceProvider.GetService<TaskInfoViewModel>();
                taskInfoViewModel.Task = task;
                await NavigationService.NavigateAsync(viewModel: taskInfoViewModel);
            });
        }, CanExecuteCommands);

        /// <summary>
        /// Sets IsDropdownOpen to false
        /// </summary>
        public IRelayCommand CloseCalendarCommand => new RelayCommand(() => { IsCalendarOpen = !IsCalendarOpen; });

        public ICommand NavigateToActionsCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToActionsAsync(obj);
            });
        }, CanExecuteCommands);


        public ICommand NavigateToPictureProofDetailsCommand => new Command<object>(
           obj => ExecuteLoadingAction(async () => await NavigateToPictureProofDetailsAsync(obj)),
           (obj) => !IsRefreshing);

        #endregion

        #region Services

        private readonly IShiftService _shiftService;
        private readonly ITasksService _taskService;
        private readonly IWorkAreaService _areaService;

        #endregion

        public CompletedTaskViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IShiftService shiftService,
            ITasksService tasksService,
            IWorkAreaService workAreaService) : base(navigationService, userService, messageService, actionsService)
        {
            _shiftService = shiftService;
            _taskService = tasksService;
            _areaService = workAreaService;

            SegmentItems = new ObservableCollection<SfSegmentItem>()
            {
                new SfSegmentItem()
                {
                    Text = TranslateExtension.GetValueFromDictionary("BASE_TEXT_SHIFT"),
                },
                new SfSegmentItem()
                {
                    Text = TranslateExtension.GetValueFromDictionary("BASE_TEXT_DAY"),
                },
                new SfSegmentItem()
                {
                    Text = TranslateExtension.GetValueFromDictionary("BASE_TEXT_WEEK"),
                },
            };

            FilterCommand = new Command<object>((status) => ExecuteLoadingAction(() => FilterTasks(status as TaskStatusEnum?)), CanExecuteCommands);
            CalendarCommand = new RelayCommand(() =>
            {
                IsCalendarOpen = !IsCalendarOpen;
            }, CanExecuteCommands);
        }

        public override async Task Init()
        {
            IsBusy = true;
            Settings.SubpageTasks = MenuLocation.TasksCompleted;

            await MessageHelper.ErrorMessageIsNotSent(_messageService);

            allShifts = await Task.Run(async () => await _shiftService.GetShiftsAsync());
            allShifts ??= new List<ShiftModel>();
            allShifts = allShifts.OrderBy(x => x.Weekday).ThenBy(x => x.ShiftNr).ToList();
            currentShift = await Task.Run(async () => await _shiftService.GetCurrentShiftOrNullAsync());

            // If there is no current shift
            if (currentShift == null)
            {
                // Find the closest shift to the current day
                var closeShifts = allShifts
                    .OrderBy(x => DateTime.Now.DayOfWeek - x.DayOfWeek)
                    .ThenBy(x => x.ShiftNr);

                currentShift = closeShifts
                    // Need to use the second shift because when updating interval selection Previous() method will be called
                    .ElementAtOrDefault(1);

                // In case there's only one shift use the first one.
                currentShift ??= closeShifts.FirstOrDefault();
            }


            currentArea = await Task.Run(async () => await _areaService.GetWorkAreaAsync(Settings.WorkAreaId));
            currentArea ??= new BasicWorkAreaModel();

            // If there are no shifts for the company
            if (allShifts.Any() == false)
            {
                // Disable the shift segment
                SegmentItems[0].IsEnabled = false;

                // If the user selected shifts as the interval
                if (CurrentInterval == AggregationTimeInterval.Shift)
                {
                    // Change interval to 'Day'
                    CurrentInterval = AggregationTimeInterval.Day;
                }
            }

            if (!IsFromDeepLink)
                await Task.Run(async () => await UpdateIntervalSelectionAsync());
            else
                await Task.Run(async () => await UpdateCustomRangeAsync());


            FromDate = DateTimeHelper.Today.PlusDays(-2);
            ToDate = DateTimeHelper.Today.PlusDays(-1);

            IsBusy = false;
            await Task.Run(async () => await base.Init());
        }

        public override bool CanExecuteCommands(object commandParameter)
        {
            return !IsBusy && !IsLoading && !IsRefreshing;
        }

        public override bool CanExecuteCommands()
        {
            return !IsBusy && !IsLoading && !IsRefreshing;
        }

        private void SetItems()
        {
            _Items = _Items.OrderBy(x => x.TaskStatus == TaskStatusEnum.Todo).ThenBy(x => x.SignedAt ?? x.DueAtDT).ThenBy(x => x.Id).ToList();
            TaskFilter.SetUnfilteredItems(_Items);

            TaskHelper.CalculateTaskAmounts(TaskFilter);
            if (!TaskFilter.StatusFilters.IsNullOrEmpty())
                TaskFilter.Filter(TaskFilter.StatusFilters, useDataSource: false);
        }

        private void FilterTasks(TaskStatusEnum? status = null)
        {
            if (TaskFilter.UnfilteredItems != null)
                TaskFilter.Filter(status, useDataSource: false);
        }

        /// <summary>
        /// Navigates to related actions overview asynchronous.
        /// </summary>
        /// <param name="obj">Command object.</param>
        private async Task NavigateToActionsAsync(object obj)
        {
            if (obj is CompletedTaskListItemViewModel item)
            {
                using var scope = App.Container.CreateScope();
                var actionOpenActionsViewModel = scope.ServiceProvider.GetService<ActionOpenActionsViewModel>();
                actionOpenActionsViewModel.TaskId = item.Id;
                actionOpenActionsViewModel.TaskTemplateId = item.TaskTemplateId;
                actionOpenActionsViewModel.ActionType = ActionType.CompletedTask;

                await NavigationService.NavigateAsync(viewModel: actionOpenActionsViewModel);
            }
        }

        private async Task NavigateToPictureProofDetailsAsync(object obj)
        {
            if (obj is CompletedTaskListItemViewModel item && item.HasPictureProof)
            {
                using var scope = App.Container.CreateScope();
                var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();

                pictureProofViewModel.MainMediaElement = item.PictureProofMediaItems?.FirstOrDefault();

                if (item.PictureProofMediaItems?.Count > 1)
                    pictureProofViewModel.MediaElements = new ObservableCollection<MediaItem>(item.PictureProofMediaItems?.Skip(1));

                pictureProofViewModel.IsNew = false;
                pictureProofViewModel.EditingEnabled = false;
                pictureProofViewModel.SupportsEditing = false;

                await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
            }
        }


        private async Task UpdateIntervalSelectionAsync()
        {
            if (CurrentInterval == null || (int)CurrentInterval == -1)
                return;

            PreviousVisible = true;
            IsBusy = true;
            switch (CurrentInterval)
            {
                case AggregationTimeInterval.Shift:
                    // Set flags
                    shiftsShouldShowEntireDay = false;
                    shift_CurrentDay_Start = DateTime.Now;

                    // Change calendar mode to go to date
                    PickerMode = DatePickerMode.GoToDate;

                    // This will set the report to the current shift, but we need to go one back because this shift hasn't finished yet
                    currentDisplayShiftIndex = allShifts.IndexOf(allShifts.SingleOrDefault(x => x.Id == currentShift.Id));
                    // Not to duplicate logic simply call PreviousShift()
                    await PreviousShiftAsync();
                    break;

                case AggregationTimeInterval.Day:
                    shiftsShouldShowEntireDay = true;
                    day_CurrentDay = DateTimeHelper.Now;
                    await UpdateDayAsync();
                    break;

                case AggregationTimeInterval.Week:
                    week_CurrentDay = DateTimeHelper.Now;
                    await UpdateWeekAsync();
                    break;
            }

            IsBusy = false;
        }

        // Improvement idea: separate each switch into it's own class with a base switch class

        #region translation properties

        private string till => TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextTill);
        private string noshift => TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorNoShifts);
        private string week => TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextWeek);
        private string shift => TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextShift);
        private string shiftsTotal => TranslateExtension.GetValueFromDictionary(LanguageConstants.shiftsTotal);

        #endregion

        #region Shift Switch 

        /// <summary>
        /// The area that the user is viewing the reports for
        /// </summary>
        private BasicWorkAreaModel currentArea;

        /// <summary>
        /// Datetime representing the starting date of <see cref="currentDisplayShift"/>
        /// </summary>
        private DateTime shift_CurrentDay_Start;

        /// <summary>
        /// Datetime representing the ending date of <see cref="currentDisplayShift"/>
        /// </summary>
        private DateTime shift_CurrentDay_End;

        /// <summary>
        /// All the shifts that we have
        /// </summary>
        private List<ShiftModel> allShifts;

        /// <summary>
        /// The current in the app shift. Doesn't while moving around in this page.
        /// </summary>
        private ShiftModel currentShift;

        /// <summary>
        /// The shift that the tasks are currently displayed for
        /// </summary>
        private ShiftModel currentDisplayShift => allShifts[currentDisplayShiftIndex];

        /// <summary>
        /// Index of the <see cref="currentDisplayShift"/> in <see cref="allShifts"/>
        /// </summary>
        private int currentDisplayShiftIndex;

        /// <summary>
        /// When <see langword="true"/> going to next/previous shift should display results for the entire day.
        /// </summary>
        /// <remarks>This is used when user selects a date from the calendar, this is interpreted as 'Show all tasks for all the shifts during that day'.</remarks>
        private bool shiftsShouldShowEntireDay;

        private async Task NextShiftAsync()
        {
            await GenericMoveShifts(next: true);
        }

        private async Task PreviousShiftAsync()
        {
            await GenericMoveShifts(next: false);
        }

        private async Task GenericMoveShifts(bool next)
        {
            var moveDirection = next ? 1 : -1;

            // For new start and end date
            DateTime newStartDateTime, newEndDateTime;

            // REMAKRS: For some days there can be no shifts
            // If we're moving between entire days
            if (shiftsShouldShowEntireDay)
            {
                IEnumerable<ShiftModel> applicableShifts;

                // Set the new date the current one
                newStartDateTime = shift_CurrentDay_Start.Date;

                // In case the day we want to go to doesn't have any shifts keep going
                do
                {
                    // Keep going forward/backwards
                    newStartDateTime = newStartDateTime.AddDays(moveDirection);

                    // Determine all the applicable shifts
                    applicableShifts = allShifts.Where(x => x.DayOfWeek == newStartDateTime.DayOfWeek);
                }
                while (applicableShifts.Any() == false);

                // New end date is the day after start date
                newEndDateTime = newStartDateTime.AddDays(1);
            }
            // Normal mode, shift by shift
            else
            {
                // Going forward
                if (next)
                {
                    // If we hit the end
                    if (currentDisplayShiftIndex == allShifts.Count - 1)
                    {
                        // Jump back to the start
                        currentDisplayShiftIndex = 0;
                    }
                    else
                    {
                        //  Otherwise jump one forward
                        currentDisplayShiftIndex++;
                    }
                }
                // Going backwards
                else
                {

                    // If we hit the beginning
                    if (currentDisplayShiftIndex == 0)
                    {
                        // Complete the cycle
                        currentDisplayShiftIndex = allShifts.Count - 1;
                    }
                    // If there is space
                    else
                    {
                        // Jump one shift back
                        currentDisplayShiftIndex--;
                    }
                }

                // Determine new start date first by setting it to the date of the current display day
                newStartDateTime = shift_CurrentDay_Start.Date;

                // Until we hit the week day of the new displayed shift 
                while (newStartDateTime.DayOfWeek != currentDisplayShift.DayOfWeek)
                    // Keep going one day forward/backward
                    newStartDateTime = newStartDateTime.AddDays(moveDirection);

                // Add the time part of the starting date
                newStartDateTime = newStartDateTime.Add(currentDisplayShift.StartTime);

                // Determine new shift end date time

                // Check if overnight
                if (currentDisplayShift.IsOvernight)
                    // Calculate the end day, add one day, and then add shift end time to it
                    newEndDateTime = newStartDateTime.Date.AddDays(1).Add(currentDisplayShift.EndTime);
                // The shifts ends and starts the same day
                else
                    // The new end date time will be the new start date's date plus the end time of the shift
                    newEndDateTime = newStartDateTime.Date.Add(currentDisplayShift.EndTime);
            }

            // Set new values
            shift_CurrentDay_Start = newStartDateTime;
            shift_CurrentDay_End = newEndDateTime;

            // Update the view
            await UpdateShiftsAsync(next);
        }

        /// <summary>
        /// Get middle date between specified start and end of a shift
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private LocalDateTime GetMiddleDate(DateTime start, DateTime end)
        {
            long diff = end.Subtract(start).Ticks;

            DateTime middle = end.AddTicks(-(diff / 2));

            return Settings.ConvertDateTimeToLocal(middle);
        }

        private string GetFormat(AggregationTimeInterval? aggregationTimeInterval)
        {
            switch (aggregationTimeInterval)
            {
                case AggregationTimeInterval.Shift:
                    return BaseDateFormats.ShiftDateFormat;
                case AggregationTimeInterval.Day:
                    return BaseDateFormats.DayDateFormat;
                case AggregationTimeInterval.Week:
                    return BaseDateFormats.WeekDateFormat;
                default:
                    return BaseDateFormats.ShortDisplayDateFormat;
            }
        }

        private async Task UpdateShiftsAsync(bool next)
        {
            var shiftCurrentDayStart = Settings.ConvertDateTimeToLocal(shift_CurrentDay_Start);
            // Set according shift text
            if (shiftsShouldShowEntireDay)
            {
                PeriodText = $"{shiftCurrentDayStart.ToString(GetFormat(CurrentInterval.Value), CultureInfo.CurrentUICulture)} / {currentArea.Name}";
                PeriodSubText = shiftsTotal.Format(allShifts.Where(x => x.DayOfWeek == shift_CurrentDay_Start.DayOfWeek).Count());
            }
            else
            {
                PeriodText = $"{shiftCurrentDayStart.ToString(GetFormat(CurrentInterval.Value), CultureInfo.CurrentUICulture)} {shift} {currentDisplayShift.StartTime} - {currentDisplayShift.EndTime} / {currentArea.Name}";
                PeriodSubText = string.Empty;
            }

            // If we're showing en entire day
            if (shiftsShouldShowEntireDay)
            {
                // In case the day we want to go to doesn't have any shifts keep going one day forward/backwards
                var possibleNextStartDateTime = shift_CurrentDay_Start;

                // Until we find a day that has shifts
                while (allShifts.Where(x => x.DayOfWeek == possibleNextStartDateTime.DayOfWeek).Any() == false)
                {
                    // New start date is the next/previous day then
                    possibleNextStartDateTime = possibleNextStartDateTime.Date.AddDays(next ? 1 : -1);
                }

                // Next is visible only if the new start date is not today
                NextVisible = possibleNextStartDateTime.Date < DateTime.Today;
            }
            // Normal mode, shift by shift
            else
            {
                // Determine if we should show 'next' button
                // First get the next possible shift
                var possibleNextShift = allShifts[currentDisplayShiftIndex + 1 == allShifts.Count ? 0 : currentDisplayShiftIndex + 1];

                // Determine if we should add something to the end date if the shift is overnight
                var addToEndDate = currentDisplayShift.IsOvernight ? TimeSpan.FromDays(1) : TimeSpan.FromDays(0);

                // Determine what date time does the next shift ends
                var possibleNextShiftEndDateTime = shift_CurrentDay_Start.Date.Add(addToEndDate) + possibleNextShift.EndTime;

                // If it ends before current date time it's reachable, otherwise not
                NextVisible = possibleNextShiftEndDateTime < DateTime.Now;
            }

            // Previous is alaways visible
            PreviousVisible = true;

            var taskItems = await GetTaskForShifts();

            _Items = taskItems.Select(x => new CompletedTaskListItemViewModel(x)).ToList();

            SetItems();
        }

        /// <summary>
        /// Get tasks when <see cref="AggregationTimeInterval"/> is set to <see cref="AggregationTimeInterval.Shift"/> with respect to the <see cref="shiftsShouldShowEntireDay"/> flag.
        /// </summary>
        /// <returns>Loaded tasks.</returns>
        private async Task<List<BasicTaskModel>> GetTaskForShifts()
        {
            List<BasicTaskModel> tasks;

            // If should get all tasks for the entire day
            if (shiftsShouldShowEntireDay)
            {
                tasks = new List<BasicTaskModel>();

                // Get all the shifts for that day
                var shifts = allShifts.Where(x => x.DayOfWeek == shift_CurrentDay_Start.DayOfWeek).ToList();

                foreach (var shift in shifts)
                {
                    // Determine start date
                    var startDateTime = shift_CurrentDay_Start.Date + shift.StartTime;

                    // determine end date remembering about overnight shifts
                    var endDateTime = shift_CurrentDay_Start.Date + shift.EndTime + (shift.IsOvernight ? TimeSpan.FromDays(1) : TimeSpan.Zero);

                    // Get the middle of those
                    var middle = GetMiddleDate(startDateTime, endDateTime);

                    // Get the tasks and add them
                    tasks.AddRange(await _taskService.GetTasksForShiftAsync(middle, refresh: IsFromDeepLink));
                }
            }
            // Normal mode, shift by shift
            else
            {
                // Get the middle date
                var middle = GetMiddleDate(shift_CurrentDay_Start, shift_CurrentDay_End);

                // And get the tasks
                tasks = await _taskService.GetTasksForShiftAsync(middle, refresh: IsFromDeepLink);
            }

            return tasks;
        }

        #endregion

        #region Day Switch

        private LocalDateTime day_CurrentDay;

        private async Task PreviousDayAsync()
        {
            // Jump back one day
            day_CurrentDay = day_CurrentDay.AddDays(-1);
            await UpdateDayAsync();
        }

        private async Task NextDayAsync()
        {
            day_CurrentDay = day_CurrentDay.AddDays(1);
            await UpdateDayAsync();
        }

        private async Task UpdateDayAsync()
        {
            List<BasicTaskModel> result = await _taskService.GetTasksForYesterday(day_CurrentDay, refresh: IsFromDeepLink);

            _Items = result.Select(x => new CompletedTaskListItemViewModel(x)).ToList();

            var display_Day = day_CurrentDay.AddDays(-1);

            PeriodText = $"{display_Day.ToString(GetFormat(CurrentInterval), CultureInfo.CurrentUICulture)} / {currentArea.Name}";
            PeriodSubText = string.Empty;

            NextVisible = display_Day.Date.AddDays(1).Date < Settings.ConvertDateTimeToLocal(DateTime.Now).Date;
            PreviousVisible = true;

            SetItems();
        }

        #endregion

        #region Week Switch

        private LocalDateTime week_CurrentDay;
        private DateTime week_StartDay;

        private async Task PreviousWeekAsync()
        {
            // Jump back one week
            week_CurrentDay = week_CurrentDay.AddDays(-7);
            await UpdateWeekAsync();
        }

        private async Task NextWeekAsync()
        {
            week_CurrentDay = week_CurrentDay.AddDays(7);
            await UpdateWeekAsync();
        }

        private async Task UpdateWeekAsync()
        {

            List<BasicTaskModel> result = await _taskService.GetTasksForLastWeek(week_CurrentDay, refresh: IsFromDeepLink);

            _Items = result.Select(x => new CompletedTaskListItemViewModel(x)).ToList();

            var display_Week = week_CurrentDay.AddDays(-7);

            //var now = DateTimeHelper.Now;
            // Jump to the first Monday
            week_StartDay = display_Week.ToDateTimeUnspecified().Date.AddDays(-(int)display_Week.Date.DayOfWeek + 1);

            var startDay = Settings.ConvertDateTimeToLocal(week_StartDay);

            PeriodText = $"{week} {week_StartDay.GetWeekNumber()} - {startDay.Year} / {currentArea.Name}";
            var format = GetFormat(CurrentInterval.Value);
            PeriodSubText = $"{startDay.ToString(format, CultureInfo.CurrentUICulture)} {till} {startDay.PlusDays(7).ToString(format, CultureInfo.CurrentUICulture)}";
            //  v---Because the end week date of possible next wee must be less than current date

            NextVisible = week_StartDay.Date.AddDays(2 * 7) <= DateTime.Now.Date.AddDays(-(int)display_Week.Date.DayOfWeek + 1);

            PreviousVisible = true;

            SetItems();
        }

        #endregion

        #region Custom Date Range

        public bool DateRangeHasError => ValidateDateRange();

        private bool ValidateDateRange()
        {
            var range = Period.Between(FromDate.Date, ToDate.Date, PeriodUnits.Days);
            return range.Days <= Constants.CompletedTasksCalendarMaxDateRange;
        }

        private async Task UpdateCustomRangeAsync()
        {
            switch (PickerMode)
            {
                case DatePickerMode.DateRange:
                    {
                        var format = GetFormat(null);
                        PeriodText = $"{FromDate.ToString(format, CultureInfo.CurrentUICulture)} {till} {ToDate.ToString(format, CultureInfo.CurrentUICulture)} / {currentArea.Name}";
                        PeriodSubText = string.Empty;
                        NextVisible = false;
                        CurrentInterval = null;
                        PreviousVisible = false;

                        // we are adding the starttime of the firstshift to the FromDate
                        var firstShiftFromDateStartTime = allShifts.Where(x => x.DayOfWeek == FromDate.ToDateTimeUnspecified().DayOfWeek).FirstOrDefault()?.StartTime ?? TimeSpan.Zero;
                        var myFromDate = FromDate.ToDateTimeUnspecified().Add(firstShiftFromDateStartTime);

                        // we are finding the starttime and endtime of the last shift of the ToDate
                        var lastShiftToDateStartTime = allShifts.Where(x => x.DayOfWeek == ToDate.ToDateTimeUnspecified().DayOfWeek).LastOrDefault()?.StartTime ?? TimeSpan.Zero;
                        var lastShiftToDateEndTime = allShifts.Where(x => x.DayOfWeek == ToDate.ToDateTimeUnspecified().DayOfWeek).LastOrDefault()?.EndTime ?? TimeSpan.Zero;

                        // add the last shift end time to the ToDate
                        var myToDate = ToDate.ToDateTimeUnspecified().Add(lastShiftToDateEndTime);
                        if (lastShiftToDateEndTime < lastShiftToDateStartTime)
                        {
                            // Check if the endate is on the day after the ToDate, if so add one day
                            myToDate = myToDate.AddDays(1);
                        }

                        var result = await _taskService.GetPreviousByRangeAsync(Settings.ConvertDateTimeToLocal(myFromDate), Settings.ConvertDateTimeToLocal(myToDate), refresh: IsFromDeepLink);

                        _Items = result.Select(x => new CompletedTaskListItemViewModel(x)).ToList();

                        SetItems();
                        break;
                    }
                case DatePickerMode.GoToDate:
                    {
                        switch (CurrentInterval)
                        {
                            case AggregationTimeInterval.Shift:
                                {
                                    // Get all shifts that week day 
                                    var dayShifts = allShifts.Where(x => x.DayOfWeek == ConvertIsoDayOfWeekToDayOfWeek(GoToDateDate.DayOfWeek)).OrderBy(x => x.StartTime);

                                    // Try to get the second shift from that week day
                                    ShiftModel jumpToShift = null;
                                    if (dayShifts.Count() > 1)
                                        jumpToShift = dayShifts.Skip(1).First();
                                    else
                                        jumpToShift ??= dayShifts.FirstOrDefault();

                                    currentDisplayShiftIndex = allShifts.IndexOf(allShifts.SingleOrDefault(x => x.Id == jumpToShift.Id));

                                    // If there no shift still
                                    if (jumpToShift == null)
                                    {
                                        // Set messages
                                        PeriodText = $"{noshift} {GoToDateDate.ToString(GetFormat(null), null)}";
                                        PeriodSubText = string.Empty;
                                        NextVisible = false;
                                        PreviousVisible = false;
                                        _Items.Clear();

                                        SetItems();
                                        return;
                                    }

                                    // We got shifts to display
                                    shiftsShouldShowEntireDay = false;

                                    // Set the current start day to one day ahead
                                    shift_CurrentDay_Start = GoToDateDate.ToDateTimeUnspecified().Date.AddDays(1);

                                    // And call previous shift to keep the logic in one place
                                    await PreviousShiftAsync();

                                    break;
                                }

                            case AggregationTimeInterval.Day:
                                {
                                    day_CurrentDay = GoToDateDate.Date.AddDays(2).AddSeconds(-1);
                                    await UpdateDayAsync();
                                    break;
                                }

                            case AggregationTimeInterval.Week:
                                {
                                    week_CurrentDay = GoToDateDate.Date.AddDays(7);
                                    await UpdateWeekAsync();
                                    break;
                                }
                            case AggregationTimeInterval.Month:
                                {
                                    FromDate = new LocalDateTime(GoToDateDate.Date.Year, GoToDateDate.Date.Month, 1, 0, 0);
                                    ToDate = FromDate.Date.AddMonths(1).AddDays(-1);
                                    PickerMode = DatePickerMode.DateRange;
                                    await UpdateCustomRangeAsync();
                                    break;
                                }

                            // If there is no current interval
                            // Occurs when you go to DateRange, click OK, and then go to JumpToDate
                            default:
                                {
                                    // Set is busy to prevent default loading action occurring when CurrentInterval is set
                                    IsBusy = true;
                                    CurrentInterval = AggregationTimeInterval.Day;
                                    IsBusy = false;

                                    // Then go to the selected date
                                    day_CurrentDay = GoToDateDate.Date.AddDays(2).AddSeconds(-1);
                                    await UpdateDayAsync();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            _taskService.Dispose();
            _shiftService.Dispose();
            _areaService.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Converts an ISO 8601 day-of-week value (1 = Monday, ..., 7 = Sunday)
        /// to the .NET DayOfWeek enum (0 = Sunday, ..., 6 = Saturday).
        /// 
        /// This conversion is necessary because ISO 8601 and .NET DayOfWeek
        /// use different conventions for representing days of the week:
        /// - ISO 8601: Monday = 1, ..., Sunday = 7
        /// - .NET DayOfWeek: Sunday = 0, ..., Saturday = 6
        /// Failing to convert between these conventions can lead to incorrect
        /// day calculations or mismatches in day-dependent logic.
        /// </summary>
        /// <param name="isoDayOfWeek">An IsoDayOfWeek representing the ISO 8601 day of the week (1 = Monday, ..., 7 = Sunday).</param>
        /// <returns>The corresponding .NET DayOfWeek value.</returns>
        private static DayOfWeek ConvertIsoDayOfWeekToDayOfWeek(IsoDayOfWeek isoDayOfWeek)
        {
            // Perform the conversion: ISO Sunday (7) maps to .NET Sunday (0)
            return (DayOfWeek)((int)isoDayOfWeek % 7);
        }
    }
}
