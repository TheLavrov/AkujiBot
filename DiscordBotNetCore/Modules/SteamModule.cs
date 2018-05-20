using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Easy.Common.Interfaces;
using Easy.Common;

namespace DiscordBot.Modules
{
	public class Steam
	{
		public Response response { get; set; }
		public class Response
		{
			public string steamid { get; set; }
			public int success { get; set; }
			public string message { get; set; }

			public Player[] players { get; set; }
			public class Player
			{
				public string steamid { get; set; }
				public string gameid { get; set; }
				public string gameextrainfo { get; set; }
				public int communityvisibilitystate { get; set; }
				public int profilestate { get; set; }
				public string personaname { get; set; }
				public int lastlogoff { get; set; }
				public string profileurl { get; set; }
				public string avatar { get; set; }
				public string avatarmedium { get; set; }
				public string avatarfull { get; set; }
				public int personastate { get; set; }
				public string primaryclanid { get; set; }
				public int timecreated { get; set; }
				public int personastateflags { get; set; }
			}
		}
	}

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
				Steam User = JsonConvert.DeserializeObject<Steam>(ResolveURL);

				if (User.response.success != 1)
				{
					var message = await ReplyAsync($"`There was an error using {SteamName}. Steam callback message: \"{User.response.message}\"`");
					await Task.Delay(15000);
					await message.DeleteAsync();
					await Context.Message.DeleteAsync();
				}
				else
				{
					string SummaryURL = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={DevSteamID}&steamids={User.response.steamid}";
					SummaryURL = await client.GetStringAsync(SummaryURL);
					Steam Summary = JsonConvert.DeserializeObject<Steam>(SummaryURL);

					var embed = new EmbedBuilder()
						.WithAuthor(x => x.WithName(Summary.response.players[0].personaname)
						.WithIconUrl(Summary.response.players[0].avatar)
						.WithUrl(Summary.response.players[0].profileurl))
						.WithThumbnailUrl(Summary.response.players[0].avatarfull)
						;

					ulong steamid = (Convert.ToUInt64(Summary.response.players[0].steamid) - (76561197960265728 + (Convert.ToUInt64(Summary.response.players[0].steamid) % 2))) / 2;
					if (Convert.ToUInt64(Summary.response.players[0].steamid) % 2 == 1)
					{

						embed.AddField("SteamID", $"STEAM_0:1:{steamid}", true);
					}
					else
					{
						embed.AddField("SteamID", $"STEAM_0:0:{steamid}", true);
					}

					embed.AddField("SteamID64", Summary.response.players[0].steamid, true);

					if (Summary.response.players[0].communityvisibilitystate == 3)
					{
						embed.AddField("Profile Visibility", "Public", true);
					}
					else
					{
						embed.AddField("Profile Visibility", "Private", true);
					}

					if (Summary.response.players[0].gameextrainfo != null)                                                                                              //If user is in a game
					{
						embed.WithDescription($"In Game: {Summary.response.players[0].gameextrainfo}");
						embed.Color = new Color(144, 186, 60);
					}
					else if (Summary.response.players[0].personastate != 0)                                                                                             //If user is online in some fashion
					{
						embed.WithDescription(ProfileState[Summary.response.players[0].personastate]);
						embed.Color = new Color(84, 165, 196);
					}
					else                                                                                                                                                //If user is offline
					{
						embed.WithDescription(ProfileState[Summary.response.players[0].personastate]);
						embed.Color = new Color(127, 127, 127);
					}

					if (Summary.response.players[0].timecreated != 0)                                                                                                   //If user has a non-private TimeCreated
					{
						DateTimeOffset CreatedOn = DateTimeOffset.FromUnixTimeSeconds(Summary.response.players[0].timecreated);
						embed.WithFooter(x => x.WithText("Account Created On:"));
						embed.WithTimestamp(CreatedOn);
					}

					await ReplyAsync("", false, embed.Build());
				}
			}		
		}
	}
}
