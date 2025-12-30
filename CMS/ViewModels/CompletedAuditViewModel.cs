using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using WebApp.Models.Audit;

namespace WebApp.ViewModels
{
    public class CompletedAuditViewModel : BaseViewModel
    {
        public CompletedAuditViewModel()
        {
        }
        public List<CompletedAuditModel> CompletedAudits { get; set; }
        public int TemplateId { get; set; }
        public int AuditId { get; set; }
    }
}
