using EZGO.Api.Models.Enumerations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebApp.Models.Task
{
    public class TaskRecurrencyModel
    {
        public TaskRecurrencyModel()
        {

        }

        [Required]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        [Required]
        public int TemplateId { get; set; }
        [Required]
        public int AreaId { get; set; }
        [Required]
        public string RecurrencyType { get; set; }
        public TaskScheduleModel Schedule { get; set; }
        public List<int> Shifts { get; set; }
    }
}