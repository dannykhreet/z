using Autofac;
using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Classes.ListLayouts;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Assessments;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Assessments;
using EZGO.Maui.Core.Models.Users;
using EZGO.Maui.Core.Services.Assessments;
using EZGO.Maui.Core.Utils;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Command = Microsoft.Maui.Controls.Command;
using SelectionChangedEventArgs = Syncfusion.Maui.Inputs.SelectionChangedEventArgs;

namespace EZGO.Maui.Core.ViewModels.Assessments
{
    public class AssessmentsViewModel : BaseViewModel
    {
        private readonly IAssessmentsService _assessmentsService;
        private readonly IUpdateService _updateService;
        private readonly ISyncService _syncService;
        private readonly IInternetHelper _internetHelper;


        private BasicAssessmentModel _assessmentToDelete { get; set; }

        #region commands

        public IAsyncCommand<object> ShowUserAssessment { get; private set; }

        public IAsyncCommand<AssessmentSkillInstructionModel> NavigateToInstructionItems { get; private set; }

        public IAsyncCommand SignCommand { get; private set; }

        /// <summary>
        /// Gets the close user popup command.
        /// </summary>
        /// <value>
        /// The close user popup command.
        /// </value>
        public ICommand CloseUserPopupCommand => new Microsoft.Maui.Controls.Command(() => ExecuteLoadingAction(CloseUserPopup), CanExecuteCommands);

        /// <summary>
        /// Gets the auto complete selected command.
        /// </summary>
        /// <value>
        /// The auto complete selected command.
        /// </value>
        public ICommand AutoCompleteSelectedCommand => new Microsoft.Maui.Controls.Command<SelectionChangedEventArgs>(AutoCompleteSelected);

        /// <summary>
        /// Gets the remove resource command.
        /// </summary>
        /// <value>
        /// The remove resource command.
        /// </value>
        public ICommand RemoveResourceCommand => new Microsoft.Maui.Controls.Command<ResourceModel>((resourceModel) =>
        {
            ExecuteLoadingAction(() => RemoveResource(resourceModel));
        }, CanExecuteCommands);

        /// <summary>
        /// Gets the submit user popup command.
        /// </summary>
        /// <value>
        /// The submit user popup command.
        /// </value>
        public IAsyncRelayCommand SubmitUserPopupCommand => new AsyncRelayCommand(async () => await ExecuteLoadingActionAsync(SubmitUserPopup), CanExecuteCommands);

        public IAsyncRelayCommand SubmitDeleteParticipantPopupCommand => new AsyncRelayCommand(async () => await SubmitDeleteParticipantPopup(), CanExecuteCommands);

        /// <summary>
        /// Gets the open user popup command.
        /// </summary>
        /// <value>
        /// The open user popup command.
        /// </value>
        public ICommand OpenUserPopupCommand => new Command(() => ExecuteLoadingAction(OpenUserPopup), CanExecuteCommands);

        public ICommand OpenDeleteParticipantPopupCommand { get; private set; }

        public ICommand CloseDeleteParticipantPopupCommand => new Microsoft.Maui.Controls.Command(() => ExecuteLoadingAction(CloseDeleteParticipantPopup), CanExecuteCommands);

        public ICommand ListViewLayoutCommand => new Microsoft.Maui.Controls.Command<object>((listview) => ExecuteLoadingAction(() => SetListViewLayout(listview)), CanExecuteCommands);

        #endregion

        public AssessmentsTemplateModel SelectedAssessmentTemplate { get; set; }

        private List<BasicAssessmentModel> _assessments;
        public List<BasicAssessmentModel> Assessments
        {
            get => _assessments;
            set
            {
                if (_assessments != value)
                {
                    _assessments = value;
                    OnPropertyChanged(nameof(Assessments));

                    if (_assessments != null && _assessments.Any())
                    {
                        SelectedUserAssessment = _assessments.First();
                    }
                }
            }
        }

