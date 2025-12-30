using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Logic.Interfaces
{
    public interface IActionService
    {
        Task<int> MyCommentsCount();
    }
}
