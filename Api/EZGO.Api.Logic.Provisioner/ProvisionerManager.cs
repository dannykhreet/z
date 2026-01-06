using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Provisioner;
using EZGO.Api.Logic.Provisioner.Base;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models;
using Microsoft.Extensions.Logging;
using EZGO.Api.Models.Provisioner;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Npgsql;
using System.ComponentModel;
using EZGO.Api.Interfaces.Settings;
using System.ComponentModel.Design;
using EZGO.Api.Models.Enumerations;
using System.Text;
using System.Data.SqlTypes;

//TODO add validations
//TODO add pre-check
namespace EZGO.Api.Logic.Provisioner
{
    /// <summary>
    /// ProvisionerManager; Provisioning manager containing all relevant logic to proces file (txt) data to import large set of users. 
    /// Depending on which logic is used most related parameters will either be based on config or supplied by the content or the method (company ids, user ids etc)
    /// </summary>
    public class ProvisionerManager : BaseManager<ProvisionerManager>, IProvisionerManager
    {
        private readonly IDatabaseAccessHelper _dbmanager;
        private readonly IConfigurationHelper _configHelper;
        private readonly IConnectionHelper _connectionHelper;
        private bool _saveAreaEnabled = true; //disabled for now, until areas are supplied by customer
        private List<string> messages = new List<string>();
        private bool _demoModeOnly { get { return _configHelper.GetValueAsBool("AppSettings:DemoModeOnly"); } }


        public ProvisionerManager(IDatabaseAccessHelper dbmanager, IConfigurationHelper configHelper, IConnectionHelper connectionHelper, ILogger<ProvisionerManager> logger) : base(logger)
        {
            _dbmanager = dbmanager;
            _configHelper = configHelper;
            _connectionHelper = connectionHelper;
        }

        /// <summary>
        /// Provision; Main method for provisioning, based on company, can be run directly, based on executing user. 
        /// </summary>
        /// <param name="companyId">Company where information need to be parsed.</param>
        /// <param name="userId">UserId which does the processing.</param>
        /// <param name="type">Type of processing (depending on type logic may change)</param>
        /// <param name="content">Content containing all content.</param>
        /// <returns></returns>
        public async Task<bool> Provision(int companyId, int userId, string type, string content)
        {
            await AddProvisionerLogEvent(message: "Start Provision");

            if (!string.IsNullOrEmpty(content))
            {
                ProvisionerData provisionerData = await ParseProvisioningData(companyId: companyId, userId: userId, type: type, rawData: content);
                //get informational data
                provisionerData.SystemUsers = await GetPossibleSystemUsersBasedOnCompanyIds(holdingId: -1, companyId: companyId);
                if(type == "atoss")
                {
                    provisionerData.ExternalCompanyMapping = await GetPossibleCompaniesBasedOnDataAtoss(holdingId: -1, companyId: companyId);

                    provisionerData.AreaMapping = await GetPossibleAreasBasedOnCompanyIds(holdingId: -1, companyId: companyId); 
                }
                //create users
                provisionerData = await CreateUsers(companyId: companyId, userId: userId, provisionerData: provisionerData);
                //save users
                await SaveUsers(companyId:companyId, userId: userId, provisionerData: provisionerData);
            }

            if (messages.Any())
            {
                await AddProvisionerLogEvent(message: "Provision messages", description: string.Join("\n", messages));
            }

            await AddProvisionerLogEvent(message: "End Provision");

            return false;
        }

        /// <summary>
        /// Provision based on holding. Within holding provisioning, multiple companies can be available per company.
        /// Logic will therefor be a bit different, based on the data the company and user will be determined and therefor be dynamic per provisioned user. 
        /// 
        /// </summary>
        /// <param name="holdingId">Company where information need to be parsed.</param>
        /// <param name="type">Type of processing (depending on type logic may change)</param>
        /// <param name="content">Content containing all content.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ProvisionByHolding(int holdingId, string type, string content)
        {
            await AddProvisionerLogEvent(message: "Start Provision");

            if (!string.IsNullOrEmpty(content))
            {
                var ignoredCompanyId = -1;
                var ignoredUserId = -1;
                //parse data
                ProvisionerData provisionerData = await ParseProvisioningData(companyId: ignoredCompanyId, userId: ignoredUserId, type: type, rawData: content);
                //get informational data
                provisionerData.SystemUsers = await GetPossibleSystemUsersBasedOnCompanyIds(holdingId: holdingId, companyId: ignoredCompanyId);
                if (type == "atoss")
                {
                    provisionerData.ExternalCompanyMapping = await GetPossibleCompaniesBasedOnDataAtoss(holdingId: holdingId, companyId: ignoredCompanyId);

                    provisionerData.AreaMapping = await GetPossibleAreasBasedOnCompanyIds(holdingId: holdingId, companyId: ignoredCompanyId);
                }
                //create users
                provisionerData = await CreateUsers(companyId: ignoredCompanyId, userId: ignoredUserId, provisionerData: provisionerData);
                //save users
                await SaveUsers(companyId: ignoredCompanyId, userId: ignoredUserId, provisionerData: provisionerData);
            }

            if (messages.Any())
            {
                await AddProvisionerLogEvent(message: "Provision messages", description: string.Join("\n", messages));
            }

            await AddProvisionerLogEvent(message: "End Provision");

            return false;
        }

