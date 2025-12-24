using EZGO.Api.Models.Basic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IOfflineManager
    {
        Task<List<MediaBasic>> GetMediaUriAsync(int companyId);
        List<Exception> GetPossibleExceptions();
    }
}
