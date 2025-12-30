using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Settings
{
    /// <summary>
    /// LanguageResourceItem; Resource items for use with <see cref="LanguageResource">LanguageResource</see>
    /// </summary>
    public class LanguageResourceItemCulture
    {
        public string ResourceKey { get; set; }
        public string ResourceValue { get; set; }
        public string Culture { get; set; }
    }
}
