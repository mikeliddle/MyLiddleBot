using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using LiteDB;
using TyniBot;
using Discord.WebSocket;

namespace Discord.SCBang
{
    public class SCBangCommand : ModuleBase<TyniBot.CommandContext>
    {
        #region Commands
        [Command("scbang"), Summary("**!scbang <matchName> {<numDeputies> <numOutlaws> <numRenegades>}** Creates a new game of SCBang!")]
        public async Task NewSCBangCommand([Remainder] string message = "")
        {
            try
            {
                Game game = Game.CreateGame(Context.Message.MentionedUsers.Select(s => (IUser) s).ToList());
                
                var games = Context.Database.GetCollection<Game>();

                // Delete current game if exists
                try
                {
                    var existingGame = await Game.GetGameAsync(Context.Channel.Id, Context.Client, games);
                    if (existingGame != null)
                        games.Delete(g => g.Id == existingGame.Id);
                }
                catch (Exception) { }

                // Insert into DB
                games.Insert(game);
                games.EnsureIndex(x => x.Id);

                IUserMessage scoringMessage = await Output.StartGame(game, Context.Channel);

                var reactionHandlers = Context.Database.GetCollection<IReactionHandler>();
                reactionHandlers.Insert(new GameHandler() { MsgId = scoringMessage.Id, GameId = game.Id });
                reactionHandlers.EnsureIndex(x => x.MsgId);
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync($"Error: {e.Message}");
            }
        }

        [Command("scbang")]
        public async Task NewSCBangCommand(int numDeputies, int numOutlaws, int numRenegades, [Remainder]string message = "")
        {
            try
            {
                var game = Game.CreateGame(Context.Message.MentionedUsers.Select(s => (IUser)s).ToList(), numDeputies, numOutlaws, numRenegades);
                
                var games = Context.Database.GetCollection<Game>();

                // Delete current game if exists
                try
                {
                    var existingGame = await Game.GetGameAsync(Context.Channel.Id, Context.Client, games);
                    if (existingGame != null)
                        games.Delete(g => g.Id == existingGame.Id);
                }
                catch (Exception) { }

                // Insert into DB
                games.Insert(game);
                games.EnsureIndex(x => x.Id);


                IUserMessage scoringMessage = await Output.StartGame(game, Context.Channel);

                var reactionHandlers = Context.Database.GetCollection<IReactionHandler>();
                reactionHandlers.Insert(new GameHandler() { MsgId = scoringMessage.Id, GameId = game.Id });
                reactionHandlers.EnsureIndex(x => x.MsgId);
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync($"Error: {e.Message}");
            }
        }

        [Command("scbang")]
        public async Task NewSCBangCommand(string helpText, [Remainder] string message = "")
        {
            if (helpText.ToLower() == "help")
            {
                await HelpCommand();
                return;
            }

            await NewSCBangCommand();
        }

        [Command("scbang"), Summary("**!scbang help** | Displays this help text.")]
        public async Task HelpCommand()
        {
            await Output.HelpText(Context.Channel);
        }
        #endregion

        #region Helpers

        #endregion
    }
}
