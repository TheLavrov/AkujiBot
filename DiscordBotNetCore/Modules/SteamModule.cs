using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Easy.Common.Interfaces;
using Easy.Common;

namespace DiscordBot.Modules
{
	public class SteamModule : ModuleBase<SocketCommandContext>
	{
		[Command("steam")]
		[Remarks("steam [Steam name]")]
		[Summary("Gives you a summary of your Steam profile.")]
		public async Task Steam(string SteamName)
		{
			string[] ProfileState = { "Offline", "Online", "Busy", "Away", "Snooze", "Looking to Play", "Looking to Trade" };

			string DevSteamID = "83D896ED50FFA71F28E69A4C672218D3";
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
	}
}