        /// <summary>
        /// Provision based on company.
        /// Logic will therefor be a bit different, based on the data the company and user will be determined and therefor be dynamic per provisioned user. 
        /// 
        /// </summary>
        /// <param name="companyId">Company where information need to be parsed.</param>
        /// <param name="type">Type of processing (depending on type logic may change)</param>
        /// <param name="content">Content containing all content.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ProvisionByCompany(int companyId, string type, string content)
        {
            await AddProvisionerLogEvent(message: "Start Provision");

            if (!string.IsNullOrEmpty(content))
            {
                var ignoredUserId = -1;
                //parse data
                ProvisionerData provisionerData = await ParseProvisioningData(companyId: companyId, userId: ignoredUserId, type: type, rawData: content);
                //get informational data
                provisionerData.SystemUsers = await GetPossibleSystemUsersBasedOnCompanyIds(holdingId: -1, companyId: companyId);
                if (type == "atoss")
                {
                    provisionerData.ExternalCompanyMapping = await GetPossibleCompaniesBasedOnDataAtoss(holdingId: -1, companyId: companyId);

                    provisionerData.AreaMapping = await GetPossibleAreasBasedOnCompanyIds(holdingId: -1, companyId: companyId);
                }
                //create users
                provisionerData = await CreateUsers(companyId: companyId, userId: ignoredUserId, provisionerData: provisionerData);
                //save users
                await SaveUsers(companyId: companyId, userId: ignoredUserId, provisionerData: provisionerData);
            }

            if(messages.Any())
            {
                await AddProvisionerLogEvent(message: "Provision messages", description: string.Join("\n", messages));
            }

            await AddProvisionerLogEvent(message: "End Provision");

            return false;
        }

        /// <summary>
        /// ParseProvisioningData; Parse string containing all data to be processed. Normally is based on CSV txt file output. 
        /// Split will be auto determined. 
        /// </summary>
        /// <param name="companyId">CompanyId of item to be processed</param>
        /// <param name="userId">User id of item to be processed</param>
        /// <param name="type">Type of processing (depending on type logic may change)</param>
        /// <param name="rawData">Raw data string containing relevant data.</param>
        /// <returns>ProvisionerData, containing parsed data.</returns>
        private async Task<ProvisionerData> ParseProvisioningData(int companyId, int userId, string type, string rawData)
        {
            await AddProvisionerLogEvent(message: ">> Start ParseProvisioningData");

            ProvisionerData output = new ProvisionerData();
            output.DataType = type;
            output.DataContentItems = new List<ProvisionerDataItem>();

            //determine split
            var split = "";
            if (rawData.Contains("\r\n")) { split = "\r\n"; }
            else if (rawData.Contains("\n\r")) { split = "\n\r"; }
            else if (rawData.Contains("\n")) { split = "\n"; }
            else { split = "\r"; }

            //split data based on rows
            var possibleRows = rawData.Split(split);
            foreach(string possibleRow in possibleRows)
            {
                var possibleDataItem = await ParseProvisioningDataItem(companyId: companyId, userId: userId, type: output.DataType, rawItemData: possibleRow);
                output.DataContentItems.Add(possibleDataItem);
            }

            await AddProvisionerLogEvent(message: "ParseProvisioningData Stats", description: string.Format("Number of content items {0}", output.DataContentItems.Count));
            await AddProvisionerLogEvent(message: ">> End ParseProvisioningData");

            return output;
        }

        /// <summary>
        /// ParseProvisioningDataItem; Split text into array and add to provisionDataItem. 
        /// 
        /// Example data:
        ///
        /// ATOSS RULES (Personeelsnummer;Voornaam;Achternaam;Datum indiensttreding;leave_date;Company_ID;E-mailadres;Inactief)
        /// A00000000;Firstname;Lastname;01-01-2024;;31;somekind@ezfactory.nl;nee
        /// A00000001;Firstname;Lastname;01-01-2024;01-01-2025;31;somekind@ezfactory.nl;ja
        ///
        /// EZGO RULES
        /// userid;companyid;firstname;lastname;email;upn
        /// 
        /// </summary>
        /// <param name="companyId">CompanyId of item to be processed</param>
        /// <param name="userId">User id of item to be processed</param>
        /// <param name="type">Type of processing (depending on type logic may change)</param>
        /// <param name="rawItemData">Raw data string containing relevant data.</param>
        /// <returns>ProvisionerDataItem; containing parsed data.</returns>
        private async Task<ProvisionerDataItem> ParseProvisioningDataItem(int companyId, int userId, string type, string rawItemData)
        {
            ProvisionerDataItem output = new ProvisionerDataItem();
            //determine split, split and get items.
            string[] fieldItems = rawItemData.Split(this.DetermineSplitDelimiter(rawItemData));
            //add to collection if any items
            if(fieldItems.Any())
            {
                output.DataContentItem = fieldItems;
            }

            return output;
        }

