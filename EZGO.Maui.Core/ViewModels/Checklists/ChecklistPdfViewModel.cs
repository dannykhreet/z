using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class ChecklistPdfViewModel : BaseViewModel
    {
        public ChecklistPdfViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
        }

        public string PdfFilename { get; set; }

        public Stream PdfDocumentStream { get; set; }

        public ICommand ShareCommand => new Command(async () =>
        {
            await Share.RequestAsync(new ShareFileRequest()
            {
                Title = "Share this document",
                File = new ShareFile(PdfFilename, "application/pdf"),
                PresentationSourceBounds = Microsoft.Maui.Devices.DeviceInfo.Platform == DevicePlatform.iOS
                            ? new Rect((int)(Application.Current.MainPage.Bounds.Width / 2), (int)(Application.Current.MainPage.Bounds.Height), 0, 0)
                            : Rect.Zero
            });
        }, CanExecuteCommands);

        public override async Task Init()
        {
            await base.Init();

            await Task.Run(async () =>
                  await LoadPdfDocumentStream()
            );
        }

        private async Task LoadPdfDocumentStream()
        {
            IFileService fileService = DependencyService.Get<IFileService>();

            string shortPdfFilename = PdfFilename.Split("/").Last();
            Stream pdfStream = await fileService.ReadFromInternalStorageAsBytesAsync(shortPdfFilename, "pdf/");

            PdfDocumentStream = pdfStream;
        }
    }
}
