using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using System.Collections.Generic;

namespace DiscordBot.Modules
{
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		//~help
		private CommandService _service;

		public HelpModule(CommandService service)           // Create a constructor for the commandservice dependency
		{
			_service = service;
		}

		[Command("help")]
		[Remarks("help [command]")]
		[Summary("Gives you a list of available commands. (Why are you looking for help on the help command?)")]
		public async Task HelpSimple(string command = null)
		{
			string summary = null;
			List<string> commands = new List<string>();
			if (string.IsNullOrWhiteSpace(command))																					//////start of help with no command
			{
				foreach (var module in _service.Modules)																			//for every module in the bot...
				{
					foreach (var cmd in module.Commands)																			//for every command in said modules...
					{
						var result = await cmd.CheckPreconditionsAsync(Context);													//check if a name of a command is there
						if (result.IsSuccess)
							commands.Add($"`{cmd.Aliases.First()}` ");																//if so, add it to the "summary" string
					}
				}

				commands.Sort();

				if (commands.Count() != 0)																							//if summary isn't empty
				{
					foreach (var text in commands)
						summary += text;
					await ReplyAsync($"These are the commands you can use: \n{summary}");											//post the summary to the chat
				}
			}
			else																													//////start of help with command
			{
				bool found = false;																									//start with a false boolean for the search
				foreach (var module in _service.Modules)																			//for every module in the bot...
				{
					foreach (var cmd in module.Commands)                                                                            //for every command in said modules...
					{
						if (cmd.Name == command)                                                                                    //check if the current command's name matches the user's input
						{
							if (string.IsNullOrEmpty(cmd.Remarks))                                                                  //if it has remarks as well, post the help command template with remarks
								await ReplyAsync($"`{cmd.Name}`\n`{cmd.Summary}`");
							else                                                                                                    //if it doesn't have remarks, post the help command template without remarks
								await ReplyAsync($"`{cmd.Name}`\n`{cmd.Remarks}`\n`{cmd.Summary}`");
							found = true;                                                                                           //make found true to show that we got the command successfully
							break;                                                                                                  //exit the foreach to avoid unnecessary looping
						}
					}

					if (found)                                                                                                      //if found is true we can break out of this foreach as well
						break;

				}

				if (!found)                                                                                                         //if we never found the command, notify the user
					await ReplyAsync($"`Unable to find the command you wanted help with.`");
			}
		}
	}
}
