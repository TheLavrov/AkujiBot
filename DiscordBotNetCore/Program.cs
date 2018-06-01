using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot;
using System.Runtime.InteropServices;

namespace DiscordBot
{
	class Program
	{
		private readonly DiscordSocketClient _client;
		private readonly IServiceCollection _map = new ServiceCollection();
		private readonly CommandService _commands = new CommandService();

		public static void Main(string[] args) =>
			new Program().Start().GetAwaiter().GetResult();

		private Program()
		{
			_client = new DiscordSocketClient(new DiscordSocketConfig
			{
				// How much logging do you want to see?
				LogLevel = LogSeverity.Info,

				WebSocketProvider = WS4NetCore.WS4NetProvider.Instance,
				// If you or another service needs to do anything with messages
				// (eg. checking Reactions), you should probably
				// set the MessageCacheSize here.
				MessageCacheSize = 50,
			});
		}

		private static Task Logger(LogMessage message)
		{
			var cc = Console.ForegroundColor;
			switch (message.Severity)
			{
				case LogSeverity.Critical:
				case LogSeverity.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case LogSeverity.Warning:
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				case LogSeverity.Info:
					Console.ForegroundColor = ConsoleColor.White;
					break;
				case LogSeverity.Verbose:
				case LogSeverity.Debug:
					Console.ForegroundColor = ConsoleColor.DarkGray;
					break;
			}
			Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.ToString()}");
			Console.ForegroundColor = cc;
			return Task.CompletedTask;
		}

		public async Task Start()
		{
			Config.EnsureExists();

			_client.Log += Logger;

			await InitCommands();

			//Logging In
			await _client.LoginAsync(TokenType.Bot, Config.Load().Token);
			await _client.StartAsync();

			await _client.SetGameAsync(null);

			await Task.Delay(-1);
		}

		private IServiceProvider _services;

		private async Task InitCommands()
		{
			// Repeat this for all the service classes and other dependencies that your commands might need.
			_map.AddSingleton(new AudioService());

			// Either search the program and add all Module classes that can be found:
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
			// Or add Modules manually if you prefer to be a little more explicit:
			// await _commands.AddModuleAsync<InfoModule>();



			// When all your required services are in the collection, build the container.
			// Tip: There's an overload taking in a 'validateScopes' bool to make sure
			// you haven't made any mistakes in your dependency graph.
			_services = _map.BuildServiceProvider();

			// Subscribe a handler to see if a message invokes a command.
			_client.MessageReceived += HandleCommandAsync;
		}

		private async Task HandleCommandAsync(SocketMessage arg)
		{
			// Bail out if it's a System Message.
			var msg = arg as SocketUserMessage;
			if (msg == null) return;

			// Create a number to track where the prefix ends and the command begins
			int pos = 0;
			// Replace the '!' with whatever character
			// you want to prefix your commands with.
			// Uncomment the second half if you also want
			// commands to be invoked by mentioning the bot instead.
			if (msg.HasCharPrefix(Config.Load().Prefix, ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
			{
				// Create a Command Context
				var context = new SocketCommandContext(_client, msg);

				// Execute the command. (result does not indicate a return value, 
				// rather an object stating if the command executed succesfully).
				var result = await _commands.ExecuteAsync(context, pos, _services);

				// Uncomment the following lines if you want the bot
				// to send a message if it failed (not advised for most situations).
				if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
				    await msg.Channel.SendMessageAsync($"`{result.ErrorReason}`");
			}
		}

	}
}
