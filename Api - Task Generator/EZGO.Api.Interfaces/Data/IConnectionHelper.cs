using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Interfaces.Data
{
    /// <summary>
    /// IConnectionHelper; Interface for usage with .NetCore DI.
    /// Later this can also be used for testing purposes.
    /// </summary>
    public interface IConnectionHelper
    {
        string GetConnectionString();

        string GetConnectionStringReader();

        string GetConnectionStringWriter();

        string GetActiveDatabaseEnvironment();
    }
}
