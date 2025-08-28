using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Services.Instructions;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Audits;
using EZGO.Maui.Core.ViewModels.Checklists;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using MvvmHelpers.Interfaces;
using EZGO.Maui.Core.Interfaces.Checklists;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Core.ViewModels
{
    public abstract class BasicTaskViewModel : BaseViewModel
    {
        public bool IsChangeStatusPopupOpen { get; set; }

        public bool IsSkipTaskPopupOpen { get; set; }

        public bool IsSkipAllTasksPopupOpen { get; set; }

        public bool IsDiscardChangesPopupOpen { get; set; }

        public bool IsSkipAllAvailable { get; set; } = true;

        public Task DiscardChangesPopupClosed { get; set; }

        private TaskCompletionSource<bool> discardChangesTaskCompletionSource;

        protected TaskStatusEnum? CurrentStatus { get; set; }

        #region Commands
        public ICommand CloseChangeStatusPopupCommand { get; private set; }

        public ICommand CloseSkipTaskPopupCommand { get; private set; }

        public ICommand SubmitSkipTaskPopupCommand { get; private set; }

        public ICommand CloseSkipAllTasksPopupCommand { get; private set; }

        public ICommand SubmitSkipAllTasksPopupCommand { get; private set; }

        public ICommand RemoveButtonChangeStatusPopupCommand { get; private set; }

        public ICommand KeepButtonChangeStatusPopupCommand { get; private set; }

        public ICommand OpenSkipAllTasksPopupCommand { get; private set; }

        public IAsyncCommand OpenDiscardChangesPopupCommand => new MvvmHelpers.Commands.AsyncCommand(() => TogglePopup(IsDiscardChangesPopupOpen));
        public IAsyncCommand CancelDiscardChangesPopupCommand => new MvvmHelpers.Commands.AsyncCommand(CancelDiscardChangesPopup);
        public IAsyncCommand SubmitDiscardChangesPopupCommand => new MvvmHelpers.Commands.AsyncCommand(SubmitDiscardChangesPopup);
        public IAsyncCommand CloseDiscardChangesPopupCommand => new MvvmHelpers.Commands.AsyncCommand(CloseDiscardChangesPopup);
        #endregion

        protected BasicTaskViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
            SubmitSkipTaskPopupCommand = new Command(async () => await ExecuteLoadingActionAsync(SubmitSkipCommandAsync), CanExecuteCommands);
            CloseSkipTaskPopupCommand = new Command(() => IsSkipTaskPopupOpen = !IsSkipTaskPopupOpen);
            CloseChangeStatusPopupCommand = new Command(() => IsChangeStatusPopupOpen = !IsChangeStatusPopupOpen);
            RemoveButtonChangeStatusPopupCommand = new Command(async () => await ExecuteLoadingActionAsync(RemoveButtonChangeStatusPopupCommandAsync), CanExecuteCommands);
            KeepButtonChangeStatusPopupCommand = new Command(async () => await ExecuteLoadingActionAsync(KeepButtonChangeStatusPopupCommandAsync), CanExecuteCommands);
            CloseSkipAllTasksPopupCommand = new Command(() => IsSkipAllTasksPopupOpen = !IsSkipAllTasksPopupOpen);
            SubmitSkipAllTasksPopupCommand = new Command(async () => await ExecuteLoadingActionAsync(SubmitSkipAllTasksCommandAsync), CanExecuteCommands);
            OpenSkipAllTasksPopupCommand = new Command(OpenSkipAllTasksPopup);
        }

        private async Task TogglePopup(bool popupOpen)
        {
            await ExecuteLoadingActionAsync(() =>
            {
                popupOpen = !popupOpen;
                return Task.CompletedTask;
            });
        }

        public async virtual Task SubmitSkipAllTasksCommandAsync()
        {
            IsSkipAllTasksPopupOpen = !IsSkipAllTasksPopupOpen;
        }

        public void OpenSkipAllTasksPopup()
        {
            IsSkipAllTasksPopupOpen = IsSkipAllAvailable ? !IsSkipAllTasksPopupOpen : IsSkipAllTasksPopupOpen;
        }

        public void OpenSkipTaskPopup()
        {
            IsSkipTaskPopupOpen = !IsSkipTaskPopupOpen;
        }

        public void OpenChangeStatusPopup()
        {
            IsChangeStatusPopupOpen = !IsChangeStatusPopupOpen;
        }

        public async virtual Task SubmitSkipCommandAsync()
        {
            IsSkipTaskPopupOpen = !IsSkipTaskPopupOpen;
            CurrentStatus = null;
        }

        public async virtual Task RemoveButtonChangeStatusPopupCommandAsync()
        {
            IsChangeStatusPopupOpen = !IsChangeStatusPopupOpen;
            CurrentStatus = null;
        }

        public async virtual Task KeepButtonChangeStatusPopupCommandAsync()
        {
            IsChangeStatusPopupOpen = !IsChangeStatusPopupOpen;
            CurrentStatus = null;
        }

        public async virtual Task UntapTaskAsync()
        {

        }

        public async virtual Task SeePicturesAsync()
        {

        }

        public async virtual Task CancelDiscardChangesPopup()
        {
            discardChangesTaskCompletionSource.TrySetResult(true);
        }

        public async virtual Task SubmitDiscardChangesPopup()
        {
            discardChangesTaskCompletionSource.TrySetResult(true);
        }

        public async virtual Task CloseDiscardChangesPopup()
        {
            discardChangesTaskCompletionSource.TrySetResult(false);
        }

        public async Task OpenUntapConfirmationDialogAsync()
        {
            Page page = NavigationService.GetCurrentPage();

            string title = TranslateExtension.GetValueFromDictionary(LanguageConstants.untapTaskConfirmation);

            string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCancel);
            string yes = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertYesButtonTitle);

            string action = await page.DisplayActionSheet(title, null, cancel, null, yes);

            DependencyService.Resolve<IStatusBarService>().HideStatusBar();

            if (action == yes)
            {
                await UntapTaskAsync();
            }
        }

        public async Task OpenUntapTaskDialogAsync()
        {
            Page page = NavigationService.GetCurrentPage();


            string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCancel);
            string untapTask = TranslateExtension.GetValueFromDictionary(LanguageConstants.untapTaskText);
            string seePictures = TranslateExtension.GetValueFromDictionary(LanguageConstants.seePicturesText);

            string action = await page.DisplayActionSheet(null, null, cancel, untapTask, seePictures);

            DependencyService.Resolve<IStatusBarService>().HideStatusBar();

            if (action == untapTask)
            {
                await OpenUntapConfirmationDialogAsync();
            }
            else if (action == seePictures)
            {
                await SeePicturesAsync();
            }
        }

        public async Task OpenCantTapDialogAsync()
        {
            Page page = NavigationService.GetCurrentPage();
            string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCancel);
            string text = TranslateExtension.GetValueFromDictionary(LanguageConstants.cantTapText);
            await page.DisplayActionSheet(text, null, cancel);
        }


        protected async Task NavigateToDeepLinkAsync(BasicTaskModel task)
        {
            if (!task.DeepLinkId.HasValue)
                return;

            OnlineShiftCheck.IsShiftChangeAllowed = false;

            using var scope = App.Container.CreateScope();
            if (task.DeepLinkTo == "audit")
            {
                var auditTaskTemplatesViewModel = scope.ServiceProvider.GetService<AuditTaskTemplatesViewModel>();
                auditTaskTemplatesViewModel.AuditTemplateId = task.DeepLinkId.Value;
                auditTaskTemplatesViewModel.PagesFromDeepLink = 1;
                auditTaskTemplatesViewModel.TaskFromDeepLink = task;
                auditTaskTemplatesViewModel.DeepLinkCompletionIsRequired = task.DeepLinkCompletionIsRequired ?? false;

                await NavigationService.NavigateAsync(viewModel: auditTaskTemplatesViewModel);
            }
            else
            {
                await NavigateToChecklistDeepLinkAsync(task);
            }
        }

        private async Task NavigateToChecklistDeepLinkAsync(BasicTaskModel task)
        {
            using var scope = App.Container.CreateScope();

            var checklistService = scope.ServiceProvider.GetService<IChecklistService>();
            var incompleteChecklists = await checklistService.GetIncompleteDeeplinkChecklistsAsync(taskId: task.Id, refresh: true);
            if (incompleteChecklists.Count > 0)
            {
                var incompleteChecklistsViewModel = scope.ServiceProvider.GetService<IncompleteChecklistsViewModel>();
                incompleteChecklistsViewModel.ChecklistTemplateId = incompleteChecklists.FirstOrDefault().TemplateId;
                incompleteChecklistsViewModel.PagesFromDeepLink = 1;
                incompleteChecklistsViewModel.TaskFromDeepLink = task;
                incompleteChecklistsViewModel.DeepLinkCompletionIsRequired = task.DeepLinkCompletionIsRequired ?? false;
                await NavigationService.NavigateAsync(viewModel: incompleteChecklistsViewModel);
                return;
            }

            var taskTemplatesViewModel = scope.ServiceProvider.GetService<TaskTemplatesViewModel>();
            taskTemplatesViewModel.ChecklistTemplateId = task.DeepLinkId.Value;
            taskTemplatesViewModel.PagesFromDeepLink = 1;
            taskTemplatesViewModel.TaskFromDeepLink = task;
            taskTemplatesViewModel.DeepLinkCompletionIsRequired = task.DeepLinkCompletionIsRequired ?? false;
            taskTemplatesViewModel.ShouldClearStatuses = CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled;

            await NavigationService.NavigateAsync(viewModel: taskTemplatesViewModel);
        }

        protected async Task NavigateToWorkInstructions(List<InstructionsModel> workInstructionRelations)
        {
            using var scope = App.Container.CreateScope();
            if (workInstructionRelations.Count > 1)
            {
                var workInstructionViewModel = scope.ServiceProvider.GetService<InstructionsViewModel>();
                workInstructionViewModel.WorkInstructions = workInstructionRelations;
                workInstructionViewModel.IsFromDeeplink = true;
                await NavigationService.NavigateAsync(viewModel: workInstructionViewModel);
            }
            else
            {
                var _instructionsService = scope.ServiceProvider.GetService<IInstructionsService>();
                var instructionId = workInstructionRelations.FirstOrDefault()?.Id ?? 0;

                var selectedInstruction = await _instructionsService.GetInstruction(instructionId, refresh: true);
                var vm = scope.ServiceProvider.GetService<InstructionsItemsViewModel>();
                vm.Instructions = workInstructionRelations;
                vm.WorkInstructionTemplateId = selectedInstruction.Id;
                vm.SelectedInstruction = selectedInstruction ?? new InstructionsModel();
                vm.IsFromDeeplink = true;

                await NavigationService.NavigateAsync(viewModel: vm);
            }
            return;
        }

        protected Task<bool> WaitForDiscardChangesConfirmation()
        {
            discardChangesTaskCompletionSource = new TaskCompletionSource<bool>();
            return discardChangesTaskCompletionSource.Task;
        }
    }
}
