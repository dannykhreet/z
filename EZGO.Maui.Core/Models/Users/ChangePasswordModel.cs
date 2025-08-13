namespace EZGO.Maui.Core.Models.Users
{
    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordValidation { get; set; }
    }
}
