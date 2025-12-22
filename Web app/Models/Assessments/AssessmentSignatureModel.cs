using System;
namespace WebApp.Models.Assessments
{
    public class AssessmentSignatureModel
    {
        public string SignatureImage { get; set; }
        public DateTime SignedAt { get; set; }
        public int SignedById { get; set; }
        public string SignedBy { get; set; }
    }
}
