using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    public static class RoleTypeEnumExtensions
    {
        public static string ToDatabaseString(this RoleTypeEnum role)
        {
            string output = "";
            switch (role)
            {
                case RoleTypeEnum.Basic: case RoleTypeEnum.Manager: output = role.ToString().ToLower(); break;
                case RoleTypeEnum.ShiftLeader: output = "shift_leader"; break;
                case RoleTypeEnum.Staff: output = "staff"; break;
                case RoleTypeEnum.SuperUser: output = "superuser"; break;
            }
            return output.ToLower();
        }
    }
}
