using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// SettingResourceTypeEnum; A list of settings that are used within the apps, data and api's.
    /// NOTE! all settings have somekind of technical implementation, which can differ per setting. Do not change or optimize this 'just because you can' when not correctly changed or implemented it can break the system.
    /// NOTE! all settings should only be managed by EZFactory personal not by the customers themselves.
    /// </summary>
    public enum SettingResourceSettingsTypeEnum
    {
        /// <summary>
        /// General; General setting. Is used within the API for general settings that are not directly related to a feature, company or user.
        /// Data is stored within the SettingResources table.
        /// </summary>
        General = 1,
        /// <summary>
        /// Feature; Feature setting. Is used for enabling and disabling a feature based per company or for the entire application.
        /// Contains a list of id's or true/false depending on implementation.  Data is stored within the SettingResources table.
        /// </summary>
        Feature = 2,
        /// <summary>
        /// Company: Company setting is used for setting certain values for a company. E.g. company timezone, company location.
        /// Data specific to the company is stored in a specific settings table for the companies.
        /// </summary>
        Company = 3,
        /// <summary>
        /// User: User setting is used for setting certain values for a specific user. E.g. user application OS availability.
        /// Data specific to the user is stored in a specific settings table for the users.
        /// </summary>
        User = 4,
        /// <summary>
        /// MarketPlace; Market place item. Used for a connector to a seperate (3rd party) platform. E.g. Ultimo.
        /// Data specific to the company will be stored in the company setting table.
        /// </summary>
        MarketPlace = 5,
        /// <summary>
        /// HoldingCompany: HoldingCompany setting is used for setting certain values for a holding. E.g. export schedules.
        /// Data specific to the holding is stored in a specific settings table for the holding, if no holdings are available for certain companies the settings are stored in the company settings.
        /// </summary>
        HoldingCompany = 6,
        /// <summary>
        /// Application: Application setting, used for checking if a company has access to a certain application.
        /// </summary>
        Application = 7,
        /// <summary>
        /// Technical; technical settings, used for advanced debugging, logging and other.
        /// </summary>
        Technical = 999

    }
}
