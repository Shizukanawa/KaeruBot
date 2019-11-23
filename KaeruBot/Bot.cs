using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;
using Newtonsoft.Json;

namespace Shizukanawa.KaeruBot
{
    public class Bot
    {
        public static bool ReadyChecker = false;
        public static DiscordClient discord;
        public static CommandsNextExtension commands;
        //DiscordShardedClient shard;

        public static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static async Task MainAsync(string[] args)
        {
            var filePath = "./Data/Token.json";
            var jsonData = System.IO.File.ReadAllText(filePath);
            Token _Token = JsonConvert.DeserializeObject<Token>(jsonData);

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _Token.token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Info
            });

            discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower().Equals("lemme smash"))
                    await e.Message.RespondAsync("no ron");
            };

            discord.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = DSharpPlus.Interactivity.Enums.PaginationBehaviour.Default,
                Timeout = TimeSpan.FromMinutes(2)
            });

            List<string> prefixes = new List<string>() { "|" };

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = prefixes,
                EnableDms = true,
                EnableMentionPrefix = true
            });

            var vcfg = new VoiceNextConfiguration
            {
                AudioFormat = AudioFormat.Default,
            };

            var game = new DiscordActivity()
            {
                Name = "Type |help for help!",
                ActivityType = ActivityType.Playing,
            };

            commands.RegisterCommands<Commands>();
            commands.RegisterCommands<ServerCommands>();
            commands.RegisterCommands<GBFCommands>();
            commands.RegisterCommands<LeagueCommands>();

            await discord.ConnectAsync();

            discord.Ready += async e =>
            {
                await discord.UpdateStatusAsync(game);
                DisplayGuildCount();
                ReadyChecker = true;
            };

#pragma warning disable CS1998
            discord.Heartbeated += async e =>
            {
                if (ReadyChecker == true)
                {
                    DisplayGuildCount();
                }
            };
#pragma warning restore CS1998

            await Task.Delay(-1);
        }

        private static void DisplayGuildCount()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[Server count: {discord.Guilds.Count}]");
        }
    }
    public class Token
    {
        public string token { get; set; }
    }
}
