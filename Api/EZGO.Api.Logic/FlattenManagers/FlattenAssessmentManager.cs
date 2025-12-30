using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Skills;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.FlattenManagers
{
    public class FlattenAssessmentManager : BaseFlattenDataManager<FlattenAssessmentManager>, IFlattenAssessmentManager
    {
        public FlattenAssessmentManager(IDatabaseAccessHelper datamanager, IConfigurationHelper configurationHelper, ILogger<FlattenAssessmentManager> logger) : base(logger, datamanager, configurationHelper) { }

        public async Task<AssessmentTemplate> RetrieveFlattenData(int templateId, string version, int companyId)
        {
            return await base.RetrieveFlattenData<AssessmentTemplate>(templateId, version, companyId);
        }

        public async Task<string> RetrieveLatestAvailableVersion(int templateId, int companyId)
        {
            return await base.RetrieveLatestAvailableVersion(Models.Enumerations.VersionedTemplateTypeEnum.AssessmentTemplate, templateId, companyId);
        }

        public async Task<AssessmentTemplate> RetrieveLatestFlattenData(int templateId, int companyId)
        {
            return await base.RetrieveLatestFlattenData<AssessmentTemplate>(templateId, companyId);
        }

        public async Task<bool> SaveFlattenData(int companyId, int userId, AssessmentTemplate flattenObject)
        {
            return await base.SaveFlattenData(companyId, userId, flattenObject, flattenObject.Id);
        }

        public async Task<string> RetrieveVersionForExistingObjectAsync(int objectId, int companyId)
        {
            return await base.RetrieveVersionForExistingObjectAsync(VersionedTemplateTypeEnum.AssessmentTemplate, objectId, companyId);
        }

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}
