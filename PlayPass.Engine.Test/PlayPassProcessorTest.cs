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
        public void ProcessPassEncounteredException()
        {
            _playOn.Setup(p => p.GetCatalog()).Throws(new System.Exception("Simulated Error"));

            var scanChannelAction = new PassScanAction { Name = "Random TV Network" };
            var queueAction = new PassQueueAction { Name = "*" };
            scanChannelAction.Actions.Add(queueAction);

            _processor.QueueMode = true;
            _processor.ProcessPasses(GetPasses(scanChannelAction));

            _playOn.Verify(p => p.GetCatalog());
            _playOn.VerifyNoOtherCalls();

            _logManager.Verify(m => m.LogException(It.Is<System.Exception>(e => e.Message.Contains("Simulated Error"))));
        }

        [Test]
        public void QueueAlreadyQueued()
        {
            _playOn.Setup(p => p.QueueMedia(It.IsAny<PlayOnVideo>())).Returns(PlayOnConstants.QueueVideoResult.AlreadyInQueue);

            var scanChannelAction = new PassScanAction { Name = "Random TV Network" };
            var queueAction = new PassQueueAction { Name = "*" };
            scanChannelAction.Actions.Add(queueAction);

            _processor.QueueMode = true;
            _processor.ProcessPasses(GetPasses(scanChannelAction));

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv", It.IsAny<IList<PlayOnItem>>()));
            _playOn.Verify(p => p.QueueMedia(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-clip1")));
            _playOn.VerifyNoOtherCalls();

            string msg;
            _queueValidator.Verify(q => q.AddTemporaryQueueLimits(It.IsAny<PassQueueAction>()));
            _queueValidator.Verify(q => q.CanQueueMedia(It.IsAny<PlayOnVideo>(), out msg));
            _queueValidator.VerifyNoOtherCalls();

            _logManager.Verify(m => m.Log(It.IsAny<string>(), It.Is<object[]>(v => v.Length == 2 && v[1].ToString().Contains("Already queued"))));
        }

        [Test]
        public void QueueLinkNotFound()
        {
            _playOn.Setup(p => p.QueueMedia(It.IsAny<PlayOnVideo>())).Returns(PlayOnConstants.QueueVideoResult.PlayLaterNotFound);

            var scanChannelAction = new PassScanAction { Name = "Random TV Network" };
            var queueAction = new PassQueueAction { Name = "*" };
            scanChannelAction.Actions.Add(queueAction);

            _processor.QueueMode = true;
            _processor.ProcessPasses(GetPasses(scanChannelAction));

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv", It.IsAny<IList<PlayOnItem>>()));
            _playOn.Verify(p => p.QueueMedia(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-clip1")));
            _playOn.VerifyNoOtherCalls();

            string msg;
            _queueValidator.Verify(q => q.AddTemporaryQueueLimits(It.IsAny<PassQueueAction>()));
            _queueValidator.Verify(q => q.CanQueueMedia(It.IsAny<PlayOnVideo>(), out msg));
            _queueValidator.VerifyNoOtherCalls();

            _logManager.Verify(m => m.Log(It.IsAny<string>(), It.Is<object[]>(v => v.Length == 2 && v[0].ToString() == "Skipped" && v[1].ToString().Contains("queue link not found"))));
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

        [TestCase(false)]
        [TestCase(true)]
        public void QueueReverse(bool reverseValue)
        {
            // Track the queue order
            var urls = new List<string>();
            _playOn.Setup(p => p.QueueMedia(It.IsAny<PlayOnVideo>()))
                .Returns<PlayOnVideo>((video) =>
                {
                    urls.Add(video.Url);
                    return PlayOnConstants.QueueVideoResult.Success;
                });

            var scanChannelAction = new PassScanAction { Name = "Random TV Network" };
            var scanWatchList = new PassScanAction { Name = "My Things To Watch" };
            scanChannelAction.Actions.Add(scanWatchList);
            var queueVideos = new PassQueueAction { Name = "*", Reverse = reverseValue };
            scanWatchList.Actions.Add(queueVideos);

            _processor.QueueMode = true;
            _processor.ProcessPasses(GetPasses(scanChannelAction));

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv", It.IsAny<IList<PlayOnItem>>()));
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv-queue", It.IsAny<IList<PlayOnItem>>()));
            _playOn.Verify(p => p.QueueMedia(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-vid1")));
            _playOn.Verify(p => p.QueueMedia(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-vid2")));
            _playOn.VerifyNoOtherCalls();

            // Verify the queue order
            Assert.AreEqual(2, urls.Count);
            Assert.AreEqual((!reverseValue ? 0 : 1), urls.IndexOf("/data/data.xml?id=rtv-vid1"));
            Assert.AreEqual((!reverseValue ? 1 : 0), urls.IndexOf("/data/data.xml?id=rtv-vid2"));
        }

        [Test]
        public void QueueThrewError()
        {
            _playOn.Setup(p => p.QueueMedia(It.IsAny<PlayOnVideo>())).Throws(new System.Exception("Simulated Error"));

            var scanChannelAction = new PassScanAction { Name = "Random TV Network" };
            var queueAction = new PassQueueAction { Name = "*" };
            scanChannelAction.Actions.Add(queueAction);

            _processor.QueueMode = true;
            _processor.ProcessPasses(GetPasses(scanChannelAction));

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv", It.IsAny<IList<PlayOnItem>>()));
            _playOn.Verify(p => p.QueueMedia(It.Is<PlayOnVideo>(v => v.Url == "/data/data.xml?id=rtv-clip1")));
            _playOn.VerifyNoOtherCalls();

            string msg;
            _queueValidator.Verify(q => q.AddTemporaryQueueLimits(It.IsAny<PassQueueAction>()));
            _queueValidator.Verify(q => q.CanQueueMedia(It.IsAny<PlayOnVideo>(), out msg));
            _queueValidator.VerifyNoOtherCalls();

            _logManager.Verify(m => m.Log(It.IsAny<string>(), It.Is<object[]>(v => v.Length == 2 && v[0].ToString() == "Skipped" && v[1].ToString().Contains("Simulated Error"))));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ScanReverse(bool reverseValue)
        {
            // Track the scan order
            var urls = new List<string>();
            _playOn.Setup(p => p.GetItems(It.IsAny<string>(), It.IsAny<IList<PlayOnItem>>()))
                .Callback<string, IList<PlayOnItem>>((url, list) => urls.Add(url));

            var scanChannelAction = new PassScanAction { Name = "*", Reverse = reverseValue };
            var queueVideos = new PassQueueAction { Name = "*" };
            scanChannelAction.Actions.Add(queueVideos);

            _processor.QueueMode = true;
            _processor.ProcessPasses(GetPasses(scanChannelAction));

            _playOn.Verify(p => p.GetCatalog());
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=stv", It.IsAny<IList<PlayOnItem>>()));
            _playOn.Verify(p => p.GetItems("/data/data.xml?id=rtv", It.IsAny<IList<PlayOnItem>>()));
            _playOn.VerifyNoOtherCalls();

            // Verify the scan order
            Assert.AreEqual(2, urls.Count);
            Assert.AreEqual((!reverseValue ? 0 : 1), urls.IndexOf("/data/data.xml?id=rtv"));
            Assert.AreEqual((!reverseValue ? 1 : 0), urls.IndexOf("/data/data.xml?id=stv"));
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
        public void SearchNotSearchable()
        {
            var scanChannelAction = new PassScanAction { Name = "Random TV Network" };
            var searchChannelAction = new PassSearchAction { Name = "Nothing" };
            scanChannelAction.Actions.Add(searchChannelAction);
            var queueVideos = new PassQueueAction { Name = "*" };
            searchChannelAction.Actions.Add(queueVideos);

            _processor.QueueMode = true;
            _processor.ProcessPasses(GetPasses(scanChannelAction));

            _playOn.Verify(p => p.GetCatalog());
            _playOn.VerifyNoOtherCalls();

            _logManager.Verify(m => m.Log(It.Is<string>(s => s.Contains("not searchable")), It.IsAny<object[]>()));
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
