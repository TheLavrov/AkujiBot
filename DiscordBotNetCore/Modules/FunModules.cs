using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Collections.Generic;
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

        public class CurrentPlayer
        {
            public ulong UserID { get; set; }
            public ulong ChannelID { get; set; }
            public List<CommandSelection> LastViewedIDs { get; set; }
            public bool UsedRoll { get; set; }

            public int HP { get; set; }
            public DateTime LastHPChange { get; set; }
            public int MP { get; set; }
            public DateTime LastMPChange { get; set; }
            public int StatusCondition { get; set; }
            public DateTime LastStatusChange { get; set; }
            public int HocusPocusCondition { get; set; }
            public DateTime LastHocusPocusChange { get; set; }

            public CurrentPlayer(ulong userid, ulong channelid)
            {
                UserID = userid;
                ChannelID = channelid;
                LastViewedIDs = new List<CommandSelection>();
                UsedRoll = true;

                //Initialize Player
                HP = 100;
                LastHPChange = DateTime.Now;
                MP = 100;
                LastMPChange = DateTime.Now;
                StatusCondition = -1;
                LastStatusChange = DateTime.Now;
                HocusPocusCondition = -1;
                LastHocusPocusChange = DateTime.Now;

                StatusCondition = -1;
            }
        }

        public static List<CurrentPlayer> AllCurrentPlayers = new List<CurrentPlayer>();

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
                foreach (var lastUsed in AllCurrentPlayers)
                {
                    if (lastUsed.UserID == Context.User.Id)
                    {
                        containsUser = true;
                        foreach (var command in lastUsed.LastViewedIDs)
                        {
                            commandList.RemoveAll(x => x.ID == command.ID);
                        }
                    }
                }

                if (!containsUser)
                {
                    AllCurrentPlayers.Add(new CurrentPlayer(Context.User.Id, Context.Channel.Id));
                }
            }

            return commandList;
        }

        public string HocusPocusRoll()
        {
            List<CommandSelection> hocusPocus = new List<CommandSelection>
            {
                new CommandSelection($"{Context.User.Username} casts *Sizz*!", 0, 252, 0),
                new CommandSelection($"{Context.User.Username} casts *Sizzle*!", 0, 252, 1),
                new CommandSelection($"{Context.User.Username} casts *Bang*!", 0, 252, 2),
                new CommandSelection($"{Context.User.Username} casts *Kaboom*!", 0, 252, 3),
                new CommandSelection($"{Context.User.Username} casts *Snooze*!", 0, 252, 4),
                new CommandSelection($"{Context.User.Username} strikes with *Flame Slash*!", 0, 252, 5),
                new CommandSelection($"{Context.User.Username} strikes with *Kacrackle Slash*!", 0, 252, 6),
                new CommandSelection($"{Context.User.Username} strikes with *Metal Slash*!", 0, 252, 7),
                new CommandSelection($"{Context.User.Username} strikes with *Hatchet Man*!", 0, 252, 8),
                new CommandSelection($"{Context.User.Username} casts *Whack*!", 0, 252, 9),
                new CommandSelection($"{Context.User.Username} casts *Thwack*!", 0, 252, 10),
                new CommandSelection($"{Context.User.Username} summons *Magic Burst*!", 0, 252, 11),
                new CommandSelection($"{Context.User.Username} blew themselves up with *Kamikazee*!", 0, 99, 12),
                new CommandSelection($"{Context.User.Username} powers up with *Psyche Up*!", 0, 252, 13),
                new CommandSelection($"{Context.User.Username} powers up with *Oomph*!", 0, 252, 14),
                new CommandSelection($"{Context.User.Username} powers up with *Acceleratle*!", 0, 252, 15),
                new CommandSelection($"{Context.User.Username} casts *Kaclang*!", 0, 160, 16),
                new CommandSelection($"{Context.User.Username} casts *Bounce*!", 0, 252, 17),
                new CommandSelection($"{Context.User.Username} casts *Heal*!", 0, 160, 18),
                new CommandSelection($"{Context.User.Username} casts *Zoom*!", 0, 252, 19),

                new CommandSelection($"{Context.User.Username} turned *giant*!", 0, 496, 21),
                new CommandSelection($"{Context.User.Username} became *invincible*!", 0, 130, 22),
                new CommandSelection($"{Context.User.Username} *refilled all mana*!", 100, 404, 23),
                new CommandSelection($"{Context.User.Username} was *slowed*!", 0, 618, 24),
                new CommandSelection($"{Context.User.Username} *lost all mana*!", -100, 618, 25),
                new CommandSelection($"{Context.User.Username} was *poisoned*!", 0, 618, 26),
                new CommandSelection($"{Context.User.Username} fell into a *deep sleep*!", 0, 618, 27),
                new CommandSelection($"{Context.User.Username} turned *tiny*!", 0, 557, 28),
                new CommandSelection($"{Context.User.Username} grew a *flower* on their head!", 0, 618, 29),
                new CommandSelection($"{Context.User.Username} turned *invisible*!", 0, 618, 30)
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

        public string FormatFinishedCommands(List<CommandSelection> commandList, int choice = 0, bool ShowMP = false)
        {
            ulong HeroCursorId = 617960009623535621;
            Emote.TryParse($"<a:HeroCursor:{HeroCursorId}>", out Emote Cursor);

            string finishedFormattedString = String.Empty;

            foreach (var command in commandList)
            {
                if (commandList.ElementAtOrDefault(choice) == command)
                    finishedFormattedString += " > " + Cursor + " " + command.Name + "\n";
                else
                {
                    if (ShowMP)
                        finishedFormattedString += $">         {command.Name} `MP   {command.MP}`\n";
                    else
                        finishedFormattedString += $">         {command.Name}\n";
                }
            }

            finishedFormattedString = finishedFormattedString.Substring(0, finishedFormattedString.Length - 1);

            return finishedFormattedString;
        }

        public string SelectionResponse(int selection)
        {
            switch (selection)
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
            var commandList = InitializeCommandList();
            var finishedList = CommandListGen(commandList);
            string formattedCommands = FormatFinishedCommands(finishedList);

            foreach (var user in AllCurrentPlayers)
            {
                if (user.UserID == Context.User.Id)
                {
                    user.LastViewedIDs.Clear();
                    foreach (var lastCommand in finishedList)
                    {
                        user.LastViewedIDs.Add(lastCommand);
                    }
                    user.UsedRoll = true;
                }
            }

            var commandResponse = SelectionResponse(finishedList[0].ID);
            var completedCommandUI = await ReplyAsync(formattedCommands + $"`MP   {finishedList[0].MP}`\n{commandResponse}");
        }

        [Command("rollcommand")]
        [Summary("Roll a Command Selection.")]
        public async Task CommandRoll()
        {
            var commandList = InitializeCommandList();
            var finishedList = CommandListGen(commandList);
            string formattedCommands = FormatFinishedCommands(finishedList, -1, true);

            foreach (var user in AllCurrentPlayers)
            {
                if (user.UserID == Context.User.Id)
                {
                    user.LastViewedIDs.Clear();
                    foreach (var lastCommand in finishedList)
                    {
                        user.LastViewedIDs.Add(lastCommand);
                    }
                    user.UsedRoll = false;
                }
            }

            var completedCommandUI = await ReplyAsync(formattedCommands + $"\nChoose your option using `{Config.Load().Prefix}rolloption [number]` or roll again.");
        }

        [Command("rolloption")]
        [Remarks("rolloption [number]")]
        [Summary("Roll a Command Selection.")]
        public async Task RollOption(string choice)
        {
            int option = Convert.ToInt32(choice) - 1;
            bool userExists = false;

            foreach (var user in AllCurrentPlayers)
            {
                if (user.UserID == Context.User.Id)
                {
                    userExists = true;
                    if (user.UsedRoll)
                    {
                        await ReplyAsync($"You already used your previous roll! Re-roll again using `{Config.Load().Prefix}rollcommand`.");
                        return;
                    }

                    if (option >= 0 && option < user.LastViewedIDs.Count)          //if element index exists
                    {
                        string formattedCommands = FormatFinishedCommands(user.LastViewedIDs, option, false);
                        var commandResponse = SelectionResponse(user.LastViewedIDs[option].ID);
                        await ReplyAsync(formattedCommands + $"`MP   {user.LastViewedIDs[option].MP}`\n{commandResponse}");
                        user.UsedRoll = true;
                    }
                    else
                    {
                        await ReplyAsync($"Your option appears to be out of range. Try again.");
                        return;
                    }
                    
                }
            }

            if (!userExists)
            {
                await ReplyAsync($"You need to roll a Command Selection first. Roll by using `{Config.Load().Prefix}rollcommand`.");
                return;
            }
        }
    }
}
