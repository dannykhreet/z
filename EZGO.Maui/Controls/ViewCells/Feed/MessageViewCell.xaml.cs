using System.Windows.Input;

namespace EZGO.Maui.Controls.ViewCells.Feed;

public partial class MessageViewCell : ViewCell
{
    public static readonly BindableProperty LikedUsersCommandProperty = BindableProperty.Create(nameof(LikedUsersCommand), typeof(ICommand), typeof(MessageViewCell));

    public ICommand LikedUsersCommand
    {
        get => (ICommand)GetValue(LikedUsersCommandProperty);
        set => SetValue(LikedUsersCommandProperty, value);
    }

    public static readonly BindableProperty LikeCommandProperty = BindableProperty.Create(nameof(LikeCommand), typeof(ICommand), typeof(MessageViewCell));

    public ICommand LikeCommand
    {
        get => (ICommand)GetValue(LikeCommandProperty);
        set => SetValue(LikeCommandProperty, value);
    }

    public static readonly BindableProperty CommentsCommandProperty = BindableProperty.Create(nameof(CommentsCommand), typeof(ICommand), typeof(MessageViewCell));

    public ICommand CommentsCommand
    {
        get => (ICommand)GetValue(CommentsCommandProperty);
        set => SetValue(CommentsCommandProperty, value);
    }

    public static readonly BindableProperty AddCommentCommandProperty = BindableProperty.Create(nameof(AddCommentCommand), typeof(ICommand), typeof(MessageViewCell));

    public ICommand AddCommentCommand
    {
        get => (ICommand)GetValue(AddCommentCommandProperty);
        set => SetValue(AddCommentCommandProperty, value);
    }

    public static readonly BindableProperty DeleteCommentCommandProperty = BindableProperty.Create(nameof(DeleteCommentCommand), typeof(ICommand), typeof(MessageViewCell));

    public ICommand DeleteCommentCommand
    {
        get => (ICommand)GetValue(DeleteCommentCommandProperty);
        set => SetValue(DeleteCommentCommandProperty, value);
    }

    public static readonly BindableProperty EditCommentCommandProperty = BindableProperty.Create(nameof(EditCommentCommand), typeof(ICommand), typeof(MessageViewCell));

    public ICommand EditCommentCommand
    {
        get => (ICommand)GetValue(EditCommentCommandProperty);
        set => SetValue(EditCommentCommandProperty, value);
    }

    public static readonly BindableProperty EditDeleteItemCommandProperty = BindableProperty.Create(nameof(EditDeleteItemCommand), typeof(ICommand), typeof(MessageViewCell));

    public ICommand EditDeleteItemCommand
    {
        get => (ICommand)GetValue(EditDeleteItemCommandProperty);
        set => SetValue(EditDeleteItemCommandProperty, value);
    }

    public static readonly BindableProperty SwipeStartedCommandProperty = BindableProperty.Create(nameof(SwipeStartedCommand), typeof(ICommand), typeof(MessageViewCell));

    public ICommand SwipeStartedCommand
    {
        get => (ICommand)GetValue(SwipeStartedCommandProperty);
        set => SetValue(SwipeStartedCommandProperty, value);
    }

    public MessageViewCell()
    {
        InitializeComponent();
    }
}
