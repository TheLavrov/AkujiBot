using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Easy.Common.Interfaces;
using Easy.Common;
using System.Xml.Linq;

namespace DiscordBot.Modules
{


    public class SteamModule : ModuleBase<SocketCommandContext>
    {
        string DevSteamID = "83D896ED50FFA71F28E69A4C672218D3";

        [Command("steam")]
        [Remarks("steam [Steam Profile ID]")]
        [Summary("Gives you a summary of your Steam profile.")]
        public async Task Steam(string SteamName)
        {
            string[] ProfileState = { "Offline", "Online", "Busy", "Away", "Snooze", "Looking to Play", "Looking to Trade" }; 
            string ResolveURL = $"http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={DevSteamID}&vanityurl={SteamName}";

            using (IRestClient client = new RestClient())
            {
                ResolveURL = await client.GetStringAsync(ResolveURL);
                dynamic User = JsonConvert.DeserializeObject(ResolveURL);

                if (User["response"]["success"] != 1)
                {
                    var message = await ReplyAsync($"`There was an error using {SteamName}. Steam callback message: \"{User["response"]["message"]}\"`");
                    await Task.Delay(15000);
                    await message.DeleteAsync();
                    await Context.Message.DeleteAsync();
                }
                else
                {
                    string SummaryURL = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={DevSteamID}&steamids={User["response"]["steamid"]}";
                    SummaryURL = await client.GetStringAsync(SummaryURL);
                    dynamic Summary = JsonConvert.DeserializeObject(SummaryURL);

                    var embed = new EmbedBuilder()
                        .WithAuthor($"{Summary["response"]["players"][0]["personaname"]}", $"{Summary["response"]["players"][0]["avatar"]}", $"{Summary["response"]["players"][0]["profileurl"]}")
                        .WithThumbnailUrl($"{Summary["response"]["players"][0]["avatarfull"]}")
                        ;

                    ulong steamid = (Convert.ToUInt64(Summary["response"]["players"][0]["steamid"]) - (76561197960265728 + (Convert.ToUInt64(Summary["response"]["players"][0]["steamid"]) % 2))) / 2;
                    if (Convert.ToUInt64(Summary["response"]["players"][0]["steamid"]) % 2 == 1)
                    {

                        embed.AddField("SteamID", $"STEAM_0:1:{steamid}", true);
                    }
                    else
                    {
                        embed.AddField("SteamID", $"STEAM_0:0:{steamid}", true);
                    }

                    embed.AddField("SteamID64", Summary["response"]["players"][0]["steamid"], true);

                    if (Summary["response"]["players"][0]["communityvisibilitystate"] == 3)
                    {
                        embed.AddField("Profile Visibility", "Public", true);
                    }
                    else
                    {
                        embed.AddField("Profile Visibility", "Private", true);
                    }

                    if (Summary["response"]["players"][0]["gameextrainfo"] != null)                                                                                              //If user is in a game
                    {
                        embed.WithDescription($"In Game: {Summary["response"]["players"][0]["gameextrainfo"]}");
                        embed.Color = new Color(144, 186, 60);
                    }
                    else if (Summary["response"]["players"][0]["personastate"] != 0)                                                                                             //If user is online in some fashion
                    {
                        embed.WithDescription(ProfileState[Summary["response"]["players"][0]["personastate"]]);
                        embed.Color = new Color(84, 165, 196);
                    }
                    else                                                                                                                                                //If user is offline
                    {
                        embed.WithDescription(ProfileState[Summary["response"]["players"][0]["personastate"]]);
                        embed.Color = new Color(127, 127, 127);
                    }

                    if (Summary["response"]["players"][0]["timecreated"] != 0)                                                                                                   //If user has a non-private TimeCreated
                    {
                        DateTimeOffset CreatedOn = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(Summary["response"]["players"][0]["timecreated"]));
                        embed.WithFooter("Account Created On:");
                        embed.WithTimestamp(CreatedOn);
                    }

                    await ReplyAsync("", false, embed.Build());
                }
            }
        }

