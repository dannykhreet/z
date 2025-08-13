using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Assessments;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Assessments;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using System.Windows.Input;
using Command = Microsoft.Maui.Controls.Command;

namespace EZGO.Maui.Core.ViewModels
{
    public class AssessmentsSlideViewModel : BaseViewModel
    {
        private readonly IAssessmentsService _assessmentsService;

        public int MinScore { get; set; }

        public int MaxScore { get; set; }

        public FilterControl<BasicAssessmentInstructionItemModel, InstructionTypeEnum> InstructionsFilter { get; set; }

        public BasicAssessmentInstructionItemModel SelectedInstruction { get; set; }

        public BasicAssessmentModel SelectedUserAssessment { get; set; }

        public List<AssessmentSkillInstructionModel> UserSkillInstructions { get; set; }

        public int SelectedIndex { get; set; }

        public string Pager { get; set; }

        public List<ScoreModel> Scores { get; set; } = new List<ScoreModel>();

        public double ScoreWidth { get; set; }

        public IScoreColorCalculator ColorCalculator { get; set; }

        public bool IsScorePopupOpen { get; set; }

        public ICommand OpenScoreCommand { get; set; }

        public IAsyncRelayCommand TaskScoreCommand { get; set; }

        public IAsyncCommand AttachmentsCommand { get; set; }

        public IAsyncCommand<BasicAssessmentInstructionItemModel> DetailCommand { get; set; }

        public AssessmentsSlideViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService, IAssessmentsService assessmentsService) : base(navigationService, userService, messageService, actionsService)
        {
            _assessmentsService = assessmentsService;
            OpenScoreCommand = new Command(() => ExecuteLoadingAction(() => IsScorePopupOpen = !IsScorePopupOpen), CanExecuteCommands);
            TaskScoreCommand = new AsyncRelayCommand<object>(str => ExecuteLoadingActionAsync(async () => await SetTaskScore(str)), CanExecuteCommands);
            DetailCommand = new AsyncCommand<BasicAssessmentInstructionItemModel>(NavigateToDetailCommand);
            AttachmentsCommand = new AsyncCommand(NavigateToAttachments);

        }

        private async Task NavigateToDetailCommand(BasicAssessmentInstructionItemModel obj)
        {
            using var scope = App.Container.CreateScope();
            var itemsDetailViewModel = scope.ServiceProvider.GetService<ItemsDetailViewModel>();
            itemsDetailViewModel.Items = new List<Interfaces.Utils.IDetailItem>(InstructionsFilter.FilteredList);
            itemsDetailViewModel.SelectedItem = obj;
            itemsDetailViewModel.SenderClassName = nameof(AssessmentsSlideViewModel);
            await NavigationService.NavigateAsync(viewModel: itemsDetailViewModel);
        }

        public override async Task Init()
        {
            await base.Init();
            UpdatePager();

            MessagingCenter.Subscribe<string, int>(this, Constants.UpdateSlideIndex, (senderClassName, index) =>
            {
                if (senderClassName != nameof(AssessmentsSlideViewModel))
                    return;

                SelectedInstruction = InstructionsFilter.FilteredList.ElementAt(index);
                OnSelectedInstructionChanged();
            });
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<string, int>(this, Constants.UpdateSlideIndex);
            base.Dispose(disposing);
        }

        public void OnSelectedInstructionChanged()
        {
            UpdatePager();
        }

        private void UpdatePager()
        {
            var treanslatedPager = TranslateExtension.GetValueFromDictionary(LanguageConstants.instructionPageNumberText);

            if (SelectedInstruction != null)
            {
                Pager = string.Format(treanslatedPager.ReplaceLanguageVariablesCumulative(), SelectedIndex + 1, InstructionsFilter.ItemsCount);
            }
            else
            {
                Pager = string.Empty;
            }
        }

        /// <summary>
        /// Set the task sore and sets status to Todo.
        /// </summary>
        /// <param name="score">The score value.</param>
        private async Task SetTaskScore(object score)
        {
            IsScorePopupOpen = false;

            if (SelectedInstruction == null)
            {
                SelectedInstruction = InstructionsFilter.UnfilteredItems.FirstOrDefault();
                SelectedIndex = 0;
            }

            if (score is int result)
            {
                if (result == SelectedInstruction.Score)
                {
                    SelectedInstruction.Score = null;
                }
                else
                {
                    SelectedInstruction.Score = result;
                }

                SelectedInstruction.NewScore = new ScoreModel { Number = result, MinimalScore = MinScore, NumberOfScores = Math.Abs(MaxScore - 0 + 1) };

                if (SelectedUserAssessment?.SkillInstructions?.FirstOrDefault(s => s.Id == SelectedInstruction.AssessmentSkillInstructionId)?.InstructionItems != null)
                {
                    SelectedUserAssessment.SkillInstructions.FirstOrDefault(s => s.Id == SelectedInstruction.AssessmentSkillInstructionId).InstructionItems = InstructionsFilter.UnfilteredItems;
                }

                await _assessmentsService.SetAssessmentScore(SelectedUserAssessment, SelectedInstruction);
            }
        }

        public override Task CancelAsync()
        {
            MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.RecalculateAssessmentScore); });

            return base.CancelAsync();
        }

        private async Task NavigateToAttachments()
        {
            using var scope = App.Container.CreateScope();

            var attachment = SelectedInstruction.Attachments.FirstOrDefault();

            if (attachment?.AttachmentType.ToLower() == "pdf")
            {
                var pdfViewerViewModel = scope.ServiceProvider.GetService<PdfViewerViewModel>();
                pdfViewerViewModel.DocumentUri = attachment.Uri;
                pdfViewerViewModel.Title = attachment.FileName;
                await NavigationService.NavigateAsync(viewModel: pdfViewerViewModel);
                return;
            }
            else
            {
                await Launcher.OpenAsync(new Uri(attachment.Uri as string));
            }
        }
    }
}
