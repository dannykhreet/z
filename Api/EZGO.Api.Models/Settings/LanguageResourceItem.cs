using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Settings
{
    /// <summary>
    /// LanguageResourceItem; Resource items for use with <see cref="LanguageResource">LanguageResource</see>
    /// </summary>
    public class LanguageResourceItem
    {
        public string Description { get; set; }
        public string Guid { get; set; }
        public string ResourceKey { get; set; }
        public string ResourceValue { get; set; }
    }
}
