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
    [Order(10)]
    [TestFixture]
    public class ApiIntegrationFeedsTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region - Retrieve Collections -
        [Test]
        [TestCase("")]
        public async Task TestRetrieveFeeds(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/feeds", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Retrieve Individuals -

        #endregion

        #region - Send new items -

        [Test]
        [TestCase("feed_item_new", Description = "Add feed item")]
        [TestCase("feed_item_with_images_new", Description = "Add feed item - With images")]
        [TestCase("feed_item_with_video_new", Description = "Add feed item - With video")]
        public async Task TestAddFeedItem(string jsonobject)
        {
            var resp = await PostResponse("/v1/feeds/item/add", Helpers.RunHelpers.GetResource(string.Concat(jsonobject, ".json"), _companyid, _userid), true);
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
