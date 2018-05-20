using System.Threading.Tasks;
using Discord.Commands;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Xml;
using Discord;
using Easy.Common.Interfaces;
using Easy.Common;
using System;

namespace DiscordBot.Modules
{
	public class MarioMakerModule : ModuleBase<SocketCommandContext>
	{
		public class Mariomaker
		{
			public string Code { get; set; }
			public string Title { get; set; }
			public string Cover { get; set; }
			public string Map { get; set; }
			public string Type { get; set; }
			public string Difficulty { get; set; }
			public string Keyword { get; set; }
			public string Created { get; set; }
			public Statistics statistics { get; set; }
			public Creator creator { get; set; }
			public First first { get; set; }
			public Fastest fastest { get; set; }

			public class Statistics
			{
				public string Tried { get; set; }
				public string Solved { get; set; }
				public string Played { get; set; }
				public string Rated { get; set; }
				public string Shared { get; set; }
				public string Clearrate { get; set; }
				public string Comments { get; set; }
			}

			public class User
			{
				public string Name { get; set; }
				public string Avatar { get; set; }
				public string Url { get; set; }
				public string Duration { get; set; }
			}

			public class Creator
			{
				public User User { get; set; }
			}

			public class First
			{
				public User User { get; set; }
			}

			public class Fastest
			{
				public User User { get; set; }
			}
		}

		string uri = "http://www.blar.de/smm/fetch?code=";
		string mariomakeruri = "https://supermariomakerbookmark.nintendo.net/courses/";

		[Command("bookmark")]
		[Summary("Gives more details on a Mario Maker Bookmark page.")]
		public async Task Bookmark(string bookmark)
		{
			bookmark = Regex.Replace(bookmark, "[^a-zA-Z0-9]", "");
			if (bookmark.Length == 16)
			{
				bookmark = bookmark.Substring(0, 4) + "-" + bookmark.Substring(4, 4) + "-" + bookmark.Substring(8, 4) + "-" + bookmark.Substring(12, 4);
			}
			else
			{
				var message = await ReplyAsync($"`Unable to use the value given.`");
				await Task.Delay(15000);
				await message.DeleteAsync();
				return;
			}
			string url = uri + bookmark.ToUpper();

			using (IRestClient client = new RestClient())
			{
				string xml = await client.GetStringAsync(url);

				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xml);
				var jsonText = JsonConvert.SerializeXmlNode(doc.FirstChild.NextSibling, Newtonsoft.Json.Formatting.Indented, true).Replace("@", null).Replace("clear-rate","clearrate");
				var mariomaker = JsonConvert.DeserializeObject<Mariomaker>(jsonText);
				var code = mariomaker.Code;

				if (!String.IsNullOrWhiteSpace(mariomaker.Code))
				{
					var embed = new EmbedBuilder()
					.WithAuthor(mariomaker.creator.User.Name, mariomaker.creator.User.Avatar)
					.WithThumbnailUrl(mariomaker.Cover)
					.WithColor(new Color(000000))
					.AddField("Clears", $"{mariomaker.statistics.Solved} / {mariomaker.statistics.Tried} ({mariomaker.statistics.Clearrate}%)", true)
					.AddField("Fastest Time Cleared", mariomaker.fastest.User.Duration, true)
					.WithImageUrl(mariomaker.Map)
					.WithTitle(mariomaker.Title)
					.WithUrl(mariomakeruri + mariomaker.Code);

					if (!String.IsNullOrWhiteSpace(mariomaker.Keyword))
						embed.WithDescription(mariomaker.Keyword);

					switch (mariomaker.Difficulty)
					{
						case "Easy":
							embed.WithColor(new Color(40, 173, 138));
							break;
						case "Normal":
							embed.WithColor(new Color(38, 145, 188));
							break;
						case "Expert":
							embed.WithColor(new Color(234, 52, 139));
							break;
						case "Super Expert":
							embed.WithColor(new Color(255, 69, 69));
							break;
						default:
							break;
					}

					await ReplyAsync("", false, embed.Build());
				}
				else
				{
					var message = await ReplyAsync($"There was an error with the value you provided.");
					await Task.Delay(1500);
					await message.DeleteAsync();
				}
			}
		}
	}
}
