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

namespace Shizukanawa.KaeruBot
{
    class LeagueCommands
    {
        [Command("summoner"), Description("Gets a summoner by name.\n**Usage** |summoner (region) (name)")]
        public async Task PSummoner(CommandContext ctx, string region, [RemainingTextAttribute] string name)
        {
            var filePath = "./Data/APIKeys/LeagueAPI.json";
            var jsonData = System.IO.File.ReadAllText(filePath);
            apikey _apikey = JsonConvert.DeserializeObject<apikey>(jsonData);

            string apiKey = _apikey.LeagueAPI;
            string endpoint = Summoner.GetRegion(region);
            string version = _apikey.Version;
            string summonerName = name;

            if (endpoint == "invalidregion")
            {
                await ctx.RespondAsync("Invalid region");
            }
            else
            {
                Uri url = new Uri($"https://{endpoint}/lol/summoner/v{version}/summoners/by-name/{summonerName}?api_key={apiKey}");
                string jsonDataSummoner = await Summoner.GetSummonerAsync(url);

                if (jsonDataSummoner == "BadRequest".ToLower()) await ctx.RespondAsync("Bad Request.");
                if (jsonDataSummoner == "Unauthorized".ToLower()) await ctx.RespondAsync("Unauthorized");
                if (jsonDataSummoner == "Forbidden".ToLower()) await ctx.RespondAsync("Forbidden.");
                if (jsonDataSummoner == "DataNotFound".ToLower()) await ctx.RespondAsync("Player not found.");
                if (jsonDataSummoner == "MethodNotAllowed".ToLower()) await ctx.RespondAsync("MethodNotAllowed");
                if (jsonDataSummoner == "UnsupportedMediaType".ToLower()) await ctx.RespondAsync("Unsupported Media Type");
                if (jsonDataSummoner == "PlayerExistNoMatch".ToLower()) await ctx.RespondAsync("Player exist but no matches played.");
                if (jsonDataSummoner == "RateLimit  Exceeded".ToLower()) await ctx.RespondAsync("Rate Limit Exceeded.");
                if (jsonDataSummoner == "InternalServerError".ToLower()) await ctx.RespondAsync("Internal Server Error");
                if (jsonDataSummoner == "BadGateway".ToLower()) await ctx.RespondAsync("Bad Gateway");
                if (jsonDataSummoner == "ServiceUnavailable".ToLower()) await ctx.RespondAsync("Service Unavailable");
                if (jsonDataSummoner == "GatewayTimeout".ToLower()) await ctx.RespondAsync("Gateway Timeout");
                else
                {
                    //Summoner
                    Summoner.SummonerData summoner = JsonConvert.DeserializeObject<Summoner.SummonerData>(jsonDataSummoner);

                    //Data Dragon
                    string output = DataDragon.GetDDVersionAsync(region).GetAwaiter().GetResult();
                    DataDragon.Realms DataDragonRegion = JsonConvert.DeserializeObject<DataDragon.Realms>(output);

                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Summoner: {summoner.name}",
                        Description = $"**Level:** {summoner.summonerLevel}\n**Summoner ID:** {summoner.id}\n**Account ID:** {summoner.accountId}",
                        ThumbnailUrl = $"http://ddragon.leagueoflegends.com/cdn/{DataDragonRegion.n.profileicon}/img/profileicon/{summoner.profileIconId}.png"
                    };
                    await ctx.RespondAsync(embed: embed.Build());
                }
            }
        }

        public class apikey
        {
            public string LeagueAPI { get; set; }
            public string Version { get; set; }
        }

    }
}
