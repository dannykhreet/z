using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using EZGO.Api.Models;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Core.Models.Stages
{
    public class StageTemplateModel : StageTemplate, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<BasicTaskTemplateModel> TaskTemplates { get; set; }
        public List<BasicTaskTemplateModel> FilteredTaskTemplates { get; set; }
        public bool IsSigned => Signatures?.Count > 0;
        public bool IsLocked { get; set; }
        public List<Signature> Signatures { get; set; }
        public int? StageId { get; set; }
        public bool IsCompleted => !TaskTemplates?.Any(x => x.FilterStatus == Api.Models.Enumerations.TaskStatusEnum.Todo) ?? false;
        public bool IsHeaderVisible { get; set; } = true;
        public bool AnyTaskChanges { get; set; }
        public bool AnyStageChanges { get; set; }
        public string ShiftNotes { get; set; }
        public bool HasTags => Tags?.Any() ?? false;
        public List<int> TaskIds { get; set; }
    }
}