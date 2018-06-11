using System;
using Moq;
using NUnit.Framework;
using PlaySharp;

namespace PlayPass.Engine.Extensions.Test
{
    [TestFixture]
    public class QueueValidatorTests
    {
        private Mock<IPlayOn> _playOn;
        private PlayOnVideo _video1, _video2, _video3;
        private QueueValidator _validator;

        [SetUp]
        public void Setup()
        {
            _playOn = new Mock<IPlayOn>();
            _playOn.Setup(p => p.LoadItemDetails(It.IsAny<PlayOnItem>()));
            _validator = new QueueValidator(new MemoryQueueList());
            _video1 = new PlayOnVideo(_playOn.Object) { Series = "Random Series", MediaTitle = "My Random - s01e15 - Episode 15 Title", RunTime = "00:10:00"};
            _video2 = new PlayOnVideo(_playOn.Object) { Series = "Random Series", MediaTitle = "My Random - s01e16 - Episode 16 Title", RunTime = "00:15:00" };
            _video3 = new PlayOnVideo(_playOn.Object) { Series = "Random Series", MediaTitle = "My Random - s01e17 - Episode 17 Title", RunTime = "00:20:00" };
        }

        [Test]
        public void CanQueueMedia()
        {
            ValidateCanQueueMedia(_video1);
            _validator.AddMediaToQueueList(_video1);
            ValidateAlreadyQueued(_video1);
        }

        [Test]
        public void CanQueueMediaLimitCount()
        {
            _validator.QueueCountLimit = 1;
            ValidateCanQueueMedia(_video1);
            ValidateCanQueueMedia(_video2);
            _validator.AddMediaToCounts(_video1);
            ValidateQueueLimitReached(_video2);
            _validator.QueueCountLimit = 2;
            ValidateCanQueueMedia(_video2);
        }

        [Test]
        public void CanQueueMediaLimitDuration()
        {
            _validator.QueueDurationLimit = new TimeSpan(0, 0, 15, 0);
            ValidateCanQueueMedia(_video1);
            ValidateCanQueueMedia(_video2);
            _validator.AddMediaToCounts(_video1);
            ValidateQueueDurationLimitReached(_video2);
            _validator.QueueDurationLimit = new TimeSpan(0, 0, 25, 0);
            ValidateCanQueueMedia(_video2);
            _validator.QueueDurationLimit = new TimeSpan(0, 0, 30, 0);
            ValidateCanQueueMedia(_video2);
        }

        [Test]
        public void CanQueueMediaLimitBadDuration()
        {
            _validator.QueueDurationLimit = new TimeSpan(0, 0, 15, 0);
            ValidateCanQueueMedia(_video1);
            ValidateCanQueueMedia(_video2);
            _video2.RunTime = "";
            ValidateQueueDurationLimitReached(_video2);
            _video2.RunTime = "5 HOURS";
            ValidateQueueDurationLimitReached(_video2);
            _validator.QueueDurationLimit = new TimeSpan(0, 5, 0, 0);
            ValidateCanQueueMedia(_video2);
        }

        [Test]
        public void CanQueueMediaTemporaryLimitCount()
        {
            _validator.QueueCountLimit = 3;
            ValidateCanQueueMedia(_video1);
            _validator.AddMediaToQueueList(_video1);
            ValidateCanQueueMedia(_video2);
            var queueAction = new PassQueueAction { CountLimit = 1 };
            using (_validator.AddTemporaryQueueLimits(queueAction))
            {
                ValidateCanQueueMedia(_video2);
                ValidateCanQueueMedia(_video3);
                _validator.AddMediaToQueueList(_video2);
                ValidateQueueLimitReached(_video3);
            }
            ValidateCanQueueMedia(_video3);
        }

        [Test]
        public void CanQueueMediaTemporaryDurationLimitCount()
        {
            _validator.QueueDurationLimit = new TimeSpan(0, 0, 60, 0);
            ValidateCanQueueMedia(_video1);
            _validator.AddMediaToQueueList(_video1);
            ValidateCanQueueMedia(_video2);
            var queueAction = new PassQueueAction { DurationLimit = new TimeSpan(0, 0, 20, 0) };
            using (_validator.AddTemporaryQueueLimits(queueAction))
            {
                ValidateCanQueueMedia(_video2);
                ValidateCanQueueMedia(_video3);
                _validator.AddMediaToQueueList(_video2);
                ValidateQueueDurationLimitReached(_video3);
            }
            ValidateCanQueueMedia(_video3);
        }

        private void ValidateAlreadyQueued(PlayOnVideo video)
        {
            string message;
            Assert.That(_validator.CanQueueMedia(video, out message), Is.False);
            Assert.That(message, Is.EqualTo("Already recorded or skipped."));
        }

        private void ValidateCanQueueMedia(PlayOnVideo video)
        {
            string message;
            Assert.That(_validator.CanQueueMedia(video, out message), Is.True);
            Assert.That(message, Is.Empty);
        }

        private void ValidateQueueDurationLimitReached(PlayOnVideo video)
        {
            string message;
            Assert.That(_validator.CanQueueMedia(video, out message), Is.False);
            Assert.That(message, Is.EqualTo("Queue duration limit reached."));
        }

        private void ValidateQueueLimitReached(PlayOnVideo video)
        {
            string message;
            Assert.That(_validator.CanQueueMedia(video, out message), Is.False);
            Assert.That(message, Is.EqualTo("Queue limit reached."));
        }
    }
}
