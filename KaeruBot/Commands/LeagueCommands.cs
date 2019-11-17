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
using RiotNet;
using RiotNet.Converters;
using RiotNet.Models;
using System.Net.Http;
using Shizukanawa.KaeruBot.Objects;

namespace Shizukanawa.KaeruBot
{
    class LeagueCommands
    {
        public static HttpClient webClient = new HttpClient();

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
                IRiotClient client = new RiotClient(new RiotClientSettings
                {
                    ApiKey = GetAPIKey().LeagueAPI
                });
                try
                {
                    string platform = GetRegion(region);
                    DataDragon.Realms DataDragonRegion = JsonConvert.DeserializeObject<DataDragon.Realms>(ddjson);
                    RiotNet.Models.Summoner summoner = await client.GetSummonerBySummonerNameAsync(name, platform);
                    List<LeagueEntry> Entries = await client.GetLeagueEntriesBySummonerIdAsync(summoner.Id, platform);
                    string ranks = string.Empty;
                    for (int i = 0; i < Entries.Count; ++i)
                    {
                        ranks = ranks + $"**{Entries[i].QueueType}:** {Entries[i].Tier} {Entries[i].Rank}\n";
                    }
                    var embed0 = new DiscordEmbedBuilder()
                    {
                        Title = $"Summoner: {summoner.Name}",
                        Description = $"**Level:** {summoner.SummonerLevel}\n{ranks}",
                        ThumbnailUrl = $"http://ddragon.leagueoflegends.com/cdn/{DataDragonRegion.n.profileicon}/img/profileicon/{summoner.ProfileIconId}.png"
                    };
                    await ctx.RespondAsync(embed: embed0.Build());
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync("Couldn't find the summoner in the region.");
                }
            }
        }

        [Command("game"), Description("Gets the current game for the summoner.\n**Usage:** |game (region) (name)")]
        public async Task LGame(CommandContext ctx, string region, [RemainingTextAttribute] string name)
        {
            var ddjson = await DataDragon.GetDDVersionAsync(region);
            if (ddjson == "invalidregion".ToLower())
            {
                await ctx.RespondAsync("Invalid Region");
            }
            else
            {
                IRiotClient client = new RiotClient(new RiotClientSettings
                {
                    ApiKey = GetAPIKey().LeagueAPI
                });
                try
                {
                    string platform = GetRegion(region);
                    DataDragon.Realms DataDragonRegion = JsonConvert.DeserializeObject<DataDragon.Realms>((ddjson));
                    RiotNet.Models.Summoner summoner = await client.GetSummonerBySummonerNameAsync(name, platform);
                    CurrentGameInfo spectators = await client.GetActiveGameBySummonerIdAsync(summoner.Id, platform);

                    int playerCount = spectators.Participants.Count;
                    string redTeam = string.Empty;
                    string blueTeam = string.Empty;
                    long[] championIds = new long[playerCount];
                    string[] rank = new string[playerCount];
                    string profileIcon = string.Empty;
                    List<Champions.Champion> champions = new List<Champions.Champion>();
                    int i = 0;
                    for (; i < playerCount; ++i)
                    {
                        List<LeagueEntry> Entries = await client.GetLeagueEntriesBySummonerIdAsync(spectators.Participants[i].SummonerId, platform);
                        for (int j = 0; j < Entries.Count; j++)
                        {
                            if (Entries[j].QueueType == "RANKED_SOLO_5x5")
                                rank[i] = $"{Entries[j].Tier} {Entries[j].Rank}";
                        }

                        if (string.IsNullOrEmpty(rank[i]))
                            rank[i] = $"Unranked";

                        championIds[i] = spectators.Participants[i].ChampionId;
                    }
                    champions = await GetChampionsFromIDAsync(region, championIds);

                    for (i = 0; i < playerCount / 2; ++i)
                        redTeam = redTeam + $"**Player:** {spectators.Participants[i].SummonerName} **Champ:** {champions[i].name}\n **Rank:** {rank[i]}\n\n";

                    for (; i < playerCount; ++i)
                        blueTeam = blueTeam + $"**Player:** {spectators.Participants[i].SummonerName} **Champ:** {champions[i].name}\n **Rank:** {rank[i]}\n\n";

                    for (i = 0; i < playerCount; ++i)
                    {
                        if (spectators.Participants[i].SummonerName.ToLower() == name.ToLower())
                            profileIcon = $"http://ddragon.leagueoflegends.com/cdn/{DataDragonRegion.n.champion}/img/champion/{champions[i].name}.png";
                    }
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Game for: {summoner.Name}",
                        Description = $"**Red Team:**\n {redTeam}\n**Blue Team:**\n {blueTeam}\n",
                        ThumbnailUrl = profileIcon
                    };
                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync(ex.Message);
                }
            }
        }

        /// <summary>
        /// Returns the API Key from */Data/APIKeys/LeagueAPI.json
        /// </summary>
        /// <returns></returns>
        private apikey GetAPIKey()
        {
            var filePath = "./Data/APIKeys/LeagueAPI.json";
            var jsonData = System.IO.File.ReadAllText(filePath);
            apikey _apikey = JsonConvert.DeserializeObject<apikey>(jsonData);
            return _apikey;
        }
        
        private async Task<List<Champions.Champion>> GetChampionsFromIDAsync(string region, long[] ids)
        {
            List<Champions.Champion> listOfChamps = new List<Champions.Champion>();
            var ddjson = await DataDragon.GetDDVersionAsync(region);
            if (ddjson == "invalidregion".ToLower())
            {
                return null;
            }
            else
            {
                DataDragon.Realms DataDragonRegion = JsonConvert.DeserializeObject<DataDragon.Realms>(ddjson);
                Uri url = new Uri($"http://ddragon.leagueoflegends.com/cdn/{DataDragonRegion.n.champion}/data/en_US/champion.json");
                var jsonData = await webClient.GetStringAsync(url);
                var json = JsonConvert.DeserializeObject<Champions.RootChampions>(jsonData);

                for (int i = 0; i < ids.Length; ++i)
                {
                    foreach (var champion in json.Data)
                    {
                        if (champion.Value.key == ids[i].ToString())
                        {
                            listOfChamps.Add(champion.Value);
                            break;
                        }
                    }
                }
                return listOfChamps;
            }
        }

        /// <summary>
        /// Returns the League of Legends region
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private string GetRegion(string region)
        {
            var regions = new Dictionary<string, string>
            {
                { "na", "NA1" },
                { "euw", "EUW1" },
                { "eune", "EUN1" },
                { "br", "BR1" },
                { "kr", "KR" },
                { "lan", "LA1" },
                { "las", "LA2" },
                { "tr", "TR1" },
                { "oce", "OC1" },
                { "jp", "JP1" },
                { "ru", "RU" }
            };

            if (regions.ContainsKey(region))
            {
                return regions[region];
            }
            else
            {
                return "euw1";
            }
        }



        private class apikey
        {
            public string LeagueAPI { get; set; }
        }

    }
}
