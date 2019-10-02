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
                    switch (Entries.Count)
                    {
                        case 0:
                            var embed0 = new DiscordEmbedBuilder()
                            {
                                Title = $"Summoner: {summoner.Name}",
                                Description = $"**Level:** {summoner.SummonerLevel}\n",
                                ThumbnailUrl = $"http://ddragon.leagueoflegends.com/cdn/{DataDragonRegion.n.profileicon}/img/profileicon/{summoner.ProfileIconId}.png"
                            };
                            await ctx.RespondAsync(embed: embed0.Build());
                            break;
                        case 1:
                            var embed1 = new DiscordEmbedBuilder()
                            {
                                Title = $"Summoner: {summoner.Name}",
                                Description = $"**Level:** {summoner.SummonerLevel}\n**{Entries[0].QueueType}:** {Entries[0].Tier} {Entries[0].Rank}",
                                ThumbnailUrl = $"http://ddragon.leagueoflegends.com/cdn/{DataDragonRegion.n.profileicon}/img/profileicon/{summoner.ProfileIconId}.png"
                            };
                            await ctx.RespondAsync(embed: embed1.Build());
                            break;
                        case 2:
                            var embed2 = new DiscordEmbedBuilder()
                            {
                                Title = $"Summoner: {summoner.Name}",
                                Description = $"**Level:** {summoner.SummonerLevel}\n**{Entries[0].QueueType}:** {Entries[0].Tier} {Entries[0].Rank}\n**{Entries[1].QueueType}:** {Entries[1].Tier} {Entries[1].Rank}\n",
                                ThumbnailUrl = $"http://ddragon.leagueoflegends.com/cdn/{DataDragonRegion.n.profileicon}/img/profileicon/{summoner.ProfileIconId}.png"
                            };
                            await ctx.RespondAsync(embed: embed2.Build());
                            break;
                        case 3:
                            var embed3 = new DiscordEmbedBuilder()
                            {
                                Title = $"Summoner: {summoner.Name}",
                                Description = $"**Level:** {summoner.SummonerLevel}\n**{Entries[0].QueueType}:** {Entries[0].Tier} {Entries[0].Rank}\n**{Entries[1].QueueType}:** {Entries[1].Tier} {Entries[1].Rank}\n**{Entries[2].QueueType}:** {Entries[2].Tier} {Entries[2].Rank}\n",
                                ThumbnailUrl = $"http://ddragon.leagueoflegends.com/cdn/{DataDragonRegion.n.profileicon}/img/profileicon/{summoner.ProfileIconId}.png"
                            };
                            await ctx.RespondAsync(embed: embed3.Build());
                            break;
                    }

                }
                catch (RateLimitExceededException ex)
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
                    CurrentGameInfo spectator = await client.GetActiveGameBySummonerIdAsync(summoner.Id, platform);

                    string redTeam = string.Empty;
                    string blueTeam = string.Empty;
                    Champions.Champion champion = null;
                    string rank = string.Empty;
                    string profileIcon = string.Empty;

                    int playerCount = spectator.Participants.Count;
                    int i = 0;
                    for (; i < playerCount / 2; ++i)
                    {
                        List<LeagueEntry> Entries = await client.GetLeagueEntriesBySummonerIdAsync(spectator.Participants[i].SummonerId, platform);
                        if (Entries[0].QueueType == "RANKED_SOLO_5x5")
                            rank = $"{Entries[0].Tier} {Entries[0].Rank}";
                        else if (Entries[1].QueueType == "RANKED_SOLO_5x5")
                            rank = $"{Entries[1].Tier} {Entries[1].Rank}";
                        else if (Entries[2].QueueType == "RANKED_SOLO_5x5")
                            rank = $"{Entries[2].Tier} {Entries[2].Rank}";
                        else
                            rank = "UNRANKED";


                        champion = await GetChampionFromIDAsync(region, spectator.Participants[i].ChampionId);

                        if (spectator.Participants[i].SummonerName.ToLower() == name.ToLower())
                            profileIcon = $"http://ddragon.leagueoflegends.com/cdn/{DataDragonRegion.n.champion}/img/champion/{champion.name}.png";

                        redTeam = redTeam + $"**Player:** {spectator.Participants[i].SummonerName} **Champ:** {champion.name}\n **Rank:** {rank}\n\n";
                    }
                    
                    for(; i < playerCount; ++i)
                    {
                        List<LeagueEntry> Entries = await client.GetLeagueEntriesBySummonerIdAsync(spectator.Participants[i].SummonerId, platform);
                        if (Entries[0].QueueType == "RANKED_SOLO_5x5")
                            rank = $"{Entries[0].Tier} {Entries[0].Rank}";
                        else if (Entries[1].QueueType == "RANKED_SOLO_5x5")
                            rank = $"{Entries[1].Tier} {Entries[1].Rank}";
                        else if (Entries[2].QueueType == "RANKED_SOLO_5x5")
                            rank = $"{Entries[2].Tier} {Entries[2].Rank}";
                        else
                            rank = "UNRANKED";

                        champion = await GetChampionFromIDAsync(region, spectator.Participants[i].ChampionId);

                        if (spectator.Participants[i].SummonerName.ToLower() == name.ToLower())
                            profileIcon = $"http://ddragon.leagueoflegends.com/cdn/{DataDragonRegion.n.champion}/img/champion/{champion.name}.png";

                        blueTeam = blueTeam + $"**Player:** {spectator.Participants[i].SummonerName} **Champ:** {champion.name}\n **Rank:** {rank}\n\n";
                    }

                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Game for: {summoner.Name}",
                        Description = $"**Red Team:**\n {redTeam}\n**Blue Team:**\n {blueTeam}\n",
                        ThumbnailUrl = profileIcon
                    };
                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (RateLimitExceededException ex)
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
        
        private async Task<Champions.Champion> GetChampionFromIDAsync(string region, long id)
        {
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

                foreach (var champion in json.Data)
                {
                    if (champion.Value.key == id.ToString())
                    {
                        return champion.Value;
                    }
                }
                return null;
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