        [Command("llbrank")]
        [Remarks("llbrank [Steam Profile ID]")]
        [Summary("Gives you a summary of your current Lethal League Blaze ranking.")]
        public async Task LethalLeagueRank(string SteamName)
        {
            string LLBLeaderboards = "https://steamcommunity.com/stats/553310/leaderboards/?xml=1";
            string LLBCurrent = "";
            string ResolveURL = $"http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={DevSteamID}&vanityurl={SteamName}";

            using (IRestClient client = new RestClient())
            {
                ResolveURL = await client.GetStringAsync(ResolveURL);
                dynamic User = JsonConvert.DeserializeObject(ResolveURL);

                if (User["response"]["success"] != 1)
                {
                    var message = await ReplyAsync($"`There was an error using {SteamName}. Steam callback message: \"{User["response"]["message"]}\"`");
                    await Task.Delay(15000);
                    await message.DeleteAsync();
                    await Context.Message.DeleteAsync();
                }
                else
                {
                    ResolveURL = await client.GetStringAsync(LLBLeaderboards);
                    XDocument xmllb = XDocument.Parse(ResolveURL); //or XDocument.Load(path)
                    string jsonText = JsonConvert.SerializeXNode(xmllb);
                    dynamic LLBLeaderboard = JsonConvert.DeserializeObject(jsonText);
                    int search = 0;

                    string SummaryURL = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={DevSteamID}&steamids={User["response"]["steamid"]}";
                    SummaryURL = await client.GetStringAsync(SummaryURL);
                    dynamic Summary = JsonConvert.DeserializeObject(SummaryURL);

                    if (LLBLeaderboard["response"]["leaderboardCount"] == 1)                                            //in case a new season happens
                    {
                        LLBCurrent = LLBLeaderboard["response"]["leaderboard"]["url"] + "&steamid=" + Summary["response"]["players"][0]["steamid"];
                    }
                    else
                    {
                        foreach (var leaderboard in LLBLeaderboard["response"]["leaderboard"])
                        {
                            if (Convert.ToInt32(leaderboard["lbid"]) > search)
                            {
                                search = Convert.ToInt32(leaderboard["lbid"]);
                                LLBCurrent = leaderboard["url"] + "&steamid=" + Summary["response"]["players"][0]["steamid"];
                            }
                        }
                    }

                    LLBCurrent = await client.GetStringAsync(LLBCurrent);
                    XDocument xmlcurrent = XDocument.Parse(LLBCurrent); //or XDocument.Load(path)
                    jsonText = JsonConvert.SerializeXNode(xmlcurrent);
                    dynamic LLBStats = JsonConvert.DeserializeObject(jsonText);
                    bool found = false;

                    if (LLBStats["response"]["resultCount"] == 1)
                    {
                        if (LLBStats["response"]["entries"]["entry"]["steamid"] == Summary["response"]["players"][0]["steamid"])
                        {
                            found = true;
                            var embed = new EmbedBuilder()
                                .WithAuthor($"{Summary["response"]["players"][0]["personaname"]}", $"{Summary["response"]["players"][0]["avatar"]}", $"{Summary["response"]["players"][0]["profileurl"]}")
                                .WithThumbnailUrl($"{Summary["response"]["players"][0]["avatarfull"]}")
                                .AddField("Rank", $"{LLBStats["response"]["entries"]["entry"]["rank"]}")
                                .AddField("Score", $"{LLBStats["response"]["entries"]["entry"]["score"]}");

                            if (Convert.ToInt32(LLBStats["response"]["entries"]["entry"]["rank"]) <= 100)
                                embed.WithColor(30, 254, 218);
                            else if (Convert.ToInt32(LLBStats["response"]["entries"]["entry"]["rank"]) <= 1000)
                                embed.WithColor(252, 245, 69);

                            await ReplyAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        foreach (var entry in LLBStats["response"]["entries"]["entry"])
                        {
                            if (entry["steamid"] == Summary["response"]["players"][0]["steamid"])
                            {
                                found = true;
                                var embed = new EmbedBuilder()
                                    .WithAuthor($"{Summary["response"]["players"][0]["personaname"]}", $"{Summary["response"]["players"][0]["avatar"]}", $"{Summary["response"]["players"][0]["profileurl"]}")
                                    .WithThumbnailUrl($"{Summary["response"]["players"][0]["avatarfull"]}")
                                    .AddField("Rank", $"{entry["rank"]}")
                                    .AddField("Score", $"{entry["score"]}");

                                if (Convert.ToInt32(entry["rank"]) <= 100)
                                    embed.WithColor(30, 254, 218);
                                else if (Convert.ToInt32(entry["rank"]) <= 1000)
                                    embed.WithColor(252, 245, 69);

                                await ReplyAsync("", false, embed.Build());
                            }
                        }
                    }           

                    if (!found)
                    {
                        var message = await ReplyAsync($"`There was an error finding {SteamName} on the leaderboards. Steam callback message: \"{LLBStats["response"]["message"]}\"`");
                        await Task.Delay(15000);
                        await message.DeleteAsync();
                        await Context.Message.DeleteAsync();
                    }
                }
            }
        }
    }
}
