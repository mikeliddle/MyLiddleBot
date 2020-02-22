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

        //    [TestMethod]
        //    public void TestCreateGameGeneratesValidGame()
        //    {
        //        Random r = new Random();

        //        for (int j = 0; j < 100; j++)
        //        {
        //            var mentions = new List<IUser>();
        //            for (int i = 0; i < (j % 7) + 2; i++)
        //            {
        //                mentions.Add(GenerateUser(i.ToString(), (ulong)i).Object);
        //            }

        //            for (int i = 0; i < 300; i++)
        //            {
        //                var numSCBang = (i % (mentions.Count - 1)) + 1;
        //                int random = r.Next(3);
        //                GameMode mode = GameMode.Normal;
        //                if (random == 1)
        //                    mode = GameMode.Battle;
        //                if (random == 2 && mentions.Count > numSCBang)
        //                    mode = GameMode.Joker;

        //                var game = (Discord.SCBang.Game.CreateGame(mentions, numSCBang, mode));

        //                Assert.AreEqual(numSCBang, game.SCBang.Count()); // validate actual number of mafia was as requested
        //                Assert.AreEqual(game.TeamOrange.Count() + game.TeamBlue.Count(), mentions.Count); // validate members of both teams equals total count of mentions

        //                if (mode == GameMode.Joker)
        //                {
        //                    Assert.IsNotNull(game.Joker); // game must contain a joker
        //                    Assert.IsTrue(mentions.Contains(game.Joker.DiscordUser)); // joker must be in original mentions
        //                    Assert.IsFalse(game.SCBang.Contains(game.Joker)); // joker can't be mafia
        //                    Assert.AreEqual(game.Villagers.Count, mentions.Count - numSCBang - 1); // assert number of villagers equals mentions - numSCBang - joker
        //                }
        //                else
        //                {
        //                    Assert.AreEqual(game.Villagers.Count, mentions.Count - numSCBang); // assert number of villagers equals mentions - numSCBang
        //                }

        //                if(mode == GameMode.Joker || mode == GameMode.Battle)
        //                {
        //                    int team1SCBang = game.SCBang.Where(u => u.Team == Discord.SCBang.Team.Orange).Count();
        //                    int team2SCBang = game.SCBang.Where(u => u.Team == Discord.SCBang.Team.Blue).Count();

        //                    if(numSCBang > 1) 
        //                    {
        //                        Assert.AreNotEqual(0, team1SCBang); // assert both teams have a mafia member
        //                        Assert.AreNotEqual(0, team2SCBang);

        //                        if (numSCBang % 2 == 0) // if even
        //                        {
        //                            Assert.AreEqual(team1SCBang, team2SCBang); // assert evenly split
        //                        }
        //                        else // if odd
        //                        {
        //                            int sub = team1SCBang > team2SCBang ? team1SCBang - team2SCBang : team2SCBang - team1SCBang;
        //                            Assert.AreEqual(1, sub); // assert difference is one
        //                        }
        //                    }
        //                }

        //                var mafia = new Dictionary<string, string>();
        //                var t1 = new Dictionary<string, string>();
        //                var t2 = new Dictionary<string, string>();

        //                foreach (var u in game.SCBang)
        //                {
        //                    Assert.IsTrue(mentions.Contains(u.DiscordUser)); // validate each mafia member was part of original mentions
        //                    Assert.AreEqual(game.SCBang.Where(p => p.Id == u.Id).Count(), 1); // validate users weren't added to mafia twice
        //                    mafia.Add(u.Username, u.Username);
        //                }
        //                foreach (var u in game.TeamOrange)
        //                {
        //                    t1.Add(u.Username, u.Username);
        //                    Assert.IsTrue(mentions.Contains(u.DiscordUser)); // validate every team member was part of original mentions
        //                    Assert.AreEqual(game.TeamOrange.Where(p => p.Id == u.Id).Count(), 1); // assert member is added to the team only once
        //                }
        //                foreach (var u in game.TeamBlue)
        //                {
        //                    t2.Add(u.Username, u.Username);
        //                    Assert.IsTrue(mentions.Contains(u.DiscordUser)); // validate every team member was part of original mentions
        //                    Assert.IsFalse(t1.ContainsKey(u.Username)); // validate every team2 member is not in team 1
        //                    Assert.AreEqual(game.TeamBlue.Where(p => p.Id == u.Id).Count(), 1); // assert member is added to the team only once
        //                }
        //                foreach (var u in game.TeamOrange)
        //                {
        //                    Assert.IsFalse(t2.ContainsKey(u.Username)); // validate every team1 member is not in team 2
        //                }

        //            }
        //        }
        //    }

        [TestMethod]
        public void TestValidateInputs()
        {
            var mentions = new List<IUser>();
            for (int i = 0; i < 4; i++)
            {
                mentions.Add(GenerateUser(i.ToString(), (ulong)i).Object);
            }

            Assert.ThrowsException<ArgumentException>(new Action(() => Discord.SCBang.Game.CreateGame(null, 1))); // must have players
            Assert.ThrowsException<ArgumentException>(new Action(() => Discord.SCBang.Game.CreateGame(mentions, 0))); // Can not have zero mafia
            Assert.ThrowsException<ArgumentException>(new Action(() => Discord.SCBang.Game.CreateGame(mentions, -1))); // Can not have negative mafia
            Assert.ThrowsException<ArgumentException>(new Action(() => Discord.SCBang.Game.CreateGame(mentions, 9))); // can not have more mafia than players

            // Valid states
            Assert.IsNotNull(Discord.SCBang.Game.CreateGame(mentions, 3));
            Assert.IsNotNull(Discord.SCBang.Game.CreateGame(mentions, 8));
            Assert.IsNotNull(Discord.SCBang.Game.CreateGame(mentions, 5));

            mentions.Clear();
            Assert.ThrowsException<Exception>(new Action(() => { Discord.SCBang.Game.CreateGame(mentions, 1); })); // Can not have zero players

            mentions.Add(GenerateUser("1", 1, isBot: true).Object);
            mentions.Add(GenerateUser("2", 2).Object);
            Assert.ThrowsException<Exception>(new Action(() => Discord.SCBang.Game.CreateGame(mentions, 1)));

            mentions.Clear();
            mentions.Add(GenerateUser("1", 1, isWebHook: true).Object);
            mentions.Add(GenerateUser("2", 2).Object);
            Assert.ThrowsException<Exception>(new Action(() => Discord.SCBang.Game.CreateGame(mentions, 1)));

            mentions.Clear();
            mentions.Add(GenerateUser("1", 1).Object);
            mentions.Add(GenerateUser("1", 1).Object);
            Assert.ThrowsException<Exception>(new Action(() => Discord.SCBang.Game.CreateGame(mentions, 1)));

            mentions.Clear();
            mentions.Add(GenerateUser("1", 1).Object);
            mentions.Add(GenerateUser("2", 2).Object);
            Assert.ThrowsException<Exception>(new Action(() => Discord.SCBang.Game.CreateGame(mentions, 2)));

            mentions.Clear();
            mentions.Add(GenerateUser("1", 1).Object);
            mentions.Add(GenerateUser("2", 2).Object);
            Assert.ThrowsException<Exception>(new Action(() => Discord.SCBang.Game.CreateGame(mentions, 2)));
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
