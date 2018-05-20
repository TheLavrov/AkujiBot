using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System;

namespace DiscordBot.Modules
{
	public class VoteModule : ModuleBase<SocketCommandContext>
	{
		class Answer
		{
			public Emoji Emoji { get; set; }
			public string RawAnswer { get; set; }
			public int Votes { get; set; } = 0;
		}

		public static bool currentVote = false;

		[Command("vote", RunMode = RunMode.Async)]
		[Remarks("vote [question] [length in minutes] <answers>")]
		[Summary("Creates a vote using the questions and answers provided. Make sure you separate your question and each individual answer with quotes. Time can only be as long as 30 minutes.")]
		public async Task Vote(string question, double time, params string[] answers)
		{
			if (currentVote)
			{
				var message = await ReplyAsync($"`There's already a vote taking place.`");
				await Task.Delay(1500);
				await message.DeleteAsync();
				return;
			}

			if (time > 30)
			{
				time = 30;
			}

			if (answers.Length > 10)
			{
				var message = await ReplyAsync($"`Your vote contained too many possible answers. (Limit: 10)`");
				await Task.Delay(1500);
				await message.DeleteAsync();
				return;
			}

			if (answers.Length == 1)
			{
				var message = await ReplyAsync($"`Your vote contained too few answers.`");
				await Task.Delay(1500);
				await message.DeleteAsync();
				return;
			}

			currentVote = true;

			List<string> emoji = new List<string>{ "👀", "👌", "⛄", "😂", "👻", "💩", "💯", "🎁", "🎉", "💵", "🔪", "👡", "👢", "👑", "👒", "🎩", "🎓", "💄", "💍", "🌂" };
			Random rnd = new Random();
			List<Answer> answersPlusEmoji = new List<Answer>();

			for (int i = 0; i < answers.Length; i++)									//get a bunch of random emojis to associate each answer with.
			{
				// Get the next emoji at random.
				var index = rnd.Next(0, emoji.Count);
				var item = emoji[index];

				var test = new Emoji(item);

				answersPlusEmoji.Add(new Answer { Emoji = new Emoji(item), RawAnswer = answers[i]	});

				// Remove the emoji from the list.
				emoji.RemoveAt(index);
			}

			var initialEmbed = new EmbedBuilder()
				.WithAuthor($"{Context.User.Username} has started a vote!", Context.User.GetAvatarUrl())
				.WithFooter($"Use the matching reaction to vote for the answer you want. This vote is set to last for {time} minutes.")
				.WithTitle($"**{question}**")
				.WithColor(Color.Gold);

			string description = "";

			if (time < 1)
			{
				initialEmbed.WithFooter($"Use the matching reaction to vote for the answer you want. This vote is set to last for {time * 60} seconds.");
			}

			foreach (var ape in answersPlusEmoji)										//make a description that tells the audience which reaction belongs to each answer 
			{
				description += $"{ape.Emoji}   **{ape.RawAnswer}**\n";
			}

			initialEmbed.Description = description;
			var embedMessage = await ReplyAsync("", false, initialEmbed.Build());		//create vote embed

			foreach (var ape in answersPlusEmoji)										//have bot react to embed post with the reactions people need for vote
			{
				await embedMessage.AddReactionAsync(ape.Emoji);
			}

			await Task.Delay(TimeSpan.FromMinutes(time));                                 //Between vote being finished and results


			foreach (var ape in answersPlusEmoji)													//remove the reactions placed by the bot to assist voting
			{
				await embedMessage.RemoveReactionAsync(ape.Emoji, embedMessage.Author);
			}

			await Task.Delay(TimeSpan.FromSeconds(2));									//small delay to make sure all bot reactions are removed before using said data

			int winningvotes = -1;
			int totalvotes = 0;
			bool tie = false;
			Answer winner = null;

			var allReactions = await Context.Channel.GetMessageAsync(embedMessage.Id) as IUserMessage;			//get new version of embedmessage again to see the current reactions

			foreach (var reactions in allReactions.Reactions)													//for every reaction, check every answer and find the matching emoji
			{
				foreach (var ape in answersPlusEmoji)
				{
					if (reactions.Key.Name == ape.Emoji.Name && reactions.Value.ReactionCount != 0)				//if they match, set their vote values and increment totalvotes
					{
						ape.Votes = reactions.Value.ReactionCount;
						totalvotes += reactions.Value.ReactionCount;
						if (reactions.Value.ReactionCount == winningvotes)
						{
							tie = true;
						}

						if (reactions.Value.ReactionCount > winningvotes)
						{
							winningvotes = reactions.Value.ReactionCount;
							winner = ape;
							tie = false;
						}
						break;
					}
				}
			}
			
			var finishedEmbed = new EmbedBuilder()
				.WithAuthor($"The votes are in for {Context.User.Username}'s question!", Context.User.GetAvatarUrl())
				.WithTitle($"**{question}**")
				.WithColor(Color.DarkGreen);

			var s = "s";
			if (winningvotes == 1)
				s = "";

			var ss = "s";
			if (totalvotes == 1)
				ss = "";

			if (tie)
			{
				var tiedResults = answersPlusEmoji.OrderByDescending(a => a.Votes).ToList();
				tiedResults.RemoveAll(a => a.Votes != winningvotes);
				description = "The vote was tied between: \n";
				foreach (var result in tiedResults)
				{
					description += $"{result.Emoji.Name}   **{result.RawAnswer}**\n";
				}
				finishedEmbed.WithFooter($"Each answer had {winningvotes} vote{s}, with {totalvotes} total vote{ss} cast.");
				finishedEmbed.WithColor(Color.DarkRed);
				finishedEmbed.WithDescription(description);
			}
			else if (winningvotes == -1)
			{
				finishedEmbed.WithAuthor("");
				finishedEmbed.WithTitle("");
				finishedEmbed.WithDescription($"Nobody voted for {Context.User.Username}'s question. 😢");
				finishedEmbed.WithColor(Color.DarkerGrey);
			}
			else
			{
				description = $"{winner.Emoji.Name}   **{winner.RawAnswer}**";
				finishedEmbed.WithFooter($"This answer won with {winningvotes} vote{s}, with {totalvotes} total vote{ss} cast.");
				finishedEmbed.WithDescription(description);
			}

			await embedMessage.DeleteAsync();
			var secondEmbed = ReplyAsync("", false, finishedEmbed.Build());
			currentVote = false;
		}
	}
}
