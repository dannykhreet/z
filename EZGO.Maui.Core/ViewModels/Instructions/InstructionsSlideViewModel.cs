using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class InstructionsSlideViewModel : BaseViewModel
    {

        public FilterControl<InstructionItem, InstructionTypeEnum> InstructionsFilter { get; set; }

        public InstructionItem SelectedInstruction { get; set; } = new InstructionItem();

        public TimeManager TimeManager { get; set; } = new TimeManager();

        public int SelectedIndex { get; set; }

        public string Pager { get; set; }

        public ICommand DetailCommand { get; set; }

        public IAsyncCommand AttachmentsCommand => new AsyncCommand(NavigateToAttachments);

        public InstructionsSlideViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
            DetailCommand = new Microsoft.Maui.Controls.Command<InstructionItem>(async (item) => await ExecuteLoadingActionAsync(async () => await NavigateToDetailCommand(item)), CanExecuteCommands);
        }

        private async Task NavigateToDetailCommand(InstructionItem item)
        {
            using var scope = App.Container.CreateScope();
            var itemsDetailViewModel = scope.ServiceProvider.GetService<ItemsDetailViewModel>();
            itemsDetailViewModel.Items = new List<Interfaces.Utils.IDetailItem>(InstructionsFilter.FilteredList);
            itemsDetailViewModel.SelectedItem = item;
            itemsDetailViewModel.SenderClassName = nameof(InstructionsSlideViewModel);
            await NavigationService.NavigateAsync(viewModel: itemsDetailViewModel);
        }

        public override async Task Init()
        {
            await base.Init();
            UpdatePager();

            MessagingCenter.Subscribe<string, int>(this, Constants.UpdateSlideIndex, (senderClassName, index) =>
            {
                if (senderClassName != nameof(InstructionsSlideViewModel))
                    return;

                SelectedInstruction = InstructionsFilter.FilteredList.ElementAt(index);
                SelectedIndex = index;
                OnSelectedInstructionChanged();
            });
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<string, int>(this, Constants.UpdateSlideIndex);
            });
            base.Dispose(disposing);
        }

        public void OnSelectedInstructionChanged()
        {
            TimeManager.Stop();
            TimeManager.CurrentInstruction = SelectedInstruction;
            UpdatePager();
            TimeManager.Restart();
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
