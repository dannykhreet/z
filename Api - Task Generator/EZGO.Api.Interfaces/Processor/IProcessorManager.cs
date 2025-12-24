using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Processor
{
    public interface IProcessorManager
    {
          Task<bool> AddProcessorLogEvent(string message, int eventId = 0, string type = "INFORMATION", string eventName = "", string description = "");
    }
}
