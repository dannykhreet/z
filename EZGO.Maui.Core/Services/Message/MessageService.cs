using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Services.Message
{
    /// <summary>
    /// Message service.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.Interfaces.Message.IMessageService" />
    public class MessageService : IMessageService
    {
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <param name="isClosable">Boolean indicating if the message is closable.</param>
        public void SendMessage(string text, Color backgroundColor, MessageIconTypeEnum iconType, bool isClosable = false, bool isIconVisible = false, MessageTypeEnum messageType = MessageTypeEnum.General, int? delay = null)
        {
            Models.Messaging.Message message = new Models.Messaging.Message
            {
                BackgroundColor = backgroundColor,
                IsClosable = isClosable,
                IconType = iconType,
                Text = text,
                MessageType = messageType
            };

            Models.Messaging.Message messageEmpty = new Models.Messaging.Message
            {
                BackgroundColor = Colors.Transparent,
                IsClosable = isClosable,
                IconType = MessageIconTypeEnum.None,
                Text = "",
                MessageType = MessageTypeEnum.Clear
            };
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(this, Constants.MessageCenterMessage, message);
            });
            if (delay != null)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(delay.Value);
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(this, Constants.MessageCenterMessage, messageEmpty);
                    });
                });
            }
        }

        public void SendMessage(Models.Messaging.Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.MessageCenterMessage, message); });
        }

        public void SendLinkedItemSignedMessage(BasicTaskModel task)
        {
            if (task == null)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(this, Constants.LinkedChecklistSigned, task);
            });
        }

        public void SendClosableInfo(string message)
        {
            Color greenColor = ResourceHelper.GetApplicationResource<Color>("GreenColor");
            SendMessage(message, greenColor, MessageIconTypeEnum.Info, isClosable: true, delay: 3000);
        }

        public void SendClosableWarning(string message)
        {
            Color orangeColor = ResourceHelper.GetApplicationResource<Color>("SkippedColor");
            SendMessage(message, orangeColor, MessageIconTypeEnum.None, isClosable: true, delay: 3000);
        }
    }
}
