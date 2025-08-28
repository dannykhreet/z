using System.IO;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IDetailItem
    {
        public bool HasVideo { get; }
        public bool IsTaskMarked { get; }
        public string Picture { get; set; }
        public string Name { get; set; }
        public string Video { get; set; }
        public string Description { get; set; }
        public string DueDateString { get; }
        public string OverDueString { get; }
        public string TaskMarkedString { get; }
        public Stream PDFStream { get; set; }
        public bool IsPdf => Picture?.ToLower().EndsWith(".pdf") ?? false;
        public bool IsLocalMedia { get; set; }
    }
}
