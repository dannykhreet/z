using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Utils;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class PdfViewerViewModel : BaseViewModel
    {
        private Interfaces.Pdf.IPdfService _service;
        private IFileService _fileService;
        private string fullDocumentUri;

        public string DocumentUri { get; set; }

        public Stream PdfDocumentStream { get; set; }

        public ICommand ShareCommand => new Command(() =>
        {
            ExecuteLoadingAction(ShareAsync);
        }, CanExecuteCommands);

        public PdfViewerViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            Interfaces.Pdf.IPdfService pdfService) : base(navigationService, userService, messageService, actionsService)
        {
            _service = pdfService;
            _fileService = DependencyService.Get<IFileService>();
        }

        public override async Task Init()
        {
            IsLoading = true;
            fullDocumentUri = string.Format(Constants.MediaBaseUrl, DocumentUri);
            await Task.Run(async () => await LoadPdfDocumentStreamAsync());
            IsLoading = false;
            await base.Init();
        }

        private async Task LoadPdfDocumentStreamAsync()
        {
            PdfDocumentStream = await _service.GetPfdAsync(fullDocumentUri);
        }

        private async Task ShareAsync()
        {
            try
            {

                string filePath;
                string filename = new Uri(fullDocumentUri).Segments.LastOrDefault();

                await LoadPdfDocumentStreamAsync();

                var stream = PdfDocumentStream as MemoryStream;
                filePath = _fileService.SaveFileToInternalStorage(stream.ToArray(), filename, Constants.SessionDataDirectory);

                var file = new ShareFile(filePath)
                {
                    ContentType = "application/pdf",
                    FileName = filename,
                };

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = Title,
                    File = file,
                    PresentationSourceBounds = new Rect(0, 0, 1, 1),
                });
            }
            catch (Exception e)
            {

            }
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }
    }
}
