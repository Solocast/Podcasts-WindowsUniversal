using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Solocast.Services;
using System.Diagnostics;
using Solocast.Core.Contracts;
using Solocast.DAL;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;

namespace Solocast.Tests
{
    /// <summary>
    /// This class is used for testing purposes only. No actual unit testing is done. For now...
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void RandomTest()
        {
            //Arrange
            var feedParser = new FeedParserService();
            //Act
            var podcast = feedParser.GetPodcastAsync("http://monstercat.com/podcast/feed.xml").Result;
            //Assert
            Assert.AreEqual(true, true);
        }

        [TestMethod]
        public void TestLocalStorage()
        {
            //Arrange
            var localStorage = new LocalPodcastService("podcasts.json");
            var feedParser = new FeedParserService();
            var podcastService = new PodcastService(feedParser, localStorage, null);

            var podcasts = new List<Podcast>();
            var podcast = podcastService.GetPodcastAsync("http://monstercat.com/podcast/feed.xml").Result;
            podcasts.Add(podcast);

            podcastService.SavePodcastsAsync(podcasts).Wait();
           
            //Act
            var podcastsFromStorage = podcastService.GetPodcastsAsync().Result.ToList();
            
            //Assert
            Assert.AreEqual(podcasts.Count, podcastsFromStorage.Count);

            for (int i = 0; i < podcasts.Count; i++)
            {
                Assert.AreEqual(true, podcasts[i].Equals(podcastsFromStorage[i]));
            }
        }

        [TestMethod]
        public void TestSQLiteStorage()
        {
            //Arrange
            var sqliteStorage = new SQLitePodcastService();
            sqliteStorage.Migrate();
            var feedParser = new FeedParserService();
            var podcastService = new PodcastService(feedParser, sqliteStorage, null);

            var podcasts = new List<Podcast>();
            var podcast = podcastService.GetPodcastAsync("http://monstercat.com/podcast/feed.xml").Result;
            podcasts.Add(podcast);

            podcastService.SavePodcastAsync(podcast).Wait();
            podcastService.SavePodcastsAsync(podcasts).Wait();
            
            //Act
            var podcastsFromStorage = podcastService.GetPodcastsAsync().Result.ToList();
            
            //Assert
            Assert.AreEqual(podcasts.Count, podcastsFromStorage.Count);

            for (int i = 0; i < podcasts.Count; i++)
            {
                Assert.AreEqual(true, podcasts[i].Equals(podcastsFromStorage[i]));
            }
        }

        [TestMethod]
        public void TestDownload()
        {
            //Arrange
            var localStorage = new LocalPodcastService("podcasts.json");
            var feedParser = new FeedParserService();
            var fileManager = new FileDownloadService();
            var podcastService = new PodcastService(feedParser, localStorage, fileManager);

            //Act
            var podcastFromService = podcastService.GetPodcastAsync("http://monstercat.com/podcast/feed.xml").Result;
            
            //Assert
            podcastService.DownloadEpisodeAsync(podcastFromService.Episodes.OrderByDescending(e => e.Published).ToList()[0]).Wait();
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {         
                var localFolder = ApplicationData.Current.LocalFolder;
                var file = localFolder.GetFileAsync("podcast.json").AsTask().Result;                
                file.DeleteAsync().AsTask().Wait();
            }
            catch
            {
                //file does not exist which is ok;
            }
        }
    }
}
