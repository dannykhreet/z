using EZ.Connector.SAP.Models;
using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZ.Connector.SAP.Interfaces
{
    public interface ISAPConnector
    {
        public Task<bool> SendActionToSAPAsync(ActionsAction action, int companyId);
        public Task<bool> CheckConnection();
        public bool CheckCompanyForConnector(int companyId);
        public Task<SAPConfig> GetConfiguration(int companyId);
    }
}
