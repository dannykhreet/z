using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Raw
{
    /// <summary>
    /// IDataCheckManager, Interface for use with the DataCheckManager.
    /// This manager can be used for checking certain tables.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface IDataCheckManager
    {
        //Task<string> GetTopTenEzActionsAsync();
        //Task<string> GetTopTenEzActionAssignedAreaAsync();
        //Task<string> GetTopTenEzActionAssignedUserAsync();
        //Task<string> GetTopTenEzActionCommentAsync();
        //Task<string> GetTopTenEzActionCommentViewedaAsync();
        //Task<string> GetTopTenEzAreaAsync();
        //Task<string> GetTopTenEzAuditAsync();
        //Task<string> GetTopTenEzAuditTaskAsync();
        //Task<string> GetTopTenEzAuditTemplateAsync();
        //Task<string> GetTopTenEzAuditTemplateTaskAsync();
        //Task<string> GetTopTenEzChecklistAsync();
        //Task<string> GetTopTenEzChecklistTaskAsync();
        //Task<string> GetTopTenEzChecklistTemplateAsync();
        //Task<string> GetTopTenEzChecklistTemplateTaskAsync();
        //Task<string> GetTopTenEzCompanyAsync();
        //Task<string> GetTopTenEzTaskAsync();
        //Task<string> GetTopTenEzTaskRecurrencyAsync();
        //Task<string> GetTopTenEzTaskRecurrencyOneTimeShiftAsync();
        //Task<string> GetTopTenEzTaskRecurrencyShiftAsync();
        //Task<string> GetTopTenEzTaskStatusRecordAsync();
        //Task<string> GetTopTenEzTaskTemplateAsync();
        //Task<string> GetTopTenEzTaskTemplateStepAsync();
        //Task<string> GetTopTenEzTaskTemplateTagAsync();
        //Task<string> GetTopTenEzTaskTemplateTagLinkAsync();
        //Task<string> GetTopTenEzUploadsRequestedS3LinkAsync();
        //Task<string> GetTopTenEzUserAsync();
        //Task<string> GetTopTenEzUserAllowedAreaAsync();
        //Task<string> GetTopTenEzUserAreaAsync();
        string GetEnvironment();

        Task<bool> GetCompanies();
    }
}
