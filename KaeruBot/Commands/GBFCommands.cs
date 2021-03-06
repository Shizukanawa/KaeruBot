﻿using System;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Serialization;

namespace Shizukanawa.KaeruBot
{
    class GBFCommands : BaseCommandModule
    {
        private static readonly Random RNG = new Random();
        string mode = "normal";
        string include = string.Empty;

        [Command("spark"), Description("Tracks the spark count in GBF\n**Usage:** |spark (crystal amount) (ticket amount)\n|spark will check your crystals,tickets and draws!")]
        public async Task Spark(CommandContext ctx, string crystals = null, string tickets = null)
        {
            int Crystals;
            int Tickets;
            string SName;
            string SCrystals;
            string STickets;
            int resultCrystals;
            int result;


            var filePath = "./Data/Spark.xml";
            var xmlDoc = XElement.Load(filePath);

            //Takes the User ID for the Discord user and puts it into the string Player
            string Player = $"{ctx.User.Id}";

            //Makes a new variable named player and uses LINQ to find a specific branch
            var player = xmlDoc.Elements()
                .Where(a => (string)a.Attribute("id") == Player)
                .FirstOrDefault();

            if (crystals == null && tickets == null)
            {
                //Get information from the xml file
                SName = player.Element("Name").Value;
                SCrystals = player.Element("Crystals").Value;
                STickets = player.Element("Tickets").Value;

                Crystals = int.Parse(SCrystals);
                Tickets = int.Parse(STickets);

                //Calculate how many draws you have
                resultCrystals = Crystals / 300;
                result = resultCrystals + Tickets;
                await ctx.RespondAsync($"**{SName}** has **{SCrystals} Crystals** and **{STickets} Tickets** \n" +
                    $"Which means you have: **{result} draws**");
            }

            else if (!crystals.All(char.IsDigit) || !tickets.All(char.IsDigit))
                await ctx.RespondAsync("You can't do that!");

            else if (int.Parse(crystals) < 0 || int.Parse(tickets) < 0)
            {
                await ctx.RespondAsync("I'm sorry but you can't do that.");
                return;
            }

            //If the player does exist, update the information
            else if (player != null && crystals != null && tickets != null)
            {
                //Sets the values to what the user put from the chatbox
                player.Element("Name").Value = ctx.User.Username.ToString();
                player.Element("Crystals").Value = crystals.ToString();
                player.Element("Tickets").Value = tickets.ToString();
                xmlDoc.Save(filePath);

                //Calculates and later displays the result
                resultCrystals = int.Parse(crystals) / 300;
                Tickets = int.Parse(tickets);
                result = resultCrystals + Tickets;

                await ctx.RespondAsync($"Updated for **{ctx.User.Username}** with **{crystals} Crystals** and **{tickets} Tickets**" +
                    $"\nYou have **{result} draws**");
            }

            //If the player doesn't exist, create a new one
            else if (player == null && crystals != null && tickets != null)
            {
                xmlDoc.Add(new XElement("player",
                new XAttribute("id", Player),
                new XElement("Name", ctx.User.Username),
                new XElement("Crystals", crystals),
                new XElement("Tickets", tickets)));

                //Saves the document
                xmlDoc.Save(filePath);

                //Gets the information from the newly created xml document and calculates the crystal amount.
                resultCrystals = int.Parse(crystals) / 300;
                Tickets = int.Parse(tickets);
                result = resultCrystals + Tickets;

                await ctx.RespondAsync($"Added for **{ctx.User.Username}** with **{crystals} Crystals** and **{tickets} Tickets**" +
                    $"\nYou have **{result} draws**");
                return;
            }

            else if (player == null)
            {
                await ctx.RespondAsync("You have not tracked your spark yet.\nDo **|spark (crystal amount) (ticket amount)** to add yourself");
            }
        }

        [Command("roulette"), Description("'Spins a roulette from GBF\n**Usage:** |roulette'")]
        public async Task Roulette(CommandContext ctx)
        {
            double rngresult;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":tada:");

            rngresult = RNG.Next(0, 101);

            if (rngresult <= 40)
            {
                await ctx.RespondAsync("You got **10 rolls**");
            }
            else if (rngresult > 40 && rngresult <= 80)
            {
                await ctx.RespondAsync("You got **20 rolls**");
            }
            else if (rngresult > 80 && rngresult <= 98)
            {
                await ctx.RespondAsync("You got **30 rolls**");
            }
            else if (rngresult > 98)
            {
                await ctx.RespondAsync($"{emoji} You got **100 rolls** {emoji}");
            }

        }

