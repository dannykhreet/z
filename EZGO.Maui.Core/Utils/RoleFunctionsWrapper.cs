using System;
namespace EZGO.Maui.Core.Utils
{
    public class RoleFunctionsWrapper : IRoleFunctionsWrapper
    {
        public bool checkRoleForAllowedOnlyFlag(Api.Models.Enumerations.RoleTypeEnum role)
        {
            return RoleFunctions.checkRoleForAllowedOnlyFlag(role);
        }
    }

    public interface IRoleFunctionsWrapper
    {
        bool checkRoleForAllowedOnlyFlag(Api.Models.Enumerations.RoleTypeEnum role);
    }
}
