using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IQueueableItem
    {
        Guid LocalGuid { get; }
    }
}