using EZGO.Api.Models;
using EZGO.Api.Models.Data;
using EZGO.Api.Models.Enumerations;
using EZGO.CMS.LIB.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models.Auditing;
using WebApp.Models.LogAuditing;
using WebApp.ViewModels;

namespace WebApp.Logic.Auditing
{
    public class AuditingDataTranslator
    {
        private List<string> ignoreList;
        private List<string> ignoreListForChecklists;
        private List<string> completeIgnoreList;

        //private List<string> ignoreList = new List<string> { "modified_at", "created_at", "company_id", "created_by_id", "modified_by_id" };
        //private List<string> completeIgnoreList;
        //private List<string> ignoreListForChecklists = new List<string> { "role", "machine_status" };

        //private static readonly string signatureRequiredKey = "signature_required";
        //private static readonly string doubleSignatureRequiredKey = "double_signature_required"; //for checklists
        //private static readonly string signatureTypeKey = "signature_type"; //for assesments

        //public List<string> CustomTranslationProperties = new() { signatureRequiredKey, doubleSignatureRequiredKey, signatureTypeKey };

        private List<AuditingPropertyTranslation> PropertiesTranslations = new();
        //private Dictionary<string, string> TranslatedCollectionNames = new() { { "tags_tag_relation", "Tags" } };

        //public Dictionary<string, string> ValuesOld = new Dictionary<string, string>();
        //public Dictionary<string, string> ValuesNew = new Dictionary<string, string>();

        private TimeZoneInfo timezone = TZConvert.EzFindTimeZoneInfoById("Europe/Amsterdam");

        public AuditingDataTranslator(string objectType, TimeZoneInfo timeZone)
        {
            this.timezone = timeZone;
            ignoreList = new List<string> { "modified_at", "created_at", "company_id" };
            ignoreListForChecklists = new List<string> { "machine_status" };

            //form a complete list of properties to ignore for the object type being viewed
            completeIgnoreList = new List<string>(ignoreList);
        }

        public void AddPropertyTranslation(AuditingPropertyTranslation propertyTranslation)
        {
            PropertiesTranslations.Add(propertyTranslation);
        }

        public void AddPropertyTranslations(List<AuditingPropertyTranslation> propertyTranslations)
        {
            PropertiesTranslations.AddRange(propertyTranslations);
        }

        public void TranslateData(AuditingLogChange change)
        {

            AuditingPropertyTranslation translation = PropertiesTranslations.Find(x => x.PropertyName.Equals(change.Name));
            if (translation != null)
            {
                change.Name = translation.ReadablePropertyName;

                List<string> translatedAddedValues = new();
                foreach (var value in change.AddedValues)
                {
                    if (int.TryParse(value, out int idT))
                    {
                        translatedAddedValues.Add(translation.TranslationsById.GetValueOrDefault(idT) ?? value);
                    }
                    else
                    {
                        translatedAddedValues.Add(value);
                    }
                }
                change.AddedValues = translatedAddedValues;

                List<string> translatedRemovedValues = new();
                foreach (var value in change.RemovedValues)
                {
                    if (int.TryParse(value, out int idT))
                    {
                        translatedRemovedValues.Add(translation.TranslationsById.GetValueOrDefault(idT) ?? value);
                    }
                    else
                    {
                        translatedRemovedValues.Add(value);
                    }
                }
                change.RemovedValues = translatedRemovedValues;
            }
        }

