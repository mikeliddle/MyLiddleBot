using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Discord.SC
{
    [Group("SC")]
    class SCCommand : ModuleBase<TyniBot.CommandContext>
    {
        #region Commands
        [Command("new"), Summary("**!SC new <matchName>** Creates a new game of StarCraft! Each individual player needs to join.")]
        [Alias("newMatch", "newGame", "newmatch", "newgame")]
        public async Task NewSCComman(string name)
        {
            try
            {
                var queue = await CreateQueue(name);
                await Output.QueueStarted(Context.Channel, queue);
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync($"Error: {e.Message}");
            }
        }
        #endregion

        #region Helpers
        private async Task<SCQueue> CreateQueue(string queueName)
        {
            var newQueue = new SCQueue(Context.Channel.Id, queueName);

            var queues = Context.Database.GetCollection<SCQueue>();

            // Delete current queue if exists
            try
            {
                var existing = await SCQueue.GetQueueAsync(Context.Channel.Id, queueName, Context.Client, queues);
                if (existing != null)
                    queues.Delete(g => g.Name == existing.Name);
            }
            catch (Exception) { }

            // Insert into DB
            queues.Insert(newQueue);
            queues.EnsureIndex(x => x.Name);

            return newQueue;
        }
        #endregion
    }
}
