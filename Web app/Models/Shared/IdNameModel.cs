using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Shared
{
    /// <summary>
    /// IdNameModel; simple model for use with certain calls to just get a id and/or name.
    /// </summary>
    public class IdNameModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