        public List<AuditingLogChange> TranslateSignaturesRequired(List<AuditingLogChange> changes)
        {
            string signatureRequiredKey = "signature_required";
            string doubleSignatureRequiredKey = "double_signature_required"; //for checklists
            string signatureTypeKey = "signature_type"; //for assesments

            //signatures
            string readableSignaturesTitle = "Number of signatures required"; //translate
            string doubleSignatureRequiredTranslatedValue = "two signatures"; //translate
            string singleSignatureRequiredTranslatedValue = "one signature"; //translate
            string noSignatureRequiredTranslatedValue = "none"; //translate

            AuditingLogChange signatureRequired = changes.Find(x => x.Name.Equals(signatureRequiredKey));
            AuditingLogChange doubleSignatureRequired = changes.Find(x => x.Name.Equals(doubleSignatureRequiredKey));
            AuditingLogChange signatureType = changes.Find(x => x.Name.Equals(signatureTypeKey));

            //handle signature type int
            if (signatureType != null)
            {
                signatureType.Name = readableSignaturesTitle;

                if (signatureType.AddedValues != null && signatureType.AddedValues.Count > 0 && int.TryParse(signatureType.AddedValues[0], out int intSignatureTypeAdded))
                {
                    if ((RequiredSignatureTypeEnum)intSignatureTypeAdded == RequiredSignatureTypeEnum.NoSignaturedRequired)
                    {
                        signatureType.AddedValues[0] = noSignatureRequiredTranslatedValue;
                    }
                    else if ((RequiredSignatureTypeEnum)intSignatureTypeAdded == RequiredSignatureTypeEnum.OneSignatureRequired)
                    {
                        signatureType.AddedValues[0] = singleSignatureRequiredTranslatedValue;
                    }
                    else if ((RequiredSignatureTypeEnum)intSignatureTypeAdded == RequiredSignatureTypeEnum.TwoSignatureRequired)
                    {
                        signatureType.AddedValues[0] = doubleSignatureRequiredTranslatedValue;
                    }
                }

                if (signatureType.RemovedValues != null && signatureType.RemovedValues.Count > 0 && int.TryParse(signatureType.RemovedValues[0], out int intSignatureTypeRemoved))
                {
                    if ((RequiredSignatureTypeEnum)intSignatureTypeRemoved == RequiredSignatureTypeEnum.NoSignaturedRequired)
                    {
                        signatureType.RemovedValues[0] = noSignatureRequiredTranslatedValue;
                    }
                    else if ((RequiredSignatureTypeEnum)intSignatureTypeRemoved == RequiredSignatureTypeEnum.OneSignatureRequired)
                    {
                        signatureType.RemovedValues[0] = singleSignatureRequiredTranslatedValue;
                    }
                    else if ((RequiredSignatureTypeEnum)intSignatureTypeRemoved == RequiredSignatureTypeEnum.TwoSignatureRequired)
                    {
                        signatureType.RemovedValues[0] = doubleSignatureRequiredTranslatedValue;
                    }
                }

                if (signatureRequired != null)
                {
                    changes.Remove(signatureRequired);
                    signatureRequired = null;
                }
                if (doubleSignatureRequired != null) 
                {
                    changes.Remove(doubleSignatureRequired);
                    doubleSignatureRequired = null;
                }
            }

            //handle signature booleans
            else
            {
                if (doubleSignatureRequired != null)
                {
                    if (doubleSignatureRequired.AddedValues != null && doubleSignatureRequired.AddedValues.Count > 0 && bool.TryParse(doubleSignatureRequired.AddedValues[0], out bool doubleSignatureIsRequiredAdded))
                    {
                        doubleSignatureRequired.Name = readableSignaturesTitle;
                        if (doubleSignatureIsRequiredAdded)
                        {
                            doubleSignatureRequired.AddedValues[0] = doubleSignatureRequiredTranslatedValue;
                            //if single signature exists, remove it. The entry for double is already enought information when it is true;
                            if (signatureRequired != null)
                            {
                                changes.Remove(signatureRequired);
                                signatureRequired = null;
                            }
                        } 
                        else
                        {
                            if (signatureRequired == null)
                            {
                                doubleSignatureRequired.AddedValues[0] = singleSignatureRequiredTranslatedValue;
                            }
                            else
                            {
                                doubleSignatureRequired.AddedValues[0] = noSignatureRequiredTranslatedValue;
                            }
                        }
                    }

                    if (doubleSignatureRequired.RemovedValues != null && doubleSignatureRequired.RemovedValues.Count > 0 && bool.TryParse(doubleSignatureRequired.RemovedValues[0], out bool doubleSignatureIsRequiredRemoved))
                    {
                        if (doubleSignatureIsRequiredRemoved)
                        {
                            doubleSignatureRequired.Name = readableSignaturesTitle;
                            doubleSignatureRequired.RemovedValues[0] = doubleSignatureRequiredTranslatedValue;
                            //if single signature exists, remove it. The entry for double is already enought information when it is true;
                            if (signatureRequired != null)
                            {
                                changes.Remove(signatureRequired);
                                signatureRequired = null;
                            }
                        }
                        else
                        {
                            if (signatureRequired == null)
                            {
                                doubleSignatureRequired.RemovedValues[0] = singleSignatureRequiredTranslatedValue;
                            }
                            else
                            {
                                doubleSignatureRequired.RemovedValues[0] = noSignatureRequiredTranslatedValue;
                            }
                        }
                    }
                }

                if (signatureRequired != null)
                {
                    if (signatureRequired.AddedValues != null && signatureRequired.AddedValues.Count > 0 && bool.TryParse(signatureRequired.AddedValues[0], out bool signatureIsRequiredAdded))
                    {
                        signatureRequired.Name = readableSignaturesTitle;

                        if (signatureIsRequiredAdded)
                        {
                            signatureRequired.AddedValues[0] = singleSignatureRequiredTranslatedValue;
                        }
                        else
                        {
                            signatureRequired.AddedValues[0] = noSignatureRequiredTranslatedValue;
                        }
                    }

                    if (signatureRequired.RemovedValues != null && signatureRequired.RemovedValues.Count > 0 && bool.TryParse(signatureRequired.RemovedValues[0], out bool signatureIsRequiredRemoved))
                    {
                        signatureRequired.Name = readableSignaturesTitle;

                        if (signatureIsRequiredRemoved)
                        {
                            signatureRequired.RemovedValues[0] = singleSignatureRequiredTranslatedValue;
                        }
                        else
                        {
                            signatureRequired.RemovedValues[0] = noSignatureRequiredTranslatedValue;
                        }
                    }
                }
            }

            return changes;
        }

