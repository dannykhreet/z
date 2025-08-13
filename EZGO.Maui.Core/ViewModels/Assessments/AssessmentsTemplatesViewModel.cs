using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Assessments;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Assessments;
using EZGO.Maui.Core.Models.Messaging;
using EZGO.Maui.Core.Models.Tags;
using EZGO.Maui.Core.Services.Assessments;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Services.Message;
using EZGO.Maui.Core.Utils;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using Syncfusion.TreeView.Engine;
using System.Windows.Input;
using Command = Microsoft.Maui.Controls.Command;

namespace EZGO.Maui.Core.ViewModels.Assessments
{
    public class AssessmentsTemplatesViewModel : BaseViewModel
    {
        private readonly IAssessmentsService _assessmentsService;
        private readonly IWorkAreaService _workAreaService;
        private readonly IUpdateService _updateService;
        private readonly ISyncService _syncService;

        #region Properties

        public FilterControl<AssessmentsTemplateModel, SkillTypeEnum> AssessmentsTemplatesFilter { get; set; } = new FilterControl<AssessmentsTemplateModel, SkillTypeEnum>(null);

        public bool IsSearchBarVisible { get; set; }

        #endregion

        #region Commands

        public ICommand SearchTextChangedCommand { get; private set; }

        public ICommand DropdownTapCommand { get; set; }

        public ICommand DeleteTagCommand { get; private set; }

        public IAsyncCommand<AssessmentsTemplateModel> NavigateToAssessmentsTemplates { get; private set; }

        public IAsyncCommand NavigateToCompletedAssessmentsCommand { get; private set; }

        #endregion

        #region work areas dropdown

        public IWorkAreaFilterControl WorkAreaFilterControl { get; set; }

        #endregion

        private readonly SemaphoreSlim FifteenSecondLock = new SemaphoreSlim(1, 1);

        public bool HasCompletedAssessments { get; set; }

        public AssessmentsTemplatesViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IAssessmentsService assessmentsService,
            IWorkAreaFilterControl workAreaFilterControl,
            IUpdateService updateService,
            ISyncService syncService
             ) : base(navigationService, userService, messageService, actionsService)
        {
            _assessmentsService = assessmentsService;
            _updateService = updateService;
            _syncService = syncService;
            WorkAreaFilterControl = workAreaFilterControl;
            DropdownTapCommand = new Microsoft.Maui.Controls.Command<TreeViewNode>((obj) =>
            {
                IsDropdownOpen = false;
                WorkAreaFilterControl.DropdownTapAsync(obj, async () =>
                {
                    Settings.AssessmentsWorkAreaId = WorkAreaFilterControl.SelectedWorkArea?.Id ?? Settings.WorkAreaId;
                    await LoadAssessmentsTemplates();
                }, Settings.AssessmentsWorkAreaId);
            }, CanExecuteCommands);

            SearchTextChangedCommand = new Command((obj) =>
            {
                if (obj is string searchText)
                    AssessmentsTemplatesFilter.SearchText = searchText;
                AssessmentsTemplatesFilter.Filter(AssessmentsTemplatesFilter.StatusFilters, false, useDataSource: false);
            });

            DeleteTagCommand = new Microsoft.Maui.Controls.Command<Syncfusion.Maui.ListView.ItemTappedEventArgs>(obj =>
            {
                if (obj.DataItem is TagModel tag)
                {
                    AssessmentsTemplatesFilter.SearchedTags.Remove(tag);
                    tag.IsActive = !tag.IsActive;
                    AssessmentsTemplatesFilter.Filter(false, false);
                }

            }, CanExecuteCommands);

            NavigateToAssessmentsTemplates = new AsyncCommand<AssessmentsTemplateModel>(async (template) => await ExecuteLoadingActionAsync(() => NavigateToAssessmentTemplateAsync(template)));
            NavigateToCompletedAssessmentsCommand = new AsyncCommand(NavigateToCommpletedAssessments);
        }

