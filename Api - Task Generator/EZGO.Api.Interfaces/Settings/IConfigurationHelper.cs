using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Interfaces.Settings
{
    public interface IConfigurationHelper
    {
        string GetValueAsString(string keyname);

        int GetValueAsInteger(string keyname);

        bool GetValueAsBool(string keyname);

        bool GetValueAsBoolBasedOnCompanyId(string keyname, int companyid);
    }
}
