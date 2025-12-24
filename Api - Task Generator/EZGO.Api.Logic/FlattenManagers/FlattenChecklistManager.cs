using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.FlattenManagers
{
    public class FlattenChecklistManager : BaseFlattenDataManager<FlattenChecklistManager>, IFlattenChecklistManager
    {
        public FlattenChecklistManager(IDatabaseAccessHelper datamanager, IConfigurationHelper configurationHelper, ILogger<FlattenChecklistManager> logger) : base(logger, datamanager, configurationHelper) { }

        public async Task<ChecklistTemplate> RetrieveFlattenData(int templateId, string version, int companyId)
        {
            return await base.RetrieveFlattenData<ChecklistTemplate>(templateId, version, companyId);
        }

        public async Task<string> RetrieveLatestAvailableVersion(int templateId, int companyId)
        {
            return await base.RetrieveLatestAvailableVersion(VersionedTemplateTypeEnum.ChecklistTemplate, templateId, companyId);
        }

        public async Task<ChecklistTemplate> RetrieveLatestFlattenData(int templateId, int companyId)
        {
            return await base.RetrieveLatestFlattenData<ChecklistTemplate>(templateId, companyId);
        }

        public async Task<bool> SaveFlattenData(int companyId, int userId, ChecklistTemplate flattenObject)
        {
            return await base.SaveFlattenData(companyId, userId, flattenObject, flattenObject.Id);
        }

        public async Task<string> RetrieveVersionForExistingObjectAsync(int objectId, int companyId)
        {
            return await base.RetrieveVersionForExistingObjectAsync(VersionedTemplateTypeEnum.ChecklistTemplate, objectId, companyId);
        }

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}
