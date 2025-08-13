using System;
using EZGO.Api.Models;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Classes.Stages;

public class StagesControl : NotifyPropertyChanged, IStagesControl
{
    public List<StageTemplateModel> StageTemplates { get; set; }
    public List<BasicTaskTemplateModel> TaskTemplates { get; set; }
    public StageTemplateModel CurrentStageTemplate { get; private set; }

    public bool HasStages => StageTemplates?.Any(x => x.Id != -1) ?? false;

    public bool AreStagesSigned => !HasStages || HasStages && !StageTemplates.Where(x => x.Id != -1).Any(x => !x.IsSigned);

    public StagesControl(List<StageTemplateModel> stages, List<BasicTaskTemplateModel> taskTemplates, List<BasicTaskTemplateModel> filteredTaskTemplates)
    {
        StageTemplates = stages?.OrderBy(x => x.Index).ToList();
        TaskTemplates = taskTemplates;
    }

    public List<BasicTaskTemplateModel> GetAllFilteredTaskTemplates()
    {
        if (StageTemplates == null)
            return new List<BasicTaskTemplateModel>();

        return StageTemplates
            .Where(stage => stage.FilteredTaskTemplates != null)
            .SelectMany(stage => stage.FilteredTaskTemplates)
            .ToList();
    }

    public void SetStages(object filteredTasks, bool addStageSign = true)
    {
        if (TaskTemplates == null)
            return;

        StageTemplates ??= new List<StageTemplateModel>();
        for (int i = 0; i < StageTemplates.Count; i++)
        {
            var item = StageTemplates[i];
            if (item == null)
                continue;

            item.TaskTemplates = TaskTemplates?.Where(x => item.TaskTemplateIds != null && item.TaskTemplateIds.Contains(x.Id)).ToList();
            item.TaskTemplates.ForEach(x => x.StageTemplateId = item.Id);

            //Add sign item
            if (addStageSign)
            {
                var signItem = new BasicTaskTemplateModel() { Id = -1, StageTemplateId = item.Id };
                item.TaskTemplates.Add(signItem);
            }
        }

        SetTasksWithoutStage();
        SetFilteredItems(filteredTasks);
        LockStages();
    }

    private void LockStages()
    {
        bool lockNextStages = false;

        foreach (var stage in StageTemplates)
        {
            if (stage == null)
                continue;

            if (stage.Id != -1)
            {
                stage.IsLocked = lockNextStages || (stage.LockStageAfterCompletion && stage.IsSigned);
            }
            else
            {
                // Artificial stages (Id == -1) should never be locked
                stage.IsLocked = false;
            }

            stage.FilteredTaskTemplates?.ForEach(x => x.IsStageLocked = stage.IsLocked);

            if (stage.BlockNextStagesUntilCompletion && !stage.IsSigned)
                lockNextStages = true;
        }
    }

    public void SetTasksWithoutStage()
    {
        var tasksWithoutStage = TaskTemplates.Where(x => x.StageTemplateId == null).ToList();
        if (tasksWithoutStage?.Count == 0)
            return;

        tasksWithoutStage.ForEach(x => x.StageTemplateId = -1);

        //Create artificial stage
        var artificialStage = new StageTemplateModel()
        {
            Id = -1,
            BlockNextStagesUntilCompletion = false,
            IsLocked = false,
            LockStageAfterCompletion = false,
            TaskTemplates = tasksWithoutStage,
            IsHeaderVisible = false,
            Index = int.MaxValue,
        };

        StageTemplates.Add(artificialStage);
    }

    public bool IsTaskLocked(BasicTaskTemplateModel task)
    {
        if (task == null || task.StageTemplateId == null)
            return false;

        var stage = StageTemplates?.FirstOrDefault(x => x.Id == task.StageTemplateId);
        if (stage == null)
            return false;

        return stage.IsLocked;
    }

    public void SendClosableWarning(BasicTaskTemplateModel task)
    {
        if (task == null)
            return;

        string text;
        var stage = GetStageTemplate(task.StageTemplateId);
        if (stage.IsSigned)
            text = TranslateExtension.GetValueFromDictionary(LanguageConstants.stageIsLocked);
        else
            text = TranslateExtension.GetValueFromDictionary(LanguageConstants.signStageToProceed);

        using var scope = App.Container.CreateScope();
        var messagingCenter = scope.ServiceProvider.GetService<IMessageService>();
        messagingCenter.SendClosableWarning(text);
    }

    public bool IsTaskSigned(BasicTaskTemplateModel task)
    {
        if (task == null || task.StageTemplateId == null)
            return false;

        var stage = StageTemplates?.FirstOrDefault(x => x.Id == task.StageTemplateId);
        if (stage == null)
            return false;

        return stage.IsSigned;
    }

