using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// MediaStorageTypeEnum; Media storage type. Is used for routing for specific images/media.
    /// </summary>
    public enum MediaStorageTypeEnum
    {
        ActionComments = 16, // = "actioncomments";
        Actions = 15,// = "actions";
        Announcement = 26,
        Area = 17, // = "areas";
        AssessmentSignature = 25,
        AssessmentTemplate = 24,
        AuditDescriptions = 12, // = "task_descriptions";
        AuditItems = 10, // = "tasks";
        Audits = 9, // = "lists";
        AuditSignatures = 13, // = "signatures"
        AuditSteps = 11, // = "steps";
        ChecklistDescriptions = 7, // = "task_descriptions";
        ChecklistItems = 5, // = "tasks";
        Checklists = 4, // = "lists";
        ChecklistSignatures = 8, // = "signatures"
        ChecklistSteps = 6, // = "steps";
        Comments = 18, // = "comments";
        Company = 21, // = "company"
        FactoryFeed = 19,
        FactoryFeedMessages = 20,
        PictureProof = 27,
        ProfileImage = 14, // = "profile";
        TaskDescriptions = 3, // = "task_descriptions";
        Tasks = 1, //= "tasks"
        TaskSteps = 2, //= "steps";
        WorkInstruction = 22,
        WorkInstructionItem = 23
    }
}
