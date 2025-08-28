using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Enumerations
{
    public enum MessageTypeEnum
    {
        /// <summary>
        /// no special indication
        /// </summary>
        General=0,
        /// <summary>
        /// Connection problem
        /// </summary>
        Connection = 1,
        /// <summary>
        /// Clear message
        /// </summary>
        Clear = 10
    }
}
