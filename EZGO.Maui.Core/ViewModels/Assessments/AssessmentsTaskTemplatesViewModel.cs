using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Assessments;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Assessments;

namespace EZGO.Maui.Core.ViewModels.Assessments
{
    public class AssessmentsTaskTemplatesViewModel : BaseViewModel
    {
        private readonly IAssessmentsService _assessmentsService;

        public bool IsSignatureRequired { get; set; }

        public AssessmentsTemplateModel SelectedAssessment { get; set; }

        public FilterControl<AssessmentTemplateSkillInstructionModel, SkillTypeEnum> AssessmentsTemplatesFilter { get; set; } = new FilterControl<AssessmentTemplateSkillInstructionModel, SkillTypeEnum>(null);


        public AssessmentsTaskTemplatesViewModel(
             INavigationService navigationService,
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
            await Task.Run(async () => await LoadInstructionItems(SelectedAssessment.Id));

            await base.Init();
        }

        private async Task LoadInstructionItems(int id)
        {
            var items = await _assessmentsService.GetAssessmentTemplate(id);
            AssessmentsTemplatesFilter.SetUnfilteredItems(items.SkillInstructions);
        }

    }
}
