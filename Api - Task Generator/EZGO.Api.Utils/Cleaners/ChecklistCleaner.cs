using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using EZGO.Api.Models;
using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.Cleaners
{
    public static class ChecklistCleaner
    {
        public static List<Checklist> CleanChecklistsForRetrieval(List<EZGO.Api.Models.Checklist> checklists)
        {
            List<Checklist> output = new List<Checklist>();
            foreach (var checklist in checklists)
            {
                output.Add(CleanChecklistForRetrieval(checklist));
            }
            return output;
        }
        public static Checklist CleanChecklistForRetrieval(EZGO.Api.Models.Checklist checklist)
        {
            if(checklist != null)
            {
                var checklistUserIds = new List<int>();
                var technicakUserIdsFound = false;
                var continueCleaning = true;
                try
#pragma warning disable CS0168 // Variable is declared but never used
                {
                    if (checklist.Tasks != null && checklist.Tasks.Any())
                    {
                        foreach (var task in checklist.Tasks)
                        {
                            if (task.PictureProof != null)
                            {
                                checklistUserIds.Add(task.PictureProof.ProofTakenByUserId);
                            }
                            if (task.PropertyUserValues != null && task.PropertyUserValues.Any())
                            {
                                checklistUserIds.AddRange(task.PropertyUserValues.Select(x => x.UserId));
                            }
                            if (task.Signature != null && task.Signature.SignedById.HasValue)
                            {
                                checklistUserIds.Add(task.Signature.SignedById.Value);
                            }
                        }
                    }
                    if (checklist.OpenFieldsPropertyUserValues != null && checklist.OpenFieldsPropertyUserValues != null)
                    {
                        checklistUserIds.AddRange(checklist.OpenFieldsPropertyUserValues.Select(x => x.UserId));
                    }
                    if (checklist.Signatures != null && checklist.Signatures.Any())
                    {
                        checklistUserIds.AddRange(checklist.Signatures.Where(y => y.SignedById.HasValue).Select(x => x.SignedById.Value));
                    }
                    if(checklist.Stages != null && checklist.Stages.Any())
                    {
                        foreach (var stage in checklist.Stages) {
                            if(stage.Signatures != null)
                            {
                                checklistUserIds.AddRange(stage.Signatures.Where(y => y.SignedById.HasValue).Select(x => x.SignedById.Value));
                            }
                           
                        }
                    }
                } catch(Exception ex)
                {
                    //do nothing, if for some reason data is incomplete.
                    Debug.WriteLine(ex);
                    continueCleaning = false;
                }
#pragma warning restore CS0168 // Variable is declared but never used

                if (checklist.CreatedById.HasValue && checklist.CreatedById > 0)
                {
                    if (checklistUserIds.Contains(checklist.CreatedById.Value)) technicakUserIdsFound = true;
                }
                if(checklist.ModifiedById.HasValue && checklist.ModifiedById > 0)
                {
                    if (checklistUserIds.Contains(checklist.ModifiedById.Value)) technicakUserIdsFound = true;
                }
                //technical users not found within normal data of checklist, so checklist probably posted by user who did not create or modify data due to offline mode. 
                if(continueCleaning && !technicakUserIdsFound && checklistUserIds.Any() && checklist.Signatures != null && checklist.Signatures.Any())
                {
                    try
#pragma warning disable CS0168 // Variable is declared but never used
                    {
                        //overrule technical creation data. 
                        var firstSignature = checklist.Signatures[0];
                        if (checklist.CreatedById.HasValue && checklist.CreatedById > 0) checklist.CreatedById = checklist.Signatures[0].SignedById;
                        if (!string.IsNullOrEmpty(checklist.CreatedBy)) checklist.CreatedBy = checklist.Signatures[0].SignedBy;
                        checklist.CreatedByUser = new Models.Basic.UserBasic() { Id = checklist.Signatures[0].SignedById.Value, Name = checklist.Signatures[0].SignedBy, Picture = checklist.Signatures[0].SignedByPicture };
                        if (checklist.ModifiedById.HasValue && checklist.ModifiedById > 0) checklist.ModifiedById = checklist.Signatures[0].SignedById;
                        if (!string.IsNullOrEmpty(checklist.ModifiedBy)) checklist.CreatedBy = checklist.Signatures[0].SignedBy;
                        checklist.ModifiedByUser = new Models.Basic.UserBasic() { Id = checklist.Signatures[0].SignedById.Value, Name = checklist.Signatures[0].SignedBy, Picture = checklist.Signatures[0].SignedByPicture };
                    } catch(Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
#pragma warning restore CS0168 // Variable is declared but never used

                }
            }
            return checklist;
        }
    }
}
