﻿using System;
using BezyFB.BetaSerie;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BezyFreebTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethodGuessItCall()
        {
            Assert.AreEqual("Disconnect", GuessIt.GuessNom("Disconnect.2012.TRUEFRENCH.BRRip.Xvid-BLUB.avi"));
        }
    }
}