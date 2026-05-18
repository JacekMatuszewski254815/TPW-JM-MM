using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicAbstractAPIUnitTest
    {
        [TestMethod]
        public void BusinessLogicConstructorTestMethod()
        {
            //inicjalizacja nowej instancji API logiki
            BusinessLogicAbstractAPI instance = BusinessLogicAbstractAPI.CreateAPI();
            Assert.IsNotNull(instance);

            instance.Dispose();
        }
    }
}