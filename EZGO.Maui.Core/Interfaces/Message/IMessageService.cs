using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Models.Tasks;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Interfaces.Message
{
    /// <summary>
    /// Message service.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <param name="iconType">The type of icon to display next to the message</param>
        /// <param name="isClosable">Boolean indicating if the message is closable.</param>
        /// <param name="messageType">Enum indicating use of message.</param>
        void SendMessage(string text, Color backgroundColor, MessageIconTypeEnum iconType, bool isClosable = false, bool isIconVisible = false, MessageTypeEnum messageType = MessageTypeEnum.General, int? delay = null);

        /// <summary>
        /// Sends the message
        /// </summary>
        /// <param name="message">The message to send</param>
        void SendMessage(Models.Messaging.Message message);

        void SendLinkedItemSignedMessage(BasicTaskModel task);
        void SendClosableInfo(string message);
        void SendClosableWarning(string message);
    }
}
