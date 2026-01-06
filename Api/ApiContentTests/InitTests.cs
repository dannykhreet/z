using ApiContentTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiContentTests
{
    /// <summary>
    /// ApiContentIntegration tests. 
    /// Main docs, see UnitTests.
    /// Only add content checks (for resources, languages etc) to this project. 
    /// This will run on a seperate test schedule.
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
