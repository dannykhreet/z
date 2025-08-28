using CommunityToolkit.Mvvm.Input;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Assessments;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Assessments;
using EZGO.Maui.Core.PdfTemplates;
using EZGO.Maui.Core.Utils;
using MvvmHelpers;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using System.Diagnostics;

namespace EZGO.Maui.Core.ViewModels.Assessments
{
    public class CompletedAssessmentsViewModel : BaseViewModel
    {
        #region Private Fields

        private readonly IAssessmentsService _assessmentsService;

        private int currentOffset = 0;

        private int currentLimit = 0;

        private const int limit = 10;

        #endregion

        #region Properties

        public ObservableRangeCollection<AssessmentsModel> CompletedAssessments { get; set; }

        public AssessmentsModel SelectedAssessment { get; set; }

        private CancellationTokenSource generatePdfCancelationTokenSource;

        public bool IsBusy { get; set; }

        public IWorkAreaFilterControl WorkAreaFilterControl { get; set; }

        /// <summary>
        /// Indicated if the load more option should be available.
        /// In other words if there are more items available to load.
        /// </summary>
        public bool CanLoadMore { get; set; } = true;

        public bool HasSignatures { get; set; }

        public bool ContainsTags { get; set; }

        #endregion

        #region Commands

        public IAsyncCommand<object> SelectAssessmentCommand { get; private set; }

        public IAsyncCommand LoadMoreCommand { get; private set; }


        public IAsyncCommand<object> DropdownTapCommand { get; private set; }

        public IAsyncRelayCommand GeneratePdfCommand => new AsyncRelayCommand(async () =>
        {
            IsLoading = true;
            await GeneratePdfAsync(generatePdfCancelationTokenSource.Token);
        }, () => !IsLoading);

        #endregion

        #region Initialisation

        public CompletedAssessmentsViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IAssessmentsService assessmentsService,
            IWorkAreaFilterControl workAreaFilterControl) : base(navigationService, userService, messageService, actionsService)
        {
            _assessmentsService = assessmentsService;
            WorkAreaFilterControl = workAreaFilterControl;

            SelectAssessmentCommand = new AsyncCommand<object>(SelectAssessment);

            LoadMoreCommand = new AsyncCommand(LoadMore);

            DropdownTapCommand = new AsyncCommand<object>(async (obj) =>
            {
                IsDropdownOpen = false;
                await WorkAreaFilterControl.DropdownTapAsync(obj, async () =>
                {
                    Settings.AssessmentsWorkAreaId = WorkAreaFilterControl.SelectedWorkArea?.Id ?? Settings.WorkAreaId;
                    await RefreshAsync();
                }, Settings.AssessmentsWorkAreaId);
            });
        }


