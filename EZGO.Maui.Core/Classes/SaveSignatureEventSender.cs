using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Classes
{
    public class SaveSignatureEventSender : ISignatureChangedEventSender
    {
        public SaveSignatureEventSender(StreamImageSource firstSignature, StreamImageSource secnodSignature = null)
        {
            FirstSignature = firstSignature;
            SecondSignature = secnodSignature;
        }

        public StreamImageSource FirstSignature { get; set; }
        public StreamImageSource SecondSignature { get; set; }

        public async Task Send(string message)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagingCenter.Send(this, message);
            });
        }
    }
}
