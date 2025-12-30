using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Shared
{
    public class SharedSignatureModel
    {
        public string SignatureImage { get; set; }
        public DateTime SignedAt { get; set; }
        public int SignedById { get; set; }
        public string SignedBy { get; set; }
    }
}
