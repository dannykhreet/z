using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Signatures;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Assessments;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Assessments;
using EZGO.Maui.Core.Utils;


namespace EZGO.Maui.Core.ViewModels.Assessments
{
    public class AssessmentSignViewModel : BaseViewModel
    {
        private readonly IAssessmentsService _assessmentsService;

        private AssessmentsModel assessment;

        public int AssessmentId { get; set; }

        public bool IsBusy { get; set; } = false;

        private string _coSignerName;
        public string CoSignerName
        {
            get => _coSignerName;
            set
            {
                _coSignerName = value;
                OnPropertyChanged();
            }
        }

        private bool buttonEnabled = true;
        public bool ButtonEnabled
        {
            get => buttonEnabled;
            set
            {
                buttonEnabled = value;

                OnPropertyChanged();
            }
        }

        public int PagesFromDeepLink { get; set; }

        public SignatureHelperControl SignatureHelper { get; set; }

        public AssessmentSignViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IAssessmentsService assessmentsService) : base(navigationService, userService, messageService, actionsService)
        {
            _assessmentsService = assessmentsService;
        }

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            if (!await InternetHelper.HasInternetConnection())
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.noInternetConnectionAssessmentsUnavailable);
                _messageService.SendMessage(result, Colors.Red, MessageIconTypeEnum.Warning, false, true, MessageTypeEnum.Connection);
            }
            else
            {
                Fullname = UserSettings.Fullname;
                assessment = await _assessmentsService.GetAssessment(AssessmentId);

                if (assessment.SignatureType == Api.Models.Enumerations.RequiredSignatureTypeEnum.TwoSignatureRequired)
                {
                    CoSignerName = assessment.CompletedFor;
                }

                SignatureHelper = new SignatureHelperControl(_assessmentsService)
                {
                    IsDoubleSignatureRequired = assessment.SignatureType == Api.Models.Enumerations.RequiredSignatureTypeEnum.TwoSignatureRequired,
                };

                MessagingCenter.Subscribe<SaveSignatureEventSender>(this, Constants.SignTemplateMessage, async (sender) =>
                {
                    if (sender == null)
                        return;

                    if (!SignatureHelper.SaveSignatureStreams(sender.FirstSignature, sender.SecondSignature)) return;

                    await Submit();
                });
            }

            await base.Init();
        }

        private async Task Submit()
        {
            ButtonEnabled = false;

            SignatureHelper.CoSignerName = CoSignerName;

            await SubmitAssessmentAndContinueAsync();

            IsBusy = false;
        }

        private async Task SubmitAssessmentAndContinueAsync()
        {
            var model = new PostTemplateModel(AssessmentId, assessment.Name, null, null, version: assessment.Version);
            await SignatureHelper.Submit(model);

            var assessmentTemplates = await _assessmentsService.GetAssessmentTemplates(Settings.WorkAreaId);
            var assessmentTemplate = assessmentTemplates.FirstOrDefault(a => a.Id == assessment.TemplateId);

            if (assessmentTemplate?.NumberOfOpenAssessments > 0)
            {
                await CancelAsync();
            }
            else
                await NavigationService.PopOrNavigateToPage<AssessmentsTemplatesViewModel>(typeof(AssessmentsTemplatesViewModel));
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<SaveSignatureEventSender>(this, Constants.SignTemplateMessage);
            SignatureHelper = null;
            base.Dispose(disposing);
        }
    }
}
