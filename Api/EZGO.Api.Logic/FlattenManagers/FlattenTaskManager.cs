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
    public class FlattenTaskManager : BaseFlattenDataManager<FlattenTaskManager>, IFlattenTaskManager
    {
        public FlattenTaskManager(IDatabaseAccessHelper datamanager, IConfigurationHelper configurationHelper, ILogger<FlattenTaskManager> logger) : base(logger, datamanager, configurationHelper) { }

        public async Task<TaskTemplate> RetrieveFlattenData(int templateId, string version, int companyId)
        {
            return await base.RetrieveFlattenData<TaskTemplate>(templateId, version, companyId);
        }

        public async Task<string> RetrieveLatestAvailableVersion(int templateId, int companyId)
        {
            return await base.RetrieveLatestAvailableVersion(VersionedTemplateTypeEnum.TaskTemplate, templateId, companyId);
        }

        public async Task<TaskTemplate> RetrieveLatestFlattenData(int templateId, int companyId)
        {
            return await base.RetrieveLatestFlattenData<TaskTemplate>(templateId, companyId);
        }

        public async Task<bool> SaveFlattenData(int companyId, int userId, TaskTemplate flattenObject)
        {
            return await base.SaveFlattenData(companyId, userId, flattenObject, flattenObject.Id);
        }

        public async Task<string> RetrieveVersionForExistingObjectAsync(int objectId, int companyId)
        {
            return await base.RetrieveVersionForExistingObjectAsync(VersionedTemplateTypeEnum.TaskTemplate, objectId, companyId);
        }

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}
