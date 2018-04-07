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
                    new PlayOnFolder(playOn) {Name = "Random TV Network", Url = "/data/data.xml?id=rtv"}
                })
                {
                    Name = "SERVER",
                    Url = "/data/data.xml?id=0"
                }
            );

            // Random TV Network
            mock.Setup(p => p.GetItems(It.IsAny<string>(), It.IsAny<IList<PlayOnItem>>()))
                .Callback<string, IList<PlayOnItem>>((url, list) =>
                {
                    switch (url) {
                        case "/data/data.xml?id=rtv":
                            list.Add(new PlayOnFolder(playOn) { Name = "My Things To Watch", Url = "/data/data.xml?id=rtv-queue" });
                            break;
                        case "/data/data.xml?id=rtv-queue":
                            list.Add(new PlayOnVideo(playOn) { Name = "Test Video 1", Url = "/data/data.xml?id=rtv-vid1" });
                            list.Add(new PlayOnVideo(playOn) { Name = "Test Video 2", Url = "/data/data.xml?id=rtv-vid2" });
                            break;
                        default:
                            throw new Exception("Unhandled URL: " + url);
                    }
                });

            // Queue Media Successfully
            mock.Setup(p => p.QueueMedia(It.IsAny<PlayOnVideo>())).Returns(PlayOnConstants.QueueVideoResult.Success);

            return mock;
        }
    }
}
