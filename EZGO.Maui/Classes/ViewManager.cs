using System;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.ViewModels;
using EZGO.Maui.Core.ViewModels.AllTasks;
using EZGO.Maui.Core.ViewModels.Assessments;
using EZGO.Maui.Core.ViewModels.Audits;
using EZGO.Maui.Core.ViewModels.Bookmarks;
using EZGO.Maui.Core.ViewModels.Checklists;
using EZGO.Maui.Core.ViewModels.Feed;
using EZGO.Maui.Core.ViewModels.Reports;
using EZGO.Maui.Core.ViewModels.Shared;
using EZGO.Maui.Core.ViewModels.Tasks;
using EZGO.Maui.Views;
using EZGO.Maui.Views.Actions;
using EZGO.Maui.Views.Areas;
using EZGO.Maui.Views.Assessments;
using EZGO.Maui.Views.Audits;
using EZGO.Maui.Views.Bookmarks;
using EZGO.Maui.Views.Checklists;
using EZGO.Maui.Views.Feed;
using EZGO.Maui.Views.Home;
using EZGO.Maui.Views.Instructions;
using EZGO.Maui.Views.Login;
using EZGO.Maui.Views.Reports;
using EZGO.Maui.Views.Shared;
using EZGO.Maui.Views.Step;
using EZGO.Maui.Views.Tasks;
using EZGO.Maui.Views.Tasks.Comments;
using EZGO.Maui.Views.User;

namespace EZGO.Maui.Classes
{
    public class ViewManager
    {
        public static void RegisterAllViews()
        {
            ViewFactory.UnregisterAll();

            RegisterTabletViews();

        }

        private static void RegisterTabletViews()
        {

            ViewFactory.RegisterView<LoginPage, LoginViewModel>();
            ViewFactory.RegisterView<WorkAreaPage, WorkAreaViewModel>();
            ViewFactory.RegisterView<HomePage, HomeViewModel>();
            ViewFactory.RegisterView<PdfViewerPage, PdfViewerViewModel>();
            ViewFactory.RegisterView<VideoPlayerPage, VideoPlayerViewModel>();

            //// Profile
            ViewFactory.RegisterView<ProfilePage, ProfileViewModel>();
            ViewFactory.RegisterView<ChangePassPage, ChangePassViewModel>();

            //// Checklist
            ViewFactory.RegisterView<ChecklistTemplatesPage, ChecklistTemplatesViewModel>();
            ViewFactory.RegisterView<TaskTemplatesPage, TaskTemplatesViewModel>();
            ViewFactory.RegisterView<CompletedChecklistsPage, CompletedChecklistsViewModel>();
            ViewFactory.RegisterView<ChecklistSlidePage, ChecklistSlideViewModel>();
            ViewFactory.RegisterView<ChecklistSignPage, ChecklistSignViewModel>();
            ViewFactory.RegisterView<ChecklistPdfPage, ChecklistPdfViewModel>();
            ViewFactory.RegisterView<IncompleteChecklistsPage, IncompleteChecklistsViewModel>();

            //// Audit
            ViewFactory.RegisterView<AuditPage, AuditViewModel>();
            ViewFactory.RegisterView<AuditSlidePage, AuditSlideViewModel>();
            ViewFactory.RegisterView<CompletedAuditPage, CompletedAuditViewModel>();
            ViewFactory.RegisterView<AuditTaskTemplatesPage, AuditTaskTemplatesViewModel>();
            ViewFactory.RegisterView<AuditSignPage, AuditSignViewModel>();

            //// Actions
            ViewFactory.RegisterView<ActionPage, ActionViewModel>();
            ViewFactory.RegisterView<ActionOpenActionsPage, ActionOpenActionsViewModel>();
            ViewFactory.RegisterView<ActionConversationPage, ActionConversationViewModel>();
            ViewFactory.RegisterView<ActionNewPage, ActionNewViewModel>();
            ViewFactory.RegisterView<ActionDetailPage, ActionDetailViewModel>();
            ViewFactory.RegisterView<ActionTaskTemplateDetailPage, ActionTaskTemplateDetailViewModel>();
            ViewFactory.RegisterView<ActionTaskTemplateFullDetailPage, ActionTaskTemplateFullDetailViewModel>();
            ViewFactory.RegisterView<ActionReportActionsPage, ActionReportActionsViewModel>();

            //// Tasks
            ViewFactory.RegisterView<TaskPage, TaskViewModel>();
            ViewFactory.RegisterView<AllTasksPage, AllTasksViewModel>();
            ViewFactory.RegisterView<AllTasksSlidePage, AllTasksSlideViewModel>();
            //ViewFactory.RegisterView<EditTaskPage, EditTaskViewModel>();
            //ViewFactory.RegisterView<EditTaskRecurrencePage, EditTaskRecurrenceViewModel>();
            ViewFactory.RegisterView<TaskListPage, TaskListViewModel>();
            ViewFactory.RegisterView<CompletedTaskPage, CompletedTaskViewModel>();
            //ViewFactory.RegisterView<EditTaskInstructionsPage, EditTaskInstructionsViewModel>();
            ViewFactory.RegisterView<TaskInfoPage, TaskInfoViewModel>();
            ViewFactory.RegisterView<TaskSlidePage, TaskSlideViewModel>();
            ViewFactory.RegisterView<TaskSlideDetailPage, TaskSlideDetailViewModel>();
            ViewFactory.RegisterView<TaskCommentEditPage, TaskCommentEditViewModel>();

            //// Report
            ViewFactory.RegisterView<ReportPage, ReportViewModel>();
            ViewFactory.RegisterView<ChecklistReportPage, ChecklistReportViewModel>();
            ViewFactory.RegisterView<AuditReportPage, AuditReportViewModel>();
            ViewFactory.RegisterView<ActionReportPage, ActionReportViewModel>();
            ViewFactory.RegisterView<TaskReportPage, TaskReportViewModel>();
            ViewFactory.RegisterView<ReportFilterPage, ReportFilterViewModel>();

            //// Step
            ViewFactory.RegisterView<StepsPage, StepsViewModel>();

            //// Instructions
            ViewFactory.RegisterView<InstructionsPage, InstructionsViewModel>();
            ViewFactory.RegisterView<InstructionsItemsPage, InstructionsItemsViewModel>();
            ViewFactory.RegisterView<InstructionsSlidePage, InstructionsSlideViewModel>();

            //// Assessments            
            ViewFactory.RegisterView<AssessmentsTemplatesPage, AssessmentsTemplatesViewModel>();
            ViewFactory.RegisterView<AssessmentsPage, AssessmentsViewModel>();
            ViewFactory.RegisterView<AssessmentsInstructionItemsPage, AssessmentInstructionItemsViewModel>();
            ViewFactory.RegisterView<CompletedAssessmentsPage, CompletedAssessmentsViewModel>();
            ViewFactory.RegisterView<AssessmentSignPage, AssessmentSignViewModel>();
            ViewFactory.RegisterView<AssessmentsSlidePage, AssessmentsSlideViewModel>();

            //// Shared
            ViewFactory.RegisterView<ItemsDetailPage, ItemsDetailViewModel>();
            ViewFactory.RegisterView<PictureProofPage, PictureProofViewModel>();

            ///// Startup Page
            ViewFactory.RegisterView<StartupPage, StartupViewModel>();
            ViewFactory.RegisterView<StageSignPage, StageSignViewModel>();

            //// Bookmarks Page
            ViewFactory.RegisterView<BookmarkPage, BookmarkViewModel>();

            //// Feed Page
            ViewFactory.RegisterView<FeedPage, FeedViewModel>();
        }
    }
}

