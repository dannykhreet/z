using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Utils;
using System.Text.Json.Serialization;

namespace EZGO.Maui.Core.Models.Instructions
{
    public class InstructionItem : InstructionItemTemplate, IItemFilter<InstructionTypeEnum>, IDetailItem
    {
        public InstructionTypeEnum FilterStatus { get; set; } = InstructionTypeEnum.BasicInstruction;

        public bool HasVideo => !Video.IsNullOrWhiteSpace();

        public string DisplayPicture { get => !HasVideo ? Picture : VideoThumbnail; }

        public bool IsTaskMarked => false;

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

        public string DueDateString => "";

        public string OverDueString => "";

        public string TaskMarkedString => "";

        public Stream PDFStream { get; set; } = null;

        public DateTime? StartDate { get; set; }


        [JsonIgnore]
        public bool IsLocalMedia { get; set; }
    }
}
