namespace EZGO.Maui.Core.Enumerations
{
    /// <summary>
    /// ImeAction enumeration for android
    /// </summary>
    public enum ImeAction
    {
        /// <summary>
        /// Generic unspecified type for Android.Views.InputMethods.EditorInfo.ImeOptions.
        /// </summary>
        ImeNull = 0,

        /// <summary>
        /// Bits of Android.Views.InputMethods.ImeAction.ImeMaskAction: no specific action
        /// has been associated with this editor, let the editor come up with its own
        /// if it can. 
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Bits of Android.Views.InputMethods.ImeAction.ImeMaskAction: there is no available action.
        /// </summary>
        None = 1,

        /// <summary>
        ///  Bits of Android.Views.InputMethods.ImeAction.ImeMaskAction: the action key performs
        ///  a "go" operation to take the user to the target of the text they typed. 
        ///  Typically used, for example, when entering a URL. 
        /// </summary>
        Go = 2,

        /// <summary>
        /// Bits of Android.Views.InputMethods.ImeAction.ImeMaskAction: the action key performs
        /// a "search" operation, taking the user to the results of searching for the
        /// text the have typed (in whatever context is appropriate).
        /// </summary>
        Search = 3,

        /// <summary>
        /// Bits of Android.Views.InputMethods.ImeAction.ImeMaskAction: the action key performs
        /// a "send" operation, delivering the text to its target. This is typically
        /// used when composing a message.
        /// </summary>
        Send = 4,

        /// <summary>
        /// Bits of Android.Views.InputMethods.ImeAction.ImeMaskAction: the action key performs
        ///  a "next" operation, taking the user to the next field that will accept text.
        /// </summary>
        Next = 5,

        /// <summary>
        /// Bits of Android.Views.InputMethods.ImeAction.ImeMaskAction: the action key performs a "done" 
        /// operation, typically meaning the IME will be closed.
        /// </summary>
        Done = 6,

        /// <summary>
        /// Bits of Android.Views.InputMethods.ImeAction.ImeMaskAction: Like Android.Views.InputMethods.ImeAction.Next,
        /// but for moving to the previous field. This will normally not be used to
        /// specify an action (since it precludes Android.Views.InputMethods.ImeAction.Next,
        /// but can be returned to the app if it sets Android.Views.InputMethods.ImeFlags.NavigatePrevious.
        /// </summary>
        Previous = 7,

        /// <summary>
        /// Set of bits in Android.Views.InputMethods.EditorInfo.ImeOptions that provide
        /// alternative actions /// associated with the "enter" key. This both helps the
        /// IME provide better feedback about what the enter key will do, and also allows
        /// it to provide alternative mechanisms for providing that command.
        /// </summary>
        ImeMaskAction = 255
    }
}
