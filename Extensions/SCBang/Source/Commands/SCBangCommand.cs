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
            await Output.HelpText(Context.Channel);
        }

        [Command("scbang"), Summary("**!scbang {@player1 @player2 ...}** Creates a new game of SCBang!")]
        public async Task NewGameCommand([Remainder]string message = "")
        {
            if(message == "help")
            {
                await HelpCommand();
            }
            else
            {
                await CreateGame();
            }
        }
        #endregion

        #region Helpers

        private async Task CreateGame()
        {
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

            IUserMessage scoringMessage = await Output.StartGame(game, Context.Channel);
        }
        #endregion
    }
}
