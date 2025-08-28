using System;
namespace EZGO.Maui.Core.Classes.DeviceFormats
{
    public class EightInchFormat : BaseFormat
    {
        public EightInchFormat()
        {
            NavigationControlPercentage = 0.10f;
            ItemWidth = 600;
            FullWidth = 685;
            PropertyWidth = FullWidth - ItemWidth;
            AreAdditionalButtonsVisible = false;
        }
    }
}