    public bool SaveStage(int? stageTemplateId, List<Signature> signatures = null)
    {
        if (stageTemplateId == null)
            return false;

        var stage = StageTemplates.FirstOrDefault(x => x.Id == stageTemplateId);
        if (stage == null || (stage.IsSigned && stage.LockStageAfterCompletion && stage.AnyTaskChanges) || !stage.IsCompleted || stage.IsLocked)
            return false;

        signatures ??= new List<Signature> { new Signature { SignedAt = DateTime.UtcNow, SignedById = UserSettings.Id, SignedBy = UserSettings.Fullname } };
        stage.Signatures = signatures;
        stage.AnyTaskChanges = false;
        stage.AnyStageChanges = true;
        CurrentStageTemplate = stage;

        LockStages();
        //Send message for signed tile to respond
        SendTaskChangedMessage();

        return true;
    }

    public bool CanSaveStage(int? stageTemplateId)
    {
        if (stageTemplateId == null)
            return false;

        var stage = StageTemplates.FirstOrDefault(x => x.Id == stageTemplateId);
        if (stage == null || stage.IsSigned || !stage.IsCompleted || stage.IsLocked)
            return false;

        return true;
    }

    public void SendTaskChangedMessage()
    {
        MessagingCenter.Send(this, Constants.TasksChanged);
    }

    public StageTemplateModel GetStageTemplate(int? id)
    {
        if (id == null)
            return null;

        var stage = StageTemplates.FirstOrDefault(x => x.Id == id);

        return stage;
    }

    public void SetCurrentStage(int? id)
    {
        if (id == null)
            return;
        CurrentStageTemplate = GetStageTemplate(id);
    }

    public void UpdateStages(List<StageModel> stages)
    {
        if (stages == null || StageTemplates == null)
            return;

        foreach (var item in stages)
        {
            var stageTemplate = StageTemplates.FirstOrDefault(x => x.Id == item.StageTemplateId);
            if (stageTemplate == null)
                continue;

            stageTemplate.StageId = item.Id;
            stageTemplate.Signatures = item.Signatures;
            stageTemplate.ShiftNotes = item.ShiftNotes;
        }
        LockStages();
        //Send message for signed tile to respond
        MessagingCenter.Send(this, Constants.TasksChanged);
    }

    public void SetFilteredItems(object filteredTasks)
    {
        List<BasicTaskTemplateModel> tasks = filteredTasks as List<BasicTaskTemplateModel>;

        if (tasks == null)
            return;

        if (StageTemplates == null)
            return;

        // Group all tasks once to avoid repeated searches per stage
        var tasksByStage = tasks.GroupBy(x => x.StageTemplateId)
                                .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var item in StageTemplates)
        {
            tasksByStage.TryGetValue(item.Id, out var filteredItems);
            filteredItems ??= new List<BasicTaskTemplateModel>();

            //Add sign item to "real" stages
            if (item.Id != -1)
            {
                filteredItems.Add(new BasicTaskTemplateModel { Id = -1, StageTemplateId = item.Id });
            }

            item.FilteredTaskTemplates = filteredItems;
        }
    }

    public void TaskChanged(BasicTaskTemplateModel basicTaskTemplateModel)
    {
        if (basicTaskTemplateModel == null)
            return;

        var stage = StageTemplates.FirstOrDefault(x => basicTaskTemplateModel?.StageTemplateId == x.Id);
        if (stage == null)
            return;

        stage.AnyTaskChanges = true;
        stage.Signatures = new List<Signature>();
        LockStages();
        MessagingCenter.Send(this, Constants.TasksChanged);
    }

    public void SignatureChanged(BasicTaskTemplateModel basicTaskTemplateModel)
    {
        if (basicTaskTemplateModel == null)
            return;

        var stage = StageTemplates.FirstOrDefault(x => basicTaskTemplateModel?.StageTemplateId == x.Id);
        if (stage == null)
            return;

        if (stage.Signatures?.Count == 0)
            return;

        if (stage.Signatures?.Count > 0)
        {
            stage.AnyStageChanges = true;
        }
        stage.Signatures = null;
    }

    public List<StageTemplateModel> GetChangedStage()
    {
        if (StageTemplates == null)
            return new List<StageTemplateModel>();

        return StageTemplates.Where(x => x.AnyStageChanges).ToList();
    }

    public void ClearChangedStages()
    {
        if (StageTemplates == null)
            return;

        foreach (var item in StageTemplates)
        {
            item.AnyStageChanges = false;
        }
    }

    internal void SetCurrentStageHeaderVisibility(bool shouldHeaderBeVisible)
    {
        if (CurrentStageTemplate == null || CurrentStageTemplate.Id < 0)
            return;

        CurrentStageTemplate.IsHeaderVisible = shouldHeaderBeVisible;
    }
}
