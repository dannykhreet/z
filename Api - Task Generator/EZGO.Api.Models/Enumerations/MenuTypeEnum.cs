using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// MenuTypeEnum; The basic menu structure that is being used within the EZGO application.
    /// In the current application the menu parts are always displayed, but for the future this can change.
    /// If menu parts are added within the application please extend the MenuTypeEnum.
    /// </summary>
    public enum MenuTypeEnum
    {
        Actions = 0,
        Audits = 1,
        Checklists = 2,
        Reports = 3,
        Tasks = 4
    }
}
