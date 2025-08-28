using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Models.Messaging;
using EZGO.Maui.Core.Services.Message;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Classes;

namespace EZGO.Maui.Controls;

public partial class MessageCenter : StackLayout
{
    private string messageText;

    /// <summary>
    /// Gets or sets the message text.
    /// </summary>
    /// <value>
    /// The message text.
    /// </value>
    public string MessageText
    {
        get { return messageText; }
        set
        {
            messageText = value;

            OnPropertyChanged();
        }
    }

    private bool isMessageVisible;

    /// <summary>
    /// Gets or sets a value indicating whether the message is visible.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the message is visible; otherwise, <c>false</c>.
    /// </value>
    public bool IsMessageVisible
    {
        get { return isMessageVisible; }
        set
        {
            isMessageVisible = value;

            OnPropertyChanged();
        }
    }

    private Color messageBackgroundColor;

    /// <summary>
    /// Gets or sets the color which will fill the background of a VisualElement. This is a bindable property.
    /// </summary>
    /// <value>
    /// The color that is used to fill the background of a VisualElement. The default is <see cref="P:Xamarin.Forms.Color.Default" />.
    /// </value>
    /// <remarks>
    /// To be added.
    /// </remarks>
    public Color MessageBackgroundColor
    {
        get { return messageBackgroundColor; }
        set
        {
            messageBackgroundColor = value;

            OnPropertyChanged();
        }
    }

    private bool isClosable;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is closable.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is closable; otherwise, <c>false</c>.
    /// </value>
    public bool IsClosable
    {
        get { return isClosable; }
        set
        {
            isClosable = value;

            OnPropertyChanged();
        }
    }

    private MessageTypeEnum messageType;

    public MessageTypeEnum MessageType
    {
        get => messageType;
        set
        {
            messageType = value;

            OnPropertyChanged();
        }
    }

    private MessageIconTypeEnum messageIconType;
    public MessageIconTypeEnum MessageIconType
    {
        get => messageIconType;
        set
        {
            messageIconType = value;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageCenter"/> class.
    /// </summary>
    public MessageCenter()
    {
#if DEBUG
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        System.Diagnostics.Debug.WriteLine($"Started Creating new instance on MessageCenter: {watch.ElapsedMilliseconds}");
#endif
        BindingContext = this;

        InitializeComponent();

        MessagingCenter.Subscribe<MessageService, Message>(this, Constants.MessageCenterMessage, (formsApp, message) =>
        {
            MessageBackgroundColor = message.BackgroundColor;
            IsClosable = message.IsClosable;
            MessageIconType = message.IconType;
            MessageType = message.MessageType;
            MessageText = SetMessage(message);

            IsMessageVisible = message.MessageType != MessageTypeEnum.Clear;
            SetIconType();
        });

        MessagingCenter.Subscribe<MessageCenter>(this, Constants.MessageCenterCloseMessage, (mc) =>
        {
            if (IsClosable)
            {
                MessageBackgroundColor = Colors.Transparent;
                IsClosable = false;
                MessageType = MessageTypeEnum.Clear;
                MessageText = string.Empty;
                MessageIconType = MessageIconTypeEnum.None;
                IsMessageVisible = false;
                SetIconType();
            }
        });
#if DEBUG
        watch.Stop();
        System.Diagnostics.Debug.WriteLine($"Creating new instance of MessageCenter took: {watch.ElapsedMilliseconds}");
#endif
    }

    private void SetIconType()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (MessageIconType)
            {
                case MessageIconTypeEnum.Warning:
                    iconLabel.Text = IconFont.Warning;
                    iconLabel.IsVisible = true;
                    spinner.IsVisible = false;
                    break;
                case MessageIconTypeEnum.Info:
                    iconLabel.Text = IconFont.Info;
                    iconLabel.IsVisible = true;
                    spinner.IsVisible = false;
                    break;
                case MessageIconTypeEnum.Spinner:
                    iconLabel.Text = string.Empty;
                    iconLabel.IsVisible = false;
                    spinner.IsVisible = true;
                    break;

                case MessageIconTypeEnum.None:
                default:
                    iconLabel.Text = string.Empty;
                    iconLabel.IsVisible = false;
                    spinner.IsVisible = false;
                    break;
            }
        });
    }

    private string SetMessage(Message message)
    {
        string result = message.Text;
        if (message.MessageType == MessageTypeEnum.Connection)
        {
            // Set general connection message
            result = Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.syncStatesViewConnectionProblemMessage);

            if (Settings.MenuLocation == MenuLocation.None ||
                Settings.MenuLocation == MenuLocation.Tasks ||
                Settings.MenuLocation == MenuLocation.TasksAll ||
                Settings.MenuLocation == MenuLocation.TasksCompleted)
            {
                if (Settings.SubpageTasks == MenuLocation.TasksAll ||
                Settings.SubpageTasks == MenuLocation.TasksCompleted)
                {
                    result = Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.onlyOnlineAction);
                    IsClosable = false;
                }
            }
            else if (
                Settings.MenuLocation == MenuLocation.Report)
            {
                result = Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.noInternetConnectionReportsUnavailable);
                IsClosable = false;
            }
            else if (Settings.MenuLocation == MenuLocation.Assessments)
            {
                result = Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.noInternetConnectionAssessmentsUnavailable);
                IsClosable = false;
            }
        }

        return result;
    }

    private void SfButton_Clicked(object sender, System.EventArgs e)
    {
        MessagingCenter.Send(this, Constants.MessageCenterCloseMessage);
        IsMessageVisible = false;
    }
}
