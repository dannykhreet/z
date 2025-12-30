using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Auditing
{
    public class AuditingPropertyTranslation
    {
        public string PropertyName { get; set; }
        public string ReadablePropertyName { get; set; }
        public Dictionary<int, string> TranslationsById { get; set; }

        public AuditingPropertyTranslation(string propertyName, string readableName, Dictionary<int, string> translationsById)
        {
            PropertyName = propertyName;
            ReadablePropertyName = readableName;
            TranslationsById = translationsById;
        }
    }
}
