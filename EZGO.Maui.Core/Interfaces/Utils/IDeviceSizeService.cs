using System;
namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IDeviceSizeService
    {
        double CalculateDeviceSizeInInches();
        void SetDeviceSize();
    }
}
