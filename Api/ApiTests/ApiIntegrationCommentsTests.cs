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
    [Order(6)]
    [TestFixture]
    public class ApiIntegrationCommentsTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region - Retrieve Collections -
        [Test]
        [TestCase("")]
        public async Task TestRetrieveComments(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/comments", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        [TestCase("")]
        public async Task TestRetrieveCommentRelations(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/comments/relations", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        [TestCase("")]
        public async Task TestRetrieveCommentCounts(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/comments/counts", query));
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
