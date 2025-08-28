using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using System.Windows.Input;
using Command = Microsoft.Maui.Controls.Command;

namespace EZGO.Maui.Core.ViewModels.AllTasks
{
    public class AllTasksSlideViewModel : BaseViewModel
    {
        public BasicTaskModel SelectedTask { get; set; }

        public List<BasicTaskModel> AllTasks { get; set; }

        public int CurrentIndex { get; set; }

        public string Pager { get; set; }

        public ICommand SelectionChangedCommand => new Command((obj) =>
        {
            if (Microsoft.Maui.Devices.DeviceInfo.Idiom == DeviceIdiom.Phone && DeviceSettings.PhoneViewsEnabled)
            {
                var swipe = (SwipeDirection)Enum.Parse(typeof(SwipeDirection), obj.ToString());
                if (swipe == SwipeDirection.Left)
                    CurrentIndex++;
                else
                    CurrentIndex--;
                if (CurrentIndex < AllTasks.Count() && CurrentIndex >= 0)
                    SelectedTask = AllTasks.ElementAt(CurrentIndex);
                else
                {
                    if (swipe == SwipeDirection.Left)
                        CurrentIndex--;
                    else
                        CurrentIndex++;
                }
            }
            ExecuteLoadingAction(OnSelectedTaskChanged);
        });

        public ICommand DetailCommand => new Microsoft.Maui.Controls.Command(() =>
        {
            ExecuteLoadingAction(async () => await NavigateToDetailAsync());
        }, CanExecuteCommands);

        public IAsyncCommand AttachmentsCommand => new AsyncCommand(NavigateToAttachments);

        public AllTasksSlideViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService)
            : base(navigationService, userService, messageService, actionsService)
        {
        }

        public override async Task Init()
        {
            UpdatePager();

            MessagingCenter.Subscribe<string, int>(this, Constants.UpdateSlideIndex, (senderClassName, index) =>
            {
                if (senderClassName != nameof(AllTasksSlideViewModel))
                    return;

                CurrentIndex = index;
                SelectedTask = AllTasks?.ElementAt(CurrentIndex);
                UpdatePager();
            });

            await base.Init();
        }

        private void OnSelectedTaskChanged()
        {
            UpdatePager();
        }

        private void UpdatePager()
        {
            if (SelectedTask != null)
            {
                if (CurrentIndex == -1) { Pager = string.Empty; }
                else
                {
                    string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskPageNumberText);

                    var selectedTaskIndex = AllTasks.FindIndex(x => x.Id == SelectedTask.Id);

                    Pager = string.Format(result.ReplaceLanguageVariablesCumulative(), (selectedTaskIndex + 1), AllTasks.Count());
                }
            }
            else { Pager = string.Empty; }
        }

        private async Task NavigateToDetailAsync()
        {
            using var scope = App.Container.CreateScope();
            var itemsDetailViewModel = scope.ServiceProvider.GetService<ItemsDetailViewModel>();
            itemsDetailViewModel.Items = new List<Interfaces.Utils.IDetailItem>(AllTasks);
            itemsDetailViewModel.SelectedItem = SelectedTask;
            itemsDetailViewModel.SenderClassName = nameof(AllTasksSlideViewModel);
            itemsDetailViewModel.CommentString = SelectedTask.CommentString;
            itemsDetailViewModel.HasComment = SelectedTask.HasComment;
            await NavigationService.NavigateAsync(viewModel: itemsDetailViewModel);
        }

        private async Task NavigateToAttachments()
        {
            using var scope = App.Container.CreateScope();
            if (SelectedTask.HasWorkInstructions)
                await NavigateToWorkInstructions(SelectedTask.WorkInstructionRelations);

            if (SelectedTask.HasSteps)
            {
                var stepsViewModel = scope.ServiceProvider.GetService<StepsViewModel>();
                stepsViewModel.Steps = SelectedTask.Steps;
                stepsViewModel.Name = SelectedTask.Name;
                await NavigationService.NavigateAsync(viewModel: stepsViewModel);
                return;
            }

            if (SelectedTask.HasAttachments)
            {
                var attachement = SelectedTask.Attachments.FirstOrDefault();

                switch (SelectedTask.AttachmentType)
                {
                    case AttachmentEnum.Pdf:
                        var pdfViewerViewModel = scope.ServiceProvider.GetService<PdfViewerViewModel>();
                        pdfViewerViewModel.DocumentUri = attachement.Uri;
                        await NavigationService.NavigateAsync(viewModel: pdfViewerViewModel);
                        break;
                    case AttachmentEnum.Link:
                        await Launcher.OpenAsync(new Uri(attachement.Uri as string));
                        break;
                }
                return;
            }
        }

        private async Task NavigateToWorkInstructions(List<InstructionsModel> workInstructionRelations)
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

                var selectedInstruction = await _instructionsService.GetInstruction(instructionId);
                var vm = scope.ServiceProvider.GetService<InstructionsItemsViewModel>();
                vm.Instructions = workInstructionRelations;
                vm.SelectedInstruction = selectedInstruction ?? new InstructionsModel();
                vm.IsFromDeeplink = true;

                await NavigationService.NavigateAsync(viewModel: vm);
            }
            return;
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<string, int>(this, Constants.UpdateSlideIndex);
            });
            base.Dispose(disposing);
        }
    }
}

