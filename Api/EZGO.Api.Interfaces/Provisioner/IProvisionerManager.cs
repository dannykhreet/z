using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Provisioner
{
    public interface IProvisionerManager
    {
        //direct execution
        Task<bool> Provision(int companyId, int userId, string type, string content);
        //automated execution
        Task<bool> ProvisionByHolding(int holdingId, string type, string content);
        //automated execution
        Task<bool> ProvisionByCompany(int companyId, string type, string content);

        Task<bool> AddProvisionerLogEvent(string message, int eventId = 0, string type = "INFORMATION", string eventName = "", string description = "");
    }
}
