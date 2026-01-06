using EZ.Connector.Solvace.Models;
using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZ.Connector.Solvace.Interfaces
{
    public interface ISolvaceConnector
    {
        public Task<bool> SendActionToSolvaceAsync(ActionsAction action, int companyId);
        public Task<bool> CheckConnection();
        public bool CheckCompanyForConnector(int companyId);
        public Task<SolvaceConfig> GetConfiguration(int companyId);
    }
}
