using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Properties;

namespace WebApp.Models.Shared
{
    public class SharedTaskModel// : EZGO.Api.Models.TasksTask
    {
        public int Index { get; set; }
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int ActionsCount { get; set; }
        public int CommentCount { get; set; }
        public string Picture { get; set; }
        public int AreaId { get; set; }
        public bool IsDoubleSignatureRequired { get; set; }
        public List<UserBasic> EditedByUsers { get; set; }
        public bool IsSignatureRequired { get; set; }
        public string Video { get; set; }
        public string VideoThumbnail { get; set; }
        public List<TemplatePropertyModel> Properties { get; set; } = new List<TemplatePropertyModel>();
        public List<PropertyUserValueModel> PropertyUserValues { get; set; } = new List<PropertyUserValueModel>();
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public Signature Signature { get; set; }
        public PictureProof PictureProof { get; set; }

        // Audits
        public int Score { get; set; }

        // Property string
        public string PropertyString { get; set; }
    }
}
