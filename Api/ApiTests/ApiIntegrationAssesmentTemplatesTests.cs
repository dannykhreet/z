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
    [Order(13)]
    [TestFixture]
    public class ApiIntegrationAssessmentTemplatesTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region - Retrieve Collections -
        [Test]
        [TestCase("")]
        public async Task TestRetrieveAssessmentTemplates(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/assessmenttemplates", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Retrieve Individuals -

        #endregion

        #region - Send new items -
        [Test]
        [TestCase("assessmenttemplate_new", Description = "Add new - assessmenttemplate")]
        public async Task TestAddAssessmentTemplate(string jsonobject)
        {
            var resp = await PostResponse("/v1/assessmenttemplate/add", Helpers.RunHelpers.GetResource(string.Concat(jsonobject, ".json"), _companyid, _userid), addCmsHeader: true);
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Send Existing items -


        #endregion

        #region - Other functionalities -
        [Test]
        [TestCase("false", Description = "Set assessment inactive")]
        [TestCase("true", Description = "Set assessment active")]

        public async Task TestSetAsssessmentTemplateInactive(string jsonBody)
        {
            var assessmentTemplateInactiveId = 156; //Specifically created assessment for isActive Test
            var resp = await PostResponse(string.Concat("/v1/assessmenttemplate/setactive/", assessmentTemplateInactiveId), Convert.ToBoolean(jsonBody).ToJsonFromObject(), addCmsHeader:true);
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        #endregion

        #region - Chains -


        #endregion

    }
}
