using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TP.ConcurrentProgramming.Presentation.Model.Test
{
    [TestClass]
    public class PresentationModelUnitTest
    {
        [TestMethod]
        public void CreateAndDisposeTestMethod()
        {
            using (ModelAbstractApi newInstance = ModelAbstractApi.CreateModel())
            {
                Assert.IsNotNull(newInstance);
            }
        }

        [TestMethod]
        public void StartTestMethod()
        {
            using (ModelAbstractApi newInstance = ModelAbstractApi.CreateModel())
            {
                newInstance.Start(5);
                Assert.IsNotNull(newInstance);
            }
        }
    }
}