using ApiTests.Helpers;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Diagnostics;
using System.Net;

namespace ApiTests
{
    /// <summary>
    /// Intergration Health tests, will run all routes on configured call. 
    /// </summary>
    [Order(1)]
    [TestFixture]
    public class ApiIntegrationHealthTests: Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
          
        }

        #region - general -
        [Test]
        public async Task TestApiHealth()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/health"));
                HttpStatusCode status = resp.StatusCode;
                Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            }
        }

        [Test]
        public async Task TestApiHealthContent()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/health"));
                HttpStatusCode status = resp.StatusCode;
                string message = string.Empty;
                if (status == HttpStatusCode.OK)
                {
                    message = await resp.Content.ReadAsStringAsync();
                }
                Assert.That(message, Is.EqualTo("true"));
            }
        }


        [Test]
        public async Task TestApiHealthDb()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/health/db"));
                HttpStatusCode status = resp.StatusCode;
                Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            }
        }


        [Test]
        public async Task TestApiHealthContentDb()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/health/db"));
                HttpStatusCode status = resp.StatusCode;
                string message = string.Empty;
                if (status == HttpStatusCode.OK)
                {
                    message = await resp.Content.ReadAsStringAsync();
                }
                Assert.That(message, Is.EqualTo("true"));
            }
        }

        [Test]
        public async Task TestApiHealthCheckRead()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/health/checkread"));
                HttpStatusCode status = resp.StatusCode;
                string message = string.Empty;
                if (status == HttpStatusCode.OK)
                {
                    message = await resp.Content.ReadAsStringAsync();
                }
                Assert.That(message, Is.EqualTo("true"));
            }
        }

        [Test]
        public async Task TestApiHealthCheckWrite()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/health/checkwrite"));
                HttpStatusCode status = resp.StatusCode;
                string message = string.Empty;
                if (status == HttpStatusCode.OK)
                {
                    message = await resp.Content.ReadAsStringAsync();
                }
                Assert.That(message, Is.EqualTo("true"));
            }
        }
        #endregion

        #region - actions - 

        [Test]
        public async Task TestApiHealthActions()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/actions/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            }
        }

        [Test]
        public async Task TestApiHealthActionsContent()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/actions/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                string message = string.Empty;
                if (status == HttpStatusCode.OK)
                {
                    message = await resp.Content.ReadAsStringAsync();
                }
                Assert.That(message, Is.EqualTo("true"));
            }
        }

        [Test]
        public async Task TestApiHealthActionComments()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/actioncomments/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            }
        }

        [Test]
        public async Task TestApiHealthActionCommentsContent()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/actioncomments/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                string message = string.Empty;
                if (status == HttpStatusCode.OK)
                {
                    message = await resp.Content.ReadAsStringAsync();
                }
                Assert.That(message, Is.EqualTo("true"));
            }
        }
        #endregion

        #region - audits - 

        [Test]
        public async Task TestApiHealthAudits()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/audits/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            }
        }

        [Test]
        public async Task TestApiHealthAuditsContent()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/audits/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                string message = string.Empty;
                if (status == HttpStatusCode.OK)
                {
                    message = await resp.Content.ReadAsStringAsync();
                }
                Assert.That(message, Is.EqualTo("true"));
            }
        }
        #endregion

        #region - assessments - 

        [Test]
        public async Task TestApiHealthAssessments()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/assessments/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            }
        }

        [Test]
        public async Task TestApiHealthAssessmentsContent()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/assessments/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                string message = string.Empty;
                if (status == HttpStatusCode.OK)
                {
                    message = await resp.Content.ReadAsStringAsync();
                }
                Assert.That(message, Is.EqualTo("true"));
            }
        }
        #endregion

        #region - checklists - 

        [Test]
        public async Task TestApiHealthChecklists()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/checklists/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            }
        }

        [Test]
        public async Task TestApiHealthChecklistsContent()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/checklists/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                string message = string.Empty;
                if (status == HttpStatusCode.OK)
                {
                    message = await resp.Content.ReadAsStringAsync();
                }
                Assert.That(message, Is.EqualTo("true"));
            }
        }
        #endregion

        #region - tasks - 
        [Order(0)]
        [Test]
        public async Task TestApiHealthTasks()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/tasks/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            }
        }

        [Order(1)]
        [Test]
        public async Task TestApiHealthTasksContent()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/tasks/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                string message = string.Empty;
                if (status == HttpStatusCode.OK)
                {
                    message = await resp.Content.ReadAsStringAsync();
                }
                Assert.That(message, Is.EqualTo("true"));
            }
        }
        #endregion

        #region - users - 

        [Test]
        public async Task TestApiHealthUsers()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/users/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            }
        }

        [Test]
        public async Task TestApiHealthUsersContent()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(string.Concat(_baseUri, "/v1/users/healthcheck"));
                HttpStatusCode status = resp.StatusCode;
                string message = string.Empty;
                if (status == HttpStatusCode.OK)
                {
                    message = await resp.Content.ReadAsStringAsync();
                }
                Assert.That(message, Is.EqualTo("true"));
            }
        }
        #endregion
    }
}