using Autofac;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Comments;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Services.Actions;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Tasks;
using MvvmHelpers;
using PropertyChanged;
using Syncfusion.Maui.Buttons;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;

namespace EZGO.Maui.Core.ViewModels
{
    public class ActionOpenActionsViewModel : BaseViewModel
    {

        #region Private Members

        private readonly string OpenActionsTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionsScreenOpenActions);
        private readonly string ActionsTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.sidebarTitleActions);
        private readonly string ActionsTitleFormatted = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionsScreenActionsFormatted);
        private readonly string CommentsTitleFormatted = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionsScreenCommentsFormated);
        private readonly string CommentsTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionDetailScreenCommentsSectionTitle);
        private readonly string ActionsSegment = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionSegement);
        private readonly string CommentsSegment = TranslateExtension.GetValueFromDictionary(LanguageConstants.commentSegement);
        private readonly ITaskCommentService _commentService;

        private bool openActionsOnly;

        #endregion

        #region Public Properties

        public ActionType ActionType { get; set; }

        public long? TaskId { get; set; }

        public int TaskTemplateId { get; set; }

        public bool CanAddAction { get; set; } = false;

        public string TaskTitle { get; set; }

        public bool CanAddComment { get; set; } = false;

        public bool CanEditComment { get; set; } = false;

        public ObservableRangeCollection<BasicActionsModel> Actions { get; set; }

        public ObservableRangeCollection<CommentModel> Comments { get; set; }

        public ObservableCollection<SfSegmentItem> Segments { get; set; } = new ObservableCollection<SfSegmentItem>();

        public int SelectedSegmentIndex { get; set; }

        public bool ActionsVisible => SelectedSegmentIndex == 0;

        public bool CommentsVisible => SelectedSegmentIndex == 1;

        public new bool HasItems => (ActionsVisible ? Actions?.Any() : Comments?.Any()) ?? false;

        /// <summary>
        /// Used only if <see cref="ActionType"/> is different than a <see cref="ActionType.Task"/>.
        /// </summary>
        /// <value>Stores current task template model with the most recent comments.</value>
        [DoNotNotify]
        public BasicTaskTemplateModel LocalTask { get; set; }

        #endregion

        #region Commands

        public ICommand NavigateToConversationCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToConversationAsync(obj);
            });
        }, CanExecuteCommands);

        public ICommand ActionSolvedCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await ToggleActionStatus(obj, ActionStatusEnum.Solved);
            });
        }, CanExecuteCommands);

        public ICommand NewActionCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NewActionAsync();
            });
        }, CanExecuteCommands);

        public ICommand NewCommentCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToCommentDetail(isNew: true);
            });
        }, CanExecuteCommands);

        public ICommand CommentTapCommand => new Command<object>((obj) =>
        {
            ExecuteLoadingAction(async () =>
            {
                if (obj is ItemTappedEventArgs item)
                {
                    await NavigateToCommentDetail(item.DataItem as CommentModel);
                }
            });
        }, CanExecuteCommands);

        #endregion

        public ActionOpenActionsViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            ITaskCommentService taskCommentService) : base(navigationService, userService, messageService, actionsService)
        {
            _commentService = taskCommentService;
        }

        public override async Task Init()
        {
            Segments.Add(new SfSegmentItem()
            {
                Text = ActionsSegment,
            });

            Segments.Add(new SfSegmentItem()
            {
                Text = CommentsSegment,
            });

            MessagingCenter.Subscribe<ActionsService>(this, Constants.ActionsChanged, async (actionService) =>
            {
                await LoadDataAsync();
            });

            MessagingCenter.Subscribe<ActionsService>(this, Constants.ActionChanged, async (actionService) =>
            {
                IsRefreshing = true;
                await LoadDataAsync();
                IsRefreshing = false;
            });

            MessagingCenter.Subscribe<TaskViewModel>(this, Constants.TaskCommentChanged, async _ =>
            {
                await RefreshAsync();
            });

            MessagingCenter.Subscribe<TaskCommentEditViewModel>(this, Constants.TaskCommentChanged, async _ =>
            {
                await RefreshAsync();
            });

            MessagingCenter.Subscribe<ActionsService, ActionCommentModel>(this, Constants.ChatChanged, async (sender, comment) =>
            {
                await LoadActionsAsync();
                LocalTask?.UpdateActionBubbleCount();
            });

            openActionsOnly = ActionType == ActionType.Audit ||
                ActionType == ActionType.Checklist ||
                ActionType == ActionType.Task;
            await Task.Run(async () => await LoadDataAsync());

            if (ActionType == ActionType.Audit ||
                ActionType == ActionType.Checklist ||
                ActionType == ActionType.Task)
            {
                CanAddAction = true;
                CanAddComment = true;
                CanEditComment = true;
            }

            string title;

            // If no actions
            if (Actions?.Count == 0)
            {
                // Check if no comments
                if (Comments?.Count == 0)
                {
                    // If no comments set actions page as default
                    if (openActionsOnly)
                        title = OpenActionsTitle;
                    else
                        title = ActionsTitle;
                }
                else
                {
                    // If we have comments but no actions switch to comments page
                    SelectedSegmentIndex = 1;
                    title = CommentsTitle;
                }
            }
            else
            {
                if (openActionsOnly)
                    title = OpenActionsTitle;
                else
                    title = ActionsTitle;
            }

            Title = title;

            await base.Init();
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<ActionsService>(this, Constants.ActionsChanged);
            MessagingCenter.Unsubscribe<ActionsService>(this, Constants.ActionChanged);
            MessagingCenter.Unsubscribe<TaskCommentEditViewModel>(this, Constants.TaskCommentChanged);
            MessagingCenter.Unsubscribe<ActionsService, ActionCommentModel>(this, Constants.ChatChanged);
            MessagingCenter.Unsubscribe<TaskViewModel>(this, Constants.TaskCommentChanged);

            LocalTask = null;
            Segments?.Clear();
            Actions?.Clear();
            Actions = null;
            Comments?.Clear();
            Comments = null;
            _commentService.Dispose();
            base.Dispose(disposing);
        }

        private async Task LoadDataAsync()
        {
            await LoadActionsAsync();
            await LoadCommentsAsync();

            LocalTask?.UpdateActionBubbleCount();

            // There seems to be a bug in Syncfusion segmented control on iOS
            // When you change the text of a segment like below it SOMETIMES causes a null reference exception
            // After the issue is resolved on the Syncfusion side this can be re-enabled
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                //TODO CHange ActionsBubbleCount to get set so we don't get few change refreshes
                if (Segments?.Count > 0)
                    Segments[0].Text = ActionsTitleFormatted.Format(Actions?.Count ?? 0);

                if (Segments?.Count > 1)
                    Segments[1].Text = CommentsTitleFormatted.Format(Comments?.Count ?? 0);

                //This is needed because SfSegmentedControl doesn't update on itself
                var updatedSegments = new ObservableCollection<SfSegmentItem>(Segments);
                Segments = updatedSegments;
                OnPropertyChanged(nameof(Segments));
            });
            UpdateTitle();
        }

        private async Task LoadActionsAsync()
        {
            var actionslist = new List<ActionsModel>();
            var allactions = await _actionService.GetActionsAsync(refresh: IsRefreshing, tasktemplateId: TaskTemplateId);

            if (TaskId.HasValue &&
                ActionType == ActionType.CompletedChecklistOrAudit ||
                ActionType == ActionType.CompletedTask)
            {
                actionslist = allactions.Where(x => x.TaskId == TaskId.Value).ToList();

                if (LocalTask?.LocalActions.Any() ?? false)
                    actionslist.AddRange(LocalTask.LocalActions);
            }

            if (ActionType == ActionType.Audit ||
                ActionType == ActionType.Checklist ||
                ActionType == ActionType.Task)
            {
                actionslist = allactions.Where(x => x.TaskTemplateId == TaskTemplateId).ToList();
            }

            if (actionslist.Any())
            {
                actionslist = actionslist.Distinct(new ActionsComparer(true)).ToList();
                if (ActionType == ActionType.Audit ||
                    ActionType == ActionType.Checklist ||
                    ActionType == ActionType.Task)
                {
                    actionslist = actionslist.Where(item => item.IsResolved.HasValue && !item.IsResolved.Value).ToList();
                }

                actionslist = actionslist.OrderByDescending(x => x.DueDate).ThenByDescending(x => x.Id).ToList();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Actions = new ObservableRangeCollection<BasicActionsModel>(actionslist.ToBasicList<BasicActionsModel, ActionsModel>());
                });
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Actions ??= new ObservableRangeCollection<BasicActionsModel>();
            });
        }

        private async Task LoadCommentsAsync()
        {
            List<CommentModel> comments;
            if (ActionType == ActionType.Task || ActionType == ActionType.CompletedTask)
            {
                comments = await _commentService.GetCommentsForTaskAsync((int)TaskId, refresh: true);
            }
            else if (ActionType == ActionType.CompletedChecklistOrAudit)
            {
                if (TaskId != 0)
                    comments = await _commentService.GetCommentsForTaskAsync((int)TaskId, refresh: true);
                else
                {
                    comments = new List<CommentModel>();
                    if (LocalTask?.LocalComments.Any() ?? false)
                        comments.AddRange(LocalTask.LocalComments);
                }
            }
            else
            {
                comments = LocalTask?.LocalComments ?? new List<CommentModel>();
            }



            comments = comments.OrderByDescending(x => x.CreatedAt).ToList();

            if (comments.Any())
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Comments = new ObservableRangeCollection<CommentModel>(comments);
                });
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Comments ??= new ObservableRangeCollection<CommentModel>(comments);
            });
        }

        protected override async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        private async Task ToggleActionStatus(object obj, ActionStatusEnum status)
        {
            if (obj is BasicActionsModel action)
            {
                if (status == ActionStatusEnum.Solved)
                {
                    if (action.FilterStatus != status)
                    {
                        Page page = NavigationService.GetCurrentPage();

                        string confirm = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertConfirmAction);
                        string yes = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertYesButtonTitle);
                        string no = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertNoButtonTitle);

                        string result = await page.DisplayActionSheet(confirm, null, null, yes, no);

                        if (result == yes && await _actionService.SetActionResolvedAsync(action))
                            action.FilterStatus = status;

                        _statusBarService.HideStatusBar();
                    }
                }
            }
        }

        private async Task NavigateToConversationAsync(object obj)
        {
            if (obj is ItemTappedEventArgs eventArgs)
            {
                if (eventArgs.DataItem is BasicActionsModel item)
                {
                    using var scope = App.Container.CreateScope();
                    var actionConversationViewModel = scope.ServiceProvider.GetService<ActionConversationViewModel>();

                    actionConversationViewModel.SelectedAction = item;
                    actionConversationViewModel.Actions = Actions.ToList();

                    await NavigationService.NavigateAsync(viewModel: actionConversationViewModel);
                }
            }
        }

        private async Task NewActionAsync()
        {
            using var scope = App.Container.CreateScope();
            var actionNewViewModel = scope.ServiceProvider.GetService<ActionNewViewModel>();

            actionNewViewModel.TaskId = TaskId;
            actionNewViewModel.TaskTemplateId = TaskTemplateId;
            actionNewViewModel.ActionType = ActionType;
            actionNewViewModel.Title = TaskTitle;
            actionNewViewModel.LocalTask = LocalTask;

            await NavigationService.NavigateAsync(viewModel: actionNewViewModel);
        }

        private async Task NavigateToCommentDetail(CommentModel model = null, bool isNew = false)
        {
            using var scope = App.Container.CreateScope();
            var taskCommentEditViewModel = scope.ServiceProvider.GetService<TaskCommentEditViewModel>();

            taskCommentEditViewModel.TaskId = TaskId ?? 0;
            taskCommentEditViewModel.TaskTemplateId = TaskTemplateId;
            taskCommentEditViewModel.IsNew = isNew;
            taskCommentEditViewModel.Type = ActionType;
            taskCommentEditViewModel.LocalTask = LocalTask;
            taskCommentEditViewModel.Comment = model;
            taskCommentEditViewModel.Title = TaskTitle;
            taskCommentEditViewModel.SupportsEditing = CanEditComment;

            await NavigationService.NavigateAsync(viewModel: taskCommentEditViewModel);
        }

        /// <summary>
        /// On-property changed method for <see cref="SelectedSegmentIndex"/>
        /// </summary>
#pragma warning disable IDE0051 // Remove unused private members
        private void OnSelectedSegmentIndexChanged()
        {
            UpdateTitle();
        }
#pragma warning restore IDE0051 // Remove unused private members

        private void UpdateTitle()
        {
            if (CommentsVisible)
            {
                Title = CommentsTitle;
            }
            else if (ActionsVisible)
            {
                if (openActionsOnly)
                    Title = OpenActionsTitle;
                else
                    Title = ActionsTitle;
            }
        }
    }
}