        public override async Task Init()
        {
            generatePdfCancelationTokenSource = new CancellationTokenSource();
            if (!await InternetHelper.HasInternetConnection())
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.noInternetConnectionAssessmentsUnavailable);
                _messageService.SendMessage(result, Colors.Red, MessageIconTypeEnum.Warning, false, true, MessageTypeEnum.Connection);
            }
            else
            {
                await Task.Run(async () =>
                {
                    await WorkAreaFilterControl.LoadWorkAreasAsync(Settings.AssessmentsWorkAreaId);

                    await Task.Run(() => LoadCompletedAssessmentsAsync());

                    await SelectAssessment(CompletedAssessments.FirstOrDefault());
                });
            }

            await base.Init();
        }

        public override Task CancelAsync()
        {
            MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.AssessmentAreaChanged); });
            return base.CancelAsync();
        }
        #endregion

        #region Private Methods
        private const string _cat = "[PdfGen]:\n\t";
        private async Task GeneratePdfAsync(CancellationToken token)
        {
            // Analytics.TrackEvent("Complete assessment PDF", new Dictionary<string, string>() {
            //     { "Company", string.Format("{0} ({1})", UserSettings.CompanyName.ToString(), UserSettings.CompanyId.ToString()) },
            //     { "Role", UserSettings.Role.ToString() }
            // });

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (DeviceInfo.Platform.Equals(DevicePlatform.Android))
                {
                    bool isAllowed = await CheckPermissions();

                    if (!isAllowed)
                    {
                        IsLoading = false;
                        return;
                    }
                }
                Debug.WriteLine("Checked permissions", _cat);
                var template = new CompletedAssessmentTemplate() { Model = SelectedAssessment };

                Debug.WriteLine("Loaded template", _cat);
                string html = template.GenerateString();

                Debug.WriteLine("Generated html", _cat);
                IPdfService pdfService = DependencyService.Get<IPdfService>();

                string pdfFilename = string.Empty;

                string datetimepart = DateTimeHelper.Now.ToString(Constants.PdfNameDateTimeFormat, null);
                string idpart = SelectedAssessment.Id.ToString();
                Debug.WriteLine("Setted FileName", _cat);
                pdfFilename = pdfService.SaveHtmlToPdf(html, $"assessment_{datetimepart}_{idpart}", async () =>
                {
                    if (token.IsCancellationRequested)
                        return;
                    Debug.WriteLine("Saving Html To Pdf", _cat);
                    using var scope = App.Container.CreateScope();
                    var assessmentPdfViewModel = scope.ServiceProvider.GetService<ChecklistPdfViewModel>();
                    assessmentPdfViewModel.PdfFilename = pdfFilename;
                    await NavigationService.NavigateAsync(viewModel: assessmentPdfViewModel);

                    Debug.WriteLine("Navigation ended", _cat);
                    IsLoading = false;
                });
            });
            //TODO Implement Pdf Generation/Display from Api.
        }

        private async Task<bool> CheckPermissions()
        {
            // On android 13 (and possibly next ones as well) we don't need permission for reading storage
            if (DeviceInfo.Version.Major >= 13)
                return true;

            var isStorageReadPermissionGranted = await PermissionsHelper.CheckAndRequestPermissionAsync<Permissions.StorageRead>();

            if (!isStorageReadPermissionGranted)
            {
                string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.contextMenuChooseMediaDialogCancel);
                string storageReadMessage = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionStorageMessage);
                string storageRead = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionStorage);

                Page page = NavigationService.GetCurrentPage();
                await page.DisplayAlert(storageRead, storageReadMessage, cancel);
            }
            return isStorageReadPermissionGranted;
        }

        private async Task SelectAssessment(object obj)
        {
            if (obj is AssessmentsModel assessment)
            {
                await LoadAssessmentModelById(assessment.Id);
                ContainsTags = assessment?.Tags?.Count > 0;
            }
            else if (obj is Syncfusion.Maui.ListView.ItemTappedEventArgs itemTapped && itemTapped.DataItem is AssessmentsModel selectedAssessment)
            {
                await LoadAssessmentModelById(selectedAssessment.Id);
                ContainsTags = selectedAssessment?.Tags?.Count > 0;
            }
        }

        private async Task LoadAssessmentModelById(int id)
        {
            SelectedAssessment = CompletedAssessments?.FirstOrDefault(c => c.Id == id);
            HasSignatures = SelectedAssessment?.NumberOfSignatures > 0;
        }

        protected override async Task RefreshAsync()
        {
            currentOffset = 0;
            currentLimit = limit;
            await LoadCompletedAssessmentsAsync(IsRefreshing);
            SelectedAssessment = CompletedAssessments.FirstOrDefault();
        }

        private async Task LoadCompletedAssessmentsAsync(bool refresh = false)
        {
            try
            {
                IsBusy = true;

                var assessments = await _assessmentsService.GetCompletedAssessments(WorkAreaFilterControl.SelectedWorkArea.Id, limit, currentOffset, refresh);

                if (currentOffset > 0)
                {
                    CompletedAssessments.AddRange(assessments.OrderByDescending(x => x.ModifiedAt));
                }
                else
                {
                    CompletedAssessments = new ObservableRangeCollection<AssessmentsModel>(assessments.OrderByDescending(x => x.ModifiedAt));
                }

                currentOffset += assessments.Count();
                currentLimit += limit;
                CanLoadMore = currentOffset == currentLimit;
                HasItems = CompletedAssessments.Any();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadMore()
        {
            if (CanLoadMore) await LoadCompletedAssessmentsAsync();
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            generatePdfCancelationTokenSource.Cancel();
            generatePdfCancelationTokenSource.Dispose();
            base.Dispose(disposing);
        }
    }
}
