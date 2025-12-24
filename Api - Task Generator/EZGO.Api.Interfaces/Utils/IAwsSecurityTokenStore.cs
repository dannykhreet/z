using EZGO.Api.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Utils
{
    public interface IAwsSecurityTokenStore
    {
        Task<MediaToken> FetchMediaToken();
    }
}
