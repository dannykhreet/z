using System;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IMediaResizer
    {
        byte[] ResizeImage(byte[] arr, int width, int height);
    }
}
