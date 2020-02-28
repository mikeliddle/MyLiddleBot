using Discord;
using Discord.SCBang;
using LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TyniBot;

namespace SCBang.UnitTests
{
    [TestClass]
    public class SCBangTests
    {
        [TestMethod]
        public void Test()
        {
            using (var Database = new LiteDatabase(@"test.db"))
            {
                var col = Database.GetCollection<IReactionHandler>();
                col.Delete(u => true);
                col.Insert(new GameHandler() { MsgId = 1, GameId = 4 });
                col.EnsureIndex(x => x.MsgId);
                var handler = col.Find(rh => rh.MsgId == 1).FirstOrDefault() as GameHandler;
                Assert.IsNotNull(handler);
                Assert.AreEqual(handler.MsgId, (ulong)1);
                Assert.AreEqual(handler.GameId, (ulong)4);
            }
        }

        //    [TestMethod]
        //    public async Task TestDbStoreRetrieveGame()
        //    {
        //        using (var Database = new LiteDatabase(@"test.db"))
        //        {
        //            var mentions = new List<IUser>();

        //            var user1 = new Mock<IUser>();
        //            user1.Setup(u => u.Username).Returns("bob");
        //            user1.Setup(u => u.Id).Returns(1);
        //            mentions.Add(user1.Object);

        //            var user2 = new Mock<IUser>();
        //            user2.Setup(u => u.Username).Returns("joe");
        //            user2.Setup(u => u.Id).Returns(2);
        //            mentions.Add(user2.Object);

        //            var input = Discord.SCBang.Game.CreateGame(mentions, 1, GameMode.Joker);
        //            input.Id = 1;

        //            var gamesCollection = Database.GetCollection<Discord.SCBang.Game>();
        //            gamesCollection.Delete(u => true);
        //            gamesCollection.Insert(input);
        //            gamesCollection.EnsureIndex(x => x.Id);

        //            var channelMock = new Mock<IDiscordClient>();
        //            channelMock.Setup(u => u.GetUserAsync(user1.Object.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(user1.Object));
        //            channelMock.Setup(u => u.GetUserAsync(user2.Object.Id, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(user2.Object));

        //            var output = await Discord.SCBang.Game.GetGameAsync(input.Id, channelMock.Object, gamesCollection);

        //            Assert.AreEqual(output.Mode, input.Mode);
        //            Assert.AreEqual(input.Players.Count, output.Players.Count);
        //            Assert.AreEqual(output.SCBang.Where(u => input.SCBang.Where(o => o.Id != u.Id).Count() > 0).Count(), 0);
        //            Assert.AreEqual(output.TeamOrange.Where(u => input.TeamOrange.Where(o => o.Id != u.Id).Count() > 0).Count(), 0);
        //            Assert.AreEqual(output.TeamBlue.Where(u => input.TeamBlue.Where(o => o.Id != u.Id).Count() > 0).Count(), 0);
        //            Assert.AreEqual(output.Joker.Id, input.Joker.Id);
        //        }
        //    }

        [TestMethod]
        public void TestCreateGameGeneratesValidGame()
        {
            var mentions = new List<IUser>();
            for (int i = 0; i < 8; i++)
            {
                mentions.Add(GenerateUser(i.ToString(), (ulong)i).Object);
            }

            var game = Discord.SCBang.Game.CreateGame(mentions);

            // Must have teams assigned.
            Assert.IsNotNull(game.SheriffTeam);
            Assert.IsNotNull(game.OutlawTeam);
            Assert.IsNotNull(game.RenegadeTeam);

            Assert.AreEqual(3, game.SheriffTeam.Count);
            Assert.AreEqual(3, game.OutlawTeam.Count);
            Assert.AreEqual(2, game.RenegadeTeam.Count);
        }

        [TestMethod]
        public void TestValidateInputs()
        {
            var mentions = new List<IUser>();
            for (int i = 0; i < 4; i++)
            {
                mentions.Add(GenerateUser(i.ToString(), (ulong)i).Object);
            }

            Assert.ThrowsException<ArgumentNullException>(new Action(() => Discord.SCBang.Game.CreateGame(null))); // must have players
            Assert.ThrowsException<ArgumentException>(new Action(() => Discord.SCBang.Game.CreateGame(new List<IUser>()))); // Can not have zero players
            
            for (int i = 0; i < 5; i++)
            {
                mentions.Add(GenerateUser(i.ToString(), (ulong)i).Object);
            }
            Assert.ThrowsException<ArgumentException>(new Action(() => Discord.SCBang.Game.CreateGame(mentions))); // can not have more mafia than players

            // Valid states

            mentions = new List<IUser>();
            for (int i = 0; i < 3; i++)
            {
                mentions.Add(GenerateUser(i.ToString(), (ulong)i).Object);
            }
            Assert.IsNotNull(Discord.SCBang.Game.CreateGame(mentions));

            mentions = new List<IUser>();
            for (int i = 0; i < 8; i++)
            {
                mentions.Add(GenerateUser(i.ToString(), (ulong)i).Object);
            }
            Assert.IsNotNull(Discord.SCBang.Game.CreateGame(mentions));

            mentions = new List<IUser>();
            for (int i = 0; i < 5; i++)
            {
                mentions.Add(GenerateUser(i.ToString(), (ulong)i).Object);
            }
            Assert.IsNotNull(Discord.SCBang.Game.CreateGame(mentions));

            mentions.Clear();
            Assert.ThrowsException<ArgumentException>(new Action(() => { Discord.SCBang.Game.CreateGame(mentions); })); // Can not have zero players
        }

        private Mock<IUser> GenerateUser(string username, ulong id, bool isBot = false, bool isWebHook = false)
        {
            var user = new Mock<IUser>();
            user.Setup(u => u.IsBot).Returns(isBot);
            user.Setup(u => u.IsWebhook).Returns(isWebHook);
            user.Setup(u => u.Username).Returns(username);
            user.Setup(u => u.Id).Returns(id);
            return user;
        }
    }
}
