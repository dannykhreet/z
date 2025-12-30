using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Language
{
    public class LanguageModel
    {

        public string Language { get; set; }

        public string LanguageEnglishName { get; set; }

        [Required]
        public string LanguageCulture { get; set; }

        public string LanguageIso { get; set; }

        public List<LanguageResourceItemModel> ResourceItems { get; set; }

        public LanguageModel()
        {
        }
    }

    public class LanguageModelComparer : IEqualityComparer<LanguageModel>
    {

        public bool Equals(LanguageModel x, LanguageModel y)
        {
            return x.LanguageCulture == y.LanguageCulture;
        }

        public int GetHashCode(LanguageModel obj)
        {
            return obj.LanguageCulture.GetHashCode();
        }
    }
}
