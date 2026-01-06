using ApiTests.Helpers;
using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiTests
{
    [Order(19)]
    [TestFixture]
    public class ApiIntegrationGeneralTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region - Retrieve Collections -
        [Test]
        [TestCase("?language=de_de&resourcetype=1")]
        [TestCase("?language=en_gb&resourcetype=1")]
        [TestCase("?language=en_us&resourcetype=1")]
        [TestCase("?language=es_es&resourcetype=1")]
        [TestCase("?language=fr_fr&resourcetype=1")]
        [TestCase("?language=nl_nl&resourcetype=1")]
        [TestCase("?language=pt_pt&resourcetype=1")]
        [TestCase("?language=th_th&resourcetype=1")]
        [TestCase("?language=ko_kr&resourcetype=1")]
        [TestCase("?language=it_it&resourcetype=1")]
        [TestCase("?language=nb_no&resourcetype=1")]
        [TestCase("?language=fi_fi&resourcetype=1")]
        [TestCase("?language=pl_pl&resourcetype=1")]
        [TestCase("?language=ar_eg&resourcetype=1")]
        [TestCase("?language=el_gr&resourcetype=1")]
        [TestCase("?language=id_id&resourcetype=1")]
        [TestCase("?language=ms_my&resourcetype=1")]
        public async Task TestRetrieveLanguageResourceApp(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/app/resources/language/", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            LanguageResource language = (await resp.Content.ReadAsStringAsync()).ToObjectFromJson<LanguageResource>();

            var nrOfitems = language.ResourceItems.Where(x => !string.IsNullOrEmpty(x.ResourceValue)).ToList().Count;
            Assert.That(nrOfitems, Is.GreaterThan(0));
        }

        [Test]
        [TestCase("?language=de_de&resourcetype=2")]
        [TestCase("?language=en_gb&resourcetype=2")]
        [TestCase("?language=en_us&resourcetype=2")]
        [TestCase("?language=es_es&resourcetype=2")]
        [TestCase("?language=fr_fr&resourcetype=2")]
        [TestCase("?language=nl_nl&resourcetype=2")]
        [TestCase("?language=pt_pt&resourcetype=2")]
        [TestCase("?language=th_th&resourcetype=2")]
        [TestCase("?language=ko_kr&resourcetype=2")]
        [TestCase("?language=it_it&resourcetype=2")]
        [TestCase("?language=nb_no&resourcetype=2")]
        [TestCase("?language=fi_fi&resourcetype=2")]
        [TestCase("?language=pl_pl&resourcetype=2")]
        [TestCase("?language=ar_eg&resourcetype=2")]
        [TestCase("?language=el_gr&resourcetype=2")]
        [TestCase("?language=id_id&resourcetype=2")]
        [TestCase("?language=ms_my&resourcetype=2")]
        public async Task TestRetrieveLanguageResourceCms(string query)
        {
            var resp = await GetResponse(string.Concat("/v1/app/resources/language/", query));
            HttpStatusCode status = resp.StatusCode;
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            LanguageResource language = (await resp.Content.ReadAsStringAsync()).ToObjectFromJson<LanguageResource>();

            var nrOfitems = language.ResourceItems.Where(x => !string.IsNullOrEmpty(x.ResourceValue)).ToList().Count;
            Assert.That(nrOfitems, Is.GreaterThan(0));
        }


        // NOTE count tests moved to SEPERATE CONTENT TEST PROJECT.

        //[Test]
        //[TestCase("?language=de_de&resourcetype=1")]
        //[TestCase("?language=en_gb&resourcetype=1")]
        //[TestCase("?language=en_us&resourcetype=1")]
        //[TestCase("?language=es_es&resourcetype=1")]
        //[TestCase("?language=fr_fr&resourcetype=1")]
        //[TestCase("?language=nl_nl&resourcetype=1")]
        //[TestCase("?language=pt_pt&resourcetype=1")]
        //[TestCase("?language=th_th&resourcetype=1")]
        //[TestCase("?language=ko_kr&resourcetype=1")]
        //[TestCase("?language=it_it&resourcetype=1")]
        //[TestCase("?language=nb_no&resourcetype=1")]
        //[TestCase("?language=fi_fi&resourcetype=1")]
        //[TestCase("?language=pl_pl&resourcetype=1")]
        //[TestCase("?language=ar_eg&resourcetype=1")]
        //[TestCase("?language=el_gr&resourcetype=1")]
        //[TestCase("?language=id_id&resourcetype=1")]
        //[TestCase("?language=ms_my&resourcetype=1")]
        //public async Task TestRetrieveLanguageResourceCountsApp(string query)
        //{
        //    var resp = await GetResponse(string.Concat("/v1/app/resources/language/", query));
        //    HttpStatusCode status = resp.StatusCode;
        //    Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        //    LanguageResource language = (await resp.Content.ReadAsStringAsync()).ToObjectFromJson<LanguageResource>();

        //    var respDefault = await GetResponse("/v1/app/resources/language/?language=en_us&resourcetype=1");
        //    HttpStatusCode statusDefault = respDefault.StatusCode;
        //    Assert.That(statusDefault, Is.EqualTo(HttpStatusCode.OK));
        //    LanguageResource languageDefault = (await respDefault.Content.ReadAsStringAsync()).ToObjectFromJson<LanguageResource>();

        //    var nrOfitems = language.ResourceItems.Where(x => !string.IsNullOrEmpty(x.ResourceValue)).ToList().Count;
        //    var nrOfDefaultItems = languageDefault.ResourceItems.Where(x => !string.IsNullOrEmpty(x.ResourceValue)).ToList().Count;

        //    Assert.That(nrOfitems, Is.EqualTo(nrOfDefaultItems), message: string.Format("{0} number of items, should be {1}", nrOfitems, nrOfDefaultItems));

        //}

        //[Test]
        //[TestCase("?language=de_de&resourcetype=2")]
        //[TestCase("?language=en_gb&resourcetype=2")]
        //[TestCase("?language=en_us&resourcetype=2")]
        //[TestCase("?language=es_es&resourcetype=2")]
        //[TestCase("?language=fr_fr&resourcetype=2")]
        //[TestCase("?language=nl_nl&resourcetype=2")]
        //[TestCase("?language=pt_pt&resourcetype=2")]
        //[TestCase("?language=th_th&resourcetype=2")]
        //[TestCase("?language=ko_kr&resourcetype=2")]
        //[TestCase("?language=it_it&resourcetype=2")]
        //[TestCase("?language=nb_no&resourcetype=2")]
        //[TestCase("?language=fi_fi&resourcetype=2")]
        //[TestCase("?language=pl_pl&resourcetype=2")]
        //[TestCase("?language=ar_eg&resourcetype=2")]
        //[TestCase("?language=el_gr&resourcetype=2")]
        //[TestCase("?language=id_id&resourcetype=2")]
        //[TestCase("?language=ms_my&resourcetype=2")]
        //public async Task TestRetrieveLanguageResourceCountsCms(string query)
        //{
        //    var resp = await GetResponse(string.Concat("/v1/app/resources/language/", query));
        //    HttpStatusCode status = resp.StatusCode;
        //    Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        //    LanguageResource language = (await resp.Content.ReadAsStringAsync()).ToObjectFromJson<LanguageResource>();

        //    var respDefault = await GetResponse("/v1/app/resources/language/?language=en_us&resourcetype=2");
        //    HttpStatusCode statusDefault = respDefault.StatusCode;
        //    Assert.That(statusDefault, Is.EqualTo(HttpStatusCode.OK));
        //    LanguageResource languageDefault = (await respDefault.Content.ReadAsStringAsync()).ToObjectFromJson<LanguageResource>();

        //    var nrOfitems = language.ResourceItems.Where(x => !string.IsNullOrEmpty(x.ResourceValue)).ToList().Count;
        //    var nrOfDefaultItems = languageDefault.ResourceItems.Where(x => !string.IsNullOrEmpty(x.ResourceValue)).ToList().Count;

        //    Assert.That(nrOfitems, Is.EqualTo(nrOfDefaultItems), message: string.Format("{0} number of items, should be {1}", nrOfitems, nrOfDefaultItems));
        //}

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
