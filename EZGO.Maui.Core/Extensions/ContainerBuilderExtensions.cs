#region References

using System;
using Autofac;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.MenuFeatures;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Api;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Assessments;
using EZGO.Maui.Core.Interfaces.Audits;
using EZGO.Maui.Core.Interfaces.Bookmarks;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.Checklists;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Feed;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.HealthCheck;
using EZGO.Maui.Core.Interfaces.Home;
using EZGO.Maui.Core.Interfaces.Instructions;
using EZGO.Maui.Core.Interfaces.Login;
using EZGO.Maui.Core.Interfaces.Menu;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Reports;
using EZGO.Maui.Core.Interfaces.Shifts;
using EZGO.Maui.Core.Interfaces.Tags;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Services.Actions;
using EZGO.Maui.Core.Services.Api;
using EZGO.Maui.Core.Services.ApiRequestHandler;
using EZGO.Maui.Core.Services.Areas;
using EZGO.Maui.Core.Services.Assessments;
using EZGO.Maui.Core.Services.Audits;
using EZGO.Maui.Core.Services.Bookmarks;
using EZGO.Maui.Core.Services.Checklists;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Services.Feed;
using EZGO.Maui.Core.Services.HealthCheck;
using EZGO.Maui.Core.Services.Home;
using EZGO.Maui.Core.Services.Instructions;
using EZGO.Maui.Core.Services.Login;
using EZGO.Maui.Core.Services.Menu;
using EZGO.Maui.Core.Services.Message;
using EZGO.Maui.Core.Services.Navigation;
using EZGO.Maui.Core.Services.Pdf;
using EZGO.Maui.Core.Services.Reports;
using EZGO.Maui.Core.Services.Shifts;
using EZGO.Maui.Core.Services.Tags;
using EZGO.Maui.Core.Services.Tasks;
using EZGO.Maui.Core.Services.User;
using EZGO.Maui.Core.Services.Utils;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels;
using EZGO.Maui.Core.ViewModels.AllTasks;
using EZGO.Maui.Core.ViewModels.Assessments;
using EZGO.Maui.Core.ViewModels.Audits;
using EZGO.Maui.Core.ViewModels.Bookmarks;
using EZGO.Maui.Core.ViewModels.Checklists;
using EZGO.Maui.Core.ViewModels.Feed;
using EZGO.Maui.Core.ViewModels.Menu;
using EZGO.Maui.Core.ViewModels.Reports;
using EZGO.Maui.Core.ViewModels.Shared;
using EZGO.Maui.Core.ViewModels.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

#endregion

