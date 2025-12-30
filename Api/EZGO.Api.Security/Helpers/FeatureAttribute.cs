using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Security.Helpers
{
    public class FeatureAttribute : Attribute, IFilterMetadata
    {
        public enum FeatureFiltersEnum
        {
            Actions,
            ActionComments,
            ActionOnTheSpot,
            Audits,
            AuditProperties,
            AuditItemProperties,
            Checklists,
            ChecklistProperties,
            ChecklistItemProperties,
            ChecklistTransferable,
            Comments,
            ExportsBasicData,
            ExportsBasicTemplates,
            Exports,
            FactoryFeed,
            Ignore,
            MarketPlace,   
            Reports,
            ReportsBasic,
            ReportsAdvanced,
            RequiredProof,
            Roles,
            RunningAssessmentsInCMS,
            Skills,
            SkillAssessments,
            SkillMatrix,
            TaskGenerationManager,
            Tags,
            Tasks,
            TaskProperties,
            TemplateSharing,
            UserProfiles,
            UserExtendedDetails,
            WorkInstructions,
            WorkInstructionItemAttachmentPdf,
            WorkInstructionItemAttachmentLink
        }

        public FeatureFiltersEnum Feature { get; set; }
    }
}
