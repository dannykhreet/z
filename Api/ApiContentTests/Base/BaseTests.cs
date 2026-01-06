using ApiContentTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiContentTests.Base
{
    /// <summary>
    /// BaseTests; Base class used in all test classes.
    /// </summary>
    public class BaseTests
    {
        ///public readonly string BASE_URI = "https://localhost:10001", "https://ezgo.testapi.ezfactory.nl";
        protected static string _baseUri = string.Empty;
        protected static string _internalToken = string.Empty;
        protected static int _companyid = 0;
        protected static int _userid = 0;

        [OneTimeSetUp]
        public void BaseOneTimeSetup()
        {
            RunHelpers.SetEnvironmentalVariables();
            if (_companyid == 0) _companyid = Convert.ToInt32(Environment.GetEnvironmentVariable("AUTOMATED_MAIN_TEST_USER_COMPANY_ID"));
            if (_userid == 0) _userid = Convert.ToInt32(Environment.GetEnvironmentVariable("AUTOMATED_MAIN_TEST_USER_ID"));
            if (string.IsNullOrEmpty(_internalToken)) _internalToken = RetrieveToken().Result;
        }

        /// <summary>
        /// RetrieveToken, retrieves a user token, will be used for getting and posting data.
        /// NOTE! when same user is called in a test run, there token will reset in the DB, so preferably run once per run.
        /// </summary>
        /// <returns>Token used for posts.</returns>
        public async Task<string> RetrieveToken()
        {
            using (var client = new HttpClient())
            {
#pragma warning disable CS8601 // Possible null reference assignment.
                _baseUri = Environment.GetEnvironmentVariable("AUTOMATED_BASE_URI");
#pragma warning restore CS8601 // Possible null reference assignment.
                var user = Environment.GetEnvironmentVariable("AUTOMATED_MAIN_TEST_USER");
                var pwd = Environment.GetEnvironmentVariable("AUTOMATED_MAIN_TEST_USER_PWD");
                var resp = await client.PostAsync (string.Concat(_baseUri, "/v1/authentication/login"), new StringContent(string.Concat("{\"UserName\" : \"",user,"\", \"Password\" : \"",pwd,"\"}"), System.Text.Encoding.UTF8, "application/json"));
                HttpStatusCode status = resp.StatusCode;
                if (status == HttpStatusCode.OK)
                {
                    var possibleValue = await resp.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(possibleValue))
                    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
                        return (string)JsonSerializer.Deserialize(possibleValue, typeof(string));
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    }
                }
            }

            return "";
          
        }

        /// <summary>
        /// AddAuthenticationHeaders; Add bearer tokens to header for authorized posts and gets.
        /// </summary>
        /// <param name="requestMessage"></param>
        public void AddAuthenticationHeaders(HttpRequestMessage requestMessage)
        {
            if (!string.IsNullOrEmpty(_internalToken)) {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);
            }
            
        }

        /// <summary>
        /// AddClientHeaderInformation; Add client header (user agent) to posts to API request.
        /// </summary>
        /// <param name="requestMessage">Message where the headers need to be added.</param>
        private void AddCMSClientHeaderInformation(HttpRequestMessage requestMessage)
        {
            try
            {
                if (requestMessage.Headers != null)
                {                   
                    var userAgentString = string.Format("{0} ({1}) {0}/{2}", "MY EZ-GO", "TEST Client", "0.0.0");
                    requestMessage.Headers.Add("User-Agent", userAgentString);
                }
#pragma warning disable CS0168 // Do not catch general exception types
            }
            catch (Exception ex)
            {
                //ignore it
            }
#pragma warning restore CS0168 // Do not catch general exception types
        }

        /// <summary>
        /// GetResponse; Get a response based on a URI
        /// </summary>
        /// <param name="uri">Uri containing the uri and or parameters (querystring) of the specific call.</param>
        /// <param name="addCmsHeader">Add CMS client header for use with CMS calls.</param>
        /// <returns>A response message.</returns>
        public async Task<HttpResponseMessage> GetResponse(string uri, bool addCmsHeader = false)
        {
            HttpResponseMessage result;
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                AddAuthenticationHeaders(requestMessage: requestMessage);
                if (addCmsHeader) { AddCMSClientHeaderInformation(requestMessage: requestMessage); };

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUri);
                    result = await client.SendAsync(requestMessage);
                }
            }
            return result;
        }

        /// <summary>
        /// PostResponse; Post response based on a URI and a JSON body.
        /// </summary>
        /// <param name="uri">Uri containing the uri and or parameters (querystring) of the specific call.</param>
        /// <param name="jsonBody">Json containing the data to be posted.</param>
        /// <param name="addCmsHeader">Add CMS client header for use with CMS calls.</param>
        /// <returns>A response message.</returns>
        public async Task<HttpResponseMessage> PostResponse(string uri, string jsonBody, bool addCmsHeader = false)
        {
            HttpResponseMessage result;
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                AddAuthenticationHeaders(requestMessage: requestMessage);
                if (addCmsHeader) { AddCMSClientHeaderInformation(requestMessage: requestMessage); };
                requestMessage.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUri);
                    result = await client.SendAsync(requestMessage);
                }
            }
            return result;
        }

    }
}