        [Command ("include"), Description ("Can include a limited pool from GBF\n**Usage:** |include (draw pool)")]
        public async Task Include(CommandContext ctx, [RemainingTextAttribute] string including = null)
        {
            if (including == null || including.ToLower() == " ")
            {
                await ctx.RespondAsync("Write either **Summer**, **Halloween**, **Winter**, **Zodiac**, **Valentine** or **None**.");
            }
            else if (including.ToLower() == "summer")
            {
                include = "Summer";
                await ctx.RespondAsync($"The pool now includes **{include}**");
            }
            else if (including.ToLower() == "halloween")
            {
                include = "Halloween";
                await ctx.RespondAsync($"The pool now includes **{include}**");
            }
            else if (including.ToLower() == "winter" || including.ToLower() == "holiday")
            {
                include = "Winter";
                await ctx.RespondAsync($"The pool now includes **{include}**");
            }
            else if (including.ToLower() == "zodiac")
            {
                include = "Zodiac";
                await ctx.RespondAsync($"The pool now includes **{include}**");
            }
            else if (including.ToLower() == "valentine")
            {
                include = "Valentine";
                await ctx.RespondAsync($"The pool now includes **{include}**");
            }
            else if (including.ToLower() == "none")
            {
                include = string.Empty;
                await ctx.RespondAsync("There is no longer a limited pool");
            }
        }

