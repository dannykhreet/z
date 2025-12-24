using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    public class PictureProof
    {
        public int Id { get; set; }
        public List<PictureProofMedia> Media { get; set; }
        public string Description { get; set; }
        public DateTime ProofTakenUtc { get; set; }
        public int ProofTakenByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
