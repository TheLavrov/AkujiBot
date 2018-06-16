using Discord;
using Discord.Commands;
using Easy.Common;
using Easy.Common.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
	public class SalmonRun
	{
		public List<Detail> details { get; set; }
		public class Detail
		{
			public int start_time { get; set; }
			public int end_time { get; set; }
			public Stage stage { get; set; }
			public List<Weapon> weapons { get; set; }
			public class Stage
			{
				public string id { get; set; }
				public string name { get; set; }
				public string image { get; set; }
			}
			public class Weapon
			{
				public string thumbnail { get; set; }
				public Sub sub { get; set; }
				public string id { get; set; }
				public string image { get; set; }
				public string name { get; set; }
				public Special special { get; set; }
				public CoopSpecialWeapon coop_special_weapon { get; set; }
				public class Sub
				{
					public string name { get; set; }
					public string image_a { get; set; }
					public string id { get; set; }
					public string image_b { get; set; }
				}
				public class Special
				{
					public string image_b { get; set; }
					public string id { get; set; }
					public string name { get; set; }
					public string image_a { get; set; }
				}
				public class CoopSpecialWeapon
				{
					public string name { get; set; }
					public string image { get; set; }
				}
			}
		}
	}

	public class Splatoon
	{
		public List<Battle> regular { get; set; }
		public List<Battle> league { get; set; }
		public List<Battle> gachi { get; set; }
		public class Battle
		{
			public int end_time { get; set; }
			public Rule rule { get; set; }
			public StageA stage_a { get; set; }
			public StageB stage_b { get; set; }
			public int start_time { get; set; }
			public object id { get; set; }
			public GameMode game_mode { get; set; }
			public class Rule
			{
				public string key { get; set; }
				public string multiline_name { get; set; }
				public string name { get; set; }
			}
			public class StageA
			{
				public string name { get; set; }
				public string id { get; set; }
				public string image { get; set; }
			}
			public class StageB
			{
				public string image { get; set; }
				public string name { get; set; }
				public string id { get; set; }
			}
			public class GameMode
			{
				public string name { get; set; }
				public string key { get; set; }
			}
		}
	}

	public class SplatnetShop
	{
		public List<Merchandise> merchandises { get; set; }
		public class Merchandise
		{
			public string kind { get; set; }
			public int end_time { get; set; }
			public Skill skill { get; set; }
			public int price { get; set; }
			public string id { get; set; }
			public Gear gear { get; set; }
			public class Gear
			{
				public int rarity { get; set; }
				public string thumbnail { get; set; }
				public string image { get; set; }
				public string name { get; set; }
				public Brand brand { get; set; }
				public string id { get; set; }
				public string kind { get; set; }
				public class Brand
				{
					public FrequentSkill frequent_skill { get; set; }
					public string image { get; set; }
					public string name { get; set; }
					public string id { get; set; }
					public class FrequentSkill
					{
						public string name { get; set; }
						public string image { get; set; }
						public string id { get; set; }
					}
				}
			}
			public class Skill
			{
				public string name { get; set; }
				public string image { get; set; }
				public string id { get; set; }
			}
		}
	}

	public class SplatoonModule : ModuleBase<SocketCommandContext>
	{
		string uri = "https://splatoon2.ink";
		string nintendouri = "https://app.splatoon2.nintendo.net";

		//Current Stages
		[Command("splatoon")]
		[Remarks("splatoon [ranked, league]")]
		[Summary("Checks the current Splatoon 2 schedule. Type \"~splatoon ranked\" or \"~splatoon league\" to see the Ranked Battle or League Battle rotation respectively.")]
		public async Task Splatoon(string choice = null)
		{
			string splatoonurl = uri + "/data/schedules.json";

			using (IRestClient client = new RestClient())
			{
				splatoonurl = await client.GetStringAsync(splatoonurl);
				var splatoon = JsonConvert.DeserializeObject<Splatoon>(splatoonurl);

				var embed = new EmbedBuilder();
				var TimeLeft = DateTimeOffset.FromUnixTimeSeconds(splatoon.regular[0].end_time).LocalDateTime - DateTimeOffset.Now;

				if (choice == "ranked")
				{
					embed.WithAuthor("Ranked Battle Maps", null);
					embed.WithThumbnailUrl(nintendouri + splatoon.gachi[0].stage_a.image);

					string stageList = $"{splatoon.gachi[0].stage_a.name}, {splatoon.gachi[0].stage_b.name}";
					embed.AddField("Current Stages", stageList);
					embed.AddField("Current Gamemode", splatoon.gachi[0].rule.name);

					string stageListNext = $"{splatoon.gachi[1].stage_a.name}, {splatoon.gachi[1].stage_b.name}";
					embed.AddField("Next Stages", stageListNext);
					embed.AddField("Next Gamemode", splatoon.gachi[1].rule.name);

					embed.WithFooter($"Next rotation is in {(TimeLeft.Days * 24) + TimeLeft.Hours} hours, {TimeLeft.Minutes} minutes, and {TimeLeft.Seconds} seconds. Data is taken from the splatoon2.ink website.", null);
					embed.Color = new Color(255, 128, 0);
				}
				else if (choice == "league")
				{
					embed.WithAuthor("League Battle Maps", null);
					embed.WithThumbnailUrl(nintendouri + splatoon.league[0].stage_a.image);

					string stageList = $"{splatoon.league[0].stage_a.name}, {splatoon.league[0].stage_b.name}";
					embed.AddField("Current Stages", stageList);
					embed.AddField("Current Gamemode", splatoon.league[0].rule.name);

					string stageListNext = $"{splatoon.league[1].stage_a.name}, {splatoon.league[1].stage_b.name}";
					embed.AddField("Next Stages", stageListNext);
					embed.AddField("Next Gamemode", splatoon.league[1].rule.name);

					embed.WithFooter($"Next rotation is in {(TimeLeft.Days * 24) + TimeLeft.Hours} hours, {TimeLeft.Minutes} minutes, and {TimeLeft.Seconds} seconds. Data is taken from the splatoon2.ink website.", null);
					embed.Color = new Color(255, 128, 255);
				}
				else if (String.IsNullOrWhiteSpace(choice))
				{
					embed.WithAuthor("Turf War Maps", null);
					embed.WithThumbnailUrl(nintendouri + splatoon.regular[0].stage_a.image);

					string stageList = $"{splatoon.regular[0].stage_a.name}, {splatoon.regular[0].stage_b.name}";
					embed.AddField("Current Stages", stageList);
					embed.AddField("Current Gamemode", splatoon.regular[0].rule.name);

					string stageListNext = $"{splatoon.regular[1].stage_a.name}, {splatoon.regular[1].stage_b.name}";
					embed.AddField("Next Stages", stageListNext);
					embed.AddField("Next Gamemode", splatoon.regular[1].rule.name);

					embed.WithFooter($"Next rotation is in {(TimeLeft.Days * 24) + TimeLeft.Hours} hours, {TimeLeft.Minutes} minutes, and {TimeLeft.Seconds} seconds. Data is taken from the splatoon2.ink website.", null);
					embed.Color = new Color(33, 233, 22);
				}
				else
				{
					var message = await ReplyAsync("Map choice seems to be invalid. Showing regular maps.");
					embed.WithAuthor("Turf War Maps", null);
					embed.WithThumbnailUrl(uri + splatoon.regular[0].stage_a.image);

					string stageList = $"{splatoon.regular[0].stage_a.name}, {splatoon.regular[0].stage_b.name}";
					embed.AddField("Current Stages", stageList);
					embed.AddField("Current Gamemode", splatoon.regular[0].rule.name);

					string stageListNext = $"{splatoon.regular[1].stage_a.name}, {splatoon.regular[1].stage_b.name}";
					embed.AddField("Next Stages", stageListNext);
					embed.AddField("Next Gamemode", splatoon.regular[1].rule.name);

					embed.WithFooter($"Next rotation is in {(TimeLeft.Days * 24) + TimeLeft.Hours} hours, {TimeLeft.Minutes} minutes, and {TimeLeft.Seconds} seconds. Data is taken from the splatoon2.ink website.", null);
					embed.Color = new Color(33, 233, 22);
				}

				await ReplyAsync("", false, embed.Build());
			}
		}

		//Items on SplatNet2
		[Command("splatnet")]
		[Remarks("splatnet [item]")]
		[Summary("Checks the current Splatnet shop. After checking the current item list, type \"~splatnet\" followed by the ID associated with a certain item for more details.")]
		public async Task SplatnetShop(string choice = null)
		{
			string splatneturl = uri + "/data/merchandises.json";

			using (IRestClient client = new RestClient())
			{
				splatneturl = await client.GetStringAsync(splatneturl);
				var splatnet = JsonConvert.DeserializeObject<SplatnetShop>(splatneturl);

				int MoreInfo = -1;
				if (!String.IsNullOrWhiteSpace(choice))
				{
					for (int i = 0; i < splatnet.merchandises.Count; i++)
					{
						if (splatnet.merchandises[i].gear.id == choice)
						{
							MoreInfo = i;
						}
					}
				}

				var embed = new EmbedBuilder();

				if (MoreInfo == -1)             //look at the list of items on the shop
				{
					embed.WithAuthor("Current Splatnet2 Shop", null);
					foreach (var item in splatnet.merchandises)
					{
						embed.AddField(item.gear.name, item.gear.id, true);
					}
					embed.WithDescription($"Type \"~splatnet\" followed by the ID listed under an item to see more details.");
					embed.WithFooter($"Data is taken from the splatoon2.ink website.", null);
					embed.Color = new Color(33, 233, 22);
				}
				else                            //look at a specific item in the shop
				{
					embed.WithAuthor(splatnet.merchandises[MoreInfo].gear.name, null);
					embed.WithDescription($"{splatnet.merchandises[MoreInfo].gear.brand.name}");
					embed.WithThumbnailUrl(nintendouri + splatnet.merchandises[MoreInfo].gear.thumbnail);

					embed.AddField("Price", splatnet.merchandises[MoreInfo].price, true);
					embed.AddField("Main Skill", splatnet.merchandises[MoreInfo].skill.name, true);
					embed.AddField("Common Skill", splatnet.merchandises[MoreInfo].gear.brand.frequent_skill.name, true);

					var TimeLeft = DateTimeOffset.FromUnixTimeSeconds(splatnet.merchandises[MoreInfo].end_time).LocalDateTime - DateTimeOffset.Now;
					embed.WithFooter($"This item will leave the store in {TimeLeft.Hours} hours, {TimeLeft.Minutes} minutes, and {TimeLeft.Seconds} seconds. Data is taken from the splatoon2.ink website.", null);
					embed.Color = new Color(33, 233, 22);
				}

				await ReplyAsync("", false, embed.Build());
			}
		}

		//Salmon Run
		[Command("salmonrun")]
		[Summary("Checks if Salmon Run is open or closed.")]
		public async Task SalmonRun()
		{ 
			string salmonrunurl = uri + "/data/coop-schedules.json";
			var embed = new EmbedBuilder();

			using (IRestClient client = new RestClient())
			{
				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
				salmonrunurl = await client.GetStringAsync(salmonrunurl);
				var salmonrun = JsonConvert.DeserializeObject<SalmonRun>(salmonrunurl);
				
				bool IsLive = false;
				int latest = 0;
				for (int i = 0; i < salmonrun.details.Count; i++)
				{
					if (Context.Message.Timestamp.ToUnixTimeSeconds() >= salmonrun.details[i].start_time && Context.Message.Timestamp.ToUnixTimeSeconds() < salmonrun.details[i].end_time)
					{
						IsLive = true;
						latest = i;
						break;
					}
					if (Context.Message.Timestamp.ToUnixTimeSeconds() < salmonrun.details[i].start_time)
					{
						IsLive = false;
						latest = i;
						break;
					}
				}

				var ScheduleTimePST = DateTimeOffset.FromUnixTimeSeconds(salmonrun.details[latest].start_time);
				var TimeLeft = DateTimeOffset.FromUnixTimeSeconds(salmonrun.details[latest].end_time).LocalDateTime - DateTimeOffset.Now;
				var TimeUntil = DateTimeOffset.FromUnixTimeSeconds(salmonrun.details[latest].start_time).LocalDateTime - DateTimeOffset.Now;

				if (IsLive)
				{
					embed.WithAuthor("Salmon Run is live!", null);
					embed.WithThumbnailUrl(nintendouri + salmonrun.details[latest].stage.image);
					embed.WithDescription($"Time Started: {ScheduleTimePST.LocalDateTime} PST \nTime Left: {(TimeLeft.Days * 24) + TimeLeft.Hours} Hours, {TimeLeft.Minutes} Minutes, {TimeLeft.Seconds} Seconds");
					embed.AddField("Current Stage", salmonrun.details[latest].stage.name);
					string weaponList = "";
					foreach (var weapon in salmonrun.details[latest].weapons)
					{
						if (weapon != null)
							weaponList += weapon.name + '\n';
						else
							weaponList += "A Random Item?" + '\n';
					}
					embed.AddField("Weapons Given", weaponList);
					embed.WithFooter("Time is displayed in Pacific Standard Time (PST). Data is taken from the splatoon2.ink website.", null);
					embed.Color = new Color(144, 186, 60);
				}
				else
				{
					embed.WithAuthor("Salmon Run is down.", null);
					embed.WithThumbnailUrl(nintendouri + salmonrun.details[latest].stage.image);
					embed.WithDescription($"Next Run: {ScheduleTimePST.LocalDateTime} PST \nWill Start At: {(TimeUntil.Days * 24) + TimeUntil.Hours} Hours, {TimeUntil.Minutes} Minutes, {TimeUntil.Seconds} Seconds");
					embed.AddField("Next Stage", salmonrun.details[latest].stage.name);
					string weaponList = "";
					foreach (var weapon in salmonrun.details[latest].weapons)
					{
						if (!String.IsNullOrWhiteSpace(weapon.coop_special_weapon.name))
							weaponList += weapon.coop_special_weapon.name + '\n';
						else if (weapon != null)
							weaponList += weapon.name + '\n';
					}
					embed.AddField("Weapons Given", weaponList);
					embed.WithFooter("Time is displayed in Pacific Standard Time (PST). Data is taken from the splatoon2.ink website.", null);
					embed.Color = new Color(153, 0, 0);
				}

				await ReplyAsync("", false, embed.Build());
			}		
		}
	}
}
