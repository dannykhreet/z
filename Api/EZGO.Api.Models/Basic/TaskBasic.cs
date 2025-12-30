using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Basic
{
    /// <summary>
    /// TaskBasic; Basic object used within other classes.
    /// Basic objects are used for simple datasets and only contain a few Ids and a visual reference (e.g. name or description).
    /// NOTE! this is not meant to be a base class within the API, so don't use it as such within the API.
    /// </summary>
    public class TaskBasic
    {
        int TaskId { get; set; }
        int TaskTemplateId { get; set; }
        int Name { get; set; }
        string Type { get; set; }
    }
}
