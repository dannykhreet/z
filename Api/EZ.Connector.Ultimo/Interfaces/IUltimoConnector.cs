using EZ.Connector.Ultimo.Models;
using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZ.Connector.Ultimo.Interfaces
{
    public interface IUltimoConnector
    {
        public Task<bool> SendActionToUltimoAsync(ActionsAction action, int companyId, int userId);
        public Task<bool> CheckConnection();
        public bool CheckCompanyForConnector(int companyId);
        public Task<UltimoConfig> GetConfiguration(int companyId);

        public Task<bool> HandleOutput(string responseMessage);
    }
}
