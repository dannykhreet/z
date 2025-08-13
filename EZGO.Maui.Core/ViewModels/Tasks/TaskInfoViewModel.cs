using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Comments;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.ViewModels.Shared;
using EZGO.Maui.Core.ViewModels.Tasks;
using EZGO.Maui.Core.ViewModels.Tasks.CompletedTasks;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class TaskInfoViewModel : BaseViewModel
    {
        #region Public Properties

        public CompletedTaskListItemViewModel Task { get; set; }
        public ActionType? ActionType { get; set; }
        public List<CommentModel> LocalComments { get; set; }
        public List<ActionsModel> LocalActions { get; set; }
        public bool AreDatesVisible { get; set; } = true;
        public bool ContainsTags => Task?.Tags?.Count > 0;
        public StageModel Stage { get; internal set; }
        public bool HasStage => Stage?.Id > 0;

        #endregion

        #region Commands

        public ICommand DetailCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () => await NavigateToDetailAsync());
        }, CanExecuteCommands);

        public ICommand StepsCommand => new Command<CompletedTaskListItemViewModel>(obj =>
        {
            ExecuteLoadingAction(async () => await NavigateToMoreInfoAsync(obj));
        }, CanExecuteCommands);

        public ICommand NavigateToActionsCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToActionsAsync(obj);
            });
        }, CanExecuteCommands);

        public ICommand NavigateToPictureProofDetailsCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToPictureProofDetailsAsync(obj);
            });
        }, CanExecuteCommands);



        #endregion

        public TaskInfoViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
        }

        public override async Task Init()
        {
            await base.Init();
        }

        #region Private 

        private async Task NavigateToDetailAsync()
        {
            using var scope = App.Container.CreateScope();
            var taskSlideDetailViewModel = scope.ServiceProvider.GetService<TaskSlideDetailViewModel>();
            taskSlideDetailViewModel.SelectedTask = Task.ToBasic();
            taskSlideDetailViewModel.AreDatesVisible = AreDatesVisible;

            await NavigationService.NavigateAsync(viewModel: taskSlideDetailViewModel);
        }

        private async Task NavigateToMoreInfoAsync(object obj)
        {
            if (obj is CompletedTaskListItemViewModel taskTemplate)
            {
                using var scope = App.Container.CreateScope();
                if (taskTemplate.HasWorkInstructions)
                {
                    if (taskTemplate.WorkInstructionRelations.Count > 1)
                    {
                        var workInstructionViewModel = scope.ServiceProvider.GetService<InstructionsViewModel>();
                        workInstructionViewModel.WorkInstructions = taskTemplate.WorkInstructionRelations;
                        workInstructionViewModel.IsFromDeeplink = true;
                        await NavigationService.NavigateAsync(viewModel: workInstructionViewModel);
                    }
                    else
                    {
                        var _instructionsService = scope.ServiceProvider.GetService<IInstructionsService>();
                        var instructionId = taskTemplate.WorkInstructionRelations.FirstOrDefault()?.Id ?? 0;

                        var selectedInstruction = await _instructionsService.GetInstruction(instructionId);
                        var vm = scope.ServiceProvider.GetService<InstructionsItemsViewModel>();
                        vm.Instructions = taskTemplate.WorkInstructionRelations;
                        vm.SelectedInstruction = selectedInstruction ?? new InstructionsModel();
                        vm.IsFromDeeplink = true;

                        await NavigationService.NavigateAsync(viewModel: vm);
                    }
                    return;
                }
                if (taskTemplate.HasSteps)
                {
                    var stepsViewModel = scope.ServiceProvider.GetService<StepsViewModel>();
                    stepsViewModel.Steps = taskTemplate.Steps;
                    stepsViewModel.Name = taskTemplate.Name;
                    await NavigationService.NavigateAsync(viewModel: stepsViewModel);
                    return;
                }
                if (taskTemplate.HasDescriptionFile)
                {
                    var pdfViewerViewModel = scope.ServiceProvider.GetService<PdfViewerViewModel>();
                    pdfViewerViewModel.DocumentUri = taskTemplate.DescriptionFile;
                    pdfViewerViewModel.Title = taskTemplate.Name;
                    await NavigationService.NavigateAsync(viewModel: pdfViewerViewModel);
                    return;
                }
            }
        }

        private async Task NavigateToActionsAsync(object obj)
        {
            if (obj is CompletedTaskListItemViewModel item)
            {
                using var scope = App.Container.CreateScope();
                var actionOpenActionsViewModel = scope.ServiceProvider.GetService<ActionOpenActionsViewModel>();
                actionOpenActionsViewModel.TaskId = item.Id;
                actionOpenActionsViewModel.TaskTemplateId = item.TaskTemplateId;
                actionOpenActionsViewModel.ActionType = ActionType ?? Enumerations.ActionType.CompletedTask;

                if (LocalComments?.Any() ?? false)
                    actionOpenActionsViewModel.Comments = new ObservableRangeCollection<Models.Comments.CommentModel>(LocalComments);

                if (LocalActions?.Any() ?? false)
                    actionOpenActionsViewModel.Actions = new ObservableRangeCollection<Models.Actions.BasicActionsModel>(LocalActions.ToBasicList<BasicActionsModel, ActionsModel>());

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

        #endregion
    }
}
