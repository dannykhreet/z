using System.Threading.Tasks;
using EZGO.Api.Models;

namespace EZGO.Maui.Core.Interfaces.Bookmarks
{
    public interface IBookmarkService
    {
        Task ParseQRCode(Bookmark bookmark);
    }
}
