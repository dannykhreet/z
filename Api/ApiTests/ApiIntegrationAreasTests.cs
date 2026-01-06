using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiTests
{
    [Order(11)]
    [TestFixture]
    public class ApiIntegrationAreaTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region - Retrieve Collections -
        [Test]
        [TestCase("")]
        [TestCase("?allowedonly=true", Description = "Allowed only")]
        [TestCase("?maxlevel=0", Description = "Max level 0")]
        [TestCase("?maxlevel=1", Description = "Max level 1")]
        [TestCase("?maxlevel=2", Description = "Max level 2")]
        [TestCase("?include=companyroot", Description = "Company root")]
        public async Task TestRetrieveAreas(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/areas", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Retrieve Individuals -

        #endregion

        #region - Send new items -

        #endregion

        #region - Send Existing items -


        #endregion

        #region - Other functionalities -


        #endregion

        #region - Chains -


        #endregion

    }
}
