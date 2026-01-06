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
    [Order(17)]
    [TestFixture]
    public class ApiIntegrationTaskTemplatesTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region - Retrieve Collections -
        [Test]
        [TestCase("")]
        [TestCase("?allowedonly=true")]
        [TestCase("?include=steps")]
        [TestCase("?include=areapaths")]
        [TestCase("?include=propertyuservalues")]
        [TestCase("?include=propertvalues")]
        [TestCase("?include=properties")]
        [TestCase("?include=instructionrelation")]
        [TestCase("?include=instructions")]
        [TestCase("?include=tags")]
        [TestCase("?include=userinformation")]
        [TestCase("?include=recurrency")]
        [TestCase("?include=steps,areapaths")]
        [TestCase("?include=steps,areapaths,propertyuservalues")]
        [TestCase("?include=steps,areapaths,propertyuservalues,propertvalues")]
        [TestCase("?include=steps,areapaths,propertyuservalues,propertvalues,properties")]
        [TestCase("?include=steps,areapaths,propertyuservalues,propertvalues,properties,instructionrelation")]
        [TestCase("?include=steps,areapaths,propertyuservalues,propertvalues,properties,instructionrelation,tags")]
        [TestCase("?include=steps,areapaths,propertyuservalues,propertvalues,properties,instructionrelation,tags,userinformation")]
        [TestCase("?include=steps,areapaths,propertyuservalues,propertvalues,properties,instructionrelation,tags,userinformationmrecurrency")]
        public async Task TestRetrieveTaskTemplates(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasktemplates", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Retrieve Individuals -

        #endregion

        #region - Send new items -
        [Test]
        [TestCase("tasktemplate_dailyinterval_2_day_new", Description = "Add TaskTemplate Daily Interval Every 2 Days")]
        [TestCase("tasktemplate_dailyintervaldynamic_2day_new", Description = "Add TaskTemplate Daily Interval Dynamic Every 2 Days")]
        [TestCase("tasktemplate_month_1_day_1_new", Description = "Add TaskTemplate Month Every 1 Month Day 1")]
        [TestCase("tasktemplate_norecurrency_new", Description = "Add TaskTemplate No Recurrency")]
        [TestCase("tasktemplate_shifts_all_new", Description = "Add TaskTemplate Shifts All Shifts")]
        [TestCase("tasktemplate_week_1_monday_new", Description = "Add TaskTemplate Every 1 Week On Monday")]
        public async Task TestAddTaskTemplate(string jsonobject)
        {
            var resp = await PostResponse("/v1/tasktemplate/add", Helpers.RunHelpers.GetResource(string.Concat(jsonobject, ".json"), _companyid, _userid), addCmsHeader: true);
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Send Existing items -


        #endregion

        #region - Other functionalities -


        #endregion

        #region - Chains -


        #endregion

    }
}
