using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EZGO.Api.Models;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Core.Interfaces.Sign
{
    public interface ISignService
    {
        Task PostAndSignTemplateAsync(PostTemplateModel model);
    }
}
