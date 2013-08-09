using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneNightWerewolf.Models;

namespace OneNightWerewolf.Tests.Models
{
    [TestClass]
    public class CardTest
    {
        [TestMethod]
        public void VillagerCardTest()
        {
            var actual = new VillagerCard();

            Assert.AreEqual(1, actual.CardId);
            Assert.AreEqual("村人", actual.CardName);
        }
    }
}
