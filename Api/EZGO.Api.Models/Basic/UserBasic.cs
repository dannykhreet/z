using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Basic
{
    /// <summary>
    /// UserBasic; Basic object for a user.
    /// Basic objects are used for simple datasets and only contain a few Ids and a visual reference (e.g. name or description).
    /// In this case the name will be a concatted FirstName and LastName and a Picte.
    /// NOTE! this is not meant to be a base class within the API, so don't use it as such within the API.
    /// </summary>
    public class UserBasic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
    }
}
