using System.Threading.Tasks;
using BetaseriesStandardLib;
using CommonStandardLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BezyFreebTest
{
    [TestClass]
    public class GuessItFixture
    {
        [TestMethod]
        public async Task TestMethodGuessItCall()
        {
            var guess = new GuessIt
            {
                ApiConnector = new ApiConnector()
            };
            Assert.AreEqual("Disconnect", await guess.GuessNom("Disconnect.2012.TRUEFRENCH.BRRip.Xvid-BLUB.avi"));
        }
    }
}
