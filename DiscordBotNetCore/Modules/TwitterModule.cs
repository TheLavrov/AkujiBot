using Discord.Commands;
using System.Threading.Tasks;
using Tweetinvi;
using Discord;
using DiscordBot.Attributes;
using System;
using Tweetinvi.Streaming;
using System.Threading;
using System.Collections.Generic;
using Tweetinvi.Models;
using System.Linq;

namespace DiscordBot.Modules
{
	public class TwitterModule : ModuleBase<SocketCommandContext>
	{
		public class TwitterFeed
		{
			public string twitterUser;
			public List<Discord.WebSocket.ISocketMessageChannel> channels = new List<Discord.WebSocket.ISocketMessageChannel>();

			public TwitterFeed(string twitteruser, Discord.WebSocket.ISocketMessageChannel channel)
			{
				twitterUser = twitteruser;
				channels.Add(channel);
			}
		}

		private readonly string consumerKey = Config.Load().consumerKey;
		private readonly string consumerSecret = Config.Load().consumerSecret;
		private readonly string userAccessToken = Config.Load().userAccessToken;
		private readonly string userAccessSecret = Config.Load().userAccessSecret;

		private IFilteredStream stream;
		public static bool running = false;
		public static List<TwitterFeed> twitterFeeds = new List<TwitterFeed>();

		public void TweetTask(string text, bool displayMsg = true)
		{
			RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;                                                   //to avoid ratelimits, let Tweetinvi handle it
            TweetinviConfig.CurrentThreadSettings.TweetMode = TweetMode.Extended;

            var embed = new EmbedBuilder();

			stream = Stream.CreateFilteredStream();                                                                             //create a stream for the live feed

            stream.ClearFollows();
			foreach (var feed in twitterFeeds.Select(x => x.twitterUser).Distinct())
				stream.AddFollow(User.GetUserFromScreenName(feed));						                						//set the stream to follow said user(s)


			stream.MatchingTweetReceived += async (sender, args) =>
			{
				var tweet = args.Tweet;

				foreach (var feed in twitterFeeds)
				{
					if (tweet.CreatedBy.ScreenName == feed.twitterUser)											//if the twitter screen name matches a saved screen name...
					{
						embed = new EmbedBuilder();
						embed.WithAuthor($"{tweet.CreatedBy.Name} (@{tweet.CreatedBy.ScreenName})", tweet.CreatedBy.ProfileImageUrl, tweet.Url);
						embed.WithDescription(tweet.FullText);
						embed.WithColor(0, 172, 237);     //Twitter blue

						if (tweet.Media.Count > 0)
							embed.WithImageUrl(tweet.Media[0].MediaURL);

						foreach (var channel in feed.channels)													//post the embedded tweet in every channel
							await channel.SendMessageAsync("", false, embed.Build());
					}
					
				}
			};

			stream.StreamStopped += (sender, args) =>
			{
                if (args.Exception != null)
                {
                    Console.WriteLine(args.Exception);
                }
            };

			var MainThread = new Thread(() => stream.StartStreamMatchingAnyConditionAsync());
			MainThread.Start();
			MainThread = new Thread( async() =>
			{
				if (!String.IsNullOrWhiteSpace(text) && displayMsg)
				{
					var msg = await ReplyAsync($"`Live feed is now hooked into @{text}.`");
					await Task.Delay(2000);
					await msg.DeleteAsync();
				}
				else if (!displayMsg)
				{
					var msg = await ReplyAsync($"`The live feed of @{text} has been removed.`");
					await Task.Delay(2000);
					await msg.DeleteAsync();
				}
				
				while (true)
				{
					if (!running)
					{
						stream.StopStream();
						stream.ClearFollows();
						break;
					}
				}
			});
            MainThread.IsBackground = true;
            MainThread.Start();			
		}

