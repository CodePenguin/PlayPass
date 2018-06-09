using System.IO;
using Moq;
using NUnit.Framework;
using PlaySharp;

namespace PlayPass.Engine.Extensions.Test
{
    [TestFixture]
    public class FileQueueListTests
    {
        private Mock<IPlayOn> _playOn;
        private PlayOnVideo _video1;
        private string _video1SkipFileName = "Random Series - My Random - s01e15 - Episode 15 Title.playpass.skip";
        private IQueueList _queue;
        private string _queueLocation;

        [SetUp]
        public void Setup()
        {
            _playOn = new Mock<IPlayOn>();
            _playOn.Setup(p => p.LoadItemDetails(It.IsAny<PlayOnItem>()));
            _queueLocation = TestContext.CurrentContext.TestDirectory;
            _queue = new FileQueueList();
            _queue.Initialize($"Provider=FileQueueList;Data Source={_queueLocation}");

            _video1 = new PlayOnVideo(_playOn.Object) { Series = "Random Series", MediaTitle = "My Random - s01e15 - Episode 15 Title" };
        }

        [Test]
        public void ExceptionForInvalidPath()
        {
            Assert.That(() => _queue.Initialize("Data Source=SomeBadFolder"), Throws.Exception.With.Message.Contains("Queue List data path does not exist"));
        }

        [Test]
        public void SkipFileName()
        {
            var skipFilePath = Path.Combine(_queueLocation, _video1SkipFileName);
            // Make sure the skip file does not exist
            File.Delete(skipFilePath);
            Assert.That(File.Exists(skipFilePath), Is.False);
            Assert.That(_queue.MediaInList(_video1), Is.False);
            // Skip the file
            _queue.AddMediaToList(_video1);
            Assert.That(_queue.MediaInList(_video1), Is.True);
            Assert.That(File.Exists(skipFilePath), Is.True);
        }

        [Test]
        public void SkipAlreadySkippedFileName()
        {
            var skipFilePath = Path.Combine(_queueLocation, _video1SkipFileName);
            // Make sure the skip file does not exist
            File.Delete(skipFilePath);
            Assert.That(File.Exists(skipFilePath), Is.False);
            // Skip the file once
            _queue.AddMediaToList(_video1);
            Assert.That(_queue.MediaInList(_video1), Is.True);
            Assert.That(File.Exists(skipFilePath), Is.True);
            // Skip the file a second time
            _queue.AddMediaToList(_video1);
            Assert.That(_queue.MediaInList(_video1), Is.True);
            Assert.That(File.Exists(skipFilePath), Is.True);
        }
    }
}
