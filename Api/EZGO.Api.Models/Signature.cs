using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Signature, object used within several objects that contain a signature. Depending on the type of object the storage in the database is handled differently.
    /// </summary>
    public class Signature
    {
        public string SignatureImage { get; set; }
        public DateTime? SignedAt { get; set; }
        public int? SignedById { get; set; }
        public string SignedBy { get; set; }
        public string SignedByPicture { get; set; }
        public SignatureTypeEnum SignatureType { get; set; } = SignatureTypeEnum.Default;
    }
}