        /// <summary>
        /// CreateUsers; create users based on the ProvisionerData. 
        /// </summary>
        /// <param name="companyId">CompanyId of item to be processed</param>
        /// <param name="userId">User id of item to be processed</param>
        /// <param name="provisionerData">Containing all relevant data and will be updated.</param>
        /// <returns>Provisioner data containing user item.</returns>
        private async Task<ProvisionerData> CreateUsers(int companyId, int userId, ProvisionerData provisionerData)
        {
            await AddProvisionerLogEvent(message: ">> Start CreateUsers");

            if (provisionerData != null && provisionerData.DataContentItems != null)
            {
                provisionerData.Users = new List<ProvisionerUser>();

                if (provisionerData.DataType == "atoss") //replace with enum
                {
                    foreach (var item in provisionerData.DataContentItems)
                    {
                        var possibleUser = await ProvisionUserAtoss(companyId: companyId, userId: userId, systemUsers: provisionerData.SystemUsers, externalCompanyMapping: provisionerData.ExternalCompanyMapping, areaMapping: provisionerData.AreaMapping, item: item);
                        if (possibleUser.CompanyId > 0 && !string.IsNullOrEmpty(possibleUser.EmployeeId) && possibleUser.EmployeeId != "Personeelsnummer") //TODO add extra validators
                        {
                            provisionerData.Users.Add(possibleUser);
                        } else
                        {
                            messages.Add(string.Format("User not added {0}-{1}", possibleUser.CompanyId, possibleUser.EmployeeId));
                        }
                        
                    }
                }
                else
                {
                    foreach (var item in provisionerData.DataContentItems)
                    {
                        var possibleUser = await ProvisionUserEZGO(companyId: companyId, userId: userId, systemUsers: provisionerData.SystemUsers, item: item);
                        if(possibleUser.CompanyId > 0) //TODO add extra validators
                        {
                            provisionerData.Users.Add(possibleUser);
                        }
                        else
                        {
                            messages.Add(string.Format("User not added. {0}", possibleUser.CompanyId));
                        }

                    }
                }

              
            }
            await AddProvisionerLogEvent(message: "CreateUsers Stats", description: string.Format("Number of users: {0}", provisionerData.Users.Count().ToString()));

            await AddProvisionerLogEvent(message: ">> End CreateUsers");

            return provisionerData;
        }

        /// <summary>
        /// ProvisionUserEZGO; Provision an ezgo user, create a user object for further processing, if not valid or not complete empty object will be returned. 
        /// </summary>
        /// <param name="companyId">CompanyId of item to be processed</param>
        /// <param name="userId">User id of item to be processed</param>
        /// <param name="systemUsers">List of system users used for retrieving extra information</param>
        /// <param name="item">Item containing the information to be processed.</param>
        /// <returns>User object.</returns>
        private async Task<ProvisionerUser> ProvisionUserEZGO(int companyId, int userId, List<ProvisionerSystemUser> systemUsers, ProvisionerDataItem item)
        {
            var possibleCompanyId = companyId;
            var possibleUserId = userId;

            if (userId < 0)
            {
                if (systemUsers != null)
                {
                    possibleUserId = systemUsers.Where(x => x.CompanyId == possibleCompanyId).FirstOrDefault().UserId;
                }
                else
                {
                    messages.Add(string.Format("SystemUsers are empty for {0}", companyId.ToString()));
                }

            }

            var output = new ProvisionerUser();
            output.Id = Convert.ToInt32(item.DataContentItem[0].ToString());
            output.CompanyId = Convert.ToInt32(item.DataContentItem[1].ToString());
            output.ModifiedByUserId = possibleUserId;

            output.FirstName = item.DataContentItem[2].ToString();
            output.LastName = item.DataContentItem[3].ToString();
            output.Email = item.DataContentItem[4].ToString();
            output.Upn = item.DataContentItem[5].ToString();

            await Task.CompletedTask;

            return output;
        }

