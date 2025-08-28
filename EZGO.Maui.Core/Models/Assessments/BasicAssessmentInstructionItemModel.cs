using System;
using System.Collections.Generic;
using System.IO;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Audits;
using Newtonsoft.Json;

namespace EZGO.Maui.Core.Models.Assessments
{
    public class BasicAssessmentInstructionItemModel : NotifyPropertyChanged, IItemFilter<InstructionTypeEnum>, IDetailItem
    {
        [JsonIgnore]
        public InstructionTypeEnum FilterStatus { get; set; } = InstructionTypeEnum.BasicInstruction;
        [JsonIgnore]
        public ScoreModel NewScore { get; set; }
        [JsonIgnore]
        public string DisplayPicture => HasVideo ? VideoThumbnail : Picture;



        private int? score;
        public int? Score
        {
            get { return score; }
            set { score = value == 0 ? null : value; }
        }

        public bool? IsCompleted { get => Score.HasValue && Score.Value > 0; }
        public int? AssessmentId { get; set; }
        public int? AssessmentTemplateId { get; set; }
        public int? AssessmentSkillInstructionId { get; set; }
        public int? WorkInstructionTemplateId { get; set; }
        public int? WorkInstructionTemplateItemId { get; set; }
        public int? CompletedForId { get; set; }
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int Index { get; set; }
        public string Picture { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string Video { get; set; }
        public string VideoThumbnail { get; set; }

        public List<Attachment> Attachments { get; set; }
        public bool HasAttachments => Attachments?.Count > 0;

        private AttachmentEnum? _attachmentType;
        public AttachmentEnum? AttachmentType
        {

            get
            {
                if (HasAttachments)
                    return Attachments[0].AttachmentType.ToLower() switch
                    {
                        "pdf" => AttachmentEnum.Pdf,
                        _ => AttachmentEnum.Link
                    };
                else
                    return null;

            }


            private set { _attachmentType = value; }

        }

        [JsonIgnore]
        public bool HasVideo => !string.IsNullOrEmpty(Video) && !string.IsNullOrEmpty(VideoThumbnail);

        [JsonIgnore]
        public bool IsTaskMarked => false;

        [JsonIgnore]
        public string DueDateString => "";

        [JsonIgnore]
        public string OverDueString => "";

        [JsonIgnore]
        public string TaskMarkedString => "";

        [JsonIgnore]
        public List<Tag> Tags { get; set; }

        public Stream PDFStream { get; set; } = null;

        [JsonIgnore]
        public bool IsLocalMedia { get; set; }
    }
}

