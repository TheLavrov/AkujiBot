using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Net;
using System.Globalization;
using System;

namespace DiscordBot.Modules
{
    public class FunModules : ModuleBase<SocketCommandContext>
    {
        private static Random RNGesus = new Random();

        public class CommandSelection
        {
            public CommandSelection(string name, int mp, int weight, int id)
            {
                Name = name;
                MP = mp;
                Weight = weight;
                ID = id;
            }

            public CommandSelection(CommandSelection cs)
            {
                Name = cs.Name;
                MP = cs.MP;
                Weight = cs.Weight;
                ID = cs.ID;
            }

            public string Name { get; set; }
            public int MP { get; set; }
            public int Weight { get; set; }
            public int ID { get; set; }
        }

        public class LastCommandsViewed
        {
            public LastCommandsViewed(ulong id)
            {
                UserID = id;
                LastUsedIDs = new List<int>();
            }
            public ulong UserID { get; set; }
            public List<int> LastUsedIDs { get; set; }
        }

        public static List<LastCommandsViewed> lastCommandsList = new List<LastCommandsViewed>();

        public List<CommandSelection> InitializeCommandList(bool checkLastSelection = true)
        {
            List<CommandSelection> commandList = new List<CommandSelection>
            {
                new CommandSelection("Sizz                           ", 8, 16, 0),
                new CommandSelection("Sizzle                        ", 20, 20, 1),
                new CommandSelection("Bang                         ", 9, 16, 2),
                new CommandSelection("Kaboom                   ", 37, 20, 3),
                new CommandSelection("Snooze                     ", 16, 17, 4),
                new CommandSelection("Flame Slash             ", 12, 18, 5),
                new CommandSelection("Kacrackle Slash     ", 11, 18, 6),
                new CommandSelection("Metal Slash             ", 6, 7, 7),
                new CommandSelection("Hatchet Man          ", 15, 18, 8),
                new CommandSelection("Whack                     ", 10, 8, 9),
                new CommandSelection("Thwack                    ", 30, 12, 10),
                new CommandSelection("Magic Burst            ", 100, 5, 11),
                new CommandSelection("Kamikazee              ", 1, 5, 12),
                new CommandSelection("Psyche Up               ", 14, 16, 13),
                new CommandSelection("Oomph                    ", 16, 16, 14),
                new CommandSelection("Acceleratle             ", 13, 16, 15),
                new CommandSelection("Kaclang                    ", 6, 5, 16),
                new CommandSelection("Bounce                     ", 14, 16, 17),
                new CommandSelection("Heal                          ", 7, 7, 18),
                new CommandSelection("Zoom                       ", 8, 15, 19),
                new CommandSelection("Hocus Pocus          ", 4, 3, 20)
            };

            if (checkLastSelection)
            {
                bool containsUser = false;
                foreach (var lastUsed in lastCommandsList)
                {
                    if (lastUsed.UserID == Context.User.Id)
                    {
                        containsUser = true;
                        foreach (int id in lastUsed.LastUsedIDs)
                        {
                            commandList.RemoveAll(x => x.ID == id);
                        }
                    }                 
                }

                if (!containsUser)
                {
                    lastCommandsList.Add(new LastCommandsViewed(Context.User.Id));
                }
            }

            return commandList;
        }

        public string HocusPocusRoll()
        {
            List<CommandSelection> hocusPocus = new List<CommandSelection>
            {
                new CommandSelection($"{Context.User.Username} casts *Sizz*!", -1, 252, 0),
                new CommandSelection($"{Context.User.Username} casts *Sizzle*!", -1, 252, 1),
                new CommandSelection($"{Context.User.Username} casts *Bang*!", -1, 252, 2),
                new CommandSelection($"{Context.User.Username} casts *Kaboom*!", -1, 252, 3),
                new CommandSelection($"{Context.User.Username} casts *Snooze*!", -1, 252, 4),
                new CommandSelection($"{Context.User.Username} strikes with *Flame Slash*!", -1, 252, 5),
                new CommandSelection($"{Context.User.Username} strikes with *Kacrackle Slash*!", -1, 252, 6),
                new CommandSelection($"{Context.User.Username} strikes with *Metal Slash*!", -1, 252, 7),
                new CommandSelection($"{Context.User.Username} strikes with *Hatchet Man*!", -1, 252, 8),
                new CommandSelection($"{Context.User.Username} casts *Whack*!", -1, 252, 9),
                new CommandSelection($"{Context.User.Username} casts *Thwack*!", -1, 252, 10),
                new CommandSelection($"{Context.User.Username} summons *Magic Burst*!", -1, 252, 11),
                new CommandSelection($"{Context.User.Username} blew themselves up with *Kamikazee*!", -1, 99, 12),
                new CommandSelection($"{Context.User.Username} powers up with *Psyche Up*!", -1, 252, 13),
                new CommandSelection($"{Context.User.Username} powers up with *Oomph*!", -1, 252, 14),
                new CommandSelection($"{Context.User.Username} powers up with *Acceleratle*!", -1, 252, 15),
                new CommandSelection($"{Context.User.Username} casts *Kaclang*!", -1, 160, 16),
                new CommandSelection($"{Context.User.Username} casts *Bounce*!", -1, 252, 17),
                new CommandSelection($"{Context.User.Username} casts *Heal*!", -1, 160, 18),
                new CommandSelection($"{Context.User.Username} casts *Zoom*!", -1, 252, 19),
                new CommandSelection($"{Context.User.Username} turned *giant*!", -1, 496, 21),
                new CommandSelection($"{Context.User.Username} became *invincible*!", -1, 130, 22),
                new CommandSelection($"{Context.User.Username} *refilled all mana*!", -1, 404, 23),
                new CommandSelection($"{Context.User.Username} was *slowed*!", -1, 618, 24),
                new CommandSelection($"{Context.User.Username} *lost all mana*!", -1, 618, 25),
                new CommandSelection($"{Context.User.Username} was *poisoned*!", -1, 618, 26),
                new CommandSelection($"{Context.User.Username} fell into a *deep sleep*!", -1, 618, 27),
                new CommandSelection($"{Context.User.Username} turned *tiny*!", -1, 557, 28),
                new CommandSelection($"{Context.User.Username} grew a *flower* on their head!", -1, 618, 29),
                new CommandSelection($"{Context.User.Username} turned *invisible*!", -1, 618, 30)              
            };

            return CommandRoll(hocusPocus).Name;
        }

