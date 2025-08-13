using System.Threading.Tasks;
using Autofac;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Models.Messaging;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Classes.ShiftChecks
{
    public class OfflineShiftCheck
    {
        public static async Task CheckCycleChange()
        {
            var shiftChanged = await ShiftChanged.PerformChangeAsync();

            if (shiftChanged)
            {
                using var scope = App.Container.CreateScope();
                var messageCenter = scope.ServiceProvider.GetService<IMessageService>();

                // Send a message
                messageCenter.SendMessage(Message.Info(Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.shiftChangeOfflineNotification), isClosable: true, spinner: false));
            }
        }
    }
}
