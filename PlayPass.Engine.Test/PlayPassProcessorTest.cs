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
            _queueValidator = Mocks.GetQueueValidator();
            _playOn = Mocks.GetPlayOn();

            _processor = new PlayPassProcessor(_playOn.Object, _logManager.Object, _queueValidator.Object);
        }

        [Test]
        public void BasicScanQueueWorkflow()
        {
            _processor.QueueMode = true;
            _processor.ProcessPasses(GetBasicWorkflowPass());

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv", It.IsAny<IList<PlayOnItem>>()));
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv-queue", It.IsAny<IList<PlayOnItem>>()));
            _playOn.Verify(p => p.QueueMedia(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-vid1")));
            _playOn.Verify(p => p.QueueMedia(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-vid2")));
            _playOn.VerifyNoOtherCalls();
        }

        [Test]
        public void PreviewMode()
        {
            _processor.ProcessPasses(GetBasicWorkflowPass());

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetItems(It.IsAny<string>(), It.IsAny<IList<PlayOnItem>>()), Times.Exactly(2));
            _playOn.VerifyNoOtherCalls();
            _queueValidator.Verify(q => q.AddMediaToCounts(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-vid1")));
            _queueValidator.Verify(q => q.AddMediaToCounts(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-vid2")));
        }

        [Test]
        public void QueueMixedFolder()
        {
            var scanChannelAction = new PassScanAction { Name = "Random TV Network" };
            var queueAction = new PassQueueAction { Name = "*" };
            scanChannelAction.Actions.Add(queueAction);

            _processor.QueueMode = true;
            _processor.ProcessPasses(GetPasses(scanChannelAction));

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv", It.IsAny<IList<PlayOnItem>>()));
            _playOn.Verify(p => p.QueueMedia(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-clip1")));
            _playOn.VerifyNoOtherCalls();
        }

        [Test]
        public void SearchQueueMatches()
        {
            var scanChannelAction = new PassScanAction { Name = "Static TV Network" };
            var searchChannelAction = new PassSearchAction { Name = "Video" };
            scanChannelAction.Actions.Add(searchChannelAction);
            var queueVideos = new PassQueueAction { Name = "*" };
            searchChannelAction.Actions.Add(queueVideos);

            _processor.QueueMode = true;
            _processor.ProcessPasses(GetPasses(scanChannelAction));

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetSearchResults("/data/data.xml?id=stv", "Video"));
            _playOn.Verify(p => p.QueueMedia(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=stv-vid1")));
            _playOn.Verify(p => p.QueueMedia(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=stv-vid2")));
            _playOn.VerifyNoOtherCalls();
        }

        [Test]
        public void SearchNoMatch()
        {
            var scanChannelAction = new PassScanAction { Name = "Static TV Network" };
            var searchChannelAction = new PassSearchAction { Name = "Nothing" };
            scanChannelAction.Actions.Add(searchChannelAction);
            var queueVideos = new PassQueueAction { Name = "*" };
            searchChannelAction.Actions.Add(queueVideos);

            _processor.QueueMode = true;
            _processor.ProcessPasses(GetPasses(scanChannelAction));

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetSearchResults("/data/data.xml?id=stv", "Nothing"));
            _playOn.VerifyNoOtherCalls();
        }

        [Test]
        public void SkipDisabledPass()
        {
            var scanChannelAction = new PassScanAction { Name = "Random TV Network" };
            var passes = GetPasses(scanChannelAction);
            passes[0].Enabled = false;

            _processor.QueueMode = true;
            _processor.ProcessPasses(passes);

            _playOn.VerifyNoOtherCalls();
            _queueValidator.VerifyNoOtherCalls();
        }

        [Test]
        public void SkipExcludedPattern()
        {
            var scanChannelAction = new PassScanAction { Name = "*", Exclude = "*Static*"};
            var scanSubFolderAction = new PassScanAction { Name = "*" };
            scanChannelAction.Actions.Add(scanSubFolderAction);

            _processor.ProcessPasses(GetPasses(scanChannelAction));

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv", It.IsAny<IList<PlayOnItem>>()));
            _playOn.VerifyNoOtherCalls();
            _queueValidator.VerifyNoOtherCalls();
        }

        [Test]
        public void SkipMode()
        {
            _processor.SkipMode = true;
            _processor.ProcessPasses(GetBasicWorkflowPass());

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetItems(It.IsAny<string>(), It.IsAny<IList<PlayOnItem>>()));
            _playOn.VerifyNoOtherCalls();
            _queueValidator.Verify(q => q.AddMediaToQueueList(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-vid1")));
            _queueValidator.Verify(q => q.AddMediaToQueueList(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-vid2")));
            _queueValidator.Verify(q => q.AddTemporaryQueueLimits(It.IsAny<PassQueueAction>()));
            _queueValidator.VerifyNoOtherCalls();
        }

        private PassItems GetBasicWorkflowPass()
        {
            var scanChannelAction = new PassScanAction { Name = "Random TV Network" };
            var scanWatchList = new PassScanAction { Name = "My Things To Watch" };
            scanChannelAction.Actions.Add(scanWatchList);
            var queueVideos = new PassQueueAction { Name = "*" };
            scanWatchList.Actions.Add(queueVideos);
            return GetPasses(scanChannelAction);
        }

        private PassItems GetPasses(PassAction action = null)
        {
            var passes = new PassItems();
            var pass = new PassItem("Test Pass", true);
            passes.Add(pass);
            if (action != null) pass.Actions.Add(action);
            return passes;
        }
    }
}
