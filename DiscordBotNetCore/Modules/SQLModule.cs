﻿using System;
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
		/*
		 * Executes a query for a SQLite connection.
		 */
		private static void doQuery(SqliteConnection connection, string query)
		{
			var command = connection.CreateCommand();
			command.CommandText = query;
			command.ExecuteNonQuery();
		}

		private static void doPQuery(SqliteConnection connection, string query, params string[] args)
		{
			var command = connection.CreateCommand();
			command.CommandText = query;
			for(int i = 0; i < args.Length; i++)
			{
				command.Parameters.AddWithValue("$" + (i + 1), args[i]);
			}
			command.ExecuteNonQuery();
		}

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

				var m_dbConnection = new SqliteConnection("" + new SqliteConnectionStringBuilder{DataSource = $"{file}"});  // Create connection
				m_dbConnection.Open();                                                                                                  // Open

				// table creation query
				this.doQuery(m_dbConnection, @"
					CREATE TABLE IF NOT EXISTS friendcode (
						id ULONG PRIMARY KEY,
						fcswitch VARCHAR DEFAULT NULL,
						fc3ds VARCHAR DEFAULT NULL,
						fcwiiu VARCHAR DEFAULT NULL,
						private BOOLEAN DEFAULT TRUE
					);
				");

				// run command to find if id is already associated
				this.doQuery(m_dbConnection,
					"SELECT * FROM friendcode WHERE id=$1;",
					Context.User.Id
				);
				
				bool exists;
				using (var reader = command.ExecuteReader())
				{
					exists = reader.HasRows;
					reader.Close();
				}

                command = m_dbConnection.CreateCommand();
                if (!exists)																											// if not exists, we insert new
				{
                    command.CommandText = $"insert into friendcode (fc{choice}, id) values (@fc, @param1)";
                    command.Parameters.AddWithValue("@fc", fc);
                    command.Parameters.AddWithValue("@param1", Context.User.Id);
				}
				else																													// if exists, we update current
				{
                    command.CommandText = $"update friendcode set fc{choice} = @fc where id = @param1";
                    command.Parameters.AddWithValue("@fc", fc);
                    command.Parameters.AddWithValue("@param1", Context.User.Id);
				}			
				command.ExecuteNonQuery();
				m_dbConnection.Close();																									// close connection

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

        [Command("fcdump")]
        [Remarks("fcdump")]
        [Summary("(ADMIN ONLY) Dump every saved friend code to DMs.")]
        public async Task FriendCodeDump()
        {
            if (!Config.Load().IsAdmin(Context.User.Id))            //if they arent an admin, turn them down
            {
                var message = await ReplyAsync($"`Only admins can use this command.`");
                await Task.Delay(2000);
                await message.DeleteAsync();
                return;
            }

            string FileName = "config/database.sqlite";
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(file))                                                                                                 // Check if the configuration file exists.
            {
                string path = Path.GetDirectoryName(file);                                                                          // Create config directory if doesn't exist.
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            var m_dbConnection = new SqliteConnection("" + new SqliteConnectionStringBuilder { DataSource = $"{file}" });  // Create connection
            m_dbConnection.Open();                                                                                                  // Open

            string sql = "CREATE TABLE IF NOT EXISTS friendcode (id ULONG PRIMARY KEY, fcswitch VARCHAR DEFAULT NULL, fc3ds VARCHAR DEFAULT NULL, fcwiiu VARCHAR DEFAULT NULL)";
            var command = m_dbConnection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();

            sql = $"SELECT * FROM friendcode";
            command = m_dbConnection.CreateCommand();
            command.CommandText = sql;

            List<string> users = new List<string>();
            using (var rdr = command.ExecuteReader())
            {
                while (rdr.Read())
                {
                    string name = rdr["id"].ToString();
                    foreach (var user in Context.Guild.Users)
                    {
                        if (name == user.Id.ToString())
                        {
                            name = user.Username;
                        }
                    }
                    users.Add($"\t\t{name}\t\tSwitch: {rdr["fcswitch"].ToString()}\t3DS: {rdr["fc3ds"].ToString()}\tWii U: {rdr["fcwiiu"].ToString()}");
                }
                rdr.Close();
            }
            m_dbConnection.Close();

            string TextFileName = "config/dump.txt";
            File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, TextFileName), users.ToArray());

            await Context.User.SendFileAsync(Path.Combine(AppContext.BaseDirectory, TextFileName));
        }
    }
}
