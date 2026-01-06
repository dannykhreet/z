using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTests.Helpers
{
    /// <summary>
    /// BaseObject for tests, can be used when retrieving tests to do basic parsing without inclusing the entire specific objects. 
    /// This object should ONLY contain id fields.
    /// </summary>
    public class BaseObject
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int ActionId { get; set; }
    }
}
