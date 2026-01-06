using EZGO.Api.Interfaces;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Raw;
//using EZGO.Api.Interfaces.Raw;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Logic.Generation;
using EZGO.Api.Logic.Managers;
using EZGO.Api.Logic.Raw;
using EZGO.Api.Logic.Exporting;
//using EZGO.Api.Logic.Reporting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using EZGO.Api.Logic.FlattenManagers;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Models;
using EZGO.Api.Logic.Base;

namespace EZGO.Api.Logic.Helpers
{
    /// <summary>
    /// StartupHelperExtension; Helper created for managing startup services (project startup).
    /// </summary>
    public static class StartupHelperExtension
    {
        /// <summary>
        /// Add logic services to service collection.
        /// This extension can be executed from startup and will initiate all DI managers that are going to be used.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        public static void AddLogicServices(this IServiceCollection services)
        {
            //services.AddScoped<IAutomatedExportingManager, AutomatedExportingManager>();
            //services.AddScoped<IExportingManager, ExportingManager>();
            services.AddScoped<IDataCheckManager, DataCheckManager>();
            services.AddScoped<IActionManager, ActionManager>();
            services.AddScoped<IAreaManager, AreaManager>();
            services.AddScoped<IAreaBasicManager, AreaBasicManager>();
            services.AddScoped<IAuditManager, AuditManager>();
            services.AddScoped<IChecklistManager, ChecklistManager>();
            services.AddScoped<ICompanyManager, CompanyManager>();
            services.AddScoped<IGeneralManager, GeneralManager>();
            services.AddScoped<IShiftManager, ShiftManager>();
            services.AddScoped<ITaskManager, TaskManager>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IOfflineManager, OfflineManager>();
            services.AddScoped<IToolsManager, ToolsManager>();
            services.AddScoped<IStatisticsManager, StatisticsManager>();
            services.AddScoped<IReportManager, ReportManager>();
            services.AddScoped<IUserAccessManager, UserAccessManager>();
            services.AddScoped<ICommentManager, CommentManager>();
            services.AddScoped<IPropertyValueManager, PropertyValueManager>();
            services.AddScoped<IFeedManager, FeedManager>();
            services.AddScoped<IMarketPlaceManager, MarketPlaceManager>();
            services.AddScoped<IDashboardsManager, DashboardsManager>();
            services.AddScoped<IVersionReleaseManager, VersionReleaseManager>();
            services.AddScoped<IDataMigrationManager, DataMigrationManager>();
            services.AddScoped<IMatrixManager, MatrixManager>();
            services.AddScoped<IAssessmentManager, AssessmentManager>();
            services.AddScoped<IWorkInstructionManager, WorkInstructionManager>();
            services.AddScoped<IUserStandingManager, UserStandingManager>();
            services.AddScoped<ISearchManager, SearchManager>();
            services.AddScoped<IBookmarkManager, BookmarkManager>();
            services.AddScoped<ITaskPlanningManager, TaskPlanningManager>();
            services.AddScoped<ITagManager, TagManager>();
            services.AddScoped<IAuthenticationSettingsManager, AuthenticationSettingManager>();
            services.AddScoped<IVersionManager, VersionManager>();
            services.AddScoped<ISapPmManager, SapPmManager>();
            services.AddScoped<ISapPmProcessingManager, SapPmProcessingManager>();
            //flatten managers
            services.AddScoped<IFlattenAutomatedManager, FlattenAutomatedManager>();
            services.AddScoped<IFlattenChecklistManager, FlattenChecklistManager>();
            services.AddScoped<IFlattenAuditManager, FlattenAuditManager>();
            services.AddScoped<IFlattenWorkInstructionManager, FlattenWorkInstructionManager>();
            services.AddScoped<IFlattenAssessmentManager, FlattenAssessmentManager>();
            services.AddScoped<IFlattenTaskManager, FlattenTaskManager>();
        }
    }
}
