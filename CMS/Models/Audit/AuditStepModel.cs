using System;
namespace WebApp.Models.Audit
{
    public class AuditStepModel
    {

        public int Id { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; }
        public int TaskTemplateId { get; set; }
        public bool isNew { get; set; }
        public int Index { get; set; }
        public string Video { get; set; }
        public string VideoThumbnail { get; set; }
    }
}
