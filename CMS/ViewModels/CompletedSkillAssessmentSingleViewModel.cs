using System;
using System.Collections.Generic;
using EZGO.Api.Models.Settings;
using Microsoft.AspNetCore.Http;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.Models.Checklist;
using WebApp.Models.Skills;

namespace WebApp.ViewModels
{
    public class CompletedSkillAssessmentSingleViewModel : SkillAssessment
    {
        public string CompletedTextValue { get; set; }
        public TimeZoneInfo Timezone { get; set; }
        public ApplicationSettings ApplicationSettings { get; set; }

        public Dictionary<string, string> CmsLanguage { get; set; }
        public string Locale { get; set; }

        public IMediaService MediaService { get; set; }
    }
}
