using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Models.Messaging
{
    /// <summary>
    /// Message
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is closable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is closable; otherwise, <c>false</c>.
        /// </value>
        public bool IsClosable { get; set; }

        /// <summary>
        /// Gets or sets the type of iceon to be displayed
        /// </summary>
        /// <value>The type of icon to display</value>
        public MessageIconTypeEnum IconType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the type of message.
        /// </summary>
        /// <value>
        ///   <c>MessageTypeEnum.General</c>.
        /// </value>
        public MessageTypeEnum MessageType { get; set; }

        public static Message Info(string message, bool isClosable = true, bool spinner = false)
        {
            return new Message()
            {
                BackgroundColor = ResourceHelper.GetApplicationResource<Color>("GreenColor"),
                IconType = spinner ? MessageIconTypeEnum.Spinner : MessageIconTypeEnum.Info,
                IsClosable = isClosable,
                MessageType = MessageTypeEnum.General,
                Text = message,
            };
        }

        public static Message Warning(string message, bool isClosable = true, bool spinner = false)
        {
            return new Message()
            {
                BackgroundColor = ResourceHelper.GetApplicationResource<Color>("RedColor"),
                IconType = spinner ? MessageIconTypeEnum.Spinner : MessageIconTypeEnum.Info,
                IsClosable = isClosable,
                MessageType = MessageTypeEnum.General,
                Text = message,
            };
        }
    }
}
