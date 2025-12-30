using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.WorkInstructions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.FlattenManagers
{
    public class FlattenWorkInstructionManager : BaseFlattenDataManager<FlattenWorkInstructionManager>, IFlattenWorkInstructionManager
    {
        public FlattenWorkInstructionManager(IDatabaseAccessHelper datamanager, IConfigurationHelper configurationHelper, ILogger<FlattenWorkInstructionManager> logger) : base(logger, datamanager, configurationHelper) { }

        public async Task<WorkInstructionTemplate> RetrieveFlattenData(int templateId, string version, int companyId)
        {
            return await base.RetrieveFlattenData<WorkInstructionTemplate>(templateId, version, companyId);
        }

        public async Task<string> RetrieveLatestAvailableVersion(int templateId, int companyId)
        {
            return await base.RetrieveLatestAvailableVersion(Models.Enumerations.VersionedTemplateTypeEnum.WorkInstructionTemplate, templateId, companyId);
        }

        public async Task<WorkInstructionTemplate> RetrieveLatestFlattenData(int templateId, int companyId)
        {
            return await base.RetrieveLatestFlattenData<WorkInstructionTemplate>(templateId, companyId);
        }

        public async Task<bool> SaveFlattenData(int companyId, int userId, WorkInstructionTemplate flattenObject)
        {
            return await base.SaveFlattenData(companyId, userId, flattenObject, flattenObject.Id);
        }

        public async Task<string> RetrieveVersionForExistingObjectAsync(int objectId, int companyId)
        {
            return await base.RetrieveVersionForExistingObjectAsync(VersionedTemplateTypeEnum.WorkInstructionTemplate, objectId, companyId);
        }

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}
