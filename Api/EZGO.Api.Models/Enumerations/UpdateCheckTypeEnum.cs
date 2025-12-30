using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// UpdateCheckTypeEnum; Contains a set of items used within the update check.
    /// Every item represents a database object in the database.
    /// </summary>
    public enum UpdateCheckTypeEnum
    {
        Actions = 0,
        Audits = 1,
        AuditTemplates = 2,
        Checklists = 3,
        ChecklistTemplates = 4,
        Company = 5,
        Media = 6,
        Shifts = 7,
        Tasks = 8,
        TaskTemplates = 9,
        Users = 10,
        PropertyValues = 11,
        Comments = 12,
        Assessments = 13,
        EzFeed = 14,
        DeepLinks = 15,
        EZFeedMessages = 16,
        WorkInstructions = 17,
        AssessmentTemplates = 18,
        OpenChecklists = 19,
        WorkInstructionTemplateChangesNotifications = 20,
        All = 99
    }
}
