using System.Collections.Generic;

namespace WebApp.ViewModels
{
    public class AuditingLogDetailsViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> AddedValues { get; set; }
        public List<string> RemovedValues { get; set; }
    }
}
