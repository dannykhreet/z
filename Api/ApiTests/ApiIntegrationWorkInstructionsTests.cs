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
    [Order(8)]
    [TestFixture]
    public class ApiIntegrationWorkInstructionsTests : Base.BaseTests
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
            var resp = await GetResponse(string.Concat("/v1/workinstructions", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Retrieve Individuals -
        [Test]
        [TestCase("316", Description = "Retrieve simple workinstruction")]
        public async Task TestRetrieveWorkInstructionById(string id)
        {
            var resp = await GetResponse(string.Concat("/v1/workinstruction/", id));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
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
