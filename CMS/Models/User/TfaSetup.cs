namespace WebApp.Models.User
{
    public class TfaSetup
    {
        public string ManualEntryKey { get; set; }
        public string QrCodeSetupImageUrl { get; set; }
        public bool Enabled { get; set; }

    }
}
