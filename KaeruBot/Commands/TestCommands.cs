﻿using System;
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
using Newtonsoft.Json;
using Shizukanawa.RiotAPI;

namespace Shizukanawa.KaeruBot
{
    class TestCommands : BaseCommandModule
    {

        [Command("fields")]
        public async Task Fields(CommandContext ctx)
        {

            var embed = new DiscordEmbedBuilder()
            {
                Title = "Test",
                Description = "hi",
            };
            embed.AddField("Test", "Hi", true);
            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("phone")]
        public async Task Phone(CommandContext ctx)
        {
            await ctx.RespondAsync(ctx.User.Verified.Value.ToString());
        }

        [Command("reaction"), Description("a test")]
        public async Task React(CommandContext ctx)
        {
            var emoji1 = DiscordEmoji.FromName(ctx.Client, ":one:");
            var emoji2 = DiscordEmoji.FromName(ctx.Client, ":two:");

            var embed = new DiscordEmbedBuilder()
            {
                Description = "1",
            };

            var embed2 = new DiscordEmbedBuilder()
            {
                Description = "2",
            };

            DiscordMessage Embed = await ctx.RespondAsync(embed: embed.Build());
            await Embed.CreateReactionAsync(emoji1);
            await Task.Delay(500);
            await Embed.CreateReactionAsync(emoji2);

            var interactivity = ctx.Client.GetInteractivity();
            var result1 = await interactivity.WaitForReactionAsync(x => x.Emoji == emoji1, ctx.User, TimeSpan.FromSeconds(60));
            var result2 = await interactivity.WaitForReactionAsync(x => x.Emoji == emoji2, ctx.User, TimeSpan.FromSeconds(60));
            
            if (result1.Result != null)
            {
                await Embed.ModifyAsync(embed: embed.Build());
                await Embed.DeleteAllReactionsAsync();
            }
            if (result2.Result != null)
            {
                await Embed.ModifyAsync(embed: embed2.Build());
                await Embed.DeleteAllReactionsAsync();
            }
        }
    }
}
