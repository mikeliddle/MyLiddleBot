using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using LiteDB;
using TyniBot;
using Discord.WebSocket;
using Discord.Matches;

namespace Discord.SC.Commands
{
    [Group("SC")]
    class SCCommand : ModuleBase<TyniBot.CommandContext>
    {
    }
}
