using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Settings
{
    /// <summary>
    ///
    /// </summary>
    public static class MediaSettings
    {
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_TASKS = "tasks";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_TASK_STEPS = "steps";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_TASK_DESCRIPTIONS = "task_descriptions";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_CHECKLISTS = "lists";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_CHECKLIST_ITEMS = "tasks";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_CHECKLIST_STEPS = "steps";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_CHECKLIST_DESCRIPTIONS = "task_descriptions";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_AUDITS = "lists";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_AUDIT_ITEMS = "tasks";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_AUDIT_STEPS = "steps";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_AUDIT_DESCRIPTIONS = "task_descriptions";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_SIGNATURE = "signatures";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_PROFILEIMAGE = "profile";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_ACTIONS = "actions";
        /// <summary>
        /// Type Constants are based apon existing structures within the media storage.
        /// These are copied from the original whyellow api upload system.
        /// </summary>
        public const string TYPE_ACTIONCOMMENTS = "actioncomments";
        /// <summary>
        /// Type Constants are based upon existing structures within the media storage.
        /// </summary>
        public const string TYPE_AREAS = "areas";
        /// <summary>
        /// Type Constants are based upon existing structures within the media storage.
        /// </summary>
        public const string TYPE_COMMENTS = "comments";
        /// <summary>
        /// Type Constants are based upon existing structures within the media storage.
        /// </summary>
        public const string TYPE_FACTORYFEED = "factoryfeed";
        /// <summary>
        /// Type Constants are based upon existing structures within the media storage.
        /// </summary>
        public const string TYPE_FACTORYFEEDMESSAGES = "factoryfeedmessages";
        /// <summary>
        /// Type Constants are based upon existing structures within the media storage.
        /// </summary>
        public const string TYPE_COMPANY = "companies";
        /// <summary>
        /// 
        /// </summary>
        public const string TYPE_WORKINSTRUCTION = "workinstruction";
        /// <summary>
        /// 
        /// </summary>
        public const string TYPE_ASSESSMENT = "assessment";
        /// <summary>
        /// 
        /// </summary>
        public const string TYPE_ANNOUNCEMENT = "announcement";
        /// <summary>
        /// 
        /// </summary>
        public const string TYPE_PICTUREPROOF = "pictureproof";
        /// <summary>
        /// ALLOWED_EXTENSIONS_MEDIA; contains a list of valid upload extensions that may be used within the upload mechanisms.
        /// </summary>
        public const string ALLOWED_EXTENSIONS_MEDIA = "jpeg,jpg,svg,png,gif,mp4,avi";
        /// <summary>
        /// AWSS3_BUCKETREGIONURL_CONFIG_KEY; Config key for AWS connections. (based apon AWS SDK docs)
        /// </summary>
        public const string AWSS3_BUCKETREGIONURL_CONFIG_KEY = "S3Config:BucketRegionUrl";
        /// <summary>
        /// AWSS3_BUCKETNAME_CONFIG_KEY;Config key for AWS connections. (based apon AWS SDK docs)
        /// </summary>
        public const string AWSS3_BUCKETNAME_CONFIG_KEY = "S3Config:BucketName";
        /// <summary>
        /// AWSS3_ACCESSKEY_CONFIG_KEY;Config key for AWS connections. (based apon AWS SDK docs)
        /// </summary>
        public const string AWSS3_ACCESSKEY_CONFIG_KEY = "S3Config:AccessKey";
        /// <summary>
        /// AWSS3_SECRETACCESSKEY_CONFIG_KEY;Config key for AWS connections. (based apon AWS SDK docs)
        /// </summary>
        public const string AWSS3_SECRETACCESSKEY_CONFIG_KEY = "S3Config:SecretAccesskey";
        /// <summary>
        /// MEDIA_STORAGE_LOCATION_CONFIG_KEY; Config key for getting the diskmediastorage location from the configuration settings (app config)
        /// </summary>
        public const string MEDIA_STORAGE_LOCATION_CONFIG_KEY = "AppSettings:DiskStorageMediaLocation";



        /// <summary>
        /// AWSS3_BUCKETREGIONURL_CONFIG_KEY; Config key for AWS connections. (based apon AWS SDK docs)
        /// </summary>
        public const string AWSS3_MEDIA_BUCKETREGIONURL_CONFIG_KEY = "S3Config:MediaBucketRegionUrl";
        /// <summary>
        /// AWSS3_BUCKETNAME_CONFIG_KEY;Config key for AWS connections. (based apon AWS SDK docs)
        /// </summary>
        public const string AWSS3_MEDIA_BUCKETNAME_CONFIG_KEY = "S3Config:MediaBucketName";
        /// <summary>
        /// AWSS3_ACCESSKEY_CONFIG_KEY;Config key for AWS connections. (based apon AWS SDK docs)
        /// </summary>
        public const string AWSS3_MEDIA_ACCESSKEY_CONFIG_KEY = "S3Config:MediaAccessKey";
        /// <summary>
        /// AWSS3_SECRETACCESSKEY_CONFIG_KEY;Config key for AWS connections. (based apon AWS SDK docs)
        /// </summary>
        public const string AWSS3_MEDIA_SECRETACCESSKEY_CONFIG_KEY = "S3Config:MediaSecretAccesskey";
        /// <summary>
        /// MEDIA_STORAGE_LOCATION_CONFIG_KEY; Config key for getting the diskmediastorage location from the configuration settings (app config)
        /// </summary>
        public const string MEDIA_MEDIA_STORAGE_LOCATION_CONFIG_KEY = "AppSettings:DiskStorageMediaLocation";
        /// <summary>
        /// ENABLE_MEDIA_UPLOAD_CONFIG_KEY; Enable extra logging config key
        /// </summary>
        public const string ENABLE_MEDIA_UPLOAD_CONFIG_KEY = "AppSettings:EnableUploadLogging";

    }
}
