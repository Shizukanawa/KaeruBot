using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace KaeruBot
{
    class ServerCommands
    {
        [Command("getmembers"), Description("Gets the counts of members")]
        public async Task GetMembers(CommandContext ctx)
        {
            int members = 0;

            for (int i = 0; i < ctx.Guild.Members.Count; i++)
            {
                members = members + 1;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"There are **{members.ToString()}** members"
            };
            await ctx.RespondAsync(embed: embed.Build());
        }
    }
}