        /// <summary>
        /// ProvisionUserAtoss; provisioner atoss user, create a user object for further processing, if not valid or not complete empty object will be returned.
        /// </summary>
        /// <param name="companyId">CompanyId of item to be processed</param>
        /// <param name="userId">User id of item to be processed</param>
        /// <param name="systemUsers">List of system users used for retrieving extra information</param>
        /// <param name="externalCompanyMapping">List of company mapping information used for retrieving / processing extra information.</param>
        /// <param name="areaMapping">Area mapping if needed.</param>
        /// <param name="item">Item containing the information to be processed.</param>
        /// <returns>User object.</returns>
        private async Task<ProvisionerUser> ProvisionUserAtoss(int companyId, int userId, List<ProvisionerSystemUser> systemUsers, List<ProvisionerCompanyMapper> externalCompanyMapping, List<ProvisionerAreaMapper> areaMapping, ProvisionerDataItem item)
        {
            var possibleCompanyId = companyId;
            var possibleUserId = userId;

            //content can not be empty, must contain 8 elements. 
            if(item.DataContentItem == null || item.DataContentItem.Length != 8 || item.DataContentItem[0] == "Personeelsnummer")
            {
                return new ProvisionerUser(); //return empty user
            }

            if (companyId < 0) //use mapper
            {
                if (externalCompanyMapping != null && externalCompanyMapping.Any())
                {
                    if(externalCompanyMapping.Where(x => x.ExternalCompanyId == item.DataContentItem[5].ToString()).Any())
                    {
                        possibleCompanyId = externalCompanyMapping.Where(x => x.ExternalCompanyId == item.DataContentItem[5].ToString()).FirstOrDefault().CompanyId;
                    }
                    if(possibleCompanyId < 0)
                    {
                        messages.Add(string.Format("Company ({0}) not valid", item.DataContentItem[5].ToString()));
                    }
                }
                else { 
                    //Add logging
                }
                
            }
            if (userId < 0)
            {
                if(systemUsers != null)
                {
                    if(systemUsers.Where(x => x.CompanyId == possibleCompanyId).Any())
                    {
                        possibleUserId = systemUsers.Where(x => x.CompanyId == possibleCompanyId).FirstOrDefault().UserId;
                    } else
                    {
                        messages.Add(string.Format("SystemUsers are empty for {0}", companyId.ToString()));
                    }
                }
               
            }
            //TODO move to other structure lateron when items will be dynamic addons. 
            int possibleAreaId = 0;
            if (areaMapping.Where(x => x.CompanyId == possibleCompanyId).Any())
            {
                possibleAreaId = areaMapping.Where(x => x.CompanyId == possibleCompanyId).FirstOrDefault().AreaId;
            }

            var output = new ProvisionerUser();

            if (possibleCompanyId > 0 && possibleUserId > 0)
            {
                output.AreaId = possibleAreaId;
                output.CompanyId = possibleCompanyId;
                output.ModifiedByUserId = possibleUserId;
                output.EmployeeId = item.DataContentItem[0].ToString();
                output.FirstName = item.DataContentItem[1].ToString();
                output.LastName = item.DataContentItem[2].ToString();
                output.EmployeeStartDateString = item.DataContentItem[3].ToString();
                output.EmployeeEndDateString = item.DataContentItem[4].ToString();
                output.ExternalCompanyIdentifier = item.DataContentItem[5].ToString();
                output.Email = item.DataContentItem[6].ToString();
                output.ExternalActiveString = item.DataContentItem[7].ToString();
            } else
            {
                messages.Add(string.Format("User not created for {0} - {1}", possibleCompanyId.ToString(), possibleUserId.ToString()));
            }

            await Task.CompletedTask;

            return output;
        }

        /// <summary>
        /// ValidateAtossDataContent; Validator to check data before processing. 
        /// (validator needed due to issues with source data)
        /// </summary>
        /// <param name="item">Item containing the data</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> ValidateAtossDataContent(ProvisionerDataItem item)
        {
            var sbValidationMessage = new StringBuilder();
            if(item!= null && item.DataContentItem != null && item.DataContentItem.Length == 8) 
            {
                if(item.DataContentItem[0] == "Personeelsnummer")
                {
                    sbValidationMessage.Append("Item starts with [Personeelsnummer] therefor header is presumed;");
                } else
                {
                    if(string.IsNullOrEmpty(item.DataContentItem[5]))
                    {
                        sbValidationMessage.Append("External company id is empty;");
                    }
                    if (string.IsNullOrEmpty(item.DataContentItem[0]))
                    {
                        sbValidationMessage.Append("EmployeeId is empty;");
                    }
                    if(string.IsNullOrEmpty(item.DataContentItem[1]) && string.IsNullOrEmpty(item.DataContentItem[2]))
                    {
                        sbValidationMessage.Append("First name and last name are empty;");
                    }
                    if (string.IsNullOrEmpty(item.DataContentItem[7]))
                    {
                        sbValidationMessage.Append("Active string is empty;");
                    }
                    if (!string.IsNullOrEmpty(item.DataContentItem[6]) && item.DataContentItem[6].Contains("@") && item.DataContentItem[6].Contains(".") && (item.DataContentItem[6].Length <= 6)) {
                        sbValidationMessage.Append("Invalid email address found;");
                    }


                }
            } else
            {
                sbValidationMessage.Append("Item is empty or contains less than expected columns;");
            }
            messages.Add(string.Format("[{0}][{1}]", (item != null && item.DataContentItem != null && item.DataContentItem.Length > 0 ? item.DataContentItem[0] : "UNKNOWN"), sbValidationMessage.ToString()));
            
            return (sbValidationMessage.Length == 0); //return true when there are no messages.
        }


