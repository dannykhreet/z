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
    public class FlattenAuditManager : BaseFlattenDataManager<FlattenAuditManager>, IFlattenAuditManager
    {
        public FlattenAuditManager(IDatabaseAccessHelper datamanager, IConfigurationHelper configurationHelper, ILogger<FlattenAuditManager> logger) : base(logger, datamanager, configurationHelper) { }

        public async Task<AuditTemplate> RetrieveFlattenData(int templateId, string version, int companyId)
        {
            return await base.RetrieveFlattenData<AuditTemplate>(templateId, version, companyId);
        }

        public async Task<string> RetrieveLatestAvailableVersion(int templateId, int companyId)
        {
            return await base.RetrieveLatestAvailableVersion(Models.Enumerations.VersionedTemplateTypeEnum.AuditTemplate, templateId, companyId);
        }
        public async Task<AuditTemplate> RetrieveLatestFlattenData(int templateId, int companyId)
        {
            return await base.RetrieveLatestFlattenData<AuditTemplate>(templateId, companyId);
        }

        public async Task<bool> SaveFlattenData(int companyId, int userId, AuditTemplate flattenObject)
        {
            return await base.SaveFlattenData(companyId, userId, flattenObject, flattenObject.Id);
        }

        public async Task<string> RetrieveVersionForExistingObjectAsync(int objectId, int companyId)
        {
            return await base.RetrieveVersionForExistingObjectAsync(VersionedTemplateTypeEnum.AuditTemplate, objectId, companyId);
        }

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}