        public List<AuditingLogChange> TranslateDeepLinks(List<AuditingLogChange> changes)
        {
            if (changes == null || changes.Count == 0) return changes;

            AuditingLogChange deepLinkIdChange = changes.Find(x => x.Name.Equals("deeplink_id"));
            AuditingLogChange deepLinkToChange = changes.Find(x => x.Name.Equals("deeplink_to"));
            AuditingLogChange deepLinkConfigurationChange = changes.Find(x => x.Name.Equals("deeplink_configuration"));

            AuditingPropertyTranslation checklistTranslation = PropertiesTranslations.Find(x => x.PropertyName.Equals("connected_checklists"));
            AuditingPropertyTranslation auditTranslation = PropertiesTranslations.Find(x => x.PropertyName.Equals("connected_audits"));

            AuditingLogChange deepLinksChanges = new()
            {
                Name = "Connected checklist or audit"
            };

            if (deepLinkConfigurationChange != null)
            {
                //if deepLinkConfigurationChange exists, this deep link was added after mandatory checklists and audits was developed
                DeepLinkConfiguration addedConfig = null;
                DeepLinkConfiguration removedConfig = null;

                if (deepLinkConfigurationChange.AddedValues != null && deepLinkConfigurationChange.AddedValues.Count > 0 && !deepLinkConfigurationChange.AddedValues[0].IsNullOrEmpty())
                {
                    addedConfig = deepLinkConfigurationChange.AddedValues[0].ToObjectFromJson<DeepLinkConfiguration>();
                }

                if (deepLinkConfigurationChange.RemovedValues != null && deepLinkConfigurationChange.RemovedValues.Count > 0 && !deepLinkConfigurationChange.RemovedValues[0].IsNullOrEmpty())
                {
                    removedConfig = deepLinkConfigurationChange.RemovedValues[0].ToObjectFromJson<DeepLinkConfiguration>();
                }

                if (addedConfig != null)
                {
                    if(addedConfig.DeepLinks != null)
                    {
                        foreach (DeepLink deepLink in addedConfig.DeepLinks)
                        {
                            string type = "";
                            string name = "";

                            if (deepLink.DeepLinkTo.Equals("checklist"))
                            {
                                type = deepLink.IsRequired ? "Mandatory checklist" : "Checklist";
                                name = checklistTranslation.TranslationsById.GetValueOrDefault(deepLink.DeepLinkId) ?? deepLink.DeepLinkId.ToString();

                            }
                            if (deepLink.DeepLinkTo.Equals("audit"))
                            {
                                type = deepLink.IsRequired ? "Mandatory audit" : "Audit";
                                name = auditTranslation.TranslationsById.GetValueOrDefault(deepLink.DeepLinkId) ?? deepLink.DeepLinkId.ToString();
                            }

                            deepLinksChanges.AddedValues.Add(type + ": " + name);
                        }
                    }
                   
                }
                if (removedConfig != null)
                {
                    if(removedConfig.DeepLinks != null)
                    {
                        foreach (DeepLink deepLink in removedConfig.DeepLinks)
                        {
                            string type = "";
                            string name = "";

                            if (deepLink.DeepLinkTo.Equals("checklist"))
                            {
                                type = deepLink.IsRequired ? "Mandatory checklist" : "Checklist";
                                name = checklistTranslation.TranslationsById.GetValueOrDefault(deepLink.DeepLinkId) ?? deepLink.DeepLinkId.ToString();

                            }
                            if (deepLink.DeepLinkTo.Equals("audit"))
                            {
                                type = deepLink.IsRequired ? "Mandatory audit" : "Audit";
                                name = auditTranslation.TranslationsById.GetValueOrDefault(deepLink.DeepLinkId) ?? deepLink.DeepLinkId.ToString();
                            }

                            deepLinksChanges.RemovedValues.Add(type + ": " + name);
                        }
                    }
                   
                }

                changes.Add(deepLinksChanges);

                if(deepLinkIdChange != null) changes.Remove(deepLinkIdChange);
                if(deepLinkToChange != null) changes.Remove(deepLinkToChange);
                if(deepLinkConfigurationChange != null) changes.Remove(deepLinkConfigurationChange);
                var checklistChange = changes.Find(x => x.Name.Equals("connected_checklists"));
                if (checklistChange != null)
                    changes.Remove(checklistChange);
                var auditChange = changes.Find(x => x.Name.Equals("connected_audits"));
                if (auditChange != null)
                    changes.Remove(auditChange);
            }
            else
            {
                //deepLinkConfigurationChange is null
                //that means the deep link was added before the mandatory checklists and audits feature was developed
                //only changes in the DeeplinkId and DeeplinkTo fields were made at that time
                //this block handles this legacy situation
                string type = "";
                string name = "";

                if (deepLinkToChange != null)
                {
                    if(deepLinkToChange.AddedValues != null && deepLinkToChange.AddedValues.Count > 0)
                    {
                        if (deepLinkToChange.AddedValues[0].Equals("checklist"))
                        {
                            type = "Checklist";
                            if (int.TryParse(deepLinkIdChange.AddedValues[0], out int id))
                                name = checklistTranslation.TranslationsById.GetValueOrDefault(id) ?? id.ToString();
                            deepLinksChanges.AddedValues.Add(type + ": " + name);
                        }
                        else if (deepLinkToChange.AddedValues[0].Equals("audit"))
                        {
                            type = "Audit";
                            if (int.TryParse(deepLinkIdChange.AddedValues[0], out int id))
                                name = auditTranslation.TranslationsById.GetValueOrDefault(id) ?? id.ToString();
                            deepLinksChanges.AddedValues.Add(type + ": " + name);
                        }
                    }
                    
                    if(deepLinkToChange.RemovedValues != null && deepLinkToChange.RemovedValues.Count > 0)
                    {
                        if (deepLinkToChange.RemovedValues[0].Equals("checklist"))
                        {
                            type = "Checklist";
                            if (int.TryParse(deepLinkIdChange.RemovedValues[0], out int id))
                                name = checklistTranslation.TranslationsById.GetValueOrDefault(id) ?? id.ToString();
                            deepLinksChanges.RemovedValues.Add(type + ": " + name);
                        }
                        else if (deepLinkToChange.RemovedValues[0].Equals("audit"))
                        {
                            type = "Audit";
                            if (int.TryParse(deepLinkIdChange.RemovedValues[0], out int id))
                                name = auditTranslation.TranslationsById.GetValueOrDefault(id) ?? id.ToString();
                            deepLinksChanges.RemovedValues.Add(type + ": " + name);
                        }
                    }
                   
                }
            }

            return changes;
        }

