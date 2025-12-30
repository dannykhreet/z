using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.SapPmConnector
{
    public interface ISapPmConnectionManager
    {
        Task<int> SendNotificationMessagesToSapPM(string companiesList = "");
        Task<int> SynchFunctionalLocations(string companiesList = "");
    }
}
