using ApiTests.Helpers;
using EZGO.Api.Models;
using System.Collections;
using System.Net;

namespace ApiTests
{
    [Order(4)]
    [TestFixture]
    public class ApiIntegrationChecklistsTests : Base.BaseTests
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
        public async Task TestRetrieveChecklists(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/checklists", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Retrieve Individuals -

        #endregion

        #region - Send new items -

        [Test]
        [TestCase("checklist_add_large", Description = "Add checklist - large checklist")]
        public async Task TestAddChecklist(string jsonobject)
        {
            var resp = await PostResponse("/v1/checklist/add", Helpers.RunHelpers.GetResource(string.Concat(jsonobject, ".json"), _companyid, _userid));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }
        #endregion

        #region - Send Existing items -

        [Test]
        [TestCaseSource(typeof(ChangeChecklistTestData), nameof(ChangeChecklistTestData.TestChecklists))]
        public async Task TestChangeChecklist(int checklistId, Checklist checklist, HttpStatusCode statusCode, string expectedMessage)
        {
            var resp = await PostResponse($"/v1/checklist/change/{checklistId}", checklist.ToJsonFromObject());
            HttpStatusCode status = resp.StatusCode;
            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(statusCode));
                Assert.That(resp.Content.ReadAsStringAsync().Result, Is.EqualTo((expectedMessage)));
            });
        }

        #endregion

        #region - Other functionalities -
        [Test]
        [TestCase("false", Description = "Set checklist inactive")]
        [TestCase("true", Description = "Set checklist active")]

        public async Task TestSetAuditInactive(string jsonBody)
        {
            var changelistInactiveId = 118946; //Specifically created checklist for isActive Test
            var resp = await PostResponse(string.Concat("/v1/checklist/setactive/", changelistInactiveId), Convert.ToBoolean(jsonBody).ToJsonFromObject());
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        #endregion

        #region - Chains -


        #endregion

        #region - Test Data -

        public class ChangeChecklistTestData
        {
            private static readonly string checklistJson = "{\r\n    \"CompanyId\": 136,\r\n    \"TemplateId\": 5714,\r\n    \"IsCompleted\": false,\r\n    \"Signatures\": [\r\n        {\r\n            \"SignatureImage\": \"\",\r\n            \"SignedAt\": \"2024-12-10T14:18:30.811Z\",\r\n            \"SignedById\": 5563,\r\n            \"SignedBy\": \"Rik van de Laar\"\r\n        }\r\n    ],\r\n    \"Tasks\": [\r\n        {\r\n            \"Status\": \"ok\",\r\n            \"CompanyId\": 136,\r\n            \"TemplateId\": 67843,\r\n            \"PropertyUserValues\": [],\r\n            \"PictureProof\": null,\r\n            \"Id\": 16381122,\r\n            \"Signature\": {\r\n                \"SignatureImage\": null,\r\n                \"SignedAt\": \"2024-12-10T14:18:54.086Z\",\r\n                \"SignedById\": 5563,\r\n                \"SignedBy\": \"Rik van de Laar\"\r\n            }\r\n        }\r\n    ],\r\n    \"OpenFieldsPropertyUserValues\": [],\r\n    \"Id\": 119296,\r\n    \"Stages\": []\r\n}";
            static Checklist checklist = checklistJson.ToObjectFromJson<Checklist>();
            public static IEnumerable TestChecklists
            {
                get
                {
                    checklist.Id = 119298;
                    yield return new TestCaseData(119298, checklist, HttpStatusCode.BadRequest, "Completed checklist can not be changed.\n".ToJsonFromObject());

                    checklist = checklistJson.ToObjectFromJson<Checklist>();
                    checklist.Id = 119295;
                    yield return new TestCaseData(119296, checklist, HttpStatusCode.BadRequest, "Id cannot be changed.\n".ToJsonFromObject());

                    checklist = checklistJson.ToObjectFromJson<Checklist>();
                    checklist.CompanyId = 1251; //company id of 'EZ Factory Kantoor' on prod
                    yield return new TestCaseData(119296, checklist, HttpStatusCode.BadRequest, "Company connection is not valid.\n".ToJsonFromObject()); //message from ValidateAndClean triggers before ValidateMutation, the ValidateMutation wouldbe "Company id cannot be changed."

                    checklist = checklistJson.ToObjectFromJson<Checklist>();
                    checklist.TemplateId = 5713;
                    yield return new TestCaseData(119296, checklist, HttpStatusCode.BadRequest, "Template id cannot be changed.\n".ToJsonFromObject());

                    checklist = checklistJson.ToObjectFromJson<Checklist>();
                    checklist.CreatedById = 5562;
                    yield return new TestCaseData(119296, checklist, HttpStatusCode.BadRequest, "Created by id cannot be changed.\n".ToJsonFromObject());
                }
            }
        }

        #endregion
    }
}
