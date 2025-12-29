using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Language;

namespace WebApp.Logic.Interfaces
{
    public interface ILanguageService
    {
        Task PrimeLanguages();
        Task<Dictionary<string, string>> GetLanguageDictionaryAsync(string locale);
        Task<List<SelectListItem>> GetLanguageSelectorItems();
        Task<List<string>> GetActiveLanguages();
        Task<List<LanguageModel>> GetLanguageItems();
    }
}
