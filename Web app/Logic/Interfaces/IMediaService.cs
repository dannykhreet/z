using System.Threading.Tasks;

namespace WebApp.Logic.Interfaces
{
    public interface IMediaService
    {
        Task<string> GetMediaAsBase64(string url);
        Task<string> GetPreSignedURL(string url);
    }
}