        /// <summary>
        /// GetPossibleCompaniesBasedOnDataAtoss; TODO -> make dynamic
        /// </summary>
        /// <param name="holdingId">Retrieve based on holding id</param>
        /// <param name="companyId">Retrieve based on company id</param>
        /// <returns>List of mapping information.</returns>
        private async Task<List<ProvisionerCompanyMapper>> GetPossibleCompaniesBasedOnDataAtoss(int holdingId, int companyId)
        {
            var output = new List<ProvisionerCompanyMapper>();

            if (_configHelper.GetValueAsString("AppSettings:EnvironmentConfig") == "production")
            {
                //production settings, these will be added to DB later on for dynamic retrieval
                output.Add(new ProvisionerCompanyMapper { CompanyId = 885, ExternalCompanyId = "34" });
                output.Add(new ProvisionerCompanyMapper { CompanyId = 886, ExternalCompanyId = "31" });
                output.Add(new ProvisionerCompanyMapper { CompanyId = 1019, ExternalCompanyId = "32" });
                output.Add(new ProvisionerCompanyMapper { CompanyId = 1020, ExternalCompanyId = "33" });
                output.Add(new ProvisionerCompanyMapper { CompanyId = 1092, ExternalCompanyId = "35" });
                output.Add(new ProvisionerCompanyMapper { CompanyId = 1545, ExternalCompanyId = "36" });
            } else
            {
                //internal setting for testing and debugging purposes, map all to demo company.
                output.Add(new ProvisionerCompanyMapper { CompanyId = 136, ExternalCompanyId = "34" });
                output.Add(new ProvisionerCompanyMapper { CompanyId = 136, ExternalCompanyId = "31" });
                output.Add(new ProvisionerCompanyMapper { CompanyId = 136, ExternalCompanyId = "32" });
                output.Add(new ProvisionerCompanyMapper { CompanyId = 136, ExternalCompanyId = "33" });
                output.Add(new ProvisionerCompanyMapper { CompanyId = 136, ExternalCompanyId = "35" });
                output.Add(new ProvisionerCompanyMapper { CompanyId = 136, ExternalCompanyId = "36" });
                output.Add(new ProvisionerCompanyMapper { CompanyId = 136, ExternalCompanyId = "99" });
            }

            await Task.CompletedTask;//for making method async callable.

            return output;
        }

        /// <summary>
        /// GetPossibleAreasBasedOnCompanyIds; Get Areas for automapping data. 
        /// </summary>
        /// <param name="holdingId"></param>
        /// <param name="companyId"></param>
        /// <returns>return mapping collection for auto adding area to newly created user</returns>
        private async Task<List<ProvisionerAreaMapper>> GetPossibleAreasBasedOnCompanyIds(int holdingId, int companyId)
        {
            var output = new List<ProvisionerAreaMapper>();

            if(_configHelper.GetValueAsString("AppSettings:EnvironmentConfig") == "production")
            {
                output.Add(new ProvisionerAreaMapper { AreaId = 29333, CompanyId = 886 });
                output.Add(new ProvisionerAreaMapper { AreaId = 29334, CompanyId = 1019 });
                output.Add(new ProvisionerAreaMapper { AreaId = 29335, CompanyId = 1020 });
                output.Add(new ProvisionerAreaMapper { AreaId = 29336, CompanyId = 885 });
                output.Add(new ProvisionerAreaMapper { AreaId = 29337, CompanyId = 1092 });
                output.Add(new ProvisionerAreaMapper { AreaId = 29338, CompanyId = 1545 });
                output.Add(new ProvisionerAreaMapper { CompanyId = 136, AreaId = 2826 });
            } else
            {
                output.Add(new ProvisionerAreaMapper { CompanyId = 136, AreaId = 2826 });
            }


            await Task.CompletedTask;

            return output;
        }

