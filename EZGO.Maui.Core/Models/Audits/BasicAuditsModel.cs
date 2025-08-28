using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Audits
{
    public class BasicAuditsModel : NotifyPropertyChanged
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; }
        public List<SignatureModel> Signatures { get; set; }
        public List<BasicTaskModel> Tasks { get; set; }
        public int TotalScore { get; set; }
    }
}
