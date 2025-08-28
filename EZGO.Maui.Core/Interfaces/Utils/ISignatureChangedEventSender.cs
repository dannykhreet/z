namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface ISignatureChangedEventSender
    {
        public StreamImageSource FirstSignature { get; set; }
        public StreamImageSource SecondSignature { get; set; }
        async Task Send(string message) { }
    }
}
