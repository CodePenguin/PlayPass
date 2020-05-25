using System;
using System.Collections.Generic;
using Moq;
using PlaySharp;

namespace PlayPass.Engine.Test
{
    public static class Mocks
    {
        public static Mock<IPlayOn> GetPlayOn()
        {
            var mock = new Mock<IPlayOn>();
            var playOn = mock.Object;

            // Catalog
            mock.Setup(p => p.GetCatalog()).Returns(
                new PlayOnCatalog(playOn, new List<PlayOnItem>
                {
                    new PlayOnFolder(playOn) {Name = "Random TV Network", Url = "/data/data.xml?id=rtv"},
                    new PlayOnFolder(playOn) {Name = "Static TV Network", Url = "/data/data.xml?id=stv", Searchable = true}
                }) { Name = "SERVER", Url = "/data/data.xml?id=0" }
            );

            // Random TV Network
            mock.Setup(p => p.GetItems(It.IsAny<string>(), It.IsAny<IList<PlayOnItem>>()))
                .Callback<string, IList<PlayOnItem>>((url, list) =>
                {
                    switch (url) {
                        case "/data/data.xml?id=rtv":
                            list.Add(new PlayOnFolder(playOn) { Name = "My Things To Watch", Url = "/data/data.xml?id=rtv-queue" });
                            list.Add(new PlayOnVideo(playOn) { Name = "Random Clip 1", Url = "/data/data.xml?id=rtv-clip1" });
                            break;
                        case "/data/data.xml?id=rtv-queue":
                            list.Add(new PlayOnVideo(playOn) { Name = "Random Video 1", Url = "/data/data.xml?id=rtv-vid1" });
                            list.Add(new PlayOnVideo(playOn) { Name = "Random Video 2", Url = "/data/data.xml?id=rtv-vid2" });
                            break;
                        case "/data/data.xml?id=stv":
                            list.Add(new PlayOnVideo(playOn) { Name = "Static Video 1", Url = "/data/data.xml?id=stv-vid1" });
                            list.Add(new PlayOnVideo(playOn) { Name = "Static Video 2", Url = "/data/data.xml?id=stv-vid2" });
                            break;
                        default: throw new Exception("Unhandled URL: " + url);
                    }
                });

            mock.Setup(p => p.GetSearchResults(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string,string>((url, searchTerm) =>
                {
                    switch (url)
                    {
                        case "/data/data.xml?id=stv":
                            var searchResults = new List<PlayOnItem>();
                            if (searchTerm == "Video")
                            {
                                searchResults.Add(new PlayOnVideo(playOn) { Name = "Static Video 1", Url = "/data/data.xml?id=stv-vid1" });
                                searchResults.Add(new PlayOnVideo(playOn) { Name = "Static Video 2", Url = "/data/data.xml?id=stv-vid2" });
                            }
                            return new PlayOnFolder(playOn, searchResults) { Name = "Static TV Network", Url = "/data/data.xml?id=stv", Searchable = true };
                        default: throw new Exception("Unhandled URL: " + url);
                    }
                 });


            // Queue Media Successfully
            mock.Setup(p => p.QueueMedia(It.IsAny<PlayOnVideo>())).Returns(PlayOnConstants.QueueVideoResult.Success);

            return mock;
        }

        public static Mock<IQueueValidator> GetQueueValidator()
        {
            var queueValidator = new Mock<IQueueValidator>() { DefaultValue = DefaultValue.Mock };
            string message;
            queueValidator.Setup(v => v.AddMediaToQueueList(It.IsAny<PlayOnVideo>()));
            queueValidator.Setup(v => v.AddMediaToCounts(It.IsAny<PlayOnVideo>()));
            queueValidator.Setup(v => v.CanQueueMedia(It.IsAny<PlayOnVideo>(), out message)).Returns(true);
            queueValidator.Setup(v => v.AddTemporaryQueueLimits(It.IsAny<PassQueueAction>())).Returns<IDisposable>(null);
            return queueValidator;
        }
    }
}
