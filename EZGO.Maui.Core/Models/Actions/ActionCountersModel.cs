using System;

namespace EZGO.Maui.Core.Models.Actions;

public class ActionCountersModel
{
    public int TotalCount { get; set; }
    public int IsResolvedCount { get; set; }
    public int IsOverdueCount { get; set; }
    public int IsUnresolvedCount { get; set; }
    public int IsCreatedByMeCount { get; set; }
    public int IsAssignedToMeCount { get; set; }
    public int HasCommentsCount { get; set; }
    public int HasUnviewedCommentsCount { get; set; }
    public int IsDueTodayCount { get; set; }
    public int IsCreatedTodayCount { get; set; }
    public int IsModifiedTodayCount { get; set; }
    public int IsActionOnTheSpotCount { get; set; }
}
