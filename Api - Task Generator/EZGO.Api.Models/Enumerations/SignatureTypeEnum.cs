using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// SignatureTypeEnum; Describes a type of a filled in signature
    /// </summary>
    public enum SignatureTypeEnum
    {
        /// <summary>
        /// Default
        /// </summary>
        [Description("Default")]
        Default = 0,
        /// <summary>
        /// Assessor
        /// </summary>
        [Description("Assessor")]
        Assessor = 1,
        /// <summary>
        /// Assessee
        /// </summary>
        [Description("Assessee")]
        Assessee = 2
    }
}
