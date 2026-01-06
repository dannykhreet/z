using ApiTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTests
{
    /// <summary>
    /// ApiIntegration tests. 
    /// The integrations tests are based on the backbone of the NUnit framework. 
    /// We do not fully implement specific Unit tests but we use that framework as a runner to run the tests.
    /// 
    /// The tests are run in a certain order, first the setup/init (this class) is run, this fills certain values.
    /// Afterwards a the other tests are run in ORDER based on the Order attribute. 
    /// 
    /// The tests run against a 'live' test API. This means certain ids (as environmental variables) or constant need to be based on the version that runs on test. 
    /// The runner can run locally but make sure you than change the URI that is runs against is local. 
    /// The tests will always contain a GET or POST depending on test. No specific code tests are done only 'integration tests'; 
    /// 
    /// We can have complex or simple tests. 
    /// The simple tests will be just 'I receive certain data' or 'I post certain data'. 
    /// But also we can chain several tests in one test case, for instance to retrieve the first item of a collection, change the title, retrieve th object again and test if the title has changed. 
    /// These are more or less based on a scenario, maybe an existing bug we want to test permanently or related.
    /// 
    /// Within a test fixture, multiple tests can exist. Every test can have multiple test cases. These are for the most part also used within the test and dynamically update as for use with a URI part. 
    /// We could add an method to dynamically buildup based on the test case but its more easier to use or misuse the test case as a large dynamic variable. 
    /// The test case is then mostly used with Get / Posts on the API containing parameters in the query string. 
    /// 
    /// Every controller where test need to be run, should have their own test fixture. Within that fixture all tests for that specific controller can be done. 
    /// In time this should include ALL GETS, ALL POSTS within a controller, with as many test cases as possible to hit all specific functionalities, include the include parameters, posting multiple different objects containing
    /// different sets of data to fully cover all logic and or DB table updates/inserts. 
    /// In time also complex scenario test should be added, that do multiple things in sequence for instance:
    ///     - Adding -> Retrieving -> Mutating -> Adding again -> filtering overview call so newly added items are available when sorted on modification date. 
    ///     
    /// When sending data a json object can be added to the project. 
    /// Within that project the JSON object will needs to be made partly dynamic. To create a JSON object, the easiest way is to capture a request from the webclient or the debugger locally. Get the JSON object from that request and add it to the project. 
    /// Replace certain values within the JSON with tags. (certain dates, certain date values etc). These will be replaces afterwards. Tags that are supported can be found in the GetResource() method within the run helpers. If you need an
    /// specific tag that not exist, add them there. Make sure the JSON is added as a embedded resource. Replace all static items with 'known' items (e.g. images etc) or items specifically for this test, replace main user ids with either the automated tester user id. 
    /// Secondary user ids can be existing users of the test database or 1 of the other automated test users. 
    /// 
    /// To add more dynamic uris (mostly for gets) TestCases can be used. Every test case is run as a separate test. That way multiple includes, filters etc can be tested. 
    /// 
    /// Within each test, make sure the test is as basic as possible and make sure tests are setup the same way. The test runner itself CAN NOT connect to the DB or any specific logic from the API, everything must run through requests
    /// to make sure to simulate the use of the clients. If more complex tests are needed (a flow) for simulating certain logic (e.g. posting something, then retrieving, then changing, than retrieving, than changing again) make sure that 
    /// the functionalities you use are ALSO available as a separate test.
    /// 
    /// Examples can be found in the Action (gets, new post, existing post), Audit (gets and post new), Checklists (gets and post new)
    /// 
    /// When using existing or purpose build templates, make add some reference in the title or description that they should not be changed and/or deleted.
    ///
    /// NOTE! make sure you wont forget the ORDER attributes when adding fixtures. 
    /// NOTE! this is 'live' running on the TEST API, and called after each deployment of that API to TEST. Make sure when making changes the test work properly and don't overload the system. 
    /// NOTE! tests currently run under the main automated test user with role manager.
    /// NOTE! if there need to be any structural changes or dynamic new values where variables are needed, make sure you discuss these seeing the test pipelines probably would need to change. 
    /// NOTE! when adding tests, make sure you follow the same structure (naming, build up etc)
    /// NOTE! when done adding, test locally (either against local api or the test api). 
    /// NOTE! all adding of tests must be done on the development branch (this branch is run on devops when running the tests, test are recompiled every run) or on a separate branch with needs to merge to the dev branch when done. 
    /// NOTE! when possible use the test cases to extend the retrieval/posting. For gets this mostly will be mapped directly to the query string (seeing those mostly determine what content is retrieved) for posting data test cases can be used for mapping to the files containing the data. 
    ///         Make sure the cases are different enough (e.g. use other includes, parameters, files with different kind of configured items to hit as much of the logic as possible.
    /// 
    /// Automated test users:
    /// 
    /// 5747	automated.test.48DgroPZ1Lpg3TN9JBmx.user.basic
    /// 5779	automated.test.48DgroPZ1Lpg3TN9JBmx.user.shiftleader
    /// 5780	automated.test.48DgroPZ1Lpg3TN9JBmx.user.manager
    /// </summary>
    [Order(0)]
    [TestFixture]
    public class InitTests : Base.BaseTests
    {
        [SetUp]
        public void Setup()
        {

        }

        #region - authentication -
        [Order(0)]
        [Test]
        public async Task TestInit()
        {
            RunHelpers.SetEnvironmentalVariables();

            await Task.CompletedTask;

            _companyid = Convert.ToInt32(Environment.GetEnvironmentVariable("AUTOMATED_MAIN_TEST_USER_COMPANY_ID"));
            _userid = Convert.ToInt32(Environment.GetEnvironmentVariable("AUTOMATED_MAIN_TEST_USER_ID"));
            Assert.Multiple(() =>
            {
                Assert.That(_companyid, Is.GreaterThan(0));
                Assert.That(_userid, Is.GreaterThan(0));
            });
        }

        [Order(1)]
        [Test]
        public async Task TestInithAuthentication()
        {
            _internalToken = await RetrieveToken();
            Assert.That(_internalToken, Is.Not.EqualTo(string.Empty));
        }

        [Test]
        public async Task TestApiHealthAuthenticationAfterRetrieval()
        {
            await Task.CompletedTask;
            Assert.That(_internalToken, Is.Not.EqualTo(string.Empty));
        }
        #endregion

    }
}
