using EZ.Connector.Solvace.Interfaces;
using EZ.Connector.Solvace.Models;
using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZ.Connector.Solvace.Managers
{
    /// <summary>
    /// SolvaceConnector; Connector for SOLVACE connectivity.
    /// NOTE! only for use within the SOLVACE connector or SOLVACE connector derivatives
    /// NOTE! current SOLVACE implementation is set on hold!
    /// </summary>
    public class SolvaceConnector : ISolvaceConnector
    {
        public bool CheckCompanyForConnector(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckConnection()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendActionToSolvaceAsync(ActionsAction action, int companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<SolvaceConfig> GetConfiguration(int companyId)
        {
            await Task.CompletedTask;
            return new SolvaceConfig();
        }
    }
}
