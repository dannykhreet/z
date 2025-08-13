namespace EZGO.Maui.Core.Classes
{
    public class DialogResult<TResult>
    {
        public TResult Result { get; private set; }

        public bool IsCanceled => !IsSuccess;

        public bool IsSuccess { get; private set; }

        public bool HasResult => Result != null;

        public bool IsRemoved { get; set; }

        private DialogResult()
        {
        }

        public static DialogResult<TResult> Success<TResult>(TResult result)
        {
            return new DialogResult<TResult>()
            {
                Result = result,
                IsSuccess = true,
            };
        }

        public static DialogResult<TResult> Canceled()
        {
            return new DialogResult<TResult>()
            {
                IsSuccess = false,
            };
        }

        public static DialogResult<TResult> Removed()
        {
            return new DialogResult<TResult>
            {
                IsSuccess = true,
                IsRemoved = true,
            };
        }
    }
}
