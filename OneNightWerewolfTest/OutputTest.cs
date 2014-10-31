using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneNightWerewolf.Controllers;

namespace OneNightWerewolfTest
{
    [TestClass]
    public class OutputTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var target = new GameController();
            var result = target.Night(1);
            Assert.IsNotNull(result);
        }
    }
}
