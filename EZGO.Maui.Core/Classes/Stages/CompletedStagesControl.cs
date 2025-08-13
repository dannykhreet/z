using System;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Core.Classes.Stages;

public class CompletedStagesControl : NotifyPropertyChanged, IStagesControl
{
    public List<StageModel> Stages { get; set; }
    public List<TasksTaskModel> Tasks { get; set; }

    public bool HasStages => Stages?.Any(x => x.Id != -1) ?? false;

    public CompletedStagesControl(List<StageModel> stages, List<TasksTaskModel> tasks)
    {
        Stages = stages?.OrderBy(x => x.Index).ToList();
        Tasks = tasks;
    }

    public void SetStages(object task = null, bool addStageSign = true)
    {
        if (Tasks == null)
            return;

        Stages ??= new List<StageModel>();
        for (int i = 0; i < Stages.Count; i++)
        {
            var item = Stages[i];
            if (item == null)
                continue;

            item.Tasks = Tasks?.Where(x => item.TaskIds != null && item.TaskIds.Contains((int)x.Id)).ToList();
            item.Tasks.ForEach(x => x.StageId = item.Id);

            //Add sign item
            if (addStageSign)
            {
                var signItem = new TasksTaskModel() { Id = -1, StageId = item.Id };
                item.Tasks.Add(signItem);
            }
        }

        SetTasksWithoutStage();
    }

    public void SetTasksWithoutStage()
    {
        var tasksWithoutStage = Tasks.Where(x => x.StageId == null).ToList();
        if (tasksWithoutStage?.Count == 0)
            return;

        tasksWithoutStage.ForEach(x => x.StageId = -1);

        //Create artificial stage
        var artificialStage = new StageModel()
        {
            Id = -1,
            Tasks = tasksWithoutStage,
            IsHeaderVisible = false,
            Index = int.MaxValue
        };

        Stages.Add(artificialStage);
    }

    public StageModel GetStage(int? id)
    {
        if (id == null)
            return null;

        var stage = Stages.FirstOrDefault(x => x.Id == id);

        return stage;
    }
}
