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
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Serialization;

namespace KaeruBot
{
    class GBFCommands
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

            //If the player does exist, update the information
            if (player != null && crystals != null && tickets != null)
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
            if (player == null && crystals != null && tickets != null)
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

            if (player == null)
            {
                await ctx.RespondAsync("You have not tracked your spark yet.\nDo **|spark (crystal amount) (ticket amount)** to add yourself");
            }

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

        string[] NormalCharacter()
        {
            return new string[]
            { "(Agielba)", "(Albert)", "(Aletheia)", "(Aliza)", "(Altair)", "(Amira)", "(Anne)", "(Anthuria)",
                "(Aoidos)", "(Arriet)", "(Arulumaya)", "(Ayer)", "(Azazel)", "(Baotorda)", "(Beatrix (Dark))",
                "(Cagliostro (Dark))", "(Carmelina)", "(Catherine)", "(Cerberus)", "(Charlotta)", "(Chat Noir)",
                "(Clarisse (Fire))", "(Clarisse (Light))", "(De La Fille (Light))", "(De La Fille (Earth))", "(Dorothy and Claudia)",
                "(Eustace)", "(Feena)", "(Ferry)", "(Forte)", "(Gawain)", "(Ghandagoza)", "(Grea)", "(Hallessena)",
                "(Heles)", "(Ilsa)", "(Izmir)", "(Jeanne d'Arc (Light))", "(Jeanne d'Arc (Dark))", "(Juliet)",
                "(Korwa)", "(Lady Grey)", "(Lady Katapillar and Vira)", "(Lancelot)", "(Lennah)", "(Lilele)", "(Lily)",
                "(Magisa)", "(Marquiares)", "(Melissabelle)", "(Melleau)", "(Metera (Wind))", "(Metera (Fire))",
                "(Narmaya)", "(Nemone)", "(Nezahualpilli)", "(Nicholas)", "(Percival)", "(Petra)", "(Razia)",
                "(Robomi)", "(Romeo)", "(Rosamia)", "(Sara)", "(Sarunan (Light))", "(Sarunan (Dark))", "(Scathacha)",
                "(Seruel)", "(Siegfried)", "(Silva)", "(Societte (Water))", "(Societte (Fire))", "(Sophia)", "(Therese)",
                "(Tiamat)", "(Vane)", "(Vania)", "(Vaserega (Dark))", "(Vaserega (Earth))", "(Veight)", "(Vira (Dark))",
                "(Yggdrasil)", "(Yngwie)", "(Yodarha)", "(Yuel (Fire))", "(Yuel (Water))", "(Yuisis)", "(Zalhamelina)",
                "(Zeta)", "(Zeta (Dark))", "(Zooey (Light))", "(Cucouroux)", "(Soriz)", "(Wulf and Renie)"
            };
        }

        string[] GrandCharacter()
        {
            return new string[]
            {
                "(Katalina (Grand))", "(Rackam (Grand))", "(Io (Grand))", "(Eugene (Grand))", "(Rosetta (Grand)", "(Lecia (Grand)), Cain (Grand))"
            };
        }

        string[] FlashCharacter()
        {
            return new string[]
            {
                "(Black Knight)", "(Orchid)", "(Sturm)", "(Drang)", "(Vira (Grand))", "(Lucio)", "(Olivia)", "(Alexiel)", "(Summer Zooey)"
            };
        }

        string[] SummerCharacter()
        {
            return new string[]
            {
                "(Beatrix (Summer))", "(Danua (Summer))", "(De La Fille (Summer))", "(Diantha (Summer))", "(Heles (Summer))",
                "(Io (Summer))", "(Izmir (Summer))", "(Jeanne d'Arc (Summer))", "(Korwa (Summer))", "(Narmaya (Summer))",
                "(Percival (Summer))", "(Siegfried (Summer))", "(Vira (Summer))", "(Zeta (Summer))", "(Zooey (Summer))"
            };
        }

        string[] HalloweenCharacter()
        {
            return new string[]
            {
                "(Cagliostro (Halloween))", "(Charlotta (Halloween))", "(Eustace (Halloween))", "(Danua (Halloween))"
            };
        }

        string[] WinterCharacter()
        {
            return new string[]
            {
                "(Arulumaya (Winter))", "(Clarisse (Winter))", "(Mary (Winter))", "(Rosetta (Winter)"
            };
        }

        string[] ZodiacCharacter()
        {
            return new string[]
            {
                "(Andira)", "(Anila)", "(Mahira)", "(Vajra)"
            };
        }
        
        string[] ValentineCharacter()
        {
            return new string[]
            {
                "Medusa (Valentine)"
            };
        }
    }
}
