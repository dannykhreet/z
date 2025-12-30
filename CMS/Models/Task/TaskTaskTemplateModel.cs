using System;
using System.Collections.Generic;
using WebApp.Models.Checklist;

namespace WebApp.Models.Task
{
    public class TaskTaskTemplateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        //if filled show total instructions on checklist template. combined with or stepscount
        public string DescriptionFile { get; set; }
        public int StepsCount { get; set; }
        public List<TaskStepModel> Steps { get; set; }
    }
}
