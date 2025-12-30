using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.SapPm
{
    public class SapPmLocation
    {
        #region - fields -
        public int Id { get; set; }
        public string FunctionalLocation { get; set; }
        public string FunctionalLocationName { get; set; }
        public bool MarkedForDeletion { get; set; }
        public bool HasChildren { get; set; }
        #endregion
    }
}
