using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataAbstractAPIUnitTest
    {
        [TestMethod]
        public void ConstructorTestTestMethod()
        {
            DataAbstractAPI instance = DataAbstractAPI.CreateAPI();
            Assert.IsNotNull(instance);

            instance.Dispose();
        }
    }
}