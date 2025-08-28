using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Audits;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Utils;
using System.Windows.Input;


namespace EZGO.Maui.Core.ViewModels.Reports
{
    public class ReportFilterViewModel : BaseViewModel
    {
        private readonly IAuditsService _auditsService;

        private MenuLocation menuLocation;
        public MenuLocation MyLocation
        {
            get => menuLocation;
            set
            {
                menuLocation = value;

                OnPropertyChanged();
            }
        }

        private TimespanTypeEnum tempPeriod;

        private TimespanTypeEnum period;
        public TimespanTypeEnum Period
        {
            get => period;
            set
            {
                period = value;

                OnPropertyChanged();
            }
        }

        private List<AuditTemplateModel> audits;
        public List<AuditTemplateModel> Audits
        {
            get => audits;
            set
            {
                audits = value;

                OnPropertyChanged();
            }
        }

        public AuditTemplateModel SelectedAudit { get; set; }

        public ICommand PeriodCommand => new Command<object>(status =>
        {
            ExecuteLoadingAction(() =>
            {
                ChangePeriod(status);
            });
        }, CanExecuteCommands);

        public ICommand ApplyCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await Apply();
            });
        }, CanExecuteCommands);

        public ReportFilterViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IAuditsService auditsService) : base(navigationService, userService, messageService, actionsService)
        {
            _auditsService = auditsService;
        }

        public override async Task Init()
        {
            tempPeriod = Settings.ReportInterval;
            Period = Settings.ReportInterval;
            MyLocation = Settings.SubpageReporting;
            _statusBarService.HideStatusBar();

            if (MyLocation == MenuLocation.ReportAudits)
            {
                Audits = await Task.Run(async () => await _auditsService.GetReportAuditTemplatesAsync());
            }
            if (Settings.ReportAuditId != 0)
            {
                SelectedAudit = Audits?.FirstOrDefault(x => x.Id == Settings.ReportAuditId);
                if (SelectedAudit == null)
                {
                    Settings.ReportAuditId = 0;
                    MessagingCenter.Send(this, Constants.ReportAuditIdChanged);
                }
            }
            await base.Init();
        }

        private void ChangePeriod(object status)
        {
            if (status is TimespanTypeEnum result)
            {
                Period = result;
            }
        }

        private async Task Apply()
        {
            if (Settings.ReportAuditId != SelectedAudit?.Id)
            {
                Settings.ReportAuditId = SelectedAudit?.Id ?? 0;
                await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.ReportAuditIdChanged); });
            }

            if (Settings.ReportInterval != Period)
            {
                Settings.ReportInterval = Period;
                await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.ReportPeriodChanged); });
            }

            await CancelAsync();
        }

        protected override void Dispose(bool disposing)
        {
            _auditsService.Dispose();
            base.Dispose(disposing);
        }
    }
}
