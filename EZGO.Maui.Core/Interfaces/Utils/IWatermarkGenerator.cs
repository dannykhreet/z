using System;
using System.Threading.Tasks;
using NodaTime;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IWatermarkGenerator
    {
        Task<string> GeneratePictureProofWatermark(string path, string itemName, LocalDateTime? localDateTime = null);
    }
}
