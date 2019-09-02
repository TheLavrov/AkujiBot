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

        public class HocusPocus
        {
            public string Name { get; set; }
            public float Percent { get; set; }
            public int ID { get; set; }
        }

        

        public void InitializeCommandList(List<CommandSelection> commandList)
        {
            commandList.Clear();
            commandList.Add(new CommandSelection("Sizz                           ", 8, 16, 0));
            commandList.Add(new CommandSelection("Sizzle                        ", 20, 20, 1));
            commandList.Add(new CommandSelection("Bang                         ", 9, 16, 2));
            commandList.Add(new CommandSelection("Kaboom                   ", 37, 20, 3));
            commandList.Add(new CommandSelection("Snooze                     ", 16, 17, 4));
            commandList.Add(new CommandSelection("Flame Slash             ", 12, 18, 5));
            commandList.Add(new CommandSelection("Kacrackle Slash     ", 11, 18, 6));
            commandList.Add(new CommandSelection("Metal Slash             ", 6, 7, 7));
            commandList.Add(new CommandSelection("Hatchet Man          ", 15, 18, 8));
            commandList.Add(new CommandSelection("Whack                     ", 10, 8, 9));
            commandList.Add(new CommandSelection("Thwack                    ", 30, 12, 10));
            commandList.Add(new CommandSelection("Magic Burst            ", 100, 5, 11));
            commandList.Add(new CommandSelection("Kamikazee              ", 1, 5, 12));
            commandList.Add(new CommandSelection("Psyche Up               ", 14, 16, 13));
            commandList.Add(new CommandSelection("Oomph                    ", 16, 16, 14));
            commandList.Add(new CommandSelection("Acceleratle             ", 13, 16, 15));
            commandList.Add(new CommandSelection("Kaclang                    ", 6, 5, 16));
            commandList.Add(new CommandSelection("Bounce                     ", 14, 16, 17));
            commandList.Add(new CommandSelection("Heal                          ", 7, 7, 18));
            commandList.Add(new CommandSelection("Zoom                       ", 8, 15, 19));
            commandList.Add(new CommandSelection("Hocus Pocus          ", 4, 3, 20));                      
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

        public static List<CommandSelection> CommandListGen(List<CommandSelection> commandList, int Turns)
        {
            List<CommandSelection> finishedList = new List<CommandSelection>();
            for(int i = 0; i < Turns; i++)
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

        //~topdeck
        [Command("topdeck")]
        [Summary("Don't think, just gamble on a Down-B!")]
        public async Task TopDeck()
        {
            ulong HeroCursorId = 617960009623535621;
            Emote Cursor;
            Emote.TryParse($"<a:HeroCursor:{HeroCursorId}>", out Cursor);

            var commandList = new List<CommandSelection>();
            InitializeCommandList(commandList);

            var finishedList = CommandListGen(commandList, 4);

            string formattedCommands = FormatFinishedCommands(finishedList, Cursor);

            var message = await ReplyAsync($"> " + Cursor + " " + formattedCommands + $"`MP   {finishedList[0].MP}`");
        }
    }
}
