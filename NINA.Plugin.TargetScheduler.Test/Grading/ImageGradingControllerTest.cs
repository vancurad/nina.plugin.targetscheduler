using Moq;
using NINA.Plugin.TargetScheduler.Grading;
using NUnit.Framework;
using System.Threading;

namespace NINA.Plugin.TargetScheduler.Test.Grading {

    [TestFixture]
    public class ImageGradingControllerTest {

        [Test]
        public void testImageGradingController() {
            Mock<IImageGrader> mock = new Mock<IImageGrader>();
            mock.SetupAllProperties();
            CancellationToken ct = new CancellationTokenSource().Token;

            ImageGradingController sut = new ImageGradingController(mock.Object);
            sut.Enqueue(new GradingWorkData(), ct).Wait();
            sut.Enqueue(new GradingWorkData(), ct).Wait();
            sut.Enqueue(new GradingWorkData(), ct).Wait();

            Thread.Sleep(50); // give it time...
            sut.Shutdown();

            mock.Verify(x => x.Grade(It.IsAny<GradingWorkData>()), Times.Exactly(3));
        }

        [Test]
        public void testSingletonUnused() {
            ImageGradingController.Instance.Shutdown();
        }
    }
}