using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Audits;
using EZGO.Maui.Core.Interfaces.Checklists;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using System.Windows.Input;
using MediaItem = EZGO.Maui.Core.Classes.MediaItem;

namespace EZGO.Maui.Core.ViewModels
{
    public class ActionTaskTemplateDetailViewModel : BaseViewModel
    {
        private readonly IAuditsService _auditService;
        private readonly IChecklistService _checklistService;
        private readonly ITasksService _taskService;
        public IScoreColorCalculator ScoreColorCalculator { get; set; }

        public ActionParentBasic ActionParent { get; set; }

        public ScoreTypeEnum ScoreType { get; set; } = ScoreTypeEnum.Thumbs;

        public bool HasTask { get; set; }

        public MediaItem MediaItem { get; set; }

        private TaskTemplateModel taskTemplate;

        private BasicTaskTemplateModel selectedTaskTemplate;

        public BasicTaskTemplateModel SelectedTaskTemplate
        {
            get => selectedTaskTemplate;
            set
            {
                selectedTaskTemplate = value;
                if (value != null)
                {
                    if (value.HasMediaItems)
                        MediaItem = value.HasVideo ? MediaItem.OnlineVideo(value.Video, value.VideoThumbnail) : MediaItem.OnlinePicture(value.Picture);
                    else
                        MediaItem = MediaItem.Picture(Constants.PlaceholderImage, true);

                }

                OnPropertyChanged();
            }
        }

