using LiteDB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.SC
{
    public class SCQueue
    {
        public ulong ChannelId { get; set; }

        [BsonId]
        public string Name { get; set; }

        public Dictionary<ulong, SCPlayer> Players { get; private set; } = new Dictionary<ulong, SCPlayer>();

        public SCQueue() { }

        public SCQueue(ulong channelId, string name)
        {
            ChannelId = channelId;
            Name = name;
            Players = new Dictionary<ulong, SCPlayer>();
        }
        public static async Task<SCQueue> GetQueueAsync(ulong channelId, string name, IDiscordClient channel, LiteCollection<SCQueue> collection)
        {
            var queue = collection.FindOne(g => g.ChannelId == channelId && g.Name == name);
            if (queue == null)
                return null;

            foreach (var u in queue.Players.Values)
                u.DiscordUser = await channel.GetUserAsync(u.Id);

            return queue;
        }
    }
}
