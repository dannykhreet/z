using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// ShiftValidators; contains all validation methods for validating shifts and values of the shifts. Shifts are part of the company structure.
    /// </summary>
    public static class ShiftValidators
    {
        public const string MESSAGE_SHIFT_ID_IS_NOT_VALID = "ShiftId is not valid";
        public static bool ShiftIdIsValid(int shiftid)
        {
            if (shiftid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.Shift> shifts, int companyId)
        {
            return !(shifts.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.Shift shift, int companyId)
        {
            return (shift.CompanyId == companyId);
        }

        //Move to own item
        public static Shift ValidateAndSetDefaults(Shift shift)
        {
            shift.Day = shift.Weekday == 6 ? 1 : shift.Weekday + 2;

            return shift;
        }
    }
}
