using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DiscordBot
{
	/// <summary> 
	/// A file that contains information you either don't want public
	/// or will want to change without having to compile another bot.
	/// </summary>
	public class Config
	{
		[JsonIgnore]
		/// <summary> The location and name of your bot's configuration file. </summary>
		public static string FileName { get; private set; } = "config/configuration.json";
		/// <summary> Ids of users who will have owner access to the bot. </summary>
		public ulong[] Owners { get; set; }
		/// <summary> Ids of roles who can have special access to certain commands. </summary>
		public List<ulong> Moderators { get; set; } = new List<ulong>();
		/// <summary> Your bot's command prefix. </summary>
		public char Prefix { get; set; } = '~';
		/// <summary> Your bot's login token. </summary>
		public string Token { get; set; } = "";
		/// <summary> Your bot's volume for audio. </summary>
		public double Volume { get; set; } = 15;
		/// <summary> Needed for Twitter related commands. </summary>
		public string consumerKey { get; set; } = "";
		/// <summary> Needed for Twitter related commands. </summary>
		public string consumerSecret { get; set; } = "";
		/// <summary> Needed for Twitter related commands. </summary>
		public string userAccessToken { get; set; } = "";
		/// <summary> Needed for Twitter related commands. </summary>
		public string userAccessSecret { get; set; } = "";

		public static void EnsureExists()
		{
			string file = Path.Combine(AppContext.BaseDirectory, FileName);
			if (!File.Exists(file))                                 // Check if the configuration file exists.
			{
				string path = Path.GetDirectoryName(file);          // Create config directory if doesn't exist.
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				string[] readme = { "Info on the variables within configuration.json:",
									"\"Owners\": Adding your Discord ID to this list will allow you to use admin commands.",
									"\"Prefix\": What symbol you type before a command's name.",
									"\"Token\": Where your bot token goes.",
									"\"Volume\": Volume for audio-related commands.",
									"\"consumerKey\", \"consumerSecret\", \"userAccessToken\", \"userAccessSecret\": Used for Twitter commands like \"live\". If you don't care about Twitter LEAVE THESE BLANK."};
				File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "config/readme.txt"), readme);

				var config = new Config();                          // Create a new configuration object.

				Console.WriteLine("Please enter your token: ");
				string token = Console.ReadLine();                  // Read the bot token from console.

				config.Token = token;
				config.SaveJson();                                  // Save the new configuration object to file.
			}
			Console.WriteLine("Configuration Loaded");
		}

		/// <summary>
		/// Checks if an ID is in the admin list.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool IsAdmin(ulong id)
		{
			bool result = false;
			for (int i = 0; i < Owners.Length; i++ )
			{
				if (Owners[i] == id)
				{
					result = true;
				}
			}
			return result;
		}

		/// <summary> Save the configuration to the path specified in FileName. </summary>
		public void SaveJson()
		{
			string file = Path.Combine(AppContext.BaseDirectory, FileName);
			File.WriteAllText(file, ToJson());
		}

		/// <summary> Load the configuration from the path specified in FileName. </summary>
		public static Config Load()
		{
			string file = Path.Combine(AppContext.BaseDirectory, FileName);
			return JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
		}

		/// <summary> Convert the configuration to a json string. </summary>
		public string ToJson()
			=> JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}