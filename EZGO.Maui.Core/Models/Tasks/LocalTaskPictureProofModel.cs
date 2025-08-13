using System;
using System.Collections.Generic;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class LocalTaskPictureProofModel
    {
        public long TaskId { get; set; }
        public int UserId { get; set; }
        public List<MediaItem> PictureProofMediaItems { get; set; }
    }
}
