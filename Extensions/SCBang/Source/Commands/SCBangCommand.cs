using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using LiteDB;
using TyniBot;
using Discord.WebSocket;
using System.Diagnostics;

namespace Discord.SCBang
{
    public class SCBangCommand : ModuleBase<TyniBot.CommandContext>
    {
        #region Commands
        [Command("scbang"), Summary("**!scbang help** | Displays this help text.")]
        public async Task HelpCommand()
        {
            Debug.WriteLine("Entered Help Command");

            await Output.HelpText(Context.Channel);
        }

        [Command("scbang"), Summary("**!scbang {@player1 @player2 ...}** Creates a new game of SCBang!")]
        public async Task NewGameCommand([Remainder]string message = "")
        {
            Debug.WriteLine("Entering NewSCBangCommand");

            if(message == "help")
            {
                await HelpCommand();
            }
            else
            {
                await TestGame();
            }
        }
        #endregion

        #region Helpers

        private async Task TestGame()
        {
            Console.WriteLine("Creating a new game");
            Game game;
            try
            {
                game = Game.CreateGame(Context.Message.MentionedUsers.Select(s => (IUser)s).ToList());
            }
            catch (ArgumentException e)
            {
                await ReplyAsync($"{e.Message}");
                return;
            }

            Console.WriteLine("Created Game!");

            Debug.WriteLine("Starting game");
            IUserMessage scoringMessage = await Output.StartGame(game, Context.Channel);
        }

        private async Task CreateGame()
        {
            Debug.WriteLine("Creating a new game");

            Game game = Game.CreateGame(Context.Message.MentionedUsers.Select(s => (IUser)s).ToList());

            Debug.WriteLine("Game Created");

            var games = Context.Database.GetCollection<Game>();

            // Delete current game if exists
            try
            {
                Debug.WriteLine("Checking if game exists");

                var existingGame = await Game.GetGameAsync(Context.Channel.Id, Context.Client, games);
                if (existingGame != null)
                {
                    Debug.WriteLine("Deleting existing game");
                    games.Delete(g => g.Id == existingGame.Id);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error: {e.Message}");
                await Context.Channel.SendMessageAsync($"Error: {e.Message}");
            }

            // Insert into DB
            games.Insert(game);
            games.EnsureIndex(x => x.Id);

            Debug.WriteLine("Starting game");
            IUserMessage scoringMessage = await Output.StartGame(game, Context.Channel);

            var reactionHandlers = Context.Database.GetCollection<IReactionHandler>();
            reactionHandlers.Insert(new GameHandler() { MsgId = scoringMessage.Id, GameId = game.Id });
            reactionHandlers.EnsureIndex(x => x.MsgId);
        }
        #endregion
    }
}
