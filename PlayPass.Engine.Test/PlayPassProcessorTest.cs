using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using PlaySharp;

namespace PlayPass.Engine.Test
{
    [TestFixture]
    public class PlayPassProcessorTest
    {
        private Mock<ILogManager> _logManager;
        private Mock<IQueueValidator> _queueValidator;
        private Mock<IPlayOn> _playOn;
        private PlayPassProcessor _processor;

        [SetUp]
        public void Setup()
        {
            _logManager = new Mock<ILogManager>();
            _queueValidator = new Mock<IQueueValidator>();
            string message;
            _queueValidator.Setup(v => v.CanQueueMedia(It.IsAny<PlayOnVideo>(), out message)).Returns(true);
            _playOn = Mocks.GetPlayOn();

            _processor = new PlayPassProcessor(_playOn.Object, _logManager.Object, _queueValidator.Object)
            {
                QueueMode = true
            };
        }

        [Test]
        public void BasicScanQueueWorkflow()
        {
            var passes = new PassItems();
            var pass = new PassItem("Test", true);
            passes.Add(pass);
            var scanChannelAction = new PassAction {Type = PassActionType.Scan, Name = "Random TV Network"};
            pass.Actions.Add(scanChannelAction);
            var scanWatchList = new PassAction { Type = PassActionType.Scan, Name = "My Things To Watch" };
            scanChannelAction.Actions.Add(scanWatchList);
            var queueVideos = new PassAction { Type = PassActionType.Queue, Name = "*"};
            scanWatchList.Actions.Add(queueVideos);

            _processor.ProcessPasses(passes);

            _playOn.Verify(p => p.GetCatalog(), Times.Exactly(1));
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv", It.IsAny<IList<PlayOnItem>>()));
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv-queue", It.IsAny<IList<PlayOnItem>>()));
            _playOn.Verify(p => p.QueueMedia(It.IsAny<PlayOnVideo>()), Times.Exactly(2));
            _playOn.VerifyNoOtherCalls();
        }

    }
}
