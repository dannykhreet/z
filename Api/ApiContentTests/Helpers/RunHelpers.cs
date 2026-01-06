using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ApiContentTests.Base;
using System.Diagnostics;
using System.Net.Http.Json;
using System.IO;

namespace ApiContentTests.Helpers
{
    public static class RunHelpers
    {
        /// <summary>
        /// SetEnvironmentalVariables(); set env. variables for test run.
        /// </summary>
        public static void SetEnvironmentalVariables()
        {
            if(Environment.OSVersion.ToString().Contains("Microsoft Windows"))
            {
                //for running locally, cuz of reasons test projects DO NOT have local environmental variables.
                using (var file = File.OpenText("..\\..\\..\\Properties\\launchSettings.json"))
                {
                    var reader = new JsonTextReader(file);
                    var jObject = JObject.Load(reader);

                    if (jObject != null)
                    {
#pragma warning disable CS8604 // Possible null reference argument.
                        var variables = jObject.GetValue("profiles").SelectMany(profiles => profiles.Children()).SelectMany(profile => profile.Children<JProperty>()).Where(prop => prop.Name == "environmentVariables").SelectMany(prop => prop.Value.Children<JProperty>()).ToList();
#pragma warning restore CS8604 // Possible null reference argument.

                        foreach (var variable in variables)
                        {
                            Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                        }
                    }

                }
            }

        }

        /// <summary>
        /// GetResource; Retrieve resource (json, not fully valid, can contain parameters to be replaced.), for sending with post calls. 
        /// Resources can be predetermined to push the same kind of data to the API. 
        /// NOTE! only extend parameters based on general id data NOT specific meta fields of objects. If needed create seperate method for that. 
        /// </summary>
        /// <param name="resourceName">Name of the item.</param>
        /// <param name="companyId">Company id, for replacing within JSON</param>
        /// <param name="userId">User id, for replacing within JSON</param>
        /// <param name="id">Possible id of object for updating</param>
        /// <returns>string containing the JSON with updated/replaced keys for further implementation/use.</returns>
        public static string GetResource(string resourceName, int companyId, int userId, int id = 0)
        {
            // Get the assembly where the resource is embedded
            Assembly assembly = Assembly.GetExecutingAssembly();

            if(assembly != null)
            {
                // Read the embedded resource
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                using Stream stream = assembly.GetManifestResourceStream(string.Concat("ApiTests.Helpers.Objects.", resourceName));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (stream == null)
                {
                    Console.WriteLine("Resource not found.");
                    return string.Empty;
                }

                using StreamReader reader = new StreamReader(stream);
                string jsonContent = reader.ReadToEnd();

                //Replace certain keys, add more if needed. 
                return jsonContent
                    .Replace("||COMPANYID||", companyId.ToString())
                    .Replace("||USERID||", userId.ToString())
                    .Replace("||NOWUTC||", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"))
                    .Replace("||NOW+1M||", DateTime.Now.AddMonths(1).ToString("yyyy-MM-ddTHH:mm:ss"))
                    .Replace("||NOW||", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"))
                    .Replace("||ID||", id.ToString());
            }

            return string.Empty;
          
        }

        /// <summary>
        /// ParseQuery; Parse querystring within a uri to dynamically replace data to make it more dynamic based on the test that is needed.
        /// </summary>
        /// <param name="query">Query containing a string and or keys that need to be replaced.</param>
        /// <param name="companyId">Company id, for replacing within string</param>
        /// <param name="userId">User id, for replacing within string</param>
        /// <returns>string containing the string with updated/replaced keys for further implementation/use.</returns>
        public static string ParseQuery(string query, int companyId, int userId)
        {

            return query
                  .Replace("||COMPANYID||", companyId.ToString())
                  .Replace("||USERID||", userId.ToString())
                  .Replace("||NOWUTC||", DateTime.Now.ToUniversalTime().ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC-1D||", DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC-2D||", DateTime.Now.AddDays(-2).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC-3D||", DateTime.Now.AddDays(-3).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC-4D||", DateTime.Now.AddDays(-4).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC-5D||", DateTime.Now.AddDays(-5).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC-6D||", DateTime.Now.AddDays(-6).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC-7D||", DateTime.Now.AddDays(-7).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC+1D||", DateTime.Now.AddDays(1).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC+2D||", DateTime.Now.AddDays(2).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC+3D||", DateTime.Now.AddDays(3).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC+4D||", DateTime.Now.AddDays(4).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC+5D||", DateTime.Now.AddDays(5).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC+6D||", DateTime.Now.AddDays(6).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTC+7D||", DateTime.Now.AddDays(7).ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTCBEGINOFWEEK||", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"))
                  .Replace("||NOWUTCENDOFWEEK||", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

        }

    }
}
