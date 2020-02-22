using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TyniBot;

namespace Discord.SCBang
{
    public class Output
    {
        public static readonly string OutlawEmoji = EmojiLibrary.ByShortname(":sunglasses:").Unicode;
        public static readonly string SheriffEmoji = EmojiLibrary.ByShortname(":star:").Unicode;
        public static readonly string DeputyEmoji = EmojiLibrary.ByShortname(":cowboy:").Unicode;
        public static readonly string RenegadeEmoji = EmojiLibrary.ByShortname(":man_in_suit:").Unicode;
        public static readonly string EndedEmoji = EmojiLibrary.ByShortname(":checkered_flag:").Unicode;

        private static List<string[]> PossibleEmjoiGroups = new List<string[]>()
        {
            new string[]{
                EmojiLibrary.ByShortname(":one:").Unicode,
                EmojiLibrary.ByShortname(":two:").Unicode,
                EmojiLibrary.ByShortname(":three:").Unicode,
                EmojiLibrary.ByShortname(":four:").Unicode,
                EmojiLibrary.ByShortname(":five:").Unicode,
                EmojiLibrary.ByShortname(":six:").Unicode,
                EmojiLibrary.ByShortname(":seven:").Unicode,
                EmojiLibrary.ByShortname(":eight:").Unicode,
            },
        };

        private static Random rand = new Random();
        private static string[] PossiblePlayerEmojis()
        {
            return PossibleEmjoiGroups[rand.Next(PossibleEmjoiGroups.Count)];
        }

        public static async Task<List<IUserMessage>> NotifyStartGame(Game game)
        {
            // Notify each Player
            var msgs = new List<IUserMessage>();
            foreach (var player in game.Players.Values)
                msgs.Add(await player.SendMessageAsync($"You are a {player.Role}!"));
            return msgs;
        }

        public static async Task<IUserMessage> StartGame(Game game, IMessageChannel channel)
        {
            await NotifyStartGame(game);

            EmbedBuilder embedBuilder = new EmbedBuilder();

            embedBuilder.AddField("Game Result:", $"{Output.SheriffEmoji} Sheriff Died! {Output.DeputyEmoji} Deputy Died! {Output.OutlawEmoji} Outlaw Died! {Output.RenegadeEmoji} Renegade Died! {Output.EndedEmoji} End Game!");

            var msg = await channel.SendMessageAsync($"**New Mafia Game! Deputies: {game.SheriffTeam.Count - 1}, Outlaws: {game.OutlawTeam}, Renegades: {game.RenegadeTeam}**", false, embedBuilder.Build());

            var reactions = new List<IEmote>() { new Emoji(Output.SheriffEmoji), new Emoji(Output.DeputyEmoji), new Emoji(Output.OutlawEmoji), new Emoji(Output.RenegadeEmoji), new Emoji(Output.EndedEmoji) };
            await msg.AddReactionsAsync(reactions.ToArray());

            return msg;
        }

        public static async Task<IUserMessage> HelpText(IMessageChannel channel)
        {
            var commands = typeof(SCBangCommand).GetMethods()
                      .Where(m => m.GetCustomAttributes(typeof(SummaryAttribute), false).Length > 0)
                      .ToArray();

            EmbedBuilder embedBuilder = new EmbedBuilder();

            foreach (var command in commands)
            {
                var name = (CommandAttribute)command.GetCustomAttributes(typeof(CommandAttribute), false)[0];
                var summary = (SummaryAttribute)command.GetCustomAttributes(typeof(SummaryAttribute), false)[0];
                // Get the command Summary attribute information
                string embedFieldText = summary.Text ?? "No description available\n";

                embedBuilder.AddField(name.Text, embedFieldText);
            }

            return await channel.SendMessageAsync("**Mafia Commands:** ", false, embedBuilder.Build());
        }
    }
}
