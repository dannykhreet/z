using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
//using Elastic.Apm.Api;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Converters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
//using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace EZGO.Api.Logic.Managers
{
    public class TranslationManager : BaseManager<TranslationManager>, ITranslationManager
    {
        #region - private -
        private readonly HttpClient _httpClient;
        private readonly IDatabaseAccessHelper _manager;
        private readonly IDataAuditing _dataAuditing;
        private readonly IConfigurationHelper _configurationHelper;
        private IHttpContextAccessor _httpcontextaccessor;
        #endregion

        #region - managers -
        public TranslationManager(HttpClient httpClient, IDatabaseAccessHelper manager, IDataAuditing dataAuditing, IConfigurationHelper configurationHelper, IHttpContextAccessor httpContextAccessor, ILogger<TranslationManager> logger) : base(logger)
        {
            _httpClient = httpClient;
            _manager = manager;
            _dataAuditing = dataAuditing;
            _configurationHelper = configurationHelper;
            _httpcontextaccessor = httpContextAccessor;
        }
        #endregion

        #region - Convert language format -
        public static string[] ConvertToThreeLetterCodes(string[] languages)
        {
            string[] result = new string[languages.Length];

            for (int i = 0; i < languages.Length; i++)
            {
                var parts = languages[i].Split('_');
                if (parts.Length == 0)
                {
                    result[i] = null; // or handle error case
                    continue;
                }

                try
                {
                    CultureInfo culture = new CultureInfo(parts[0]);
                    result[i] = culture.ThreeLetterISOLanguageName;
                }
                catch (CultureNotFoundException)
                {
                    result[i] = null; // or fallback, e.g., parts[0]
                }
            }

            return result;
        }
        #endregion

        // takes any type of 
        #region - TranslateAndSaveObjectAsync -
        public async Task<bool> TranslateAndSaveObjectAsync(int objectId, string type)
        {
            string bearerToken = await getBearerToken();
            var baseUrl = _configurationHelper.GetValueAsString("AppSettings:BaseUriSmartAPI");
            var url = $"{baseUrl}/api/Translate/translatetemplate?type={type}&templateid={objectId}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TranslationManager.TranslateAndSaveObjectAsync(): ", ex.Message));
            }

            return true;
        }
        #endregion

        private async Task<string> getBearerToken()
        {
            string bearerToken;
            StringValues currenttoken = string.Empty;
            var succes = _httpcontextaccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out currenttoken);

            await Task.CompletedTask;

            if (succes)
            {
                bearerToken = StringConverters.ConvertAuthorizationHeaderToToken(currenttoken.ToString());
            }
            else
            {
                _logger.LogWarning("TranslationManager.getBearerToken(): No token found. {0}", "Token not found returning empty string.");
                return string.Empty;
            }

            return bearerToken;
        }

        #region - GetTranslationAsync -
        public async Task<(bool hasTranslation, object translation)> GetTranslationAsync(int objectId, int companyId, string language, string functionName, object component)
        {
            if (string.IsNullOrEmpty(language))
                return (false, component);

            string[] one_language = new string[] { language };

            language = ConvertToThreeLetterCodes(one_language)[0];

            //if (component is TasksTask tasksTask)
            //{
            //    if (tasksTask.TaskType == "checklist")
            //    {
            //        var tempId = (int)tasksTask.Id;

            //        var temp_parameters = new List<NpgsqlParameter>
            //        {

            //            new("@_objectId", tempId),
            //            new("@_table", "checklists_checklist")
            //        };

            //        objectId = Convert.ToInt32(await _manager.GetDataReader(
            //            $"SELECT public.get_templateid(@_objectId, @_table)",
            //            commandType: CommandType.Text,
            //            parameters: temp_parameters));
            //    }
            //    if (tasksTask.AuditId != null)
            //    {
            //        var tempId = (int)tasksTask.AuditId;
            //    }
            //}

            var parameters = new List<NpgsqlParameter>
            {

                new("@_objectId", objectId),
                new("@_companyid", companyId)
            };

            await using var reader = await _manager.GetDataReader(
                $"SELECT {functionName}(@_objectId, @_companyid, '{language}')",
                commandType: CommandType.Text,
                parameters: parameters);

            // ✅ Case 1: no row at all
            if (!await reader.ReadAsync())
                return (false, component);

            // ✅ Case 2: row exists but column is NULL
            if (reader.IsDBNull(0))
                return (false, component);

            string jsonString = reader.GetFieldValue<string>(0);
            if (string.IsNullOrEmpty(jsonString))
                return (false, component);

            //try
            //{
            using JsonDocument document = JsonDocument.Parse(jsonString);
            object translated_component = component;
            ApplyTranslationsRecursively(component, document.RootElement.Clone());
            return (true, component); // Clone to persist beyond the document's lifetime
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Hello!");
            //    // put a breakpoint here
            //    var message = ex.Message;   // inspect in debugger
            //    var stack = ex.StackTrace;  // inspect call stack
            //    throw; // rethrow if needed
            //}
        }
        #endregion

        #region -ApplyTranslationsRecursively-
        private void ApplyTranslationsRecursively(object obj, JsonElement translationElement)
        {
            if (obj == null) return;

            var properties = obj.GetType().GetProperties().ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            foreach (var prop in translationElement.EnumerateObject())
            {
                if (prop.Name.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    if (properties.TryGetValue("name", out var nameProperty) &&
                        nameProperty.CanWrite &&
                        nameProperty.PropertyType == typeof(string))
                    {
                        nameProperty.SetValue(obj, prop.Value.GetString());
                    }
                }
                else if (prop.Name.Equals("description", StringComparison.OrdinalIgnoreCase))
                {
                    if (properties.TryGetValue("description", out var descriptionProperty) &&
                        descriptionProperty.CanWrite &&
                        descriptionProperty.PropertyType == typeof(string))
                    {
                        descriptionProperty.SetValue(obj, prop.Value.GetString());
                    }
                }
                else
                {
                    if (!properties.TryGetValue(prop.Name, out var property))
                    {
                        // Special mapping: if JSON has TaskTemplates but CLR has Tasks
                        if (prop.Name.Equals("TaskTemplates", StringComparison.OrdinalIgnoreCase) &&
                            properties.TryGetValue("Tasks", out var tasksProperty))
                        {
                            property = tasksProperty;
                        }
                    }

                    if (property != null)
                    {
                        var propertyValue = property.GetValue(obj);
                        if (propertyValue != null)
                        {
                            if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) &&
                                property.PropertyType != typeof(string))
                            {
                                if (prop.Value.ValueKind == JsonValueKind.Array)
                                {
                                    var collection = propertyValue as IEnumerable;
                                    var translationArray = prop.Value.EnumerateArray().ToArray();

                                    int index = 0;
                                    foreach (var item in collection)
                                    {
                                        if (index < translationArray.Length)
                                        {
                                            ApplyTranslationsRecursively(item, translationArray[index]);
                                        }
                                        index++;
                                    }

                                    // Optional: warn if counts differ
                                    if (translationArray.Length != index)
                                    {
                                        Console.WriteLine($"Note: Collection '{property.Name}' has {index} items, translations supplied {translationArray.Length}");
                                    }
                                }
                            }
                            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                            {
                                if (prop.Value.ValueKind == JsonValueKind.Object)
                                {
                                    ApplyTranslationsRecursively(propertyValue, prop.Value);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}

