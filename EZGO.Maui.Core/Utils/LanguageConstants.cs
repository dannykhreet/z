using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Utils
{
    public class LanguageConstants
    {
        //tasks
        public const string taskTimeLabelText = "TASK_TIME_LABEL_TEXT";
        public const string galleryScreenShiftOverdueText = "GALLERY_SCREEN_SHIFT_OVERDUE_TEXT";
        public const string taskDetailMarkedBy = "TASK_DETAIL_MARKED_BY";
        public const string taskRealizedTimeIn = "TASK_REALIZED_TIME_IN";
        public const string taskRealizedTimeTitle = "TASK_REALIZED_TIME_TITLE";
        public const string taskPageNumberText = "TASK_PAGE_NUMBER_TEXT";
        public const string taskMinutesLabelText = "TASK_MINUTES_LABEL_TEXT";
        public const string taskDetailMarkedOn = "TASK_DETAIL_MARKED_ON";
        public const string taskDetailMarked = "TASK_DETAIL_MARKED";
        public const string taskDetailDue = "TASK_DETAIL_DUE";
        public const string taskStepsPageNumberText = "TASK_STEPS_PAGE_NUMBER_TEXT";
        public const string changePhotoTitle = "EDIT_SCREEN_EDIT_PHOTO_BUTTON_TITLE";
        public const string addPhotoTitle = "TASK_CONSTRUCTOR_BASIC_VIEW_ADD_PHOTO_TEXT";
        public static string totalTaskCountText = "TOTAL_TASKS_COUNT_TEXT";
        public const string completedAllTasks = "COMPLETED_ALL_TASKS";
        public const string maxDateRangeText = "MAX_DATE_RANGE_TEXT";
        public const string untapTaskText = "UNTAP_TASK_TEXT";
        public const string seePicturesText = "SEE_PICTURES_TEXT";
        public const string cantTapText = "TASK_CANT_TAP_TEXT";
        public const string multiskipTaskComment = "MULTISKIP_TASK_COMMENT";

        //tasks screen
        public const string tasksScreenShiftFilterText = "TASKS_SCREEN_SHIFT_FILTER_TEXT";
        public const string tasksScreenTodayFilterText = "TASKS_SCREEN_TODAY_FILTER_TEXT";
        public const string tasksScreenWeekFilterText = "TASKS_SCREEN_WEEK_FILTER_TEXT";
        public const string tasksScreenShiftOverdueText = "TASKS_SCREEN_SHIFT_OVERDUE_TEXT";
        public const string tasksScreenNoFilterText = "TASKS_SCREEN_NO_FILTER_TEXT";
        public const string chooseTasksScreenTitle = "CHOOSE_TASKS_SCREEN_TITLE";

        //shift type
        public const string shiftTypeMonth = "SHIFT_TYPE_MONTH";
        public const string shiftTypeOnce = "SHIFT_TYPE_ONCE";
        public const string shiftTypeShift = "SHIFT_TYPE_SHIFT";
        public const string shiftTypeWeek = "SHIFT_TYPE_WEEK";
        public const string shiftTypeDailyInterval = "SHIFT_TYPE_DAILY_INTERVAL";
        public const string shiftTypeDynamicDailyInterval = "SHIFT_TYPE_DYNAMIC_DAILY_INTERVAL";

        //task constructor
        public const string taskConstructorBasicUserRole = "TASK_CONSTRUCTOR_BASIC_USER_ROLE";
        public const string taskConstructorManagerRole = "TASK_CONSTRUCTOR_MANAGER_ROLE";
        public const string taskConstructorShiftUserRole = "TASK_CONSTRUCTOR_SHIFT_USER_ROLE";
        public const string taskConstructorRecurrenceOnce = "TASK_CONSTRUCTOR_RECURRENCE_ONCE";
        public const string taskConstructorRecurrenceWeekly = "TASK_CONSTRUCTOR_RECURRENCE_WEEKLY";
        public const string taskConstructorRecurrenceMonthly = "TASK_CONSTRUCTOR_RECURRENCE_MONTHLY";
        public const string taskConstructorRecurrenceByShift = "TASK_CONSTRUCTOR_RECURRENCE_BY_SHIFT";
        public const string taskConstructorNoShifts = "TASK_CONSTRUCTOR_NO_SHIFTS";
        public const string taskConstructorPdfError = "TASKS_CONSTRUCTOR_PDF_ERROR";

        //download Media
        public const string downloadMediaProgress = "DOWNLOADING_MEDIA_PROGRESS";
        public const string downloadMediaFinished = "DOWNLOADING_MEDIA_FINISHED";

        //menu item
        public const string contextMenuItemPhoto = "CONTEXT_MENU_ITEM_PHOTO";
        public const string contextMenuItemPhotoGallery = "CONTEXT_MENU_ITEM_PHOTO_GALLERY";
        public const string contextMenuItemVideo = "CONTEXT_MENU_ITEM_VIDEO";
        public const string contextMenuItemVideoGallery = "CONTEXT_MENU_ITEM_VIDEO_GALLERY";
        public const string contextMenuItemRemoveMedia = "CONTEXT_MENU_ITEM_REMOVE_MEDIA";
        public const string contextMenuChooseMediaDialogTitle = "CONTEXT_MENU_CHOOSE_MEDIA_DIALOG_TITLE";
        public const string contextMenuChooseMediaDialogCancel = "CONTEXT_MENU_CHOOSE_MEDIA_DIALOG_CANCEL";
        public const string contextMenuItemPDF = "CONTEXT_MENU_ITEM_PDF";

        //security permissions
        public const string securityPermissionCamera = "SECURITY_PERMISSION_CAMERA";
        public const string securityPermissionPhotos = "SECURITY_PERMISSION_PHOTOS";
        public const string securityPermissionOk = "SECURITY_PERMISSION_OK";
        public const string securityPermissionNotSupported = "SECURITY_PERMISSION_NOT_SUPPORTED";
        public const string securityPermissionNotSupportedDecs = "SECURITY_PERMISSION_NOT_SUPPORTED_DECS";
        public const string securityPermissionStorageMessage = "SECURITY_PERMISSION_STORAGE_MESSAGE";
        public const string securityPermissionStorage = "SECURITY_PERMISSION_STORAGE";
        public const string takePhotoTitle = "GENERAL_TAKE_A_FOTO_TITLE";

        //actions links converter
        public const string actionDetailScreenActionIsLinkedToAuditItem = "ACTION_DETAIL_SCREEN_ACTION_IS_LINKED_TO_AUDIT_ITEM";
        public const string actionDetailScreenActionIsLinkedToChecklistItem = "ACTION_DETAIL_SCREEN_ACTION_IS_LINKED_TO_CHECKLIST_ITEM";
        public const string actionDetailScreenActionIsLinkedToTask = "ACTION_DETAIL_SCREEN_ACTION_IS_LINKED_TO_TASK";

        //shift change
        public const string shiftChangeOfflineNotification = "SHIFT_CHANGE_OFFLINE_NOTIFICATION";
        public const string shiftChangeStarted = "SHIFT_CHANGE_STARTED";
        public const string shiftChangeCompleted = "SHIFT_CHANGE_COMPLETED";

        //alerts
        public const string alertConfirmAction = "ALERT_CONFIRM_ACTION";
        public const string alertYesButtonTitle = "ALERT_YES_BUTTON_TITLE";
        public const string alertNoButtonTitle = "ALERT_NO_BUTTON_TITLE";

        //task constructor validation
        public const string taskConstructorValidationGreaterThan0 = "TASK_CONSTRUCTOR_VALIDATION_GREATER_THAN_0";
        public const string taskConstructorValidationNoShifts = "TASK_CONSTRUCTOR_VALIDATION_NO_SHIFTS";
        public const string taskConstructorValidationNoWeekDay = "TASK_CONSTRUCTOR_VALIDATION_NO_WEEK_DAY";
        public const string taskConstructorValidationWeek = "TASK_CONSTRUCTOR_VALIDATION_WEEK";
        public const string taskConstructorValidationNoRecurrency = "TASK_CONSTRUCTOR_VALIDATION_NO_RECURRENCY";
        public const string taskConstructorValidationDay = "TASK_CONSTRUCTOR_VALIDATION_DAY";
        public const string taskConstructorValidationDayOfWeek = "TASK_CONSTRUCTOR_VALIDATION_DAY_OF_WEEK";
        public const string taskConstructorValidationWeekDaySet = "TASK_CONSTRUCTOR_VALIDATION_WEEK_DAY_SET";
        public const string taskConstructorValidationMonth = "TASK_CONSTRUCTOR_VALIDATION_MONTH";
        public const string taskConstructorValidationMessage = "TASK_CONSTRUCTOR_VALIDATION_MESSAGE";
        public const string taskConstructorValidationError = "TASK_CONSTRUCTOR_VALIDATION_ERROR";
        public const string taskConstructorValidationClose = "TASK_CONSTRUCTOR_VALIDATION_CLOSE";
        public const string taskConstructorValidationUpdateError = "TASK_CONSTRUCTOR_VALIDATION_UPDATE_ERROR";
        public const string taskConstructorValidationEmptyStep = "TASK_CONSTRUCTOR_VALIDATION_EMPTY_STEP";
        public const string taskConstructorValidationNoWorkArea = "TASK_CONSTRUCTOR_VALIDATION_NO_WORK_AREA";
        public const string taskConstructorValidationEmpty = "TASK_CONSTRUCTOR_VALIDATION_EMPTY";
        public const string taskConstructorValidationName = "TASK_CONSTRUCTOR_VALIDATION_NAME";
        public const string taskConstructorValidationDescription = "TASK_CONSTRUCTOR_VALIDATION_DESCRIPTION";

        // task comments
        public const string commentValidationErrorTitle = "TASK_COMMENT_VALIDATION_ERROR_TITLE";
        public const string commentValidationErrorText = "TASK_COMMENT_VALIDATION_ERROR_TEXT";
        public const string commentCannotSaveErrorTitle = "TASK_COMMENT_ERROR_TITLE";
        public const string commentCannotSaveErrorText = "TASK_COMMENT_ERROR_TEXT";

        //chat
        public const string chatScreenInputPictureAttached = "CHAT_SCREEN_INPUT_PICTURE_ATTACHED";
        public const string chatScreenInputVideoAttached = "CHAT_SCREEN_INPUT_VIDEO_ATTACHED";

        //action
        public const string actionEditedTitle = "ACTION_EDITED_TITLE";
        public const string actionDetailScreenCommentsSectionTitle = "ACTION_DETAIL_SCREEN_COMMENTS_SECTION_TITLE";
        public const string actionScreenTitle = "ACTION_SCREEN_TITLE";
        public const string actionDetailScreenResourcesSectionTitle = "ACTION_DETAIL_SCREEN_RESOURCES_SECTION_TITLE";
        public const string actionMediaEdited = "ACTION_MEDIA_EDITED";
        public const string createActionScreenDueDateLabel = "CREATE_ACTION_SCREEN_DUE_DATE_LABEL";
        public const string createActionScreenCommentsLabel = "CREATE_ACTION_SCREEN_COMMENTS_LABEL";
        public const string createActionScreenActionLabel = "CREATE_ACTION_SCREEN_ACTION_LABEL";
        public const string actionSegement = "ACTION_SEGMENT";
        public const string commentSegement = "COMMENT_SEGMENT";
        public const string actionAdded = "ACTION_ADDED";
        public const string actionChanged = "ACTION_CHANGED";
        public const string ultimoSent = "ULTIMO_SENT";
        public const string ultimoNotSent = "ULTIMO_NOT_SENT";
        public const string ultimoSend = "ULTIMO_SEND";
        public const string ultimoReadySent = "ULTIMO_READY_SENT";
        public const string ultimoErrorSent = "ULTIMO_ERROR_SENDING";

        //actions
        public const string actionsScreenOpenActions = "ACTIONS_SCREEN_OPEN_ACTIONS";
        public const string sidebarTitleActions = "SIDEBAR_TITLE_ACTIONS";
        public const string actionsScreenDueDate = "ACTIONS_SCREEN_DUE_DATE";
        public const string actionsScreenIAmInvolvedIn = "ACTIONS_SCREEN_I_AM_INVOLVED_IN";
        public const string actionsScreenAssignedToMe = "ACTIONS_SCREEN_ASSIGNED_TO_ME";
        public const string actionsScreenStartedByMe = "ACTIONS_SCREEN_STARTED_BY_ME";
        public const string actionsScreenAllActions = "ACTIONS_SCREEN_ALL_ACTIONS";
        public const string actionsScreenAllAreas = "ACTIONS_SCREEN_ALL_AREAS";
        public const string actionsScreenOpenActionsFormated = "ACTIONS_SCREEN_OPEN_ACTIONS_FORMATTED";
        public const string actionsScreenActionsFormatted = "ACTIONS_SCREEN_ACTIONS_FORMATTED";
        public const string actionsScreenCommentsFormated = "ACTIONS_SCREEN_OPEN_COMMENTS_FORMATTED";
        public const string actionsOnTheSpotName = "ACTIONS_ON_THE_SPOT_NAME";
        public const string forAuditItem = "FOR_AUDIT_ITEM";
        public const string forChecklistItem = "FOR_CHECKLIST_ITEM";
        public const string forTaskItem = "FOR_TASK_ITEM";

        //online action
        public const string onlyOnlineAction = "ONLY_ONLINE_ACTION";

        //reports
        public const string noInternetConnectionReportsUnavailable = "NO_INTERNET_CONNECTION_REPORTS_UNAVAILABLE";
        public const string reportsDeviationsFilter = "REPORTS_DEVIATIONS_FILETER";
        public const string reportsSkippedFilter = "REPORTS_SKIPPED_FILETER";
        public const string reportsNotOKFilter = "REPORTS_NOT_OK_FILETER";
        public const string reportsNotDoneFilter = "REPORTS_NOT_DONE_FILETER";
        public const string reportOnArea = "REPORT_ON_AREA";
        public const string userActionsTitle = "USER_ACTIONS_TITLE";
        public const string taskActionsTitle = "TASK_ACTIONS_TITLE";
        public const string checklistActionsTitle = "CHECKLIST_ACTIONS_TITLE";
        public const string auditActionsTitle = "AUDIT_ACTIONS_TITLE";
        public const string reports12DaysPeriod = "REPOSRTS_12_DAYS_PERIOD";
        public const string reports12WeeksPeriod = "REPOSRTS_12_WEEKS_PERIOD";
        public const string reports12MonthsPeriod = "REPOSRTS_12_MONTHS_PERIOD";
        public const string reportsThisYearPeriod = "REPOSRTS_THIS_YEAR_PERIOD";

        //sync
        public const string syncStatesViewSynsingMessage = "SYNC_STATES_VIEW_SYNSING_MESSAGE";
        public const string syncStatesViewConnectionProblemMessage = "SYNC_STATES_VIEW_CONNECTION_PROBLEM_MESSAGE";
        public const string syncLocalData = "SYNC_LOCAL_DATA";
        public const string syncLocalDataFinished = "SYNC_LOCAL_DATA_FINISHED";

        //main screen messages
        public const string mainScreenLogoutMessageTitle = "MAIN_SCREEN_LOGOUT_MESSAGE_TITLE";
        public const string mainScreenLogoutMessageText = "MAIN_SCREEN_LOGOUT_MESSAGE_TEXT";
        public const string mainScreenEditProfileMessageText = "MAIN_SCREEN_EDIT_PROFILE_MESSAGE_TEXT";
        public const string mainScreenChangeAreaText = "MAIN_SCREEN_CHANGE_AREA_TEXT";
        public const string mainScreenQRScannerText = "MAIN_SCREEN_QRSCANNER_TEXT";

        //audit sign
        public const string signChecklistScreenNamePlaceholderText = "SIGN_CHECKLIST_SCREEN_NAME_PLACEHOLDER_TEXT";
        public const string signAuditScreenNoSignatureError = "SIGN_AUDIT_SCREEN_NO_SIGNATURE_ERROR";
        public const string signAuditScreenNoNamesError = "SIGN_CHECKLIST_SCREEN_NO_NAMES_ERROR";

        //home
        public const string homeScreenWelcomText = "HOME_SCREEN_WELCOM_TEXT";

        //login
        public const string loginScreenAuthorizationFalied = "LOGIN_SCREEN_AUTHORIZATION_FAILED";
        public const string samlLoginIncorrectCredentialsError = "SAML_LOGIN_INCORECT_CREDENTIALS_ERROR";

        //text messages
        public const string baseTextCancel = "BASE_TEXT_CANCEL";
        public const string baseTextTill = "BASE_TEXT_TILL";
        public const string baseTextOk = "BASE_TEXT_OK";
        public const string baseTextAndMore = "BASE_TEXT_AND_MORE";
        public const string baseTextAnd = "BASE_TEXT_AND";
        public const string baseTextWeek = "BASE_TEXT_WEEK";
        public const string baseTextShift = "BASE_TEXT_SHIFT";
        public const string baseTextCompleted = "BASE_TEXT_COMPLETED";
        public const string shiftsTotal = "SHIFTS_TOTAL";

        //general texts
        public const string generalTextFirst = "GENERAL_TEXT_FIRST";
        public const string generalTextSecond = "GENERAL_TEXT_SECOND";
        public const string generalTextThird = "GENERAL_TEXT_THIRD";
        public const string generalTextFourth = "GENERAL_TEXT_FOURTH";
        public const string generalTextDescription = "TASK_CONSTRUCTOR_BASIC_VIEW_DESCRIPTION_TITLE_TEXT";

        //profile
        public const string userProfileScreenTitle = "USER_PROFILE_SCREEN_TITLE";
        public const string editProfileEmptyCurrentPassword = "EDIT_PROFILE_EMPTY_CURRENT_PASSWORD";
        public const string editProfileInvalidPassword = "EDIT_PROFILE_INVALID_PASSWORD";
        public const string editProfileNotMatchPassword = "EDIT_PROFILE_NOT_MATCH_PASSWORD";

        // instructions
        public const string instructionPageNumberText = "INSTRUCTION_PAGE_NUMBER_TEXT";
        public const string instructionsScreenTitle = "INSTRUCTIONS_SCREEN_TITLE";
        //area
        public const string changeAreaNoInternet = "CHANGE_AREA_NO_INTERNET";

        //assessments
        public const string assessmentOnArea = "ASSESSMENT_ON_AREA";
        public const string assessmentsScreenCompletedItemsTitle = "ASSESSMENTS_SCREEN_COMPLETED_ITEMS_TITLE";
        public const string completedAssessmentsForArea = "COMPLETED_ASSESSMENTS_FOR_AREA";
        public const string completedAssessments = "COMPLETED_ASSESSMENTS";
        public const string avarageScore = "AVARAGE_SCORE";
        public const string ongoingAssessment = "ONGOING_ASSESSMENT";
        public const string completedInstructions = "ASSESSMENTS_COMPLETED_INSTRUCTIONS";
        public const string allInstructions = "ASSESSMENTS_ALL_INSTRUCTIONS";
        public const string addParticipant = "ADD_PARTICIPANTS";
        public const string participantName = "PARTICIPANT_NAME";
        public const string noInternetConnectionAssessmentsUnavailable = "NO_INTERNET_CONNECTION_ASSESSMENTS_UNAVAILABLE";
        public const string deleteParticipantMessage = "DELETE_PARTICIPANT_MESSAGE";
        public const string completedAllInstructions = "COMPLETED_ALL_INSTRUCTIONS";
        public const string assessmentParticipants = "APP_ASSESSMENTS_PARTICIPANTS";
        public const string assessmentNotAdded = "APP_ASSESSMENTS_NOT_ADDED";

        //checklists
        public const string checklistAdded = "CHECKLIST_ADDED";
        public const string checklistScreenTitle = "CHECKLISTS_SCREEN_TITLE";

        //audtis
        public const string auditAdded = "AUDIT_ADDED";
        public const string auditListScreenTitle = "AUDIT_LIST_SCREEN_TITLE";

        //picture proof
        public const string pictureProofValidationErrorText = "PICTURE_PROOF_VALIDATION_ERROR_TEXT";
        public const string untapTaskConfirmation = "UNTAP_TASK_CONFIRMATION";

        //qr scanner
        public const string qrScannerNoInternet = "QR_SCANNER_NO_INTERNET";
        public const string qrScannerNoAccess = "QR_SCANNER_NO_ACCESS";

        //tags
        public const string baseTextTags = "BASE_TEXT_TAGS";

        //ez feed
        public const string feedMaxNumberOfAttachments = "FEED_MAX_NR_ATTACHMENTS";

        //general
        public const string maxNumberOfAttachments = "MAX_NR_ATTACHMENTS";
        public const string lastCompleted = "LAST_COMPLETED";

        //stages
        public const string signStageToProceed = "SIGN_STAGE_TO_PROCEED";
        public const string stageIsLocked = "STAGE_IS_LOCKED";
    }
}
