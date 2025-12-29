namespace WebApp.Logic
{
    //NOTE DO NOT REMOVE  '//@(Model?.CmsLanguage?.... ' comments until fully added to database!!!
    public static class LanguageKeys
    {
        public static class Action
        {
            public const string ActionsLabel = @"CMS_ACTIONS_LABEL";
            public const string OverViewTitle = @"CMS_ACTION_OVERVIEW_TITLE";
            public const string SetActionResolved = @"CMS_ACTION_SET_RESOLVED";
            public const string SetActionResolvedAlert = @"CMS_ACTION_SET_RESOLVED_ALERT";
            public const string ListTitle = @"CMS_ACTION_OVERVIEW_LIST_TITLE";
            public const string AddActionButton = @"CMS_ACTION_OVERVIEW_BTN_ADD_ACTION";
            public const string Search = @"CMS_ACTION_OVERVIEW_SEARCH_TITLE";
            public const string StartDate = @"CMS_ACTION_OVERVIEW_STARTDATE_TITLE";
            public const string DueDate = @"CMS_ACTION_OVERVIEW_DUEDATE_TITLE";
            public const string Author = @"CMS_ACTION_OVERVIEW_AUTHOR_TITLE";
            public const string DetailsBackToAction = @"CMS_ACTION_NAV_BACK_TITLE";
            public const string DetailsTitle = @"CMS_ACTION_DETAILS_TITLE";
            public const string DetailsCommentTitle = @"CMS_ACTION_DETAILS_COMMENT_TITLE";
            public const string DetailsActionTitle = @"CMS_ACTION_DETAILS_ACTION_TITLE";
            public const string DetailsAuthorTitle = @"CMS_ACTION_DETAILS_AUTHOR_TITLE";
            public const string DetailsStartedTitle = @"CMS_ACTION_DETAILS_STARTED_TITLE";
            public const string DetailsModifiedTitle = @"CMS_ACTION_DETAILS_MODDATE_TITLE";
            public const string DetailsDuedateTitle = @"CMS_ACTION_DETAILS_DUEDATE_TITLE";
            public const string DetailsResourcesTitle = @"CMS_ACTION_DETAILS_RESOURCES_TITLE";
            public const string DetailsResourcesTitleAreas = @"CMS_ACTION_DETAILS_RESOURCES_SUB_TITLE_AREAS";
            public const string DetailsResourcesTitleUsers = @"CMS_ACTION_DETAILS_RESOURCES_SUB_TITLE_USERS";
            public const string DetailsMediaTitle = @"CMS_ACTION_DETAILS_MEDIA_TITLE";
            public const string DetailsCommentsTitle = @"CMS_ACTION_DETAILS_COMMENTS_TITLE";
            public const string DetailsTypeMessage = @"CMS_ACTION_DETAILS_TYPE_MESSAGE_PLACEHOLDER";
            public const string DetailsSendButton = @"CMS_ACTION_DETAILS_BTN_SEND";
            public const string LastRefreshTitle = @"CMS_ACTION_LAST_REFRESH_TITLE";
            public const string PauseRefreshTitle = @"CMS_ACTION_PAUSE_REFRESH_TITLE";
            public const string StartRefreshTitle = @"CMS_ACTION_START_REFRESH_TITLE";
            public const string ExportActionsTitle = @"CMS_ACTION_EXPORT_ACTIONS_TITLE";
            public const string ExportCommentsTitle = @"CMS_ACTION_EXPORT_COMMENTS_TITLE";
            public const string BtnHerstelFiltersTitle = @"CMS_ACTION_BTN_HERSTEL_FILTERS_TITLE";
            public const string ActionId = @"CMS_ACTION_ACTION_ID";
            // public const string DetailsActionTitle = @"CMS_ACTION_DETAILS_CONNECTED_CHECKLIST";
            // public const string DetailsActionTitle = @"CMS_ACTION_DETAILS_CONNECTED_AUDIT";

            public const string EditActionTitle = @"CMS_ACTION_EDIT_TITLE";
            public const string EditActionButton = @"CMS_ACTION_EDIT_BUTTON";
            public const string EnterTextPlaceholder = @"CMS_ACTION_ENTER_TEXT";
            public const string EnsureFutureDateValidator = @"CMS_ACTION_VAL_ENSURE_FUTURE_DATE";

            public const string WasEditedTitle = @"CMS_ACTION_WAS_EDITED";
            //"The following items of this action have been changed:"
            public const string CommentChangedTitle = @"CMS_ACTION_COMMENT_EDITED";
            //"Comment"
            public const string DescriptionChangedTitle = @"CMS_ACTION_DESCRIPTION_EDITED";
            //"Description"
            public const string DueDateChangedTitle = @"CMS_ACTION_DUEDATE_EDITED";
            //"Due date"
            public const string ResourcesChangedTitle = @"CMS_ACTION_RESOURCES_EDITED";
            //"Resources"
            public const string MediaChangedTitle = @"CMS_ACTION_MEDIA_EDITED";
            //"Media"
            public const string IsResolvedChangedTitle = @"CMS_ACTION_ISRESOLVED_EDITED";
            //"Completed"

            public const string ErrorComment = @"CMS_ACTION_ERROR_COMMENT";
            public const string OptionYes = @"CMS_ACTION_OPTION_YES";
            public const string OptionNo = @"CMS_ACITON_OPTION_NO";
            public const string ErrorMessage = @"CMS_ACTION_ERROR_MESSAGE";


        }

        public static class Announcement
        {
            public const string Title = @"CMS_ANNOUNCEMENT_TITLE";
            public const string AnnouncementsTitle = @"CMS_ANNOUNCEMENT_ANNOUNCEMENTS_TITLE";
            public const string BtnNewAnnouncement = @"CMS_ANNOUNCEMENT_BTN_NEW_ANNOUNCEMENT";
            public const string AnnouncementTitle = @"CMS_ANNOUNCEMENT_ANNOUNCEMENT_TITLE";
            public const string AnnouncementDescription = @"CMS_ANNOUNCEMENT_ANNOUNCEMENT_DESCRIPTION";
            public const string AnnouncementType = @"CMS_ANNOUNCEMENT_ANNOUNCEMENT_TYPE"; //Release announcement type
            public const string AnnouncementDate = @"CMS_ANNOUNCEMENT_ANNOUNCEMENT_DATE";
            public const string BtnCancel = @"CMS_ANNOUNCEMENT_BTN_CANCEL";
            public const string BtnSave = @"CMS_ANNOUNCEMENT_BTN_SAVE";
            public const string DescriptionTitle = @"CMS_ANNOUNCEMENT_DESCRIPTION_TITLE";
            public const string TitlePlaceholder = @"CMS_ANNOUNCEMENT_TITLE_PLACEHOLDER";
            public const string DescriptionPlaceholder = @"CMS_ANNOUNCEMENT_DESCRIPTION_PLACEHOLDER";
            public const string IdText = @"CMS_ANNOUNCEMENT_ID_TEXT";
            public const string TypeText = @"CMS_ANNOUNCEMENT_TYPE_TEXT";
            public const string RemoveIsDisabled = @"CMS_ANNOUNCEMENT_REMOVE_IS_DISABLED";
            public const string AnnouncementSaved = @"CMS_ANNOUNCEMENT_ANNOUNCEMENT_SAVED";
        }

        public static class Audit
        {
            public const string AuditTemplateLabel = @"CMS_AUDIT_TEMPLATE_LABEL";
            public const string AuditsLabel = @"CMS_AUDITS_LABEL";
            public const string OverviewTitle = @"CMS_AUDIT_OVERVIEW_TITLE";
            public const string OverviewListTitle = @"CMS_AUDIT_OVERVIEW_LIST_TITLE";
            public const string ExportTemplates = @"CMS_AUDIT_EXPORT_TEMPLATES";
            public const string TemplatesDesc = @"CMS_AUDIT_EXPORT_TEMPLATES_DESC";
            public const string ExportTitle = @"CMS_AUDIT_EXPORT_TITLE";
            public const string ExportProgress = @"CMS_AUDIT_EXPORT_PROGRESS";
            public const string ExportStatus = @"CMS_AUDIT_EXPORT_STATUS";
            public const string ExportStartDate = @"CMS_AUDIT_EXPORT_STARTDATE";
            public const string ExportEndDate = @"CMS_AUDIT_EXPORT_ENDDATE";
            public const string Search = @"CMS_AUDIT_SEARCH";
            public const string NavBackTitle = @"CMS_AUDIT_NAV_BACK_TITLE";
            public const string Title = @"CMS_AUDIT_TITLE";
            public const string AuditId = @"CMS_AUDIT_AUDIT_ID";
            public const string SelectRoleTitle = @"CMS_AUDIT_SELECT_ROLE_TITLE";
            public const string SelectSignatureTitle = @"CMS_AUDIT_SELECT_SIGNATURE_TITLE";
            public const string OptionNone = @"CMS_AUDIT_SELECT_OPTION_NONE";
            public const string OptionOneSignature = @"CMS_AUDIT_SELECT_OPTION_ONE_SIGNTURE";
            public const string OptionTwoSignature = @"CMS_AUDIT_SELECT_OPTION_TWO_SIGNATURE";
            public const string OptionBasic = @"CMS_AUDIT_SELECT_OPTION_BASIC";
            public const string OptionShiftleader = @"CMS_AUDIT_SELECT_OPTION_SHIFTLEADER";
            public const string OptionManager = @"CMS_AUDIT_SELECT_OPTION_MANAGER";
            public const string ItemsTitle = @"CMS_AUDIT_AUDIT_ITEMS_TITLE";
            public const string ItemTitle = @"CMS_AUDIT_AUDIT_ITEM_TITLE";
            public const string ItemTitlePlaceholder = @"CMS_AUDIT_ITEM_TITLE_PLACEHOLDER";
            public const string ItemDescriptionPlaceholder = @"CMS_AUDIT_ITEM_DESCRIPTION_PLACEHOLDER";
            public const string BtnAddProperty = @"CMS_AUDIT_BTN_ADD_PROPERTY";
            public const string PropertyDescTitle = @"CMS_AUDIT_PROPERTY_DESC_TITLE";
            public const string DisplayFormat = @"CMS_AUDIT_DISPLAY_FORMAT";
            public const string DialogAddProperty = @"CMS_AUDIT_DIALOG_ADD_PROPERTY";
            public const string InstructionDialogTitle = @"CMS_AUDIT_INSTRUCTION_DIALOG_TITLE";
            public const string AddInstruction = @"CMS_AUDIT_ADD_INSTRUCTION";
            public const string AddWorkInstruction = @"CMS_AUDIT_ADD_WORKINSTRUCTION";
            public const string InstructionTitle = @"CMS_AUDIT_INSTRUCTION_TITLE";
            public const string InstructionDelete = @"CMS_AUDIT_INSTRUCTION_DELETE";
            public const string InstructionPrevStep = @"CMS_AUDIT_DIALOG_PREV_STEP";
            public const string DialogClose = @"CMS_AUDIT_DIALOG_CLOSE";
            public const string DialogNextStep = @"CMS_AUDIT_DIALOG_NEXT_STEP";
            public const string DialogAddStep = @"CMS_AUDIT_DIALOG_ADD_STEP";
            public const string DialogPrevItem = @"CMS_AUDIT_DIALOG_PREVIOUS_ITEM";
            public const string DialogNextItem = @"CMS_AUDIT_DIALOG_NEXT_ITEM";
            public const string DialogDelete = @"CMS_AUDIT_DIALOG_DELETE";
            public const string AreaTitle = @"CMS_AUDIT_AREA_TITLE";
            public const string BtnPrintTitle = @"CMS_AUDIT_BTN_PRINT_TITLE";
            public const string BtnDuplicate = @"CMS_AUDIT_BTN_DUPLICATE_TITLE";
            public const string BtnDelete = @"CMS_AUDIT_BTN_DELETE_TITLE";
            public const string BtnSave = @"CMS_AUDIT_BTN_SAVE_TITLE";
            public const string InstructionAddItem = @"CMS_AUDIT_INSTRUCTION_ADD_ITEM";
            public const string InstructionAddItemLower = @"CMS_AUDIT_INSTRUCTION_ADD_ITEM_LOWER";
            public const string InstructionNextItem = @"CMS_AUDIT_INSTRUCTION_NEXT_ITEM";
            public const string ScoringTitle = @"CMS_AUDIT_SCORING_TITLE";
            public const string ScoringDisabledTitle = @"CMS_AUDIT_SCORING_DISABLED_TITLE";
            public const string ScoringThumbsTitle = @"CMS_AUDIT_SCORING_THUMBS_TITLE";
            public const string ScoringCustomTitle = @"CMS_AUDIT_SCORING_CUSTOM_TITLE";
            public const string TemplateNamePlaceholder = @"CMS_AUDIT_TEMPLATE_NAME_PLACEHOLDER";
            public const string InstructionDescPlaceholder = @"CMS_CHECKLIST_INSTRUCTION_DESC_PLACEHOLDER";
            public const string QuestionWeightTitle = @"CMS_AUDIT_QUESTION_WEIGHT_TITLE";
            public const string ItemAddPdf = @"CMS_AUDIT_ITEM_ADD_PDF";
            public const string AuditScoreTitle = @"CMS_AUDIT_AUDITSCORE_TITLE";
            public const string AddOpenField = @"CMS_AUDIT_ADD_OPEN_FIELD";
            public const string BtnClose = @"CMS_AUDIT_BTN_CLOSE";
            public const string HeaderOpenFields = @"CMS_AUDIT_HEADER_OPEN_FIELDS";
            public const string SearchPlaceholder = @"CMS_AUDIT_SEARCH_PLACEHOLDER";
            public const string AvailableWorkInstrucitons = @"CMS_AUDIT_AVAILABLE_WORK_INSTRUCTIONS";
            public const string DownloadDataTitle = @"CMS_AUDIT_DOWNLOAD_DATA_TITLE";
            public const string DownloadAuditsTitle = @"CMS_AUDIT_DOWNLOAD_AUDITS_TITLE";
            public const string ExportAuditTitle = @"CMS_AUDIT_EXPORT_AUDIT_TITLE";
            public const string BtnResetFiltersTitle = @"CMS_AUDIT_BTN_RESET_FILTERS_TITLE";
            public const string AuditDetailsTitle = @"CMS_AUDIT_DETAILS_TITLE";
            public const string AuditsTitle = @"CMS_AUDIT_TITLE_AUDITS";

            public const string ItemAddLink = @"CMS_AUDIT_ITEM_ADD_LINK";

            public const string ItemAddLinkModalTitle = @"CMS_AUDIT_ITEM_ADD_LINK_MODAL_TITLE";

            public const string ItemAddLinkModalInsertLinkHere = @"CMS_AUDIT_ITEM_ADD_LINK_MODAL_INSERT_LINK_HERE";
            public const string ItemAddLinkModalValidationError = @"CMS_AUDIT_ITEM_ADD_LINK_MODAL_VALIDATION_ERROR";
            public const string ItemAddLinkModalSaveChanges = @"CMS_AUDIT_ITEM_ADD_LINK_MODAL_SAVE_CHANGES";
            public const string ItemAddLinkModalClose = @"CMS_AUDIT_ITEM_ADD_LINK_MODAL_CLOSE";

            public const string ConfirmationLeaveMessage = @"CMS_AUDIT_CONFIRMATIONLEAVEMESSAGE";
        }

        public static class Authentication
        {
            public const string IndentityPlatformConsent = @"CMS_AUTHENTICATION_INDENTITY_PLATFORM_CONSENT";
            public const string OptionMicrosoftPlatform = @"CMS_AUTHENTICATION_OPTION_MICROSOFT_PLATFORM";
            public const string MicrosoftPlatformTitle = @"CMS_AUTHENTICATION_MICROSOFT_PLATFORM_TITLE";
            public const string BtnSignIn = @"CMS_AUTHENTICATION_BTN_SIGN_IN";
            public const string ThankYouText = @"CMS_AUTHENTICATION_THANK_YOU_TEXT";
            public const string ExternalLoginProviderText = @"CMS_AUTHENTICATION_EXTERNAL_LOGIN_PROVIDER_TEXT";
            public const string LoadingTitle = @"CMS_AUTHENTICATION_LOADING_TITLE";
            public const string AccessDeniedText = @"CMS_AUTHENTICATION_ACCESS_DENIED_TEXT";
            public const string InsufficientRightsText = @"CMS_AUTHENTICATION_INSUFFICIENT_RIGHTS_TEXT";
            public const string ClickLoginPageLink = @"CMS_AUTHENTICATION_CLICK_LOGIN_PAGE_LINK";
            public const string ConnectionKeyText = @"CMS_AUTHENTICATION_CONNECTION_KEY_TEXT";
            public const string ConnectionKeyPlaceholder = @"CMS_AUTHENTICATION_CONNECTION_KEY_PLACEHOLDER";
            public const string ExternalLoginTitle = @"CMS_AUTHENTICATION_EXTERNAL_LOGIN_TITLE";
            public const string BtnExternalLogin = @"CMS_AUTHENTICATION_BTN_EXTERNAL_LOGIN";
            public const string LoggedOutText = @"CMS_AUTHENTICATION_LOGGED_OUT_TEXT";
            public const string ConsentPageTitle = @"CMS_AUTHENTICATION_CONSENT_PAGE_TITLE";
            public const string AuthenticationPageTitle = @"CMS_AUTHENTICATION_AUTHENTICATION_PAGE_TITLE";
            public const string LoginPageTitle = @"CMS_AUTHENTICATION_LOGIN_PAGE_TITLE";
            public const string UnknownUsernameOrPassword = @"CMS_LOGIN_UNKNOWN_USERNAME_OR_PASSWORD";
            public const string EmptyUsernameOrPassword = @"CMS_LOGIN_EMPTY_USERNAME_OR_PASSWORD";
        }

        public static class Checklist
        {
            public const string ChecklistTemplateLabel = @"CMS_CHECKLIST_TEMPLATE_LABEL";
            public const string ChecklistsLabel = @"CMS_CHECKLISTS_LABEL";
            public const string OverviewTitle = @"CMS_CHECKLIST_OVERVIEW_TITLE";
            public const string OverviewHeader = @"CMS_CHECKLIST_OVERVIEW_HEADER";
            public const string OverviewListTitle = @"CMS_CHECKLIST_OVERVIEW_LIST_TITLE";
            public const string ExportTemplates = @"CMS_CHECKLIST_EXPORT_TEMPLATES";
            public const string ExportTemplatesDesc = @"CMS_CHECKLIST_EXPORT_TEMPLATES_DESC";
            public const string ExportTitle = @"CMS_CHECKLIST_EXPORT_TITLE";
            public const string ExportProgress = @"CMS_CHECKLIST_EXPORT_PROGRESS";
            public const string ExportStatus = @"CMS_CHECKLIST_EXPORT_STATUS";
            public const string ExportStartDate = @"CMS_CHECKLIST_EXPORT_STARTDATE";
            public const string ExportEndDate = @"CMS_CHECKLIST_EXPORT_ENDDATE";
            public const string Search = @"CMS_CHECKLIST_SEARCH";
            public const string BackTitle = @"CMS_CHECKLIST_NAV_BACK_TITLE";
            public const string Title = @"CMS_CHECKLIST_TITLE";
            public const string ChecklistId = @"CMS_CHECKLIST_CHECKLIST_ID";
            public const string RoleTitle = @"CMS_CHECKLIST_SELECT_ROLE_TITLE";
            public const string SignatureTitle = @"CMS_CHECKLIST_SELECT_SIGNATURE_TITLE";
            public const string OptionNone = @"CMS_CHECKLIST_SELECT_OPTION_NONE";
            public const string OptionOneSignature = @"CMS_CHECKLIST_SELECT_OPTION_ONE_SIGNTURE";
            public const string OptionTwoSignature = @"CMS_CHECKLIST_SELECT_OPTION_TWO_SIGNATURE";
            public const string OptionBasic = @"CMS_CHECKLIST_SELECT_OPTION_BASIC";
            public const string OptionShiftLeader = @"CMS_CHECKLIST_SELECT_OPTION_SHIFTLEADER";
            public const string OptionManager = @"CMS_CHECKLIST_SELECT_OPTION_MANAGER";
            public const string ItemsTitle = @"CMS_CHECKLIST_CHECKLIST_ITEMS_TITLE";
            public const string ItemTitle = @"CMS_CHECKLIST_CHECKLIST_ITEM_TITLE";
            public const string TemplateNamePlaceholder = @"CMS_CHECKLIST_TEMPLATE_NAME_PLACEHOLDER";
            public const string StageTitlePlaceholder = @"CMS_CHECKLIST_STAGE_TITLE_PLACEHOLDER";
            public const string StageDescriptionPlaceholder = @"CMS_CHECKLIST_STAGE_DESCRIPTION_PLACEHOLDER";
            public const string TitlePlaceholder = @"CMS_CHECKLIST_ITEM_TITLE_PLACEHOLDER";
            public const string DescriptionPlaceholder = @"CMS_CHECKLIST_ITEM_DESCRIPTION_PLACEHOLDER";
            public const string BtnAddProperty = @"CMS_CHECKLIST_BTN_ADD_PROPERTY";
            public const string PropertyDescTitle = @"CMS_CHECKLIST_PROPERTY_DESC_TITLE";
            public const string DisplayFormat = @"CMS_CHECKLIST_DISPLAY_FORMAT";
            public const string DialogAddProperty = @"CMS_CHECKLIST_DIALOG_ADD_PROPERTY";
            public const string InstructionDialogTitle = @"CMS_CHECKLIST_INSTRUCTION_DIALOG_TITLE";
            public const string AddInstruction = @"CMS_CHECKLIST_ADD_INSTRUCTION";
            public const string AddWorkInstruction = @"CMS_CHECKLIST_ADD_WORKINSTRUCTION";
            public const string InstructionTitle = @"CMS_CHECKLIST_INSTRUCTION_TITLE";
            public const string InstructionDelete = @"CMS_CHECKLIST_INSTRUCTION_DELETE";
            public const string DialogPrevStep = @"CMS_CHECKLIST_DIALOG_PREV_STEP";
            public const string DialogClose = @"CMS_CHECKLIST_DIALOG_CLOSE";
            public const string DialogNextStep = @"CMS_CHECKLIST_DIALOG_NEXT_STEP";
            public const string DialogAddStep = @"CMS_CHECKLIST_DIALOG_ADD_STEP";
            public const string DialogPrevItem = @"CMS_CHECKLIST_DIALOG_PREVIOUS_ITEM";
            public const string DialogNextItem = @"CMS_CHECKLIST_DIALOG_NEXT_ITEM";
            public const string DialogDelete = @"CMS_CHECKLIST_DIALOG_DELETE";
            public const string AreaTitle = @"CMS_CHECKLIST_AREA_TITLE";
            public const string BtnPrintTitle = @"CMS_CHECKLIST_BTN_PRINT_TITLE";
            public const string BtnDuplicateTitle = @"CMS_CHECKLIST_BTN_DUPLICATE_TITLE";
            public const string BtnDeleteTitle = @"CMS_CHECKLIST_BTN_DELETE_TITLE";
            public const string BtnSaveTitle = @"CMS_CHECKLIST_BTN_SAVE_TITLE";
            public const string InstructionAddItem = @"CMS_CHECKLIST_INSTRUCTION_ADD_ITEM";
            public const string InstructionAddStage = @"CMS_CHECKLIST_INSTRUCTION_ADD_Stage";
            public const string InstructionAddItemLower = @"CMS_CHECKLIST_INSTRUCTION_ADD_ITEM_LOWER";
            public const string InstructionNextItem = @"CMS_CHECKLIST_INSTRUCTION_NEXT_ITEM";
            public const string InstructionDescPlaceholder = @"CMS_CHECKLIST_INSTRUCTION_DESC_PLACEHOLDER";
            public const string ItemAddPdf = @"CMS_CHECKLIST_ITEM_ADD_PDF";
            public const string OpenFields = @"CMS_CHECKLIST_OPENFIELDS";
            public const string AddOpenFields = @"CMS_CHECKLIST_ADD_OPEN_FIELDS";
            public const string ScanNewQrCodeTitle = @"CMS_CHECKLIST_SCAN_NEW_QR_CODE_TITLE";
            public const string BtnReportAction = @"CMS_CHECKLIST_BTN_REPORT_ACTION";
            public const string BtnNextItem = @"CMS_CHECKLIST_BTN_NEXT_ITEM";
            public const string BtnDone = @"CMS_CHECKLIST_BTN_DONE";
            public const string ReportActionTitle = @"CMS_CHECKLIST_REPORT_ACTION_TITLE";
            public const string BtnReportNow = @"CMS_CHECKLIST_BTN_REPORT_NOW";
            public const string AvailableWorkInstructions = @"CMS_CHECKLIST_AVAILABLE_WORK_INSTRUCTIONS";
            public const string BtnClose = @"CMS_CHECKLIST_BTN_CLOSE";
            public const string SearchPlaceholder = @"CMS_CHECKLIST_SEARCH_PLACEHOLDER";
            public const string DownloadDataTitle = @"CMS_CHECKLIST_DOWNLOAD_DATA_TITLE";
            public const string ChecklistDetailsTitle = @"CMS_CHECKLIST_CHECKLIST_DETAILS_TITLE";
            public const string ChecklistTitle = @"CMS_CHECKLIST_CHECKLIST_TITLE";
            public const string InstrucitonsFinished = @"CMS_CHECKLIST_INSTRUCTION_FINISHED";

            public const string AssignButton = @"CMS_CHECKLIST_ASSIGN_BUTTON";
            public const string ItemAddLink = @"CMS_CHECKLIST_ITEM_ADD_LINK";

            public const string ItemAddLinkModalTitle = @"CMS_CHECKLIST_ITEM_ADD_LINK_MODAL_TITLE";

            public const string ItemAddLinkModalInsertLinkHere = @"CMS_CHECKLIST_ITEM_ADD_LINK_MODAL_INSERT_LINK_HERE";
            public const string ItemAddLinkModalValidationError = @"CMS_CHECKLIST_ITEM_ADD_LINK_MODAL_VALIDATION_ERROR";
            public const string ItemAddLinkModalSaveChanges = @"CMS_CHECKLIST_ITEM_ADD_LINK_MODAL_SAVE_CHANGES";
            public const string ItemAddLinkModalClose = @"CMS_CHECKLIST_ITEM_ADD_LINK_MODAL_CLOSE";

            public const string ConfirmationLeaveMessage = @"CMS_CHECKLIST_CONFIRMATIONLEAVEMESSAGE";

            public const string LockNextStageTitle = @"CMS_CHECKLIST_LOCK_STAGE_TITLE";
            public const string LockNextStageDescription = @"CMS_CHECKLIST_LOCK_STAGE_DESCRIPTION";

            public const string NotLockNextStageTitle = @"CMS_CHECKLIST_NOT_LOCK_STAGE_TITLE";
            public const string NotLockNextStageDescription = @"CMS_CHECKLIST_NOT_LOCK_STAGE_DESCRIPTION";

            public const string EnableNotesTitle = @"CMS_CHECKLIST_ENABLE_NOTES_TITLE";
            public const string EnableNotesDescription = @"CMS_CHECKLIST_ENABLE_NOTES_DESCRIPTION";
        }

        public static class CmsLanguage
        {
            public const string TranslationPlaceholder = @"CMS_CMSLANGUAGE_TRANSLATION_PLACEHOLDER";
            public const string BtnEdit = @"CMS_CMSLANGUAGE_BTN_EDIT";
            public const string BtnClose = @"CMS_CMSLANGUAGE_BTN_CLOSE";
            public const string BtnSave = @"CMS_CMSLANGUAGE_BTN_SAVE";
            public const string BtnDone = @"CMS_CMSLANGUAGE_BTN_DONE";
            public const string BtnUpadate = @"CMS_CMSLANGUAGE_BTN_UPDATE";
            public const string NewResourceTitle = @"CMS_CMSLANGUAGE_NEW_RESOURCE_TITLE";
            public const string ManageLanguagesHeader = @"CMS_CMSLANGUAGE_MANAGE_LANGUAGES_HEADER";
            public const string ExportTitle = @"CMS_CMSLANGUAGE_EXPORT_TITLE";
            public const string ExportUpdateTitle = @"CMS_CMSLANGUAGE_EXPORT_UPDATE_TITLE";
            public const string ReInitializeStore = @"CMS_CMSLANGUAGE_RE_INITIALIZE_STORE";
            public const string SearchPlaceholder = @"CMS_CMSLANGUAGE_SEARCH_PLACEHOLDER";
            public const string LanguageText = @"CMS_CMSLANGUAGE_LANGUAGE_TEXT";
        }

        public static class Company
        {
            public const string BackToCompaniesTitle = @"CMS_COMPANY_BACK_TO_COMPANIES_TITLE";
            public const string Title = @"CMS_COMPANY_TITLE";
            public const string BtnSave = @"CMS_COMPANY_BTN_SAVE";
            public const string BtnClose = @"CMS_COMPANY_BTN_CLOSE";
            public const string CompanyName = @"CMS_COMPANY_COMPANY_NAME";
            public const string CompanyDescription = @"CMS_COMPANY_COMPANY_DESCRIPTION";
            public const string CompanyManager = @"CMS_COMPANY_COMPANY_MANAGER";
            public const string CardCompanyHoldingTitle = @"CMS_COMPANY_CARD_COMPANY_HOLDING_TITLE";
            public const string CompanyHolding = @"CMS_COMPANY_COMPANY_HOLDING";
            public const string OptionNoHoldingSelected = @"CMS_COMPANY_OPTION_NO_HOLDING_SELECTED";
            public const string CompanyHoldingUnitTitle = @"CMS_COMPANY_COMPANY_HOLDING_UNIT_TITLE";
            public const string OptionNoHoldingUnitSelected = @"CMS_COMPANY_OPITON_NO_HOLDING_UNIT_SELECTED";
            public const string CompaniesTitle = @"CMS_COMPANY_COMPANIES_TITLE";
            public const string BtnAddNewCompany = @"CMS_COMPANY_BTN_ADD_NEW_COMPANY";
            public const string BtnManageHoldings = @"CMS_COMPANY_BTN_MANAGE_HOLDINGS";
            public const string CompanySettingsNameTitle = @"CMS_COMPANY_COMPANY_SETTINGS_NAME_TITLE";
            public const string CompanyTagLimitTitle = @"CMS_COMPANY_COMPANY_TAG_LIMIT_TITLE";
            public const string CompanyTagGroupLimitTitle = @"CMS_COMPANY_COMPANY_TAGGROUP_LIMIT_TITLE";
            public const string CompanyTimezoneTitle = @"CMS_COMPANY_COMPANY_TIMEZONE_TITLE";
            public const string OptionNoCompanyTimezoneSelected = @"CMS_COMPANY_OPTION_NO_COMPANY_TIMEZONE_SELECTED";
            public const string CompanyLanguageTitle = @"CMS_COMPANY_COMPANY_LANGUAGE_TITLE";
            public const string OptionNoCompanyLanguageOrLocaleSelected = @"CMS_COMPANY_OPTION_NO_COMPANY_LANGUAGE_OR_LOCALE_SELECTED";
            public const string HoldingTitle = @"CMS_COMPANY_HOLDING_TITLE";
            public const string OptionNoHoldingSelectedOrAvailable = @"CMS_COMPANY_OPTION_NO_HOLDING_SELECTED_OR_AVAILABE";
            public const string OptionNoHoldingUnitSelectedOrAvailable = @"CMS_COMPANY_OPTION_NO_HOLDING_UNIT_SELECTED_OR_AVAILABE";
            public const string HeaderCompanyOverview = @"CMS_COMPANY_HEADER_COMPANY_OVERVIEW";
            public const string DescriptionTitle = @"CMS_COMPANY_DESCRIPTION_TITLE";
            public const string CompanyAdministratorFirstname = @"CMS_COMPANY_COMPANY_ADMINISTRATOR_FIRSTNAME";
            public const string CompanyAdministratorLastname = @"CMS_COMPANY_COMPANY_ADMINISTRATOR_LASTNAME";
            public const string CompanyAdministratorUsername = @"CMS_COMPANY_COMPANY_ADMINISTRATOR_USERNAME";
            public const string CompanyAdministratorPassword = @"CMS_COMPANY_COMPANY_ADMINISTRATOR_PASSWORD";
            public const string BtnGenerateNewPassword = @"CMS_COMPANY_BTN_GENERATE_NEW_PASSWORD";
            public const string OptionNoLanguageOrLocaleSelected = @"CMS_COMPANY_OPTION_NO_LANGUAGE_OR_LOCALE_SELECTED";
            public const string CompanyFeatureTierTitle = @"CMS_COMPANY_COMPANY_FEATURE_TIER_TITLE";
            public const string OptionEssential = @"CMS_COMPANY_OPTION_ESSENTIAL";
            public const string OptionAdvanced = @"CMS_COMPANY_OPTION_ADVANCED";
            public const string OptionPremium = @"CMS_COMPANY_OPTION_PREMIUM";
            public const string CompanyReportingTitle = @"CMS_COMPANY_COMPANY_REPORTING_TITLE";
            public const string CompanyStatisticsTitle = @"CMS_COMPANY_COMPANY_STATISTICS_TITLE";
            public const string NoHoldingSelectedTitle = @"CMS_COMPANY_NO_HOLDING_SELECTED_TITLE";
            public const string ManagementOverviewTitle = @"CMS_COMPANY_MANAGEMENT_OVERVIEW_TITLE";
            public const string StatisticsOverviewTitle = @"CMS_COMPANY_STATISTICS_OVERVIEW_TITLE";
            public const string NoCompanyTimezoneTitle = @"CMS_COMPANY_NO_COMPANY_TIMEZONE_TITLE";
            public const string NoCompanyLanguageSelectedTitle = @"CMS_COMPANY_NO_COMPANY_LANGUAGE_SELECTED_TITLE";
            public const string NoHoldingSelectedOrAvailableTitle = @"CMS_COMPANY_NO_HOLDING_SELECTED_OR_AVAILABLE_TITLE";
            public const string AddHoldingTitle = @"CMS_COMPANY_ADD_HOLDING_TITLE";
            public const string ChangeHoldingTitle = @"CMS_COMPANY_CHANGE_HOLDING_TITLE";
            public const string NoHoldingUnitSelectedOrAvailableTitle = @"CMS_COMPANY_NO_HOLDING_UNIT_SELECTED_OR_AVAILABLE_TITLE";
            public const string AddHoldingUnitTitle = @"CMS_COMPANY_ADD_HOLDING_UNIT_TITLE";
            public const string ChangeHoldingUnitTitle = @"CMS_COMPANY_CHANGE_HOLDING_UNIT_TITLE";
            public const string AddHoldingUnitChildTitle = @"CMS_COMPANY_ADD_HOLDING_UNIT_CHILD_TITLE";
            public const string SearchPlaceholder = @"CMS_COMPANY_SEARCH_PLACEHOLDER";
            public const string EditCompanyDetailsTitle = @"CMS_COMPANY_EDIT_COMPANY_DETAILS_TITLE";
            public const string EditCompanySettingsTitle = @"CMS_COMPANY_EDIT_COMPANY_SETTINGS_TITLE";
            public const string NoCompanyTimezoneAvailableTitle = @"CMS_COMPANY_NO_COMPANY_TIMEZONE_AVAILABLE_TITLE";
            public const string NoHoldingUnitSelectedTitle = @"CMS_COMPANY_NO_HOLDING_UNIT_SELECTED_TITLE";
            public const string NoCompanyTimezoneSelectedTitle = @"CMS_COMPANY_NO_COMPANY_TIMEZONE_SELECTED_TITLE";
            public const string EssentialTierTitle = @"CMS_COMPANY_ESSENTIAL_TIER_TITLE";
            public const string AdvancedTierTitle = @"CMS_COMPANY_ADVANCED_TIER_TITLE";
            public const string PremiumTierTitle = @"CMS_COMPANY_PREMIUM_TIER_TITLE";
            public const string SettingSaved = @"CMS_COMPANY_SETTING_SAVED";
            public const string SaveNewHolding = @"CMS_COMPANY_SAVE_NEW_HOLDING";
            public const string SaveChangedHolding = @"CMS_COMPANY_SAVE_CHANGED_HOLDING";
            public const string SaveNewChildUnit = @"CMS_COMPANY_SAVE_NEW_CHILD_UNIT";
            public const string SaveNewUnit = @"CMS_COMPANY_SAVE_NEW_UNIT";
            public const string SaveChangedUnit = @"CMS_COMPANY_SAVE_CHANGED_UNIT";
            public const string CompanyHoldingSaved = @"CMS_COMPANY_COMPANY_HOLDING_SAVED";
            public const string CompanyHoldingUnitSaved = @"CMS_COMPNAY_COMPANY_HOLDING_UNIT_SAVED";
            public const string CompanySaved = @"CMS_COMPNAY_COMPANY_SAVED";
            public const string ReportRetrieved = @"CMS_COMPANY_REPORT_RETRIEVED";
        }

        public static class Comment
        {
            public const string CommentsLabel = @"CMS_COMMENTS_LABEL";
            public const string DetailsTitle = @"CMS_COMMENT_DETAILS_TITLE";
            public const string DetailsBackToOverview = @"CMS_COMMENT_NAV_BACK_TITLE";
            public const string DetailsPostedTitle = @"CMS_COMMENT_DETAILS_POSTED_TITLE";
        }

        public static class LastCompletedItems
        {
            public const string LastCompleted = @"CMS_LASTCOMPLETEDITEMS_LAST_COMPLETED";
            public const string Audits = @"CMS_LASTCOMPLETEDITEMS_AUDITS";
            public const string Checklists = @"CMS_LASTCOMPLETEDITEMS_CHECKLISTS";
            public const string Tasks = @"CMS_LASTCOMPLETEDITEMS_TASKS";
            public const string Assessments = @"CMS_LASTCOMPLETEDITEMS_ASSESSMENTS";
            public const string NoCompletedAuditsFound = @"CMS_LASTCOMPLETEDITEMS_NO_COMPLETED_AUDITS_FOUND";
            public const string NoCompletedChecklistsFound = @"CMS_LASTCOMPLETEDITEMS_NO_COMPLETED_CHECKLISTS_FOUND";
            public const string NoCompletedTasksFound = @"CMS_LASTCOMPLETEDITEMS_NO_COMPLETED_TASKS_FOUND";
            public const string NoCompletedAssessmentsFound = @"CMS_LASTCOMPLETEDITEMS_NO_COMPLETED_ASSESSMENTS_FOUND";
        }

        public static class Config
        {
            public const string Title = @"CMS_CONFIG_TITLE";
            public const string ButtonShifts = @"CMS_CONFIG_BTN_SHIFTS";
            public const string ButtonAddNewArea = @"CMS_CONFIG_BTN_ADD_NEW_AREA";
            public const string ButtonSetRoleNames = @"CMS_CONFIG_BTN_SET_ROLE_NAMES";
            public const string AreaDetails = @"CMS_CONFIG_CONTEXT_AREA_DETAILS";
            public const string AreaEditArea = @"CMS_CONFIG_CONTEXT_EDIT_AREA";
            public const string ButtonAddSubArea = @"CMS_CONFIG_BTN_ADD_SUB_AREA";
            public const string ButtonRemove = @"CMS_CONFIG_BTN_REMOVE";
            public const string NavigateBack = @"CMS_CONFIG_NAV_BACK_TITLE";
            public const string SchedulerTitle = @"CMS_CONFIG_SCHEDULER_TITLE";
            public const string LinkAddShift = @"CMS_CONFIG_LINK_ADD_SHIFT";
            public const string ButtonAddShift = @"CMS_CONFIG_BTN_ADD_SHIFT";
            public const string SchedulerStartTime = @"CMS_CONFIG_SCHEDULER_STARTTIME";
            public const string SchedulerEndTime = @"CMS_CONFIG_SCHEDULER_ENDTIME";
            public const string DialogButtonRemove = @"CMS_CONFIG_DIALOG_BTN_REMOVE";
            public const string DialogButtonCancel = @"CMS_CONFIG_DIALOG_BTN_CANCEL";
            public const string DialogButtonChange = @"CMS_CONFIG_DIALOG_BTN_CHANGE";
            public const string DialogButtonSave = @"CMS_CONFIG_DIALOG_BTN_SAVE";
            public const string DialogDayOfWeekTitle = @"CMS_CONFIG_DIALOG_DAYOFWEEK";
            public const string DialogButtonAdd = @"CMS_CONFIG_DIALOG_ADD";
            public const string DialogButtonAddNew = @"CMS_CONFIG_DIALOG_ADD_NEW";
            public const string IntegrationTitle = @"CMS_CONFIG_INTERGRATION_TITLE"; //new
            public const string MarketplaceTitle = @"CMS_CONFIG_MARKETPLACE_TITLE";
            public const string SettingBTN = @"CMS_CONFIG_SETTING_BTN";
            public const string SettingTitle = @"CMS_CONFIG_SETTING_TITLE";
            public const string CompanyAreaTitle = @"CMS_COMPANY_AREA_TITLE";
            public const string NoIntegrationsAddedText = @"CMS_CONFIG_NO_INTEGRATION_ADDED_TEXT";
            public const string DialogButtonSaveAndAdd = @"CMS_CONFIG_DIALOG_BUTTON_SAVE_AND_ADD";
            public const string Roles = @"CMS_CONFIG_ROLES";
            public const string ManagerPlaceholder = @"CMS_CONFIG_MANAGER_PLACEHOLDER";
            public const string ShiftLeaderPlaceholder = @"CMS_CONFIG_SHIFT_LEADER_PLACEHOLDER";
            public const string BasicUserPlaceholder = @"CMS_CONFIG_BASIC_USER_PLACEHOLDER";
            public const string NumberOfActiveTaskTemplatesTitle = @"CMS_CONFIG_NUMBER_OF_ACTIVE_TASK_TEMPLATES_TITLE";
            public const string NumberOfActiveChecklistTemplatesTitle = @"CMS_CONFIG_NUMBER_OF_ACTIVE_CHECKLIST_TEMPLATES_TITLE";
            public const string NumberOfActiveAuditTemplatesTitle = @"CMS_CONFIG_NUMBER_OF_ACTIVE_AUDIT_TEMPLATES_TITLE";
            public const string NumberOfShiftsConnectedTitle = @"CMS_CONFIG_NUMBER_OF_SHIFTS_CONNECTED_TITLE";
            public const string NumberOfActionsConnectedTitle = @"CMS_CONFIG_NUMBER_OF_ACTIONS_CONNECTED_TITLE";
            public const string NumberOfActiveAreasTitle = @"CMS_CONFIG_NUMBER_OF_ACTIVE_AREAS_TITLE";
            public const string NumberOfActiveWorkInstructions = @"CMS_CONFIG_NUMBER_OF_ACTIVE_WORK_INSTRUCTIONS_TITLE";
            public const string NumberOfActiveAssessmentTemplates = @"CMS_CONFIG_NUMBER_OF_ACTIVE_ASSESSMENT_TEMPLATES_TITLE";
            public const string NumberOfActiveSkillsMatrices = @"CMS_CONFIG_NUMBER_OF_ACTIVE_SKILLS_MATRICES_TITLE";
            public const string TaskTemplatesText = @"CMS_CONFIG_TASK_TEMPLATES_TEXT";
            public const string ChecklistTemplatesText = @"CMS_CONFIG_CHECKLIST_TEMPLATES_TEXT";
            public const string AuditTemplatesText = @"CMS_CONFIG_AUDIT_TEMPLATES_TEXT";
            public const string ShiftText = @"CMS_CONFIG_SHIFT_TEXT";
            public const string ActionsText = @"CMS_CONFIG_ACTIONS_TEXT";
            public const string AreasText = @"CMS_CONFIG_AREAS_TEXT";
            public const string WorkInstructionsText = @"CMS_CONFIG_WORK_INSTRUCTIONS_TEXT";
            public const string AssessmentTemplatesText = @"CMS_CONFIG_ASSESSMENT_TEMPLATES_TEXT";
            public const string SkillsMatricesText = @"CMS_CONFIG_SKILLS_MATRICES_TEXT";
            public const string HeaderIntegrateWithSolvace = @"CMS_CONFIG_HEADER_INTEGRATE_WITH_SOLVACE";
            public const string TimeStartPlaceholder = @"CMS_CONFIG_TIME_START_PLACEHOLDER";
            public const string TimeEndPlaceholder = @"CMS_CONFIG_TIME_END_PLACEHOLDER";
            public const string AreaShift = @"CMS_CONFIG_AREA_SHIFT";
            public const string AddIntegration = @"CMS_CONFIG_ADD_INTEGRATION";
            public const string SavingAreasDisabled = @"CMS_CONFIG_SAVING_AREAS_DISABLED";
            public const string SavingShiftsDisabled = @"CMS_CONFIG_SAVING_SHIFTS_DISABLED";
            public const string AreYouSureDelete = @"CMS_CONFIG_ARE_YOU_SURE_DELETE";
            public const string ActionCanNotBeUndone = @"CMS_CONFIG_ACTION_CAN_NOT_BE_UNDONE";
            public const string OptionYes = @"CMS_CONFIG_OPTION_YES";
            public const string OptionNo = @"CMS_CONFIG_OPTION_NO";
            public const string RolesSaved = @"CMS_CONFIG_ROLES_SAVED";
            public const string BasicMustFilled = @"CMS_CONFIG_BASIC_MUST_FILLED";
            public const string ShiftLeaderMustFilled = @"CMS_CONFIG_SHIFT_LEADER_MUST_FILLED";
            public const string ManagerMustFilled = @"CMS_CONFIG_MANAGER_MUST_FILLED";
            public const string AddNewArea = @"CMS_CONFIG_ADD_NEW_AREA";
            public const string EditArea = @"CMS_CONFIG_EDIT_AREA";
            public const string AreaDetailsText = @"CMS_CONFIG_AREA_DETAILS_TEXT";
            public const string AddNewSubarea = @"CMS_CONFIG_ADD_NEW_SUBAREA";
            public const string PleaseCompleteAdding = @"CMS_CONFIG_PLEASE_COMPLETE_ADDING";
            public const string IntegrationSaved = @"CMS_CONFIG_INTEGRATION_SAVED";
            public const string AnErrorOccured = @"CMS_CONFIG_AN_ERROR_OCCURED";
            public const string WhenUsing24Hour = @"CMS_CONFIG_WHEN_USING_24_HOUR";
            public const string OverlappingShift = @"CMS_CONFIG_OVERLAPPING_SHIFT";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.CompanyRoleNamesText, "Here you can configure role names for your company.") ?? "Here you can configure role names for your company.")
            public const string CompanyRoleNamesText = @"CMS_CONFIG_ROLENAMES_TEXT";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.AccesToAllText, "Has access to all data of the company and all settings configuration.") ?? "Has access to all data of the company and all settings configuration.")
            public const string AccesToAllText = @"CMS_CONFIG_ACCESSTOALL_TEXT";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.AccesToSelectedText, "Has access to selected areas within the company and can only configure those areas. Has access to the app. Can view reports in the app.") ?? "Has access to selected areas within the company and can only configure those areas. Has access to the app. Can view reports in the app.")
            public const string AccesToSelectedText = @"CMS_CONFIG_ACCESSTOSELECTED_TEXT";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.AccesToAreaBasic, "Has access to selected areas within the company can only perform tasks, checklists, audits. Only app access (no web access). Can view reports.") ?? "Has access to selected areas within the company can only perform tasks, checklists, audits. Only app access (no web access). Can view reports.")
            public const string AccesToAreaBasic = @"CMS_CONFIG_ACCESSAREABASIC_TEXT";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.AreaModalTitleText, "Area") ?? "Area")
            public const string AreaModalTitleText = @"CMS_CONFIG_AREAMODAL_TITLE_TEXT";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.AreaModalNameTitle, "Name") ?? "Name")
            public const string AreaModalNameTitle = @"CMS_CONFIG_AREAMODAL_NAME_TITLE";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.AreaModalDescriptionTitle, "Description") ?? "Description")
            public const string AreaModalDescriptionTitle = @"CMS_CONFIG_AREAMODAL_DESCRIPTION_TITLE";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.AreaModalCancelButton, "Cancel") ?? "Cancel")
            public const string AreaModalCancelButton = @"CMS_CONFIG_AREAMODAL_CANCEL_BUTTON";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.AreaModalSaveButton, "Save") ?? "Save")
            public const string AreaModalSaveButton = @"CMS_CONFIG_AREAMODAL_SAVE_BUTTON";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.AreaModalNamePlaceholder, "Area name") ?? "Area name")
            public const string AreaModalNamePlaceHolder = @"CMS_CONFIG_AREAMODAL_NAME_PLACEHOLDER";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.AreaModalDescriptionPlaceHolder, "Area description") ?? "Area description")
            public const string AreaModalDescriptionPlaceHolder = @"CMS_CONFIG_AREAMODAL_DESCRIPTION_PLACEHOLDER";
            //@(CmsLanguage?.GetValue(LanguageKeys.Config.AreaDetailsModalTitle, "Details") ?? "Details")
            public const string AreaDetailsModalTitle = @"CMS_CONFIG_AREADETAILSMODAL_TITLE";
            //@(Model?.CmsLanguage.GetValue(LanguageKeys.Config.FormGroupName, "Name") ?? "Name")
            public const string FormGroupName = @"CMS_CONFIG_FORM_GROUP_NAME";
            //@(Model?.CmsLanguage.GetValue(LanguageKeys.Config.FormGroupLocation, "Location") ?? "Location")
            public const string FormGroupLocation = @"CMS_CONFIG_FORM_GROUP_LOCATION";
            //@(Model?.CmsLanguage.GetValue(LanguageKeys.Config.FormGroupConnectedItems, "Directly connected active items") ?? "Directly connected active items")
            public const string FormGroupConnectedItems = @"CMS_CONFIG_FORM_GROUP_CONNECTED_ITEMS";
        }

        public static class CompanySetting
        {
            public const string NotifyEndUserWorkInstructionChangeDoneCorrectly = @"CMS_NOTIFY_ENDUSER_WORKINSTRUCTION_CHANGE_Correctly";
            public const string NotifyEndUserWorkInstructionChangeNotCorrectly = @"CMS_NOTIFY_ENDUSER_WORKINSTRUCTION_CHANGE_Not_Correctly";

            public const string NotifyEndUserSkillMatrixChangeDoneCorrectly = @"CMS_NOTIFY_ENDUSER_SkillMatrix_CHANGE_Correctly";
            public const string NotifyEndUserSkillMatrixChangeNotCorrectly = @"CMS_NOTIFY_ENDUSER_SkillMatrix_CHANGE_Not_Correctly";

        }

        public static class Marketplace
        {
            public const string Title = @"CMS_MARKETPLACE_TITLE";
            public const string FeaturesAndProductivityAndIntegrationsTitle = @"CMS_MARKETPLACE_FEATURES_AND_PRODUCTIVITY_AND_INTEGRATIONS_TITLE";
            public const string RequestInformationTitle = @"CMS_MARKETPLACE_REQUEST_INFORMATION_TITLE";
            public const string ProvideValidEmailText = @"CMS_MARKETPLACE_PROVIDE_VALID_EMAIL_TEXT";
            public const string SendRequestText = @"CMS_MARKETPLACE_SEND_REQUEST_TEXT";
            public const string YourEmailPlaceholder = @"CMS_MARKETPLACE_YOUR_EMAIL_PLACEHOLDER";
            public const string CommentsPlaceholder = @"CMS_MARKETPLACE_COMMENTS_PLACEHOLDER";
            public const string MarketplaceTitle = @"CMS_MARKETPLACE_MARKETPLACE_TITLE";
            public const string EmailSent = @"CMS_MARKETPLACE_EMAIL_SENT";
        }

        /// <summary>
        /// Dashboard language keys
        /// </summary>
        public static class Dashboard
        {
            public const string Title = @"CMS_DASHBOARD_TITLE";
            public const string StatsChecklistTitle = @"CMS_DASHBOARD_STATS_CHECKLIST_TEMPLATE_TITLE";
            public const string StatsChecklistShowTitle = @"CMS_DASHBOARD_STATS_CHECKLIST_SHOW_TITLE";
            public const string StatsTaskTitle = @"CMS_DASHBOARD_STATS_TASK_TEMPLATE_TITLE";
            public const string StatsTaskShowTitle = @"CMS_DASHBOARD_STATS_TASK_TEMPLATE_SHOW_TITLE";
            public const string StatsAuditTitle = @"CMS_DASHBOARD_STATS_AUDIT_TEMPLATE_TITLE";
            public const string StatsAuditShowTitle = @"CMS_DASHBOARD_STATS_AUDIT_TEMPLATE_SHOW_TITLE";
            public const string StatsActionTitle = @"CMS_DASHBOARD_STATS_ACTION_TEMPLATE_TITLE";
            public const string StatsActionShowTitle = @"CMS_DASHBOARD_STATS_ACTION_TEMPLATE_SHOW_TITLE";
            public const string StatsChecklistCompletedTitle = @"CMS_DASHBOARD_STATS_CHECKLIST_COMPLETED_TITLE";
            public const string StatsTaskCompletedTitle = @"CMS_DASHBOARD_STATS_TASK_COMPLETED_TITLE";
            public const string StatsTaskCompletedTitleTimeFrame = @"CMS_DASHBOARD_STATS_TASK_COMPLETED_TITLE_TIMEFRAME";
            public const string StatsAuditCompletedTitle = @"CMS_DASHBOARD_STATS_AUDIT_COMPLETED_TITLE";
            public const string StatsActionCompletedTitle = @"CMS_DASHBOARD_STATS_ACTION_COMPLETED_TITLE";
            public const string StatsAssessmentsCompletedTitle = @"CMS_DASHBOARD_STATS_ASSESSMENTS_COMPLETED_TITLE";
            public const string ShowAll = @"CMS_DASHBOARD_SHOW_ALL_TITLE";
            public const string CompletedBy = @"CMS_DASHBOARD_COMPLETED_BY";
            public const string LastModifiedBy = @"CMS_DASHBOARD_LAST_MODIFIED_BY";
            public const string CompletedFor = @"CMS_DASHBOARD_COMPLETED_FOR";
            public const string ClickToClose = @"CMS_DASHBOARD_CLICK_TO_CLOSE";
            public const string NoDataView = @"CMS_DASHBOARD_NO_DATA_TO_VIEW";
            public const string ReleaseNotes = @"CMS_DASHBOARD_RELEASE_NOTES_TITLE";
            public const string Collapse = @"CMS_DASHBOARD_COLLAPSE_TITLE";
            public const string Remove = @"CMS_DASHBOARD_REMOVE_TITLE";
            public const string LastCompletedAssessment = @"CMS_DASHBOARD_LAST_COMPLETED_ASSESSMENT";
            public const string NoCompletedAssessmentsFound = @"CMS_DASHBOARD_NO_COMPLETED_ASSESSMENTS_FOUND";
            public const string Popover = @"CMS_DASHBOARD_POPOVER";
            public const string NewActionIsAdded = @"CMS_DASHBOARD_NEW_ACTION_IS_ADDED";
            public const string TheActionShouldBe = @"CMS_DASHBOARD_THE_ACTION_SHOULD_BE";
            public const string TaskTappingsTitle = @"CMS_DASHBOARD_TASK_TAPPINGS_TITLE";
            public const string TillNowThere8TaskTappings = @"CMS_DASHBOARD_TILL_NOW_THERE_8_TASK_TAPPINGS";
            public const string CompletedAndSignedChecklistTitle = @"CMS_DASHBOARD_COMPLETED_AND_SIGNED_CHEKLIST_TITLE";
            public const string TodayThereAreChecklistsTitle = @"CMS_DASHBOARD_TODAY_THERE_ARE_CHECKLISTS_TITLE";
            public const string ActionText = @"CMS_DASHBOARD_ACTION_TEXT";
            public const string AnotherActionText = @"CMS_DASHBOARD_ANOTHER_ACTION_TEXT";
            public const string SomethingElseHereText = @"CMS_DASHBOARD_SOMETHING_ELSE_HERE_TEXT";
            public const string SeparatedLinkText = @"CMS_DASHBOARD_SEPARATED_LINK_TEXT";
            public const string BackToDashboard = @"CMS_DASHBOARD_BACK_TO_DASHBOARD";
            public const string DownloadCompanyManagementOverviewTitle = @"CMS_DASHBOARD_DOWNLOAD_COMPANY_MANAGEMENT_OVERVIEW_TITLE";
            public const string CompanyManagementOverview = @"CMS_DASHBOARD_COMPANY_MANAGEMENT_OVERVIEW";
            public const string WorkInstructionTemplatedText = @"CMS_DASHBOARD_WORK_INSTRUCTION_TEMPLATED_TEXT";
            public const string ShowWorkInstructionTemplatesText = @"CMS_DASHBOARD_SHOW_WORK_INSTRUCTION_TEMPLATES_TEXT";
            public const string AssessmentTemplatesText = @"CMS_DASHBOARD_ASSESSMENT_TEMPLATES_TEXT";
            public const string ShowAssessmentTemplatesText = @"CMS_DASHBOARD_SHOW_ASSESSMENT_TEMPLATES_TEXT";
            public const string AssessmentInstructionTemplatesText = @"CMS_DASHBOARD_ASSESSMENT_INSTRUCTION_TEMPLATES_TEXT";
            public const string ShowAssessmentInstructionTemplatesText = @"CMS_DASHBOARD_SHOW_ASSESSMENT_INSTRUCTION_TEMPLATES_TEXT";
            public const string MatricesText = @"CMS_DASHBOARD_MATRICES_TEXT";
            public const string ShowMatricesText = @"CMS_DASHBOARD_SHOW_MATRICES_TEXT";
            public const string AllReleasesTitle = @"CMS_DASHBOARD_ALL_RELEASES_TITLE";
            public const string Assessor = @"CMS_DASHBOARD_ASSESSOR";
            public const string AverageScore = @"CMS_DASHBOARD_AVERAGE_SCORE";
            public const string DueAt = @"CMS_DASHBOARD_DUE_AT";
            public const string At = @"CMS_DASHBOARD_AT";
            public const string ToDo = @"CMS_DASHBOARD_TO_DO";
            public const string Ok = @"CMS_DASHBOARD_OK";
            public const string NotOk = @"CMS_DASHBOARD_NOT_OK";
            public const string Skipped = @"CMS_DASHBOARD_SKIPPED";
            public const string SelectAnAreaToView = @"CMS_DASHBOARD_SELECT_AN_AREA_TO_VIEW";
            public const string NoCompletedTasksFound = @"CMS_DASHBOARD_NO_COMPLETED_TASKS_FOUND";
            public const string UseThisToManagePage = @"CMS_DASHBOARD_USE_THIS_TO_MANAGE_PAGE";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsCompanies, "Companies") ?? "Companies")
            public const string AdminStatsCompanies = @"CMS_DASHBOARD_STATS_ADMIN_COMPANIES";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsUsers, "Users") ?? "Users")
            public const string AdminStatsUsers = @"CMS_DASHBOARD_STATS_ADMIN_USERS";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsAnnouncements, "Announcements") ?? "Announcements")
            public const string AdminStatsAnnouncements = @"CMS_DASHBOARD_STATS_ADMIN_ANNOUNCEMENTS";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsFactoryFeedMessages, "Factoryfeed messages") ?? "Factoryfeed messages")
            public const string AdminStatsFactoryFeedMessages = @"CMS_DASHBOARD_STATS_ADMIN_FACTMESSAGES";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsFactoryAudits, "Audits") ?? "Audits")
            public const string AdminStatsAudits = @"CMS_DASHBOARD_STATS_ADMIN_AUDITS";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsAuditTemplates, "Audit templates") ?? "Audit templates")
            public const string AdminStatsAuditTemplates = @"CMS_DASHBOARD_STATS_ADMIN_AUDITTEMPLATES";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsChecklists, "Checklists") ?? "Checklists")
            public const string AdminStatsChecklists = @"CMS_DASHBOARD_STATS_ADMIN_CHECKLISTS";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsChecklistTemplates, "Checklist templates") ?? "Checklist templates")
            public const string AdminStatsChecklistTemplates = @"CMS_DASHBOARD_STATS_ADMIN_CHECKLISTTEMPLATES";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsTasks, "Tasks") ?? "Tasks")
            public const string AdminStatsTasks = @"CMS_DASHBOARD_STATS_ADMIN_TASKS";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsTaskTemplates, "Task templates") ?? "Task templates")
            public const string AdminStatsTaskTemplates = @"CMS_DASHBOARD_STATS_ADMIN_TASKTEMPLATES";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsActions, "Actions") ?? "Actions")
            public const string AdminStatsActions = @"CMS_DASHBOARD_STATS_ADMIN_ACTIONS";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsActionComments, "Actions comments") ?? "Actions comments")
            public const string AdminStatsActionComments = @"CMS_DASHBOARD_STATS_ADMIN_ACTIONCOMMENTS";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsSettings, "Settings") ?? "Settings")
            public const string AdminStatsSettings = @"CMS_DASHBOARD_STATS_ADMIN_SETTINGS";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsRawViewer, "Raw viewer") ?? "Raw viewer")
            public const string AdminStatsRawViewer = @"CMS_DASHBOARD_STATS_ADMIN_RAW_VIEWER";
            //@(Model?.CmsLanguage.GetValue(LanguageKeys.Dashboard.AdminStatsScheduler, "Scheduler") ?? "Scheduler")
            public const string AdminStatsScheduler = @"CMS_DASHBOARD_STATS_ADMIN_SCHEDULER";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsWorkInstructions, "Workinstructions") ?? "Workinstructions")
            public const string AdminStatsWorkInstructions = @"CMS_DASHBOARD_STATS_ADMIN_WORKINSTRUCTIONS";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsMatrices, "Matrices") ?? "Matrices")
            public const string AdminStatsMatrices = @"CMS_DASHBOARD_STATS_ADMIN_MATRICES";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsAssessments, "Assessments") ?? "Assessments")
            public const string AdminStatsAssessments = @"CMS_DASHBOARD_STATS_ADMIN_ASSESSMENTS";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsAssessmentTemplates, "Assessment templates") ?? "Assessment templates")
            public const string AdminStatsAssessmentTemplates = @"CMS_DASHBOARD_STATS_ADMIN_ASSESSMENTTEMPLATES";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsAreas, "Areas") ?? "Areas")
            public const string AdminStatsAreas = @"CMS_DASHBOARD_STATS_ADMIN_AREAS";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Dashboard.AdminStatsHoldings, "Holdings") ?? "Holdings")
            public const string AdminStatsHoldings = @"CMS_DASHBOARD_STATS_ADMIN_HOLDINGS";

        }

        public static class ExternalLink
        {
            public const string Title = @"CMS_EXTERNALLINK_TITLE";
            public const string SubTitle = @"CMS_EXTERNALLINK_SUBTITLE";
            public const string ProceedDescription = @"CMS_EXTERNALLINK_PROCEED_DESCRIPTION";
            public const string ExternalLinkDescription = @"CMS_EXTERNALLINK_EXTERNAL_LINK_DESCRIPTION";
            public const string ProceedButtonText = @"CMS_EXTERNALLINK_PROCEED_BUTTON_TEXT";
        }

        public static class FactoryFeed
        {
            public const string NewsTitle = @"CMS_FACTORYFEED_NEWS_TITLE";
            public const string NewsText = @"CMS_FACTORYFEED_NEWS_TEXT";
            public const string FactoryUpdatesTitle = @"CMS_FACTORYFEED_FACTORYUPDATES_TITLE";
            public const string CheckItNowText = @"CMS_FACTORYFEED_CHECK_IT_NOW_TEXT";
            public const string BtnGoodJob = @"CMS_FACTORYFEED_BTN_GOOD_JOB";
            public const string BtnComment = @"CMS_FACTORYFEED_BTN_COMMENT";
            public const string BtnPlace = @"CMS_FACTORYFEED_BTN_PLACE";
            public const string EditTitle = @"CMS_FACTORYFEED_EDIT_TITLE";
            public const string DeleteTitle = @"CMS_FACTORYFEED_DELETE_TITLE";
            public const string ReturnToHomeScreen = @"CMS_FACTORYFEED_RETURN_TO_HOME_SCREEN";
            public const string SaveButton = @"CMS_FACTORYFEED_BUTTON_SAVE";
            public const string CancelButton = @"CMS_FACTORYFEED_BUTTON_CANCEL";
            public const string PictureTitle = @"CMS_FACTROYFEED_PICTURE_TITLE";
            public const string VideoTitle = @"CMS_FACTORYFEED_VIDEO_TITLE";
            public const string PostTitle = @"CMS_FACTORYFEED_POST_TITLE";
            public const string MostRecentTitle = @"CMS_FACTORYFEED_MOST_RECENT_TITLE";
            public const string TasksTitle = @"CMS_FACTORYFEED_TASKS_TITLE";
            public const string ChecklistTitle = @"CMS_FACTORYFEED_CHECKLIST_TITLE";
            public const string AuditsTitle = @"CMS_FACTORYFEED_AUDITS_TITLE";
            public const string ActionsTitle = @"CMS_FACTORYFEED_ACTIONS_TITLE";
            public const string ChecksTitle = @"CMS_FACTORYFEED_CHECKS_TITLE";
            public const string StatisticsPostsTitle = @"CMS_FACTORYFEED_STATISTICS_POSTS_TITLE";
            public const string StatisticsCommentsTitle = @"CMS_FACTORYFEED_STATISTICS_COMMENTS_TITLE";
            public const string StatisticsLikesTitle = @"CMS_FACTORYFEED_STATISTICS_LIKES_TITLE";
            public const string LinkText = @"CMS_FACTORYFEED_LINK_TEXT";
            public const string FactoryFeedTitle = @"CMS_FACTORYFEED_FACTORYFEED_TITLE";
            public const string AlertTitle = @"CMS_FACTORYFEED_ALERT_TITLE";
            public const string CommentPlacedReload = @"CMS_FACTORYFEED_COMMENT_PLACED_RELOAD";
            public const string FeedMessageDeletedReload = @"CMS_FACTORYFEED_FEED_MESSAGE_DELETED_RELOAD";
            public const string FeedMessageAddedReload = @"CMS_FACTORYFEED_FEED_MESSAGE_ADDED_RELOAD";
            public const string PostLikedOrUnlikedReolad = @"CMS_FACTORYFEED_POST_LIKED_OR_UNLIKED_RELOAD";
            public const string OptionYes = @"CMS_FACTORYFEED_OPTION_YES";
            public const string OptionNo = @"CMS_FACTORYFEED_OPTION_NO";
            public const string PinToTop = @"CMS_FACTORYFEED_PIN_TO_TOP";
            public const string WhatDoYouWantToShare = @"CMS_FACTORYFEED_WHAT_DO_YOU_WANT_TO_SHARE";
            public const string LastModifiedBy = @"CMS_FACTORYFEED_LAST_MODIFIED_BY";
        }

        public static class Home
        {
            public const string HeaderChecklists = @"CMS_HOME_HEADER_CHECKLSITS";
            public const string LastActivityText = @"CMS_HOME_LAST_ACTIVITY_TEXT";
            public const string TwoMinutesAgoText = @"CMS_HOME_TWO_MINUTES_AGO_TEXT";
            public const string HeaderTasks = @"CMS_HOME_HEADER_TASKS";
            public const string HeaderAudits = @"CMS_HOME_HEADER_AUDITS";
            public const string HeaderActions = @"CMS_HOME_HEADER_ACTIONS";
            public const string HeaderReports = @"CMS_HOME_HEADER_REPORTS";
            public const string HeaderConfiguration = @"CMS_HOME_HEADER_CONFIGURATION";
            public const string UseThisToPrivacyPolicy = @"CMS_HOME_USE_THIS_TO_PRIVACY_POLICY";
            public const string SettingsText = @"CMS_HOME_SETTINGS_TEXT";
            public const string SetupYourEnviromentText = @"CMS_HOME_SETUP_YOUR_ENVIROMENT_TEXT";
            public const string ShowActivityOnDashboard = @"CMS_HOME_SHOW_ACTIVITY_ON_DASHBOARD";
            public const string ShowImagesOnDashboard = @"CMS_HOME_SHOW_IMAGES_ON_DASHBOARD";
            public const string Title = @"CMS_HOME_TITLE";
            public const string PrivacyPolicyTitle = @"CMS_HOME_PRIVACY_POLICY_TITLE";
        }

        public static class EzgoList
        {
            public const string DeleteAuditItem = @"CMS_EZGOLIST_DELETE_DIALOGUES_DELETE_AUDIT_ITEM";
            public const string DeleteChecklistItem = @"CMS_EZGOLIST_DELETE_DIALOGUES_DELETE_CHECKLIST_ITEM";
            public const string DeleteWorkInstructionItem = @"CMS_EZGOLIST_DELETE_DIALOGUES_DELETE_WORKINSTRUCTION_ITEM";

            public const string DeleteAuditItemInstruction = @"CMS_EZGOLIST_DELETE_DIALOGUES_DELETE_AUDIT_ITEM_INSTRUCTION";
            public const string DeleteChecklistItemInstruction = @"CMS_EZGOLIST_DELETE_DIALOGUES_DELETE_CHECKLIST_ITEM_INSTRUCTION";
            public const string DeleteTaskInstruction = @"CMS_EZGOLIST_DELETE_DIALOGUES_DELETE_TASK_INSTRUCTION";

            public const string DeleteAuditTemplate = @"CMS_EZGOLIST_DELETE_DIALOGUES_DELETE_AUDIT_TEMPLATE";
            public const string DeleteChecklistTemplate = @"CMS_EZGOLIST_DELETE_DIALOGUES_DELETE_CHECKLIST_TEMPLATE";
            public const string DeleteSkillAssessmentTemplate = @"CMS_EZGOLIST_DELETE_DIALOGUES_DELETE_SKILLASSESSMENT_TEMPLATE";
            public const string DeleteTaskTemplate = @"CMS_EZGOLIST_DELETE_DIALOGUES_DELETE_TASK_TEMPLATE";
            public const string DeleteWorkInstructionTemplate = @"CMS_EZGOLIST_DELETE_DIALOGUES_DELETE_WORKINSTRUCTION_TEMPLATE";

            public const string ItemCounterLabel = @"CMS_ITEM_COUNTER_LABEL";

            public const string StageTemplate = @"CMS_STAGE_TEMPLATE_LABEL";
        }

        public static class Inbox
        {
            public const string NavBackTitle = @"CMS_INBOX_NAV_BACK_TITLE";
            public const string EmptyInboxMessage = @"CMS_INBOX_EMPTY_MESSAGE";
            public const string InboxMessage = @"CMS_INBOX_MESSAGE";
            public const string InboxTitle = @"CMS_INBOX_TITLE";
            public const string Reject = @"CMS_INBOX_REJECT";
            public const string Preview = @"CMS_INBOX_PREVIEW";
            public const string From = @"CMS_INBOX_FROM";
            public const string Shared = @"CMS_INBOX_SHARED";
            public const string SharedTemplateMessageBody = @"CMS_INBOX_MESSAGE_BODY";
            public const string SharedTemplateMessageGreeting = @"CMS_INBOX_MESSAGE_GREETING";
            public const string SharedTemplateMessageClosing = @"CMS_INBOX_MESSAGE_CLOSING";
        }

        public static class Info
        {
            public const string PossibleConnectionIssue = @"CMS_INFO_BANNER_POSSIBLE_CONNECTION_ISSUE";
        }

        public static class Language
        {
            public const string TranslationPlaceholder = @"CMS_LANGUAGE_TRANSLATION_PLACEHOLDER";
            public const string BtnEdit = @"CMS_LANGUAGE_BTN_EDIT";
            public const string BtnClose = @"CMS_LANGUAGE_BTN_CLOSE";
            public const string BtnSave = @"CMS_LANGUAGE_BTN_SAVE";
            public const string NewResourceKeyTitle = @"CMS_LANGUAGE_NEW_RESOURCE_KEY_TITLE";
            public const string InternalDescriptionOfThisKey = @"CMS_LANGUAGE_INTERNAL_DESCRIPTION_OF_THIS_KEY";
            public const string HeaderManageAppLanguages = @"CMS_LANGUAGE_HEADER_MANAGE_APP_LANGUAGES";
            public const string ExportTitle = @"CMS_LANGUAGE_EXPORT_TITLE";
            public const string ExportUpdateTitle = @"CMS_LANGUAGE_EXPORT_UPADTE_TITLE";
            public const string SearchPlaceholder = @"CMS_LANGUAGE_SEARCH_PLACEHOLDER";
        }

        public static class Exports
        {
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.DownloadAudits, "Download audits") ?? "Download audits")
            public const string DownloadAudits = @"CMS_EXPORTS_DOWNLOAD_AUDITS";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ChecklistAndAuditsTitle, "Checklists and audits") ?? "Checklists and audits")';// 'Checklists and audits';
            public const string ChecklistAndAuditsTitle = @"CMS_EXPORTS_CHECKLISTANDAUDITSTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ChecklistAndAuditTemplatesTitle, "Checklist- and audit-templates"); ?? "Checklist- and audit-templates")';//'Checklist- and audit-templates';
            public const string ChecklistAndAuditTemplatesTitle = @"CMS_EXPORTS_CHECKLISTANDAUDITTEMPLATESTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.TaskTemplatesTitle, "Task templates") ?? "Task templates")';//'Task templates';
            public const string TaskTemplatesTitle = @"CMS_EXPORTS_TASKTEMPLATESTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ExportAllActiveTaskTemplatesText, "Export all active tasktemplates") ?? "Export all active tasktemplates")';//'Export all active tasktemplates';
            public const string ExportAllActiveTaskTemplatesText = @"CMS_EXPORTS_EXPORTALLACTIVETASKTEMPLATESTEXT";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.TaskTitle, "Task tasks") ?? "Task tasks")';//'Task tasks';
            public const string TaskTitle = @"CMS_EXPORTS_TASKTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.TaskPropertiesTitle, "Value registration") ?? "Value registration")';// 'Value registration';
            public const string TaskPropertiesTitle = @"CMS_EXPORTS_TASKPROPERTIESTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.AuditTaskPropertiesTitle, "Value registration") ?? "Value registration")';//'Value registration';
            public const string AuditTaskPropertiesTitle = @"CMS_EXPORTS_AUDITTASKPROPERTIESTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ChecklistTaskPropertiesTitle, "Value registration") ?? "Value registration")';//'Value registration';
            public const string ChecklistTaskPropertiesTitle = @"CMS_EXPORTS_CHECKLISTTASKPROPERTIESTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ChecklistsTitle, "Checklists") ?? "Checklists")';//'Checklists';
            public const string ChecklistsTitle = @"CMS_EXPORTS_CHECKLISTSTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.AuditsTitle, "Audits") ?? "Audits")';//'Audits';
            public const string AuditsTitle = @"CMS_EXPORTS_AUDITSTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.AuditTemplatesTitle, "Audit templates") ?? "Audit templates")';//'Audit templates';
            public const string AuditTemplatesTitle = @"CMS_EXPORTS_AUDITTEMPLATESTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.AuditTemplatesText, "Export all active audittemplates") ?? "Export all active audittemplates")';//'Export all active audittemplates';
            public const string AuditTemplatesText = @"CMS_EXPORTS_AUDITTEMPLATESTEXT";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ChecklistsTemplateTitle, "Checklist templates") ?? "Checklist templates")';//'Checklist templates';
            public const string ChecklistsTemplateTitle = @"CMS_EXPORTS_CHECKLISTSTEMPLATETITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ChecklistsTemplateText, "Export all active checklisttemplates") ?? "Export all active checklisttemplates")';//'Export all active checklisttemplates';
            public const string ChecklistsTemplateText = @"CMS_EXPORTS_CHECKLISTSTEMPLATETEXT";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ChecklistsTemplateTitle, "Checklist templates") ?? "Checklist templates")';//'Checklist templates';
            public const string WorkInstructionTemplateTitle = @"CMS_EXPORTS_WORKINSTRUCTIONTEMPLATETITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ChecklistsTemplateText, "Export all active checklisttemplates") ?? "Export all active checklisttemplates")';//'Export all active checklisttemplates';
            public const string WorkInstructionTemplateText = @"CMS_EXPORTS_WORKINSTRUCTIONTEMPLATETEXT";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.LanguagesTitle, "Export translations") ?? "Export translations")';//'Export translations';
            public const string LanguagesTitle = @"CMS_EXPORTS_LANGUAGESTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.LanguagesText, "Export all language translations") ?? "Export all language translations")';//'Export all language translations';
            public const string LanguagesText = @"CMS_EXPORTS_LANGUAGESTEXT";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.LanguagesImportTitle, "Import translations") ?? "Import translations")';//'Import translations';
            public const string LanguagesImportTitle = @"CMS_EXPORTS_LANGUAGESIMPORTTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.LanguagesImportText, "Export language translations updates for import.") ?? "Export language translations updates for import.")';//'Export language translations updates for import.';
            public const string LanguagesImportText = @"CMS_EXPORTS_LANGUAGESIMPORTTEXT";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.CompanyTitle, "Company overview") ?? "Company overview")';//'Company overview';
            public const string CompanyTitle = @"CMS_EXPORTS_COMPANYTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ActionsTitle, "Actions") ?? "Actions")';//'Actions';
            public const string ActionsTitle = @"CMS_EXPORTS_ACTIONSTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ExportButtonTitle, "Export") ?? "Export")';//'Export';
            public const string ExportButtonTitle = @"CMS_EXPORTS_EXPORTBUTTONTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ValidationDateTitle, "Start date must be earlier than the end date") ?? "Start date must be earlier than the end date")';//'Start date must be earlier than the end date';
            public const string ValidationDateTitle = @"CMS_EXPORTS_VALIDATIONDATETITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.ValidationTimeTitle, "Start time must be earlier than the end date") ?? "Start time must be earlier than the end date")';//'Start time must be earlier than the end date';
            public const string ValidationTimeTitle = @"CMS_EXPORTS_VALIDATIONTIMETITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.BusyMessage, "Busy with getting your file...") ?? "Busy with getting your file...")';//'Busy with getting your file...';
            public const string BusyMessage = @"CMS_EXPORTS_BUSYMESSAGE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.DoneMessage, "Done!") ?? "Done!")';//'Done!';
            public const string DoneMessage = @"CMS_EXPORTS_DONEMESSAGE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.StartDateTitle, "Start date") ?? "Start date")';//'Start date';
            public const string StartDateTitle = @"CMS_EXPORTS_STARTDATETITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.EndDateTitle, "End date") ?? "End date")';//'End date';
            public const string EndDateTitle = @"CMS_EXPORTS_ENDDATETITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.SkillAssessmentsTitle, "Skill assessments") ?? "Skill assessments")';//'Skill assessments';
            public const string SkillAssessmentsTitle = @"CMS_EXPORTS_SKILLASSESSMENTSTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.SkillAssessmentTemplatesTitle, "Skill assessment templates") ?? "Skill assessment templates")';//'Skill assessment templates';
            public const string SkillAssessmentTemplatesTitle = @"CMS_EXPORTS_SKILLASSESSMENTTEMPLATESTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.SkillAssessmentTemplatesText, "Export all active assessmenttemplates") ?? "Export all active assessmenttemplates")';//'Export all active assessmenttemplates';
            public const string SkillAssessmentTemplatesText = @"CMS_EXPORTS_SKILLASSESSMENTTEMPLATESTEXT";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.CompanyAreasTitle, "Active Areas") ?? "Active Areas")';//'Active Areas';
            public const string CompanyAreasTitle = @"CMS_EXPORTS_AREASTITLE";

            //'@(Model?.CmsLanguage?.GetValue(LanguageKeys.Exports.CompanyAreasText, "Export all active areas") ?? "Export all active areas")';//'Export all active areas';
            public const string CompanyAreasText = @"CMS_EXPORTS_AREASTEXT";

        }
        public static class Setting
        {
            public const string WorkInstruction = "WORK_INSTRUCTION";
            public const string EnableNotificationsTitle = "ENABLE_NOTIFICATIONS_TITLE";
            public const string DisableNotificationsTitle = "DISABLE_NOTIFICATIONS_TITLE";
            public const string DisableNotificationsDescription = "Disable_NOTIFICATIONS_DESCRIPTION";
            public const string EnableNotificationsDescription = "ENABLE_NOTIFICATIONS_DESCRIPTION";

            public const string SkillMatrix = "SkillMatrix";
            public const string EnableSkillMatrixTitle = "ENABLE_SKILLMATRIX_TITLE";
            public const string DisableSkillMatrixTitle = "DISABLE_SKILLMATRIX_TITLE";
            public const string DisableSkillMatrixDescription = "Disable_SKILLMATRIX_DESCRIPTION";
            public const string EnableSkillMatrixDescription = "ENABLE_SKILLMATRIX_DESCRIPTION";
        }
        public static class Filters
        {
            public const string CardTitle = @"CMS_FILTER_CARD_TITLE";
            public const string AreaTitle = @"CMS_FILTER_AREA_TITLE";
            public const string RoleTitle = @"CMS_FILTER_ROLE_TITLE";
            public const string InstructionTitle = @"CMS_FILTER_INSTRUCTION_TITLE";
            public const string PhotoTitle = @"CMS_FILTER_PHOTO_TITLE";
            public const string ResetHover = @"CMS_FILTER_RESET_HOVER";
            public const string RecurrenceTitle = @"CMS_FILTER_RECCURENCE_TITLE";
            public const string RoleBasic = @"CMS_FILTER_ROLE_BASIC";
            public const string RoleShiftleader = @"CMS_FILTER_ROLE_SHIFTLEADER";
            public const string RoleManager = @"CMS_FILTER_ROLE_MANAGER";
            public const string OptionYes = @"CMS_FILTER_OPTION_YES";
            public const string OptionNo = @"CMS_FILTER_OPTION_NO";
            public const string OptionOnlyOnce = @"CMS_FILTER_OPTION_ONLY_ONCE";
            public const string OptionCertainShifts = @"CMS_FILTER_OPTION_CERTAIN_SHIFTS";
            public const string OptionDailyWeekly = @"CMS_FILTER_OPTION_DAILY_WEEKLY";
            public const string OptionMonthly = @"CMS_FILTER_OPTION_MONTHLY";
            public const string OptionDailyInterval = @"CMS_TASK_OPTION_PERIODDAY";
            public const string OptionDynamicDailyInterval = @"CMS_TASK_OPTION_DYNAMICDAY";

            public const string StatusLabel = @"CMS_CHECKLIST_STATUS_LABEL";

            public const string StatusIsCompleted = @"CMS_CHECKLIST_STATUS_IS_COMPLETED";
            public const string StatusIsOpen = @"CMS_CHECKLIST_STATUS_OPEN";

            public const string OngoingLabel = @"CMS_CHECKLIST_ONGOING_LABEL";

            public const string OptionVideoAttached = @"CMS_FILTER_OPTION_VIDEO_ATTACHED_TITLE";
            public const string OptionVideos = @"CMS_FILTER_OPTION_VIDEOS";

            public const string ActionStatusTitle = @"CMS_FILTER_ACTION_STATUS_TITLE";
            public const string StatusOptionResolved = @"CMS_FILTER_STATUS_OPTION_RESOLVED";
            public const string StatusOptionUnresolved = @"CMS_FILTER_STATUS_OPTION_UNRESOLVED";
            public const string StatusOptionOverdue = @"CMS_FILTER_STATUS_OPTION_OVERDUE";
            public const string StatusOptionUnviewedComments = @"CMS_FILTER_STATUS_OPTION_UNVIEWED_COMMENTS";

            public const string ActionInvolvementTitle = @"CMS_FILTER_ACTION_INVOLVED_TITLE";
            public const string InvolvementOptionAllActions = @"CMS_FILTER_ACTION_OPTION_ALLACTIONS";
            public const string InvolvementOptionMyActions = @"CMS_FILTER_ACTION_OPTION_MYACTIONS";
            public const string InvolvementOptionIamInvolvedIn = @"ACTIONS_SCREEN_I_AM_INVOLVED_IN";

            public const string InvolvementOptionAssignedToMe = @"CMS_FILTER_ACTION_OPTION_ASSIGNEDTOME";
            public const string ActionsStartByMe = @"ACTIONS_SCREEN_STARTED_BY_ME";

            //@(CmsLanguage?.GetValue(LanguageKeys.Filters.DateChoice, "Date") ?? "Date")
            public const string DateChoice = @"CMS_FILTER_REPORT_DATECHOICE";

            public const string UsersTitle = @"CMS_FILTER_USERS_TITLE";
            public const string AssessmentsTitle = @"CMS_FILTER_ASSESSMENTS_TITLE";
            public const string ScoreTitle = @"CMS_FILTER_SCORE_TITLE";
            public const string AssessorTitle = @"CMS_FILTER_ASSESSOR_TITLE";
            public const string AssesseeTitle = @"CMS_FILTER_ASSESSEE_TITLE";
            public const string TypeTitle = @"CMS_FILTER_TYPE_TITLE";
            public const string GroupTasksPerTitle = @"CMS_FILTER_GROUP_TASKS_PER_TITLE";
            public const string CreationDate = @"CMS_FILTER_CREATION_DATE";
            public const string ResolutionDate = "CMS_FILTER_RESOLUTIION_DATE";
            public const string DueDate = "CMS_FILTER_DUE_DATE";
            public const string OptionBasicInstruction = @"CMS_FILTER_OPTION_BASIC_INSTRUCTION";
            public const string OptionAssessmentInstruction = @"CMS_FILTER_OPTION_ASSESSMENT_INSTRUCTION";
            public const string OptionNoScore = @"CMS_FILTER_OPTION_NO_SCORE";
            public const string OptionScoreOne = @"CMS_FILTER_OPTION_SCORE_ONE";
            public const string OptionScoreTwo = @"CMS_FILTER_OPTION_SCORE_TWO";
            public const string OptionScoreThree = @"CMS_FILTER_OPTION_SCORE_THREE";
            public const string OptionScoreFour = @"CMS_FILTER_OPTION_SCORE_FOUR";
            public const string OptionScoreFive = @"CMS_FILTER_OPITON_SCORE_FIVE";
            public const string OptionPreviousDays = @"CMS_FILTER_OPTION_PREVIOUS_DAYS";
            public const string OptionPreviousWeeks = @"CMS_FILTER_OPTION_PREVIOUS_WEEKS";
            public const string OptionPreviousShifts = @"CMS_FILTER_OPTION_PREVIOUS_SHIFTS";
            public const string InstructionsTitle = @"CMS_FILTER_INSTRUCITONS_TITLE";

            public const string ActionType = @"CMS_FILTER_ACTION_TYPE";
            public const string ActionTypeAction = @"CMS_FILTER_ACTION_TYPE_OPTION_ACTION";
            public const string ActionTypeComment = @"CMS_FILTER_ACTION_TYPE_OPTION_COMMENT";
            public const string ApplyButton = "GENERAL_TEXT_APPLY";

            public const string CommentStatus = "CMS_FILTER_COMMENT_STATUS";
            public const string AllComments = "CMS_FILTER_ALLCOMMENTS";
            public const string FilterHasUnviewedComments = "CMS_FILTER_HAS_UNVIEWED_COMMENTS";
        }

        /// <summary>
        /// FrameWork language keys
        /// </summary>
        public static class FrameWork
        {
            public const string TopMenuDashboardTitle = @"CMS_FW_TOPMENU_DASHBOARD_TITLE";
            public const string TopMenuLogOff = @"CMS_FW_TOPMENU_LOGOFF";
            public const string MenuDashboardTitle = @"CMS_FW_MENU_DASHBOARD_TITLE";
            public const string MenuDashboardTitleHover = @"CMS_FW_MENU_DASHBOARD_HOVER";
            public const string MenuWorkInstructionTitle = @"CMS_FW_MENU_WORK_INSTRUCTION_TITLE";
            public const string MenuSkillsTitle = @"CMS_FW_MENU_SKILLS_TITLE";
            public const string MenuAssessmentsTitle = @"CMS_FW_MENU_ASSESSMENTS_TITLE";
            public const string MenuSkillMatricesTitle = @"CMS_FW_MENU_SKILL_MATRICES_TITLE";
            public const string MenuEZFeedTitle = @"CMS_FW_MENU_EZ_FEED_TITLE";
            public const string MenuChecklistTitle = @"CMS_FW_MENU_CHECKLIST_TITLE";
            public const string MenuChecklistTitleHover = @"CMS_FW_MENU_CHECKLIST_HOVER";
            public const string MenuTaskTitle = @"CMS_FW_MENU_TASK_TITLE";
            public const string MenuTaskTitleHover = @"CMS_FW_MENU_TASK_HOVER";
            public const string MenuAuditTitle = @"CMS_FW_MENU_AUDIT_TITLE";
            public const string MenuAuditTitleHover = @"CMS_FW_MENU_AUDIT_HOVER";
            public const string MenuActionTitle = @"CMS_FW_MENU_ACTION_TITLE";
            public const string MenuActionTitleHover = @"CMS_FW_MENU_ACTION_HOVER";
            public const string MenuUserTitle = @"CMS_FW_MENU_USER_TITLE";
            public const string MenuUserTitleHover = @"CMS_FW_MENU_USER_HOVER";
            public const string MenuConfigTitle = @"CMS_FW_MENU_CONFIG_TITLE";
            public const string MenuConfigTitleHover = @"CMS_FW_MENU_CONFIG_HOVER";
            public const string MenuLanguageTitle = @"CMS_FW_MENU_LANGUAGE_TITLE";
            public const string MenuAppLanguageTitle = @"CMS_FW_MENU_LANGUAGE_APP_TITLE";
            public const string MenuCmsLanguageTitle = @"CMS_FW_MENU_LANGUAGE_CMS_TITLE";
            public const string VersionTitle = @"CMS_FW_VERSION_TITLE";
            public const string CopyrightTitle = @"CMS_FW_COPYRIGHT_TITLE";
            public const string RightsTitle = @"CMS_FW_RIGHTS_TITLE";
            public const string PrivacyTitle = @"CMS_FW_PRIVACY_TITLE";
            public const string DocumentationTitle = @"CMS_FW_DOCUMENTATION_TITLE";
            public const string MessageDeleteInstruction = @"CMS_FW_MSG_DELETE_INSTRUCTION";
            public const string MessageDeleteTemplate = @"CMS_FW_MSG_DELETE_TEMPLATE";
            public const string MessageDialogYes = @"CMS_FW_MSG_DIALOG_YES";
            public const string MessageDialogNo = @"CMS_FW_MSG_DIALOG_NO";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.FrameWork.ActiveLanguage, "Active language") ?? "Active language")
            public const string ActiveLanguage = @"CMS_FW_ACTIVE_LANGUAGE_TEXT";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.FrameWork.NoTemplatesGeneralMessage, "No templates available") ?? "No templates available")</h2>
            public const string NoTemplatesGeneralMessage = @"CMS_FW_NOTEMPLATESGENERALMESSAGE_TEXT";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.FrameWork.NoTemplatesGeneralDetailMessage, "There are no templates added to this area yet.") ?? "There are no templates added to this area yet.")</p>
            public const string NoTemplatesGeneralDetailMessage = @"CMS_FW_NOTEMPLATESGENERALDETAILMESSAGE_TEXT";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.FrameWork.SessionExpiredTitle, "Your session has expired.") ?? "Your session has expired.")
            public const string SessionExpiredTitle = @"CMS_FW_SESSIONEXPIREDTITLE";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.FrameWork.SessionExpiredMessage, "Your current session has expired. Please login again.") ?? "Your current session has expired. Please login again.")
            public const string SessionExpiredMessage = @"CMS_FW_SESSIONEXPIREDMESSAGE";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.FrameWork.SessionExpiredButtonTitle, "Login") ?? "Login")
            public const string SessionExpiredButtonTitle = @"CMS_FW_SESSIONEXPIREDBUTTONTITLE";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.NoDataAvailable, "No data available.") ?? "No data available.")
            public const string NoDataAvailable = @"CMS_FW_NO_DATA_AVAILABLE";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.ThereIsNoDataAvailable, "There is no data available.") ?? "There is no data available.")
            public const string ThereIsNoDataAvailable = @"CMS_FW_THERE_IS_NO_DATA_AVAILABLE";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.ChooseLanguageTitle, "Choose language") ?? "Choose language")
            public const string ChooseLanguageTitle = @"CMS_FW_CHOOSE_LANGUAGE_TITLE";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.BrowserNoSupporForHTML5, "Your browser doesn't support HTML5 video tag.") ?? "Your browser doesn't support HTML5 video tag.")
            public const string BrowserNoSupporForHTML5 = @"CMS_FW_BROWSER_NO_SUPPORT_FOR_HTML5";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.FileSizeTooBig, "File size for this media file is too big!") ?? "File size for this media file is too big!")
            public const string FileSizeTooBig = @"CMS_FW_FILE_SIZE_TOO_BIG";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.Error, "Error.") ?? "Error.")
            public const string Error = @"CMS_FW_ERROR";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.AnErrorOccured, "An error occurred while processing your request.") ?? "An error occurred while processing your request.")
            public const string AnErrorOccured = @"CMS_FW_AN_ERROR_OCCURED";

            //(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.RequestId, "Request ID:") ?? "Request ID:")
            public const string RequestId = @"CMS_FW_REQUEST_ID";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.DevelopmentMode, "Development Mode") ?? "Development Mode")
            public const string DevelopmentMode = @"CMS_FW_DEVELOPMENT_MODE";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.SwappingTo, "Swapping to") ?? "Swapping to")
            public const string SwappingTo = @"CMS_FW_SWAPPING_TO";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.Development, "Development") ?? "Development")
            public const string Development = @"CMS_FW_DEVELOPMENT";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.EnviromentWillDisplayMore, "environment will display more detailed information about the error that occurred.") ?? "environment will display more detailed information about the error that occurred.")
            public const string EnviromentWillDisplayMore = @"CMS_FW_ENVIROMENT_WILL_DISPLAY_MORE";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.DevelopmentShouldntBeEnabled, "The Development environment shouldn't be enabled for deployed applications.") ?? "The Development environment shouldn't be enabled for deployed applications.")
            public const string DevelopmentShouldntBeEnabled = @"CMS_FW_DEVELOPMENT_SHOULDNT_BE_ENABLED";

            // @(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.ItCanResult, "It can result in displaying sensitive information from exceptions to end users.") ?? "It can result in displaying sensitive information from exceptions to end users.")
            public const string ItCanResult = @"CMS_FW_IT_CAN_RESULT";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.LocalDebugging, "For local debugging, enable the") ?? "For local debugging, enable the")
            public const string LocalDebugging = @"CMS_FW_LOCAL_DEBUGGING";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.EnviromentSetting, "environment by setting the") ?? "environment by setting the")
            public const string EnviromentSetting = @"CMS_FW_ENVIROMENT_SETTING";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.EnviromentVariable, "environment variable to") ?? "environment variable to")
            public const string EnviromentVariable = @"CMS_FW_ENVIROMENT_VARIABLE";

            //@(Model?.CmsLanguage.GetValue(LanguageKeys.FrameWork.RestartingApp, "and restarting the app.") ?? "and restarting the app.")
            public const string RestartingApp = @"CMS_FW_RESTARTING_APP";
        }

        public static class Login
        {
            public const string UserName = @"CMS_AUTH_LOGIN_USERNAME";
            public const string PassWord = @"CMS_AUTH_LOGIN_PASWORD";
            public const string PleaseSelect = @"CMS_AUTH_PLEASE_SELECT";
            public const string LoginButton = @"CMS_AUTH_LOGIN_BUTTON";
        }

        public static class Media
        {
            public const string AddMedia = @"CMS_MEDIA_ADD_MEDIA";
        }

        public static class MediaOptimize
        {
            public const string HeaderTitle = @"CMS_MEDIA_OPTIMIZE_HEADER_TITLE";
            public const string HeaderDescription = @"CMS_MEDIA_OPTIMIZE_HEADER_DESCRIPTION";
            public const string HeaderOptimizeAll = @"CMS_MEDIA_OPTIMIZE_HEADER_OPTIMIZEALL";
            public const string HeaderNotNow = @"CMS_MEDIA_OPTIMIZE_HEADER_NOTNOW";
            public const string HeaderSaveMBs = @"CMS_MEDIA_OPTIMIZE_HEADER_SAVEMBS";
            public const string ModalTitle = @"CMS_MEDIA_OPTIMIZE_MODAL_TITLE";
            public const string ModalHeader = @"CMS_MEDIA_OPTIMIZE_MODAL_HEADER";
            public const string ModalTemplateImage = @"CMS_MEDIA_OPTIMIZE_MODAL_TEMPLATEIMAGE";
            public const string ModalTemplateItemImage = @"CMS_MEDIA_OPTIMIZE_MODAL_TEMPLATEITEMIMAGE";
            public const string ModalInstructionItemImage = @"CMS_MEDIA_OPTIMIZE_MODAL_INSTRUCTIONITEMIMAGE";
            public const string ModalOptimizeButton = @"CMS_MEDIA_OPTIMIZE_MODAL_OPTIMIZEBUTTON";
            public const string BubbleHeader = @"CMS_MEDIA_OPTIMIZE_BUBBLE_HEADER";
            public const string BubbleDescription = @"CMS_MEDIA_OPTIMIZE_BUBBLE_DESCRIPTION";
            public const string BubbleDownloadButtonAlt = @"CMS_MEDIA_OPTIMIZE_BUBBLE_DOWNLOADBUTTON_ALT";
            public const string BubbleOptimizeButton = @"CMS_MEDIA_OPTIMIZE_BUBBLE_OPTIMIZEBUTTON";
        }

        public static class Pdf
        {
            public const string Step = @"CMS_PDF_GENERAL_STEP";
            public const string Steps = @"CMS_PDF_GENERAL_STEPS";
            public const string PageOf = @"CMS_PDF_PAGE_OF";
            public const string TotalPages = @"CMS_PDF_TOTAL_PAGES";
            public const string GeneratePDF = @"CMS_PDF_GENERATE_PDF";
            public const string Task = @"CMS_PDF_TASK";
            public const string Status = @"CMS_PDF_STATUS";
            public const string Remark = @"CMS_PDF_REMARK";

            public const string GeneratingSkillsMatrixImage = @"CMS_PDF_GENERATING_SKILLS_MATRIX_IMAGE";
            public const string DownloadGeneratedMatrixImage = @"CMS_PDF_DOWNLOAD_GENERATED_MATRIX_IMAGE";
        }

        public static class PdfAssessment
        {
            public const string TemplateId = @"CMS_PDF_ASSESSMENT_TEMPLATE_ID";
        }

        public static class PdfAssessmentTemplateFields
        {
            public const string Name = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELDS_NAME";
            public const string Description = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELDS_DESCRIPTION";
            public const string Picture = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELDS_PICTURE";
            public const string AreaPath = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELDS_AREA_PATH";
            public const string AreaId = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELDS_AREA_ID";
            public const string ModifiedBy = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELDS_MODIFIED_BY";
            public const string CreatedBy = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELDS_CREATED_BY";
            public const string ModifiedAt = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELDS_MODIFIED_AT";
            public const string CreatedAt = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELDS_CREATED_AT";
            public const string NumberOfSkillInstructions = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_NUMBER_OF_SKILL_INSTRUCITONS";
            public const string SignatureRequired = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_SIGNATURE_REQUIRED";
            public const string SignatureType = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_SIGNATURE_TYPE";
            public const string Role = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_ROLE";
            public const string TotalScore = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_TOTAL_SCORE";
            public const string Media = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_MEDIA";
            public const string AreaPathIds = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_AREA_PATH_IDS";
            public const string AssessmnetType = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_ASSESSMENT_TYPE";
            public const string CompanyId = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_COMPANY_ID";
            public const string Id = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_ID";
            public const string CreatedById = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_CREATED_BY_ID";
            public const string ModifiedById = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_MODIFIED_BY_ID";
            public const string NumberOfAssessments = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_NUMBER_OF_ASSESSMENTS";
            public const string NumberOfOpenAssessments = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_NUMBER_OF_OPEN_ASSESSMENTS";
            public const string LastActivityDate = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_LAST_ACTIVITY_DATE";
            public const string AssessmentTemplateId = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_ASSESSMENT_TEMPLATE_ID";
            public const string WorkInstructionTemplateId = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_WORK_INSTRUCTION_TEMPLATE_ID";
            public const string Index = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_INDEX";
            public const string WorkInstructionType = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_WORK_INSTRUCTION_TYPE";
            public const string NumberOfInstructionItems = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_NUMBER_OF_INSTRUCTION_ITEMS";
            public const string InstructionTemplateId = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_INSTRUCTION_TEMPLATE_ID";
            public const string CompletedAssessment = @"CMS_PDF_ASSESSMENT_TEMPLATE_FIELD_COMPLETED_ASSESSMENT";
        }

        public static class PdfAuditTemplate
        {
            public const string AuditTemplate = @"CMS_PDF_AUDIT_TEMPLATE_AUDIT_TEMPLATE";
            public const string Title = @"CMS_PDF_AUDIT_TITLE";
            public const string AuditId = @"CMS_PDF_AUDIT_TEMPLATE_OVERVIEW_AUDITID";
            public const string Role = @"CMS_PDF_AUDIT_TEMPLATE_OVERVIEW_ROLE";
            public const string Signatures = @"CMS_PDF_AUDIT_TEMPLATE_OVERVIEW_SIGNATURES";
            public const string ScoringSystem = @"CMS_PDF_AUDIT_TEMPLATE_OVERVIEW_SCORINGSYSTEM";
            public const string Areas = @"CMS_PDF_AUDIT_TEMPLATE_OVERVIEW_AREAS";
            public const string InformationTitle = @"CMS_PDF_AUDIT_TEMPLATE_INFORMATION_TITLE";
            public const string InformationProperties = @"CMS_PDF_AUDIT_TEMPLATE_INFORMATION_PROPERTIES";
            public const string InformationQuestionweight = @"CMS_PDF_AUDIT_TEMPLATE_INFORMATION_QUESTIONWEIGHT";
            public const string InformationInstructions = @"CMS_PDF_AUDIT_TEMPLATE_INFORMATION_INSTRUCTIONS";
            public const string OpenFieldsTitle = @"CMS_PDF_AUDIT_TEMPLATE_OPENFIELDS_TITLE";
            public const string OpenFieldsColumnHeader = @"CMS_PDF_AUDIT_TEMPLATE_OPENFIELDS_COLUMNHEADER";
            public const string OpenFieldsRequired = @"CMS_PDF_AUDIT_TEMPLATE_OPENFIELDS_REQUIRED";
            public const string Instructions = @"CMS_PDF_AUDIT_TEMPLATE_INSTRUCTIONS";
        }

        public static class PdfChecklistTemplate
        {
            public const string ChecklistTemplate = @"CMS_PDF_CHEKLIST_TEMPLATE_CHECKLIST_TEMPLATE";
            public const string ChecklistId = @"CMS_PDF_CHECKLIST_TEMPLATE_OVERVIEW_CHECKLISTID";
            public const string Role = @"CMS_PDF_CHECKLIST_TEMPLATE_OVERVIEW_ROLE";
            public const string Signatures = @"CMS_PDF_CHECKLIST_TEMPLATE_OVERVIEW_SIGNATURES";
            public const string Areas = @"CMS_PDF_CHECKLIST_TEMPLATE_OVERVIEW_AREAS";
            public const string InformationTitle = @"CMS_PDF_CHECKLIST_TEMPLATE_INFORMATION_TITLE";
            public const string InformationProperties = @"CMS_PDF_CHECKLIST_TEMPLATE_INFORMATION_PROPERTIES";
            public const string InformationInstructions = @"CMS_PDF_CHECKLIST_TEMPLATE_INFORMATION_INSTRUCTIONS";
            public const string OpenFieldsTitle = @"CMS_PDF_CHECKLIST_TEMPLATE_OPENFIELDS_TITLE";
            public const string OpenFieldsColumnHeader = @"CMS_PDF_CHECKLIST_TEMPLATE_OPENFIELDS_COLUMNHEADER";
            public const string OpenFieldsRequired = @"CMS_PDF_CHECKLIST_TEMPLATE_OPENFIELDS_REQUIRED";
            public const string Instructions = @"CMS_PDF_CHECKLSIT_TEMPLATE_INSTRUCITONS";
        }

        public static class PdfTaskTemplate
        {
            public const string TaskTemplate = @"CMS_PDF_TASK_TEMPLATE_TASK_TEMPLATE";
            public const string TaskId = @"CMS_PDF_TASK_TEMPLATE_OVERVIEW_TASKID";
            public const string Role = @"CMS_PDF_TASK_TEMPLATE_OVERVIEW_ROLE";
            public const string Connect = @"CMS_PDF_TASK_TEMPLATE_OVERVIEW_CONNECT";
            public const string Instructions = @"CMS_PDF_TASK_TEMPLATE_OVERVIEW_INSTRUCTIONS";
            public const string Areas = @"CMS_PDF_TASK_TEMPLATE_OVERVIEW_AREAS";
            public const string ValuePropertiesTitle = @"CMS_PDF_TASK_TEMPLATE_VALUEPROPERTIES_TITLE";
            public const string ValuePropertiesMachineStatus = @"CMS_PDF_TASK_TEMPLATE_VALUEPROPERTIES_MACHINESTATUS";
            public const string ValuePropertiesPlannedTime = @"CMS_PDF_TASK_TEMPLATE_VALUEPROPERTIES_PLANNEDTIME";
            public const string RecurrenceTitle = @"CMS_PDF_TASK_TEMPLATE_RECURRENCE_TITLE";
            public const string RecurrenceNone = @"CMS_PDF_TASK_TEMPLATE_RECURRENCE_TYPE_NONE";
            public const string RecurrenceShifts = @"CMS_PDF_TASK_TEMPLATE_RECURRENCE_TYPE_SHIFTS";
            public const string RecurrenceWeek = @"CMS_PDF_TASK_TEMPLATE_RECURRENCE_TYPE_WEEK";
            public const string RecurrenceMonth = @"CMS_PDF_TASK_TEMPLATE_RECURRENCE_TYPE_MONTH";
            public const string RecurrenceFixed = @"CMS_PDF_TASK_TEMPLATE_RECURRENCE_FIXED";
            public const string RecurrenceRecurring = @"CMS_PDF_TASK_TEMPLATE_RECURRENCE_RECURRING";
            public const string InstructionsTitle = @"CMS_PDF_TASK_TEMPLATE_INSTRUCTIONS_TITLE";
            public const string Unknown = @"CMS_PDF_TASK_TEMPLATE_UNKNOWN";
        }

        public static class PdfAuditCompleted
        {
            public const string Title = @"CMS_PDF_AUDIT_COMPLETED_TITLE";
            public const string Id = @"CMS_PDF_AUDIT_COMPLETED_ID";
            public const string AreaId = @"CMS_PDF_AUDIT_COMPLETED_AREA_ID";
            public const string CompnayId = @"CMS_PDF_AUDIT_COMPLETED_COMPANY_ID";
            public const string IsCompleted = @"CMS_PDF_AUDIT_COMPLETED_IS_COMPLATED";
            public const string TotalScore = @"CMS_PDF_AUDIT_COMPLETED_TOTAL_SCORE";
            public const string MinTaskScore = @"CMS_PDF_AUDIT_COMPLETED_MIN_TASK_SCORE";
            public const string MaxTaskScore = @"CMS_PDF_AUDIT_COMPLETED_MAX_TASK_SCORE";
            public const string TemplateId = @"CMS_PDF_AUDIT_COMPLETED_TEMPLATE_ID";
            public const string Name = @"CMS_PDF_AUDIT_COMPLETED_NAME";
            public const string Description = @"CMS_PDF_AUDIT_COMPLETED_DESCRIPTION";
            public const string Picture = @"CMS_PDF_AUDIT_COMPLETED_PICTURE";
            public const string ScoreType = @"CMS_PDF_AUDIT_COMPLETED_SCORE_TYPE";
            public const string IsDoubleSignatureRequired = @"CMS_PDF_AUDIT_COMPLETED_IS_DOUBLE_SIGNATURE_REQUIRED";
            public const string IsSignatureRequired = @"CMS_PDF_AUDIT_COMPLETED_IS_SIGNATURE_REQUIRED";
            public const string CreatedAt = @"CMS_PDF_AUDIT_COMPLETED_CREATED_AT";
            public const string ModifiedAt = @"CMS_PDF_AUDIT_COMPLETED_MODIFIED_AT";
            public const string AreaPath = @"CMS_PDF_AUDIT_COMPLETED_AREA_PATH";
            public const string AreaPathIds = @"CMS_PDF_AUDIT_COMPLETED_AREA_PATH_IDS";

            public const string PictureProofDate = @"CMS_PDF_AUDIT_COMPLETED_DATE";
            public const string PictureProofTakenBy = @"CMS_PDF_AUDIT_COMPLETED_TAKEN_BY";

            public const string Actions = @"CMS_PDF_AUDIT_COMPLETED_ACTIONS";
            public const string Comments = @"CMS_PDF_AUDIT_COMPLETED_COMMENTS";
            public const string Signatures = @"CMS_PDF_AUDIT_COMPLETED_SIGNATURES";

            public const string ActionComment = @"CMS_PDF_AUDIT_COMPLETED_ACTIONS_COMMENT";
            public const string ActionAction = @"CMS_PDF_AUDIT_COMPLETED_ACTIONS_ACTION";
            public const string ActionAuthor = @"CMS_PDF_AUDIT_COMPLETED_ACTIONS_AUTHOR";
            public const string ActionDate = @"CMS_PDF_AUDIT_COMPLETED_ACTIONS_DATE";
            public const string ActionModificationDate = @"CMS_PDF_AUDIT_COMPLETED_ACTIONS_MODIFICATION_DATE";
            public const string ActionDueDate = @"CMS_PDF_AUDIT_COMPLETED_ACTIONS_DUE_DATE";

            public const string CommentComment = @"CMS_PDF_AUDIT_COMPLETED_COMMENTS_COMMENT";
            public const string CommentAuthor = @"CMS_PDF_AUDIT_COMPLETED_COMMENTS_AUTHOR";
            public const string CommentDate = @"CMS_PDF_AUDIT_COMPLETED_COMMENTS_DATE";
            public const string CommentModificationDate = @"CMS_PDF_AUDIT_COMPLETED_COMMENTS_MODIFICATION_DATE";
        }

        public static class PdfChecklistCompleted
        {
            public const string Title = @"CMS_PDF_CHECKLIST_COMPLETED_TITLE";
            public const string Id = @"CMS_PDF_CHECKLIST_COMPLETED_ID";
            public const string CompanyId = @"CMS_PDF_CHECKLIST_COMPLETED_COMPANY_ID";
            public const string TemplateId = @"CMS_PDF_CHECKLIST_COMPLETED_TEMPLATE_ID";
            public const string IsCompleted = @"CMS_PDF_CHECKLIST_COMPLETED_IS_COMPLETED";
            public const string Name = @"CMS_PDF_CHECKLIST_COMPLETED_NAME";
            public const string Description = @"CMS_PDF_CHECKLIST_COMPLETED_DESCRIPTION";
            public const string Picture = @"CMS_PDF_CHECKLIST_COMPLETED_PICTURE";
            public const string IsDoubleSignatureRequired = @"CMS_PDF_CHECKLIST_COMPLETED_IS_DOUBLE_SIGNATURE_REQUIRED";
            public const string IsSignatureRequired = @"CMS_PDF_CHECKLIST_COMPLETED_IS_SIGNATURE_REQUIRED";
            public const string AreaId = @"CMS_PDF_CHECKLIST_COMPLETED_AREA_ID";
            public const string CreatedAt = @"CMS_PDF_CHECKLIST_COMPLETED_CREATED_AT";
            public const string ModifiedAt = @"CMS_PDF_CHECKLIST_COMPLETED_MODIFIED_AT";
            public const string AreaPath = @"CMS_PDF_CHECKLIST_COMPLETED_AREA_PATH";
            public const string AreaPathIds = @"CMS_PDF_CHECKLIST_COMPLETED_AREA_PATH_IDS";
            public const string PdfTitle = @"CMS_PDF_CHECKLIST_COMPLETED_PDF_TITLE";

            public const string PictureProofDate = @"CMS_PDF_CHECKLIST_COMPLETED_DATE";
            public const string PictureProofTakenBy = @"CMS_PDF_CHECKLIST_COMPLETED_TAKEN_BY";

            public const string Actions = @"CMS_PDF_CHECKLIST_COMPLETED_ACTIONS";
            public const string Comments = @"CMS_PDF_CHECKLIST_COMPLETED_COMMENTS";
            public const string Signatures = @"CMS_PDF_CHECKLIST_COMPLETED_SIGNATURES";

            public const string ActionComment = @"CMS_PDF_CHECKLIST_COMPLETED_ACTIONS_COMMENT";
            public const string ActionAction = @"CMS_PDF_CHECKLIST_COMPLETED_ACTIONS_ACTION";
            public const string ActionAuthor = @"CMS_PDF_CHECKLIST_COMPLETED_ACTIONS_AUTHOR";
            public const string ActionDate = @"CMS_PDF_CHECKLIST_COMPLETED_ACTIONS_DATE";
            public const string ActionModificationDate = @"CMS_PDF_CHECKLIST_COMPLETED_ACTIONS_MODIFICATION_DATE";
            public const string ActionDueDate = @"CMS_PDF_CHECKLIST_COMPLETED_ACTIONS_DUE_DATE";

            public const string CommentComment = @"CMS_PDF_CHECKLIST_COMPLETED_COMMENTS_COMMENT";
            public const string CommentAuthor = @"CMS_PDF_CHECKLIST_COMPLETED_COMMENTS_AUTHOR";
            public const string CommentDate = @"CMS_PDF_CHECKLIST_COMPLETED_COMMENTS_DATE";
            public const string CommentModificationDate = @"CMS_PDF_CHECKLIST_COMPLETED_COMMENTS_MODIFICATION_DATE";
        }

        //TODO Add language keys for workinstructiontemplate pdf
        public static class PdfWorkInstructionTemplate
        {
            public const string AuditId = @"CMS_PDF_AUDIT_TEMPLATE_OVERVIEW_AUDITID";
            public const string Role = @"CMS_PDF_AUDIT_TEMPLATE_OVERVIEW_ROLE";
            public const string Signatures = @"CMS_PDF_AUDIT_TEMPLATE_OVERVIEW_SIGNATURES";
            public const string ScoringSystem = @"CMS_PDF_AUDIT_TEMPLATE_OVERVIEW_SCORINGSYSTEM";
            public const string Areas = @"CMS_PDF_AUDIT_TEMPLATE_OVERVIEW_AREAS";
            public const string InformationTitle = @"CMS_PDF_AUDIT_TEMPLATE_INFORMATION_TITLE";
            public const string InformationProperties = @"CMS_PDF_AUDIT_TEMPLATE_INFORMATION_PROPERTIES";
            public const string InformationQuestionweight = @"CMS_PDF_AUDIT_TEMPLATE_INFORMATION_QUESTIONWEIGHT";
            public const string InformationInstructions = @"CMS_PDF_AUDIT_TEMPLATE_INFORMATION_INSTRUCTIONS";
            public const string OpenFieldsTitle = @"CMS_PDF_AUDIT_TEMPLATE_OPENFIELDS_TITLE";
            public const string OpenFieldsColumnHeader = @"CMS_PDF_AUDIT_TEMPLATE_OPENFIELDS_COLUMNHEADER";
            public const string OpenFieldsRequired = @"CMS_PDF_AUDIT_TEMPLATE_OPENFIELDS_REQUIRED";
            public const string WorkInstructionTemplateId = @"CMS_PDF_WORK_INSTRUCTION_TEMPLATE_WORK_INSTRUCITON_TEMPLATE_ID";
            public const string InstrucitonType = @"CMS_PDF_WORK_INSTRUCTION_TEMPLATE_INSTRUCTION_TYPE";
            public const string WorkInstruction = @"CMS_PDF_WORK_INSTRUCTION_TEMPLATE_WORK_INSTRUCTION";
            public const string AssessmentInstruction = @"CMS_PDF_WORK_INSTRUCTION_TEMPLATE_ASSESSMENT_INSTRUCTION";
        }

        public static class PictureProof
        {
            public const string PictureProofTitle = @"CMS_PICTUREPROOF_TITLE";
            public const string TakenAt = @"CMS_PICTUREPROOF_TAKENAT";
            public const string TakenBy = @"CMS_PICTUREPROOF_TAKENBY";
            public const string At = @"CMS_PICTUREPROOF_AT";
            public const string PictureProofItem = @"CMS_PICTUREPROOF_ITEM";
            public const string BelongsTo = @"CMS_PICTUREPROOF_BELONGSTO";
            public const string Enable = @"CMS_PICTUREPROOF_ENABLE";
            public const string Disable = @"CMS_PICTUREPROOF_DISABLE";
            public const string PictureProofForAllItems = @"CMS_PICTUREPROOF_ENABLEDISABLE_FORALLITEMS";
        }

        public static class RawViewer
        {
            public const string HeaderRawViewer = @"CMS_RAWVIEWER_HEADER_RAW_VIEWER";
            public const string OptionTasks = @"CMS_RAWVIEWER_OPTION_TASKS";
            public const string OptionAudits = @"CMS_RAWVIEWER_OPTION_AUDITS";
            public const string OptionChecklists = @"CMS_RAWVIEWER_OPTION_CHECKLISTS";
            public const string OptionAssessments = @"CMS_RAWVIEWER_OPTION_ASSESSMENTS";
            public const string OptionActions = @"CMS_RAWVIEWER_OPTION_ACTIONS";
            public const string OptionComments = @"CMS_RAWVIEWER_OPTION_COMMENTS";
            public const string OptionShifts = @"CMS_RAWVIEWER_OPTION_SHIFTS";
            public const string OptionLogAuditing = @"CMS_RAWVIEWER_OPTION_LOG_AUDITING";
            public const string HeaderScheduler = @"CMS_RAWVIEWER_HEADER_SCHEDULER";
            public const string HeaderSchedulerInUTC = @"CMS_RAWVIEWER_HEADER_SCHEDULER_IN_UTC";
            public const string MondayText = @"CMS_RAWVIEWER_MONDAY_TEXT";
            public const string TuesdayText = @"CMS_RAWVIEWER_TUESDAY_TEXT";
            public const string WednesdayText = @"CMS_RAWVIEWER_WEDNESDAY_TEXT";
            public const string ThursdayText = @"CMS_RAWVIEWER_THURSDAY_TEXT";
            public const string FridayText = @"CMS_RAWVIEWER_FRIDAY_TEXT";
            public const string SaturdayText = @"CMS_RAWVIEWER_SATURDAY_TEXT";
            public const string SundayText = @"CMS_RAWVIEWER_SUNDAY_TEXT";
            public const string HourTitle = @"CMS_RAWVIEWER_HOUR_TITLE";
            public const string BtnClose = @"CMS_RAWVIEWER_BTN_CLOSE";
            public const string SearchPlaceholder = @"CMS_RAWVIEWER_SEARCH_PLACEHOLDER";
            public const string ReportRetrieved = @"CMS_RAWVIEWER_REPORT_RETRIEVED";
        }

        public static class Reports
        {
            //(Model?.CmsLanguage?.GetValue(LanguageKeys.Reports.NoCompletedAuditsFound, "No completed audits found") ?? "No completed audits found")
            public const string NoCompletedAuditsFound = @"CMS_REPORT_NO_COMPLETED_AUDITS_FOUND";

            //<h4>@(Model?.CmsLanguage?.GetValue(LanguageKeys.Reports.NoCompletedChecklistFound, "No completed checklists found") ?? "No completed checklists found")</h4>
            public const string NoCompletedChecklistsFound = @"CMS_REPORT_NO_COMPLETED_CHECKLISTS_FOUND";
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Reports.LoadingChecklistTaskContent, "Loading...") ?? "Loading...")
            public const string LoadingChecklistTaskContent = @"CMS_REPORT_CHECKLISTS_LOADING_TEXT";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.Reports.NoCompletedTasksFound, "No completed tasks found") ?? "No completed tasks found")
            public const string NoCompletedTasksFound = @"CMS_REPORT_NO_COMPLETED_TASKS_FOUND";

            public const string LastCompletedChecklists = @"CMS_REPORT_COMPLETED_CHECKLISTS_LAST_COMPLETED_CHECKLISTS";
            public const string OpenChecklists = @"CMS_REPORT_COMPLETED_CHECKLISTS_OPEN_CHECKLISTS";
        }

        public static class OpenFields
        {
            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.OpenFields.OpenFieldAddEditTitle, "Add/edit open field") ?? "Add/edit open field")
            public const string OpenFieldAddEditTitle = @"CMS_OPENFIELDS_OPENFIELDADDEDITTITLE";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.OpenFields.SelectKindMessage, "Select kind of property") ?? "Select kind of property")
            public const string SelectKindMessage = @"CMS_OPENFIELDS_SELECTKINDMESSAGE";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.OpenFields.TextBasedTitle, "Text based property") ?? "Text based property")
            public const string TextBasedTitle = @"CMS_OPENFIELDS_TEXTBASEDTITLE";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.OpenFields.NameDisplayLabelText, "Name or display text") ?? "Name or display text")
            public const string NameDisplayLabelText = @"CMS_OPENFIELDS_NAMEDISPLAYLABELTEXT";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.OpenFields.IsRequiredText, "Is required") ?? "Is required")
            public const string IsRequiredText = @"CMS_OPENFIELDS_ISREQUIREDTEXT";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.OpenFields.DeleteButtonTitle, "Delete") ?? "Delete")
            public const string DeleteButtonTitle = @"CMS_OPENFIELDS_DELETEBUTTONTITLE";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.OpenFields.CloseButtonTitle, "Close") ?? "Close")
            public const string CloseButtonTitle = @"CMS_OPENFIELDS_CLOSEBUTTONTITLE";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.OpenFields.SaveButtonTitle, "Save and Close") ?? "Save and Close")
            public const string SaveButtonTitle = @"CMS_OPENFIELDS_SAVEBUTTONTITLE";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.OpenFields.SaveAndClodeButtonTitle, "Save") ?? "Save")
            public const string SaveAndClodeButtonTitle = @"CMS_OPENFIELDS_SAVEANDCLODEBUTTONTITLE";

            //@(Model?.CmsLanguage?.GetValue(LanguageKeys.FrameWork.PrivacyTitle, "Privacy") ?? "Privacy")
            public const string PropertyTypeTitle = @"CMS_OPENFIELDS_PROPERTYTYPETITLE";
        }

        public static class Tags
        {
            public const string TagsMainTitle = @"CMS_TAGS_MAIN_TITLE";
            public const string TagsTagsFiltering = @"CMS_TAGS_FILTER_TAGS_FILTERING";
            public const string TagsTagManagement = @"CMS_TAGS_TAGS_CONFIGURATION_TAGS_MANAGEMENT";
            public const string TagsTagConfiguration = @"CMS_TAGS_TAGS_CONFIGURATION_TAGS_CONFIGURATION";
            public const string TagsTagConfigurationDelete = @"CMS_TAGS_TAGS_CONFIGURATION_DELETE";
            public const string TagsTagConfigurationTagGroups = @"CMS_TAGS_TAGS_CONFIGURATION_TAG_GROUPS";
            public const string TagsTagConfigurationSelectionCheckboxInfo = @"CMS_TAGS_TAGS_CONFIGURATION_TAG_SELECTION_CHECKBOX_INFO";
            public const string TagsTagConfigurationHoldingTagCheckboxText = @"CMS_TAGS_TAGS_CONFIGURATION_HOLDING_TAG_CHECKBOX_TEXT";
            public const string TagsTagConfigurationAddTag = @"CMS_TAGS_TAGS_CONFIGURATION_ADD_TAG";
            public const string TagsTagConfigurationMaximum30Characters = @"CMS_TAGS_TAGS_CONFIGURATION_MAXIMUM_30_CHARACTERS";
            public const string TagsTagConfigurationFeatures = @"CMS_TAGS_TAGS_CONFIGURATION_FEATURES";
            public const string TagsTagConfigurationMaxNumberOfTagGroups = @"CMS_TAGS_TAGS_CONFIGURATION_MAXIMUM_NUMBER_OF_TAG_GROUPS";
            public const string TagsTagConfigurationMaxNumberOfTags = @"CMS_TAGS_TAGS_CONFIGURATION_MAXIMUM_NUMBER_OF_TAGS";
            public const string TagsTagConfigurationMaxNumberOfHoldingTags = @"CMS_TAGS_TAGS_CONFIGURATION_MAXIMUM_NUMBER_OF_HOLDING_TAGS";
            public const string TagsTagConfigurationThisIsATagFromTheHolding = @"CMS_TAGS_TAGS_CONFIGURATION_THIS_IS_A_TAG_FROM_THE_HOLDING";
            public const string TagsTagConfigurationThisIsAGroupTag = @"CMS_TAGS_TAGS_CONFIGURATION_THIS_IS_A_GROUP_TAG";
            public const string TagsTagConfigurationTagName = @"CMS_TAGS_TAGS_CONFIGURATION_TAG_NAME";
            public const string TagsTagConfigurationTranslate = @"CMS_TAGS_TAG_CONFIGURATION_TRANSLATE";
            public const string TagsTagConfigurationCantDeleteTagInUse = @"CMS_TAGS_TAGS_CONFIGURATION_CANT_DELETE_TAG_IN_USE";
            public const string TagsTagConfigurationCantDeleteSharedHoldingTag = @"CMS_TAGS_TAGS_CONFIGURATION_CANT_DELETE_SHARED_HOLDING_TAG";
            public const string TagsTagConfigurationCantStopSharingWhileTagInUse = @"CMS_TAGS_TAGS_CONFIGURATION_CANT_STOP_SHARING_WHILE_TAG_IN_USE";
            public const string TagsTagConfigurationReachedLimit = @"CMS_TAGS_TAGS_CONFIGURATION_REACHED_LIMIT";
            public const string TagsTagConfigurationUnableToDisableTag = @"CMS_TAGS_TAGS_CONFIGURATION_UNABLE_TO_DISABLE_TAG";
            public const string TagsTagConfigurationWhileUsedInTemplate = @"CMS_TAGS_TAGS_CONFIGURATION_WHILE_USED_IN_TEMPLATE";
            public const string TagsTagConfigurationCantDisableGroupsInUse = @"CMS_TAGS_TAGS_CONFIGURATION_CANT_DISABLE_GROUPS_IN_USE";
            public const string TagsTagConfigurationMaximumNumberOfGroupsOrTagsReached = @"CMS_TAGS_TAGS_CONFIGURATION_MAXIMUM_NUMBER_OF_GROUPS_OR_TAGS_REACHED";
            public const string TagsTagConfigurationEditTagInGroup = @"CMS_TAGS_TAGS_CONFIGURATION_EDIT_TAG_IN_GROUP";
            public const string TagsTagConfigurationTagColor = @"CMS_TAGS_TAGS_CONFIGURATION_TAG_COLOR";
            public const string TagsTagConfigurationAddTagToGroup = @"CMS_TAGS_TAGS_CONFIGURATION_ADD_TAG_TO_GROUP";
            public const string TagsHoldingTag = @"CMS_TAGS_HOLDING_TAG";
        }

        public static class Task
        {
            public const string TaskTemplateLabel = @"CMS_TASK_TEMPLATE_LABEL";
            public const string TasksLabel = @"CMS_TASKS_LABEL";
            public const string OverviewTitle = @"CMS_TASK_OVERVIEW_TITLE";
            public const string OverviewListTitle = @"CMS_TASK_OVERVIEW_LIST_TITLE";
            public const string ExportTemplates = @"CMS_TASK_EXPORT_TEMPLATES";
            public const string ExportTemplatesDesc = @"CMS_TASK_EXPORT_TEMPLATES_DESC";
            public const string ExportTitle = @"CMS_TASK_EXPORT_TITLE";
            public const string ExportProgress = @"CMS_TASK_EXPORT_PROGRESS";
            public const string ExportStatus = @"CMS_TASK_EXPORT_STATUS";
            public const string ExportStartDate = @"CMS_TASK_EXPORT_STARTDATE";
            public const string ExportEndDate = @"CMS_TASK_EXPORT_ENDDATE";
            public const string DownloadTasks = @"CMS_TASKS_DOWNLOAD_TASKS";
            public const string DownloadValueRegistration = @"CMS_TASKS_DOWNLOAD_VALUEREGISTRATION";
            public const string Search = @"CMS_TASK_SEARCH";
            public const string NavBackTitle = @"CMS_TASK_NAV_BACK_TITLE";
            public const string Title = @"CMS_TASK_TITLE";
            public const string TaskId = @"CMS_TASK_TASK_ID";
            public const string RoleTitle = @"CMS_TASK_SELECT_ROLE_TITLE";
            public const string ConnectTitle = @"CMS_TASK_CONNECT_TITLE";
            public const string DeepLinkIsRequired = @"CMS_TASK_DEEPLINK_IS_REQUIRED";
            public const string OptionNoConnect = @"CMS_TASK_OPTION_NO_CONNECT";
            public const string OptionConnectAudit = @"CMS_TASK_OPTION_CONNECT_AUDIT";
            public const string OptionConnectChecklist = @"CMS_TASK_OPTION_CONNECT_CHECKLIST";
            public const string OptionBasic = @"CMS_TASK_OPTION_BASIC";
            public const string OptionShiftLeader = @"CMS_TASK_OPTION_SHIFTLEADER";
            public const string OptionManager = @"CMS_TASK_OPTION_MANAGER";
            public const string ConnectSearchPlaceholder = @"CMS_TASK_CONNECT_SEARCH_PLACEHOLDER";
            public const string MachineStatusTitle = @"CMS_TASK_MACHINE_STATUS_TITLE";
            public const string OptionNotApplicable = @"CMS_TASK_OPTION_NOT_APPLICABLE";
            public const string OptionStopped = @"CMS_TASK_OPTION_STOPPED";
            public const string OptionRunning = @"CMS_TASK_OPTION_RUNNING";
            public const string ItemTitlePlaceholder = @"CMS_TASK_ITEM_TITLE_PLACEHOLDER";
            public const string ItemDescPlaceholder = @"CMS_TASK_ITEM_DESCRIPTION_PLACEHOLDER";
            public const string BtnAddProperty = @"CMS_TASK_BTN_ADD_PROPERTY";
            public const string PropertyDescTitle = @"CMS_TASK_PROPERTY_DESC_TITLE";
            public const string DisplayFormat = @"CMS_TASK_DISPLAY_FORMAT";
            public const string DialogAddProperty = @"CMS_TASK_DIALOG_ADD_PROPERTY";
            public const string InstructionDialogTitle = @"CMS_TASK_INSTRUCTION_DIALOG_TITLE";
            public const string AddInstruction = @"CMS_TASK_ADD_INSTRUCTION";
            public const string AddWorkInstruction = @"CMS_TASK_ADD_WORKINSTRUCTION";
            public const string InstructionTitle = @"CMS_TASK_INSTRUCTION_TITLE";
            public const string IndexNrTitle = @"CMS_TASK_INDEX_NR_TITLE";
            public const string InstructionDelete = @"CMS_TASK_INSTRUCTION_DELETE";
            public const string DialogPrevStep = @"CMS_TASK_DIALOG_PREV_STEP";
            public const string DialogClose = @"CMS_TASK_DIALOG_CLOSE";
            public const string DialogNextStep = @"CMS_TASK_DIALOG_NEXT_STEP";
            public const string DialogAddStep = @"CMS_TASK_DIALOG_ADD_STEP";
            public const string DialogPrevItem = @"CMS_TASK_DIALOG_PREVIOUS_ITEM";
            public const string DialogNextItem = @"CMS_TASK_DIALOG_NEXT_ITEM";
            public const string DialogDelete = @"CMS_TASK_DIALOG_DELETE";
            public const string AreaTitle = @"CMS_TASK_AREA_TITLE";
            public const string BtnClose = @"CMS_TASK_BTN_CLOSE_TITLE";
            public const string BtnPrint = @"CMS_TASK_BTN_PRINT_TITLE";
            public const string BtnDuplicate = @"CMS_TASK_BTN_DUPLICATE_TITLE";
            public const string BtnDelete = @"CMS_TASK_BTN_DELETE_TITLE";
            public const string BtnSave = @"CMS_TASK_BTN_SAVE_TITLE";
            public const string BtnVisibleItems = @"CMS_TASK_BTN_VISIBLE_ITEMS_TITLE";
            public const string BtnAllItems = @"CMS_TASK_BTN_ALL_ITEMS_TITLE";
            public const string InstructionAddItem = @"CMS_TASK_INSTRUCTION_ADD_ITEM";
            public const string InstructionAddItemLower = @"CMS_TASK_INSTRUCTION_ADD_ITEM_LOWER";
            public const string InstructionNextItem = @"CMS_TASK_INSTRUCTION_NEXT_ITEM";
            public const string TemplateId = @"CMS_TASK_TEMPLATE_ID";
            public const string TemplateLinkTitle = @"CMS_TASK_TEMPLATE_LINK_TITLE";
            public const string TemplateRoleTitle = @"CMS_TASK_TEMPLATE_ROLE_TITLE";
            public const string TemplateRecurrenceTitle = @"CMS_TASK_TEMPLATE_RECURRENCE_TITLE";
            public const string TemplateChecklistTitle = @"CMS_TASK_TEMPLATE_CHECKLIST_TITLE";
            public const string TemplateAuditTitle = @"CMS_TASK_TEMPLATE_AUDIT_TITLE";
            public const string TemplateNorecurrencyTitle = @"CMS_TASK_TEMPLATE_NO_RECURRENCY_TITLE";
            public const string PlannedTimeTitle = @"CMS_TASK_PLANNED_TIME_TITLE";
            public const string PlannedTimeMinutesTitle = @"CMS_TASK_PLANNED_TIME_MINUTES_TITLE";
            public const string OptionNoRecurrency = @"CMS_TASK_OPTION_NO_RECURRENCY";
            public const string OptionWeek = @"CMS_TASK_OPTION_WEEK";
            public const string OptionMonth = @"CMS_TASK_OPTION_MONTH";
            public const string OptionShifts = @"CMS_TASK_OPTION_SHIFTS";
            public const string OptionPeriodDay = @"CMS_TASK_OPTION_PERIODDAY";
            public const string OptionDynamicDay = @"CMS_TASK_OPTION_DYNAMICDAY";
            public const string OptionRecurring = @"CMS_TASK_OPTION_RECURRING";
            public const string OptionFixed = @"CMS_TASK_OPTION_FIXED";
            public const string OptionFixedFrom = @"CMS_TASK_OPTION_FIXEED_FROM";
            public const string OptionFixedTill = @"CMS_TASK_OPTION_FIXED_TILL";
            public const string OptionWeekRecurEvery = @"CMS_TASK_OPTION_WEEK_RECUR_EVERY_TITLE";
            public const string OptionDynamicDayRecurAfter = @"CMS_TASK_OPTION_DYNAMICDAY_RECUR_AFTER_TITLE";
            public const string OptionWeekWeekon = @"CMS_TASK_OPTION_WEEK_WEEKON_TITLE";
            public const string OptionMonday = @"CMS_TASK_OPTION_MONDAY";
            public const string OptionTuesday = @"CMS_TASK_OPTION_TUESDAY";
            public const string OptionWednesday = @"CMS_TASK_OPTION_WEDNESDAY";
            public const string OptionThursday = @"CMS_TASK_OPTION_THURSDAY";
            public const string OptionFriday = @"CMS_TASK_OPTION_FRIDAY";
            public const string OptionSaturday = @"CMS_TASK_OPTION_SATURDAY";
            public const string OptionSunday = @"CMS_TASK_OPTION_SUNDAY";
            public const string OptionUnknownDay = @"CMS_TASK_OPTION_UNKNOWN_DAY";
            public const string OptionDays = @"CMS_TASK_OPTION_DAYS";
            public const string RecurrenceOnceFromdate = @"CMS_TASK_RECURRENCE_ONCE_FROMDATE";
            public const string MonthSpecificDate = @"CMS_TASK_MONTH_SPECIFIC_DATE_TITLE";
            public const string MonthDayOfEvery = @"CMS_TASK_MONTH_DAYOFEVERY_TITLE";
            public const string MonthSpecificWeek = @"CMS_TASK_MONTH_SPECIFIC_WEEK_TITLE";
            public const string OptionMonthFirst = @"CMS_TASK_OPTION_MONTH_FIRST";
            public const string OptionMonthSecond = @"CMS_TASK_OPTION_MONTH_SECOND";
            public const string OptionMonthThird = @"CMS_TASK_OPTION_MONTH_THIRD";
            public const string OptionMonthFourth = @"CMS_TASK_OPTION_MONTH_FOURTH";
            public const string OptionMonthEvery = @"CMS_TASK_MONTH_EVERY_TITLE";
            public const string OptionMonthOfEvery = @"CMS_TASK_MONTH_OFEVERY_TITLE";
            public const string MonthMonths = @"CMS_TASK_MONTH_MONTHS_TITLE";
            public const string TemplateAttachmentTitle = @"CMS_TASK_TEMPLATE_ATTACHMENT_TITLE";
            public const string ValuePropertiesTitle = @"CMS_TASK_VALUE_PROPERTIES_TITLE";
            public const string ItemAddPdf = @"CMS_TASK_ITEM_ADD_PDF";
            public const string ChangeTaskSortTitle = @"CMS_TASK_CHANGE_TASK_SORT_TITLE";
            public const string SaveIndicesTitle = @"CMS_TASK_SAVE_INDICES_TITLE";
            public const string ReindexIndicesVisibleTitle = @"CMS_TASK_REINDEX_INDICES_VISIBLE_TITLE";
            public const string ReindexIndicesAllTitle = @"CMS_TASK_REINDEX_INDICES_ALL_TITLE";
            public const string AvailableWorkInstrucitons = @"CMS_TASK_AVAILABLE_WORK_INSTRUCTIONS";
            public const string TaskDetailsTitle = @"CMS_TASK_TASK_DETAILS_TITLE";
            public const string TasksTitle = @"CMS_TASK_TASKS_TITLE";
            public const string ChangeOrder = @"CMS_TASK_CHANGE_ORDER";
            public const string AllShifts = @"CMS_TASK_ALL_SHIFTS";

            public const string ItemAddLink = @"CMS_TASK_ITEM_ADD_LINK";

            public const string ItemAddLinkModalTitle = @"CMS_TASK_ITEM_ADD_LINK_MODAL_TITLE";

            public const string ItemAddLinkModalInsertLinkHere = @"CMS_TASK_ITEM_ADD_LINK_MODAL_INSERT_LINK_HERE";
            public const string ItemAddLinkModalValidationError = @"CMS_TASK_ITEM_ADD_LINK_MODAL_VALIDATION_ERROR";
            public const string ItemAddLinkModalSaveChanges = @"CMS_TASK_ITEM_ADD_LINK_MODAL_SAVE_CHANGES";
            public const string ItemAddLinkModalClose = @"CMS_TASK_ITEM_ADD_LINK_MODAL_CLOSE";

            public const string ConfirmationLeaveMessage = @"CMS_TASK_ConfirmationLeaveMessage";

            public const string CompletedTaskLoaderStats = @"CMS_COMPLETED_TASKS_LOADER_STATS";
            public const string CompletedTaskLoaderTasks = @"CMS_COMPLETED_TASKS_LOADER_TASKS";

            public const string ReasonForSkippingThisTask = @"CMS_COMPLETED_TASKS_REASON_FOR_SKIPPING_THIS_TASK";
            public const string TaskGenerationTitle = "CMS_TASK_GENERATIONTITLE";

        }

        public static class User
        {
            public const string OverviewTitle = @"CMS_USER_OVERVIEW_TITLE";
            public const string OverviewListTitle = @"CMS_USER_OVERVIEW_LIST_TITLE";
            public const string Manager = @"CMS_USER_OVERVIEW_ROLE_MANAGER_TITLE";
            public const string Basic = @"CMS_USER_OVERVIEW_ROLE_BASIC_TITLE";
            public const string ShiftLeader = @"CMS_USER_OVERVIEW_ROLE_SHIFTLEADER_TITLE";
            public const string ButtonAddUser = @"CMS_USER_OVERVIEW_BTN_ADDUSER";
            public const string ListSearchTitle = @"CMS_USER_OVERVIEW_SEARCH_PLACEHOLDER";
            public const string NavigateBack = @"CMS_USER_NAV_BACK_TITLE";
            public const string UserProfileTitle = @"CMS_USER_PROFILE_TITLE";
            public const string FirstName = @"CMS_USER_PROFILE_FIRSTNAME_TITLE";
            public const string LastName = @"CMS_USER_PROFILE_LASTNAME_TITLE";
            public const string Email = @"CMS_USER_PROFILE_EMAIL_TITLE";
            public const string UPN = @"CMS_USER_PROFILE_UPN_TITLE";
            public const string UserName = @"CMS_USER_PROFILE_USERNAME_TITLE";
            public const string AccessLevelTitle = @"CMS_USER_PROFILE_ACCESSLEVEL_TITLE";
            public const string ChangePassword = @"CMS_USER_PROFILE_BTN_CHANGE_PASSWORD";
            public const string Delete = @"CMS_USER_PROFILE_BTN_DELETE";
            public const string Save = @"CMS_USER_PROFILE_BTN_SAVE";
            public const string AddAnotherUser = @"CMS_USER_PROFILE_BTN_ADD_ANOTHER_USER";
            public const string DialogTitle = @"CMS_USER_DIALOG_TITLE";
            public const string DialogPasswordPlaceholder = @"CMS_USER_DIALOG_PASSWORD_PLACEHOLDER";
            public const string DialogConfirmPasswordPlaceholder = @"CMS_USER_DIALOG_CONFIRM_PASSWORD_PLACEHOLDER";
            public const string DialogPasswordCancel = @"CMS_USER_DIALOG_PASSWORD_BTN_CANCEL";
            public const string DialogPasswordSave = @"CMS_USER_DIALOG_PASSWORD_BTN_SAVE";
            public const string ProfileChangeButton = @"CMS_USER_PROFILE_BTN_CHANGE_ALLOWED_AREAS";
            public const string DialogAreasTitle = @"CMS_USER_DIALOG_AREAS_TITLE";
            public const string ProfileDeleteConfirm = @"CMS_USER_PROFILE_DELETE_CONFIRM_MESSAGE";
            public const string ProfileDeleteMessage = @"CMS_USER_PROFILE_DELETE_MESSAGE";
            public const string ProfileDeleteMessageTitle = @"CMS_USER_PROFILE_DELETE_TITLE";
            public const string ProfileDeleteMessageUndone = @"CMS_USER_PROFILE_DELETE_UNDONE";
            public const string ProfileDeleteSuccessorMessage = @"CMS_USER_PROFILE_DELETE_SUCCESSOR_MESSAGE";
            public const string UserAreaSearchMessage = @"CMS_USER_AREA_SEARCH_TITLE_MESSAGE";
            public const string UserAreaSearchResetMessage = @"CMS_USER_AREA_SEARCH_RESET_TITLE_MESSAGE";
            public const string SelectSuccessor = @"CMS_USER_SELECT_SUCCESSOR";
            public const string RecentChangesByUser = @"CMS_USER_RECENT_CHANGES_BY";
            public const string UserId = @"CMS_USER_USER_ID";
            public const string LoadMoreAuditing = @"CMS_USER_LOAD_MORE_AUDITING";


            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserSavedDialog, "User saved.") ?? "User saved.")
            public const string UserSavedDialog = @"CMS_USER_SAVED_DIALOG";

            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserSavedDialogReload, "User saved..reloading.") ?? "User saved..reloading.")
            public const string UserSavedDialogReload = @"CMS_USER_SAVED_DIALOG_RELOAD";

            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserPasswordSaveDialog, "Password saved.") ?? "Password saved.")
            public const string UserPasswordSaveDialog = @"CMS_USER_PASSWORD_SAVED_DIALOG";

            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserValidationDialogRoleRequired, "Role is a required field.") ?? "Role is a required field.")
            public const string UserValidationDialogRoleRequired = @"CMS_USER_VALIDATION_DIALOG_ROLE_REQUIRED";

            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserValidationDialogUserNameRequired, "User name is a required field.") ?? "User name is a required field.")
            public const string UserValidationDialogUserNameRequired = @"CMS_USER_VALIDATION_DIALOG_USER_NAME_REQUIRED";

            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserValidationDialogLastNameRequired, "Last name is a required field.") ?? "Last name is a required field.")
            public const string UserValidationDialogLastNameRequired = @"CMS_USER_VALIDATION_DIALOG_LAST_NAME_REQUIRED";

            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserValidationDialogAreasRequired, "Areas are required.") ?? "Areas are required.")
            public const string UserValidationDialogAreasRequired = @"CMS_USER_VALIDATION_DIALOG_AREAS_REQUIRED";

            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserValidationDialogPasswordRequired, "Password name is a required field.") ?? "Password name is a required field.")
            public const string UserValidationDialogPasswordRequired = @"CMS_USER_VALIDATION_DIALOG_PASSWORD_REQUIRED";

            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserValidationDialogPasswordConfirmationRequired, "Password confirmation name is a required field.") ?? "Password confirmation name is a required field.")
            public const string UserValidationDialogPasswordConfirmationRequired = @"CMS_USER_VALIDATION_DIALOG_PASSWORD_CONFIRMATION_REQUIRED";

            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserValidationDialogPasswordPasswordConfirmationRequired, "Password and password confirmation can not be different.") ?? "Password and password confirmation can not be different.")
            public const string UserValidationDialogPasswordPasswordConfirmationRequired = @"CMS_USER_VALIDATION_DIALOG_PASSWORD_PASSWORD_CONFIRMATION_REQUIRED";

            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserValidationDialogPasswordValidationRules, "Password must contain at least 6 character, uppercase characters, lowercase characters and a number.") ?? "Password must contain at least 6 character, uppercase characters, lowercase characters and a number.")
            public const string UserValidationDialogPasswordValidationRules = @"CMS_USER_VALIDATION_DIALOG_PASSWORD_VALIDATION_RULES";

            //@(Model.CmsLanguage?.GetValue(LanguageKeys.User.UserDeleteDialogReload, "User deleted..reloading.") ?? "User deleted..reloading.")
            public const string UserDeleteDialogReload = @"CMS_USER_DELETE_DIALOG_RELOAD";

            public const string UserExtendedDataExtendedDetails = @"CMS_USER_EXTENDED_DATA_EXTENDED_DETAILS";
            public const string UserExtendedDataDescription = @"CMS_USER_EXTENDED_DATA_DESCRIPTION";
            public const string UserExtendedDataEmployeeIdentifierCode = @"CMS_USER_EXTENDED_DATA_EMPLOYEE_IDENTIFIER_CODE";
            public const string UserExtendedDataEmployeeFunction = @"CMS_USER_EXTENDED_DATA_EMPLOYEE_FUNCTION";


        }

        public static class WorkInsctruction
        {
            public const string WorkInsctructionLabel = @"CMS_WORK_INSTRUCTION_LABEL";
            public const string WorkInsctructionsLabel = @"CMS_WORK_INSTRUCTIONS_LABEL";
            public const string OverViewTitle = @"CMS_WORK_INSTRUCTION_OVERVIEW_TITLE";
            public const string OverViewTitleDetails = @"CMS_WORK_INSTRUCTION_OVERVIEW_TITLE_DETAILS";
            public const string OverviewListTitle = @"CMS_WORK_INSTRUCTION_OVERVIEW_LIST_TITLE";
            public const string NavBackTitle = @"CMS_WORK_INSTRUCTION_NAV_BACK_TITLE";
            public const string BtnSave = @"CMS_WORK_INSTRUCTION_BTN_SAVE_TITLE";
            public const string BtnDelete = @"CMS_WORK_INSTRUCTION_BTN_DELETE_TITLE";
            public const string BtnDuplicate = @"CMS_WORK_INSTRUCTION_BTN_DUPLICATE_TITLE";
            public const string WorkInctructionId = @"CMS_WORK_INSTRUCTION_WORK_INSTRUCTION_ID";
            public const string SelectTypeTitle = @"CMS_WORK_INSTRUCTION_SELECT_TYPE_TITLE";
            public const string SelectTypeDisabled = @"CMS_WORK_INSTRUCTION_SELECT_TYPE_TITLE_DISABLED";
            public const string OptionWorkinstruction = @"CMS_WORK_INSTRUCTION_OPTION_WORKINSTRUCTION";
            public const string OptionAssessmentinstruction = @"CMS_WORK_INSTRUCTION_OPTION_ASSESSMENTINSTRUCTION";
            public const string SelectRoleTitle = @"CMS_WORK_INSTRUCTION_SELECT_ROLE_TITLE";
            public const string OptionBasic = @"CMS_WORK_INSTRUCTION_OPTION_BASIC";
            public const string OptionShiftLeader = @"CMS_WORK_INSTRUCTION_OPTION_SHIFT_LEADER";
            public const string OptionManager = @"CMS_WORK_INSTRUCTION_OPTION_MANAGER";
            public const string SelectSignatureTitle = @"CMS_WORK_INSTRUCTION_SELECT_SIGNATURE_TITLE";
            public const string OptionNone = @"CMS_WORK_INSTRUCTION_OPTION_NONE";
            public const string OptionOneSignature = @"CMS_WORK_INSTRUCTION_OPTION_ONE_SIGNATURE";
            public const string OptionTwoSignature = @"CMS_WORK_INSTRUCTION_OPTION_TWO_SIGNATURE";
            public const string StepsTitle = @"CMS_WORK_INSTRUCTION_STEPS_TITLE";
            public const string WorkAndAssessmentInstructionTitle = @"CMS_WORK_INSTRUCTION_WORK_AND_ASSESSMENT_INSTRUCTION_TITLE";
            public const string WorkInstructionsTitle = @"CMS_WORK_INSTRUCTION_WORK_INSTRUCTION_TITLE";
            public const string AssessmentInstructionsTitle = @"CMS_WORK_INSTRUCTION_ASSESSMENT_INSTRUCTION_TITLE";
            public const string Search = @"CMS_WORK_INSTRUCTION_SEARCH";
            public const string ItemsTitle = @"CMS_WORK_INSTRUCTION_ITEMS_TITLE";
            public const string WorkInstructionTypeTitle = @"CMS_WORK_INSTRUCTION_WORK_INSTRUCTION_TYPE_TITLE";
            public const string BasicInstructionType = @"CMS_WORK_INSTRUCTION_BASIC_INSTRUCTION_TYPE";
            public const string AssessmentInstructionType = @"CMS_WORK_INSTRUCTION_ASSESSMENT_INSTRUCTION_TYPE";
            public const string ItemTitle = @"CMS_WORK_INSTRUCTION_ITEM_TITLE";
            public const string QuestionWeightTitle = @"CMS_WORK_INSTRUCTION_QUESTION_WEIGHT_TITLE";
            public const string InstructionDialogTitle = @"CMS_WORK_INSTRUCTION_INSTRUCTION_DIALOG_TITLE";
            public const string DialogDelete = @"CMS_WORK_INSTRUCTION_DIALOG_DELETE";
            public const string DialogPrevItem = @"CMS_WORK_INSTRUCTION_DIALOG_PREV_ITEM";
            public const string DialogClose = @"CMS_WORK_INSTRUCTION_DIALOG_CLOSE";
            public const string DialogNextItem = @"CMS_WORK_INSTRUCTION_DIALOG_NEXT_ITEM";
            public const string FileSizeAlert = @"CMS_WORK_INSTRUCTION_FILE_SIZE_ALERT";
            public const string EnterTitlePlaceholder = @"CMS_WORK_INSTRUCTION_ENTER_TITLE_PLACEHOLDER";
            public const string EnterDescriptionPlaceholder = @"CMS_WORK_INSTRUCTION_ENTER_DESCRIPTION_PLACEHOLDER";
            public const string InstructionItem = @"CMS_WORK_INSTRUCTION_INSTRUCTION_ITEM";
            public const string InstructionId = @"CMS_WORK_INSTRUCTION_INSTRUCTION_ID";
            public const string InstructionAddItem = @"CMS_WORK_INSTRUCTION_INSTRUCTION_ADD_ITEM";
            public const string AddInstruction = @"CMS_WORK_INSTRUCTION_ADD_INSTRUCTION";
            public const string ScoringSystem = @"CMS_WORK_INSTRUCTION_SCORING_SYSTEM";
            public const string ScoringDisabledTitle = @"CMS_WORK_INSTRUCTION_SCORING_DISABLED_TITLE";
            public const string ConfirmationLeaveMessage = @"CMS_WORK_INSTRUCTION_CONFIRMATION_LEAVE_MESSAGE";
            public const string WorkInsctructionTitle = @"CMS_WORK_INSTRUCTION_WORK_INSTRUCTION_TITLE";
            public const string WorkInsctructionDetailsTitle = @"CMS_WORK_INSTRUCTION_WORK_DETAILS_INSTRUCTION_TITLE";
            public const string ExportTemplates = @"CMS_WORK_INSTRUCTION_EXPORT_TEMPLATES";
            public const string AvailableForAllAreas = @"CMS_WORK_INSTRUCTION_AVAILABLE_FOR_ALL_AREAS";
            public const string ItemAddPdf = @"CMS_WORK_INSTRUCTION_ITEM_ADD_PDF";
            public const string ItemAddLink = @"CMS_WORK_INSTRUCTION_ITEM_ADD_LINK";

            public const string ItemAddLinkModalTitle = @"CMS_WORK_INSTRUCTION_ITEM_ADD_LINK_MODAL_TITLE";

            public const string ItemAddLinkModalInsertLinkHere = @"CMS_WORK_INSTRUCTION_ITEM_ADD_LINK_MODAL_INSERT_LINK_HERE";
            public const string ItemAddLinkModalValidationError = @"CMS_WORK_INSTRUCTION_ITEM_ADD_LINK_MODAL_VALIDATION_ERROR";
            public const string ItemAddLinkModalSaveChanges = @"CMS_WORK_INSTRUCTION_ITEM_ADD_LINK_MODAL_SAVE_CHANGES";
            public const string ItemAddLinkModalClose = @"CMS_WORK_INSTRUCTION_ITEM_ADD_LINK_MODAL_CLOSE";

            public const string TitlePlaceholder = @"CMS_WORK_INSTRUCTION_TITLE_PLACEHOLDER";
            public const string DescriptionPlaceholder = @"CMS_WORK_INSTRUCTION_DESCRIPTION_PLACEHOLDER";

        }

        public static class Settings
        {
            public const string SettingsTitle = @"CMS_SETTINGS_SETTINGS_TITLE";
            public const string SettingTitle = @"CMS_SETTINGS_SETTING_TITLE";
            public const string SettingsValue = @"CMS_SETTINGS_SETTINGS_VALUE";
            public const string BtnAddOrChangeCompany = @"CMS_SETTINGS_BTN_ADD_OR_CHANGE_COMPANY";
            public const string DeselectAllCompanies = @"CMS_SETTINGS_DESELECT_ALL_COMPANIES";
            public const string SelectAllCompanies = @"CMS_SETTINGS_SELECT_ALL_COMPANIES";
            public const string BtnCancel = @"CMS_SETTINGS_BTN_CANCEL";
            public const string BtnSave = @"CMS_SETTINGS_BTN_SAVE";
            public const string HeaderSettingsOverview = @"CMS_SETTINGS_HEADER_SETTINGS_OVERVIEW";
            public const string DescriptionText = @"CMS_SETTINGS_DESCRIPTION_TEXT";
            public const string ValuesText = @"CMS_SETTINGS_VALUES_TEXT";
            public const string AllTitle = @"CMS_SETTINGS_ALL_TITLE";
            public const string SearchPlaceholder = @"CMS_SETTINGS_SEARCH_PLACEHOLDER";
            public const string CompanyTitle = @"CMS_SETTINGS_COMPANY_TITLE";
            public const string FeatureTitle = @"CMS_SETTINGS_FEATURE_TITLE";
            public const string GeneralTitle = @"CMS_SETTINGS_GENERAL_TITLE";
            public const string MarketplaceTitle = @"CMS_SETTINGS_MARKETPLACE_TITLE";
            public const string TechnicalTitle = @"CMS_SETTINGS_TECHNICAL_TITLE";
            public const string UserTitle = @"CMS_SETTIGNS_USER_TITLE";
            public const string ApplicationTitle = @"CMS_SETTINGS_APPLICATION_TITLE";
            public const string Type = @"CMS_SETTINGS_TYPE";
            public const string SettingSaved = @"CMS_SETTINGS_SETTING_SAVED";
            public const string SettingNotSaved = @"CMS_SETTINGS_SETTING_NOT_SAVED";
            public const string SettingsValuePlaceholder = @"CMS_SETTINGS_SETTINGS_VALUE_PLACEHOLDER";
            public const string SelectAllCompaniesTitle = @"CMS_SETTINGS_SELECT_ALL_COMPANIES_TITLE";
        }

        public static class Shared
        {
            public const string NavHomeTitle = @"CMS_SHARED_NAV_HOME_TITLE";
            public const string NavSettingsTitle = @"CMS_SHARED_NAV_SETTINGS_TITLE";
            public const string NavUsersTitle = @"CMS_SHARED_NAV_USERS_TITLE";
            public const string NavLogoffTitle = @"CMS_SHARED_NAV_LOGOFF_TITLE";
            public const string BtnToogleNavigation = @"CMS_SHARED_BTN_TOOGLE_NAVIGATION";
            public const string PhotosTitle = @"CMS_SHARED_PHOTOS_TITLE";
            public const string VideosTitle = @"CMS_SHARED_VIDEOS_TITLE";
            public const string GroupTasks = @"CMS_SHARED_GROUP_TASKS";
            public const string MyEZGOTitle = @"CMS_SHARED_MY_EZGO_TITLE";
            public const string Reports = @"CMS_SHARED_REPORTS";
            public const string Actions = @"CMS_SHARED_ACITONS";
            public const string NewComments = @"CMS_SHARED_NEW_COMMENTS";
            public const string ErrorTitle = @"CMS_SHARED_ERROR_TITLE";
            public const string NewTitle = @"CMS_SHARED_NEW_TITLE";
            public const string GenerateQRCode = @"CMS_SHARED_GENERATE_QR_CODE";
            public const string ShareButtonTitle = @"CMS_SHARED_SHARE_BUTTON_TITLE";
            public const string ShareTemplateButtonTitle = @"CMS_SHARED_SHARE_TEMPLATE_BUTTON_TITLE";
            public const string NavBackToInboxTitle = @"CMS_SHARED_NAV_BACK_TITLE";
            public const string Cancel = @"CMS_SHARED_CANCEL";
            public const string ChooseCompaniesToShareTo = @"CMS_SHARED_CHOOSE_COMPANIES_TO_SHARE_TO";
            public const string SelectDeselectAll = @"CMS_SAHRED_SELECT_DESELECT_ALL";
            public const string SaveBeforeSharing = @"CMS_SHARED_SAVE_BEFORE_SHARING";
            public const string SharingSuccess = @"CMS_SHARED_SHARING_SUCCESS";
            public const string SharingFailed = @"CMS_SHARED_SHARING_FAILED";
        }

        public static class Skills
        {
            public const string AssessmentTemplateLabel = @"CMS_ASSESSMENT_TEMPLATE_LABEL";
            public const string AssessmentsLabel = @"CMS_ASSESSMENTS_LABEL";
            public const string AssessmentExportTitle = @"CMS_ASSESSMENT_EXPORT_TITLE";
            public const string SkillExportTitle = @"CMS_Skill_EXPORT_TITLE";
            public const string AssessmentTemplateExportTitle = @"CMS_ASSESSMENT_TEMPLATE_EXPORT_TITLE";
            public const string AssessmentExecute = @"CMS_SKILLS_ASSESSMENT_EXECUTE";
            public const string AssessmentContinue = @"CMS_SKILLS_ASSESSMENT_CONTINUE";
            public const string AssessmentId = @"CMS_SKILLS_ASSESSMENT_ID";
            public const string AssessedTitle = @"CMS_SKILLS_ASSESSED_TITLE";
            public const string AssessmentInstructionDialogTitle = @"CMS_SKILLS_ASSESSMENT_DIALOG_TITLE";
            public const string AssessmentOpenTitle = @"CMS_SKILLS_ASSESSMENT_OPEN_TITLE";
            public const string AssessmentShowAllCompleted = @"CMS_SKILLS_ASSESSMENT_SHOW_ALL_COMPLETED";
            public const string OverViewTitle = @"CMS_SKILLS_OVER_VIEW_TITLE";
            public const string StatsAssessmentsTitle = @"CMS_SKILLS_STATS_ASSESSMENTS_TITLE";
            public const string StatsAssessmentsCompletedTitle = @"CMS_SKILLS_STATS_ASSESSMENTS_COMPLETED_TITLE";
            public const string Search = @"CMS_SKILLS_SEARCH";
            public const string AvailableUsersTitle = @"CMS_SKILLS_AVAILABLE_USERS_TITLE";
            public const string SelectParticipantsTitle = @"CMS_SKILLS_SELECT_PARTICIPANTS_TITLE";
            public const string ParticipantsInfoTitle = @"CMS_SKILLS_PARTICIPANTS_INFO_TITLE";
            public const string BtnAdd = @"CMS_SKILLS_BTN_ADD";
            public const string BtnAddNewMatrix = @"CMS_SKILLS_BTN_ADD_NEW_MATRIX";
            public const string BtnCancel = @"CMS_SKILLS_BTN_CANCEL";
            public const string BtnClose = @"CMS_SKILLS_BTN_CLOSE";
            public const string BtnChange = @"CMS_SKILLS_BTN_CHANGE";
            public const string BtnDelete = @"CMS_SKILLS_BTN_DELETE";
            public const string BtnDuplicate = @"CMS_SKILLS_BTN_DUPLICATE";
            public const string BtnExecute = @"CMS_SKILLS_BTN_EXECUTE";
            public const string BtnFinishAssessment = @"CMS_SKILLS_BTN_FINISH_ASSESSMENT";
            public const string BtnNext = @"CMS_SKILLS_BTN_NEXT";
            public const string BtnUpdate = @"CMS_SKILLS_BTN_UPDATE";
            public const string BtnSave = @"CMS_SKILLS_BTN_SAVE";
            public const string BtnSaveGropu = @"CMS_SKILLS_BTN_SAVE_GROUP";
            public const string BtnSign = @"CMS_SKILLS_BTN_SIGN";
            public const string BtnPrevious = @"CMS_SKILLS_BTN_PREVIOUS";
            public const string OngoingAssessmentsTitle = @"CMS_SKILLS_ONGOING_ASSESSMENTS_TITLE";
            public const string ParticipantsTitle = @"CMS_SKILLS_PARTICIPANTS_TITLE";
            public const string InstructionsTitle = @"CMS_SKILLS_INSTRUCTIONS_TITLE";
            public const string DescriptionTitle = @"CMS_SKILLS_DESCRIPTION_TITLE";
            public const string SignAssessmentTitle = @"CMS_SKILLS_SIGN_ASSESSMENT_TITLE";
            public const string SelectAssessorTitle = @"CMS_SKILLS_SELECT_ASSESSOR_TITLE";
            public const string FirstItemTitle = @"CMS_SKILLS_FIRST_ITEM_TITLE";
            public const string SearchAssessor = @"CMS_SKILLS_SEARCH_ASSESSOR";
            public const string MatrixId = @"CMS_SKILLS_MATRIX_ID";
            public const string MatrixUserGroups = @"CMS_SKILLS_MATRIX_USER_GROUPS";
            public const string MatrixOperationalSkills = @"CMS_SKILLS_MATRIX_OPERATIONAL_SKILLS";
            public const string MatrixMandatorySkills = @"CMS_SKILLS_MATRIX_MANDATORY_SKILLS";
            public const string NavBackTitle = @"CMS_SKILLS_NAV_BACK_TITLE";
            public const string BtnAddOrChangeUserGroups = @"CMS_SKILLS_BTN_ADD_OR_CHANGE_USER_GROUPS";
            public const string BtnAddOrChangeSelected = @"CMS_SKILLS_BTN_ADD_OR_CHANGE_SELECTED";
            public const string BtnAddOrChangeSkills = @"CMS_SKILLS_BTN_ADD_OR_CHANGE_SKILLS";
            public const string BtnViewMatrixLegend = @"CMS_SKILLS_BTN_VIEW_MATRIX_LEGEND";
            public const string BtnChangeSkillOrder = @"CMS_SKILLS_BTN_CHANGE_SKILL_ORDER";
            public const string ChangeSkillOrder = @"CMS_SKILLS_CHANGE_SKILL_ORDER";
            public const string NumberOfOperationalSkillsTitle = @"CMS_SKILLS_NUMBER_OF_OPERATIONAL_SKILLS_TITLE";
            public const string NumberOfUserGroupsTitle = @"CMS_SKILLS_NUMBER_OF_USER_GROUPS_TITLE";
            public const string MatrixGoalTitle = @"CMS_SKILLS_MATRIX_GOAL_TITLE";
            public const string MatrixResultTitle = @"CMS_SKILLS_MATRIX_RESULT_TITLE";
            public const string MatrixDeltaTitle = @"CMS_SKILLS_MATRIX_DELTA_TITLE";
            public const string MandatorySkillsTitle = @"CMS_SKILLS_MANDATORY_SKILLS_TITLE";
            public const string IndexNr = @"CMS_SKILLS_INDEX_NR";
            public const string MatrixGoalValue = @"CMS_SKILLS_MATRIX_GOAL_VALUE";
            public const string MatrixResultValue = @"CMS_SKILLS_MATRIX_RESULT_VALUE";
            public const string MatrixDifferenceValue = @"CMS_SKILLS_MATRIX_DIFFERENCE_VALUE";
            public const string UserGroupTitle = @"CMS_SKILLS_USER_GROUP_TITLE";
            public const string UsernameTitle = @"CMS_SKILLS_USERNAME_TITLE";
            public const string OperationalSkills = @"CMS_SKILLS_OPERATIONAL_SKILLS";
            public const string OperationalBehavior = @"CMS_SKILLS_OPERATIONAL_BEHAVIOR";
            public const string AddOrChangeSkillTitle = @"CMS_SKILLS_ADD_OR_CHANGE_SKILL_TITLE";
            public const string SelectSkillTitle = @"CMS_SKILLS_SELECT_SKILL_TITLE";
            public const string OptionMandatorySkill = @"CMS_SKILLS_OPTION_MANDATORY_SKILL";
            public const string OptionOperationalSkill = @"CMS_SKILLS_OPTION_OPERATIONAL_SKILL";
            public const string OptionNoSkillsAvailable = @"CMS_SKILLS_OPTION_NO_SKILLS_AVAILABLE";
            public const string DialogSkillName = @"CMS_SKILLS_DIALOG_SKILL_NAME";
            public const string SkillInMatrix = @"CMS_SKILLS_DIALOG_SKILL_IN_MATRIX";
            public const string DialogSkillDescription = @"CMS_SKILLS_DIALOG_SKILL_DESCRIPTION";
            public const string SkillTypeTitle = @"CMS_SKILLS_SKILL_TYPE_TITLE";
            public const string DialogNotificationWindowDays = @"CMS_SKILLS_DIALOG_NOTIFICATION_WINDOW_DAYS";
            public const string DialogExpiryInDays = @"CMS_SKILLS_DIALOG_EXPIRY_IN_DAYS";
            public const string DialogValidFrom = @"CMS_SKILLS_DIALOG_VALID_FROM";
            public const string DialogValidTo = @"CMS_SKILLS_DIALOG_VALID_TO";
            public const string SelectAssessmentTitle = @"CMS_SKILLS_SELECT_ASSESSMENT_TITLE";
            public const string AddOrChangeUserGroupsTitle = @"CMS_SKILLS_ADD_OR_CHANGE_USER_GROUPS_TITLE";
            public const string OptionSelectAnExistingGroup = @"CMS_SKILLS_OPTION_SELECT_AN_EXISTING_GROUP";
            public const string OptionNoGroupAvailable = @"CMS_SKILLS_OPTION_NO_GROUP_AVAILABLE";
            public const string GroupName = @"CMS_SKILLS_GROUP_NAME";
            public const string GroupDesctiprion = @"CMS_SKILLS_GROUP_DESCRIPTION";
            public const string MatrixLegendTitle = @"CMS_SKILLS_MATRIX_LEGEND_TITLE";
            public const string MatrixMandatorySkillsTitle = @"CMS_SKILLS_MATRIX_MANDATORY_SKILLS_TITLE";
            public const string ValueMastersTheSkill = @"CMS_SKILLS_VALUE_MASTERS_THE_SKILL";
            public const string ValueAlmostExpired = @"CMS_SKILLS_VALUE_ALMOST_EXPIRED";
            public const string ValueExpired = @"CMS_SKILLS_VALUE_EXPIRED";
            public const string MatrixOperationalSkillsTitle = @"CMS_SKILLS_MATRIX_OPERATIONAL_SKILLS_TITLE";
            public const string ValueOperationalSkillExpired = @"CMS_SKILLS_VALUE_OPERATIONAL_SKILL_EXPIRED";
            public const string ValueCanEducateOthers = @"CMS_SKILLS_VALUE_CAN_EDUCATE_OTHERS";
            public const string ValueNonStandardConditions = @"CMS_SKILLS_VALUE_NON_STANDARD_CONDITIONS";
            public const string ValueStandardSituations = @"CMS_SKILLS_VALUE_STANDARD_SITUATIONS";
            public const string ValueKnowTheTheory = @"CMS_SKILLS_VALUE_KNOW_THE_THEORY";
            public const string ValueNoKnowTheTheory = @"CMS_SKILLS_VALUE_NO_KNOW_THE_THEORY";
            public const string MatrixOperationalBehaviorTitle = @"CMS_SKILLS_MATRIX_OPERATIONAL_BEHAVIOR_TITLE";
            public const string ValueTheKingScoreOfTotal = @"CMS_SKILLS_VALUE_THE_KING_SCORE_OF_TOTAL";
            public const string ValueChampionScoreOfTotal = @"CMS_SKILLS_VALUE_CHAMPION_SCORE_OF_TOTAL";
            public const string ValueSpotOnScoreOfTotal = @"CMS_SKILLS_VALUE_SPOT_ON_SCORE_OF_TOTAL";
            public const string ValueRisingStarScoreOfTotal = @"CMS_SKILLS_VALUE_RISING_STAR_SCORE_OF_TOTAL";
            public const string ValuePowerUpScoreOfTotal = @"CMS_SKILLS_VALUE_POWER_UP_SCORE_OF_TOTAL";
            public const string ModalHeaderTitle = @"CMS_SKILLS_MODAL_HEADER_TITLE";
            public const string ModalContentTitle = @"CMS_SKILLS_MODAL_CONTENT_TITLE";
            public const string MatrixNavBackTitle = @"CMS_SKILLS_MATRIX_NAV_BACK_TITLE";
            public const string MatrixOverviewTitle = @"CMS_SKILLS_MATRIX_OVERVIEW_TITLE";
            public const string SkillMatricesTitle = @"CMS_SKILLS_SKILL_MATRICES_TITLE";
            public const string AddNewMatrixTitle = @"CMS_SKILLS_ADD_NEW_MATRIX_TITLE";
            public const string MatrixName = @"CMS_SKILLS_MATRIX_NAME";
            public const string MatrixDescription = @"CMS_SKILLS_MATRIX_DESCRIPTION";
            public const string MatrixArea = @"CMS_SKILLS_MATRIX_AREA";
            public const string MatrixTitle = @"CMS_SKILLS_MATRIX_TITLE";
            public const string NavBackToAssessmentTitle = @"CMS_SKILLS_NAV_BACK_TO_ASSESSMENT_TITLE";
            public const string AssessmentInstructionId = @"CMS_SKILLS_ASSESSMENT_INSTRUCTION_ID";
            public const string SelectSignatureTitle = @"CMS_SKILLS_SELECT_SIGNATURE_TITLE";
            public const string OptionNone = @"CMS_SKILLS_OPTION_NONE";
            public const string OptionOneSignature = @"CMS_SKILLS_OPTION_ONE_SIGNATURE";
            public const string OptionTwoSignature = @"CMS_SKILLS_OPTION_TWO_SIGNATURE";
            public const string AssessmentInstructionTitle = @"CMS_SKILLS_ASSESSMENT_INSTRUCTION_TITLE";
            public const string EnterAssessmentTitlePlacholder = "ENTER_ASSESSMENT_TITLE_PLACHOLDER";
            public const string EnterAssessmentDescriptionPlacholder = "ENTER_ASSESSMENT_DESCRIPTION_PLACHOLDER";
            public const string AssignInstruction = @"CMS_SKILLS_ASSIGN_INSTRUCTION";
            public const string AreaTitle = @"CMS_SKILLS_AREA_TITLE";
            public const string AssessmentDetailsTitle = @"CMS_SKILLS_ASSESSMENT_DETAILS_TITLE";
            public const string CompletedAssessmentsTitle = @"CMS_SKILLS_COMPLETED_ASSESSMENT_TITLE";
            public const string AddNewAssessmentTitle = @"CMS_SKILLS_ADD_NEW_ASSESSMENT_TITLE";
            public const string MatricesOverviewTitle = @"CMS_SKILLS_MATRICES_OVERVIEW_TITLE";
            public const string SelectPersonTitle = @"CMS_SKILLS_SELECT_PERSON_TITLE";
            public const string ResetSearch = @"CMS_SKILLS_RESET_SEARCH";
            public const string ExecuteAssessment = @"CMS_SKILLS_EXECUTE_ASSESSMENT";
            public const string SignatureNumberOneTitle = @"CMS_SKILLS_SIGNATURE_NUMBER_ONE_TITLE";
            public const string SignatureNumberTwoTitle = @"CMS_SKILLS_SIGNATURE_NUMBER_TWO_TITLE";
            public const string SkillMatrixDetailsTitle = @"CMS_SKILLS_MATRIX_DETAILS_TITLE";
            public const string CircleBtnPowerUpTitle = @"CMS_SKILLS_CIRCLE_BTN_POWER_UP_TITLE";
            public const string CircleBtnRisingStarTitle = @"CMS_SKILLS_CIRCLE_BTN_RISING_STAR_TITLE";
            public const string CircleBtnSpotOnTitle = @"CMS_SKILLS_CIRCLE_BTN_SPOT_ON_TITLE";
            public const string CircleBtnChampionTitle = @"CMS_SKILLS_CIRCLE_BTN_CHAMPION_TITLE";
            public const string CircleBtnKingTitle = @"CMS_SKILLS_CIRCLE_BTN_KING_TITLE";
            public const string SkillMatricesOverViewTitle = @"CMS_SKILLS_SKILL_MATRICES_OVER_VIEW_TITLE";
            public const string SkillMatrixOverViewTitle = @"CMS_SKILLS_SKILL_MATRIX_OVER_VIEW_TITLE";
            public const string SkillsAssesmentDetailsTitle = @"CMS_SKILLS_SKILLS_ASSESMENT_DETAILS_TITLE";
            public const string AssessmentIdTitle = @"CMS_SKILLS_ASSESSMENT_ID_TITLE";
            public const string SkillAssessmentTitle = @"CMS_SKILL_ASSESSMENT_TITLE";
            public const string CompletedAssessments = @"CMS_SKILLS_COMPLETED_ASSESSMENTS";
            public const string MatrixUserSkills = @"CMS_SKILLS_MATRIX_USER_SKILLS";
            public const string MatrixUserSkillModalUserSkills = @"CMS_SKILLS_MATRIX_USER_SKILLS_MODAL_USER_SKILLS";
            public const string MatrixUserSkillModalExpiryInDays = @"CMS_SKILLS_MATRIX_USER_SKILLS_MODAL_EXPIRYINDAYS";
            public const string MatrixUserSkillModalExpiryInDaysNotSet = @"CMS_SKILLS_MATRIX_USER_SKILLS_MODAL_EXPIRYINDAYS_NOTSET";
            public const string MatrixUserSkillModalExpiryWarningInDays = @"CMS_SKILLS_MATRIX_USER_SKILLS_MODAL_EXPIRYWARNINGINDAYS";
            public const string MatrixUserSkillModalExpiryWarningInDaysNotSet = @"CMS_SKILLS_MATRIX_USER_SKILLS_MODAL_EXPIRYWARNINGINDAYS_NOTSET";
            public const string MatrixUserSkillModalCertifiedAt = @"CMS_SKILLS_MATRIX_USER_SKILLS_MODAL_CERTIFIED_AT";
            public const string MatrixUserSkillModalConfirm = @"CMS_SKILLS_MATRIX_USER_SKILLS_MODAL_CONFIRM";
            public const string MatrixUserSkillModalKeepOldScoresMessageBody = @"CMS_SKILLS_MATRIX_KEEP_OLD_SCORES_MESSAGE_BODY";
            public const string MatrixUserSkillModalKeepOldScoresBtn = @"CMS_SKILLS_MATRIX_KEEP_OLD_SCORES_BTN";
            public const string MatrixUserSkillModalRemoveOldScoresBtn = @"CMS_SKILLS_MATRIX_REMOVE_OLD_SCORES_BTN";

            public const string CompletedAssessmentsInstructionScore = @"CMS_COMPLETED_ASSESSMENTS_INSTRUCTION_SCORE";
            public const string CompletedAssessmentsAssessmentScore = @"CMS_COMPLETED_ASSESSMENTS_ASSESSMENT_SCORE";
            public const string AssessmentAddItem = "CMS_EZGOLIST_ASSESSMET_ADD_ITEM_LOWER";

            public const string SkillMatrixRequiredLabel = @"CMS_SKILL_MATRIX_REQUIRED_LABEL";
            public const string SkillMatrixAchievedLabel = @"CMS_SKILL_MATRIX_ACHIEVED_LABEL";
            public const string SkillMatrixGapLabel = @"CMS_SKILL_MATRIX_GAP_LABEL";
        }

        public static class Auditing
        {
            public const string AuditingTitle = @"CMS_AUDITING_TITLE";
            public const string SelectAChange = @"CMS_AUDITING_SELECT_A_CHANGE";
            public const string ExportLog = @"CMS_AUDITING_EXPORT_LOG";
            public const string ChecklistTemplate = @"CMS_AUDITING_CHECKLIST_TEMPLATE";
            public const string AuditTemplate = @"CMS_AUDITING_AUDIT_TEMPLATE";
            public const string WorkInstructionTemplate = @"CMS_AUDITING_WORK_INSTRUCTION_TEMPLATE";
            public const string AssessmentTemplate = @"CMS_AUDITING_ASSESSMENT_TEMPLATE";
            public const string TaskTemplate = @"CMS_AUDITING_TASK_TEMPLATE";
            public const string LastModifiedAt = @"CMS_AUDITING_LAST_MODIFIED_AT";
            public const string NoItemsMessage = @"CMS_AUDITING_NO_ITEMS_MESSAGE";
        }

        public static class TaskGenerationManagement
        {
            public const string TaskGenerationManagementTitle = @"CMS_TGM_TASK_GENERATION_MANAGEMENT_TITLE";
            public const string DisableTaskGenerationTitle = @"CMS_TGM_DISABLE_TASK_GENERATION_TITLE";
            public const string BtnSave = @"CMS_TGM_BTN_SAVE";
            public const string BtnDelete = @"CMS_TGM_BTN_DELETE";
            public const string BasicInformation = @"CMS_TGM_BASIC_INFORMATION";
            public const string ReasonExplanation = @"CMS_TGM_REASON_EXPLANATION";
            public const string Reason = @"CMS_TGM_REASON";
            public const string ReasonPlaceholder = @"CMS_TGM_REASON_PLACEHOLDER";
            public const string Shifts = @"CMS_TGM_SHIFTS";
            public const string ShiftsExplatation = @"CMS_TGM_SHIFTS_EXPLANATION";
            public const string BtnClose = @"CMS_TGM_BTN_CLOSE";
            public const string DisabledAreas = @"CMS_TGM_DISABLED_AREAS";
            public const string None = @"CMS_TGM_NONE";
            public const string DisabledShifts = @"CMS_TGM_DISABLED_SHIFTS";
            public const string DisabledTaskTemplates = @"CMS_TGM_DISABLED_TASK_TEMPLATES";
            public const string NoPlanningAvailable = @"CMS_TGM_NO_PLANNING_AVAILABLE";
            public const string From = @"CMS_TGM_FROM";
            public const string To = @"CMS_TGM_TO";
            public const string Areas = @"CMS_TGM_AREAS";
            public const string AreasExplanation = @"CMS_TGM_AREAS_EXPLANATION";
            public const string TaskTemplates = @"CMS_TGM_TASK_TEMPLATES";
            public const string TaskTemplatesExplanation = @"CMS_TGM_TASK_TEMPLATES_EXPLANATION";
            public const string SelectedItems = @"CMS_TGM_SELECTED_ITEMS";
            public const string NoteDelayedEffect = @"CMS_TGM_NOTE_DELAYED_EFFECT";
            public const string NoteSafetyMargin = @"CMS_TGM_NOTE_SAFETLY_MARGIN";
            public const string NoteCombinationExplanation = @"CMS_TGM_NOTE_COMBINATION_EXPLANATION";
            public const string ErrorSelectItemsOrPeriod = @"CMS_TGM_ERROR_SELECT_ITEMS_OR_PERIOD";
            public const string NoStartDate = @"CMS_TGM_NO_START_DATE";
            public const string NoEndDate = @"CMS_TGM_NO_END_DATE";
            public const string NoReason = @"CMS_TGM_NO_REASON";
            public const string DeletePlanning = @"CMS_TGM_DELETE_PLANNING";
            public const string BackWithoutSaving = @"CMS_TGM_BACK_WITHOUT_SAVING";
            public const string FromDateBeforeToDate = @"CMS_TGM_FROM_DATE_BEFORE_TO_DATE";
            public const string ToDateAfterFromDate = @"CMS_TGM_TO_DATE_AFTER_FROM_DATE";
            public const string Period = @"CMS_TGM_PERIOD";
            public const string Yes = @"CMS_TGM_COMMON_YES";
            public const string No = @"CMS_TGM_COMMON_NO";
        }

        public static class Version
        {
            public const string OverviewTitle = @"CMS_VERSION_OVERVIEW_TITLE";
            public const string Versions = @"CMS_VERSIONS";
            public const string AppName = @"CMS_VERSION_APP_NAME";
            public const string AppVersionInternal = @"CMS_VERSION_APP_VERSION_INTERNAL";
            public const string AppVersion = @"CMS_VERSION_APP_VERSION";
            public const string OctopusVersion = @"CMS_VERSION_OCTOPUS_VERSION";
            public const string Platform = @"CMS_VERSION_Platform";
            public const string IsValidated = @"CMS_VERSION_IS_VALIDATED";
            public const string IsLive = @"CMS_VERSION_IS_LIVE";
            public const string IsCurrentLiveVersion = @"CMS_VERSION_IS_CURRENT_LIVE_VERSION";
            public const string ReleaseDate = @"CMS_VERSION_RELEASE_DATE";
            public const string ReleaseNotes = @"CMS_VERSION_RELEASE_NOTES";
        }

        public static class WiChangeNotification
        {
            public const string BackTitle = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_BACK_TITLE";

            public const string ReportPageTitle = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_REPORT_PAGE_TITLE";

            public const string NoChangeNotificationsFound = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_NO_CHANGE_NOTIFICATIONS_FOUND";

            public const string OldLabel = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_OLD_LABEL";
            public const string NewLabel = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_NEW_LABEL";

            public const string AreaNotFoundLabel = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_AREA_NOT_FOUND_LABEL";

            public const string NoPictureLabel = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_NO_PICTURE_LABEL";

            public const string NoChangesDetected = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_NO_CHANGES_DETECTED_LABEL";

            public const string ChangedLabel = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_CHANGED_LABEL";
            public const string ByLabel = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_BY_LABEL";
            public const string NoteToChangeLabel = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_NOTE_TO_CHANGE_LABEL";
            public const string SeenOnLabel = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_SEEN_ON_LABEL";

            public const string UsersThatHaveConfirmedThisChangeValueLabel = @"CMS_LAST_CHANGED_WORKINSTRUCTIONS_CONFIRMED_CHANGE_LABEL";
        }
    }
}
