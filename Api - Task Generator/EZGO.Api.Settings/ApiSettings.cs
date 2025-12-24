using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Settings
{
    /// <summary>
    /// ApiSettings; Contains specific technical settings related to the APIs
    /// </summary>
    public static class ApiSettings
    {
        /// <summary>
        /// LEGACY_BASE_API_ROUTE; legacy base route part for the legacy api functionality within this API
        /// </summary>
        public const string LEGACY_BASE_API_ROUTE = "api";
        /// <summary>
        /// VERSION_V1_BASE_API_ROUTE; Version of the API for use within base route. This will be used within V1 API routes
        /// </summary>
        public const string VERSION_V1_BASE_API_ROUTE = "v1";
        /// <summary>
        /// 
        /// </summary>
        public const string VERSION_GEN4_BASE_API_ROUTE = "gen4";
        /// <summary>
        /// LATEST_VERSION; Latest most active version in the API.
        /// </summary>
        public const string LATEST_VERSION = "v1";
        /// <summary>
        /// ENABLE_DB_LOG_CONFIG_KEY; Configuration Key used for getting if DB logging is enabled. Normally this would be the case within Dev, Test environments.
        /// </summary>
        public const string ENABLE_DB_LOG_CONFIG_KEY = "AppSettings:EnableDbLogging";
        /// <summary>
        /// ENABLE_DB_LOG_REQUESTRESPONSE_CONFIG_KEY; Configuration key used for getting if request and response logging is enabled.
        /// </summary>
        public const string ENABLE_DB_LOG_REQUESTRESPONSE_CONFIG_KEY = "AppSettings:EnableDbLoggingRequestResponse";
        /// <summary>
        /// REQUESTCHECK_ELAPSED_TIME_IN_SECONDS; Request check for time check for running requests
        /// </summary>
        public const int REQUESTCHECK_ELAPSED_TIME_IN_SECONDS = 300;
        /// <summary>
        /// HAMMER_ELAPSED_TIME_IN_SECONDS; Hammer timeout for hammer protected routes in seconds. This is an hard-coded setting.
        /// </summary>
        public const int HAMMER_ELAPSED_TIME_IN_SECONDS = 120;
        /// <summary>
        /// HAMMER_MAX_NUMBER_OF_ATTEMPTS; Max number of attempts before the hammer protection kicks in. This is an hard-coded setting.
        /// </summary>
        public const int HAMMER_MAX_NUMBER_OF_ATTEMPTS = 5;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_RETURN_ITEMS; Fall back number when getting items, if limits are used within an route / functionality this can be used if no specific override is needed.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_RETURN_ITEMS = 100;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_ACTION_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_ACTION_RETURN_ITEMS = 400;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS = 200;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_TASKTEMPLATES_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_TASKTEMPLATES_RETURN_ITEMS = 200;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_CHECKLIST_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_CHECKLIST_RETURN_ITEMS = 200;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_CHECKLISTTEMPLATES_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_CHECKLISTTEMPLATES_RETURN_ITEMS = 200;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_AUDIT_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_AUDIT_RETURN_ITEMS = 200;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_COMMENT_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_COMMENT_RETURN_ITEMS = 400;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_ASSESSMENT_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_ASSESSMENT_RETURN_ITEMS = 50;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_AUDITTEMPLATES_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_AUDITTEMPLATES_RETURN_ITEMS = 200;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_WORKINSTRUCTIONTEMPLATES_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_WORKINSTRUCTIONTEMPLATES_RETURN_ITEMS = 100;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_USERSKILLVALUES_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_USERSKILLVALUES_RETURN_ITEMS = 100;
        /// <summary>
        /// DEFAULT_MAX_NUMBER_OF_SKILLSMATRICES_RETURN_ITEMS; Default number of items returned when no specific limit is supplied with the logic that gets the list of items.
        /// </summary>
        public const int DEFAULT_MAX_NUMBER_OF_SKILLSMATRICES_RETURN_ITEMS = 500;
        /// <summary>
        /// CORRS_CONFIG_KEY; CORRS config key for use to get origin check with CORRS related security functionality.
        /// </summary>
        public const string CORRS_CONFIG_KEY = "AllowedOrigins";
        /// <summary>
        /// CORRS_CONFIG_KEY; referrer config key for use to get referrer information for check with certain calls.
        /// </summary>
        public const string REFERER_CONFIG_KEY = "AllowedReferer";
        /// <summary>
        /// ENVIRONMENT_CONFIG_KEY; Config key used for setting the environment config.
        /// </summary>
        public const string ENVIRONMENT_CONFIG_KEY = "AppSettings:EnvironmentConfig";
        /// <summary>
        /// ENABLE_LOG_READ_FOR_USER_CONFIG_KEY; Enable the log read for a certain userid.
        /// </summary>
        public const string ENABLE_LOG_READ_FOR_USER_CONFIG_KEY = "AppSettings:EnableLogReadForUserId";
        /// <summary>
        /// Represents the configuration key used to enable restricted raw viewer access for a specific user.
        /// </summary>
        /// <remarks>This constant defines the key used in the application settings to control whether a
        /// user has access  to the restricted raw viewer functionality. The value associated with this key should be a
        /// user-specific  identifier or a boolean flag.</remarks>
        public const string ENABLE_RESTRICTED_RAWVIEWER_CONFIG_KEY = "AppSettings:EnableRestrictedRawViewer";
        /// <summary>
        /// ENABLE_MULTI_LOGIN; Enable multi login. When enabled users can login on multiple devices (normally only enabled on dev and test)
        /// </summary>
        public const string ENABLE_MULTI_DEVICE_LOGIN = "AppSettings:EnableMultiDeviceLogin";
        /// <summary>
        /// ENABLE_SIMULTANEOUS_PORTAL_DEVICE_LOGIN; Enable simultaneous portal and device login. Only a new device login will reset the token.
        /// </summary>
        public const string ENABLE_SIMULTANEOUS_PORTAL_DEVICE_LOGIN = "AppSettings:EnableSimultaneousPortalDeviceLogin";
        /// <summary>
        /// HEALTHCHECK_USER_ID_CONFIG_KEY; Represents a user id that can be used for running certain health checks.
        /// </summary>
        public const string HEALTHCHECK_USER_ID_CONFIG_KEY = "AppSettings:UserIdForHealthChecks";
        /// <summary>
        /// HEALTHCHECK_COMPANY_ID_CONFIG_KEY; Represents a company id that can be used for running certain health checks.
        /// </summary>
        public const string HEALTHCHECK_COMPANY_ID_CONFIG_KEY = "AppSettings:CompanyIdForHealthChecks";
        /// <summary>
        /// HEALTHCHECK_ITEM_LIMIT; Limit for number of items used within the health functionality.
        /// </summary>
        public const int HEALTHCHECK_ITEM_LIMIT = 200;
        /// <summary>
        /// APPLICATION_NAME_CONFIG_KEY; ApplicationName as stated in config
        /// </summary>
        public const string APPLICATION_NAME_CONFIG_KEY = "AppSettings:ApplicationName";
        /// <summary>
        ///
        /// </summary>
        public const string EnvironmentNameDev = "development";
        /// <summary>
        ///
        /// </summary>
        public const string EnvironmentNameLocalDev = "localdevelopment";
        /// <summary>
        ///
        /// </summary>
        public const string EnvironmentNameTest = "test";
        /// <summary>
        ///
        /// </summary>
        public const string EnvironmentNameStaging = "acceptance";
        /// <summary>
        ///
        /// </summary>
        public const string EnvironmentNameProd = "production";
        /// <summary>
        /// ENABLE_BACKWARDS_COMPATIBILITY_CONFIG_KEY; true/false value in config for enabling backward compatibility; if enabled extra data will be posted with task templates.
        /// </summary>
        public const string ENABLE_BACKWARDS_COMPATIBILITY_CONFIG_KEY = "AppSettings:EnableBackwardCompatibility";
        /// <summary>
        /// ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY; Enable the trance functions for elastic.
        /// </summary>
        public const string ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY = "AppSettings:EnableElasticInLogicTraces";
        /// <summary>
        /// MANAGEMENT_COMPANY_ID_CONFIG;
        /// </summary>
        public const string MANAGEMENT_COMPANY_ID_CONFIG = "AppSettings:AdministratorAdminCompany";
        /// <summary>
        /// Enable authentication logging
        /// </summary>
        public const string AUTHENTICATION_LOGGING_CONFIG_KEY = "AppSettings:EnableAuthenticationLogging";
        /// <summary>
        /// FULL_INCLUDE_LIST; Full include list.
        /// </summary>
        public const string FULL_INCLUDE_LIST = "tasks,tasktemplates,steps,properties,propertyuservalues,openfields,pictureproof,tags,userinformation,instructionrelations,areapaths,sappmfunctionallocations";
        /// <summary>
        /// FULL_INCLUDE_LIST_ASSESSMENTS; Full include list for items
        /// </summary>
        public const string FULL_INCLUDE_LIST_ASSESSMENTS = "instructions,instructionitems,mutationinformation,tags,areapaths";
        /// <summary>
        /// ENABLE_USER_AREA_SYNC_CONFIG_KEY; Enable or disable syncing of area/user tables when posting a new area.
        /// </summary>
        public const string ENABLE_USER_AREA_SYNC_CONFIG_KEY = "AppSettings:EnableAreaUserSync";
        /// <summary>
        /// VALID_LANGUAGE_CULTURES; list of valid cultures within databse, can be used for validation purposes.
        /// </summary>
        public static string[] VALID_LANGUAGE_CULTURES { get; } = { "en_us", "en_gb", "nl_nl", "de_de", "fr_fr", "es_es", "pt_pt", "it_it", "el_gr", "nb_no", "fi_fi", "da_dk", "sv_se", "is_is", "pl_pl", "lt_lt", "lv_lv", "et_ee", "ro_ro", "bg_bg", "cs_cz", "hr_hr", "hu_hu", "hy_am", "ka_ge", "mk_mk", "sk_sk", "sl_si", "sq_al", "uk_ua", "af_za", "ar_sa", "he_il", "id_id", "ja_jp", "ko_kr", "ru_ru", "tr_tr", "zh_cn", "hi_in", "gd_gb", "ga_ie", "fy_nl", "my_mm", "vi_vn", "th_th", "lo_la", "ms_my", "ms_sg", "fil_ph", "bn_bd", "si_lk", "km_kh","aa_dj", "aa_er", "aa_et", "af_na", "agq_cm", "ak_gh", "am_et", "ar_001", "ar_ae", "ar_bh", "ar_dj", "ar_dz", "ar_eg", "ar_er", "ar_il", "ar_iq", "ar_jo", "ar_km", "ar_kw", "ar_lb", "ar_ly", "ar_ma", "ar_mr", "ar_om", "ar_ps", "ar_qa", "ar_sd", "ar_so", "ar_ss", "ar_sy", "ar_td", "ar_tn", "ar_ye", "arn_cl", "as_in", "asa_tz", "ast_es", "az_cyrl_az", "az_latn_az", "ba_ru", "bas_cm", "be_by", "bem_zm", "bez_tz", "bin_ng", "bm_latn_ml", "bn_in", "bo_cn", "bo_in", "br_fr", "brx_in", "bs_cyrl_ba", "bs_latn_ba", "byn_er", "ca_ad", "ca_es", "ca_es_valencia", "ca_fr", "ca_it", "ccp_cakm_bd", "ccp_cakm_in", "ce_ru", "ceb_latn_ph", "cgg_ug", "chr_cher_us", "co_fr", "cu_ru", "cy_gb", "da_gl", "dav_ke", "de_at", "de_be", "de_ch", "de_it", "de_li", "de_lu", "dje_ne", "dsb_de", "dua_cm", "dv_mv", "dyo_sn", "dz_bt", "ebu_ke", "ee_gh", "ee_tg", "el_cy", "en_001", "en_029", "en_150", "en_ae", "en_ag", "en_ai", "en_as", "en_at", "en_au", "en_bb", "en_be", "en_bi", "en_bm", "en_bs", "en_bw", "en_bz", "en_ca", "en_cc", "en_ch", "en_ck", "en_cm", "en_cx", "en_cy", "en_de", "en_dk", "en_dm", "en_er", "en_fi", "en_fj", "en_fk", "en_fm", "en_gd", "en_gg", "en_gh", "en_gi", "en_gm", "en_gu", "en_gy", "en_hk", "en_id", "en_ie", "en_il", "en_im", "en_in", "en_io", "en_je", "en_jm", "en_ke", "en_ki", "en_kn", "en_ky", "en_lc", "en_lr", "en_ls", "en_mg", "en_mh", "en_mo", "en_mp", "en_ms", "en_mt", "en_mu", "en_mw", "en_my", "en_na", "en_nf", "en_ng", "en_nl", "en_nr", "en_nu", "en_nz", "en_pg", "en_ph", "en_pk", "en_pn", "en_pr", "en_pw", "en_rw", "en_sb", "en_sc", "en_sd", "en_se", "en_sg", "en_sh", "en_si", "en_sl", "en_ss", "en_sx", "en_sz", "en_tc", "en_tk", "en_to", "en_tt", "en_tv", "en_tz", "en_ug", "en_um", "en_vc", "en_vg", "en_vi", "en_vu", "en_ws", "en_za", "en_zm", "en_zw", "eo_001", "es_419", "es_ar", "es_bo", "es_br", "es_bz", "es_cl", "es_co", "es_cr", "es_cu", "es_do", "es_ec", "es_gq", "es_gt", "es_hn", "es_mx", "es_ni", "es_pa", "es_pe", "es_ph", "es_pr", "es_py", "es_sv", "es_us", "es_uy", "es_ve", "eu_es", "ewo_cm", "fa_ir", "ff_latn_bf", "ff_latn_cm", "ff_latn_gh", "ff_latn_gm", "ff_latn_gn", "ff_latn_gw", "ff_latn_lr", "ff_latn_mr", "ff_latn_ne", "ff_latn_ng", "ff_latn_sl", "ff_latn_sn", "fo_dk", "fo_fo", "fr_029", "fr_be", "fr_bf", "fr_bi", "fr_bj", "fr_bl", "fr_ca", "fr_cd", "fr_cf", "fr_cg", "fr_ch", "fr_ci", "fr_cm", "fr_dj", "fr_dz", "fr_ga", "fr_gf", "fr_gn", "fr_gp", "fr_gq", "fr_ht", "fr_km", "fr_lu", "fr_ma", "fr_mc", "fr_mf", "fr_mg", "fr_ml", "fr_mq", "fr_mr", "fr_mu", "fr_nc", "fr_ne", "fr_pf", "fr_pm", "fr_re", "fr_rw", "fr_sc", "fr_sn", "fr_sy", "fr_td", "fr_tg", "fr_tn", "fr_vu", "fr_wf", "fr_yt", "fur_it", "gl_es", "gn_py", "gsw_ch", "gsw_fr", "gsw_li", "gu_in", "guz_ke", "gv_im", "ha_latn_gh", "ha_latn_ne", "ha_latn_ng", "haw_us", "hr_ba", "hsb_de", "ia_001", "ibb_ng", "ig_ng", "ii_cn", "it_ch", "it_sm", "it_va", "iu_cans_ca", "iu_latn_ca", "jgo_cm", "jmc_tz", "jv_java_id", "jv_latn_id", "kab_dz", "kam_ke", "kde_tz", "kea_cv", "khq_ml", "ki_ke", "kk_kz", "kkj_cm", "kl_gl", "kln_ke", "kn_in", "ko_kp", "kok_in", "kr_latn_ng", "ks_arab_in", "ks_deva_in", "ksb_tz", "ksf_cm", "ksh_de", "ku_arab_iq", "ku_arab_ir", "kw_gb", "ky_kg", "la_001", "lag_tz", "lb_lu", "lg_ug", "lkt_us", "ln_ao", "ln_cd", "ln_cf", "ln_cg", "lrc_iq", "lrc_ir", "lu_cd", "luo_ke", "luy_ke", "mas_ke", "mas_tz", "mer_ke", "mfe_mu", "mg_mg", "mgh_mz", "mgo_cm", "mi_nz", "ml_in", "mn_mn", "mn_mong_cn", "mn_mong_mn", "mni_in", "moh_ca", "mr_in", "ms_bn", "mt_mt", "mua_cm", "mzn_ir", "naq_na", "nb_sj", "nd_zw", "nds_de", "nds_nl", "ne_in", "ne_np", "nl_aw", "nl_be", "nl_bq", "nl_cw", "nl_sr", "nl_sx", "nmg_cm", "nn_no", "nnh_cm", "nqo_gn", "nr_za", "nso_za", "nus_ss", "nyn_ug", "oc_fr", "om_et", "om_ke", "or_in", "os_ge", "os_ru", "pa_arab_pk", "pa_in", "pap_029", "prg_001", "prs_af", "ps_af", "ps_pk", "pt_ao", "pt_br", "pt_ch", "pt_cv", "pt_gq", "pt_gw", "pt_lu", "pt_mo", "pt_mz", "pt_st", "pt_tl", "quc_latn_gt", "quz_bo", "quz_ec", "quz_pe", "rm_ch", "rn_bi", "ro_md", "rof_tz", "ru_by", "ru_kg", "ru_kz", "ru_md", "ru_ua", "rw_rw", "rwk_tz", "sa_in", "sah_ru", "saq_ke", "sbp_tz", "sd_arab_pk", "sd_deva_in", "se_fi", "se_no", "se_se", "seh_mz", "ses_ml", "sg_cf", "shi_latn_ma", "shi_tfng_ma", "sma_no", "sma_se", "smj_no", "smj_se", "smn_fi", "sms_fi", "sn_latn_zw", "so_dj", "so_et", "so_ke", "so_so", "sq_mk", "sq_xk", "sr_cyrl_ba", "sr_cyrl_me", "sr_cyrl_rs", "sr_cyrl_xk", "sr_latn_ba", "sr_latn_me", "sr_latn_rs", "sr_latn_xk", "ss_sz", "ss_za", "ssy_er", "st_ls", "st_za", "sv_ax", "sv_fi", "sw_cd", "sw_ke", "sw_tz", "sw_ug", "syr_sy", "ta_in", "ta_lk", "ta_my", "ta_sg", "te_in", "teo_ke", "teo_ug", "tg_cyrl_tj", "ti_er", "ti_et", "tig_er", "tk_tm", "tn_bw", "tn_za", "to_to", "tr_cy", "ts_za", "tt_ru", "twq_ne", "tzm_arab_ma", "tzm_latn_dz", "tzm_latn_ma", "tzm_tfng_ma", "ug_cn", "ur_in", "ur_pk", "uz_arab_af", "uz_cyrl_uz", "uz_latn_uz", "vai_latn_lr", "vai_vaii_lr", "ve_za", "vo_001", "vun_tz", "wae_ch", "wal_et", "wo_sn", "xh_za", "xog_ug", "yav_cm", "yi_001", "yo_bj", "yo_ng", "zgh_tfng_ma", "zh_hans_hk", "zh_hans_mo", "zh_hk", "zh_mo", "zh_sg", "zh_tw", "zu_za" };
    }
}
