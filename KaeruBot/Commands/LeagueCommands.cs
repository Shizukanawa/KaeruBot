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
                    /*To get rank of specific summoner, see in LEAGUE in api documentation*/
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
                    await ctx.RespondAsync(ex.Message);
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
                        if (Entries[0].QueueType == "RANKED_SOLO_5x5")
                            rank[i] = $"{Entries[0].Tier} {Entries[0].Rank}";
                        else if (Entries[1].QueueType == "RANKED_SOLO_5x5")
                            rank[i] = $"{Entries[1].Tier} {Entries[1].Rank}";
                        else if (Entries[2].QueueType == "RANKED_SOLO_5x5")
                            rank[i] = $"{Entries[2].Tier} {Entries[2].Rank}";
                        else
                            rank[i] = "UNRANKED";
                            
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
                /*
                foreach (var champion in json.Data)
                {
                    if (champion.Value.key == ids.ToString())
                    {
                        return champion.Value;
                    }
                }*/
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
                { "na", "na1" },
                { "euw", "euw1" },
                { "eune", "eun1" },
                { "br", "br1" },
                { "kr", "kr" },
                { "lan", "la1" },
                { "las", "la2" },
                { "tr", "tr1" },
                { "oce", "oc1" },
                { "jp", "jp1" },
                { "ru", "ru" }
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
