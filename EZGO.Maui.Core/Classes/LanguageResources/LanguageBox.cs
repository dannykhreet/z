using System;
using System.Collections.Generic;
using EZGO.Maui.Core.Classes.LanguageResources.ResourceTypes;

namespace EZGO.Maui.Core.Classes.LanguageResources
{
    /// <summary>
    /// Holds Language Resources Instances
    /// </summary>
    public class LanguageBox
    {
        private Dictionary<DictionaryEnum, BaseLanguageResource> baseLanguageResources;

        /// <summary>
        /// Creates new Instance of Language Box and creates new language resources dictionary
        /// </summary>
        public LanguageBox()
        {
            baseLanguageResources = new Dictionary<DictionaryEnum, BaseLanguageResource>();
            baseLanguageResources.Add(DictionaryEnum.API, new ApiLanguageResource());
            baseLanguageResources.Add(DictionaryEnum.LOCAL, new LocalLanguageResource());
            baseLanguageResources.Add(DictionaryEnum.EMBEDDED, new EmbeddedLanguageResource());
        }

        public BaseLanguageResource ApiInsatnce { get => baseLanguageResources.GetValueOrDefault(DictionaryEnum.API); }
        public BaseLanguageResource LocalInsatnce { get => baseLanguageResources.GetValueOrDefault(DictionaryEnum.LOCAL); }
        public BaseLanguageResource EmbeddedInsatnce { get => baseLanguageResources.GetValueOrDefault(DictionaryEnum.EMBEDDED); }

        private enum DictionaryEnum
        {
            API, LOCAL, EMBEDDED
        }
    }
}