		//~live
		[Command("live", RunMode = RunMode.Async), Ratelimit(1, 1.0/6.0, Measure.Minutes)]
		[Remarks("live [user]")]
		[Summary("(ADMIN ONLY) Creates a live Twitter feed for a Twitter user. Additional users can be added. Type the command by itself again to stop it.")]
		public async Task Live(string text = " ")
		{
			if (!Config.Load().IsAdmin(Context.User.Id))			//if they arent an admin, turn them down
			{
				var message = await ReplyAsync($"`Only admins can use this command.`");
				await Task.Delay(2000);
				await message.DeleteAsync();
				return;
			}

			//if they dont have twitter credentials
			if (String.IsNullOrWhiteSpace(Config.Load().consumerKey) || String.IsNullOrWhiteSpace(Config.Load().consumerSecret) || String.IsNullOrWhiteSpace(Config.Load().userAccessSecret) || String.IsNullOrWhiteSpace(Config.Load().userAccessToken))
			{
				var message = await ReplyAsync($"`Your config is not set to use Twitter commands. You need to get necessary keys to use this and add them to your configuration.json file.`");
				await Task.Delay(2000);
				await message.DeleteAsync();
				return;
			}

			if (String.IsNullOrWhiteSpace(text) && !running)							//if nothing is running and they left the field blank, turn them down
			{
				var message = await ReplyAsync($"`Error: No name entered.`");
				await Task.Delay(2000);
				await message.DeleteAsync();
				return;
			}

			Auth.SetUserCredentials(consumerKey, consumerSecret, userAccessToken, userAccessSecret);                               //initialize credentials

            if (!running)
			{
				var message = await ReplyAsync($"`Live feed will now commence for @{text}. Setting it up now...`");
				await Task.Delay(2000);
				await message.DeleteAsync();
				running = true;
				twitterFeeds.Add(new TwitterFeed(text, Context.Channel));
				TweetTask(text);
				return;
			}
			else if (!String.IsNullOrWhiteSpace(text))
			{
				running = false;

				bool temp = false;              //track if screen name is not in use yet
				bool removed = false;			//track if screen name is being removed
				foreach(var feed in twitterFeeds)
				{
					if (text == feed.twitterUser)								//if the screen name is already in the list...
					{
						if (!feed.channels.Contains(Context.Channel))							//if the commander's channel isn't in the list, add it
						{
							feed.channels.Add(Context.Channel);
							var message = await ReplyAsync($"`Adding @{text} to the live feed watchlist for this channel...`");
							await Task.Delay(2000);
							await message.DeleteAsync();
						}
						else																	//if the commander's channel is in the list, remove it
						{
							feed.channels.Remove(Context.Channel);
							var message = await ReplyAsync($"`Removing @{text} from the live feed watchlist for this channel...`");
							await Task.Delay(2000);
							await message.DeleteAsync();
							removed = true;
						}
						temp = true;
					}
				}

				twitterFeeds.RemoveAll(x => x.channels.Count == 0);								//clear out any blank channels to avoid unneeded searching

				if (!temp)
				{
					twitterFeeds.Add(new TwitterFeed(text, Context.Channel));
					var message = await ReplyAsync($"`Adding @{text} to the live feed watchlist for this channel.`");
					await Task.Delay(2000);
					await message.DeleteAsync();
				}

				if (twitterFeeds.Count != 0 && !removed)
				{
					running = true;
					TweetTask(text);
					return;
				}
				else if (twitterFeeds.Count != 0 && removed)
				{
					running = true;
					TweetTask(text, false);
					return;
				}
				else
				{
					var msg = await ReplyAsync($"`The live feed of @{text} has been removed.`");
					await Task.Delay(2000);
					await msg.DeleteAsync();
					return;
				}
			}
			else
			{
				running = false;
				foreach (var feed in twitterFeeds)													
				{
					feed.channels.Remove(Context.Channel);
				}

				twitterFeeds.RemoveAll(x => x.channels.Count == 0);                                //clear out any blank channels to avoid unneeded searching

				var msg = await ReplyAsync($"`Removed all live feeds for this channel.`");
				await Task.Delay(2000);
				await msg.DeleteAsync();
				if (twitterFeeds.Count != 0)
				{
					running = true;
					TweetTask(text);
				}
				return;
			}
		}
	}
}
