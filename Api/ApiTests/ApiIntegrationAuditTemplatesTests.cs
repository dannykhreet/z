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
    [Order(14)]
    [TestFixture]
    public class ApiIntegrationAuditTemplatesTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region - Retrieve Collections -
        [Test]
        [TestCase("")]
        [TestCase("?iscompleted=true", Description = "Only completed")]
        [TestCase("?iscompleted=false", Description = "Only not completed")]
        [TestCase("?allowedonly=true", Description = "Only allowed only")]
        [TestCase("?instructionsadded=true", Description = "Instructions added")]
        [TestCase("?imagesadded=true", Description = "Images added")]
        [TestCase("?include=tasktemplates", Description = "Include tasks")]
        [TestCase("?include=steps", Description = "Include steps")]
        [TestCase("?include=openfields", Description = "Include openfields")]
        [TestCase("?include=tags", Description = "Include tags")]
        [TestCase("?include=userinformation", Description = "Include user information")]
        [TestCase("?include=propertyvalues", Description = "Include property values")]
        [TestCase("?include=properties", Description = "Include properties")]
        [TestCase("?include=instructions", Description = "Include tags")]
        [TestCase("?include=instructionrelations", Description = "Include tags")]
        [TestCase("?include=areapaths", Description = "Include areapaths")]
        [TestCase("?include=tasktemplates,steps,openfields,tags,userinformation,propertyvalues,properties,instructions,instructionrelations,areapaths", Description = "Include all")]
        public async Task TestRetrieveAuditTemplates(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/audittemplates", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        [TestCase("")]
        [TestCase("?iscompleted=true", Description = "Only completed")]
        [TestCase("?iscompleted=false", Description = "Only not completed")]
        [TestCase("?allowedonly=true", Description = "Only allowed only")]
        [TestCase("?instructionsadded=true", Description = "Instructions added")]
        [TestCase("?imagesadded=true", Description = "Images added")]
        public async Task TestRetrieveAuditTemplateCounts(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/audittemplates/counts", query));
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
        [TestCase("false", Description = "Set audittemplate inactive")]
        [TestCase("true", Description = "Set audittemplate active")]

        public async Task TestSetAuditInactive(string jsonBody)
        {
            var auditInactiveId = 3502; //Specifically created audit template for isActive Test

            var resp = await PostResponse(string.Concat("v1/audittemplate/setactive/", auditInactiveId), Convert.ToBoolean(jsonBody).ToJsonFromObject(), addCmsHeader:true);
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        #endregion

        #region - Chains -


        #endregion

    }
}
