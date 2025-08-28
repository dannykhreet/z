using System;

namespace EZGO.Maui.Core.Classes.Stages;

public interface IStagesControl
{
    bool HasStages { get; }

    void SetStages(object tasks = null, bool addStageSign = true);
    void SetTasksWithoutStage();
}
