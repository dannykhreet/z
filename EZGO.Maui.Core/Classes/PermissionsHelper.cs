using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes
{
    /// <summary>
    /// Permissions helper.
    /// </summary>
    public class PermissionsHelper
    {
        /// <summary>
        /// Checks the and request permission asynchronous.
        /// </summary>
        /// <typeparam name="TPermission">The type of the permission.</typeparam>
        /// <returns>True if permission is granted; false otherwise.</returns>
        public static async Task<bool> CheckAndRequestPermissionAsync<TPermission>() where TPermission : Permissions.BasePermission, new()
        {
            bool result = false;

            PermissionStatus permissionStatus = await Permissions.CheckStatusAsync<TPermission>();

            if (permissionStatus != PermissionStatus.Granted)
                permissionStatus = await Permissions.RequestAsync<TPermission>();

            if (permissionStatus == PermissionStatus.Granted)
                result = true;

            return result;
        }
    }
}