        /// <summary>
        /// GetPossibleSystemUsersBasedOnCompanyIds; Retrieve possible system users for further processing.
        /// </summary>
        /// <param name="holdingId">HoldingId, will retrieve all system users with this holdingId</param>
        /// <param name="companyId">CompanyId, will retrieve all system users with this company</param>
        /// <returns>List of system users for further processing.</returns>
        private async Task<List<ProvisionerSystemUser>> GetPossibleSystemUsersBasedOnCompanyIds(int holdingId, int companyId)
        {
            var output = new List<ProvisionerSystemUser>();

            var parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            try
            {
                NpgsqlDataReader dr = null;

                using (dr = await _dbmanager.GetDataReader("get_system_users_based_on_holding_company", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["company_id"] != DBNull.Value && dr["user_id"] != DBNull.Value)
                        {
                            output.Add(new ProvisionerSystemUser() { CompanyId = Convert.ToInt32(dr["company_id"]), UserId = Convert.ToInt32(dr["user_id"]) });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await AddProvisionerLogEvent(message: "Error GetPossibleSystemUsersBasedOnCompanyIds", type:"ERROR", description: ex.Message);

                _logger.LogError(exception: ex, message: string.Concat("Provisioner.GetPossibleSystemUsersBasedOnCompanyIds(): ", ex.Message));

            }

            return output;
        }

        /// <summary>
        /// SaveUsers; Saved collection of users to DB, based on type other functionality will be used.
        /// User data is contained in the provisionerdata which is pre-filled.
        /// </summary>
        /// <param name="companyId">CompanyId being processed.</param>
        /// <param name="userId">UserId doing the processing</param>
        /// <param name="provisionerData">Data containing the information.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> SaveUsers(int companyId, int userId, ProvisionerData provisionerData)
        {
            await AddProvisionerLogEvent(message: ">> Start SaveUsers");

            var sb = new StringBuilder();
            var possibleCompanyIds = await GetResourceSettingIds(settingKey: "TECH_COMPANY_USER_PROVISIONING");
            if (provisionerData != null && provisionerData.DataType == "atoss")
            {
                foreach(var user in provisionerData.Users)
                {
                    if(possibleCompanyIds.Contains(user.CompanyId))
                    {
                        var success = await SaveUserAtoss(companyId: companyId, userId: userId, user: user);
                        sb.AppendLine(string.Format("User inserted or updated {0}-{1}:{2}", user.CompanyId, !string.IsNullOrEmpty(user.EmployeeId) ? user.EmployeeId : user.Id, success));

                    } else
                    {
                        messages.Add(string.Format("User company not enabled or available {0}", user.CompanyId));
                    }
                }
            } else if (provisionerData != null && provisionerData.DataType == "ezgo")
            {
                foreach (var user in provisionerData.Users)
                {
                    if (possibleCompanyIds.Contains(user.CompanyId))
                    {
                        var success = await SaveUserEzgo(companyId: companyId, userId: userId, user: user);
                        sb.AppendLine(string.Format("User inserted or updated {0}-{1}:{2}", user.CompanyId, !string.IsNullOrEmpty(user.EmployeeId) ? user.EmployeeId : user.Id, success));

                    } else
                    {
                        messages.Add(string.Format("User company not enabled or available {0}", user.CompanyId));
                    }
                }
            }

            await AddProvisionerLogEvent(message: "SaveUsers Statistics", description: sb.ToString());

            await AddProvisionerLogEvent(message: ">> End SaveUsers");

            return true;
        }

        /// <summary>
        /// SaveUserAtoss; Save user atoss. 
        /// Main check will be based on company and employee id (external id) 
        /// </summary>
        /// <param name="companyId">CompanyId (ezgo companyid)</param>
        /// <param name="userId">UserId, user id of user (system user or if manual service user) that runs the data processing.</param>
        /// <param name="user">User object containing all data to be processed. </param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> SaveUserAtoss(int companyId, int userId, ProvisionerUser user)
        {
            //Format of input data before user: 
            //_companyid integer, _email character varying, _firstname character varying, _lastname character varying, _upn character varying, _username character varying,_employeeid character varying,_modifiedbyid integer

            if(user.CompanyId <= 0 || user.ModifiedByUserId <= 0 || string.IsNullOrEmpty(user.EmployeeId))
            {
                messages.Add(string.Format("User company, modified id or employee id is not valid {0}-{1}-{2}", user.CompanyId, user.ModifiedByUserId, user.EmployeeId));
                return false;
            }

            if(string.IsNullOrEmpty(user.FirstName)||string.IsNullOrEmpty(user.LastName)) {
                messages.Add(string.Format("User first name or last name is not valid. {0}-{1}", user.CompanyId, user.EmployeeId));

                return false;
            }

            if (!string.IsNullOrEmpty(user.Email) && user.Email.Contains("@") && user.Email.Contains(".") && (user.Email.Length <= 6)) {
                messages.Add(string.Format("User first name or last name is not valid. {0}-{1}", user.CompanyId, user.EmployeeId));

                return false;
            }

            try
            {
                if(!_demoModeOnly)
                {
                    var parameters = new List<NpgsqlParameter>();

                    parameters.Add(new NpgsqlParameter("@_companyid", user.CompanyId));
                    parameters.Add(new NpgsqlParameter("@_email", user.Email));
                    parameters.Add(new NpgsqlParameter("@_firstname", user.FirstName));
                    parameters.Add(new NpgsqlParameter("@_lastname", user.LastName));
                    parameters.Add(new NpgsqlParameter("@_upn", string.IsNullOrEmpty(user.Upn) ? DBNull.Value : user.Upn));
                    parameters.Add(new NpgsqlParameter("@_username", string.Concat(user.FirstName.Replace(" ", ""), ".", user.LastName.Replace(" ", ""), ".", Guid.NewGuid().ToString().Substring(0, 3)).ToLower()));
                    parameters.Add(new NpgsqlParameter("@_employeeid", user.EmployeeId));
                    parameters.Add(new NpgsqlParameter("@_modifiedbyid", user.ModifiedByUserId));

                    var output = await _dbmanager.ExecuteScalarAsync("save_provisioner_user_atoss", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
                } else
                {
                    messages.Add(string.Format("save_provisioner_user_atoss [{0}][{1}][{2}]", user.CompanyId, user.EmployeeId, user.ModifiedByUserId));
                }
            }
            catch (Exception ex)
            {
                await AddProvisionerLogEvent(message: "Error SaveUserAtoss", type: "ERROR", description: ex.Message);

                // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                _logger.LogError(exception: ex, message: string.Concat("ProvisionerManager.SaveUserAtoss(): ", ex.Message));
                return false;
            }
            finally
            {

            }

            //Add remove user if user is added to atoss but already deleted and not in the system. 
            if(user.SetInactiveInverted) //NOTE, atoss uses inverted values. 
            {
                await SetAtossUserInactive(companyId: companyId, userId: userId, user: user);
            }

            //Add possible area id to user. 
            if (_saveAreaEnabled && user.AreaId.HasValue && user.AreaId.Value > 0)
            {
                await SetAtossUserArea(companyId: companyId, userId: userId, user: user);
            }

            return true;
        }

        /// <summary>
        /// SaveUserEzgo; Save user as EZGO user.
        /// </summary>
        /// <param name="companyId">CompanyId (ezgo companyid)</param>
        /// <param name="userId">UserId, user id of user (system user or if manual service user) that runs the data processing.</param>
        /// <param name="user">User object containing all data to be processed. </param>
        /// <returns></returns>
        private async Task<bool> SaveUserEzgo(int companyId, int userId, ProvisionerUser user)
        {
            //Format of user data before processing
            //_userid integer, _companyid integer, _email character varying, _firstname character varying, _lastname character varying, _upn character varying, _username character varying,_modifiedbyid integer

            if (companyId <= 0 || userId <= 0)
            {
                return false;
            }

            try
            {
                var parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_userid", user.Id.HasValue ? user.Id.Value : 0));
                parameters.Add(new NpgsqlParameter("@_companyid", user.CompanyId));
                parameters.Add(new NpgsqlParameter("@_email", user.Email));
                parameters.Add(new NpgsqlParameter("@_firstname", user.FirstName));
                parameters.Add(new NpgsqlParameter("@_lastname", user.LastName));
                parameters.Add(new NpgsqlParameter("@_upn", string.IsNullOrEmpty(user.Upn) ? DBNull.Value : user.Upn));
                parameters.Add(new NpgsqlParameter("@_username", string.Concat(user.FirstName.Replace(" ", ""), ".", user.LastName.Replace(" ", ""), ".", Guid.NewGuid().ToString().Substring(0, 3)).ToLower()));
                parameters.Add(new NpgsqlParameter("@_modifiedbyid", user.ModifiedByUserId));

                var output = await _dbmanager.ExecuteScalarAsync("save_provisioner_user_ezgo", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                await AddProvisionerLogEvent(message: "Error SaveUserEzgo", type: "ERROR", description: ex.Message);

                // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                _logger.LogError(exception: ex, message: string.Concat("ProvisionerManager.SaveUserEzgo(): ", ex.Message));
            }
            finally
            {

            }
            return true;
        }

        /// <summary>
        /// SetAtossUserInactive; Set user inactive based on its company and employee id. 
        /// </summary>
        /// <param name="companyId">CompanyId (ezgo companyid)</param>
        /// <param name="userId">UserId, user id of user (system user or if manual service user) that runs the data processing.</param>
        /// <param name="user">User object containing all data to be processed. </param>
        /// <returns></returns>
        private async Task<bool> SetAtossUserInactive(int companyId, int userId, ProvisionerUser user)
        {
            try
            {
                if(!_demoModeOnly)
                {
                    var parameters = new List<NpgsqlParameter>();

                    parameters.Add(new NpgsqlParameter("@_companyid", user.CompanyId));
                    parameters.Add(new NpgsqlParameter("@_employeeid", user.EmployeeId));
                    parameters.Add(new NpgsqlParameter("@_modifiedbyid", user.ModifiedByUserId));

                    var output = await _dbmanager.ExecuteScalarAsync("save_provisioner_user_atoss_set_inactive", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                }
                else
                {
                    messages.Add(string.Format("save_provisioner_user_atoss_set_inactive [{0}][{1}][{2}]", user.CompanyId, user.EmployeeId, user.ModifiedByUserId));
                }
            }
            catch (Exception ex)
            {
                await AddProvisionerLogEvent(message: "Error SetAtossUserInactive", type: "ERROR", description: ex.Message);

                // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                _logger.LogError(exception: ex, message: string.Concat("ProvisionerManager.SetAtossUserInactive(): ", ex.Message));
                return false;
            }
            finally
            {

            }
            //TODO check if add check for output is needed. 
            return true;
        }

        /// <summary>
        /// SetAtossUserArea; Add possible area to user;
        /// User area will be added with new users that are saved and have no area attached. 
        /// This will be based on creation date and or a area check in the SP. 
        /// </summary>
        /// <param name="companyId">CompanyId (ezgo companyid)</param>
        /// <param name="userId">UserId, user id of user (system user or if manual service user) that runs the data processing.</param>
        /// <param name="user">User object containing all data to be processed. </param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> SetAtossUserArea(int companyId, int userId, ProvisionerUser user)
        {
            if (user.CompanyId > 0 && !string.IsNullOrEmpty(user.EmployeeId) && user.ModifiedByUserId > 0 && user.AreaId > 0)
            {
                try
                {
                    if(!_demoModeOnly)
                    {
                        var parameters = new List<NpgsqlParameter>();

                        parameters.Add(new NpgsqlParameter("@_companyid", user.CompanyId));
                        parameters.Add(new NpgsqlParameter("@_employeeid", user.EmployeeId));
                        parameters.Add(new NpgsqlParameter("@_modifiedbyid", user.ModifiedByUserId));
                        parameters.Add(new NpgsqlParameter("@_areaid", user.AreaId.Value));

                        var output = await _dbmanager.ExecuteScalarAsync("save_provisioner_user_atoss_set_area", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                    }
                    else
                    {
                        messages.Add(string.Format("save_provisioner_user_atoss_set_area [{0}][{1}][{2}][{3}]", user.CompanyId, user.EmployeeId, user.ModifiedByUserId, user.AreaId));
                    }
                }
                catch (Exception ex)
                {
                    await AddProvisionerLogEvent(message: "Error SetAtossUserArea", type: "ERROR", description: ex.Message);

                    // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                    _logger.LogError(exception: ex, message: string.Concat("ProvisionerManager.SetAtossUserArea(): ", ex.Message));
                    return false;
                }
                finally
                {

                }
            }
            return true;//TODO check if add check for output is needed. 
        }


        /// <summary>
        /// GetResourceSettingIds; Get list of ids based on setting
        /// </summary>
        /// <param name="settingKey">Key to get values</param>
        /// <returns>List of ids</returns>
        private async Task<List<int>> GetResourceSettingIds(string settingKey)
        {
            var output = new List<int>();

            var parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_settingkey", settingKey));

            try
            {
                NpgsqlDataReader dr = null;

                using (dr = await _dbmanager.GetDataReader("get_resource_settings_by_key", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["settingvalue"] != DBNull.Value)
                        {
                            var ids = dr["settingvalue"].ToString();
                            if (ids != null)
                            {
                                output = ids.Split(",").Where(x => int.TryParse(x, out int num)).Select(x => Convert.ToInt32(x)).ToList();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await AddProvisionerLogEvent(message: "Error GetResourceSettingIds", type: "ERROR", description: ex.Message);

                _logger.LogError(exception: ex, message: string.Concat("ProvisionerManager.GetResourceSettingIds(): ", ex.Message));

            }

            return output;
        }

        /// <summary>
        /// DetermineSplitDelimiter; Determine possible delimiter for spliting a text in an array. 
        /// </summary>
        /// <param name="source">Source that could contain the information</param>
        /// <returns>the possible split char</returns>
        private string DetermineSplitDelimiter(string source)
        {
            var countPipe = CountSplit(source, "|");
            var countDotComma = CountSplit(source, ";");
            var countComma = CountSplit(source, ",");

            if (countComma > countDotComma && countComma > countPipe) { return ","; }
            if (countPipe > countDotComma && countPipe > countComma) { return "|"; }
            if (countDotComma > countPipe && countDotComma > countComma) { return ";"; }

            return "NO SPLIT ITEM";
        }

        /// <summary>
        /// CountSplit; count number of delimeter item in string. 
        /// </summary>
        /// <param name="source">text to count in</param>
        /// <param name="delimiter">possble delimiter</param>
        /// <returns>number of specific delimiter char in text.</returns>
        private int CountSplit(string source, string delimiter)
        {
            int count = 0;
            foreach (char c in source)
                if (c == Convert.ToChar(delimiter)) count++;
            return count;
        }

        #region - logging -
        /// <summary>
        /// AddProvisionerLogEvent; Adds item to provisioner log. 
        /// </summary>
        /// <param name="message">Message to add</param>
        /// <param name="eventId">Possible event id</param>
        /// <param name="type">Type of message</param>
        /// <param name="eventName">Possible event name</param>
        /// <param name="description">Description, containing more details information if available. </param>
        /// <returns>true/false (will mostly be ignored, but can be used if needed.)</returns>
        public async Task<bool> AddProvisionerLogEvent(string message, int eventId = 0, string type = "INFORMATION", string eventName = "", string description = "")
        {
            if (_configHelper.GetValueAsBool("AppSettings:EnableDbLogging"))
            {
                try
                {
                    var source = _configHelper.GetValueAsString("AppSettings:ApplicationName");

                    var parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_message", message.Length > 255 ? message.Substring(0, 254) : message));
                    parameters.Add(new NpgsqlParameter("@_type", type));
                    parameters.Add(new NpgsqlParameter("@_eventid", eventId.ToString()));
                    parameters.Add(new NpgsqlParameter("@_eventname", eventName));

                    if (string.IsNullOrEmpty(source))
                    {
                        parameters.Add(new NpgsqlParameter("@_source", ""));
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_source", source));
                    }
                    parameters.Add(new NpgsqlParameter("@_description", description));

                    var output = await _dbmanager.ExecuteScalarAsync("add_logging_provisioner", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                }
                catch (Exception ex)
                {
                    // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                    _logger.LogError(exception: ex, message: string.Concat("ProvisionerManager.AddProvisionerLogEvent(): ", ex.Message));
                }
                finally
                {

                }
            }
            return true;
        }
        #endregion

    }
}