namespace EZGO.Maui.Core.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterServices(this ContainerBuilder containerBuilder)
        {
            var fileService = DependencyService.Get<IFileService>();
            containerBuilder.RegisterInstance(fileService).As<IFileService>().SingleInstance();

            var statusBarService = DependencyService.Get<IStatusBarService>();
            containerBuilder.RegisterInstance(statusBarService).As<IStatusBarService>().SingleInstance();

            var cachingService = DependencyService.Get<ICachingService>();
            containerBuilder.RegisterInstance(cachingService).As<ICachingService>().SingleInstance();

            containerBuilder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();
            containerBuilder.RegisterType<InteretHelperWrapper>().As<IInternetHelper>().SingleInstance();
            containerBuilder.RegisterType<RoleFunctionsWrapper>().As<IRoleFunctionsWrapper>().SingleInstance();
            containerBuilder.RegisterType<LoginService>().As<ILoginService>().ExternallyOwned();
            containerBuilder.RegisterType<UserService>().As<IUserService>().ExternallyOwned();
            containerBuilder.RegisterType<ApiClient>().As<IApiClient>().ExternallyOwned();
            containerBuilder.RegisterType<WorkAreaService>().As<IWorkAreaService>().ExternallyOwned();
            containerBuilder.RegisterType<HomeService>().As<IHomeService>().ExternallyOwned();
            containerBuilder.RegisterType<MenuService>().As<IMenuService>().ExternallyOwned();
            containerBuilder.RegisterType<MenuManager>().As<IMenuManager>().ExternallyOwned().SingleInstance();
            containerBuilder.RegisterType<ChecklistsService>().As<IChecklistService>().ExternallyOwned();
            containerBuilder.RegisterType<AuditsService>().As<IAuditsService>().ExternallyOwned();
            containerBuilder.RegisterType<ActionsService>().As<IActionsService>().ExternallyOwned();
            containerBuilder.RegisterType<MessageService>().As<IMessageService>().ExternallyOwned();
            containerBuilder.RegisterType<SyncService>().As<ISyncService>().ExternallyOwned();
            containerBuilder.RegisterType<MediaHelper>().As<IMediaHelper>().ExternallyOwned();
            containerBuilder.RegisterType<ShiftService>().As<IShiftService>().ExternallyOwned();
            containerBuilder.RegisterType<TasksService>().As<ITasksService>().ExternallyOwned();
            containerBuilder.RegisterType<UpdateService>().As<IUpdateService>().ExternallyOwned();
            containerBuilder.RegisterType<SettingsService>().As<ISettingsService>().ExternallyOwned();
            containerBuilder.RegisterType<MediaService>().As<IMediaService>().ExternallyOwned();
            containerBuilder.RegisterType<SignatureService>().As<ISignatureService>().ExternallyOwned();
            containerBuilder.RegisterType<ReportService>().As<IReportService>().ExternallyOwned();
            containerBuilder.RegisterType<TaskReportService>().As<ITaskReportService>().ExternallyOwned();
            containerBuilder.RegisterType<TaskTemplatesService>().As<ITaskTemplatesSerivce>().ExternallyOwned();
            containerBuilder.RegisterType<InstructionsService>().As<IInstructionsService>().ExternallyOwned();
            containerBuilder.RegisterType<AssessmentsService>().As<IAssessmentsService>().ExternallyOwned();
            // Don't remove full namespace before IPdfService
            containerBuilder.RegisterType<PdfService>().As<Core.Interfaces.Pdf.IPdfService>().ExternallyOwned();
            containerBuilder.RegisterType<PropertyService>().As<IPropertyService>().ExternallyOwned();
            containerBuilder.RegisterType<TaskCommentService>().As<ITaskCommentService>().ExternallyOwned();
            containerBuilder.RegisterType<ApiRequestHandler>().As<IApiRequestHandler>().ExternallyOwned();
            containerBuilder.RegisterType<HealthCheckService>().As<IHealthCheckService>().ExternallyOwned();
            containerBuilder.RegisterType<ThumbnailGenerator>().As<IThumbnailGenerator>().ExternallyOwned();
            containerBuilder.RegisterType<WatermarkGenerator>().As<IWatermarkGenerator>().ExternallyOwned();
            containerBuilder.RegisterType<BookmarkService>().As<IBookmarkService>().ExternallyOwned();
            containerBuilder.RegisterType<FeedService>().As<IFeedService>().ExternallyOwned();
            containerBuilder.RegisterType<TagsService>().As<ITagsService>().ExternallyOwned();


            containerBuilder.RegisterType<MenuViewModel>().ExternallyOwned().SingleInstance();
            containerBuilder.RegisterType<HomeViewModel>().ExternallyOwned();

            /// Media
            containerBuilder.RegisterType<PdfViewerViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<VideoPlayerViewModel>().ExternallyOwned();

            /// Authentication
            containerBuilder.RegisterType<LoginViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ChangePassViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ProfileViewModel>().ExternallyOwned();

            /// Areas
            containerBuilder.RegisterType<WorkAreaViewModel>().ExternallyOwned();

            /// Checklists
            containerBuilder.RegisterType<ChecklistTemplatesViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<TaskTemplatesViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ChecklistPdfViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ChecklistSlideViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ChecklistSignViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<CompletedChecklistsViewModel>().ExternallyOwned();

            /// Audits
            containerBuilder.RegisterType<AuditViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<AuditSlideViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<AuditSignViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<AuditTaskTemplatesViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<CompletedAuditViewModel>().ExternallyOwned();

            /// Actions
            containerBuilder.RegisterType<ActionViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ActionConversationViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ActionDetailViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ActionNewViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ActionOpenActionsViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ActionReportActionsViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ActionTaskTemplateDetailViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ActionTaskTemplateFullDetailViewModel>().ExternallyOwned();

            /// Instructions
            containerBuilder.RegisterType<InstructionsViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<InstructionsItemsViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<InstructionsSlideViewModel>().ExternallyOwned();

            // Tasks
            containerBuilder.RegisterType<TaskListViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<TaskSlideViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<TaskViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<AllTasksViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<AllTasksSlideViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<CompletedTaskViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<EditTaskInstructionsViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<EditTaskRecurrenceViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<EditTaskViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<TaskCommentEditViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<TaskInfoViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<TaskSlideDetailViewModel>().ExternallyOwned();

            /// Reports
            containerBuilder.RegisterType<ReportViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ActionReportViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ActionReportActionsViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<AuditReportViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ChecklistReportViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<TaskReportViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<ReportFilterViewModel>().ExternallyOwned();

            /// Steps
            containerBuilder.RegisterType<StepsViewModel>().ExternallyOwned();

            /// Assessments
            containerBuilder.RegisterType<AssessmentsViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<AssessmentsTemplatesViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<AssessmentInstructionItemsViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<CompletedAssessmentsViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<AssessmentSignViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<AssessmentsSlideViewModel>().ExternallyOwned();

            containerBuilder.RegisterType<WorkAreaFilterControl>().As<IWorkAreaFilterControl>();

            /// Shared
            containerBuilder.RegisterType<ItemsDetailViewModel>().ExternallyOwned();
            containerBuilder.RegisterType<PictureProofViewModel>().ExternallyOwned();

            /// Startup
            containerBuilder.RegisterType<StartupViewModel>().ExternallyOwned();

            /// Bookmarks
            containerBuilder.RegisterType<BookmarkViewModel>().ExternallyOwned();

            /// Feed
            containerBuilder.RegisterType<FeedViewModel>().ExternallyOwned();


            return containerBuilder;
        }
    }
}
