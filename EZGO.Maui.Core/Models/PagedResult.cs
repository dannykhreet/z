using System;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Models
{
    public class PagedResult<T> where T : class
    {
        public IEnumerable<T> Results { get; set; }
    }
}
