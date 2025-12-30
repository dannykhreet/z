using EZGO.Api.Models.Settings;
using EZGO.CMS.LIB.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.Models.Action;
using WebApp.Models.Checklist;
using WebApp.Models.Comment;
using WebApp.Models.Pdf;
using WebApp.Models.Shared;

namespace WebApp.ViewModels
{
    public class PdfViewModel
    {
        // main model
        public PdfCompletedModel Item { get; set; }
        public List<PdfActionModel> Actions { get; set; } = new List<PdfActionModel>();
        public List<PdfCommentModel> Comments { get; set; } = new List<PdfCommentModel>();

        // viewmodel       
        public Dictionary<string, string> LanguageDictionary { get; set; } = new Dictionary<string, string>(); 
        public PdfTypeEnum Type { get; set; } = PdfTypeEnum.completedChecklist;
        public ApplicationSettings ApplicationSettings { get; set; }
        public string Locale { get; set; }
        public string TypeLanguageKey
        {
            get
            {
                switch (Type)
                {
                    case PdfTypeEnum.completedAudit:
                        return LanguageKeys.Audit.AreaTitle;

                    case PdfTypeEnum.completedChecklist:
                    default:
                        return LanguageKeys.Checklist.AreaTitle;
                }
            }
        }
    }
}
