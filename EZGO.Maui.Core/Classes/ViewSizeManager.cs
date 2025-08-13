using System;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Classes
{
    public static class ViewSizeManager
    {
        public static void ResizeView(IViewResizer viewResizer)
        {
            if (DeviceSettings.ScreenDencity < 8)
            {
                viewResizer.RecalculateViewElementsPositions();
            }
        }
    }
}
