using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Basic
{
    /// <summary>
    /// AreaBasic; Basic object for Areas.
    /// Basic objects are used for simple datasets and only contain a few Ids and a visual reference (e.g. name or description).
    /// NOTE! this is not meant to be a base class within the API, so don't use it as such within the API.
    /// </summary>
    public class AreaBasic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NamePath { get; set; }
        public int? ParentId { get; set; }
        public AreaBasic()
        {
        }
    }
}
