using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Assessments;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Assessments;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Utils;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels.Assessments
{
    public class AssessmentInstructionItemsViewModel : BaseViewModel
    {
        private readonly IAssessmentsService _assessmentsService;

        private BasicAssessmentInstructionItemModel _instructionItem;

        private int _SelectedItemIndex { get; set; }

        private const int _minScore = 1;
        private const int _maxScore = 5;

        public AssessmentSkillInstructionModel CurrentAssessmentSkillInstruction { get; set; }

        public List<BasicAssessmentInstructionItemModel> InstructionItems { get; set; }

        public FilterControl<BasicAssessmentInstructionItemModel, InstructionTypeEnum> InstructionsFilter { get; set; } = new FilterControl<BasicAssessmentInstructionItemModel, InstructionTypeEnum>(null);

        public List<AssessmentSkillInstructionModel> UserSkillInstructions { get; set; }

        public BasicAssessmentModel SelectedUserAssessment { get; set; }

        public int? AssessmentSkillInstructionId { get; set; }

        public bool IsListVisible { get; set; }
        public ListViewLayout ListViewLayout { get; set; }

        public IScoreColorCalculator ColorCalculator { get; set; } = new DefaultScoreColorCalculator(_minScore, _maxScore);


        #region Commands

        public ICommand ListViewLayoutCommand => new Command<object>((listview) => ExecuteLoadingAction(() => SetListViewLayout(listview)), CanExecuteCommands);

        public IAsyncRelayCommand<object> TaskScoreCommand => new AsyncRelayCommand<object>(async (str) => await ExecuteLoadingActionAsync(async () => await SetTaskScore(_instructionItem, str)), CanExecuteCommands);

        public ICommand NextCommand => new Command(() => NavigateToNextAssessmentSkillInstruction());
        public ICommand PreviousCommand => new Command(() => NavigateToPreviousAssessmentSkillInstruction());

        public ICommand NavigateToCarouselViewCommand { get; set; }


        public IAsyncRelayCommand<BasicAssessmentInstructionItemModel> OpenScoreCommand =>
               new AsyncRelayCommand<BasicAssessmentInstructionItemModel>(async instructionItem =>
               {
                   await ExecuteLoadingActionAsync(async () =>
                   {
                       instructionItem.Assessor = new UserBasic
                       {
                           Id = UserSettings.Id,
                           Name = UserSettings.Fullname,
                           Picture = UserSettings.UserPictureUrl
                       };
                       instructionItem.CompletedAt = DateTime.UtcNow;
                       _instructionItem = instructionItem;
                       await MainThread.InvokeOnMainThreadAsync(() =>
                       {
                           MessagingCenter.Send(this, Constants.ScorePopupMessage);
                       });
                   });
               }, CanExecuteCommands);


        #endregion
        public List<ScoreModel> Scores { get; set; } = new List<ScoreModel>();


        public double ScoreWidth { get; set; } = 615;

        public bool IsLeftArrowVisible => _SelectedItemIndex > 0;

        public bool IsRightArrowVisible => _SelectedItemIndex < UserSkillInstructions?.Count - 1;


        public AssessmentInstructionItemsViewModel(INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IAssessmentsService assessmentsService
             ) : base(navigationService, userService, messageService, actionsService)
        {
            _assessmentsService = assessmentsService;
        }

        public override async Task Init()
        {
            if (!await InternetHelper.HasInternetConnection())
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.noInternetConnectionAssessmentsUnavailable);
                _messageService.SendMessage(result, Colors.Red, MessageIconTypeEnum.Warning, false, true, MessageTypeEnum.Connection);
            }
            else
            {
                if (SelectedUserAssessment?.SkillInstructions != null && AssessmentSkillInstructionId != null)
                {
                    await LoadUserAssessment(SelectedUserAssessment.Id);
                }
                SetListViewLayout(Settings.ListViewLayout);

                LoadScores();

                MessagingCenter.Subscribe<AssessmentsSlideViewModel>(this, Constants.RecalculateAssessmentScore, async (model) =>
                {
                    await LoadUserAssessment(SelectedUserAssessment?.Id);
                });

                MessagingCenter.Subscribe<AssessmentsViewModel>(this, Constants.RecalculateAssessmentScore, async (model) =>
                {
                    await LoadUserAssessment(SelectedUserAssessment?.Id);
                });
            }
            NavigateToCarouselViewCommand = new Command<object>((obj) => ExecuteLoadingAction(() => NavigateToCarouselView(obj)), CanExecuteCommands);
            await base.Init();
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<AssessmentsSlideViewModel>(this, Constants.RecalculateAssessmentScore);
            MessagingCenter.Unsubscribe<AssessmentsViewModel>(this, Constants.RecalculateAssessmentScore);
            base.Dispose(disposing);
        }

        private async Task LoadUserAssessment(int? id)
        {
            if (id == null)
                return;

            UserSkillInstructions = SelectedUserAssessment.SkillInstructions ?? new List<AssessmentSkillInstructionModel>();
            CurrentAssessmentSkillInstruction = UserSkillInstructions.FirstOrDefault(s => s.Id == AssessmentSkillInstructionId) ?? new AssessmentSkillInstructionModel();
            //if (CurrentAssessmentSkillInstruction.StartDate == null)
            //{
            //    CurrentAssessmentSkillInstruction.StartDate = DateTime.UtcNow;
            //    await _assessmentsService.SetSkillInstructionStartDate(SelectedUserAssessment, CurrentAssessmentSkillInstruction);
            //}
            InstructionItems = CurrentAssessmentSkillInstruction.InstructionItems.OrderBy(x => x.Id).ToList();
            InstructionsFilter.SetUnfilteredItems(InstructionItems);
            InstructionsFilter.RefreshStatusFilter();
            _SelectedItemIndex = UserSkillInstructions.FindIndex(i => i.Id == CurrentAssessmentSkillInstruction.Id);
        }

        private void NavigateToNextAssessmentSkillInstruction()
        {
            GoToIndex(_SelectedItemIndex + 1);
        }

        private void NavigateToPreviousAssessmentSkillInstruction()
        {
            GoToIndex(_SelectedItemIndex - 1);
        }

        private void GoToIndex(int index)
        {
            if (index >= 0 && index < UserSkillInstructions.Count)
            {
                var item = UserSkillInstructions[index];
                CurrentAssessmentSkillInstruction = item;
                _SelectedItemIndex = index;
                AssessmentSkillInstructionId = CurrentAssessmentSkillInstruction.Id;
                InstructionItems = item.InstructionItems;
                InstructionsFilter.SetUnfilteredItems(InstructionItems);
            }
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

        /// <summary>
        /// Create the score buttons
        /// </summary>
        private void LoadScores()
        {
            var scores = new List<ScoreModel>();
            int numberOfScores = Math.Abs(_maxScore - _minScore + 1);
            for (int i = _minScore; i <= _maxScore; i++)
            {
                scores.Add(new ScoreModel
                {
                    Number = i,
                    NumberOfScores = numberOfScores,
                    MinimalScore = _minScore,
                    Color = ColorCalculator.GetColor(i),
                });
            }
            if (scores.Any()) { ScoreWidth = ((scores.Count * 60) + 15); }

            Scores = scores;
        }


        /// <summary>
        /// Set the instruction score.
        /// </summary>
        /// <param name="assessmentInstructionItem">The instruction.</param>
        /// <param name="score">The score value.</param>
        private async Task SetTaskScore(BasicAssessmentInstructionItemModel assessmentInstructionItem, object score)
        {
            if (score is int myscore)
            {
                if (myscore == assessmentInstructionItem.Score)
                {
                    assessmentInstructionItem.Score = null;
                    assessmentInstructionItem.Assessor = null;
                    assessmentInstructionItem.CompletedAt = null;
                }
                else
                {
                    assessmentInstructionItem.Score = myscore;
                }
                assessmentInstructionItem.NewScore = new ScoreModel { Number = myscore, MinimalScore = _minScore, NumberOfScores = Math.Abs(_maxScore - 0 + 1) };
                SelectedUserAssessment.SkillInstructions = UserSkillInstructions;
                await _assessmentsService.SetAssessmentScore(SelectedUserAssessment, assessmentInstructionItem);
                if (CurrentAssessmentSkillInstruction.IsCompleted && CurrentAssessmentSkillInstruction.EndDate == null)
                {
                    CurrentAssessmentSkillInstruction.EndDate = DateTime.UtcNow;
                    await _assessmentsService.SetSkillInstructionEndDate(SelectedUserAssessment, CurrentAssessmentSkillInstruction);
                }
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(this, Constants.HideScorePopupMessage);
                });

            }
        }

        protected override async Task RefreshAsync()
        {
            await LoadUserAssessment(SelectedUserAssessment?.Id);
        }

        private async Task NavigateToCarouselView(object obj)
        {
            using var scope = App.Container.CreateScope();
            var carousel = scope.ServiceProvider.GetService<AssessmentsSlideViewModel>();
            carousel.InstructionsFilter = InstructionsFilter;
            carousel.SelectedInstruction = obj as BasicAssessmentInstructionItemModel;
            carousel.SelectedIndex = InstructionsFilter.FilteredList.IndexOf(carousel.SelectedInstruction);
            carousel.Scores = Scores;
            carousel.ScoreWidth = ScoreWidth;
            carousel.ColorCalculator = ColorCalculator;
            carousel.IsScorePopupOpen = false;
            carousel.SelectedUserAssessment = SelectedUserAssessment;
            carousel.MinScore = _minScore;
            carousel.MaxScore = _maxScore;
            carousel.UserSkillInstructions = UserSkillInstructions;
            await NavigationService.NavigateAsync(viewModel: carousel);
        }

        public override Task CancelAsync()
        {
            MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.RecalculateAssessmentScore); });
            return base.CancelAsync();
        }
    }
}
