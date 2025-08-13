using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using EZGO.Maui.Core.Classes.LanguageResources;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.HealthCheck;
using EZGO.Maui.Core.ViewModels;
using EZGO.Maui.Core.ViewModels.Assessments;
using EZGO.Maui.Core.ViewModels.Feed;
using EZGO.Maui.Core.ViewModels.Menu;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes
{
    public static class StartupHelper
    {
        /// <summary>
        /// Gets startup page of the application.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        public static async Task<Page> NavigateToStartupPage()
        {
#if DEBUG
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Debug.WriteLine("[FormsApp::GetMainPageAsync]:: Getting main page");
#endif
            using var scope = App.Container.CreateScope();
            var healthService = scope.ServiceProvider.GetService<IHealthCheckService>();

            Page navigateToPage;

            // First check if the token is still valid
            if (await healthService.ValidateTokenAsync(Settings.Token))
            {
                navigateToPage = GetStartPage();
            }
            else
            {
                //UserSettings.PreferredLanguage = string.Empty;
                //var language = new Language();
                //await language.SetDefaultDictionary();

                Settings.WorkAreaId = 0;
                navigateToPage = ViewFactory.CreateView<LoginViewModel>();
            }

#if DEBUG
            stopwatch.Stop();
            Debug.WriteLine($"[FormsApp::GetMainPageAsync]:: Getting main page took {stopwatch.ElapsedMilliseconds}");
#endif            
            return navigateToPage;
        }

        /// <summary>
        /// Creates the startup page based on the current app condition.
        /// </summary>
        /// <returns>The type of viewmodel to navigate to.</returns>
        private static Page GetStartPage()
        {
            using var scope = App.Container.CreateScope();
            if (Settings.WorkAreaId > 0)
            {
                switch (Settings.MenuLocation)
                {
                    case MenuLocation.Checklist:
                        return ViewFactory.CreateView<ChecklistTemplatesViewModel>();
                    case MenuLocation.Tasks:
                        return ViewFactory.CreateView<TaskViewModel>();
                    case MenuLocation.Audits:
                        return ViewFactory.CreateView<AuditViewModel>();
                    case MenuLocation.Report:
                        return ViewFactory.CreateView<ReportViewModel>();
                    case MenuLocation.Actions:
                        return ViewFactory.CreateView<ActionViewModel>();
                    case MenuLocation.Home:
                        return ViewFactory.CreateView<HomeViewModel>();
                    case MenuLocation.Instructions:
                        return ViewFactory.CreateView<InstructionsViewModel>();
                    case MenuLocation.Assessments:
                        return ViewFactory.CreateView<AssessmentsTemplatesViewModel>();
                    case MenuLocation.Feed:
                        return ViewFactory.CreateView<FeedViewModel>();
                    case MenuLocation.Menu:
                        var menuViewModel = scope.ServiceProvider.GetService<MenuViewModel>();
                        menuViewModel.TabIndex = Settings.TabIndex;
                        return ViewFactory.CreateView(menuViewModel);
                    default:
                    case MenuLocation.None:
                        switch (Settings.SubpageReporting)
                        {
                            case MenuLocation.ReportActions:
                            case MenuLocation.ReportAudits:
                            case MenuLocation.ReportChecklists:
                            case MenuLocation.ReportTasks:
                                return ViewFactory.CreateView<ReportViewModel>();
                            default:
                                return ViewFactory.CreateView<HomeViewModel>();
                        }
                }
            }
            else
            {
                Settings.SubpageReporting = MenuLocation.None;
                return ViewFactory.CreateView<WorkAreaViewModel>();
            }
        }
    }
}
