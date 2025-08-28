using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Models.Messaging;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.ShiftChecks
{
    public static class OnlineShiftCheck
    {
        public static bool IsShiftChangeAllowed { get; set; } = true;

        public static async Task CheckCycleChange()
        {
            var shiftChanged = await ShiftChanged.PerformChangeAsync().ConfigureAwait(false);

            // If the shift changed since the last check
            if (shiftChanged && IsShiftChangeAllowed)
            {
                using var scope = App.Container.CreateScope();

                // Send a message
                var messageCenter = scope.ServiceProvider.GetService<IMessageService>();
                messageCenter.SendMessage(Message.Info(Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.shiftChangeStarted), isClosable: false, spinner: true));

                // Update cached task information
                var taskService = scope.ServiceProvider.GetService<ITasksService>();
                await taskService.GetTasksForPeriodAsync(TaskPeriod.Shift, includeProperties: true, refresh: true);
                await taskService.GetTasksForPeriodAsync(TaskPeriod.OverDue, refresh: true);

                // Update tasks reports aswell
                var taskReportService = scope.ServiceProvider.GetService<ITaskReportService>();
                await taskReportService.GetTaskOverviewReportOnlyAsync(refresh: true);

                // If we're in tasks
                if (Settings.MenuLocation == MenuLocation.Tasks)
                {
                    // Go back to the main task page
                    var navigation = scope.ServiceProvider.GetService<INavigationService>();
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await navigation.NavigateAsync<TaskViewModel>(noHistory: true, animated: false);
                    });
                }

                taskService.Dispose();
                taskReportService.Dispose();

                // Send a message
                messageCenter.SendMessage(Message.Info(Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.shiftChangeCompleted), isClosable: true, spinner: false));

            }
            if (!IsShiftChangeAllowed)
                DebugService.WriteLine($"Pending shift change", "OnlineShiftCheck");
        }
    }
}
