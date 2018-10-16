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
using DSharpPlus.VoiceNext;

namespace Shizukanawa.KaeruBot
{
    class KaraokeCommands
    {
        /*[Command("join")]
        public async Task Join(CommandContext ctx, DiscordChannel channel)
        {
            var voice = ctx.Client.GetVoiceNextClient();
            if (voice == null)
            {

            }
        }*/

        [Command("karaoke")]
        public async Task Karaoke(CommandContext ctx, [RemainingTextAttribute] string Song)
        {

            if (Song.ToLower() == "sakura skip")
            {
                DiscordMessage x = await ctx.RespondAsync("Karaoke of **Sakura Skip** will begin in 10 seconds\nMeguri megutte eburiwan\nNanika ga matteiru nokana");
                await Task.Delay(1000);
                await x.ModifyAsync("Karaoke of **Sakura Skip** will begin in 9 seconds\nMeguri megutte eburiwan\nNanika ga matteiru nokana");
                await Task.Delay(1000);
                await x.ModifyAsync("Karaoke of **Sakura Skip** will begin in 8 seconds\nMeguri megutte eburiwan\nNanika ga matteiru nokana");
                await Task.Delay(1000);
                await x.ModifyAsync("Karaoke of **Sakura Skip** will begin in 7 seconds\nMeguri megutte eburiwan\nNanika ga matteiru nokana");
                await Task.Delay(1000);
                await x.ModifyAsync("Karaoke of **Sakura Skip** will begin in 6 seconds\nMeguri megutte eburiwan\nNanika ga matteiru nokana");
                await Task.Delay(1000);
                await x.ModifyAsync("Karaoke of **Sakura Skip** will begin in 5 seconds\nMeguri megutte eburiwan\nNanika ga matteiru nokana");
                await Task.Delay(1000);
                await x.ModifyAsync("Karaoke of **Sakura Skip** will begin in 4 seconds\nMeguri megutte eburiwan\nNanika ga matteiru nokana");
                await Task.Delay(1000);
                await x.ModifyAsync("Karaoke of **Sakura Skip** will begin in 3 seconds\nMeguri megutte eburiwan\nNanika ga matteiru nokana");
                await Task.Delay(1000);
                await x.ModifyAsync("Karaoke of **Sakura Skip** will begin in 2 seconds\nMeguri megutte eburiwan\nNanika ga matteiru nokana");
                await Task.Delay(1000);
                await x.ModifyAsync("Karaoke of **Sakura Skip** will begin in 1 seconds\nMeguri megutte eburiwan\nNanika ga matteiru nokana");
                await Task.Delay(1000);
                await x.ModifyAsync("Karaoke of **Sakura Skip** will begin in 0 seconds\nMeguri megutte eburiwan\nNanika ga matteiru nokana");
                await Task.Delay(1000);
                await x.ModifyAsync("**Me**guri megutte eburiwan\nNanika ga matteiru nokana");

            }
        }

    }
}
