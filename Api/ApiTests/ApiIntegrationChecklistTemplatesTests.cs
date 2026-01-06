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
    [Order(15)]
    [TestFixture]
    public class ApiIntegrationChecklistTemplatesTests : Base.BaseTests
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
        public async Task TestRetrieveChecklistTemplates(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/checklisttemplates", query));
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
        public async Task TestRetrieveChecklistTemplateCounts(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/checklisttemplates/counts", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }


        #endregion

        #region - Retrieve Individuals -

        #endregion

        #region - Send new items -
        [Test]
        [TestCase("checklisttemplate_new", Description = "Add ChecklistTemplate")]
        public async Task TestAddCheclistTempalte(string jsonobject)
        {
            var resp = await PostResponse("/v1/checklisttemplate/add", Helpers.RunHelpers.GetResource(string.Concat(jsonobject, ".json"), _companyid, _userid), addCmsHeader:true);
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Send Existing items -


        #endregion

        #region - Other functionalities -
        [Test]
        [TestCase("false", Description = "Set checklist inactive")]
        [TestCase("true", Description = "Set checklist active")]

        public async Task TestSetChecklistTemplateInactive(string jsonBody)
        {
            var checklistTemplateInactiveId = 5674; //Specifically created assessment for isActive Test
            var resp = await PostResponse(string.Concat("/v1/checklisttemplate/setactive/", checklistTemplateInactiveId), Convert.ToBoolean(jsonBody).ToJsonFromObject(), addCmsHeader: true);
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }


        #endregion

        #region - Chains -


        #endregion

    }
}
