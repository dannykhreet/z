using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Checklists
{
    public class BasicChecklistTemplateModel : NotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TaskTemplateModel> TaskTemplates { get; set; } = new List<TaskTemplateModel>();
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
