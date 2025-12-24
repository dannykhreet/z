using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.WorkInstructions.Base
{
    public class InstructionItemBase
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; } //first item in media if media contains one item
        public string Video { get; set; }
        public string VideoThumbnail { get; set; }
        public List<string> Media { get; set; }
        public int Index { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? CreatedById { get; set; }
        public int? ModifiedById { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this instruction item template
        /// </summary>
        public List<Tag> Tags { get; set; }
        public List<Attachment> Attachments { get; set; }
    }
}
