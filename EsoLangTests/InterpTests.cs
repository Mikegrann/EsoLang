using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace EsoLang.Tests
{
    [TestClass()]
    public class InterpTests
    {
        [TestMethod()]
        public void arrTest()
        {
            List<long> indices = new List<long>();
            indices.Add(3);
            indices.Add(5);

            Val arr = new ArrV(indices, new NumV(2));

            Assert.AreEqual("{{2, 2, 2, 2, 2}, {2, 2, 2, 2, 2}, {2, 2, 2, 2, 2}}", arr.ToString());
        }
    }
}