        [Command("draw"), Description("Draws from GBF pool.\n**Usage:** |draw 1 or 10 to draw\nAlternatively you can do |draw normal, flash or premium to change the pool and rates.")]
        public async Task Draw(CommandContext ctx, string drawamount = null)
        {
            /*
            Non-Limited: 93
            Premium: 6
            Flash: 9

            Summer: 15
            Halloween: 4
            Winter: 4
            Zodiac: 4
            */

            /* Normal rates:
            3% SSR = result <= 3
            15% SR = result > 3 && result <= 18
            82% R = result > 18 && result <= 100

            Flash- Premium Gala
            6% SSR = result <= 6
            15% SR = result > 6 && result <= 21
            79% R = result > 21 && result <= 100
            */

            if (drawamount == null)
            {
                if (mode == "normal")
                {
                    await ctx.RespondAsync($"Current draw is: **{mode}**!");
                }

                if (mode != "normal")
                {
                    await ctx.RespondAsync($"Current draw is: **{mode} gala**!");
                }
            }

            if (drawamount.ToLower() == "normal")
            {
                mode = "normal";
                await ctx.RespondAsync($"Set draw to **{mode}**!");
            }

            if (drawamount.ToLower() == "premium")
            {
                mode = "premium";
                await ctx.RespondAsync($"Set draw to **{mode} gala**!");
            }

            if (drawamount.ToLower() == "flash")
            {
                mode = "flash";
                await ctx.RespondAsync($"Set draw to **{mode} gala**!");
            }

            int result;
            int SRSSR;

            string[] NormalCharacters = NormalCharacter();
            string[] GrandCharacters = GrandCharacter();
            string[] FlashCharacters = FlashCharacter();
            string[] SummerCharacters = SummerCharacter();
            string[] HalloweenCharacters = HalloweenCharacter();
            string[] WinterCharacters = WinterCharacter();
            string[] ZodiacCharacters = ZodiacCharacter();
            string[] ValentineCharacters = ValentineCharacter();

            int CountNormal = NormalCharacters.Length;
            int CountPremium = GrandCharacters.Length;
            int CountFlash = FlashCharacters.Length;
            int CountSummer = SummerCharacters.Length;
            int CountHalloween = HalloweenCharacters.Length;
            int CountWinter = WinterCharacters.Length;
            int CountZodiac = ZodiacCharacters.Length;
            int CountValentine = ValentineCharacters.Length;

            int draw_amount = int.Parse(drawamount);

            string[] results = new string[10];

            if (mode == "normal")
            {
                for (int i = 0; i < draw_amount; i++)
                {
                    result = RNG.Next(1, 101);

                    if (result <= 3)
                    {
                        int i2 = RNG.Next(0, CountNormal);

                        results[i] = "**SSR " + NormalCharacters[i2] + "**";

                        //If include isn't empty
                        if (include == "Summer")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountSummer));
                            if (result1 > CountNormal)
                            {
                                int i3 = RNG.Next(0, CountSummer);
                                results[i] = "**SSR " + SummerCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Halloween")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountHalloween));
                            if (result1 > CountNormal)
                            {
                                int i3 = RNG.Next(0, CountHalloween);
                                results[i] = "**SSR " + HalloweenCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Winter")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountWinter));
                            if (result1 > CountNormal)
                            {
                                int i3 = RNG.Next(0, CountWinter);
                                results[i] = "**SSR " + WinterCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Zodiac")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountZodiac));
                            if (result1 > CountNormal)
                            {
                                int i3 = RNG.Next(0, CountZodiac);
                                results[i] = "**SSR " + ZodiacCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Valentine")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountValentine));
                            if (result1 > CountNormal)
                            {
                                int i3 = RNG.Next(0, CountValentine);
                                results[i] = "**SSR " + ValentineCharacters[i3] + "**";
                            }
                        }
                    }
                    if (result > 3 && result <= 18)
                    {
                        results[i] = "**SR**";
                    }
                    if (result > 18 && result <= 100)
                    {
                        results[i] = "R";
                    }
                }
            }

            if (mode == "premium")
            {
                for (int i = 0; i < draw_amount; i++)
                {
                    result = RNG.Next(1, 101);

                    if (result <= 6)
                    {
                        int i2 = RNG.Next(0, (CountNormal + CountPremium));

                        if (i2 <= CountNormal)
                        {
                            results[i] = "**SSR " + NormalCharacters[i2] + "**";
                        }
                        else
                        {
                            int i3 = RNG.Next(0, CountPremium);
                            results[i] = "**SSR " + GrandCharacters[i3] + "**";
                        }

                        if (include == "Summer")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountPremium + CountSummer));
                            if (result1 > (CountNormal + CountPremium))
                            {
                                int i3 = RNG.Next(0, CountSummer);
                                results[i] = "**SSR " + SummerCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Halloween")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountPremium + CountSummer));
                            if (result1 > (CountNormal + CountPremium))
                            {
                                int i3 = RNG.Next(0, CountHalloween);
                                results[i] = "**SSR " + HalloweenCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Winter")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountPremium + CountSummer));
                            if (result1 > (CountNormal + CountPremium))
                            {
                                int i3 = RNG.Next(0, CountWinter);
                                results[i] = "**SSR " + WinterCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Zodiac")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountPremium + CountSummer));
                            if (result1 > (CountNormal + CountPremium))
                            {
                                int i3 = RNG.Next(0, CountZodiac);
                                results[i] = "**SSR " + ZodiacCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Valentine")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountPremium + CountSummer));
                            if (result1 > (CountNormal + CountPremium))
                            {
                                int i3 = RNG.Next(0, CountValentine);
                                results[i] = "**SSR " + ValentineCharacters[i3] + "**";
                            }
                        }
                    }
                    if (result > 6 && result <= 21)
                    {
                        results[i] = "**SR**";
                    }
                    if (result > 21 && result <= 100)
                    {
                        results[i] = "R";
                    }
                }
            }

            if (mode == "flash")
            {
                for (int i = 0; i < draw_amount; i++)
                {
                    result = RNG.Next(1, 101);

                    if (result <= 6)
                    {
                        int i2 = RNG.Next(0, (CountNormal + CountFlash));

                        if (i2 <= CountNormal)
                        {
                            results[i] = "**SSR " + NormalCharacters[i2] + "**";
                        }
                        else
                        {
                            int i3 = RNG.Next(0, CountFlash);
                            results[i] = "**SSR " + FlashCharacters[i3] + "**";
                        }

                        if (include == "Summer")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountFlash + CountSummer));
                            if (result1 > (CountNormal + CountFlash))
                            {
                                int i3 = RNG.Next(0, CountSummer);
                                results[i] = "**SSR " + SummerCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Halloween")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountFlash + CountSummer));
                            if (result1 > (CountNormal + CountFlash))
                            {
                                int i3 = RNG.Next(0, CountHalloween);
                                results[i] = "**SSR " + HalloweenCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Winter")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountFlash + CountSummer));
                            if (result1 > (CountNormal + CountFlash))
                            {
                                int i3 = RNG.Next(0, CountWinter);
                                results[i] = "**SSR " + WinterCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Zodiac")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountFlash + CountSummer));
                            if (result1 > (CountNormal + CountFlash))
                            {
                                int i3 = RNG.Next(0, CountZodiac);
                                results[i] = "**SSR " + ZodiacCharacters[i3] + "**";
                            }
                        }
                        else if (include == "Valentine")
                        {
                            int result1 = RNG.Next(1, (CountNormal + CountFlash + CountSummer));
                            if (result1 > (CountNormal + CountFlash))
                            {
                                int i3 = RNG.Next(0, CountValentine);
                                results[i] = "**SSR " + ValentineCharacters[i3] + "**";
                            }
                        }
                    }

                    if (result > 6 && result <= 21)
                    {
                        results[i] = "**SR**";
                    }
                    if (result > 21 && result <= 100)
                    {
                        results[i] = "R";
                    }
                }
            }

            if (draw_amount == 1)
            {
                await ctx.RespondAsync($"You have drawn: {results[0]}");
            }

            if (draw_amount == 10)
            {
                //If everything is R, this will make the 10th be SR or higher.
                if (results[0] == "R" && results[1] == "R" && results[2] == "R" && results[3] == "R" && results[4] == "R" && results[5] == "R" && results[6] == "R" && results[7] == "R" && results[8] == "R" && results[9] == "R")
                {
                    SRSSR = RNG.Next(1, 101);

                    /*
                    Normal:
                    SSR = 3%
                    SR = 97%

                    Premium:
                    SSR = 6%
                    SR = 94%
                     */
                    if (mode == "normal")
                    {
                        if (SRSSR <= 3)
                        {
                            if (mode == "normal")
                            {
                                int i2 = RNG.Next(0, CountNormal);
                                results[9] = "**SSR " + NormalCharacters[i2] + "**";
                            }

                            if (include == "Summer")
                            {
                                int result1 = RNG.Next(1, (CountNormal + CountSummer));
                                if (result1 > CountNormal)
                                {
                                    int i3 = RNG.Next(0, CountSummer);
                                    results[9] = "**SSR " + SummerCharacters[i3] + "**";
                                }
                            }
                            else if (include == "Halloween")
                            {
                                int result1 = RNG.Next(1, (CountNormal + CountHalloween));
                                if (result1 > CountNormal)
                                {
                                    int i3 = RNG.Next(0, CountHalloween);
                                    results[9] = "**SSR " + HalloweenCharacters[i3] + "**";
                                }
                            }
                            else if (include == "Winter")
                            {
                                int result1 = RNG.Next(1, (CountNormal + CountWinter));
                                if (result1 > CountNormal)
                                {
                                    int i3 = RNG.Next(0, CountWinter);
                                    results[9] = "**SSR " + WinterCharacters[i3] + "**";
                                }
                            }
                            else if (include == "Zodiac")
                            {
                                int result1 = RNG.Next(1, (CountNormal + CountZodiac));
                                if (result1 > CountNormal)
                                {
                                    int i3 = RNG.Next(0, CountZodiac);
                                    results[9] = "**SSR " + ZodiacCharacters[i3] + "**";
                                }
                            }
                            else if (include == "Valentine")
                            {
                                int result1 = RNG.Next(1, (CountNormal + CountValentine));
                                if (result1 > CountNormal)
                                {
                                    int i3 = RNG.Next(0, CountValentine);
                                    results[9] = "**SSR " + ValentineCharacters[i3] + "**";
                                }
                            }
                        }
                        else
                        {
                            results[9] = "**SR**";
                        }
                    }

                    if (mode != "normal")
                    {
                        if (SRSSR <= 6)
                        {
                            if (mode == "premium")
                            {
                                int i2 = RNG.Next(0, (CountNormal + CountPremium));

                                if (i2 <= CountNormal)
                                {
                                    results[9] = "**SSR " + NormalCharacters[i2] + "**";
                                }
                                else
                                {
                                    int i3 = RNG.Next(0, CountPremium);
                                    results[9] = "**SSR " + GrandCharacters[i3] + "**";
                                }

                                if (include == "Summer")
                                {
                                    int result1 = RNG.Next(1, (CountNormal + CountPremium + CountSummer));
                                    if (result1 > (CountNormal + CountPremium))
                                    {
                                        int i3 = RNG.Next(0, CountSummer);
                                        results[9] = "**SSR " + SummerCharacters[i3] + "**";
                                    }
                                }
                                else if (include == "Halloween")
                                {
                                    int result1 = RNG.Next(1, (CountNormal + CountPremium + CountSummer));
                                    if (result1 > (CountNormal + CountPremium))
                                    {
                                        int i3 = RNG.Next(0, CountHalloween);
                                        results[9] = "**SSR " + HalloweenCharacters[i3] + "**";
                                    }
                                }
                                else if (include == "Winter")
                                {
                                    int result1 = RNG.Next(1, (CountNormal + CountPremium + CountSummer));
                                    if (result1 > (CountNormal + CountPremium))
                                    {
                                        int i3 = RNG.Next(0, CountWinter);
                                        results[9] = "**SSR " + WinterCharacters[i3] + "**";
                                    }
                                }
                                else if (include == "Zodiac")
                                {
                                    int result1 = RNG.Next(1, (CountNormal + CountPremium + CountSummer));
                                    if (result1 > (CountNormal + CountPremium))
                                    {
                                        int i3 = RNG.Next(0, CountZodiac);
                                        results[9] = "**SSR " + ZodiacCharacters[i3] + "**";
                                    }
                                }
                                else if (include == "Valentine")
                                {
                                    int result1 = RNG.Next(1, (CountNormal + CountPremium + CountSummer));
                                    if (result1 > (CountNormal + CountPremium))
                                    {
                                        int i3 = RNG.Next(0, CountValentine);
                                        results[9] = "**SSR " + ValentineCharacters[i3] + "**";
                                    }
                                }
                            }

                            if (mode == "flash")
                            {
                                int i2 = RNG.Next(0, (CountNormal + CountFlash));
                                if (i2 <= CountNormal)
                                {
                                    results[9] = "**SSR " + NormalCharacters[i2] + "**";
                                }
                                else
                                {
                                    int i3 = RNG.Next(0, CountFlash);
                                    results[9] = "**SSR " + FlashCharacters[i3] + "**";
                                }

                                if (include == "Summer")
                                {
                                    int result1 = RNG.Next(1, (CountNormal + CountFlash + CountSummer));
                                    if (result1 > (CountNormal + CountFlash))
                                    {
                                        int i3 = RNG.Next(0, CountSummer);
                                        results[9] = "**SSR " + SummerCharacters[i3] + "**";
                                    }
                                }
                                else if (include == "Halloween")
                                {
                                    int result1 = RNG.Next(1, (CountNormal + CountFlash + CountSummer));
                                    if (result1 > (CountNormal + CountFlash))
                                    {
                                        int i3 = RNG.Next(0, CountHalloween);
                                        results[9] = "**SSR " + HalloweenCharacters[i3] + "**";
                                    }
                                }
                                else if (include == "Winter")
                                {
                                    int result1 = RNG.Next(1, (CountNormal + CountFlash + CountSummer));
                                    if (result1 > (CountNormal + CountFlash))
                                    {
                                        int i3 = RNG.Next(0, CountWinter);
                                        results[9] = "**SSR " + WinterCharacters[i3] + "**";
                                    }
                                }
                                else if (include == "Zodiac")
                                {
                                    int result1 = RNG.Next(1, (CountNormal + CountFlash + CountSummer));
                                    if (result1 > (CountNormal + CountFlash))
                                    {
                                        int i3 = RNG.Next(0, CountZodiac);
                                        results[9] = "**SSR " + ZodiacCharacters[i3] + "**";
                                    }
                                }
                                else if (include == "Valentine")
                                {
                                    int result1 = RNG.Next(1, (CountNormal + CountFlash + CountSummer));
                                    if (result1 > (CountNormal + CountFlash))
                                    {
                                        int i3 = RNG.Next(0, CountValentine);
                                        results[9] = "**SSR " + ValentineCharacters[i3] + "**";
                                    }
                                }
                            }
                        }
                        else
                        {
                            results[9] = "**SR**";
                        }

                    }
                }

                await ctx.RespondAsync($"You have drawn:\n" +
                    $"{results[0]}, {results[1]}, {results[2]}, {results[3]}, {results[4]}, {results[5]}, {results[6]}, {results[7]}, {results[8]} and {results[9]}");
            }
        }

        /// <summary>
        /// List of normal characters in */Data/GranblueFantasy/NormalCharacters.json
        /// </summary>
        /// <returns></returns>
        string[] NormalCharacter()
        {
            string _filepath = "./Data/GranblueFantasy/NormalCharacters.json";
            var _jsonData = System.IO.File.ReadAllText(_filepath);
            JArray _JnormalCharacters = JArray.Parse(_jsonData);
            string[] _normalCharacters = _JnormalCharacters.ToObject<string[]>();
            return _normalCharacters;
        }

        /// <summary>
        /// List of grand characters in */Data/GranblueFantasy/GrandCharacters.json
        /// </summary>
        /// <returns></returns>
        string[] GrandCharacter()
        {
            string _filepath = "./Data/GranblueFantasy/GrandCharacters.json";
            var _jsonData = System.IO.File.ReadAllText(_filepath);
            JArray _JgrandCharacters = JArray.Parse(_jsonData);
            string[] _grandCharacters = _JgrandCharacters.ToObject<string[]>();
            return _grandCharacters;
        }

        /// <summary>
        /// List of flash characters in */Data/GranblueFantasy/FlashCharacters.json
        /// </summary>
        /// <returns></returns>
        string[] FlashCharacter()
        {
            string _filepath = "./Data/GranblueFantasy/FlashCharacters.json";
            var _jsonData = System.IO.File.ReadAllText(_filepath);
            JArray _JflashCharacters = JArray.Parse(_jsonData);
            string[] _flashCharacters = _JflashCharacters.ToObject<string[]>();
            return _flashCharacters;
        }

        /// <summary>
        /// List of summer characters in */Data/GranblueFantasy/SummerCharacters.json
        /// </summary>
        /// <returns></returns>
        string[] SummerCharacter()
        {
            string _filepath = "./Data/GranblueFantasy/SummerCharacters.json";
            var _jsonData = System.IO.File.ReadAllText(_filepath);
            JArray _JsummerCharacters = JArray.Parse(_jsonData);
            string[] _summerCharacters = _JsummerCharacters.ToObject<string[]>();
            return _summerCharacters;
        }

        /// <summary>
        /// List of halloween characters in */Data/GranblueFantasy/HalloweenCharacters.json
        /// </summary>
        /// <returns></returns>
        string[] HalloweenCharacter()
        {
            string _filepath = "./Data/GranblueFantasy/HalloweenCharacters.json";
            var _jsonData = System.IO.File.ReadAllText(_filepath);
            JArray _JhalloweenCharacters = JArray.Parse(_jsonData);
            string[] _halloweenCharacters = _JhalloweenCharacters.ToObject<string[]>();
            return _halloweenCharacters;
        }

        /// <summary>
        /// List of winter characters in */Data/GranblueFantasy/WinterCharacters.json
        /// </summary>
        /// <returns></returns>
        string[] WinterCharacter()
        {
            string _filepath = "./Data/GranblueFantasy/WinterCharacters.json";
            var _jsonData = System.IO.File.ReadAllText(_filepath);
            JArray _JwinterCharacters = JArray.Parse(_jsonData);
            string[] _winterCharacters = _JwinterCharacters.ToObject<string[]>();
            return _winterCharacters;
        }

        /// <summary>
        /// List of zodiac characters in */Data/GranblueFantasy/ZodiacCharacters.json
        /// </summary>
        /// <returns></returns>
        string[] ZodiacCharacter()
        {
            string _filepath = "./Data/GranblueFantasy/ZodiacCharacters.json";
            var _jsonData = System.IO.File.ReadAllText(_filepath);
            JArray _JzodiacCharacters = JArray.Parse(_jsonData);
            string[] _zodiacCharacters = _JzodiacCharacters.ToObject<string[]>();
            return _zodiacCharacters;
        }

        /// <summary>
        /// List of valentine characters in */Data/GranblueFantasy/ValentineCharacters.json
        /// </summary>
        /// <returns></returns>
        string[] ValentineCharacter()
        {
            string _filepath = "./Data/GranblueFantasy/ValentineCharacters.json";
            var _jsonData = System.IO.File.ReadAllText(_filepath);
            JArray _JvalentineCharacters = JArray.Parse(_jsonData);
            string[] _valentineCharacters = _JvalentineCharacters.ToObject<string[]>();
            return _valentineCharacters;
        }
    }
}
