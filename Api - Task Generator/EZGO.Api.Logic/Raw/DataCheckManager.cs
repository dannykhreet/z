using EZGO.Api.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Raw;
using EZGO.Api.Logic.Base;
using EZGO.Api.Utils.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Raw
{
    /// <summary>
    /// DataCheckManager, checks functionality of database, getting top 10 items from each entity framework item and converting it to a JSON string.
    /// </summary>
    public class DataCheckManager : BaseManager<DataCheckManager>, IDataCheckManager
    {
        #region - private / properties -
        //private readonly EZGoContext _context; //database context (entity framework)
        private readonly IConnectionHelper _connectionHelper;
        private readonly ICompanyManager _companyManager;
        #endregion

        #region - constructor(s) -
        public DataCheckManager(ICompanyManager companyManager, IConnectionHelper connectionHelper, ILogger<DataCheckManager> logger) : base(logger)
        {
            //_context = context;
            _connectionHelper = connectionHelper;
            _companyManager = companyManager;
        }
        #endregion

        #region - methods -
        /// <summary>
        /// GetCompanies; Checks if companies can be retrieved, based on result true/false is returned and database access is determined. 
        /// </summary>
        /// <returns>true/false</returns>
        public async Task<bool> GetCompanies()
        {
            return (await _companyManager.GetCompaniesAsync()).Count > 0;
        }


        /// <summary>
        /// GetEnvironment; Get current data environment.
        /// </summary>
        /// <returns>Pre-defined string containing the type of environment the api is currenlty running at.</returns>
        public string GetEnvironment()
        {
            return _connectionHelper.GetActiveDatabaseEnvironment();
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}