        private BasicAssessmentModel _selectedUserAssessment;
        public BasicAssessmentModel SelectedUserAssessment
        {
            get => _selectedUserAssessment;
            set
            {
                if (_selectedUserAssessment != value)
                {
                    _selectedUserAssessment = value;
                    OnPropertyChanged(nameof(SelectedUserAssessment));
                }
            }
        }

        public FilterControl<AssessmentSkillInstructionModel, SkillTypeEnum> UserSkillInstructionsFilter { get; set; } = new FilterControl<AssessmentSkillInstructionModel, SkillTypeEnum>(null);

        public FilterControl<AssessmentsTemplateModel, SkillTypeEnum> AssessmentsTemplateFilter { get; set; } = new FilterControl<AssessmentsTemplateModel, SkillTypeEnum>(null);

        public List<AssessmentsTemplateModel> AssessmentsTemplates { get; internal set; }

        public bool IsListVisible { get; set; }

        public bool IsBusy { get; set; } = false;

        public LayoutManager LayoutManager { get; set; } = new LayoutManager();

        public ListViewLayout ListViewLayout { get; set; }

        public bool ContainsTags => SelectedAssessmentTemplate?.Tags?.Count > 0;

        /// <summary>
        /// Gets or sets a value indicating whether the user popup is open.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user popup is open; otherwise, <c>false</c>.
        /// </value>
        public bool IsUserPopupOpen { get; set; }

        public bool IsDeleteParticipantPopupOpen { get; set; }

        /// <summary>
        /// Gets or sets the popup resources.
        /// </summary>
        /// <value>
        /// The popup resources.
        /// </value>
        public ObservableCollection<ResourceModel> PopupResources { get; set; }

        /// <summary>
        /// Gets or sets the selected resources.
        /// </summary>
        /// <value>
        /// The selected resources.
        /// </value>
        public List<ResourceModel> SelectedResources { get; set; } = new List<ResourceModel>();

        /// <summary>
        /// Gets or sets resources.
        /// </summary>
        /// <value>
        /// The users.
        /// </value>
        public ObservableCollection<ResourceModel> Resources { get; set; }

        /// <summary>
        /// Gets or sets the automatic complete text.
        /// </summary>
        /// <value>
        /// The automatic complete text.
        /// </value>
        public string AutoCompleteText { get; set; }


        public string CompletedInstructionsText
        {
            get
            {
                var translated = TranslateExtension.GetValueFromDictionary(LanguageConstants.completedInstructions);

                return string.Format(translated.ReplaceLanguageVariablesCumulative(), SelectedUserAssessment?.SkillInstructions?.Count(s => s.IsCompleted)) ?? "";
            }
        }

        public string AllInstructionsText
        {
            get
            {
                var translated = TranslateExtension.GetValueFromDictionary(LanguageConstants.allInstructions);

                return string.Format(translated.ReplaceLanguageVariablesCumulative(), SelectedUserAssessment?.SkillInstructions?.Count.ToString() ?? "");
            }
        }

        private readonly SemaphoreSlim FifteenSecondLock = new SemaphoreSlim(1, 1);


        public AssessmentsViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IAssessmentsService assessmentsService,
            IUpdateService updateService,
            ISyncService syncService,
            IInternetHelper internetHelper

             ) : base(navigationService, userService, messageService, actionsService)
        {
            _assessmentsService = assessmentsService;
            _updateService = updateService;
            _syncService = syncService;
            _internetHelper = internetHelper;

            ShowUserAssessment = new AsyncCommand<object>(ShowUserSkillInstructionsAsync);
            SignCommand = new AsyncCommand(NavigateToSignPageOrFinishAssessmentAsync);
            NavigateToInstructionItems = new AsyncCommand<AssessmentSkillInstructionModel>(async (template) => await ExecuteLoadingActionAsync(() => NavigateToInstructionItemsAsync(template)));
            OpenDeleteParticipantPopupCommand = new Microsoft.Maui.Controls.Command<object>((obj) => ExecuteLoadingAction(() => OpenDeleteParticipantPopup(obj)), CanExecuteCommands);
        }

