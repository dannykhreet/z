using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// IncludesEnum; includes are used for getting extra data for a specific object. These are supplied by the application through the API.
    /// Depending on the call and which information is available this can be one or more items.
    /// Usually they are supplied as a comma separated string, which is split and validated against this Enumerator (values are lowercased then).
    ///
    /// Current implementations:
    /// Action -> Comments, AssignedUsers, AssignedAreas
    /// Audit -> Tasks
    /// AuditTemplates -> TaskTemplates, Steps
    /// Checklist -> Tasks
    /// ChecklistTemplate -> TaskTemplates, Steps
    /// Company -> Shifts
    /// AssignedUsers -> Actions
    /// AssignedAreas -> Actions
    /// ViewedByUsers -> ActionComments
    /// MainParent -> Actions
    /// UnviewedCommentNr -> Actions
    ///
    /// TO ADD
    ///
    /// </summary>
    public enum IncludesEnum
    {
        Actions,
        AreaPathIds,
        AreaPaths,
        Areas,
        AssignedAreas,
        AssignedUsers,
        Comments,
        Company,
        CompanyRoot,
        CompanySettings,
        DisplayAreas,
        FeedItems,
        Holding,
        HoldingRelations,
        HoldingUnits,
        HoldingSettings,
        Instructions,
        InstructionItems,
        InstructionRelations,
        ItemTemplates,
        Items,
        MainParent,
        MutationInformation,
        ObjectProperties,
        ObjectPropertyUserValues,
        OpenFields,
        OpenFieldsPropertyDetails,
        PictureProof,
        Properties,
        PropertyDetails,
        PropertyUserValues,
        PropertyValues,
        PropertiesGen4,
        Parents,
        Recurrency,
        RecurrencyShifts,
        Roles,
        SapPmFunctionalLocations,
        Shifts,
        Steps,
        Tags,
        Tasks,
        TaskTemplates,
        Usage,
        UnviewedCommentNr,
        Users,
        UserInformation,
        ViewedByUsers

    }

}
