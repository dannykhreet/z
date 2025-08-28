using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.ModelInterfaces;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Models.Tasks.Properties;
using EZGO.Maui.Core.Services.Message;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using Syncfusion.Maui.DataSource.Extensions;
using System.Diagnostics;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels.Tasks
{
    public class TaskSlideViewModel : BasicTaskViewModel, IHasPopup, IHasTaskPropertiesEditViewModel
    {
        #region Public Properties

        /// <summary>
        /// The period to get the task for
        /// </summary>
        public TaskPeriod TaskPeriod { get; set; }

        /// <summary>
        /// Current index of selected item in the carousel view
        /// </summary>
        public int CurrentIndex { get; set; }

        /// <summary>
        /// Determines the visibility of the new task button
        /// </summary>
        public bool EditTaskButtonIsVisible => UserSettings.RoleType != RoleTypeEnum.Basic;

        /// <summary>
        /// Indicates if the deeplink button should be visible
        /// </summary>
        public bool DeeplinkButtonVisible { get; set; }

        /// <summary>
        /// Indicates if the steps/infodocument button should be visible
        /// </summary>
        public bool MoreInfoButtonVisible { get; set; }

        /// <summary>
        /// The task element that corresponds to thee <see cref="CurrentIndex"/>
        /// </summary>
        public BasicTaskModel SelectedTask { get; set; }

        /// <summary>
        /// String representation of the <see cref="CurrentIndex"/> and the total count of tasks.
        /// <para>E.g. '1 of 12'</para>
        /// </summary>
        public string Pager { get; set; }

        /// <summary>
        /// Gets or sets current status filter
        /// </summary>
        /// <value>Currently selected status filter or <see langword="null"/> if there's no filter</value>
        public TaskStatusEnum? StatusFilter { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsPopupOpen { get; set; }

        /// <summary>
        /// Viewmodel for the currently opened task property popup
        /// </summary>
        public BaseTaskPropertyEditViewModel PropertyEditViewModel { get; set; }

        /// <summary>
        /// Indicates if the action/comment popup is open
        /// </summary>
        public bool IsActionPopupOpen { get; set; }

        public FilterControl<BasicTaskModel, TaskStatusEnum> TaskFilterControl { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Filters current tasks by status
        /// </summary>
        public IAsyncRelayCommand FilterCommand => new AsyncRelayCommand<object>(async (obj) =>
        {
            await ExecuteLoadingActionAsync(async () => await ApplyFilter(status: obj as TaskStatusEnum?, reset: true));
        }, CanExecuteCommands);

        public IAsyncRelayCommand SlideDetailCommand => new AsyncRelayCommand<BasicTaskModel>(async task =>
        {
            await ExecuteLoadingAction(async () => await NavigateToSlideDetailAsync(task));
        });

        public IAsyncRelayCommand EditTaskCommand => new AsyncRelayCommand<BasicTaskModel>(async task =>
        {
            await ExecuteLoadingAction(async () => await NavigateToEditTaskAsync(task));
        });

        /// <summary>
        /// Gets the task skipped command.
        /// </summary>
        /// <value>
        /// The task skipped command.
        /// </value>
        public ICommand TaskSkippedCommand { get; private set; }

        /// <summary>
        /// Gets the task not ok command.
        /// </summary>
        /// <value>
        /// The task not ok command.
        /// </value>
        public ICommand TaskNotOkCommand { get; private set; }

        /// <summary>
        /// Gets the task ok command.
        /// </summary>
        /// <value>
        /// The task ok command.
        /// </value>
        public ICommand TaskOkCommand { get; private set; }

        public ICommand ExtraInformationCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () => await NavigateToExtraInformationAsync());
        });

        public ICommand DeepLinkCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () => await NavigateToDeepLinkAsync(SelectedTask));
        });

        public ICommand SelectionChangedCommand => new Command((obj) =>
        {
            if (Microsoft.Maui.Devices.DeviceInfo.Idiom == DeviceIdiom.Phone && DeviceSettings.PhoneViewsEnabled)
            {
                var swipe = (SwipeDirection)Enum.Parse(typeof(SwipeDirection), obj.ToString());
                if (swipe == SwipeDirection.Left)
                    CurrentIndex++;
                else
                    CurrentIndex--;
                if (CurrentIndex < TaskFilterControl.FilteredList.Count() && CurrentIndex >= 0)
                    SelectedTask = TaskFilterControl.FilteredList.ElementAt(CurrentIndex);
                else
                {
                    if (swipe == SwipeDirection.Left)
                        CurrentIndex--;
                    else
                        CurrentIndex++;
                }
            }

            if (!OnlineShiftCheck.IsShiftChangeAllowed)
                _ = OnlineShiftCheck.CheckCycleChange();

            ExecuteLoadingAction(OnSelectedTaskChanged);
        });

        public ICommand OpenPopupCommand => new Command<BasicTaskPropertyModel>((prop) =>
        {
            ExecuteLoadingAction(() => OpenFeaturePopup(prop));
        }, CanExecuteCommands);

        public ICommand SubmitPopupCommand => new Command(() => { ExecuteLoadingAction(async () => { await SubmitPopupAsync(); }); }, CanExecuteCommands);

        public ICommand ClosePopupCommand => new Command(CloseFeaturePopup);

        public ICommand ActionCommand { get; private set; }

        public ICommand NavigateToNewActionCommand { get; private set; }

        public ICommand NavigateToNewCommentCommand { get; private set; }

        #endregion

        #region Private Members

        private readonly ITasksService _taskService;
        private readonly ITaskTemplatesSerivce _taskTemplateService;
        private readonly IPropertyService _propertyService;

        #endregion

        #region Initialize

        public TaskSlideViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            ITasksService tasksService,
            ITaskTemplatesSerivce taskTemplatesService,
            IPropertyService propertyService) : base(navigationService, userService, messageService, actionsService)
        {
            _taskService = tasksService;
            _taskTemplateService = taskTemplatesService;
            _propertyService = propertyService;

            _ = Task.Run(SetCommands);
        }

        private void SetCommands()
        {
            TaskSkippedCommand = new Command(() => ExecuteLoadingAction(async () =>
            {
                if (SelectedTask?.HasPictureProof ?? false)
                {
                    await SetTaskWithPictureProofStatus(TaskStatusEnum.Skipped);
                    return;
                }

                var oldStatus = SelectedTask.FilterStatus;
                await TaskHelper.SetTaskStatusAsync(SelectedTask, TaskStatusEnum.Skipped, TaskFilterControl, false);

                //user was able to set skipped status
                if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped && oldStatus != SelectedTask.FilterStatus)
                {
                    SelectedTask.ResetValidation();
                    SkipTaskIndex();
                }
            }), CanExecuteCommands);

            TaskOkCommand = new Command(() => ExecuteLoadingAction(async () =>
           {
               if (!Validate())
                   return;

               if (SelectedTask?.HasPictureProof ?? false)
                   await SetTaskWithPictureProofStatus(TaskStatusEnum.Ok);
               else
                   await TaskHelper.SetTaskStatusAsync(SelectedTask, TaskStatusEnum.Ok, TaskFilterControl, false);
           }), CanExecuteCommands);

            TaskNotOkCommand = new Command(() => ExecuteLoadingAction(async () =>
            {
                if (!Validate())
                    return;

                if (SelectedTask?.HasPictureProof ?? false)
                    await SetTaskWithPictureProofStatus(TaskStatusEnum.NotOk);
                else
                    await TaskHelper.SetTaskStatusAsync(SelectedTask, TaskStatusEnum.NotOk, TaskFilterControl, false);
            }), CanExecuteCommands);

            NavigateToNewCommentCommand = new Command(() => ExecuteLoadingAction(async () => await NavigateToNewCommentAsync()));
            NavigateToNewActionCommand = new Command(() => ExecuteLoadingAction(async () => await NavigateToNewActionAsync()));
            ActionCommand = new Command(() => ExecuteLoadingAction(async () => await OpenPopupOrNavigateToActionsAsync()));
        }

        private bool Validate()
        {
            bool isValid = true;
            isValid = SelectedTask?.Validate() ?? false;
            return isValid;
        }

        private async Task SetTaskWithPictureProofStatus(TaskStatusEnum status)
        {
            CurrentStatus = status;

            bool owner = (SelectedTask.Signature?.SignedById ?? UserSettings.Id) == UserSettings.Id;
            if (owner)
            {
                if (SelectedTask.FilterStatus == status)
                {
                    if (status == TaskStatusEnum.Skipped)
                    {
                        await TaskHelper.UploadTaskStatusAsync(SelectedTask, TaskStatusEnum.Todo, false, null);
                        FilterAndRecalculateTasks();
                    }
                    else
                    {
                        await OpenUntapTaskDialogAsync();
                    }
                }
                else
                {
                    if (status == TaskStatusEnum.Skipped)
                    {
                        OpenSkipTaskPopup();
                    }
                    else if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped || SelectedTask.FilterStatus == TaskStatusEnum.Todo)
                    {
                        await NavigateToNewPictureProof(status);
                        CurrentStatus = null;
                    }
                    else
                        OpenChangeStatusPopup();
                }
            }
            else
            {
                if (SelectedTask.FilterStatus == status)
                {
                    if (status == TaskStatusEnum.Skipped)
                        return;

                    await NavigateToPictureProofDetails();
                    return;
                }
                if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped || SelectedTask.FilterStatus == TaskStatusEnum.Todo)
                {
                    await NavigateToNewPictureProof(status);
                    CurrentStatus = null;
                    return;
                }
                await OpenCantTapDialogAsync();
            }
        }

        public async override Task SubmitSkipCommandAsync()
        {
            var oldStatus = SelectedTask.FilterStatus;
            await TaskHelper.SetTaskStatusAsync(SelectedTask, TaskStatusEnum.Skipped, TaskFilterControl, false);

            //user was able to set skipped status
            if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped && oldStatus != SelectedTask.FilterStatus)
            {
                SelectedTask.ResetValidation();
                SkipTaskIndex();
            }
            await base.SubmitSkipCommandAsync();
        }

        public async override Task KeepButtonChangeStatusPopupCommandAsync()
        {
            if (CurrentStatus != null)
            {
                await TaskHelper.SetTaskStatusAsync(SelectedTask, CurrentStatus.Value, TaskFilterControl, false);
            }
            await base.KeepButtonChangeStatusPopupCommandAsync();
        }

        public async override Task RemoveButtonChangeStatusPopupCommandAsync()
        {
            if (CurrentStatus != null)
            {
                await NavigateToPictureProofDetails(CurrentStatus.Value);
            }
        }

        public async override Task UntapTaskAsync()
        {
            await TaskHelper.UploadTaskStatusAsync(SelectedTask, TaskStatusEnum.Todo, false, null);
        }

        public async override Task SeePicturesAsync()
        {
            await NavigateToPictureProofDetails();
        }

        private async Task NavigateToNewPictureProof(TaskStatusEnum status)
        {
            using var scope = App.Container.CreateScope();
            var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
            pictureProofViewModel.SelectedTask = SelectedTask;
            pictureProofViewModel.TaskStatus = status;
            pictureProofViewModel.IsNew = true;
            pictureProofViewModel.SupportsEditing = true;
            await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
        }

        private async Task NavigateToPictureProofDetails(TaskStatusEnum? status = null)
        {
            bool isOwner = (SelectedTask.Signature?.SignedById ?? UserSettings.Id) == UserSettings.Id;

            using var scope = App.Container.CreateScope();
            var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
            pictureProofViewModel.MainMediaElement = SelectedTask.PictureProofMediaItems?.FirstOrDefault();

            if (SelectedTask.PictureProofMediaItems?.Count > 1)
                pictureProofViewModel.MediaElements = new System.Collections.ObjectModel.ObservableCollection<MediaItem>(SelectedTask.PictureProofMediaItems?.Skip(1));

            pictureProofViewModel.IsNew = false;
            pictureProofViewModel.EditingEnabled = false;
            pictureProofViewModel.SupportsEditing = false;
            if (isOwner)
            {
                pictureProofViewModel.SelectedTask = SelectedTask;
                pictureProofViewModel.TaskStatus = status ?? SelectedTask.FilterStatus;
                pictureProofViewModel.SupportsEditing = true;
            }
            await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
        }

        public override async Task Init()
        {
            TaskHelper.CalculateTaskAmounts(TaskFilterControl);

            TaskFilterControl.Filter(TaskFilterControl.StatusFilters, false, false, true);

            MessagingCenter.Subscribe<TaskViewModel>(this, Constants.RecalculateAmountsMessage, _ =>
            {
                TaskFilterControl.RefreshStatusFilter(false, false);
            });

            MessagingCenter.Subscribe<PictureProofViewModel>(this, Constants.PictureProofChanged, (_) =>
            {
                FilterAndRecalculateTasks();
            });

            MessagingCenter.Subscribe<string, int>(this, Constants.UpdateSlideIndex, (senderClassName, index) =>
            {
                if (senderClassName != nameof(TaskSlideViewModel))
                    return;

                CurrentIndex = index;
                SelectedTask = TaskFilterControl.FilteredList?.ElementAt(CurrentIndex);
                DeeplinkButtonVisible = SelectedTask?.DeepLinkId.HasValue ?? false;
                MoreInfoButtonVisible = SelectedTask?.HasExtraInformation ?? false;
                UpdateSelected();
                UpdatePager();
                UpdateTitle();
            });

            await base.Init();
        }

        private void FilterAndRecalculateTasks()
        {
            if (TaskFilterControl != null && !TaskFilterControl.StatusFilters.IsNullOrEmpty())
            {
                TaskFilterControl.Filter(TaskFilterControl.StatusFilters, resetIfTheSame: false, useDataSource: false);
            }
            TaskHelper.CalculateTaskAmounts(TaskFilterControl);
        }

        private void SkipTaskIndex()
        {
            //if filters different than skipped are active no need for manual skip
            if (TaskFilterControl.StatusFilters.Any() && (!TaskFilterControl.StatusFilters?.Contains(TaskStatusEnum.Skipped) ?? false))
                return;

            if (CurrentIndex + 1 < TaskFilterControl.FilteredList?.Count)
                CurrentIndex++;

            if (CurrentIndex < 0 || CurrentIndex >= TaskFilterControl.FilteredList.Count()) return;

            SelectedTask = TaskFilterControl.FilteredList?.ElementAt(CurrentIndex);
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<TaskViewModel>(this, Constants.RecalculateAmountsMessage);
                MessagingCenter.Unsubscribe<string, int>(this, Constants.UpdateSlideIndex);
                MessagingCenter.Unsubscribe<PictureProofViewModel>(this, Constants.PictureProofChanged);
                MessagingCenter.Unsubscribe<MessageService, BasicTaskModel>(this, Constants.LinkedChecklistSigned);
            });
            _taskService.Dispose();
            _taskTemplateService.Dispose();
            _propertyService.Dispose();
            SelectedTask = null;
            PropertyEditViewModel = null;
            base.Dispose(disposing);
        }

        public override async Task ApplyFilter(TaskStatusEnum? status = null, bool reset = true)
        {
            TaskFilterControl.Filter(status, useDataSource: false, resetIfTheSame: reset);
            UpdatePager();
            if (Microsoft.Maui.Devices.DeviceInfo.Idiom == DeviceIdiom.Phone && DeviceSettings.PhoneViewsEnabled)
            {
                UpdateSelected();
            }

            if (SelectedTask?.FilterStatus != status)
            {
                await MainThread.InvokeOnMainThreadAsync(() => CurrentIndex = 0);
            }
        }

        #endregion


        /// <summary>
        /// Called when <see cref="SelectedTask"/> is changed.
        /// </summary>
        /// <remarks>IDE marks this method as unused because the call to this method is injected by Fody at compile time</remarks>
        private async void OnSelectedTaskChanged()
        {
            try
            {
                await AsyncAwaiter.AwaitAsync("SelectedTaskChanged", async () =>
                 {

                     await Task.CompletedTask;
                     DeeplinkButtonVisible = SelectedTask?.DeepLinkId.HasValue ?? false;
                     MoreInfoButtonVisible = SelectedTask?.HasExtraInformation ?? false;
                     UpdateSelected();
                     UpdatePager();
                     UpdateTitle();
                 });
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }

        #region Selected Task properties

        private void UpdatePager()
        {
            if (SelectedTask != null)
            {
                if (CurrentIndex == -1) { Pager = string.Empty; }
                else
                {
                    string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskPageNumberText);

                    var selectedTaskIndex = TaskFilterControl.FilteredList.IndexOf(SelectedTask);

                    Pager = string.Format(result.ReplaceLanguageVariablesCumulative(), (selectedTaskIndex + 1), TaskFilterControl.FilteredList.Count());
                }
            }
            else { Pager = string.Empty; }
        }

        private void UpdateSelected()
        {
            if (TaskFilterControl?.FilteredList != null)
            {
                lock (TaskFilterControl?.FilteredList)
                {
                    TaskFilterControl?.FilteredList?.ForEach(x =>
                    {
                        if (x != null)
                            x.IsSelected = false;
                    });
                    if (SelectedTask != null)
                    {
                        var foundTask = TaskFilterControl?.FilteredList?.FirstOrDefault(t => t.Id == SelectedTask.Id);
                        if (foundTask != null)
                            foundTask.IsSelected = true;

                        SelectedTask.IsSelected = true;
                    }
                }
            }
        }

        private void UpdateTitle()
        {
            if (SelectedTask != null)
            {
                Title = SelectedTask.AreaPath ?? Settings.WorkAreaName;
            }
            else
            {
                Title = string.Empty;
            }
        }

        #endregion

        #region Popup

        private void OpenFeaturePopup(BasicTaskPropertyModel prop)
        {
            PropertyEditViewModel = BaseTaskPropertyEditViewModel.FromPropertyModel(prop);
            IsPopupOpen = !IsPopupOpen;
            _statusBarService.HideStatusBar();
        }

        private async Task SubmitPopupAsync()
        {
            if (PropertyEditViewModel.TrySubmit())
            {
                var propertyUserValue = PropertyEditViewModel.GetValue();

                // Temporary solution until RealizedTime is removed from task template 
                if (PropertyEditViewModel.Property.IsPlannedTimeProperty)
                {
                    if (int.TryParse(propertyUserValue.UserValueTime, out var time))
                    {
                        await _taskService.SetTaskRealizedTimeAsync(SelectedTask.Id, time);

                        SelectedTask.TimeTaken = time;
                        SelectedTask.TimeRealizedBy = UserSettings.Fullname;
                        await _taskService.AlterTaskCacheDataAsync(SelectedTask);
                    }
                }
                else
                {
                    propertyUserValue.TaskId = (int)SelectedTask.Id;
                    await _propertyService.RegisterTaskPropertyValueAync(propertyUserValue);
                    SelectedTask.PropertyUserValues.Add(propertyUserValue);
                }

                // Make these automatic
                SelectedTask.RefreshPropertyValueString();
                PropertyEditViewModel.Property.UpdatePrimaryDisplayValue();
                PropertyEditViewModel.Property.UpdateDisplayType();
                PropertyEditViewModel.Property.Validate();

                await _taskService.AlterTaskCacheDataAsync(SelectedTask);
                IsPopupOpen = false;
                _statusBarService.HideStatusBar();
            }
        }

        private void CloseFeaturePopup()
        {
            IsPopupOpen = false;
            _statusBarService.HideStatusBar();
        }

        #endregion

        #region Navigation

        private async Task<TaskTemplateModel> GetTaskTemplate(int templateId)
        {
            var templates = await _taskTemplateService.GetAllTemplatesForCurrentAreaAsync();
            return templates?.FirstOrDefault(x => x.Id == templateId);
        }

        private async Task NavigateToSlideDetailAsync(BasicTaskModel selectedTask)
        {
            using var scope = App.Container.CreateScope();
            var itemsDetailViewModel = scope.ServiceProvider.GetService<ItemsDetailViewModel>();
            itemsDetailViewModel.Items = new List<Interfaces.Utils.IDetailItem>(TaskFilterControl.FilteredList);
            itemsDetailViewModel.SelectedItem = selectedTask;
            itemsDetailViewModel.SenderClassName = nameof(TaskSlideViewModel);
            itemsDetailViewModel.CommentString = selectedTask.CommentString;
            itemsDetailViewModel.HasComment = selectedTask.HasComment;
            await NavigationService.NavigateAsync(viewModel: itemsDetailViewModel);
        }

        private async Task NavigateToEditTaskAsync(BasicTaskModel task)
        {
            var template = await GetTaskTemplate(task.TemplateId);
            if (template != null)
            {
                using var scope = App.Container.CreateScope();
                var editTaskViewModel = scope.ServiceProvider.GetService<EditTaskViewModel>();
                editTaskViewModel.TemplateModel = EditTaskTemplateModel.FromExisting(template);

                await NavigationService.NavigateAsync(viewModel: editTaskViewModel);
            }
        }

        private async Task NavigateToExtraInformationAsync()
        {
            using var scope = App.Container.CreateScope();
            if (SelectedTask.HasWorkInstructions)
                await NavigateToWorkInstructions(SelectedTask.WorkInstructionRelations);

            if (SelectedTask.HasSteps)
            {
                var stepsViewModel = scope.ServiceProvider.GetService<StepsViewModel>();
                stepsViewModel.Steps = SelectedTask.Steps;
                stepsViewModel.Name = SelectedTask.Name;
                await NavigationService.NavigateAsync(viewModel: stepsViewModel);
                return;
            }

            if (SelectedTask.HasAttachments)
            {
                var attachement = SelectedTask.Attachments.FirstOrDefault();

                switch (SelectedTask.AttachmentType)
                {
                    case AttachmentEnum.Pdf:
                        var pdfViewerViewModel = scope.ServiceProvider.GetService<PdfViewerViewModel>();
                        pdfViewerViewModel.DocumentUri = attachement.Uri;
                        await NavigationService.NavigateAsync(viewModel: pdfViewerViewModel);
                        break;
                    case AttachmentEnum.Link:
                        await Launcher.OpenAsync(new Uri(attachement.Uri as string));
                        break;
                }
                return;
            }
        }

        private async Task OpenPopupOrNavigateToActionsAsync()
        {
            if (SelectedTask.ActionBubbleCount > 0)
            {
                using var scope = App.Container.CreateScope();
                var actionOpenActionsViewModel = scope.ServiceProvider.GetService<ActionOpenActionsViewModel>();
                actionOpenActionsViewModel.TaskId = SelectedTask.Id;
                actionOpenActionsViewModel.TaskTemplateId = SelectedTask.TemplateId;
                actionOpenActionsViewModel.ActionType = ActionType.Task;
                actionOpenActionsViewModel.TaskTitle = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forTaskItem)} {SelectedTask.Name}";

                await NavigationService.NavigateAsync(viewModel: actionOpenActionsViewModel);
            }
            else
            {
                IsActionPopupOpen = !IsActionPopupOpen;
            }
        }

        private async Task NavigateToNewActionAsync()
        {
            using var scope = App.Container.CreateScope();
            var actionNewViewModel = scope.ServiceProvider.GetService<ActionNewViewModel>();
            actionNewViewModel.TaskId = SelectedTask.Id;
            actionNewViewModel.TaskTemplateId = SelectedTask.TemplateId;
            actionNewViewModel.ActionType = ActionType.Task;
            actionNewViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forTaskItem)} {SelectedTask.Name}";

            await NavigationService.NavigateAsync(viewModel: actionNewViewModel);
        }

        private async Task NavigateToNewCommentAsync()
        {
            using var scope = App.Container.CreateScope();
            var taskCommentEditViewModel = scope.ServiceProvider.GetService<TaskCommentEditViewModel>();
            taskCommentEditViewModel.TaskId = SelectedTask.Id;
            taskCommentEditViewModel.TaskTemplateId = SelectedTask.TemplateId;
            taskCommentEditViewModel.IsNew = true;
            taskCommentEditViewModel.Type = ActionType.Task;
            taskCommentEditViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forTaskItem)} {SelectedTask.Name}";

            await NavigationService.NavigateAsync(viewModel: taskCommentEditViewModel);
        }

        public async override Task CancelAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagingCenter.Send(this, Constants.RecalculateAmountsMessage);
            });

            OnlineShiftCheck.IsShiftChangeAllowed = true;

            await OnlineShiftCheck.CheckCycleChange();

            await base.CancelAsync();
        }

        #endregion
    }
}
