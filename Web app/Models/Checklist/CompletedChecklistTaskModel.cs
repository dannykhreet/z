using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using WebApp.Models.Properties;
using WebApp.Models.Shared;

namespace WebApp.Models.Checklist
{
    public class CompletedChecklistTaskModel : SharedTaskModel
    {
        public ApplicationSettings ApplicationSettings { get; set; }
    }
}