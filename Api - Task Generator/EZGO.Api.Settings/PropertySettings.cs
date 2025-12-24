using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Settings
{
    public class PropertySettings
    {
        /// <summary>
        /// BasicAndSpecificProperties; References group ids that are used for the basic property structure as used with Tasks, Checklist items and audit items.
        /// These contain specific properties (e.g. temperature) and basic properties (e.g. a string , a integer etc).
        /// </summary>
        public static readonly int[] BasicAndSpecificProperties = { 1, 6 };

        /// <summary>
        /// OpenFieldProperties; Specific group for open field properties; Contains properties that are part of checklists or audits that can be filled in as extra information for a property.
        /// </summary>
        public static readonly int[] OpenFieldProperties = { 7 };
    }
}
