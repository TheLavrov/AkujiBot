using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class CountdownModule : ModuleBase<SocketCommandContext>
	{
		public class CountdownGroup
		{
			public Discord.WebSocket.ISocketMessageChannel channel;
			public DateTime countdown;
			public string description;
			public bool extra = false;
			public bool live = false;
			public bool running = false;
			public CountdownGroup(Discord.WebSocket.ISocketMessageChannel chnl, DateTime cd, string d = " ", string ex = " ")
			{
				channel = chnl;
				countdown = cd;
				description = d;
				running = true;
				if (ex.ToLower().Contains("majora"))
				{
					extra = true;
				}
				if (ex.ToLower().Contains("live"))
				{
					live = true;
				}
			}
		}

		public static ulong removalID = 0;
		public static bool live = false;
		public static List<CountdownGroup> countdownlist = new List<CountdownGroup>();

		[Command("countdown", RunMode = RunMode.Async)]
		[Remarks("countdown \"[name of countdown]\" \"[MM/DD/YYYY]\"")]
		[Summary("(ADMIN ONLY) Initializes a countdown. Leaving countdown blank will close any countdowns active. Writing \"countdown list\" will display all running countdowns for a channel. Make sure to put quotes around your date and name value.")]
		public async Task Countdown(string name = " ", string dateString = " ", string extra = " ")
		{
			if (name == "list")									//non admins can list countdowns just fine
			{
				var embed = new EmbedBuilder();
				embed.WithAuthor($"Currently running countdowns in {Context.Channel.Name}");
				embed.WithColor(Color.DarkBlue);
				var fail = true;
				foreach (var cdl in countdownlist)
				{
					if ((cdl.channel == Context.Channel || cdl.live) && cdl.running)
					{
						TimeSpan timeLeft = cdl.countdown - DateTime.Now;
						string days = "";
						string isLive = "";
						if (timeLeft > TimeSpan.FromDays(1))
						{
							days = $"{timeLeft.Days}:";
						}
						if (cdl.live)
						{
							isLive = $" (Live)";
						}
						embed.AddField(cdl.description + isLive, $"{days}{timeLeft.Hours.ToString("D2")}:{timeLeft.Minutes.ToString("D2")}:{timeLeft.Seconds.ToString("D2")}\n{cdl.countdown.ToString("f")}");
						fail = false;
					}
				}
				if (fail)
				{
					var message = await ReplyAsync($"`No countdowns are currently running on this channel.`");
					await Task.Delay(2000);
					await message.DeleteAsync();
					return;
				}
				await ReplyAsync("", false, embed.Build());
				return;
			}

			if (!Config.Load().IsAdmin(Context.User.Id))            //if they arent an admin after this point, turn them down
			{
				var message = await ReplyAsync($"`Only admins can use this command.`");
				await Task.Delay(2000);
				await message.DeleteAsync();
				return;
			}

			if (String.IsNullOrWhiteSpace(name))
			{
				foreach (var cd in countdownlist)
				{
					if (cd.channel == Context.Channel)
					{
						cd.running = false;
					}
				}
				removalID = Context.Channel.Id;
				var message = await ReplyAsync($"`Removing all countdowns in this channel. May take a bit if a live countdown is being removed.`");
				await Task.Delay(2000);
				await message.DeleteAsync();
				return;
			}

			if (!DateTime.TryParse(dateString, out DateTime countdown))
			{
				var message = await ReplyAsync($"`Error: Unable to recognize the time format inputted.`");
				await Task.Delay(2000);
				await message.DeleteAsync();
				return;
			}

			if (countdown <= DateTime.Now)
			{
				var message = await ReplyAsync($"`Error: This time already happened!`");
				await Task.Delay(2000);
				await message.DeleteAsync();
				return;
			}

			if (live && extra.Contains("live"))
			{
				var message = await ReplyAsync($"`Error: There is currently a live countdown already in use. Please wait for that to finish.`");
				await Task.Delay(2000);
				await message.DeleteAsync();
				return;
			}

			removalID = 0;
			var cdg = new CountdownGroup(Context.Channel, countdown, name, extra);
			countdownlist.Add(cdg);

			var MainThread = new Thread(async () =>
			{
				var message = await ReplyAsync($"`Countdown initialized.`");
				await Task.Delay(2000);
				await message.DeleteAsync();
				int majora = 0;
				var embed = new EmbedBuilder();
				embed.WithAuthor(cdg.description);
				bool sync = true;

				if (cdg.extra)
				{
					TimeSpan timeLeft;
					string display = "";
					TimeSpan sleeping = TimeSpan.FromMinutes(1);
					if (cdg.countdown <= DateTime.Now.AddHours(72))										//set majora int to avoid posting images pointlessly
					{
						majora++;
					}
					if (cdg.countdown <= DateTime.Now.AddHours(48))
					{
						majora++;
					}
					if (cdg.countdown <= DateTime.Now.AddHours(24))
					{
						majora++;
					}

					while (cdg.running && removalID != cdg.channel.Id)
					{
						if (cdg.live)                                                                   // if user puts both majora and live (majoralive)
						{
							live = true;
							timeLeft = cdg.countdown - DateTime.Now;
							if (timeLeft > TimeSpan.FromDays(1))
							{
								display = $"{timeLeft.Days + 1} Days until {cdg.description}";
								await Context.Client.SetGameAsync($"{display}", null, ActivityType.Watching);
							}
							else if (timeLeft > TimeSpan.FromHours(10))
							{
								display = $"{timeLeft.Hours + 1} Hours until {cdg.description}";
								await Context.Client.SetGameAsync($"{display}", null, ActivityType.Watching);
							}
							else
							{
								sleeping = TimeSpan.FromSeconds(1);
								display = $"{timeLeft.Hours.ToString("D2")}:{timeLeft.Minutes.ToString("D2")}:{timeLeft.Seconds.ToString("D2")} until {cdg.description}";
								await Context.Client.SetGameAsync($"{display}", null, ActivityType.Watching);
							}

							if (sync)
							{
								Thread.Sleep(TimeSpan.FromSeconds(60 - DateTime.Now.Second));               //try to sync updating with start of a new minute (will probably be slightly off)
								sync = false;
							}
						}

						if (cdg.countdown <= DateTime.Now.AddHours(72) && majora == 0)					//post majora images before 3 days, 2 days, and 1 day
						{
							embed.WithImageUrl("https://i.imgur.com/8FHn7Dy.jpg");
							await cdg.channel.SendMessageAsync("", false, embed.Build());
							majora++;
						}
						else if (cdg.countdown <= DateTime.Now.AddHours(48) && majora == 1)
						{
							embed.WithImageUrl("https://i.imgur.com/O4GopWd.jpg");
							await cdg.channel.SendMessageAsync("", false, embed.Build());
							majora++;
						}
						else if (cdg.countdown <= DateTime.Now.AddHours(24) && majora == 2)
						{
							embed.WithImageUrl("https://i.imgur.com/ljBULJb.jpg");
							await cdg.channel.SendMessageAsync("", false, embed.Build());
							majora++;
						}
						else if (cdg.countdown <= DateTime.Now)
						{
							embed.WithImageUrl("https://i.imgur.com/Nu3dray.png");
							embed.WithDescription($"The countdown has finished.");
							embed.WithColor(Color.Green);
							await cdg.channel.SendMessageAsync("", false, embed.Build());
							break;
						}

						if (cdg.live)									//make sure sleep checks out if live is in use
							Thread.Sleep(sleeping);
						else
							Thread.Sleep(5000);
					}
					live = false;
					await Context.Client.SetGameAsync(null);
					cdg.running = false;
				}
				else if (cdg.live)										//this is if they only typed live
				{
					live = true;
					TimeSpan timeLeft;
					string display = "";
					TimeSpan sleeping = TimeSpan.FromMinutes(1);
					while (cdg.running && removalID != cdg.channel.Id)
					{
						timeLeft = cdg.countdown - DateTime.Now;
						if (timeLeft > TimeSpan.FromDays(1))
						{
							display = $"{timeLeft.Days + 1} Days until {cdg.description}";
							await Context.Client.SetGameAsync($"{display}", null, ActivityType.Watching);
						}
						else if (timeLeft > TimeSpan.FromHours(10))
						{
							display = $"{timeLeft.Hours + 1} Hours until {cdg.description}";
							await Context.Client.SetGameAsync($"{display}", null, ActivityType.Watching);
						}
						else
						{
							sleeping = TimeSpan.FromSeconds(1);
							display = $"{timeLeft.Hours.ToString("D2")}:{timeLeft.Minutes.ToString("D2")}:{timeLeft.Seconds.ToString("D2")} until {cdg.description}";
							await Context.Client.SetGameAsync($"{display}", null, ActivityType.Watching);
						}

						if (sync)
						{
							Thread.Sleep(TimeSpan.FromSeconds(60 - DateTime.Now.Second));               //try to sync updating with start of a new minute (will probably be slightly off)
							sync = false;
						}

						if (cdg.countdown <= DateTime.Now)
						{
							embed.WithDescription($"Ring Ring! The countdown has finished!");
							embed.WithImageUrl("https://i.imgur.com/MQEr5Mp.png");
							embed.WithColor(Color.Green);
							await cdg.channel.SendMessageAsync("", false, embed.Build());
							break;
						}
						Thread.Sleep(sleeping);
					}
					live = false;
					await Context.Client.SetGameAsync(null);
					cdg.running = false;
				}
				else
				{
					while (cdg.running && removalID != cdg.channel.Id)
					{
						Thread.Sleep(5000);
						if (cdg.countdown <= DateTime.Now)
						{
							embed.WithDescription($"Ring Ring! The countdown has finished!");
							embed.WithImageUrl("https://i.imgur.com/MQEr5Mp.png");
							embed.WithColor(Color.Green);
							await cdg.channel.SendMessageAsync("", false, embed.Build());
							break;
						}
					}
					cdg.running = false;
				}
			countdownlist.RemoveAll(x => x.running == false);
			});
			MainThread.Start();
			MainThread.IsBackground = true;
		}
	}
}
