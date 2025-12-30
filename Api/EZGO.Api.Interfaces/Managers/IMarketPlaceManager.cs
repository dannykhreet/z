using EZGO.Api.Models;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Marketplace;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    /// <summary>
    /// IShiftManager, Interface for use with the ShiftManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface IMarketPlaceManager
    {
        Task<List<MarketPlaceItem>> GetMarketPlace(int companyId);
        Task<bool> SaveMarketPlaceConfiguration(int companyId, string configuration);

        List<Exception> GetPossibleExceptions();
    }
}
