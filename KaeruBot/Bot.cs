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

namespace KaeruBot
{
    public class Bot
    {
        public static DiscordClient discord;
        public static CommandsNextModule commands;
        static InteractivityModule interactivity;

        public static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static async Task MainAsync(string[] args)
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = "",
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower().Equals("lemme smash"))
                    await e.Message.RespondAsync("no ron");
            };

            discord.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = TimeoutBehaviour.Ignore,
                PaginationTimeout = TimeSpan.FromMinutes(5),
                Timeout = TimeSpan.FromMinutes(2)
            });

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "|",
                EnableDms = true,
                EnableMentionPrefix = true
            });

            var vcfg = new VoiceNextConfiguration
            {
                VoiceApplication = DSharpPlus.VoiceNext.Codec.VoiceApplication.Music
            };

            var game = new DiscordGame()
            {
                Name = "Type |help for help!"
            };

            commands.RegisterCommands<Commands>();
            commands.RegisterCommands<ServerCommands>();
            commands.RegisterCommands<GBFCommands>();

            await discord.ConnectAsync();

            discord.Ready += async e =>
            {
                await discord.UpdateStatusAsync(game);
            };

            await Task.Delay(-1);
        }

    }
}
