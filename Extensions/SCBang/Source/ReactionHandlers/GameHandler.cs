using Discord.WebSocket;
using LiteDB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TyniBot;

namespace Discord.SCBang
{
    public class GameHandler : IReactionHandler
    {
        [BsonId]
        public ulong MsgId { get; set; }
        public ulong GameId { get; set; }

        private ReactionContext Context;
        private IUser UserReacted;
        private Game Game;
        private LiteCollection<Game> Games;
        private LiteCollection<IReactionHandler> ReactionHandlers;

        #region IReactionHandler

        public async Task ReactionAdded(ReactionContext context, SocketReaction reaction)
        {
            if (!(await Validate(context, reaction)))
            {
                if (!UserReacted.IsBot) // we don't need to remove messages we ourselves put there
                    await Context.Message.RemoveReactionAsync(new Emoji(reaction.Emote.Name), reaction.User.Value);
                return;
            }

            if (reaction.Emote.Name == Output.OutlawEmoji) // Which reaction was clicked?
            {
                await SelectWinningTeamAsync(Role.Outlaw);
            }
            else if (reaction.Emote.Name == Output.SheriffEmoji)
            {
                await SelectWinningTeamAsync(Role.Sheriff);
            }
            else if (reaction.Emote.Name == Output.RenegadeEmoji)
            {
                await SelectWinningTeamAsync(Role.Sheriff);
            }
            else if (reaction.Emote.Name == Output.EndedEmoji)
            {
                if (Game.WinningRole == null) // if we don't have a winner remove emoji and return
                {
                    await Context.Message.RemoveReactionAsync(new Emoji(Output.EndedEmoji), UserReacted);
                    return;
                }

                // Un-register this message for receiving new reactions
                ReactionHandlers.Delete(u => u.MsgId == this.MsgId);
            }
        }

        public async Task ReactionRemoved(ReactionContext context, SocketReaction reaction)
        {
            if (!(await Validate(context, reaction)))
                return;

            var sheriffCount = Context.Message.Reactions.Where(e => e.Key.Name == Output.SheriffEmoji).First().Value.ReactionCount;
            var outlawCount = Context.Message.Reactions.Where(e => e.Key.Name == Output.OutlawEmoji).First().Value.ReactionCount;
            var renegadeCount = Context.Message.Reactions.Where(e => e.Key.Name == Output.RenegadeEmoji).First().Value.ReactionCount;

            if (sheriffCount + outlawCount + renegadeCount == 3)
                Game.WinningRole = null;

            Games.Update(Game);
            return;
        }

        public Task ReactionsCleared(ReactionContext context) // Should we do anything here?
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Helpers

        private async Task<bool> Validate(ReactionContext context, SocketReaction reaction)
        {
            if (context == null || reaction == null || !reaction.User.IsSpecified) return false;

            Context = context;
            UserReacted = reaction.User.Value;

            if (UserReacted.IsBot || UserReacted.IsWebhook) return false;

            Games = Context.Database.GetCollection<Game>();
            ReactionHandlers = context.Database.GetCollection<IReactionHandler>();

            Game = await Game.GetGameAsync(this.GameId, Context.Client, Games);

            if (UserReacted.Id != Game.HostId) return false;

            return true;
        }

        private async Task SelectWinningTeamAsync(Role winningRole)
        {
            Game.WinningRole = winningRole; // update game first
            Games.Update(Game);

            var losingEmoji = winningRole == Role.Sheriff ? Output.SheriffEmoji : Output.OutlawEmoji;
            var lostReaction = Context.Message.Reactions.Where(e => e.Key.Name == losingEmoji).First().Value;

            if (lostReaction.ReactionCount > 1)
                await Context.Message.RemoveReactionAsync(new Emoji(losingEmoji), UserReacted);
        }

        #endregion
    }
}
