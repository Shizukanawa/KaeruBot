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
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Serialization;

namespace KaeruBot
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
            int GifCount = 5;
            int result = RNG.Next(0, GifCount);

            string[] Gif = PatGifs();

            var embed = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = usr.AvatarUrl,
                    Name = $"{usr.Username}, You got a pat from {ctx.User.Username}!"
                },
                Color = new DiscordColor("#FF6699"),
                ImageUrl = Gif[result]
            };
            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("holdhand"), Description("Lewd...\n**Usage:** |holdhand @user"), Aliases("handholding", "holdinghands", "handhold", "handholds", "holdhands")]
        public async Task HoldHand(CommandContext ctx, [RemainingTextAttribute] DiscordUser usr)
        {
            int GifCount = 9;
            int result = RNG.Next(0, GifCount);

            string[] Gif = HandHoldsGifs();

            var embed = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = usr.AvatarUrl,
                    Name = $"{ctx.User.Username} held hands with {usr.Username}... Lewd...",
                },
                Color = new DiscordColor("#FF6699"),
                ImageUrl = Gif[result]
            };
            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("kiss"), Description("Oh my...\n**Usage:** |kiss @user")]
        public async Task Kiss(CommandContext ctx, [RemainingTextAttribute] DiscordUser usr)
        {
            int GifCount = 3;
            int result = RNG.Next(0, GifCount);

            string[] Gif = KissGifs();

            var embed = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = usr.AvatarUrl,
                    Name = $"{ctx.User.Username} kissed {usr.Username}... Aww... <3",
                },
                Color = new DiscordColor("#FF6699"),
                ImageUrl = Gif[result]
            };
            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("hug"), Description("Cute!\n**Usage:** |hug @user")]
        public async Task Hug(CommandContext ctx, [RemainingTextAttribute] DiscordUser usr)
        {
            int GifCount = 3;
            int result = RNG.Next(0, GifCount);

            string[] Gif = HugGifs();

            var embed = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = usr.AvatarUrl,
                    Name = $"{ctx.User.Username} hugged {usr.Username}... OwO...",
                },
                Color = new DiscordColor("#FF6699"),
                ImageUrl = Gif[result]
            };
            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("setgame"), Description("Sets the game of the bot. Put quotation marks.")]
        public async Task SetGame(CommandContext ctx, [RemainingTextAttribute] string game)
        {
            var Game = new DiscordGame()
            {
                Name = $"{game}"
            };
            await ctx.Client.UpdateStatusAsync(Game);

            await ctx.RespondAsync($"Game is now: **{game}**");
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
            return new string[]
            {
                "https://memestatic.fjcdn.com/gifs/Cute+kanna+gif+comp+large+mentionlist+thelunchtablehttpsfunnyjunkcomchannelanimemangacute+kanna+comp+2rxorlekhttpsfunnyjunkcomchannelanimemangakanna+comp+3ydsrllqhttpsfunnyjunkcomchannelanimemangakanna+comp+4kexslxrhttpsfunnyjunkcomchannelanimemangacute+kanna+compcporlpc_5f293f_6424003.gif",
                "https://giant.gfycat.com/BlankGiftedBurro.gif",
                "https://vignette.wikia.nocookie.net/cardfight/images/b/ba/Izumi_Reina_pat.gif",
                "https://archive-media-0.nyafuu.org/c/image/1483/55/1483553008493.gif",
                "https://i.pinimg.com/originals/9b/bd/d3/9bbdd3c7884308f36df49d3a3b2eb6f7.gif"
            };
        }

        string[] HandHoldsGifs()
        {
            return new string[]
            {
                "http://i2.kym-cdn.com/photos/images/newsfeed/000/932/422/4ad.gif",
                "http://i0.kym-cdn.com/entries/icons/original/000/021/658/hand_holding.gif",
                "http://i3.kym-cdn.com/photos/images/newsfeed/000/932/366/0a7.gif",
                "http://i0.kym-cdn.com/photos/images/newsfeed/000/932/360/509.gif",
                "https://i.imgur.com/G8u9fvk.gif",
                "https://i.imgur.com/KumfkXF.gif",
                "https://i.imgur.com/zpVKJcc.gif",
                "https://pa1.narvii.com/6193/7b9e5989093734cd764c54cb42ec976c7a370c6e_hq.gif",
                "https://i.imgur.com/qgGOQng.gif"
            };
        }
        
        string[] KissGifs()
        {
            return new string[]
            {
                "https://i.imgur.com/sGVgr74.gif",
                "https://media.giphy.com/media/lWnaY7WUymoxO/giphy.gif",
                "https://i.pinimg.com/originals/6e/2f/e9/6e2fe9073f4e6aa4080e2e9ab5e3f790.gif"
            };
        }

        string[] HugGifs()
        {
            return new string[]
            {
                "https://thumbs.gfycat.com/WellgroomedVapidKitten-max-1mb.gif",
                "https://cdn140.picsart.com/247171032013202.gif?r240x240",
                "https://78.media.tumblr.com/a4119e7feb02c0094a6628e6b7cf3924/tumblr_mvqfcgI5eQ1s2p1gco1_500.gif"
            };
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
