using EZGO.Api.Models;
using EZGO.Api.Models.Data;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Logic.Auditing;
using WebApp.Logic.Interfaces;
using WebApp.Models.Auditing;
using WebApp.Models.LogAuditing;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class AuditingController : BaseController
    {
        private readonly ILogger<AuditingController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;
        private List<string> ignoreList;
        private List<string> ignoreListForChecklists;
        private List<string> completeIgnoreList;

        public AuditingController(ILogger<AuditingController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;

            //todo: move ingore list to readability translator as it is readability related functionality
            //lists of fields to ignore
            ignoreList = new List<string> { "", "modified_at", "created_at", "company_id" };
            ignoreListForChecklists = new List<string> { "machine_status" };

            //form a complete list of properties to ignore for the object type being viewed
            completeIgnoreList = new List<string>(ignoreList);
        }

        [Route("/auditing")]
        public async Task<IActionResult> IndexAsync()
        {
            var output = new AuditingViewModel();
            output.ApplicationSettings = await GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");

            if (!output.EnablingAuditing)
            {
                return NotFound();
            }

            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Auditing overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.AUDITING;
            output.Locale = _locale;

            return View(output);
        }

        [HttpGet]
        [Route("/auditing/all")]
        public async Task<IActionResult> GetAllChanges([FromQuery] string[] objecttypes, [FromQuery] string description, [FromQuery] DateTime createdonstart, [FromQuery] DateTime createdonend, [FromQuery] int? objectid = null, [FromQuery] int? userid = null, [FromQuery] int? limit = null, [FromQuery] int? offset = null)
        {
            string query = "";
            if (objecttypes != null)
            {
                foreach (string objecttype in objecttypes)
                    query += "&objecttypes=" + objecttype;
            }
            if (description != null)
            {
                query += "&description=" + description;
            }
            if (createdonstart != null)
            {
                query += "&createdonstart=" + createdonstart.ToString("dd-MM-yyyy HH:mm:ss");
            }
            if (createdonend != null)
            {
                query += "&createdonend=" + createdonend.ToString("dd-MM-yyyy HH:mm:ss");
            }
            if (objectid != null)
            {
                query += "&objectid=" + objectid;
            }
            if (userid != null)
            {
                query += "&userid=" + userid;
            }


            //var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingLogAuditing, limit, offset) + query);
            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingLogAuditingOverview, limit, offset) + query);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return Ok(result.Message);
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        [HttpGet]
        [Route("/auditing/details/{id}")]
        public async Task<IActionResult> Details(int id, [FromQuery] string objectType)
        {
            ApplicationSettings applicationSettings = await GetApplicationSettings();
            bool auditingAccessAllowed = applicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");
            if (!auditingAccessAllowed)
            {
                return NotFound();
            }

            var tz = User.FindFirst(System.Security.Claims.ClaimTypes.Country)?.Value ?? "Europe/Amsterdam";
            TimeZoneInfo timezone = TZConvert.EzFindTimeZoneInfoById(tz);

            AuditingDataTranslator auditingDataTranslator = new(objectType, timezone);

            string urlPart = "checklisttemplate";
            switch (objectType)
            {
                case "checklists_checklisttemplate":
                    urlPart = "checklisttemplate";
                    completeIgnoreList.AddRange(ignoreListForChecklists);
                    break;
                case "audits_audittemplate":
                    urlPart = "audittemplate";
                    break;
                case "assessment_templates":
                    urlPart = "assessmenttemplate";
                    break;
                case "workinstruction_templates":
                    urlPart = "workinstructiontemplate";
                    break;
                case "tasks_tasktemplate":
                    urlPart = "tasktemplate";
                    break;
            }

            var result = await _connector.GetCall(string.Format(@"/v1/logauditing/{0}/{1}/details?", urlPart, id));
            if (result.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                AuditingDetailsFullViewModel fullVM = new()
                {
                    ItemChangeDetails = new List<AuditingDetailsViewModel>(),
                    CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale),
                    Locale = _locale,
                    PageTitle = "Auditing details",
                    ApplicationSettings = applicationSettings,
                    ObjectType = objectType
                };
                fullVM.Filter.Module = FilterViewModel.ApplicationModules.AUDITING;
                fullVM.Filter.CmsLanguage = fullVM.CmsLanguage;

                List<AuditingObjectData> auditingObjects = result.Message.ToObjectFromJson<List<AuditingObjectData>>();

                foreach (AuditingObjectData auditingObject in auditingObjects)
                {
                    AuditingDetailsViewModel detailsvm = PrepRelevantAuditingDetails(auditingObject);
                    auditingDataTranslator.PrepDeepLinkForTranslation(detailsvm.AuditingLogChanges);
                    if (detailsvm.AuditingLogChanges.Count > 0)
                        fullVM.ItemChangeDetails.Add(detailsvm);
                }

                List<AuditingPropertyTranslation> translations = await GetAuditingPropertyTranslationsAsync(fullVM);
                auditingDataTranslator.AddPropertyTranslations(translations);

                //translate data to readable
                foreach (AuditingDetailsViewModel detailsVM in fullVM.ItemChangeDetails)
                {
                    if (detailsVM.AuditingLogChanges != null)
                    {
                        auditingDataTranslator.TranslateDeepLinks(detailsVM.AuditingLogChanges);
                        //make signatures readable
                        auditingDataTranslator.TranslateSignaturesRequired(detailsVM.AuditingLogChanges);
                    }

                    foreach (var change in detailsVM.AuditingLogChanges)
                    {
                        auditingDataTranslator.TranslateData(change);
                    }
                }

                return PartialView("_details", fullVM);
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        [NonAction]
        private async Task<List<AuditingPropertyTranslation>> GetAuditingPropertyTranslationsAsync(AuditingDetailsFullViewModel fullVM)
        {
            List<AuditingPropertyTranslation> auditingPropertyTranslations = new();
            Dictionary<string, List<int>> idsToTranslate = GetRelevantIdsFromViewModel(fullVM);

            foreach (string technicalId in idsToTranslate.Keys)
            {
                string readableIdTranslation = "";
                string urlPart = "";

                switch (technicalId)
                {
                    case "workinstruction_template_id":
                        readableIdTranslation = "Work Instructions";
                        urlPart = "workinstructiontemplates";
                        break;
                    case "checklisttemplate_id":
                        readableIdTranslation = "Checklist template";
                        urlPart = "checklisttemplates";
                        break;
                    case "audittemplate_id":
                        readableIdTranslation = "Audit template";
                        urlPart = "audittemplates";
                        break;
                    case "tasktemplate_id":
                        //readableIdTranslation = "Task template";
                        //urlPart = "";
                        break;
                    case "area_id":
                        readableIdTranslation = "Area";
                        urlPart = "areas";
                        break;
                    case "tag_id":
                        readableIdTranslation = "Tags";
                        urlPart = "tags";
                        break;
                    case "connected_checklists":
                        readableIdTranslation = "Connected checklists";
                        urlPart = "checklisttemplates";
                        break;
                    case "connected_audits":
                        readableIdTranslation = "Connected audits";
                        urlPart = "audittemplates";
                        break;
                }

                if (readableIdTranslation.IsNullOrEmpty() || urlPart.IsNullOrEmpty()) { continue; }

                List<int> idsToTranslateList = idsToTranslate.GetValueOrDefault(technicalId);

                Dictionary<int, string> translatedNamesById = new();
                StringBuilder stringBuilder = new();
                foreach (int id in idsToTranslateList)
                {
                    if (stringBuilder.Length > 0) stringBuilder.Append('&');
                    stringBuilder.Append("ids=");
                    stringBuilder.Append(id);
                }
                var NamesResult = await _connector.GetCall(string.Format(@"/v1/" + urlPart + "/names?" + stringBuilder.ToString()));

                if (NamesResult.StatusCode == HttpStatusCode.OK)
                {
                    translatedNamesById = NamesResult.Message.ToObjectFromJson<Dictionary<int, string>>();
                }

                AuditingPropertyTranslation workinstructiontranslations = new(technicalId, readableIdTranslation, translatedNamesById);
                auditingPropertyTranslations.Add(workinstructiontranslations);
            }

            return auditingPropertyTranslations;
        }

        [NonAction]
        private AuditingDetailsViewModel PrepRelevantAuditingDetails(AuditingObjectData auditingObjectData)
        {
            AuditingDetailsViewModel auditingDetailsVM = new();

            JToken original = JsonConvert.DeserializeObject<JToken>(auditingObjectData.OriginalObjectDataJson);
            JToken mutated = JsonConvert.DeserializeObject<JToken>(auditingObjectData.MutatedObjectDataJson);

            string nameOfObject = "";
            List<AuditingLogChange> changes = new();

            if (original != null)
            {
                if (original is JObject originalJobject)
                {
                    if (mutated is JObject mutatedJobject)
                    {
                        //if original not in mutated or in mutated but different value: original is removed
                        //if mutated not in original or in original but different value: mutated is added

                        foreach (var originalProperty in originalJobject.Properties())
                        {
                            var mutatedProperty = mutatedJobject.Property(originalProperty.Name);

                            if (mutatedProperty != null && mutatedProperty.Name.Equals("name"))
                            {
                                nameOfObject = mutatedProperty.Value.ToString();
                            }

                            if (completeIgnoreList.Contains(originalProperty.Name)) continue;

                            if (mutatedProperty == null)
                            {
                                //the property was removed
                                string removedValue = originalProperty.Value.ToString();
                                if (!removedValue.IsNullOrEmpty())
                                    changes.Add(new AuditingLogChange() { Name = originalProperty.Name, RemovedValues = new() { removedValue } });
                            }
                            else if (!mutatedProperty.Value.Equals(originalProperty.Value))
                            {
                                //the property value was edited
                                string removedValue = originalProperty.Value.ToString();
                                string addedValue = mutatedProperty.Value.ToString();
                                if (!addedValue.IsNullOrEmpty() || !removedValue.IsNullOrEmpty())
                                    changes.Add(new AuditingLogChange() { Name = mutatedProperty.Name, AddedValues = new() { addedValue }, RemovedValues = new() { removedValue } });
                            }
                        }
                        //handle added properties
                        var addedProperties = mutatedJobject.Properties().Where(mutatedProperty => !originalJobject.Properties().Select(originalProperty => originalProperty.Name).ToList().Contains(mutatedProperty.Name)).ToList();
                        foreach (var addedProperty in addedProperties)
                        {
                            changes.Add(new AuditingLogChange() { Name = addedProperty.Name, AddedValues = new() { addedProperty.Value.ToString() } });
                        }
                    }
                    else
                    {
                        foreach (var originalProperty in originalJobject.Properties())
                        {
                            if (originalProperty.Name.Equals("name")) nameOfObject = originalProperty.Value.ToString();
                            string removedValue = originalProperty.Value.ToString();
                            if (!removedValue.IsNullOrEmpty())
                                changes.Add(new AuditingLogChange() { Name = originalProperty.Name, RemovedValues = new() { originalProperty.Value.ToString() } });
                        }
                    }
                }
                else if (original is JArray originalArray)
                {
                    //handle collection
                    //todo detect name in a better way
                    string collectionObjectTypeIdentifier = "";
                    if (auditingObjectData.ObjectType.Equals("tags_tag_relation"))
                    {
                        collectionObjectTypeIdentifier = "tag_id";
                    }
                    else if (auditingObjectData.ObjectType.Equals("workinstruction_template_tasktemplate") || auditingObjectData.ObjectType.Equals("workinstruction_template_checklisttemplate_item") || auditingObjectData.ObjectType.Equals("workinstruction_template_audittemplate_item"))
                    {
                        collectionObjectTypeIdentifier = "workinstruction_template_id";
                    }

                    JArray mutatedArray = null;
                    if (mutated is JArray castedMutatedArray)
                    {
                        mutatedArray = castedMutatedArray;
                    }

                    if (!completeIgnoreList.Contains(collectionObjectTypeIdentifier))
                    {
                        AuditingLogChange change = new()
                        {
                            Name = collectionObjectTypeIdentifier
                        };
                        foreach (JToken originalToken in originalArray)
                        {
                            if (mutatedArray != null)
                            {
                                if (!mutatedArray.Select(token => token.ToString()).Contains(originalToken.ToString()))
                                {
                                    if (originalToken is JObject originalObject)
                                    {
                                        change.RemovedValues.Add(originalObject.Property(collectionObjectTypeIdentifier).Value.ToString());
                                    }

                                }
                            }
                            else
                            {
                                if (originalToken is JObject originalObject)
                                {
                                    change.RemovedValues.Add(originalObject.Property(collectionObjectTypeIdentifier).Value.ToString());
                                }
                            }
                        }

                        if (mutatedArray != null)
                        {
                            foreach (JToken mutatedToken in mutatedArray)
                            {
                                if (!originalArray.Select(token => token.ToString()).Contains(mutatedToken.ToString()))
                                {
                                    if (mutatedToken is JObject mutatedObject)
                                    {
                                        change.AddedValues.Add(mutatedObject.Property(collectionObjectTypeIdentifier).Value.ToString());
                                    }
                                }
                            }
                        }

                        if (change.RemovedValues.Count > 0 || change.AddedValues.Count > 0)
                            changes.Add(change);
                    }
                }
            }
            else if (mutated != null)
            {
                if (mutated is JObject mutatedJobject)
                {
                    foreach (var addedProperty in mutatedJobject.Properties())
                    {
                        if (addedProperty.Name.Equals("name")) nameOfObject = addedProperty.Value.ToString();
                        if (completeIgnoreList.Contains(addedProperty.Name)) continue;
                        string value = addedProperty.Value.ToString();
                        if (!value.IsNullOrEmpty())
                            changes.Add(new AuditingLogChange() { Name = addedProperty.Name, AddedValues = new() { value } });
                    }
                }
                else if (mutated is JArray mutatedArray)
                {
                    string collectionObjectTypeIdentifier = "";
                    if (auditingObjectData.ObjectType.Equals("tags_tag_relation"))
                    {
                        collectionObjectTypeIdentifier = "tag_id";
                    }
                    else if (auditingObjectData.ObjectType.Equals("workinstruction_template_tasktemplate") || auditingObjectData.ObjectType.Equals("workinstruction_template_checklisttemplate_item") || auditingObjectData.ObjectType.Equals("workinstruction_template_audittemplate_item"))
                    {
                        collectionObjectTypeIdentifier = "workinstruction_template_id";
                    }
                    AuditingLogChange change = new()
                    {
                        Name = collectionObjectTypeIdentifier
                    };
                    nameOfObject = collectionObjectTypeIdentifier;

                    if (!completeIgnoreList.Contains(collectionObjectTypeIdentifier))
                    {
                        //handle collection
                        foreach (JToken mutatedToken in mutatedArray)
                        {
                            if (mutatedToken is JObject mutatedObject)
                            {
                                change.AddedValues.Add(mutatedObject.Property(collectionObjectTypeIdentifier).Value.ToString());
                            }
                        }
                        if (change.AddedValues.Count > 0)
                            changes.Add(change);
                    }
                }
            }

            auditingDetailsVM.ObjectName = nameOfObject;
            auditingDetailsVM.Description = auditingObjectData.Description;
            auditingDetailsVM.AuditingLogChanges = changes;
            return auditingDetailsVM;
        }

        [NonAction]
        private async Task<AuditingPropertyTranslation> GetAreaPropertyTranslationAsync()
        {
            var resultAreasFlat = await _connector.GetCall(Constants.General.AreaFlatList);
            if (resultAreasFlat.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var areas = JsonConvert.DeserializeObject<List<Area>>(resultAreasFlat.Message);
            Dictionary<int, string> areaIdTranslations = new();

            foreach (var area in areas)
            {
                areaIdTranslations.Add(area.Id, area.FullDisplayName);
            }
            return new AuditingPropertyTranslation("area_id", "Area", areaIdTranslations);
        }

        [NonAction]
        private async Task<AuditingPropertyTranslation> GetTagPropertyTranslationAsync()
        {
            var resultTags = await _connector.GetCall(Constants.Tags.GetTags);
            if (resultTags.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var tags = JsonConvert.DeserializeObject<List<Tag>>(resultTags.Message);
            Dictionary<int, string> tagIdTranslations = new();

            foreach (var tag in tags)
            {
                tagIdTranslations.Add(tag.Id, tag.Name);
            }
            return new AuditingPropertyTranslation("tag_id", "Tag", tagIdTranslations);
        }

        [NonAction]
        private Dictionary<string, List<int>> GetRelevantIdsFromViewModel(AuditingDetailsFullViewModel fullVM)
        {
            List<string> technicalIdNames = new() { "workinstruction_template_id", "tag_id", "area_id", "connected_checklists", "connected_audits", "audittemplate_id", "checklisttemplate_id" };

            Dictionary<string, List<int>> TranslationIds = new();

            foreach (string technicalIdName in technicalIdNames)
            {
                HashSet<int> uniqueIds = new();
                foreach (var details in fullVM.ItemChangeDetails)
                {
                    foreach (var item in details.AuditingLogChanges)
                    {
                        if (item.Name.Equals(technicalIdName))
                        {
                            foreach (var value in item.AddedValues)
                            {
                                if (int.TryParse(value, out int id))
                                    uniqueIds.Add(id);
                            }
                            foreach (var value in item.RemovedValues)
                            {
                                if (int.TryParse(value, out int id))
                                    uniqueIds.Add(id);
                            }
                        }
                    }
                }
                TranslationIds.Add(technicalIdName, uniqueIds.ToList());
            }

            return TranslationIds;
        }
    }
}
