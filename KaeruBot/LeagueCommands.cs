using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shizukanawa.RiotAPI;
using RiotSharp;
using RiotSharp.Misc;

namespace Shizukanawa.KaeruBot
{
    class LeagueCommands
    {
        [Command("summoner"), Description("Gets a summoner by name.\n**Usage** |summoner (region) (name)")]
        public async Task LSummoner(CommandContext ctx, string region, [RemainingTextAttribute] string name)
        {
            var ddjson = await DataDragon.GetDDVersionAsync(region);
            if (ddjson == "invalidregion".ToLower())
            {
                await ctx.RespondAsync("Invalid Region");
            }
            else
            {
                var api = RiotApi.GetDevelopmentInstance(GetAPIKey().LeagueAPI);
                try
                {
                    DataDragon.Realms DataDragonRegion = JsonConvert.DeserializeObject<DataDragon.Realms>(ddjson);
                    var summoner = api.GetSummonerByName(GetRegion(region), name);
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Summoner: {summoner.Name}",
                        Description = $"**Level:** {summoner.Level}\n**Summoner ID:** {summoner.Id}\n**Account ID:** {summoner.AccountId}",
                        ThumbnailUrl = $"http://ddragon.leagueoflegends.com/cdn/{DataDragonRegion.n.profileicon}/img/profileicon/{summoner.ProfileIconId}.png"
                    };
                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (RiotSharpException ex)
                {
                    await ctx.RespondAsync(ex.Message);
                }
           }
        }

        private apikey GetAPIKey()
        {
            var filePath = "./Data/APIKeys/LeagueAPI.json";
            var jsonData = System.IO.File.ReadAllText(filePath);
            apikey _apikey = JsonConvert.DeserializeObject<apikey>(jsonData);
            return _apikey;
        }

        private Region GetRegion(string region)
        {
            var regions = new Dictionary<string, Region>
            {
                { "na", Region.na },
                { "euw", Region.euw },
                { "eune", Region.eune },
                { "br", Region.br },
                { "kr", Region.kr },
                { "lan", Region.lan },
                { "las", Region.las },
                { "tr", Region.tr },
                { "oce", Region.oce },
                { "jp", Region.jp },
                { "ru", Region.ru }
            };

            if (regions.ContainsKey(region))
            {
                return regions[region];
            }
            else
            {
                return Region.global;
            }
        }

        public class apikey
        {
            public string LeagueAPI { get; set; }
        }

    }
}
