using System;
using System.IO;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Models
{
    public class ThumbnailGridDetailModel : IDetailItem
    {
        public string Picture { get; set; }

        public string Video { get; set; }

        public bool HasVideo => !string.IsNullOrEmpty(Video);

        public string DueDateString => "";

        public string OverDueString => "";

        public string TaskMarkedString => "";

        public bool IsTaskMarked => false;

        public string Description { get; set; }

        public string Name { get; set; }

        public Stream PDFStream { get; set; } = null;

        public bool IsLocalMedia { get; set; }
    }
}

