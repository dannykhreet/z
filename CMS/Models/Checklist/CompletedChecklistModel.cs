using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using WebApp.Models.Properties;
using WebApp.ViewModels;

namespace WebApp.Models.Checklist
{
    public class CompletedChecklistModel : BaseViewModel
    {
        public int Id { get; set; }
        public string  Name { get; set; }
        public string Picture { get; set; }
        public string AreaPathIds { get; set; }
        public List<CompletedChecklistSignature> Signatures { get; set; }
        public List<CompletedChecklistTaskModel> Tasks { get; set; }
        public List<Stage> Stages { get; set; }
        public List<UserBasic> EditedByUsers { get; set; }
        public List<OpenFieldModel> OpenFieldsProperties { get; set; }
        public List<OpenFieldModel> OpenFieldsPropertyUserValues { get; set; }
        public PictureProof PictureProof { get; set; }
        public List<Tag> Tags { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool EnableInBrowserPdfPrint { get; set; }
        //while 5145 is still in development, this setting is used to determine the environments it should be available on
        public bool EnablePropertyTiles { get; set; }
    }
}
