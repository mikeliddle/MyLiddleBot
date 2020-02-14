using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.SCBang
{

    public class Game
    {
        [BsonId]
        public ulong Id { get; set; }
        public Dictionary<ulong, Player> Players { get; private set; } = new Dictionary<ulong, Player>();
        public Dictionary<ulong, ulong[]> Votes { get; set; } = new Dictionary<ulong, ulong[]>();
        public Role? WinningRole { get; set; } = null;
        public bool OvertimeReached { get; set; } = false;
        public ulong HostId { get; set; }

        [BsonIgnore]
        public List<Player> SheriffTeam => Players.Where(p => p.Value.Role == Role.Sheriff || p.Value.Role == Role.Deputy).Select(p => p.Value).ToList();
        [BsonIgnore]
        public List<Player> OutlawTeam => Players.Where(p => p.Value.Role == Role.Outlaw).Select(p => p.Value).ToList();
        [BsonIgnore]
        public List<Player> RenegadeTeam => Players.Where(p => p.Value.Role == Role.Renegade).Select(p => p.Value).ToList();

        public static Game CreateGame(List<IUser> mentions, int numPlayers)
        {
            return numPlayers switch
            {
                3 => CreateGame(mentions, 0, 1, 1),
                4 => CreateGame(mentions, 1, 1, 1),
                5 => CreateGame(mentions, 1, 2, 1),
                6 => CreateGame(mentions, 2, 2, 1),
                7 => CreateGame(mentions, 2, 3, 1),
                8 => CreateGame(mentions, 2, 3, 2),
                _ => throw new ArgumentException("You must have between 3 and 8 players."),
            };
        }

        public static Game CreateGame(List<IUser> mentions, int numDeputies, int numOutlaws, int numRenegades)
        {
            if (mentions == null)
                throw new ArgumentNullException(nameof(mentions));

            if (mentions.Where(u => u.IsBot || u.IsWebhook).Count() > 0)
                throw new Exception("Players mentioned must not be Bots or Webhooks you hacker!");

            // Validate inputs
            if (numOutlaws < 0 || numDeputies < 0 || numRenegades < 0)
                throw new Exception("All numbers must be positive!");

            // Validate that more than one users were mentioned
            if (mentions == null || mentions.Count < 3 || mentions.Count > 8)
                throw new Exception("You need between 3 and 8 to play! Mention some friends! You have friends don't you?");

            // Validate that number of mafia is less than number of players
            if (numOutlaws + numDeputies + numRenegades + 1 == mentions.Count)
                throw new Exception("Number of roles must equal the number of players!");

            return createSCBangGame(mentions, numDeputies, numOutlaws, numRenegades);
        }
        
        public bool AddVotes(ulong userId, List<ulong> votes)
        {
            bool addedAll = true;
            foreach(var v in votes)
            {
                addedAll &= AddVote(userId, v);
            }
            return addedAll;
        }

        public bool RemoveVotes(ulong userId, List<ulong> votes)
        {
            bool removedAll = true;
            foreach (var v in votes)
            {
                removedAll &= RemoveVote(userId, v);
            }
            return removedAll;
        }

        public bool AddVote(ulong userId, ulong mafiaId)
        {
            if (userId == mafiaId) return false; // We don't allow you to vote for yourself

            if (!Players.ContainsKey(userId)) return false; // filter out people voting who aren't in the game

            if (!Players.ContainsKey(mafiaId)) return false; // filter out votes for users not in the game

            if (!Votes.ContainsKey(userId))
            {
                Votes[userId] = new ulong[] { mafiaId };
                return true;
            }

            //if (Votes[userId].Length >= Mafia.Count) return false; // only accept the first votes of up to the number of mafia

            if (Votes[userId].Contains(mafiaId)) return false; // we already counted this vote

            Votes[userId] = Votes[userId].Append(mafiaId).ToArray();
            return true;
        }

        public bool RemoveVote(ulong userId, ulong mafiaId)
        {
            if (!Players.ContainsKey(userId)) return false; // filter out people voting who aren't in the game

            if (!Players.ContainsKey(mafiaId)) return false; // filter out votes for users not in the game

            if (!Votes.ContainsKey(userId)) return false; // user hasn't voted return

            if (!Votes[userId].Contains(mafiaId)) return false; // we don't have this vote anyways

            Votes[userId] = Votes[userId].Where(u => u != mafiaId).ToArray();
            return true;
        }

        public bool Score()
        {
            if (!WinningRole.HasValue) return false; // we only score games that have a winner

            foreach (var player in Players.Values)
            {
                int score = 0;
                bool wonGame = player.Role == WinningRole.Value;

                if (player.Role == Role.Outlaw)
                {
                    // Do later
                }
                else if(player.Role == Role.Renegade)
                {
                    // Must be the only renegade to survive.
                }
                else
                {
                    //Sheriff and Deputies
                }

                player.Score = Math.Max(0,score);
            }

            return true;
        }

        public static async Task<Game> GetGameAsync(ulong id, IDiscordClient channel, LiteCollection<Game> collection)
        {
            var game = collection.FindOne(g => g.Id == id);
            if (game == null)
                throw new KeyNotFoundException();

            foreach (var u in game.Players.Values)
                u.DiscordUser = await channel.GetUserAsync(u.Id);

            return game;
        }

        private static Game createSCBangGame(List<IUser> users, int numDeputies, int numOutlaws, int numRenegades)
        {
            var players = users.Shuffle().ToList().Select(u => new Player() { Id = u.Id, Role = Role.Sheriff, DiscordUser = u }).ToList();

            pickRoles(players, numDeputies, numOutlaws, numRenegades);

            Dictionary<ulong, Player> gamePlayers = null;

            try
            {
                gamePlayers = players.ToDictionary(u => u.Id);
            }
            catch (ArgumentException e)
            {
                throw new Exception("Each player must be unique!", e);
            }

            return new Game()
            {
                Players = gamePlayers
            };
        }

        private static void pickRoles(List<Player> players, int numDeputies, int numOutlaws, int numRenegades)
        {
            var randomGenerator = new Random();

            var outlaws = players.Shuffle().ToList().Take(numOutlaws);
            foreach (var o in outlaws)
            {
                o.Role = Role.Outlaw;
                players.Remove(o);
            }

            var deputies = players.Shuffle().ToList().Take(numDeputies);
            foreach (var d in deputies)
            {
                d.Role = Role.Outlaw;
                players.Remove(d);
            }

            var renegades = players.Shuffle().ToList().Take(numRenegades);
            foreach (var r in renegades)
            {
                r.Role = Role.Outlaw;
                players.Remove(r);
            }
        }
    }
}
