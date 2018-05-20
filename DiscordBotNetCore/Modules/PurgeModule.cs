using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System;

namespace DiscordBot.Modules
{
	public class PurgeModule : ModuleBase<SocketCommandContext>
	{
		//~purge
		[Command("purge")]
		[Summary("Made to remove bot-related messages from the chat.")]
		public async Task Purge(string text = " ")
		{
			int count = 0;
			string s = "s";
			bool admin = true;																												//stay true for normal bot purging, change if admin permission is required
			ITextChannel channel = Context.Channel as ITextChannel;                                                                                          //Sets a variable up with info on the channel its called from
			ulong BotID = Context.Client.CurrentUser.Id;																				    //Gets the ID of the bot
			DateTimeOffset twoweeks = Context.Message.Timestamp.AddDays(-14);																//time from two weeks ago (Bots can't touch posts older than two weeks apparently)
			int dumpAmount = 100;
			List<IMessage> MsgsToDelete = new List<IMessage>();																				//Creates a new IMessage List for storing the messages to delete
			if (text.Contains("@all"))
				dumpAmount = 20;
			var dump = await channel.GetMessagesAsync(dumpAmount, CacheMode.AllowDownload, null).FlattenAsync();									//Gets the last bunch of messages from the channel (limit 50)

			if (string.IsNullOrWhiteSpace(text))
			{
				foreach (var content in dump)																								//for every message in the message dump...
				{
					if ((content.Author.Id == BotID || content.Content.StartsWith(Config.Load().Prefix)) && content.Timestamp > twoweeks)	//if the message has the bot's ID or the message starts with the command prefix...
					{
						MsgsToDelete.Add(content);																						    //add it to the IMessage List
					}
				}
			}
			else
			{
				admin = Config.Load().IsAdmin(Context.User.Id);																				//check if they're an admin
				if (!admin)																													//if not then they can't do this
				{
					await Context.Message.DeleteAsync();
					await ReplyAsync($"`Only admins can do this.`");
					return;
				}
				if (!text.Contains("@all"))
				{
					foreach (var content in dump)																							//for every message in the message dump...
					{
						foreach (var person in Context.Message.MentionedUsers)
						{
							if (content.Author.Id == person.Id && content.Timestamp > twoweeks)												//if the message has the user's ID and is young enough... 
							{
								MsgsToDelete.Add(content);																					//add it to the IMessage List
							}
						}

					}
				}
				else
				{
					foreach (var content in dump)                                                                           //for every message in the message dump...
					{
						if (content.Timestamp > twoweeks)																	//if the message is young enough...
						{
							MsgsToDelete.Add(content);																		//add it to the IMessage List
						}
					}
				}
				
			}

			if (MsgsToDelete.Count - 1 == 1) { s = ""; }                                                                        //This is just for formatting if it only deletes one message
																																//It's Count - 1 because we don't count the message that invoked the command
			if (MsgsToDelete.Count - 1 < 1)																						//If the list is less or equal to one... (the command call will always be in the list, honestly)
			{
				await channel.DeleteMessagesAsync(MsgsToDelete);
				var message = await ReplyAsync($"`I couldn't find anything to delete within 50 posts.`");
				await Task.Delay(1500);
				await message.DeleteAsync();                                                                                //Erase that message to keep tidy
			}
			else																											//If the list is bigger than one...
			{
				
				await channel.DeleteMessagesAsync(MsgsToDelete);
				count = MsgsToDelete.Count - 1;
				var message = await ReplyAsync($"`Deleted {count} message{s}.`");
				await Task.Delay(1500);
				await message.DeleteAsync();                                                                                //Erase that message to keep tidy
			}
		}
	}
}
