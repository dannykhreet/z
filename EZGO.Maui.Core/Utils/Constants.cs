namespace EZGO.Maui.Core.Utils
{
    public class Constants
    {
        public static string MediaBaseUrl => Config.GetData(nameof(MediaBaseUrl));

        public static string VideoBaseUrl => Config.GetData(nameof(VideoBaseUrl));

        public static readonly string ApiBaseUrl = Config.GetData(nameof(ApiBaseUrl));

        public static readonly string EnvironmentIdentifier = Config.GetData(nameof(EnvironmentIdentifier));

        public static readonly string PasswordValidationScheme = Config.GetData(nameof(PasswordValidationScheme));
        public const string AreaClicked = "AreaClicked";

        public const string SignedOff = "SignedOff";

        public const string TokenExpired = "TokenExpired";

        public const string LogOff = "LogOff";

        public const string CompanyLogo = "ezf_logo.png";

        public const string SessionDataDirectory = "sessiondata";
        public const string PersistentDataDirectory = "persistentData";

        public const string PlaceholderImage = "placeholder.png";

        public const string ThumbnailFilenameFormat = "thumbnail{0}.png";

        public const string PictureProofFilenameFormat = "pictureproof{0}.png";

        public const string ApiDateTimeFormat = "dd-MM-yyyy HH:mm:ss";

        public const string UpdateCheckDateTimeFormat = "MM-dd-yyyy HH:mm:ss";

        public const string PdfNameDateTimeFormat = "yyyyMMdd_HHmm";

        public const string NoProfilePicture = "emptyprofile";

        public const string SignImage = "sign.jpeg";

        public const string NoProfilePicture2 = "profile.png";


        public const string SignaturesDirectory = "signatures";

        public const string ThumbnailsDirectory = "thumbnails";

        public const string PictureProofsDirectory = "pictureproofs";

        public const string VideoCacheDirectory = "videocache";


        public const string RecalculateAmountsMessage = "RecalculateAmounts";

        public const string RemoteChanges = "RemoteChanges";

        public const string SignTemplateMessage = "SignTemplate";

        public const string StageSigned = "StageSigned";

        public const string SaveSignatureMessage = "SaveSignature";

        public const string ResetSignatureMessage = "ResetSignature";

        public const string SaveSignature2Message = "SaveSignature2";

        public const string ResetSignature2Message = "ResetSignature2";

        public const string LinkedChecklistSigned = "LinkedChecklistSigned";

        public const string MessageCenterMessage = "MessageCenter";

        public const string MessageCenterCloseMessage = "MessageCenterClose";


        public const string ScorePopupMessage = "ScorePopup";

        public const string HideScorePopupMessage = "HideScorePopup";

        public const string HideDeletePopup = "HideDeletePopup";

        public const string ActionsChanged = "ActionsChanged";

        public const string AuditTemplateChanged = "AuditTemplateChanged";

        public const string ChecklistTemplateChanged = "ChecklistTemplateChanged";

        public const string WorkInstructionsTemplateChanged = "WorkInstructionsTemplateChanged";

        public const string WorkInstructionsTemplateNotificationConfirmed = "WorkInstructionsTemplateNotificationConfirmed";

        public const string ChecklistAdded = "ChecklistAdded";

        public const string ErrorSendingChecklist = "ErrorSendingChecklist";

        public const string ChecklistDeleted = "ChecklistDeleted";

        public const string ActionChanged = "ActionChanged";

        public const string MyActionsChanged = "MyActionsChanged";

        public const string QuickTimer = "QuickTimer";

        public const string ChatChanged = "ChatChanged";

        public const string AssessmentChanged = "AssessmentChanged";

        public const string AssessmentAdded = "AssessmentAdded";

        public const string AssessmentSigned = "AssessmentSigned";

        public const string AssessmentChangedScore = "AssessmentChangedScore";

        public const string AssessmentTemplateChanged = "AssessmentTemplateChanged";

        public const string ResetParticipantSwipe = "ResetParticipantSwipe";

        public const string AllParticipantsAssessmentFinished = "AllParticipantsAssessmentFinished";

        public const string RecalculateAssessmentScore = "RecalculateAssessmentScore";

        public const string CommentsLoaded = "CommentsLoaded";

        public const string ReportAreaChanged = "ReportAreaChanged";

        public const string AssessmentAreaChanged = "AssessmentAreaChanged";

        public const string ReportPeriodChanged = "ReportPeriodChanged";

        public const string ReportAuditIdChanged = "ReportAuditIdChanged";

        public const string ReloadUserDataMessage = "ReloadUserData";

        public const string UpdateSlideIndex = "UpdateSlideIndex";

        public const string PictureProofChanged = "PictureProofChanged";

        public const string TasksChanged = "TasksChanged";

        public const string TaskCommentAdded = "TaskCommentAdded";
        public const string TaskCommentChanged = "TaskCommentChanged";

        public const string TaskTemplatesChanged = "TaskTemplatesChanged";

        public const string TaskTemplateCommentAdded = "TaskTemplateCommentAdded";
        // Local error messages

        public const string NotFoundInCacheError = "Not found in cache";

        public const string WrongAWSS3MediaUrl = "Wrong AWS S3 media url";

        public const string UserHasChanged = "User has changed";
        public const string ValueChanged = "Value has changed";
        public const string NoUnsavedChanges = "AllChangesAreSaved";
        public const string FieldsStatusChanged = "Field status changed";

        public const string MandatoryItemFinished = "MandatoryItemFinished";

        #region HTTP Headers

        public const string EzgoCompanyIdHttpHeader = "EZ-CID";
        public const string EzgoUserAgentHttpHeader = "User-Agent";
        public const string EzgoDeviceInformationHttpHeader = "DeviceInformation";
        public const string EzgoDeviceIdHttpHeader = "DeviceIdentifier";
        public const string EzgoTimeZoneHttpHeader = "timezone";
        public const string EzgoLanguageHttpHeader = "Language";

        #endregion

        public const int CompletedTasksCalendarMaxDateRange = 30;

        public const string TaskChecklistActionsUrl = "taskschecklistactions?limit=0";
        public const string TaskAuditActionsUrl = "tasksauditactions?limit=0";
        public const string TasksActionsUrl = "tasksactions?limit=0";

        public const int FeedTitleMaxLength = 250;

        public static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(2);
    }
}
