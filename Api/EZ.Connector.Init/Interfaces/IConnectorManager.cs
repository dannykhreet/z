using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZ.Connector.Init.Interfaces
{
    public interface IConnectorManager
    {
        public Task<bool> InitConnectors(int companyId, int userId, ActionsAction action);
        public Task<bool> InitSAPConnector(int companyId, int userId, ActionsAction action);
        public Task<bool> InitUltimoConnector(int companyId, int userId, ActionsAction action);
    }
}
