using System.Threading.Tasks;
using EZGO.Maui.Core.Classes;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IMediaItemPicker
    {
        Task<MediaItem> PickMediaItem(Page page);
    }
}
