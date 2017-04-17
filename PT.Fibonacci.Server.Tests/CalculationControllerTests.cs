using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PT.Fibonacci.Server.Controllers;
using PT.Fibonacci.Server.Models;

namespace PT.Fibonacci.Server.Tests
{
    [TestClass]
    public class CalculationControllerTests
    {
        [TestMethod]
        public async Task CreateNewTest()
        {
            var repoMock = new Mock<ICalculationRepository>();
            repoMock.Setup(repository => repository.CreateNew())
                .Returns(Task.FromResult(Mock.Of<ICalculation>(c => c.Id == 1 && c.Current == 0)));

            var controller = new CalculationController(repoMock.Object,
                Mock.Of<ICalculator>(), Mock.Of<IResponseSender>());

            var result = await controller.CreateNewCalculation();
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<int>));
            var ok = result as OkNegotiatedContentResult<int>;
            Assert.AreEqual(1, ok.Content);
            repoMock.Verify(r => r.CreateNew(), Times.Once);
        }

        [TestMethod]
        public async Task CreateNewExceptionTest()
        {
            var repoMock = new Mock<ICalculationRepository>();
            repoMock.Setup(repository => repository.CreateNew()).Throws<InvalidOperationException>();

            var controller = new CalculationController(repoMock.Object,
                Mock.Of<ICalculator>(), Mock.Of<IResponseSender>());

            var result = await controller.CreateNewCalculation();
            Assert.IsInstanceOfType(result, typeof(ExceptionResult));
            repoMock.Verify(r => r.CreateNew(), Times.Once);
        }

        [TestMethod]
        public async Task PostNextTest()
        {
            var repoMock = new Mock<ICalculationRepository>();
            repoMock.Setup(repo => repo.Get(1))
                .Returns(() => Task.FromResult(Mock.Of<ICalculation>(c => c.Id == 1 && c.Current == 1)));
            repoMock.Setup(r => r.Update(It.IsAny<ICalculation>())).Returns(Task.CompletedTask);
            
            var calculatorMock = new Mock<ICalculator>();
            calculatorMock.Setup(c => c.Calculate(1, 1)).Returns(2);

            var responseMock = new Mock<IResponseSender>();
            responseMock.Setup(r => r.SendResponseAsync(It.IsAny<ICalculation>())).Returns(Task.CompletedTask);

            var controller = new CalculationController(repoMock.Object,
                calculatorMock.Object, responseMock.Object);

            var result = await controller.CalculateNext(1, 1);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            repoMock.Verify(r => r.Get(1), Times.Once);
            repoMock.Verify(r => r.Update(It.Is<ICalculation>(c => c.Id == 1 && c.Current == 2)), Times.Once);
            calculatorMock.Verify(c => c.Calculate(1, 1), Times.Once);
            responseMock.Verify(r => r.SendResponseAsync(It.Is<ICalculation>(c => c.Id == 1 && c.Current == 2)));
        }

        [TestMethod]
        public async Task PostOverflowTest()
        {
            var repoMock = new Mock<ICalculationRepository>();
            repoMock.Setup(repo => repo.Get(1))
                .Returns(() => Task.FromResult(Mock.Of<ICalculation>(c => c.Id == 1 && c.Current == 1)));

            var calculatorMock = new Mock<ICalculator>();
            calculatorMock.Setup(c => c.Calculate(It.IsAny<int>(), It.IsAny<int>())).Throws<InvalidOperationException>();

            var responseMock = new Mock<IResponseSender>();
            responseMock.Setup(r => r.SendResponseAsync(It.IsAny<ICalculation>()));

            var controller = new CalculationController(repoMock.Object,
                calculatorMock.Object, responseMock.Object);

            var result = await controller.CalculateNext(1, 1);
            Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
            var statusCodeResult = result as StatusCodeResult;
            Assert.AreEqual(HttpStatusCode.ResetContent, statusCodeResult.StatusCode);
            repoMock.Verify(r => r.Get(1), Times.Once);
            repoMock.Verify(r => r.Update(It.Is<ICalculation>(c => c.Id == 1 && c.Current == 2)), Times.Never);
            calculatorMock.Verify(c => c.Calculate(1, 1), Times.Once);
            responseMock.Verify(r => r.SendResponseAsync(It.IsAny<ICalculation>()), Times.Never);
        }

        [TestMethod]
        public async Task PostNotFoundTest()
        {
            var repoMock = new Mock<ICalculationRepository>();
            repoMock.Setup(repo => repo.Get(1))
                .Returns(() => Task.FromResult<ICalculation>(null));

            var calculatorMock = new Mock<ICalculator>();
            calculatorMock.Setup(c => c.Calculate(It.IsAny<int>(), It.IsAny<int>()));

            var responseMock = new Mock<IResponseSender>();
            responseMock.Setup(r => r.SendResponseAsync(It.IsAny<ICalculation>()));

            var controller = new CalculationController(repoMock.Object,
                calculatorMock.Object, responseMock.Object);

            var result = await controller.CalculateNext(1, 1);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            repoMock.Verify(r => r.Get(1), Times.Once);
            repoMock.Verify(r => r.Update(It.Is<ICalculation>(c => c.Id == 1 && c.Current == 2)), Times.Never);
            calculatorMock.Verify(c => c.Calculate(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            responseMock.Verify(r => r.SendResponseAsync(It.IsAny<ICalculation>()), Times.Never);
        }

        [TestMethod]
        public async Task CompleteTest()
        {
            var repoMock = new Mock<ICalculationRepository>();
            repoMock.Setup(repository => repository.Remove(1)).Returns(Task.FromResult(true));

            var controller = new CalculationController(repoMock.Object,
                Mock.Of<ICalculator>(), Mock.Of<IResponseSender>());

            var result = await controller.CompleteCalculation(1);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            repoMock.Verify(r => r.Remove(1), Times.Once);
        }

        [TestMethod]
        public async Task CompleteNotFoundTest()
        {
            var repoMock = new Mock<ICalculationRepository>();
            repoMock.Setup(repository => repository.Remove(1)).Returns(Task.FromResult(false));

            var controller = new CalculationController(repoMock.Object,
                Mock.Of<ICalculator>(), Mock.Of<IResponseSender>());

            var result = await controller.CompleteCalculation(1);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            repoMock.Verify(r => r.Remove(1), Times.Once);
        }

        [TestMethod]
        public async Task CompleteExceptionTest()
        {
            var repoMock = new Mock<ICalculationRepository>();
            repoMock.Setup(repository => repository.Remove(1)).Throws<Exception>();

            var controller = new CalculationController(repoMock.Object,
                Mock.Of<ICalculator>(), Mock.Of<IResponseSender>());

            var result = await controller.CompleteCalculation(1);
            Assert.IsInstanceOfType(result, typeof(ExceptionResult));
            repoMock.Verify(r => r.Remove(1), Times.Once);
        }
    }
}
