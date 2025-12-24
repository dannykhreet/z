using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

//ToDo add more data time structures for specific settings
namespace EZGO.Api.Models.Authentication
{
    /// <summary>
    /// UserAuthenticationSettings; Contains settings for authentication and security logic. 
    /// Should only be used for internal checks, do not use for client tooling.
    /// 
    /// Fields are mapped to the following db structures.
    /// 
    /// "mfa_topt_enabled" 
    /// 
    /// Enable or disable MFA, if enabled, user can scan a qr code to enable TOPT on login.This must be done with a authenticator app or related
    /// 
    /// "mfa_topt_token" varchar(40) COLLATE "pg_catalog"."default" NULL,
    /// 
    /// Token used for generating topt token.This will be concated with the MFA general token and User.GUID to be used for generation.
    /// 
    /// "mfa_topt_generated" timestamp(6) NULL,
    /// 
    /// Last time the TOPT token has been generated.Based on best practices we can add logic to refresh the token after x number of days (this will be system wide) and can be used for informational
    /// 
    /// "mfa_topt_last_use" timestamp(6) NULL,
    /// 
    /// Last time MFA token was used.Can be used for calculating when the MFA (TOPT) must be used again when logging in. 
    /// 
    /// "mfa_email_enabled" boolean NULL,
    /// 
    /// Enable or disable MFA based on email.When enabled after login a mail will be send which has a validation token that must be used for logging in. 
    /// 
    /// "mfa_email_email" varchar(255) COLLATE "pg_catalog"."default" NULL,
    /// 
    /// Email adres used for validating.
    /// 
    /// "mfa_email_token" varchar(40) COLLATE "pg_catalog"."default" NULL,
    /// 
    ///  Email token used for MFA, this token will be generated each time the MFA is triggered.
    /// 
    /// "mfa_email_generated" timestamp(6) NULL,
    /// 
    /// Datetime when the token is generated. This will also be used to let the token expire so a token will expire after x-number of meetings.
    /// 
    /// "mfa_email_last_use" timestamp(6) NULL,
    /// 
    ///  Datetime last time when the token was actually used to login. Can also be used for validation. 
    /// 
    /// "mfa_sms_enabled" boolean NULL,
    /// 
    /// Enable or disable MFA for SMS. When enabled, a sms message is send containing a token that must be used for login. 
    /// 
    /// "mfa_sms_phone" varchar(40) COLLATE "pg_catalog"."default" NULL,
    /// 
    /// Phone number including land code where MFA SMS needs to be send to.
    /// 
    /// "mfa_sms_token" varchar(40) COLLATE "pg_catalog"."default" NULL,
    /// 
    /// Token used in SMS to validate the login. This token will be generated/reset on each request for MFA. And on validation cleared. 
    /// 
    /// "mfa_sms_generated" timestamp(6) NULL,
    /// 
    /// DateTime when token has been generated. Can be used to validate based on time if token is valid.
    /// 
    /// "mfa_sms_last_use" timestamp(6) NULL,
    /// 
    /// Datetime last time when the token was actually used to login. Can also be used for validation. 
    /// 
    /// "mfa_general_guid" varchar(40) COLLATE "pg_catalog"."default" NULL,
    /// 
    /// General GUID of all MFA structures, used for internal validations and generation of other tokens.
    /// 
    /// "mfa_after_login_time_in_min" int4 NULL,
    /// 
    /// Number of minutes, if set larger then 0, when MFA is asked again. If set to 0 then on every login MFA must be used. 
    /// 
    /// "password_renew_timeframe_days" int NULL,
    /// 
    /// Timeframe in when the user password needs to be changed. When set to 0 or NULL it will be permanent.
    /// 
    /// "password_older_hashes" text COLLATE "pg_catalog"."default" NULL,
    /// 
    /// Older hashes of the password to make sure the same password will not be used.Will be used for validation.
    /// 
    /// "password_last_changed" timestamp(6) NULL,
    /// 
    /// DateTime when password has been last changed.
    /// 
    /// "password_must_be_changed_next_login" boolean NULL,
    /// 
    /// Bit for forcing password change after next login.
    /// 
    /// "can_login" boolean NULL,
    /// 
    /// Bit for letting user login or not, can be used for temporary disable the user login.This will only be checked on login. When user is busy the data still can be entered and on next login will not be possible.
    /// 
    /// "access_to_date" timestamp(6) NULL,
    /// 
    /// Set a data until when the user has access to the system.Can be used to temporary let certain management users to have temporary access or force standards to invalidate users after certain date.
    /// 
    /// "access_given_by_id" int4 NULL,
    /// 
    /// User that set the access to the system. 
    /// "sync_guid" varchar(40) COLLATE "pg_catalog"."default" NULL,
    /// 
    /// Sync guid used for data syncing from clients when user is not logged in but data not synced yet.
    /// 
    /// "sync_guid_generated_at" timestamp(6) NOT NULL
    /// 
    /// DateTime where sync guid has been generated.Can be used for validation.
    /// 	
    /// "created_at" timestamp(6) NOT NULL,
    /// 
    /// When this record is created.
    /// 
    /// "modified_at" timestamp(6) NOT NULL,
    /// 
    /// Last time this records is validated.
    /// 
    /// "last_modified_by_id" int4 NOT NULL
    /// 
    /// Last time modified by id has been done. 
    /// </summary>
    public class UserAuthenticationSettings
    {
        /// <summary>
        /// Id; Id of user authentication setting record.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// UserId; UserId of user.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// CompanyId; CompanyId of user.
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// MfaToptEnabled; If MFA TOPT is enabled.
        /// </summary>
        public bool MfaToptEnabled { get; set; }
        /// <summary>
        /// MfaToptToken; MFA TOPT token.
        /// </summary>
        public string MfaToptToken { get; set; }
        /// <summary>
        /// MfaToptGenerated; DateTime of MFA TOPT Generated.
        /// </summary>
        public DateTime? MfaToptGenerated { get; set; }
        /// <summary>
        /// MfaToptLastUse; DateTime of last use of MFA TOPT
        /// </summary>
        public DateTime? MfaToptLastUse { get; set; }
        /// <summary>
        /// MfaEmailEnabled; Email MFA enabled true/false
        /// </summary>
        public bool MfaEmailEnabled { get; set; }
        /// <summary>
        /// MfaEmailEmail; Email address used for MFA (usually same as profile.email)
        /// </summary>
        public string MfaEmailEmail { get; set; }
        /// <summary>
        /// MfaEmailToken; MFA email token, used for email MFA.
        /// </summary>
        public string MfaEmailToken { get; set; }
        /// <summary>
        /// MfaEmailGenerated; DateTime when email token is generated.
        /// </summary>
        public DateTime? MfaEmailGenerated { get; set; }
        /// <summary>
        /// MfaEmailLastUse; Last time email token/mfa is used.
        /// </summary>
        public DateTime? MfaEmailLastUse { get; set; }
        /// <summary>
        /// MfaSmsEnabled; MFA SMS enabled
        /// </summary>
        public bool MfaSmsEnabled { get; set; }
        /// <summary>
        /// MfaSmsPhone; Phone number used for SMS MFA
        /// </summary>
        public bool MfaSmsPhone { get; set; }
        /// <summary>
        /// MfaSmsToken; Token used for SMS MFA
        /// </summary>
        public string MfaSmsToken { get; set; }
        /// <summary>
        /// MfaSmsGenerated; Time token is generated.
        /// </summary>
        public DateTime? MfaSmsGenerated { get; set; }
        /// <summary>
        /// MfaLastUse; DateTime when SMS MFA is last used.
        /// </summary>
        public DateTime? MfaLastUse { get; set; }
        /// <summary>
        /// MfaGeneralGuid; General GUID used for MFA
        /// </summary>
        public string MfaGeneralGuid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int MfaAfterLoginTimeInMin { get; set; }
        /// <summary>
        /// PasswordRenewTimeframeDays; Renew time in days after which passsword should be renewed. (checked at login)
        /// </summary>
        public int PasswordRenewTimeframeDays { get; set; }
        /// <summary>
        /// PasswordOlderHashes; Older hashed collection for checking new password generated against if already used.
        /// </summary>
        public string PasswordOlderHashes { get; set; }
        /// <summary>
        /// PasswordLastChanged; DateTime password last changed
        /// </summary>
        public DateTime? PasswordLastChanged { get; set; }
        /// <summary>
        /// PasswordMustBeChangedNextLogin; true/false if after next login password will needs to be changed.
        /// </summary>
        public bool PasswordMustBeChangedNextLogin { get; set; }
        /// <summary>
        /// CanLogin; true/false if used can/may login.
        /// </summary>
        public bool CanLogin { get; set; }
        /// <summary>
        /// AccessToDate; Access to date until the user may login.
        /// </summary>
        public DateTime? AccessToDate { get; set; }
        /// <summary>
        /// AccessGivenById; UserId of the user that given login to specific user.
        /// </summary>
        public int AccessGivenById { get; set; }
        /// <summary>
        /// SyncGuid; SyncGUID used for syncing data that is not specically created by the current user.
        /// </summary>
        public string SyncGuid { get; set; }
        /// <summary>
        /// SyncGuidGeneratedAt; DateTime when syncguid is generated.
        /// </summary>
        public DateTime? SyncGuidGeneratedAt { get; set; }
        /// <summary>
        /// CreatedAt
        /// </summary>
        public DateTime? CreatedAt { get; set; }
        /// <summary>
        /// ModifiedAt
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
        /// <summary>
        /// LastModifiedById; Last modified by id of user that modified this record.
        /// </summary>
        public int LastModifiedById { get; set; }
    }
}
