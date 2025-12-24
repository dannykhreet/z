using Npgsql;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Helper
{
    public static class DataConnectorHelper
    {
        /// <summary>
        /// Constructs a SQL command string to invoke a PostgreSQL function with the specified parameters.
        /// </summary>
        /// <remarks>This method assumes that the function resides in the "public" schema of the PostgreSQL database. The
        /// parameter names in the resulting SQL command are prefixed with "@" to match the expected syntax for parameterized
        /// queries.</remarks>
        /// <param name="functionName">The name of the PostgreSQL function to be invoked. This value cannot be null or empty.</param>
        /// <param name="parameters">A list of <see cref="NpgsqlParameter"/> objects representing the parameters to be passed to the function. If the
        /// list is null or empty, the function will be invoked without parameters.</param>
        /// <returns>A string representing the SQL command to invoke the specified PostgreSQL function with the provided parameters. The
        /// resulting string is in the format:  <c>SELECT * FROM public.functionName(parameterName => @parameterName, ...)</c>.</returns>
        public static string WrapFunctionCommand(string functionName, List<NpgsqlParameter> parameters = null)
        {
            //SELECT * FROM public.get_userprofiles_basic(_companyid => @_companyid)
            StringBuilder result = new StringBuilder("SELECT * FROM public.");

            result.Append(functionName);

            result.Append("(");

            StringBuilder p = new StringBuilder();

            if (parameters != null)
            {
                foreach (NpgsqlParameter parameter in parameters)
                {
                    p.Append(parameter.ParameterName.Replace("@",""));
                    p.Append(" => @");
                    p.Append(parameter.ParameterName.Replace("@", ""));
                    p.Append(",");
                }
                result.Append(p.ToString().Trim(','));
            }

            result.Append(")");

            return result.ToString();
        }
    }
}