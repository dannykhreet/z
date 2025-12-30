using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    public class Bookmark
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public Guid Guid { get; set; }

        public BookmarkTypeEnum BookmarkType { get; set; }
        public DateTime BookmarkDate { get; set; }

        public ObjectTypeEnum ObjectType { get; set; }
        public int ObjectId { get; set; }

        public string ExtendedData { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
