using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private const string Expected = "Hello World!";
        [TestMethod]
        public void TestMethod1()
        {
            // Method intentionally left empty.
            var result = "Hello World!";
            Assert.AreEqual(Expected, result);
        }
    }
}
