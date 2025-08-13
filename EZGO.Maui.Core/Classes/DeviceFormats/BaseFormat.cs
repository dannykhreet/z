using System;
namespace EZGO.Maui.Core.Classes.DeviceFormats
{
    public class BaseFormat
    {
        public BaseFormat()
        {
            NavigationControlPercentage = 0.127f;
            ItemWidth = 700;
            FullWidth = 785;
            PropertyWidth = 85;
            AreAdditionalButtonsVisible = true;
        }

        public float NavigationControlPercentage { get; set; }
        public double ItemWidth { get; set; }
        public double FullWidth { get; set; }
        public double PropertyWidth { get; set; }
        public bool AreAdditionalButtonsVisible { get; set; }
    }
}
