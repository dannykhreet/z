using EZGO.Api.Models.TaskGeneration;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    public static class PlanningGenerationConverter
    {
        public static GenerationConfiguration ToGenerationConfiguration(this PlanningConfiguration planning)
        {
            GenerationConfiguration output = new GenerationConfiguration();

            List<GenerationConfigurationItem> generationItems = new List<GenerationConfigurationItem>();

            foreach (PlanningConfigurationItem planningItem in planning.ConfigurationItems)
            {
                //planning is in the past more than 7 days, don't create generation configuration
                if (planningItem.DisabledTo.HasValue && 
                    planningItem.DisabledTo.Value < DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)))
                {
                    continue;
                }

                if (planningItem.AreaIds.Count > 0 && planningItem.ShiftIds.Count == 0 && planningItem.TaskIds.Count == 0)
                {
                    //only areas are disabled
                    foreach (int areaId in planningItem.AreaIds)
                    {
                        GenerationConfigurationItem item = new GenerationConfigurationItem { AreaId = areaId, PlanningType = "area_only" };
                        if (planningItem.DisabledFrom != null && planningItem.DisabledTo != null)
                        {
                            //a perdiod was given
                            item.StartAt = planningItem.DisabledFrom;
                            item.EndAt = planningItem.DisabledTo;
                            item.PlanningType = "area_period";
                        }
                        generationItems.Add(item);
                    }
                }
                else if (planningItem.AreaIds.Count == 0 && planningItem.ShiftIds.Count > 0 && planningItem.TaskIds.Count == 0)
                {
                    //only shifts are disabled
                    foreach (int shiftId in planningItem.ShiftIds)
                    {
                        GenerationConfigurationItem item = new GenerationConfigurationItem { ShiftId = shiftId, PlanningType = "shift_only" };
                        if (planningItem.DisabledFrom != null && planningItem.DisabledTo != null)
                        {
                            //a perdiod was given
                            item.StartAt = planningItem.DisabledFrom;
                            item.EndAt = planningItem.DisabledTo;
                            item.PlanningType = "shift_period";
                        }
                        generationItems.Add(item);
                    }
                }
                else if (planningItem.AreaIds.Count == 0 && planningItem.ShiftIds.Count == 0 && planningItem.TaskIds.Count > 0)
                {
                    //only taskstemplates are disabled
                    foreach (int taskId in planningItem.TaskIds)
                    {
                        GenerationConfigurationItem item = new GenerationConfigurationItem { TemplateId = taskId, PlanningType = "template_only" };
                        if (planningItem.DisabledFrom != null && planningItem.DisabledTo != null)
                        {
                            //a perdiod was given
                            item.StartAt = planningItem.DisabledFrom;
                            item.EndAt = planningItem.DisabledTo;
                            item.PlanningType = "template_period";
                        }
                        generationItems.Add(item);
                    }
                }
                else if (planningItem.AreaIds.Count > 0 && planningItem.ShiftIds.Count > 0 && planningItem.TaskIds.Count == 0)
                {
                    //areas and shifts are disabled
                    foreach (int areaId in planningItem.AreaIds)
                    {
                        foreach (int shiftId in planningItem.ShiftIds)
                        {
                            GenerationConfigurationItem item = new GenerationConfigurationItem { AreaId = areaId, ShiftId = shiftId, PlanningType = "area_shift" };
                            if (planningItem.DisabledFrom != null && planningItem.DisabledTo != null)
                            {
                                //a perdiod was given
                                item.StartAt = planningItem.DisabledFrom;
                                item.EndAt = planningItem.DisabledTo;
                                item.PlanningType = "area_shift_period";
                            }
                            generationItems.Add(item);
                        }
                    }

                }
                else if (planningItem.AreaIds.Count > 0 && planningItem.ShiftIds.Count == 0 && planningItem.TaskIds.Count > 0)
                {
                    //areas and shifts are disabled
                    foreach (int areaId in planningItem.AreaIds)
                    {
                        foreach (int taskId in planningItem.TaskIds)
                        {
                            GenerationConfigurationItem item = new GenerationConfigurationItem { AreaId = areaId, TemplateId = taskId, PlanningType = "area_template" };
                            if (planningItem.DisabledFrom != null && planningItem.DisabledTo != null)
                            {
                                //a perdiod was given
                                item.StartAt = planningItem.DisabledFrom;
                                item.EndAt = planningItem.DisabledTo;
                                item.PlanningType = "area_template_period";
                            }
                            generationItems.Add(item);
                        }
                    }

                }
                else if (planningItem.AreaIds.Count == 0 && planningItem.ShiftIds.Count > 0 && planningItem.TaskIds.Count > 0)
                {
                    //shifts and templates are disabled
                    foreach (int taskId in planningItem.TaskIds)
                    {
                        foreach (int shiftId in planningItem.ShiftIds)
                        {
                            GenerationConfigurationItem item = new GenerationConfigurationItem { TemplateId = taskId, ShiftId = shiftId, PlanningType = "shift_template" };
                            if (planningItem.DisabledFrom != null && planningItem.DisabledTo != null)
                            {
                                //a perdiod was given
                                item.StartAt = planningItem.DisabledFrom;
                                item.EndAt = planningItem.DisabledTo;
                                item.PlanningType = "shift_template_period";
                            }
                            generationItems.Add(item);
                        }
                    }
                }
                else if (planningItem.AreaIds.Count > 0 && planningItem.ShiftIds.Count > 0 && planningItem.TaskIds.Count > 0)
                {
                    //areas and shifts are disabled
                    foreach (int areaId in planningItem.AreaIds)
                    {
                        foreach (int shiftId in planningItem.ShiftIds)
                        {
                            GenerationConfigurationItem item = new GenerationConfigurationItem { AreaId = areaId, ShiftId = shiftId, PlanningType = "area_shift" };
                            if (planningItem.DisabledFrom != null && planningItem.DisabledTo != null)
                            {
                                //a perdiod was given
                                item.StartAt = planningItem.DisabledFrom;
                                item.EndAt = planningItem.DisabledTo;
                                item.PlanningType = "area_shift_period";
                            }
                            generationItems.Add(item);
                        }
                    }

                    //shifts and templates are disabled
                    foreach (int taskId in planningItem.TaskIds)
                    {
                        foreach (int shiftId in planningItem.ShiftIds)
                        {
                            GenerationConfigurationItem item = new GenerationConfigurationItem { TemplateId = taskId, ShiftId = shiftId, PlanningType = "shift_template" };
                            if (planningItem.DisabledFrom != null && planningItem.DisabledTo != null)
                            {
                                //a perdiod was given
                                item.StartAt = planningItem.DisabledFrom;
                                item.EndAt = planningItem.DisabledTo;
                                item.PlanningType = "shift_template_period";
                            }
                            generationItems.Add(item);
                        }
                    }
                }
                else if (planningItem.AreaIds.Count == 0 && planningItem.ShiftIds.Count == 0 && planningItem.TaskIds.Count == 0)
                {
                    //nothing was selected to be disabled
                    if (planningItem.DisabledFrom != null && planningItem.DisabledTo != null)
                    {
                        GenerationConfigurationItem item = new GenerationConfigurationItem { StartAt = planningItem.DisabledFrom, EndAt = planningItem.DisabledTo, PlanningType = "period_only" };
                        generationItems.Add(item);
                    }
                    
                }
            }
            output.ConfigurationItems = generationItems;

            return output;
        }
    }
}
