using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// TemplateTypeEnum; template enum, containing list of all possible template types for implementation use with differentiation of logical parts of template processing.
    /// </summary>
    public enum TemplateTypeEnum
    {
        AuditTemplate = 0,
        ChecklistTemplate = 1,
        TaskTemplate = 2,
        WorkInstructionTemplate = 3,
        AssessmentTemplate = 4
    }
}
