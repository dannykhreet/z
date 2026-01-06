using ApiTests.Helpers;
using EZGO.Api.Models;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiTests
{
    [Order(18)]
    [TestFixture]
    public class ApiIntegrationWorkInstructionTemplatesTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region - Retrieve Collections -
        [Test]
        [TestCase("")]
        public async Task TestRetrieveWorkInstructions(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/workinstructiontemplates", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Retrieve Individuals -
        [Test]
        [TestCase("316", Description = "Retrieve simple workinstruction template")]
        public async Task TestRetrieveWorkInstructionById(string id)
        {
            var resp = await GetResponse(string.Concat("/v1/workinstructiontemplate/", id));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Send new items -
        [Test]
        [TestCase("workinstructiontemplate_new", Description = "Add WorkInstruction template")]
        public async Task TestAddWorkInstructionTemplate(string jsonobject)
        {
            var resp = await PostResponse("/v1/workinstructiontemplate/add", Helpers.RunHelpers.GetResource(string.Concat(jsonobject, ".json"), _companyid, _userid), addCmsHeader: true);
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Send Existing items -
        [Test]
        [TestCase("workinstructiontemplate_edit", Description = "Edit workinstruction template")]
        public async Task TestChangeWorkInstructionTemplate(string jsonobject)
        {
            var id = Helpers.RunHelpers.GetResource(string.Concat(jsonobject, ".json"), _companyid, _userid).ToObjectFromJson<BaseObject>().Id;
            var resp = await PostResponse(string.Concat("/v1/workinstructiontemplate/change/", id), Helpers.RunHelpers.GetResource(string.Concat(jsonobject, ".json"), _companyid, _userid), addCmsHeader: true);
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        #endregion

        #region - Other functionalities -
        [Test]
        [TestCase("false", Description = "Set workinstruction inactive")]
        [TestCase("true", Description = "Set workinstruction active")]

        public async Task TestSetChecklistTemplateInactive(string jsonBody)
        {
            var workinstructionTemplateInactiveId = 317; //Specifically created assessment for isActive Test
            var resp = await PostResponse(string.Concat("/v1/workinstructiontemplate/delete/", workinstructionTemplateInactiveId), Convert.ToBoolean(jsonBody).ToJsonFromObject(), addCmsHeader: true);
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        #endregion

        #region - Chains -


        #endregion

    }
}