        public ICommand DetailCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () => await NavigateToDetailAsync());
        }, CanExecuteCommands);

        public ICommand StepsCommand => new Command<BasicTaskTemplateModel>(obj =>
        {
            ExecuteLoadingAction(async () => await NavigateToMoreInfoAsync(obj));
        }, CanExecuteCommands);

        public ActionTaskTemplateDetailViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IAuditsService auditsService,
            IChecklistService checklistService,
            ITasksService tasksService) : base(navigationService, userService, messageService, actionsService)
        {
            _auditService = auditsService;
            _checklistService = checklistService;
            _taskService = tasksService;
        }

        public override async Task Init()
        {
            if (ActionParent != null)
            {
                Title = ActionParent.AuditTemplateName ?? ActionParent.ChecklistTemplateName ?? ActionParent.TaskName ?? "Title";
                await Task.Run(async () => await GetActionParentAsync());
            }

            await base.Init();
        }

        private async Task GetActionParentAsync()
        {
            int templateId;

            int taskTemplateId = ActionParent.TaskTemplateId ?? 0;

            if (ActionParent.ChecklistTemplateId != null)
            {
                var checklistTemplate = await _checklistService.GetChecklistTemplateAsync(ActionParent.ChecklistTemplateId ?? 0);

                taskTemplate = checklistTemplate?.TaskTemplates.FirstOrDefault(x => x.Id == taskTemplateId);

                await GetTemplate();
            }
            else if (ActionParent.AuditTemplateId != null)
            {
                templateId = ActionParent.AuditTemplateId ?? 0;

                AuditTemplateModel auditTemplate = await _auditService.GetAuditTemplateAsync(templateId);

                if (auditTemplate != null)
                {
                    ScoreColorCalculator = auditTemplate.ScoreColorCalculator;

                    taskTemplate = auditTemplate.TaskTemplates?.FirstOrDefault(task => task.Id == taskTemplateId);

                    Title = auditTemplate.Name ?? ActionParent.TaskName;
                    ScoreType = (ScoreTypeEnum)Enum.Parse(typeof(ScoreTypeEnum), auditTemplate.ScoreType ?? "thumbs", true);

                    await GetTemplate();
                }
            }
            else
            {
                taskTemplate = await _taskService.GetTaskTemplateAsync(taskTemplateId);

                Title = taskTemplate?.Name ?? string.Empty;

                await GetTemplate();
            }
        }

        private async Task GetTemplate()
        {
            if (taskTemplate != null)
            {
                SelectedTaskTemplate = taskTemplate?.ToBasic();

                await GetTask();
            }
        }

        private async Task GetTask()
        {
            if (ActionParent.TaskId != null)
            {
                BasicTaskModel task = await _taskService.GetTaskAsync(ActionParent.TaskId ?? 0, ActionParent.Type);

                if (task != null)
                {
                    Title = task.Name;
                    SelectedTaskTemplate.FilterStatus = task.FilterStatus;
                    SelectedTaskTemplate.Score = task.FilterStatus != TaskStatusEnum.Todo ? null : task.Score;
                    HasTask = true;
                }
            }
            else if (ActionParent.TaskTemplateId != null)
            {
                if (ActionParent.ChecklistTemplateId != null)
                {
                    var localChecklistTemplates = await _checklistService.GetLocalChecklistTemplatesAsync();
                    var localChecklistTemplate = localChecklistTemplates?.FirstOrDefault(item => item.Id == ActionParent.ChecklistTemplateId && item.UserId == UserSettings.Id);

                    if (localChecklistTemplate != null)
                    {
                        var task = localChecklistTemplate.TaskTemplates.FirstOrDefault(t => t.Id == ActionParent.TaskTemplateId);

                        if (task != null)
                        {
                            Title = taskTemplate.Name;
                            SelectedTaskTemplate.FilterStatus = task.Status ?? TaskStatusEnum.Todo;
                            HasTask = true;
                        }
                    }
                }
                else if (ActionParent.AuditTemplateId != null)
                {
                    var localAuditTemplates = await _auditService.GetLocalAuditTemplates();
                    var localAuditTemplate = localAuditTemplates?.FirstOrDefault(item => item.Id == ActionParent.AuditTemplateId && item.UserId == UserSettings.Id);

                    if (localAuditTemplate != null)
                    {
                        var task = localAuditTemplate.TaskTemplates.FirstOrDefault(t => t.Id == ActionParent.TaskTemplateId);

                        if (task != null)
                        {
                            Title = taskTemplate.Name;
                            SelectedTaskTemplate.FilterStatus = task.Status ?? TaskStatusEnum.Todo;
                            SelectedTaskTemplate.Score = task.Status != TaskStatusEnum.Todo ? null : task.Score;
                            HasTask = true;
                        }
                    }
                }
            }
        }

        private async Task NavigateToDetailAsync()
        {
            using var scope = App.Container.CreateScope();
            var actionTaskTemplateFullDetailViewModel = scope.ServiceProvider.GetService<ActionTaskTemplateFullDetailViewModel>();

            actionTaskTemplateFullDetailViewModel.SelectedTask = SelectedTaskTemplate;

            await NavigationService.NavigateAsync(viewModel: actionTaskTemplateFullDetailViewModel);
        }

        private async Task NavigateToMoreInfoAsync(object obj)
        {
            if (obj is BasicTaskTemplateModel taskTemplate)
            {
                using var scope = App.Container.CreateScope();
                if (taskTemplate.HasWorkInstructions)
                {
                    var workInstructionViewModel = scope.ServiceProvider.GetService<InstructionsViewModel>();
                    workInstructionViewModel.WorkInstructions = taskTemplate.WorkInstructionRelations;
                    workInstructionViewModel.IsFromDeeplink = true;
                    await NavigationService.NavigateAsync(viewModel: workInstructionViewModel);
                    return;
                }
                if (taskTemplate.HasDocument)
                {
                    var pdfViewerViewModel = scope.ServiceProvider.GetService<PdfViewerViewModel>();
                    pdfViewerViewModel.DocumentUri = taskTemplate.DescriptionFile;
                    pdfViewerViewModel.Title = taskTemplate.Name;

                    await NavigationService.NavigateAsync(viewModel: pdfViewerViewModel);
                }
                else if (taskTemplate.HasSteps && taskTemplate.StepsCount > 0)
                {
                    var stepsViewModel = scope.ServiceProvider.GetService<StepsViewModel>();
                    stepsViewModel.Steps = taskTemplate.Steps;
                    stepsViewModel.Name = taskTemplate.Name;

                    await NavigationService.NavigateAsync(viewModel: stepsViewModel);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            _auditService.Dispose();
            _checklistService.Dispose();
            _taskService.Dispose();

            base.Dispose(disposing);
        }
    }
}
