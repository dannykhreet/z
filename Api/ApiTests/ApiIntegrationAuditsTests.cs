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
    [Order(3)]
    [TestFixture]
    public class ApiIntegrationAuditsTests : Base.BaseTests
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
        [TestCase("?include=tasks", Description = "Include tasks")]
        [TestCase("?include=openfields", Description = "Include openfields")]
        [TestCase("?include=tags", Description = "Include tags")]
        [TestCase("?include=userinformation", Description = "Include user information")]
        [TestCase("?include=propertyuservalues", Description = "Include property user information")]
        [TestCase("?include=properties", Description = "Include properties")]
        [TestCase("?include=pictureproof", Description = "Include tags")]
        [TestCase("?include=areapaths", Description = "Include areapaths")]
        [TestCase("?include=tasks,openfields,tags,userinformation,propertyuservalues,properties,pictureproof,areapaths", Description = "Include all")]
        public async Task TestRetrieveAudits(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/audits", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Retrieve Individuals -

        #endregion

        #region - Send new items -
        [Test]
        public async Task TestAddAudit()
        {
            var resp = await PostResponse("/v1/audit/add", Helpers.RunHelpers.GetResource("audit_add_large.json", _companyid, _userid));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Send Existing items -
        [Test]
        [TestCase("50", Description = "Audits - Set score 50")]
        [TestCase("100", Description = "Audits - Set score 100")]
        public async Task TestAddChecklist(string jsonobject)
        {
            var id = 14791; //audit id specifically created for test.
            var resp = await PostResponse(string.Concat("/v1/audit/setscore/", id), Convert.ToInt32(jsonobject).ToJsonFromObject());
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        #endregion

        #region - Other functionalities -
        [Test]
        [TestCase("false", Description = "Set audit inactive")]
        [TestCase("true", Description = "Set audit active")]

        public async Task TestSetAuditInactive(string jsonBody)
        {
            var auditInactiveId = 14783; //Specifically created audit for isActive Test
            var resp = await PostResponse(string.Concat("/v1/audit/setactive/", auditInactiveId), Convert.ToBoolean(jsonBody).ToJsonFromObject());
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Chains -


        #endregion

    }
}
