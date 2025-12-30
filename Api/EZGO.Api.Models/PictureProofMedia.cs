using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    public class PictureProofMedia
    {
        public string UserFullName { get; set; }
        public int UserId { get; set; }
        public string ItemName { get; set; }
        public DateTime PictureTakenUtc { get; set; }
        public string UriPart { get; set; }
        public string ThumbUriPart { get; set; }
    }
}
