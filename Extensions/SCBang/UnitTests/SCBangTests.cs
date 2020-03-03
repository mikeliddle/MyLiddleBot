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
