using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BezyFB.BetaSerieLib;
using BezyFB.Helpers;

namespace BezyFreebTest
{
    [TestClass]
    public class GuessItFixture
    {
        [TestMethod]
        public void TestMethodGuessItCall()
        {
            var guess = new GuessIt
            {
                ApiConnector = new ApiConnector()
            };
            Assert.AreEqual("Disconnect", guess.GuessNom("Disconnect.2012.TRUEFRENCH.BRRip.Xvid-BLUB.avi"));
        }
    }
}
