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
    [Order(2)]
    [TestFixture]
    public class ApiIntegrationTasksTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region - Retrieve Collections -
        /* Routes for tasks are seperated to avoid timeout issues see below
         
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof,tags,userinformation")]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof,tags")]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof")]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation")]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues")]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues")]
        [TestCase("?include=steps,areapaths,areapathids")]
        [TestCase("?include=steps,areapaths")]
        [TestCase("?include=steps")]
        [TestCase("?include=areapaths")]
        [TestCase("?include=propertyuservalues")]
        [TestCase("?include=propertvalues")]
        [TestCase("?include=instructionrelation")]
        [TestCase("?include=pictureproof")]
        [TestCase("?include=tags")]
        [TestCase("?include=userinformation")]
        */


        [Order(0)]
        [Test]
        [TestCase("")]
        public async Task TestRetrieveTasks(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        /* Routes for tasks overdue are seperated to avoid timeout issues see below

        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof,tags,userinformation")]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof,tags")]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof")]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation")]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues")]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues")]
        [TestCase("?include=steps,areapaths,areapathids")]
        [TestCase("?include=steps,areapaths")]
        [TestCase("?include=steps")]
        [TestCase("?include=areapaths")]
        [TestCase("?include=propertyuservalues")]
        [TestCase("?include=propertvalues")]
        [TestCase("?include=instructionrelation")]
        [TestCase("?include=pictureproof")]
        [TestCase("?include=tags")]
        [TestCase("?include=userinformation")]
       */

        [Order(1)]
        [Test]
        [TestCase("")]
        public async Task TestRetrieveTaskOverdue(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(2)]
        [TestCase("")]
        [TestCase("?timestamp=||NOWUTC||")]
        public async Task TestRetrieveTaskStatusses(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/statusses", ApiTests.Helpers.RunHelpers.ParseQuery(query, _companyid, _userid)));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(3)]
        [TestCase("")]
        [TestCase("?from=||NOWUTC-1D||&to=||NOWUTC||")]
        public async Task TestRetrieveTaskStatussesPeriod(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/statusses/period", ApiTests.Helpers.RunHelpers.ParseQuery(query, _companyid, _userid)));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(4)]
        [TestCase("")]
        [TestCase("?timestamp=||NOWUTC||")]
        public async Task TestRetrieveTaskExtended(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/extendeddata", ApiTests.Helpers.RunHelpers.ParseQuery(query, _companyid, _userid)));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(5)]
        [TestCase("")]
        [TestCase("?timestamp=||NOWUTC||")]
        public async Task TestRetrieveTaskHistory(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/history", ApiTests.Helpers.RunHelpers.ParseQuery(query, _companyid, _userid)));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(6)]
        [TestCase("")]
        [TestCase("?timestamp=||NOWUTC||")]
        public async Task TestRetrieveTaskHistoryFirsts(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/historyfirsts", ApiTests.Helpers.RunHelpers.ParseQuery(query, _companyid, _userid)));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(7)]
        [TestCase("")]
        [TestCase("?timestamp=||NOWUTC||")]
        public async Task TestRetrieveTasksPreviousShift(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/previousshift", ApiTests.Helpers.RunHelpers.ParseQuery(query, _companyid, _userid)));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(8)]
        [TestCase("")]
        [TestCase("?timestamp=||NOWUTC||")]
        public async Task TestRetrieveTasksShift(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/shift", ApiTests.Helpers.RunHelpers.ParseQuery(query, _companyid, _userid)));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(9)]
        [TestCase("")]
        [TestCase("?timestamp=||NOWUTC||")]
        public async Task TestRetrieveTasksYesterday(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/yesterday", ApiTests.Helpers.RunHelpers.ParseQuery(query, _companyid, _userid)));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(10)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof,tags,userinformation")]
        public async Task TestRetrieveTasks10(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(11)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof,tags")]
        public async Task TestRetrieveTasks11(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(12)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof")]
        public async Task TestRetrieveTasks12(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(13)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation")]
        public async Task TestRetrieveTasks13(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(14)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation")]
        public async Task TestRetrieveTasks14(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(15)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues")]
        public async Task TestRetrieveTasks15(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(16)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues")]
        public async Task TestRetrieveTasks16(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(17)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids")]
        public async Task TestRetrieveTasks17(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(18)]
        [Test]
        [TestCase("?include=steps,areapaths")]
        public async Task TestRetrieveTasks18(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(19)]
        [Test]
        [TestCase("?include=steps")]
        public async Task TestRetrieveTasks19(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(20)]
        [Test]
        [TestCase("?include=areapaths")]
        public async Task TestRetrieveTasks20(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }


        [Order(21)]
        [Test]
        [TestCase("?include=propertyuservalues")]
        public async Task TestRetrieveTasks21(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(22)]
        [Test]
        [TestCase("?include=propertvalues")]
        public async Task TestRetrieveTasks22(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(23)]
        [Test]
        [TestCase("?include=instructionrelation")]
        public async Task TestRetrieveTasks23(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(24)]
        [Test]
        [TestCase("?include=pictureproof")]
        public async Task TestRetrieveTasks24(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(25)]
        [TestCase("?include=tags")]
        public async Task TestRetrieveTasks25(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(26)]
        [Test]
        [TestCase("?include=userinformation")]
        public async Task TestRetrieveTasks26(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(30)]
        [Test]
        [TestCase("")]
        public async Task TestRetrieveTaskOverdue30(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(31)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof,tags,userinformation")]
        public async Task TestRetrieveTaskOverdue31(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(32)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof,tags")]
        public async Task TestRetrieveTaskOverdue32(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(33)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation,pictureproof")]
        public async Task TestRetrieveTaskOverdue33(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(34)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues,instructionrelation")]
        public async Task TestRetrieveTaskOverdue34(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(35)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues,propertvalues")]
        public async Task TestRetrieveTaskOverdue35(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(36)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids,propertyuservalues")]
        public async Task TestRetrieveTaskOverdue36(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(37)]
        [Test]
        [TestCase("?include=steps,areapaths,areapathids")]
        public async Task TestRetrieveTaskOverdue37(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(38)]
        [Test]
        [TestCase("?include=steps,areapaths")]
        public async Task TestRetrieveTaskOverdue38(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(39)]
        [Test]
        [TestCase("?include=steps")]
        public async Task TestRetrieveTaskOverdue39(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(40)]
        [Test]
        [TestCase("?include=areapaths")]
        public async Task TestRetrieveTaskOverdue40(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(41)]
        [Test]
        [TestCase("?include=propertyuservalues")]
        public async Task TestRetrieveTaskOverdue41(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(42)]
        [Test]
        [TestCase("?include=propertvalues")]
        public async Task TestRetrieveTaskOverdue42(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(43)]
        [Test]
        [TestCase("?include=instructionrelation")]
        public async Task TestRetrieveTaskOverdue43(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(44)]
        [Test]
        [TestCase("?include=pictureproof")]
        public async Task TestRetrieveTaskOverdue44(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(45)]
        [Test]
        [TestCase("?include=tags")]
        public async Task TestRetrieveTaskOverdue45(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Order(46)]
        [Test]
        [TestCase("?include=userinformation")]
        public async Task TestRetrieveTaskOverdue46(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/tasks/overdue", query));
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
