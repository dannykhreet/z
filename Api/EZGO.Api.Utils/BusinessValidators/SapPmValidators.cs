using EZGO.Api.Models;
using EZGO.Api.Models.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// ChecklistValidators; contains all validation methods for validating checklists and values of the checklists.
    /// </summary>
    public static class SapPmValidators
    {
        public const string MESSAGE_LOCATION_ID_IS_NOT_VALID = "Sap Pm Location id is not valid";
        public const string MESSAGE_NOTIFICATION_ID_IS_NOT_VALID = "Sap Pm Notification id is not valid";

        public static bool LocationIdIsValid(int locationId)
        {
            if (locationId > 0)
            {
                return true;
            }

            return false;
        }
        public static bool NotificationIdIsValid(int notificationId)
        {
            if (notificationId > 0)
            {
                return true;
            }

            return false;
        }
    }
}
