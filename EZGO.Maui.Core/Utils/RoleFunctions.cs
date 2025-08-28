using EZGO.Api.Models.Enumerations;

namespace EZGO.Maui.Core.Utils
{
    public class RoleFunctions
    {
        public static bool checkRoleForAllowedOnlyFlag(RoleTypeEnum role)
        {
            bool allowedOnly = true;
            switch (role)
            {
                case RoleTypeEnum.Manager:
                case RoleTypeEnum.Staff:
                case RoleTypeEnum.SuperUser:
                    allowedOnly = false;
                    break;
                default:
                    allowedOnly = true;
                    break;
            }
            return allowedOnly;
        }
    }
}