        public override async Task Init()
        {
            if (!await _internetHelper.HasInternetConnection())
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.noInternetConnectionAssessmentsUnavailable);
                _messageService.SendMessage(result, Colors.Red, MessageIconTypeEnum.Warning, false, true, MessageTypeEnum.Connection);
            }
            else
            {
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
                                    var selectedUserAssessment = SelectedUserAssessment;
                                    await _syncService.LoadAssessmentsAsync();
                                    await UpdateAssessments();
                                    SetSelectedUserAssessment(selectedUserAssessment);
                                    await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.RecalculateAssessmentScore); });
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

                MessagingCenter.Subscribe<AssessmentsService>(this, Constants.AssessmentChangedScore, async (model) =>
                {
                    await RefreshAsync();
                    SetSelectedUserAssessment(SelectedUserAssessment);
                });

                MessagingCenter.Subscribe<AssessmentsService>(this, Constants.AssessmentAdded, async (model) =>
                {
                    await RefreshAsync();
                    SetSelectedUserAssessment(SelectedUserAssessment);
                });

                MessagingCenter.Subscribe<AssessmentsService>(this, Constants.AssessmentSigned, async (model) =>
                {
                    await RefreshAsync();
                    SetSelectedUserAssessment(SelectedUserAssessment);
                });

                await Task.Run(async () => await LoadAssessments());
                SetSelectedResources();
                await Task.Run(async () => await SetResourcesForAutocomplete());
                SetListViewLayout(Settings.ListViewLayout);
                //LayoutManager?.SetCurrentLayout();

                SetSelectedUserAssessment(Assessments?.FirstOrDefault());
            }
            await base.Init();
        }

        private async Task UpdateAssessments()
        {
            var assessments = await _assessmentsService.GetAssessments(SelectedAssessmentTemplate.Id);
            var basicAssessments = new List<BasicAssessmentModel>(assessments.ToBasicList<BasicAssessmentModel, AssessmentsModel>());

            var assessmentsToRemove = Assessments.Where(a => !basicAssessments.Any(bA => bA.Id == a.Id)).ToList();
            var assessmentsToAdd = basicAssessments.Where(bA => !Assessments.Any(a => a.Id == bA.Id)).ToList();

            foreach (var assessment in assessmentsToRemove)
            {
                var assessmentToRemove = Assessments.FirstOrDefault(a => a.Id == assessment.Id);
                if (assessmentToRemove != null)
                    Assessments.Remove(assessmentToRemove);
            }

            foreach (var assessment in Assessments)
            {
                var basicAssessment = basicAssessments.FirstOrDefault(a => a.Id == assessment.Id);
                if (basicAssessment != null)
                    assessment.SkillInstructions = basicAssessment.SkillInstructions;
            }

            Assessments.AddRange(assessmentsToAdd);
            Assessments = Assessments.OrderByDescending(a => a.Id).ToList();
            SetSelectedUserAssessment(Assessments?.FirstOrDefault());
        }

        protected override void RefreshCanExecute()
        {
            (OpenUserPopupCommand as Microsoft.Maui.Controls.Command)?.ChangeCanExecute();
            (OpenDeleteParticipantPopupCommand as Microsoft.Maui.Controls.Command)?.ChangeCanExecute();
            (SignCommand as Microsoft.Maui.Controls.Command)?.ChangeCanExecute();
            base.RefreshCanExecute();
        }

        private void SetSelectedUserAssessment(BasicAssessmentModel basicAssessmentModel)
        {
            if (Assessments == null)
            {
                SelectedUserAssessment = null;
                UserSkillInstructionsFilter.SetUnfilteredItems(SelectedUserAssessment?.SkillInstructions ?? new List<AssessmentSkillInstructionModel>());
                UserSkillInstructionsFilter.RefreshStatusFilter();
                OnPropertyChanged(nameof(SelectedUserAssessment));
                return;
            }

            if (basicAssessmentModel == null)
            {
                SelectedUserAssessment = Assessments.FirstOrDefault() ?? new BasicAssessmentModel();
                UserSkillInstructionsFilter.SetUnfilteredItems(SelectedUserAssessment?.SkillInstructions ?? new List<AssessmentSkillInstructionModel>());
                UserSkillInstructionsFilter.RefreshStatusFilter();
                OnPropertyChanged(nameof(SelectedUserAssessment));
                return;
            }

            SelectedUserAssessment = Assessments.FirstOrDefault(a => a.Id == basicAssessmentModel.Id);
            SelectedUserAssessment ??= Assessments.FirstOrDefault();
            UserSkillInstructionsFilter.SetUnfilteredItems(SelectedUserAssessment?.SkillInstructions ?? new List<AssessmentSkillInstructionModel>());
            UserSkillInstructionsFilter.RefreshStatusFilter();
            OnPropertyChanged(nameof(CompletedInstructionsText));
            OnPropertyChanged(nameof(SelectedUserAssessment));
        }

        private async Task LoadAssessments(bool refresh = false)
        {
            SelectedAssessmentTemplate ??= new AssessmentsTemplateModel();

            var assessments = await _assessmentsService.GetAssessments(SelectedAssessmentTemplate.Id, refresh);

            if (AssessmentsTemplates != null)
            {
                AssessmentsTemplateFilter.FilterCollection = AssessmentsTemplates?.Select(x => new FilterModel(x.Name, x.Id)).ToList();
                AssessmentsTemplateFilter.SetSelectedFilter(filterName: SelectedAssessmentTemplate?.Name, id: SelectedAssessmentTemplate?.Id ?? 0);
            }
            else
                AssessmentsTemplateFilter.SelectedFilter = new FilterModel(SelectedAssessmentTemplate?.Name, SelectedAssessmentTemplate?.Id ?? 0);

            var basicAssessments = new List<BasicAssessmentModel>(assessments.ToBasicList<BasicAssessmentModel, AssessmentsModel>());

            await Task.Run(() =>
            {
                Assessments = new List<BasicAssessmentModel>(basicAssessments).OrderByDescending(a => a.Id).ToList();
            });
        }

        protected override void Dispose(bool disposing)
        {
            _assessmentsService.Dispose();
            UserSkillInstructionsFilter.Dispose();
            AssessmentsTemplateFilter.Dispose();
            Assessments = null;
            LayoutManager = null;
            SelectedAssessmentTemplate = null;
            SelectedUserAssessment = null;

            MessagingCenter.Unsubscribe<AssessmentsService>(this, Constants.AssessmentAdded);
            MessagingCenter.Unsubscribe<AssessmentsService>(this, Constants.AssessmentSigned);
            MessagingCenter.Unsubscribe<Application>(Application.Current, Constants.QuickTimer);
            MessagingCenter.Unsubscribe<AssessmentsService>(this, Constants.AssessmentChangedScore);

            base.Dispose(disposing);
        }

        private async Task ShowUserSkillInstructionsAsync(object userSkillInstruction)
        {
            if (userSkillInstruction is Syncfusion.Maui.ListView.ItemTappedEventArgs eventArgs && eventArgs.DataItem is BasicAssessmentModel basicAssessmentsModel)
            {
                SetSelectedUserAssessment(basicAssessmentsModel);
            }
        }

        private async Task NavigateToInstructionItemsAsync(AssessmentSkillInstructionModel assessmentSkillInstructionModel)
        {
            using (var scope = App.Container.CreateScope())
            {
                var vm = scope.ServiceProvider.GetService<AssessmentInstructionItemsViewModel>();
                vm.SelectedUserAssessment = SelectedUserAssessment;
                vm.AssessmentSkillInstructionId = assessmentSkillInstructionModel.Id;
                await NavigationService.NavigateAsync(viewModel: vm);
            }
        }

        private async Task NavigateToSignPageOrFinishAssessmentAsync()
        {
            if (SelectedUserAssessment == null)
            {
                _messageService?.SendMessage("No assessment selected", Colors.Red, MessageIconTypeEnum.Warning);
                return;
            }
            if (SelectedUserAssessment.IsSignButtonEnabled)
            {
                if (SelectedUserAssessment.SignatureRequired)
                {
                    using var scope = App.Container.CreateScope();
                    var assessmentSignViewModel = scope.ServiceProvider.GetService<AssessmentSignViewModel>();

                    assessmentSignViewModel.AssessmentId = SelectedUserAssessment.Id;

                    await NavigationService.NavigateAsync(viewModel: assessmentSignViewModel);
                }
                else
                {
                    SelectedUserAssessment.IsCompleted = true;
                    SelectedUserAssessment.CompletedAt = DateTimeHelper.UtcNow;
                    SelectedUserAssessment.Version = SelectedAssessmentTemplate.Version;
                    var response = await _assessmentsService.PostChangeAssessment(SelectedUserAssessment);
                    if (response)
                    {
                        Assessments.Remove(SelectedUserAssessment);
                        SetSelectedUserAssessment(Assessments.FirstOrDefault());
                        await _assessmentsService.UpdateAssessmentCache(SelectedAssessmentTemplate.Id);
                        SetSelectedResources();
                        await SetResourcesForAutocomplete();

                        if (Assessments?.Count == 0)
                        {
                            using var scope = App.Container.CreateScope();
                            var vm = scope.ServiceProvider.GetService<AssessmentsTemplatesViewModel>();
                            await NavigationService.NavigateAsync(viewModel: vm);
                        }
                    }
                    else
                    {
                        SelectedUserAssessment.IsCompleted = false;
                    }
                }
            }
        }

        protected override async Task RefreshAsync()
        {
            var selectedUserAssessment = SelectedUserAssessment;
            await LoadAssessments(refresh: IsRefreshing);
            SetSelectedResources();
            await SetResourcesForAutocomplete();
            RefreshCanExecute();
            SetSelectedUserAssessment(selectedUserAssessment);
        }

        /// <summary>
        /// Closes the user popup.
        /// </summary>
        private void CloseUserPopup()
        {
            if (IsBusy)
                return;

            IsUserPopupOpen = false;
            if (PopupResources != null)
            {
                foreach (ResourceModel resourceModel in PopupResources)
                {
                    if (!SelectedResources.Contains(resourceModel) && !Resources.Contains(resourceModel))
                        Resources.Add(resourceModel);
                }
            }
            AutoCompleteText = string.Empty;
            RefreshCanExecute();
        }

        private void CloseDeleteParticipantPopup()
        {
            IsDeleteParticipantPopupOpen = false;
            _assessmentToDelete = null;
            MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.ResetParticipantSwipe); });
        }

        /// <summary>
        /// Handles the event when a selection is made in the auto complete control.
        /// </summary>
        /// <param name="args">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void AutoCompleteSelected(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.FirstOrDefault() is ResourceModel resourceModel)
            {
                PopupResources.Add(resourceModel);
                Resources.Remove(resourceModel);

                AutoCompleteText = string.Empty;
            }
        }

        /// <summary>
        /// Removes the resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        private void RemoveResource(ResourceModel resourceModel)
        {
            if (resourceModel != null)
            {
                PopupResources.Remove(resourceModel);
                Resources.Add(resourceModel);
            }
        }

        /// <summary>
        /// Submits the user popup.
        /// </summary>
        private async Task SubmitDeleteParticipantPopup()
        {
            IsBusy = true;
            if (_assessmentToDelete != null)
            {
                var assessmentsToDelete = new List<int>();
                assessmentsToDelete.Add(_assessmentToDelete.Id);
                var assessment = Assessments.FirstOrDefault(a => a.Id == _assessmentToDelete.Id);
                if (assessment != null)
                {
                    Assessments.Remove(assessment);
                }

                await _assessmentsService.DeleteAssessments(assessmentsToDelete, SelectedAssessmentTemplate);

                await _assessmentsService.UpdateAssessmentCache(SelectedAssessmentTemplate.Id);
                await RefreshAsync();
            }

            _assessmentToDelete = null;
            IsBusy = false;
            IsDeleteParticipantPopupOpen = false;
        }

        /// <summary>
        /// Submits the user popup.
        /// </summary>
        private async Task SubmitUserPopup()
        {
            IsBusy = true;
            var addedParticipants = PopupResources?.Where(s => !SelectedResources.Contains(s)).ToList() ?? new List<ResourceModel>();

            var assessments = await _assessmentsService.GetAssessments(SelectedAssessmentTemplate.Id, true);
            var existingAssessmentUserIds = assessments.Select(a => a.CompletedForId);
            var usersNotAdded = addedParticipants.Where(a => existingAssessmentUserIds.Contains(a.Id));
            if (usersNotAdded.Any())
            {
                addedParticipants = addedParticipants.Where(a => !existingAssessmentUserIds.Contains(a.Id)).ToList();
            }


            if (addedParticipants.Count > 0)
            {
                await _assessmentsService.PostAddAssessments(SelectedAssessmentTemplate, addedParticipants.Select(p => p.Id).ToList());
            }

            if (addedParticipants.Count > 0)
            {
                await _assessmentsService.UpdateAssessmentCache(SelectedAssessmentTemplate.Id);
                await RefreshAsync();
            }

            IsBusy = false;
            IsUserPopupOpen = false;
            PopupResources = null;
            if (usersNotAdded.Any())
                await DisplayUsersNotAddedPopup(usersNotAdded);

            AutoCompleteText = string.Empty;

            SetSelectedUserAssessment(Assessments?.FirstOrDefault());
        }

        private async Task DisplayUsersNotAddedPopup(IEnumerable<ResourceModel> usersNotAdded)
        {
            Page page = NavigationService.GetCurrentPage();
            string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCancel);
            string text = TranslateExtension.GetValueFromDictionary(LanguageConstants.assessmentNotAdded);

            foreach (var user in usersNotAdded)
            {
                text += $"{user.Text}, ";
            }
            await page.DisplayActionSheet(text, null, cancel);
        }

        /// <summary>
        /// Opens the user popup.
        /// </summary>
        private void OpenUserPopup()
        {
            SetSelectedResources();
            PopupResources = new ObservableCollection<ResourceModel>();
            IsUserPopupOpen = true;
        }

        /// <summary>
        /// Opens the delete participant popup.
        /// </summary>
        private void OpenDeleteParticipantPopup(object obj)
        {
            if (obj is Syncfusion.Maui.ListView.SwipingEventArgs swipingArgs && swipingArgs.DataItem is BasicAssessmentModel assessmentModel)
            {
                if (swipingArgs.OffSet > 75)
                {
                    swipingArgs.Handled = true;
                    _assessmentToDelete = assessmentModel;
                    IsDeleteParticipantPopupOpen = true;
                }
            }
        }

        private void SetSelectedResources()
        {
            SelectedResources = Assessments?.Select(a => new ResourceModel() { ActionResourceType = ActionResourceType.User, Id = a.CompletedForId ?? 0, Picture = a.Picture, Text = a.CompletedFor }).ToList();
            SelectedResources ??= new List<ResourceModel>();
        }

        private async Task SetResourcesForAutocomplete()
        {
            List<UserProfileModel> users = _userService != null ? await _userService.GetCompanyUsersAsync() : new List<UserProfileModel>();

            var selectedResourceIds = SelectedResources?.Select(s => s.Id);

            var userResources = users.Where(u => u.Id != UserSettings.Id && !selectedResourceIds.Contains(u.Id)).Select(u => new ResourceModel() { Id = u.Id, Text = u.FullName, ActionResourceType = ActionResourceType.User, Picture = u.Picture });

            Resources = new ObservableCollection<ResourceModel>(userResources);
        }

        private void SetListViewLayout(object obj)
        {
            if (obj is ListViewLayout listViewLayout)
            {
                if (listViewLayout == ListViewLayout) return;

                if (listViewLayout == ListViewLayout.Grid)
                    IsListVisible = false;
                else
                    IsListVisible = true;

                ListViewLayout = listViewLayout;
                Settings.ListViewLayout = listViewLayout;
            }
        }

    }
}
