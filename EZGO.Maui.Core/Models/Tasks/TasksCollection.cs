using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class TasksCollection
    {
        public List<BasicTaskModel> Shift { get; set; }
        public List<BasicTaskModel> Today { get; set; }
        public List<BasicTaskModel> Week { get; set; }
        public List<BasicTaskModel> Overdue { get; set; }

        public bool ListAreLoaded { get; private set; } = false;

        /// <summary>
        /// Temporary fix for tasks being present overdue and week bar at the same time
        /// </summary>
        /// <remarks>After a fix is introducted on the API side this methods should be removed</remarks>
        public void EnsureNoOverdueDuplcates()
        {
            if (Week == null || Overdue == null)
                return;

            var duplicated = Overdue.Where(x => x.RecurrencyType != "dynamicday").Join(Week, outer => outer.TemplateId, inner => inner.TemplateId, (inner, outer) => new { overdue = inner, week = outer }).ToList();

            foreach (var pair in duplicated)
            {
                if (pair.overdue.DueAt < pair.week.DueAt)
                    Overdue.Remove(pair.overdue);
                else
                    Week.Remove(pair.week);
            }
        }

        /// <summary>
        /// Temporary fix for marked in past tasks being present in week bar 
        /// </summary>
        /// <remarks>After a fix is introducted on the API side this methods should be removed</remarks>
        public void EnsureNoPastTasks()
        {
            RemovePastTasks(Week);
        }

        private void RemovePastTasks(List<BasicTaskModel> tasks)
        {
            if (tasks == null)
                return;

            var pastTasks = tasks.Where(t => t.IsTaskMarked && t.DueAt < DateTime.Now).ToList();

            if (pastTasks.Any())
            {
                foreach (var task in pastTasks)
                {
                    tasks.Remove(task);
                }
            }
        }

        public void SetByPeriod(TaskPeriodTypes period, List<BasicTaskModel> tasks)
        {
            if (tasks.IsNullOrEmpty())
                return;

            var periodTasks = tasks.Where(x => x.TaskPeriods.HasFlag(period)).ToList();
            switch (period)
            {
                case TaskPeriodTypes.Shift:
                    Shift = periodTasks;
                    break;
                case TaskPeriodTypes.Today:
                    Today = periodTasks;
                    break;
                case TaskPeriodTypes.Week:
                    Week = periodTasks;
                    break;
                case TaskPeriodTypes.OverDue:
                    Overdue = periodTasks;
                    break;
            }
            ListAreLoaded = true;
        }

        public List<BasicTaskModel> GetByPeriod(TaskPeriod period)
        {
            switch (period)
            {
                case TaskPeriod.Shift:
                    return Shift;
                case TaskPeriod.Today:
                    return Today;
                case TaskPeriod.Week:
                    return Week;
                case TaskPeriod.OverDue:
                    return Overdue;

                default:
                    return new List<BasicTaskModel>();
            }
        }

        public IEnumerable<BasicTaskModel> GetAllTasks()
        {
            var unmodifiedList = new List<BasicTaskModel>(Shift?.Concat(Today).Concat(Week).Concat(Overdue) ?? new List<BasicTaskModel>());
            var hashedList = unmodifiedList.ToHashSet();

            return hashedList.Select(x => x).ToList();
        }

        internal void SetOverdue(List<BasicTaskModel> overdue)
        {
            Overdue = overdue;
        }
    }
}
