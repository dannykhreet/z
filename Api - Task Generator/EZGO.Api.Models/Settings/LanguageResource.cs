using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Settings
{
    /// <summary>
    /// LanguageResource; Contains a full language pack for use within the application.
    /// In the database the language is stored in a GridType table, containing a key for a certain item (e.g. button_text) and a column per language containing the text for that language.
    /// This object can be used for getting an entire language set as one object.
    /// </summary>
    public class LanguageResource
    {
        public string Language { get; set; }
        public string LanguageCulture { get; set; }
        public string LanguageIso { get; set; }
        public List<LanguageResourceItem> ResourceItems{ get; set; }

        public LanguageResource()
        {
            ResourceItems = new List<LanguageResourceItem>();
        }
    }
}
