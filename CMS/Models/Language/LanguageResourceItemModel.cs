using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApp.Models.Language
{
    public class LanguageResourceItemModel
    {
        public string Description { get; set; }

        [Required]
        public string Guid { get; set; }

        [Required]
        public string ResourceKey { get; set; }

        public string ResourceValue { get; set; }

        [JsonIgnore]
        public string Locale { get; set; }

        public LanguageResourceItemModel()
        {
        }

    }

    public class LanguageResourceItemModelComparer : IEqualityComparer<LanguageResourceItemModel>
    {
        public bool Equals(LanguageResourceItemModel x, LanguageResourceItemModel y)
        {
            if (x.ResourceKey == y.ResourceKey)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(LanguageResourceItemModel obj)
        {
            return obj.ResourceKey.GetHashCode();
        }
    }
}
