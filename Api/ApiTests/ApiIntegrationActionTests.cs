using ApiTests.Helpers;
using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiTests
{
    [Order(5)]
    [TestFixture]
    public class ApiIntegrationActionTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region - Retrieve Collections -
        [Test]
        [TestCase("")]
        [TestCase("?isresolved=true", Description = "Only resolved")]
        [TestCase("?isresolved=false", Description = "Only not resolved")]
        [TestCase("?include=unviewedcommentnr", Description = "Include unviewed comment nr")]
        [TestCase("?include=mainparent", Description = "Include main parent")]
        [TestCase("?include=assignedareas", Description = "Include assigned areas")]
        [TestCase("?include=assignedusers", Description = "Include assigned users")]
        [TestCase("?include=userinformation", Description = "Include user information")]
        [TestCase("?include=tags", Description = "Include tags")]
        [TestCase("?include=unviewedcommentnr,mainparent,assignedareas,assignedusers,userinformation,tags", Description = "Include all")]
        public async Task TestRetrieveActions(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/actions", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        [TestCase("")]
        [TestCase("?isresolved=true", Description = "Only resolved")]
        [TestCase("?isresolved=false", Description = "Only not resolved")]
        public async Task TestRetrieveActionCounts(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/actions/counts", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }


        [Test]
        public async Task TestRetrieveActionComments()
        {
            var resp = await GetResponse("/v1/actioncomments");
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task TestRetrieveActionAssignedUsers()
        {
            var resp = await GetResponse("/v1/actions/assignedusers");
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task TestRetrieveActionAssignedAreas()
        {
            var resp = await GetResponse("/v1/actions/assignedareas");
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task TestRetrieveActionCommentsUnread()
        {
            var resp = await GetResponse("/v1/actioncomments/unread");
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task TestRetrieveActionCommentsUpdateCheck()
        {
            var resp = await GetResponse("/v1/actioncomments/updatecheck");
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Retrieve Individuals -

        #endregion

        #region - Send new items -

        [Test]
        [TestCase("action_new", Description = "Add Action")]
        [TestCase("action_new_with_resource_user", Description = "Add Action - With Resource User")]
        [TestCase("action_new_with_resource_area", Description = "Add Action - With Resource Area")]
        public async Task TestAddAction(string jsonobject)
        {
            var resp = await PostResponse("/v1/action/add", Helpers.RunHelpers.GetResource(string.Concat(jsonobject, ".json"), _companyid, _userid));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        #endregion

        #region - Send Existing items -
        [Test]
        public async Task TestChangeAction()
        {
            var actionJson = Helpers.RunHelpers.GetResource("action_edit.json", _companyid, _userid);
            var id = actionJson.ToObjectFromJson<BaseObject>().Id;
            var resp = await PostResponse(string.Concat("/v1/action/change/", id.ToString()), actionJson);
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task TestChangeActionRemoveResourcesArea()
        {
            var resp = await PostResponse("/v1/action/add", Helpers.RunHelpers.GetResource(string.Concat("action_new_with_resource_area", ".json"), _companyid, _userid));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await resp.Content.ReadAsStringAsync());

            var actionRemoveJson = Helpers.RunHelpers.GetResource("action_edit_remove_resource_area.json", _companyid, _userid, id);
            var respActionRemove = await PostResponse(string.Concat("/v1/action/change/", id.ToString()), actionRemoveJson);
            HttpStatusCode statusActionRemove = respActionRemove.StatusCode;
            Assert.That(statusActionRemove, Is.EqualTo(HttpStatusCode.OK));

        }

        [Test]
        public async Task TestChangeActionRemoveResourcesUser()
        {
            var resp = await PostResponse("/v1/action/add", Helpers.RunHelpers.GetResource(string.Concat("action_new_with_resource_user", ".json"), _companyid, _userid));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await resp.Content.ReadAsStringAsync());

            var actionRemoveJson = Helpers.RunHelpers.GetResource("action_edit_remove_resource_user.json", _companyid, _userid, id);
            var respActionRemove = await PostResponse(string.Concat("/v1/action/change/", id.ToString()), actionRemoveJson);
            HttpStatusCode statusActionRemove = respActionRemove.StatusCode;
            Assert.That(statusActionRemove, Is.EqualTo(HttpStatusCode.OK));

        }



        #endregion

        #region - Other functionalities -
        [Test]
        public async Task TestSetActionResolved()
        {
            var resp = await PostResponse("/v1/action/add", Helpers.RunHelpers.GetResource("action_new.json", _companyid, _userid));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await resp.Content.ReadAsStringAsync());

            var respSetResolved = await PostResponse(string.Concat("/v1/action/setresolved/", id.ToString()), true.ToJsonFromObject().ToString());
            HttpStatusCode statusSetResolved = respSetResolved.StatusCode;
            Assert.That(statusSetResolved, Is.EqualTo(HttpStatusCode.OK));

            
        }

        [Test]
        public async Task TestSetActionResolvedCheckComment()
        {
            var resp = await PostResponse("/v1/action/add", Helpers.RunHelpers.GetResource("action_new.json", _companyid, _userid));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var id = Convert.ToInt32(await resp.Content.ReadAsStringAsync());

            var respSetResolved = await PostResponse(string.Concat("/v1/action/setresolved/", id.ToString()), true.ToJsonFromObject().ToString());
            HttpStatusCode statusSetResolved = respSetResolved.StatusCode;
            Assert.That(statusSetResolved, Is.EqualTo(HttpStatusCode.OK));

            //actioncomments
            var respComments = await GetResponse(string.Concat("/v1/actioncomments?actionid=", id.ToString()));
            HttpStatusCode statusComments = resp.StatusCode;
            List<ActionComment> comments = (await respComments.Content.ReadAsStringAsync()).ToObjectFromJson<List<ActionComment>>();
            Assert.Multiple(() =>
            {
                //should contain 1 comment with action resolved.
                Assert.That(comments, Is.Not.EqualTo(null));
                Assert.That(comments, Is.Not.Empty);
                Assert.That(comments.FirstOrDefault().Comment, Is.EqualTo("The following items of this action have been changed: Completed"));
            });
        }
        #endregion

        #region - Chains -

        [Test]
        public async Task TestAddRetrieveAction()
        {
            var actionid = 0;
            var actionJson = Helpers.RunHelpers.GetResource("action_new.json", _companyid, _userid);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var actionToAdd = (ActionsAction)JsonSerializer.Deserialize(actionJson, typeof(ActionsAction));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            var responseAdd = await PostResponse("/v1/action/add", actionJson);
            HttpStatusCode statusAdd = responseAdd.StatusCode;
            Assert.That(statusAdd, Is.EqualTo(HttpStatusCode.OK));
            actionid = Convert.ToInt32(await responseAdd.Content.ReadAsStringAsync());

            var responseRetrieve = await GetResponse(string.Concat("/v1/action/", actionid));
            HttpStatusCode statusRetrieve = responseRetrieve.StatusCode;
            Assert.That(statusAdd, Is.EqualTo(HttpStatusCode.OK));
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var actionRetrieved = (ActionsAction)JsonSerializer.Deserialize(await responseRetrieve.Content.ReadAsStringAsync(), typeof(ActionsAction));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.That(actionToAdd.Comment == actionRetrieved.Comment && 
                        actionToAdd.Description == actionRetrieved.Description, Is.True);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        }

        #endregion

    }
}
