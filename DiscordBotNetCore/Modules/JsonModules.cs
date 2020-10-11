using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Xml;
using Easy.Common;
using Easy.Common.Interfaces;

namespace DiscordBot.Modules
{
	public class xkcd
	{
		public string news { get; set; }
		public string link { get; set; }
		public string img { get; set; }
		public int num { get; set; }
		public string day { get; set; }
		public string year { get; set; }
		public string month { get; set; }
		public string title { get; set; }
		public string alt { get; set; }
	};

	public class Posts
	{
		public int count { get; set; }
		public int offset { get; set; }
		public Post post { get; set; }
		public class Post
		{
			public string file_url { get; set; }
			public string id { get; set; }
		}
	}
	
	public class JsonModules : ModuleBase<SocketCommandContext>
	{
		//~gelbooru
		[Command("gelbooru")]
		[Remarks("gelbooru [tags]")]
		[Summary("Gets a random gelbooru image using the tags you provide. (NSFW Warning)")]
		public async Task Gelbooru([Remainder] string tagbulk)
		{
			string URL = "https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1&tags=";
			string[] tags = tagbulk.Split(' ');


			for (int i = 0; i < tags.Length; i++)
			{
				if (tags[i] == "e" || tags[i] == "explicit")
				{
					tags[i] = "rating:explicit";
				}
				else if (tags[i] == "s" || tags[i] == "safe")
				{
					tags[i] = "rating:safe";
				}
				else if (tags[i] == "q" || tags[i] == "questionable")
				{
					tags[i] = "rating:questionable";
				}
			}

			for (int i = 0; i < tags.Length; i++)
			{
				if (i == tags.Length - 1)
				{
					URL = URL + tags[i];
				}
				else
				{
					URL = URL + tags[i] + "+";
				}
			}

			using (IRestClient client = new RestClient())
			{
				string offsetURL = await client.GetStringAsync(URL);

				//converts XML into JSON (only XML has the count that we need, making this more annoying)
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(offsetURL);
				var xml = JsonConvert.SerializeXmlNode(doc.FirstChild.NextSibling, Newtonsoft.Json.Formatting.Indented, true).Replace("@", null);

				//converts the JSON to be useable
				var offsetgel = JsonConvert.DeserializeObject<Posts>(xml);
				Random rnd = new Random();
				int roll = rnd.Next(0, offsetgel.count);

				//take count from offsetgel, then api call again with a random pid
				string valueURL = URL + $"&pid={roll}";
				valueURL = await client.GetStringAsync(valueURL);
				doc.LoadXml(valueURL);
				xml = JsonConvert.SerializeXmlNode(doc.FirstChild.NextSibling, Newtonsoft.Json.Formatting.None, true).Replace("@", null);
				var valuegel = JsonConvert.DeserializeObject<Posts>(xml);

				var embed = new EmbedBuilder()
					.WithTitle("Gelbooru")
					.WithUrl($"https://gelbooru.com/index.php?page=post&s=view&id={valuegel.post.id}")
					.WithImageUrl(valuegel.post.file_url)
					.WithColor(new Color(114, 137, 218))
					;

				await ReplyAsync("", false, embed.Build());
			}
		}

		//~xkcd
		[Command("xkcd")]
		[Summary("Gets a random xkcd comic.")]
		public async Task xkcd()
		{
			string url = "https://xkcd.com/info.0.json";
			using (IRestClient client = new RestClient())
			{
				url = await client.GetStringAsync(url);

				xkcd comic = JsonConvert.DeserializeObject<xkcd>(url);
				Random rnd = new Random();
				int comicnum = rnd.Next(1, comic.num);

				string randomurl = ($"https://xkcd.com/{comicnum}/info.0.json");
				randomurl = await client.GetStringAsync(randomurl);
				xkcd rdmcomic = JsonConvert.DeserializeObject<xkcd>(randomurl);

				var embed = new EmbedBuilder()
					.WithAuthor(x => x.WithName(rdmcomic.title).WithIconUrl("https://xkcd.com/s/0b7742.png"))
					.WithDescription($"{rdmcomic.day}/{rdmcomic.month}/{rdmcomic.year}")
					.WithFooter(x => x.WithText(rdmcomic.alt))
					.WithImageUrl(rdmcomic.img)
					.WithColor(new Color(114, 137, 218))
					;

				await ReplyAsync("", false, embed.Build());
			}
		}
	}
}
