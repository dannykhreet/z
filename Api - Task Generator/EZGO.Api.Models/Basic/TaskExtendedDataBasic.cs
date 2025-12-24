using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Basic
{
    /// <summary>
    /// TaskExtendedDataBasic; Basic object used within other classes.
    /// Basic objects are used for simple datasets and only contain a few Ids and a visual reference (e.g. name or description).
    /// NOTE! this is not meant to be a base class within the API, so don't use it as such within the API.
    /// </summary>
    public class TaskExtendedDataBasic
    {
        public int TaskId { get; set; }
        public int? TimeRealizedById { get; set; }
        public int? TimeTaken { get; set; }
        public string TimeRealizedBy { get; set; }
        public List<TaskPropertyUserValueBasic> PropertyUserValues { get; set; }
        public int? CompletedDeeplinkId { get; set; }

    }
}
