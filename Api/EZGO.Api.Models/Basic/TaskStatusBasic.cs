using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Basic
{
    /// <summary>
    /// TaskStatusBasic; Basic object used within other classes.
    /// Basic objects are used for simple datasets and only contain a few Ids and a visual reference (e.g. name or description).
    /// NOTE! this is not meant to be a base class within the API, so don't use it as such within the API.
    /// </summary>
    public class TaskStatusBasic
    {
        public int TaskId { get; set; }
        public string Status { get; set; }
        public int? SignedById { get; set; }
        public string SignedBy { get; set; }
        public DateTime? SignedAt { get; set; }
        public int TaskTemplateId { get; set; }
        public bool HasPictureProof { get; set; }
        public PictureProof PictureProof { get; set; }
        public string Comment { get; set; }
    }
}
