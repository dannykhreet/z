using ApiTests.Helpers;
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
    [Order(7)]
    [TestFixture]
    public class ApiIntegrationAssessmentsTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region - Retrieve Collections -
        [Test]
        [TestCase("")]
        public async Task TestRetrieveAssessments(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/assessments", query));
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
        [Test]
        [TestCase("false", Description = "Set assessment inactive")]
        [TestCase("true", Description = "Set assessment active")]

        public async Task TestSetAsssessmentInactive(string jsonBody)
        {
            var assessmentInactiveId = 2902; //Specifically created assessment for isActive Test
            var resp = await PostResponse(string.Concat("/v1/assessment/setactive/", assessmentInactiveId), Convert.ToBoolean(jsonBody).ToJsonFromObject());
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        #endregion

        #region - Chains -


        #endregion

    }
}
