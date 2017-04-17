using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PT.Fibonacci.Tests
{
    [TestClass]
    public class CalculatorTest
    {
        private readonly Calculator _calculator = new Calculator();

        [TestMethod]
        public void ZeroTest()
        {
            Assert.AreEqual(1, _calculator.Calculate(0, 0));
        }

        [TestMethod]
        public void OneTest()
        {
            Assert.AreEqual(1, _calculator.Calculate(0, 1));
        }

        [TestMethod]
        public void SumTest()
        {
            Assert.AreEqual(7, _calculator.Calculate(3, 4));
        }

        [TestMethod]
        public void OverflowTest()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => _calculator.Calculate(Int32.MaxValue, Int32.MaxValue));
        }

        [TestMethod]
        public void NegativeTest()
        {
            Assert.ThrowsException<ArgumentException>(
                () => _calculator.Calculate(0, -1));
            Assert.ThrowsException<ArgumentException>(
                () => _calculator.Calculate(-1, 0));
        }
    }
}
