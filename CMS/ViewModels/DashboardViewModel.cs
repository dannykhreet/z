using Amazon.S3.Model;
using EZGO.Api.Models.Stats;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WebApp.Models.Announcements;
using WebApp.Models.Audit;
using WebApp.Models.Checklist;
using WebApp.Models.Statistics;
using WebApp.Models.Task;

namespace WebApp.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public DashboardViewModel()
        {
        }

        public List<CompanyOverviewModel> CompanyTotals { get; set; }

        public List<LogAuditingModel> LogAuditings { get; set; }

        public List<AnnouncementModel> Announcements { get; set; }

        public List<CompletedChecklistModel> CompletedChecklists { get; set; }

        public List<CompletedAuditModel> CompletedAudits { get; set; }

        public List<CompletedTaskModel> CompletedTasks { get; set; }

        public List<Models.Action.ActionModel> Actions { get; set; }

        public StatsTotals StatisticTotals { get; set; }
        public bool ShowNocNavigation { get; set; }
        public bool EnableInBrowserPdfPrint { get; set; }
    }
}