        public List<AuditingLogChange> PrepDeepLinkForTranslation(List<AuditingLogChange> changes)
        {
            AuditingLogChange deepLinkIdChange = changes.Find(x => x.Name.Equals("deeplink_id"));
            AuditingLogChange deepLinkToChange = changes.Find(x => x.Name.Equals("deeplink_to"));
            AuditingLogChange deepLinkConfigurationChange = changes.Find(x => x.Name.Equals("deeplink_configuration"));

            AuditingLogChange linkedChecklistChanges = new()
            {
                Name = "connected_checklists"
            };
            AuditingLogChange linkedAuditChanges = new()
            {
                Name = "connected_audits"
            };

            if (deepLinkConfigurationChange != null)
            {
                DeepLinkConfiguration addedConfig = null;
                DeepLinkConfiguration removedConfig = null;

                if (deepLinkConfigurationChange.AddedValues.Count > 0 && !deepLinkConfigurationChange.AddedValues[0].IsNullOrEmpty())
                {
                    addedConfig = deepLinkConfigurationChange.AddedValues[0].ToObjectFromJson<DeepLinkConfiguration>();
                }

                if (deepLinkConfigurationChange.RemovedValues.Count > 0 && !deepLinkConfigurationChange.RemovedValues[0].IsNullOrEmpty())
                {
                    removedConfig = deepLinkConfigurationChange.RemovedValues[0].ToObjectFromJson<DeepLinkConfiguration>();
                }

                if (addedConfig != null)
                {
                    foreach(DeepLink deepLink in addedConfig.DeepLinks)
                    {
                        if (deepLink.DeepLinkTo.Equals("checklist"))
                        {
                            linkedChecklistChanges.AddedValues.Add(deepLink.DeepLinkId.ToString());
                        } 
                        else if (deepLink.DeepLinkTo.Equals("audit"))
                        {
                            linkedAuditChanges.AddedValues.Add(deepLink.DeepLinkId.ToString());
                        }
                    }
                }

                if (removedConfig != null)
                {
                    foreach (DeepLink deepLink in removedConfig.DeepLinks)
                    {
                        if (deepLink.DeepLinkTo.Equals("checklist"))
                        {
                            linkedChecklistChanges.RemovedValues.Add(deepLink.DeepLinkId.ToString());
                        }
                        else if (deepLink.DeepLinkTo.Equals("audit"))
                        {
                            linkedAuditChanges.RemovedValues.Add(deepLink.DeepLinkId.ToString());
                        }
                    }
                }

                if (linkedChecklistChanges.AddedValues.Count > 0 || linkedChecklistChanges.RemovedValues.Count > 0) changes.Add(linkedChecklistChanges);
                if (linkedAuditChanges.AddedValues.Count > 0 || linkedAuditChanges.RemovedValues.Count > 0) changes.Add(linkedAuditChanges);

                //changes.Remove(deepLinkIdChange);
                //changes.Remove(deepLinkToChange);
                //changes.Remove(deepLinkConfigurationChange);
            }
            else
            {
                if (deepLinkIdChange != null && deepLinkToChange != null)
                {
                    if (deepLinkToChange.AddedValues.Count > 0)
                    {
                        if (deepLinkToChange.AddedValues[0].Equals("checklist"))
                            linkedChecklistChanges.AddedValues.Add(deepLinkIdChange.AddedValues[0]);
                        else if (deepLinkToChange.AddedValues[0].Equals("audit"))
                            linkedAuditChanges.AddedValues.Add(deepLinkIdChange.AddedValues[0]);
                    }

                    if (deepLinkToChange.RemovedValues.Count > 0)
                    {
                        if (deepLinkToChange.RemovedValues[0].Equals("checklist"))
                            linkedChecklistChanges.RemovedValues.Add(deepLinkIdChange.RemovedValues[0]);
                        else if (deepLinkToChange.RemovedValues[0].Equals("audit"))
                            linkedAuditChanges.RemovedValues.Add(deepLinkIdChange.RemovedValues[0]);
                    }
                }
            }

            return changes;
        }
    }
}
