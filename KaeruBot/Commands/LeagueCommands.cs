﻿using System;
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
using MingweiSamuel.Camille;
using MingweiSamuel.Camille.Enums;
using MingweiSamuel.Camille.LeagueV4;
using MingweiSamuel.Camille.Util;
using MingweiSamuel.Camille.SpectatorV4;
using System.Net.Http;
using Shizukanawa.KaeruBot.Objects;

namespace Shizukanawa.KaeruBot
{
    class LeagueCommands : BaseCommandModule
    {
        public static HttpClient webClient = new HttpClient();

        [Command("summoner"), Description("Gets a summoner by name.\n**Usage** |summoner (region) (name)")]
        public async Task LSummoner(CommandContext ctx, string region, [RemainingText] string name)
        {
            var ddjson = await DataDragon.GetDDVersionAsync(region);
            if (ddjson == "invalidregion".ToLower())
            {
                await ctx.RespondAsync("Invalid Region");
            }
            else
            {
                RiotApi riotApi = RiotApi.NewInstance(GetAPIKey().LeagueAPI);
                try
                {
                    Region platform = GetRegion(region);
                    DataDragon.Realms DataDragonRegion = JsonConvert.DeserializeObject<DataDragon.Realms>(ddjson);
                    var summoner = await riotApi.SummonerV4.GetBySummonerNameAsync(platform, name);
                    LeagueEntry[] Entries = await riotApi.LeagueV4.GetLeagueEntriesForSummonerAsync(platform, summoner.Id);
                    string ranks = string.Empty;
                    for (int i = 0; i < Entries.Length; ++i)
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
                catch (RiotResponseException ex)
                {
                    await ctx.RespondAsync(ex.Message);
                }
                catch (Exception)
                {
                    await ctx.RespondAsync("Couldn't find the summoner in the region.");
                }
            }
        }

        [Command("game"), Description("Gets the current game for the summoner.\n**Usage:** |game (region) (name)")]
        public async Task LGame(CommandContext ctx, string region, [RemainingText] string name)
        {
            var ddjson = await DataDragon.GetDDVersionAsync(region);
            if (ddjson == "invalidregion".ToLower())
            {
                await ctx.RespondAsync("Invalid Region");
            }
            else
            {
                RiotApi riotApi = RiotApi.NewInstance(GetAPIKey().LeagueAPI);
                try
                {
                    Region platform = GetRegion(region);
                    DataDragon.Realms DataDragonRegion = JsonConvert.DeserializeObject<DataDragon.Realms>((ddjson));
                    var summoner = await riotApi.SummonerV4.GetBySummonerNameAsync(platform, name);
                    var spectators = await riotApi.SpectatorV4.GetCurrentGameInfoBySummonerAsync(platform, summoner.Id);

                    int playerCount = spectators.Participants.Length;
                    string redTeam = string.Empty;
                    string blueTeam = string.Empty;
                    long[] championIds = new long[playerCount];
                    string[] rank = new string[playerCount];
                    string profileIcon = string.Empty;
                    List<Champions.Champion> champions = new List<Champions.Champion>();

                    rank = await GetSummonerRanksAsync(riotApi, platform, spectators);

                    championIds = GetChampionIDsFromCurrentGame(spectators);

                    champions = await GetChampionsFromIDAsync(region, championIds);

                    redTeam = PrintRedTeam(spectators, champions, rank);

                    blueTeam = PrintBlueTeam(spectators, champions, rank);

                    profileIcon = GetProfileIcon(spectators, DataDragonRegion.n.champion, champions, name);

                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Game for: {summoner.Name}",
                        ThumbnailUrl = profileIcon
                    };

                    embed.AddField("Blue Team", blueTeam, true);
                    embed.AddField("Red Team", redTeam, true);

                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (RiotResponseException ex)
                {
                    await ctx.RespondAsync(ex.Message);
                }
                catch (NullReferenceException)
                {
                    await ctx.RespondAsync("Couldn't find the summoner or the summoner isn't in game yet.");
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

        private async Task<string[]> GetSummonerRanksAsync(RiotApi riotApi, Region platform, CurrentGameInfo spectators)
        {
            int playerCount = spectators.Participants.Length;
            string[] rank = new string[playerCount];
            for (int i = 0; i < playerCount; ++i)
            {
                LeagueEntry[] Entries = await riotApi.LeagueV4.GetLeagueEntriesForSummonerAsync(platform, spectators.Participants[i].SummonerId);
                for (int j = 0; j < Entries.Length; j++)
                {
                    if (Entries[j].QueueType == "RANKED_SOLO_5x5")
                        rank[i] = $"{Entries[j].Tier} {Entries[j].Rank}";
                }

                if (string.IsNullOrEmpty(rank[i]))
                    rank[i] = $"Unranked";
            }
            return rank;
        }

        private long[] GetChampionIDsFromCurrentGame(CurrentGameInfo spectators)
        {
            int playerCount = spectators.Participants.Length;
            long[] championIds = new long[playerCount];
            for (int i = 0; i < playerCount; ++i)
            {
                championIds[i] = spectators.Participants[i].ChampionId;
            }
            return championIds;
        }

        private string PrintBlueTeam(CurrentGameInfo spectators, List<Champions.Champion> champions, string[] rank)
        {
            int playerCount = spectators.Participants.Length;
            string result = string.Empty;
            for (int i = 0; i < playerCount; ++i)
            {
                if (spectators.Participants[i].TeamId == 100)
                    result = result + $"**Player:** {spectators.Participants[i].SummonerName}\n **Champ:** {champions[i].name}\n **Rank:** {rank[i]}\n\n";
            }
            return result;
        }

        private string PrintRedTeam(CurrentGameInfo spectators, List<Champions.Champion> champions, string[] rank)
        {
            int playerCount = spectators.Participants.Length;
            string result = string.Empty;
            for (int i = 0; i < playerCount; ++i)
            {
                if (spectators.Participants[i].TeamId == 200)
                    result = result + $"**Player:** {spectators.Participants[i].SummonerName}\n **Champ:** {champions[i].name}\n **Rank:** {rank[i]}\n\n";
            }
            return result;
        }

        private string GetProfileIcon(CurrentGameInfo spectators, string DDChampion, List<Champions.Champion> champions, string name)
        {
            int playerCount = spectators.Participants.Length;
            string result = string.Empty;
            for (int i = 0; i < playerCount; ++i)
            {
                if (spectators.Participants[i].SummonerName.ToLower() == name.ToLower())
                    result = $"http://ddragon.leagueoflegends.com/cdn/{DDChampion}/img/champion/{champions[i].id}.png";
            }
            return result;
        }

        /// <summary>
        /// Returns the League of Legends region
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private Region GetRegion(string region)
        {
            var regions = new Dictionary<string, Region>
            {
                { "na", Region.NA },
                { "euw", Region.EUW },
                { "eune", Region.EUNE },
                { "br", Region.BR },
                { "kr", Region.KR },
                { "lan", Region.LAN },
                { "las", Region.LAS },
                { "tr", Region.TR },
                { "oce", Region.OCE },
                { "jp", Region.JP },
                { "ru", Region.RU }
            };

            if (regions.ContainsKey(region))
            {
                return regions[region];
            }
            return Region.EUW;
        }

        private class apikey
        {
            public string LeagueAPI { get; set; }
        }

    }
}
