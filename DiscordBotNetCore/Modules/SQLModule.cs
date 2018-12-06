using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace DiscordBot.Modules
{
    public class SQLModule : ModuleBase<SocketCommandContext>
    {
		private static SqliteConnection SqlConnect()
		{
			string filename = Path.Combine(AppContext.BaseDirectory, "config/database.sqlite");

			if (!File.Exists(filename)) // create the directory if it doesn't exist
			{
				string dir = Path.GetDirectoryName(filename);
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}

			return new SqliteConnection("" + new SqliteConnectionStringBuilder { DataSource = $"{ filename }" });
		}

		/*
		 * Executes a query for a SQLite connection.
		 */
		private static SqliteDataReader SqlQuery(SqliteConnection connection, string query)
		{
			var command = connection.CreateCommand();
			command.CommandText = query;

			return command.ExecuteReader();
		}

		private static SqliteDataReader SqlPQuery(SqliteConnection connection, string query, params object[] args)
		{
			var command = connection.CreateCommand();
			command.CommandText = query;
			for(int i = 0; i < args.Length; i++)
			{
				command.Parameters.AddWithValue("@" + (i + 1), args[i]);
			}

			return command.ExecuteReader();
		}

		// SQL query for creating the friendcode table
		private static string QUERY_FC_TABLE = @"
			CREATE TABLE IF NOT EXISTS friendcode (
				id ULONG PRIMARY KEY,
				fcswitch VARCHAR DEFAULT NULL,
				fc3ds VARCHAR DEFAULT NULL,
				fcwiiu VARCHAR DEFAULT NULL,
				hidden BOOLEAN DEFAULT 1
			);
		";

		// filename for ~fcdump executions
		private static string DUMPNAME = Path.Combine(AppContext.BaseDirectory, "config/dump.txt");

		[Command("fcadd")]
		[Remarks("fcadd [switch, 3ds, wiiu] [12 digit code]")]
		[Summary("Allows you to add a friend code to yourself.")]
		public async Task FriendCodeAdd(string choice, [Remainder] string fc)
		{
			string FileName = "config/database.sqlite";
			choice = choice.ToLower();
			
			if(choice == "switch" || choice == "3ds")
				fc = Regex.Replace(fc, "[^0-9]", "");
			
			if (((choice == "switch" || choice == "3ds") && fc.Length == 12) || choice == "wiiu")
			{
                string file = Path.Combine(AppContext.BaseDirectory, FileName);
				if (!File.Exists(file))                                                                                                 // Check if the configuration file exists.
				{
					string path = Path.GetDirectoryName(file);                                                                          // Create config directory if doesn't exist.
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);
				}

				var sqlconn = SQLModule.SqlConnect();
				sqlconn.Open();

				// table creation query
				SQLModule.SqlQuery(sqlconn, QUERY_FC_TABLE);

				var reader = SQLModule.SqlPQuery(sqlconn, "SELECT * FROM friendcode WHERE id=@1;", Context.User.Id);
				
				bool exists;
				using (reader)
				{
					exists = reader.HasRows;
					reader.Close();
				}

				// executing the query
				
				if (!exists) // if not exists, we insert new
				{
					SQLModule.SqlPQuery(sqlconn,
						$"INSERT INTO friendcode (fc{choice}, id) VALUES (@1, @2);",
						fc, Context.User.Id
					);
				}
				else // if exists, we update current
				{
					SQLModule.SqlPQuery(sqlconn,
						$"UPDATE friendcode SET fc{choice} = @1 WHERE id = @2;",
						fc, Context.User.Id
					);
				}

				sqlconn.Close();
				var message = await ReplyAsync($"`Friend code saved.`");
			}
			else
			{
				var message = await ReplyAsync($"`Error: Parameters are invalid.`");
			}
		}

		[Command("fc")]
		[Remarks("fc <optional user mention>")]
		[Summary("Allows you to view your saved friend codes, or someone's on the server with a mention if they've added one.")]
		public async Task FriendCode(string text = " ")
		{
			string FileName = "config/database.sqlite";
			bool success = false;
			ulong search = Context.User.Id;
			string name = Context.User.Username;
			string avatarurl = Context.User.GetAvatarUrl();

			if (Context.Message.MentionedUsers.Count > 0)																					//if anyone is mentioned with the command then they want the friend codes of that person, not their own
			{
				search = Context.Message.MentionedUsers.ElementAt(0).Id;
				name = Context.Message.MentionedUsers.ElementAt(0).Username;
				avatarurl = Context.Message.MentionedUsers.ElementAt(0).GetAvatarUrl();
			}

			string file = Path.Combine(AppContext.BaseDirectory, FileName);
			if (!File.Exists(file))																											// Check if the configuration file exists.
			{
				string path = Path.GetDirectoryName(file);																					// Create config directory if doesn't exist.
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
			}

			var m_dbConnection = new SqliteConnection("" + new SqliteConnectionStringBuilder { DataSource = $"{file}" });  // Create connection
			m_dbConnection.Open();                                                                                                          // Open

			string query = "CREATE TABLE IF NOT EXISTS friendcode (id ULONG PRIMARY KEY, fcswitch VARCHAR DEFAULT NULL, fc3ds VARCHAR DEFAULT NULL, fcwiiu VARCHAR DEFAULT NULL)";
			var command = m_dbConnection.CreateCommand();
			command.CommandText = query;
			command.ExecuteNonQuery();

			query = $"SELECT * FROM friendcode WHERE id={search}";                                                   // run command to find if id is already associated
			command = m_dbConnection.CreateCommand();
			command.CommandText = query;
			command.ExecuteNonQuery();
			bool exists;
			using (var reader = command.ExecuteReader())
			{
				exists = reader.HasRows;
				reader.Close();
			}

			var embed = new EmbedBuilder()
				.WithAuthor(name)
				.WithThumbnailUrl(avatarurl)
				.WithColor(new Color(16647499))
				.WithTitle("__Friend Codes__");
			
			if (exists)
			{
				query = $"SELECT * FROM friendcode WHERE id={search}";
				command = m_dbConnection.CreateCommand();
				command.CommandText = query;
				using (var rdr = command.ExecuteReader())
				{
					while (rdr.Read())
					{
						if (!String.IsNullOrWhiteSpace(rdr["fcswitch"].ToString()))
						{
							string temp = rdr["fcswitch"].ToString();
							temp = "SW-" + temp.Substring(0, 4) + "-" + temp.Substring(4, 4) + "-" + temp.Substring(8, 4);
							embed.AddField("Switch", temp, true);
							success = true;
						}
						if (!String.IsNullOrWhiteSpace(rdr["fc3ds"].ToString()))
						{
							string temp = rdr["fc3ds"].ToString();
							temp = temp.Substring(0, 4) + "-" + temp.Substring(4, 4) + "-" + temp.Substring(8, 4);
							embed.AddField("3DS", temp, true);
							success = true;
						}
						if (!String.IsNullOrWhiteSpace(rdr["fcwiiu"].ToString()))
						{
							string temp = rdr["fcwiiu"].ToString();
							embed.AddField("Wii U", temp, true);
							success = true;
						}
					}
					rdr.Close();
				}
				
				m_dbConnection.Close();                                                                                                 // close connection

				if (success)
				{
					await ReplyAsync("", false, embed.Build());
				}
				else
				{
					var message = await ReplyAsync($"`Error: No friend code data was found with your selection.`");
				}
			}
			else
			{
				m_dbConnection.Close();                                                                                                 // close connection
				var message = await ReplyAsync($"`Error: No friend code data was found with your selection.`");
			}
        }

		[Command("fchide")]
		[Remarks("fchide")]
		[Summary("Toggles the visibility of your friend codes.")]
		public async Task FriendCodeHidden()
		{
			var id = Context.User.Id;

			using(var sqlconn = SQLModule.SqlConnect())
			{
				sqlconn.Open();
				using(var response = SQLModule.SqlQuery(sqlconn, "SELECT hidden FROM friendcode;"))
				{
					if(response.HasRows)
					{
						if(response.GetBoolean(response.GetOrdinal("hidden")))
						{
							SQLModule.SqlPQuery(sqlconn, "UPDATE friendcode SET hidden = 0 WHERE id = @1;", id);
							await ReplyAsync("`Set friend code visibility to PUBLIC`");
						}
						else
						{
							SQLModule.SqlPQuery(sqlconn, "UPDATE friendcode SET hidden = 1 WHERE id = @1;", id);
							await ReplyAsync("`Set friend code visibility to PRIVATE`");
						}
					}
				}
			}
		}

        [Command("fcdump")]
        [Remarks("fcdump")]
        [Summary("DMs a list of saved friend codes to you.")]
        public async Task FriendCodeDump()
        {
			string query;
			if (Config.Load().IsAdmin(Context.User.Id))
			{
				query = "SELECT * FROM friendcode;"; // get all friendcodes (for admins)
			}
			else
			{
				query = "SELECT * FROM friendcode WHERE hidden = 0;"; // get only public friendcodes
			}

			// do query
			
			using(var sqlconn = SQLModule.SqlConnect())
			{
				sqlconn.Open();

                
                using (var reader = SQLModule.SqlQuery(sqlconn, query))
				{
					List<string> users = new List<string>();
                    string temp = $"Public Friend Codes from {Context.Channel.Name}:\r\n";
                    while (reader.Read())
                    {
                        string name = reader["id"].ToString();
                        foreach (var user in Context.Guild.Users)
                        {
                            if (name == user.Id.ToString())
                            {
                                name = user.Username;
                                if (!String.IsNullOrWhiteSpace(user.Nickname))
                                {
                                    name = name + $" ({user.Nickname})";
                                }
                            }
                        }

                        temp += $"{name}\r\n";

                        if (!String.IsNullOrWhiteSpace(reader["fcswitch"].ToString()))
                        {
                            string tempSwitch = reader["fcswitch"].ToString();
                            tempSwitch = "Switch: SW-" + tempSwitch.Substring(0, 4) + "-" + tempSwitch.Substring(4, 4) + "-" + tempSwitch.Substring(8, 4) + "\r\n";
                            temp += tempSwitch;
                        }
                        if (!String.IsNullOrWhiteSpace(reader["fc3ds"].ToString()))
                        {
                            string temp3DS = reader["fc3ds"].ToString();
                            temp3DS = "3DS: " + temp3DS.Substring(0, 4) + "-" + temp3DS.Substring(4, 4) + "-" + temp3DS.Substring(8, 4) + "\r\n";
                            temp += temp3DS;
                        }                            
                        if (!String.IsNullOrWhiteSpace(reader["fcwiiu"].ToString()))
                            temp += $"Wii U: {reader["fcwiiu"].ToString()}\r\n";

                        temp += "\r\n";

						users.Add(temp);
					}

					File.WriteAllLines(DUMPNAME, users.ToArray());

					await Context.User.SendFileAsync(DUMPNAME);
				}
			}
		}

		[Command("fcfixdb")]
		[Remarks("fcfixdb")]
		[Summary("A temporary command to fix the database")]
		public async Task FriendCodeFixDatabase()
		{
			if(!Config.Load().IsAdmin(Context.User.Id)) return; // block non-admins

			const string query = "PRAGMA table_info(friendcode);";

			using(var sqlconn = SQLModule.SqlConnect())
			{
				sqlconn.Open();

				using(var reader = SQLModule.SqlQuery(sqlconn, query))
				{
					// check for existence of "hidden" column
					bool hasHiddenCol = false;
					while(reader.Read())
					{
						if(reader.GetString(reader.GetOrdinal("name")) == "hidden") hasHiddenCol = true;
					}

					// add "hidden" column if not found
					if(!hasHiddenCol)
					{
						SQLModule.SqlQuery(sqlconn, "ALTER TABLE friendcode ADD COLUMN hidden BOOLEAN DEFAULT 1");
						await ReplyAsync("`Fixed database!`");
					}
					else
					{
						await ReplyAsync("`Nothing to fix!`");
					}
				}
			}

		}
	}
}