        public override async Task Init()
        {
            if (!await InternetHelper.HasInternetConnection())
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.noInternetConnectionAssessmentsUnavailable);
                _messageService.SendMessage(result, Colors.Red, MessageIconTypeEnum.Warning, false, true, MessageTypeEnum.Connection);
                HasCompletedAssessments = false;
            }
            else
            {
                MessagingCenter.Subscribe<AssessmentsService>(this, Constants.AssessmentSigned, async (model) =>
                {
                    await RefreshAsync();
                });

                MessagingCenter.Subscribe<AssessmentsService>(this, Constants.AssessmentTemplateChanged, async (model) =>
                {
                    await RefreshAsync();
                });

                MessagingCenter.Subscribe<SyncService>(this, Constants.AssessmentTemplateChanged, async (model) =>
                {
                    await RefreshAsync();
                });

                MessagingCenter.Subscribe<CompletedAssessmentsViewModel>(this, Constants.AssessmentAreaChanged, async (model) =>
                {
                    await WorkAreaFilterControl.LoadWorkAreasAsync(Settings.AssessmentsWorkAreaId);
                    await RefreshAsync();
                });

                await WorkAreaFilterControl.LoadWorkAreasAsync(Settings.AssessmentsWorkAreaId);
                await LoadAssessmentsTemplates();
            }

            MessagingCenter.Subscribe<MessageService, Message>(this, Constants.MessageCenterMessage, async (formsApp, message) =>
            {
                if (message.MessageType == MessageTypeEnum.Clear)
                {
                    await WorkAreaFilterControl.LoadWorkAreasAsync(Settings.AssessmentsWorkAreaId);
                    await LoadAssessmentsTemplates();
                }
            });

            MessagingCenter.Subscribe<Application>(Application.Current, Constants.QuickTimer, async (sender) =>
            {
                try
                {
                    if (await FifteenSecondLock.WaitAsync(0))
                    {
                        await Task.Run(async () =>
                        {
                            if (await _updateService?.CheckForUpdatedAssessmentsAsync())
                            {
                                await _syncService.LoadAssessmentTemplatesAsync();
                                await LoadAssessmentsTemplates();
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    //Debugger.Break();
                }
                finally
                {
                    if (FifteenSecondLock.CurrentCount == 0)
                        FifteenSecondLock.Release();
                }

            });
            await base.Init();
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<MessageService, Message>(this, Constants.MessageCenterMessage);
            MessagingCenter.Unsubscribe<AssessmentsService>(this, Constants.AssessmentSigned);
            MessagingCenter.Unsubscribe<AssessmentsService>(this, Constants.AssessmentTemplateChanged);
            MessagingCenter.Unsubscribe<SyncService>(this, Constants.AssessmentTemplateChanged);
            MessagingCenter.Unsubscribe<CompletedAssessmentsViewModel>(this, Constants.AssessmentAreaChanged);
            MessagingCenter.Unsubscribe<Application>(Application.Current, Constants.QuickTimer);
            base.Dispose(disposing);
        }

        private async Task LoadAssessmentsTemplates(bool refresh = false)
        {
            if (_assessmentsService != null && WorkAreaFilterControl?.SelectedWorkArea != null)
            {
                var areaId = WorkAreaFilterControl.SelectedWorkArea.Id;
                var result = await _assessmentsService.GetAssessmentTemplates(areaId, refresh) ?? new List<AssessmentsTemplateModel>();
                AssessmentsTemplatesFilter.SetUnfilteredItems(result);
                AssessmentsTemplatesFilter.RefreshStatusFilter();
                HasCompletedAssessments = await _assessmentsService.HaveAnyCompletedAssessments();
            }
        }

        protected override async Task RefreshAsync()
        {
            await LoadAssessmentsTemplates(refresh: IsRefreshing);
        }

        private async Task NavigateToAssessmentTemplateAsync(AssessmentsTemplateModel assessment)
        {
            using (var scope = App.Container.CreateScope())
            {
                var vm = scope.ServiceProvider.GetService<AssessmentsViewModel>();
                vm.SelectedAssessmentTemplate = assessment;
                vm.AssessmentsTemplates = AssessmentsTemplatesFilter.UnfilteredItems;
                await NavigationService.NavigateAsync(viewModel: vm);
            }
        }

        private async Task NavigateToCommpletedAssessments()
        {
            await NavigationService.NavigateAsync<CompletedAssessmentsViewModel>();
        }
    }
}
