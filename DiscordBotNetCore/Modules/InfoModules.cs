using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DiscordBot.Modules
{
	public class InfoModules : ModuleBase<SocketCommandContext>
	{
		//~info
		[Command("about")]
		[Summary("Says generic info about the bot.")]
		public async Task Info()
		{
			var embed = new EmbedBuilder();
			embed.WithAuthor($"About the Bot", Context.Client.CurrentUser.GetAvatarUrl(), "http://ko-fi.com/akujithesniper");
			embed.WithColor(Color.Gold);
			string info = $"Hello, I am a Discord bot written in **Discord.Net version {DiscordConfig.Version}**." +
						$"\nThis bot is currently running on: **{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})**.";
			embed.WithDescription(info);
			embed.WithFooter("Donate to me through Ko-fi by clicking the About the Bot in this embed (if you want).");
			await ReplyAsync("", false, embed.Build());
		}

		//~say
		[Command("say")]
		[Remarks("say [text]")]
		[Summary("Says what you tell it to say.")]
		public async Task Say([Remainder] string input)
		{
			await ReplyAsync($"`\"{input}\"` -{Context.User.Mention}");
		}

		[Command("admin")]
		[Summary("Lists out the admins for this bot.")]
		public async Task Admin()
		{
			Config c = Config.Load();
			string list = "";
			foreach (var person in c.Owners)
			{
				if (Context.Channel.GetUserAsync(person).Result != null)
				{
					list = list + "`" + Context.Channel.GetUserAsync(person).Result.Username + "`, ";
				}
			}
			list = list.Substring(0,list.Length-2);
			await ReplyAsync($"`Here's a list of our current admins:`\n\n{list}");
		}

		//just a test to see how changing a nickname works
		/*
		[Command("nickname")]
		[Summary("test")]
		public async Task Nickname()
		{
			await Context.Client.CurrentUser.ModifyAsync(x =>
			{
				x.Avatar = new Image(File.OpenRead("avatar.jpg"));
				x.Username = "";
			});
		}
		*/
	}
}