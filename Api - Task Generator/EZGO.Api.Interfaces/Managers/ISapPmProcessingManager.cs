using EZGO.Api.Models;
using EZGO.Api.Models.Authentication;
using EZGO.Api.Models.SapPm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface ISapPmProcessingManager
    {
        Task<bool> ProcessSapPmNotificationResponseAsync(int notificationId, int actionId, int companyId, bool success, string response, long? sapNotificationId);
    }

}