        public static CommandSelection CommandRoll(List<CommandSelection> currentCommands)
        {
            // totalWeight is the sum of all brokers' weight

            int totalWeight = 0;
            foreach (var commandsToWeigh in currentCommands)
            {
                totalWeight += commandsToWeigh.Weight;
            }

            int randomNumber = RNGesus.Next(0, totalWeight);

            CommandSelection selectedBroker = null;
            foreach (CommandSelection broker in currentCommands)
            {
                if (randomNumber < broker.Weight)
                {
                    selectedBroker = broker;
                    break;
                }

                randomNumber = randomNumber - broker.Weight;
            }

            return selectedBroker;
        }

        public static List<CommandSelection> CommandListGen(List<CommandSelection> commandList, int Turns = 4)
        {
            List<CommandSelection> finishedList = new List<CommandSelection>();
            for (int i = 0; i < Turns; i++)
            {
                CommandSelection turn = CommandRoll(commandList);
                finishedList.Add(new CommandSelection(turn));
                commandList.Remove(turn);
            }
            return finishedList;
        }

        public string FormatFinishedCommands(List<CommandSelection> commandList, Emote cursor)
        {
            string finishedFormattedString = String.Empty;

            foreach (var command in commandList)
            {
                if (commandList.ElementAt(0) == command)
                {
                    finishedFormattedString += $"{command.Name}\n";
                }
                else
                {
                    finishedFormattedString += $">         {command.Name}\n";
                }
            }

            finishedFormattedString = finishedFormattedString.Substring(0, finishedFormattedString.Length - 1);

            return finishedFormattedString;
        }

        public string SelectionResponse(CommandSelection selection)
        {
            switch (selection.ID)
            {
                case 0:
                    return $"{Context.User.Username} casts *Sizz*!";
                case 1:
                    return $"{Context.User.Username} casts *Sizzle*!";
                case 2:
                    return $"{Context.User.Username} casts *Bang*!";
                case 3:
                    return $"{Context.User.Username} casts *Kaboom*!";
                case 4:
                    return $"{Context.User.Username} casts *Snooze*!";
                case 5:
                    return $"{Context.User.Username} strikes with *Flame Slash*!";
                case 6:
                    return $"{Context.User.Username} strikes with *Kacrackle Slash*!";
                case 7:
                    return $"{Context.User.Username} strikes with *Metal Slash*!";
                case 8:
                    return $"{Context.User.Username} strikes with *Hatchet Man*!";
                case 9:
                    return $"{Context.User.Username} casts *Whack*!";
                case 10:
                    return $"{Context.User.Username} casts *Thwack*!";
                case 11:
                    return $"{Context.User.Username} summons *Magic Burst*!";
                case 12:
                    return $"{Context.User.Username} blew themselves up with *Kamikazee*!";
                case 13:
                    return $"{Context.User.Username} powers up with *Psyche Up*!";
                case 14:
                    return $"{Context.User.Username} powers up with *Oomph*!";
                case 15:
                    return $"{Context.User.Username} powers up with *Acceleratle*!";
                case 16:
                    return $"{Context.User.Username} casts *Kaclang*!";
                case 17:
                    return $"{Context.User.Username} casts *Bounce*!";
                case 18:
                    return $"{Context.User.Username} casts *Heal*!";
                case 19:
                    return $"{Context.User.Username} casts *Zoom*!";
                case 20:
                    string HocusPocus = $"{Context.User.Username} casts *Hocus Pocus*!\n";
                    return HocusPocus + HocusPocusRoll();
                default:
                    return $"how are you seeing this";
            }
        }


        /* Commands Here */


        //~topdeck
        [Command("topdeck")]
        [Summary("Don't think, just gamble on a Down-B!")]
        public async Task TopDeck()
        {
            ulong HeroCursorId = 617960009623535621;
            Emote.TryParse($"<a:HeroCursor:{HeroCursorId}>", out Emote Cursor);

            var commandList = InitializeCommandList();

            var finishedList = CommandListGen(commandList);

            string formattedCommands = FormatFinishedCommands(finishedList, Cursor);

            foreach (var user in lastCommandsList)
            {
                if (user.UserID == Context.User.Id)
                {
                    user.LastUsedIDs.Clear();
                    foreach (var lastCommand in finishedList)
                    {
                        user.LastUsedIDs.Add(lastCommand.ID);
                    }
                }
            }

            var commandResponse = SelectionResponse(finishedList[0]);
            var completedCommandUI = await ReplyAsync($"> " + Cursor + " " + formattedCommands + $"`MP   {finishedList[0].MP}`\n{commandResponse}");
            

        }
    }
}
