using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.IO;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Shizukanawa.KaeruBot
{
    class Commands
    {
        private static readonly Random RNG = new Random();

        [Command("hi"), Description("Says hi")]
        public async Task Hi(CommandContext ctx)
        {
            await ctx.RespondAsync($"👋 Hi, {ctx.User.Mention}!");
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id && xm.Content.ToLower() == "how are you?", TimeSpan.FromMinutes(1));
            if (msg != null)
                await ctx.RespondAsync($"I'm fine, thank you!");
        }

        [Command("random"), Description("Picks a random number from a given intervaln\n**Usage:** |random (min number) (max number)")]
        public async Task Random(CommandContext ctx, int min, int max)
        {
            var rnd = new Random();
            await ctx.RespondAsync($"Your number is: {rnd.Next(min, max + 1)}");
        }

        [Command("math"), Description("Does simple math")]
        public async Task Math(CommandContext ctx, [Description("Takes the first number")] double FirstNumber, [Description("Takes the operator")]string Operator, [Description("Takes the second number")]double SecondNumber)
        {
            double result;
            if (Operator == "+")
            {
                result = FirstNumber + SecondNumber;
                await ctx.RespondAsync($"The result is: {result.ToString()}");
            }
            if (Operator == "-")
            {
                result = FirstNumber - SecondNumber;
                await ctx.RespondAsync($"The result is: {result.ToString()}");
            }
            if (Operator == "*")
            {
                result = FirstNumber * SecondNumber;
                await ctx.RespondAsync($"The result is: {result.ToString()}");
            }
            if (Operator == "/")
            {
                result = FirstNumber / SecondNumber;
                await ctx.RespondAsync($"The result is: {result.ToString()}");
            }
        }

        [Command("ping"), Description("Returns pong")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");

            DiscordMessage x = await ctx.RespondAsync($"{emoji} Pong! Ping ");
            await x.ModifyAsync($"{emoji} Pong! Ping {ctx.Client.Ping}ms");
        }

        [Command("slot"), Description("A slot machine!"), Aliases("slotmachine")]
        public async Task Slotmachine(CommandContext ctx)
        {
            int[] Values = SlotMachine();

            if (Values[3] == 0)
            {
                await ctx.RespondAsync($"You rolled {Values[0]}, {Values[1]} and {Values[2]}. You lose!");
            }
            if (Values[3] == 1)
            {
                await ctx.RespondAsync($"You rolled {Values[0]}, {Values[1]} and {Values[2]}. You win!");
            }
        }

        [Command("user"), Description("Displays information about a user.\n**Usage:** |user @user")]
        public async Task User(CommandContext ctx, DiscordUser usr)
        {
            var embed = new DiscordEmbedBuilder()
            {
                Title = $"Profile for: {usr.Username}#{usr.Discriminator}",
                Description = $"Username: {usr.Username} \nStatus: {usr.Presence.Status}\nAvatar: {usr.AvatarUrl}",
                ThumbnailUrl = usr.AvatarUrl,
                Color = new DiscordColor("#FFFFFF"),
                Timestamp = DateTime.UtcNow,
            };

            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("pat"), Description("Pats a user.\n**Usage:** |pat @user")]
        public async Task Pat(CommandContext ctx, [RemainingTextAttribute] DiscordUser usr)
        {
            if (usr.Id == ctx.User.Id)
            {
                await ctx.RespondAsync("You can't really pat yourself... *pats*");
            }
            else
            {
                string[] _patGifs = PatGifs();
                int _result = RNG.Next(0, _patGifs.Length);

                var embed = new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = usr.AvatarUrl,
                        Name = $"{usr.Username}, You got a pat from {ctx.User.Username}!"
                    },
                    Color = new DiscordColor("#FF6699"),
                    ImageUrl = _patGifs[_result]
                };
                await ctx.RespondAsync(embed: embed.Build());
            }
        }

        [Command("holdhand"), Description("Lewd...\n**Usage:** |holdhand @user"), Aliases("handholding", "holdinghands", "handhold", "handholds", "holdhands")]
        public async Task HoldHand(CommandContext ctx, [RemainingTextAttribute] DiscordUser usr)
        {
            if (usr.Id == ctx.User.Id)
            {
                await ctx.RespondAsync("You can't do lewd things to yourself.");
            }
            else
            {
                string[] _handholdGifs = HandHoldsGifs();
                int _result = RNG.Next(0, _handholdGifs.Length);

                var embed = new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = usr.AvatarUrl,
                        Name = $"{ctx.User.Username} held hands with {usr.Username}... Lewd...",
                    },
                    Color = new DiscordColor("#FF6699"),
                    ImageUrl = _handholdGifs[_result]
                };
                await ctx.RespondAsync(embed: embed.Build());
            }
        }

        [Command("kiss"), Description("Oh my...\n**Usage:** |kiss @user")]
        public async Task Kiss(CommandContext ctx, [RemainingTextAttribute] DiscordUser usr)
        {
            if (usr.Id == ctx.User.Id)
            {
                await ctx.RespondAsync("How is that even possible?...");
            }
            else
            {
                string[] _kissGifs = KissGifs();
                int _result = RNG.Next(0, _kissGifs.Length);

                var embed = new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = usr.AvatarUrl,
                        Name = $"{ctx.User.Username} kissed {usr.Username}... Aww... <3",
                    },
                    Color = new DiscordColor("#FF6699"),
                    ImageUrl = _kissGifs[_result]
                };
                await ctx.RespondAsync(embed: embed.Build());
            }
        }

        [Command("hug"), Description("Cute!\n**Usage:** |hug @user")]
        public async Task Hug(CommandContext ctx, [RemainingTextAttribute] DiscordUser usr)
        {
            if (usr.Id == ctx.User.Id)
            {
                await ctx.RespondAsync("That's going to be a little bit hard alone... *hugs*");
            }
            else
            {
                string[] _hugGifs = HugGifs();
                int _result = RNG.Next(0, _hugGifs.Length);

                var embed = new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = usr.AvatarUrl,
                        Name = $"{ctx.User.Username} hugged {usr.Username}... OwO...",
                    },
                    Color = new DiscordColor("#FF6699"),
                    ImageUrl = _hugGifs[_result]
                };
                await ctx.RespondAsync(embed: embed.Build());
            }
        }

        [Command("setgame"), Description("Sets the game of the bot. Put quotation marks.")]
        public async Task SetGame(CommandContext ctx, [RemainingTextAttribute] string game)
        {
            if (ctx.User.Id != ctx.Client.CurrentApplication.Owner.Id)
            {
                await ctx.RespondAsync("Only the bot owner can do this!");
                return;
            }
            else
            {
                var Game = new DiscordGame()
                {
                    Name = $"{game}"
                };

                await ctx.Client.UpdateStatusAsync(Game);

                await ctx.RespondAsync($"Game is now: **{game}**");
            }
        }

        [Command("gcd"), Description("Finds the greatest common divisor between 2 numbers\n**Usage:** |GCD (number 1) (number 2)")]
        public async Task GCD(CommandContext ctx, int number1, int number2)
        {
            int _smallest;
            int _largest;
            int _remainder;

            //Using Euclidean Algorithm

            //Assigns smallest of a or b to _smallest and the largest to _largest.
            _smallest = number1 <= number2 ? number1 : number2;
            _largest = number1 <= number2 ? number2 : number1;

            //Loops until _smallest is 0
            while (_smallest > 0)
            {
                _remainder = _largest & _smallest;
                _largest = _smallest;
                _smallest = _remainder;
            }

            await ctx.RespondAsync($"The greatest common divisor is: **{_largest}**");
        }

        [Command("choose"), Description("Randomly chooses between up to 4 options\n**Usage:** |choose (choice 1) (choice 2) (choice 3) (choice 4)")]
        public async Task Choose(CommandContext ctx, string option1 = null, string option2 = null, string option3 = null, string option4 = null)
        {
            int result;

            if (option1 == null && option2 == null && option3 == null && option4 == null)
            {
                await ctx.RespondAsync("Please give me 2 or more options");
            }

            if (option1 != null && option2 == null && option3 == null && option4 == null)
            {
                await ctx.RespondAsync("Please give me one more option");
            }

            if (option1 != null && option2 != null && option3 == null && option4 == null)
            {
                result = RNG.Next(1, 3);

                if (result == 1)
                {
                    await ctx.RespondAsync($"I choose: {option1}");
                }
                if (result == 2)
                {
                    await ctx.RespondAsync($"I choose: {option2}");
                }
            }
            if (option1 != null && option2 != null && option3 != null && option4 == null)
            {
                result = RNG.Next(1, 4);

                if (result == 1)
                {
                    await ctx.RespondAsync($"I choose: {option1}");
                }
                if (result == 2)
                {
                    await ctx.RespondAsync($"I choose: {option2}");
                }
                if (result == 3)
                {
                    await ctx.RespondAsync($"I choose: {option3}");
                }
            }
            if (option1 != null && option2 != null && option3 != null && option4 != null)
            {
                result = RNG.Next(1, 5);

                if (result == 1)
                {
                    await ctx.RespondAsync($"I choose: {option1}");
                }
                if (result == 2)
                {
                    await ctx.RespondAsync($"I choose: {option2}");
                }
                if (result == 3)
                {
                    await ctx.RespondAsync($"I choose: {option3}");
                }
                if (result == 4)
                {
                    await ctx.RespondAsync($"I choose: {option4}");
                }
            }
        }

        string[] PatGifs()
        {
            //Sets the filepath
            string _filepath = "./Data/PatGifs.json";

            //Reads from the json file inside the filepath
            var _jsonData = System.IO.File.ReadAllText(_filepath);

            //Makes a new JArray that has been parsed from the file
            JArray _JpatGifs = JArray.Parse(_jsonData);

            //Converts the JArray into a regular array so we can return it
            string[] _patGifs = _JpatGifs.ToObject<string[]>();
            return _patGifs;
        }

        string[] HandHoldsGifs()
        {
            string _filepath = "./Data/HandHoldGifs.json";
            var _jsondata = System.IO.File.ReadAllText(_filepath);
            JArray _JhandholdGifs = JArray.Parse(_jsondata);
            string[] _handholdGifs = _JhandholdGifs.ToObject<string[]>();
            return _handholdGifs;
        }
        
        string[] KissGifs()
        {
            string _filepath = "./Data/KissGifs.json";
            var _jsondata = System.IO.File.ReadAllText(_filepath);
            JArray _JkissGifs = JArray.Parse(_jsondata);
            string[] _kissGifs = _JkissGifs.ToObject<string[]>();
            return _kissGifs;
        }

        string[] HugGifs()
        {
            string _filepath = "./Data/HugGifs.json";
            var _jsondata = System.IO.File.ReadAllText(_filepath);
            JArray _JhugGifs = JArray.Parse(_jsondata);
            string[] _hugGifs = _JhugGifs.ToObject<string[]>();
            return _hugGifs;
        }

        int[] SlotMachine()
        {
            int RNG1 = RNG.Next(1, 6);
            int RNG2 = RNG.Next(1, 6);
            int RNG3 = RNG.Next(1, 6);
            int result = 0;

            if (RNG1 == 1 && RNG2 == 1 && RNG3 == 1)
            {
                result = 1;
            }
            if (RNG1 == 2 && RNG2 == 2 && RNG3 == 2)
            {
                result = 1;
            }
            if (RNG1 == 3 && RNG2 == 3 && RNG3 == 3)
            {
                result = 1;
            }
            if (RNG1 == 4 && RNG2 == 4 && RNG3 == 4)
            {
                result = 1;
            }
            if (RNG1 == 5 && RNG2 == 5 && RNG3 == 5)
            {
                result = 1;
            }
            return new int[] { RNG1,RNG2,RNG3,result};
        }
    }
}
