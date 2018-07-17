# AkujiBot

This is a C# Discord bot written with Discord.Net 2.0.0-beta. I made this just to test my programming skills.

Thanks to Joe4evr for his WS4NetCore Nuget package and for his help when I had questions with Discord.Net. There was code given to me by other members of the Discord API channel a while back but I missed their names. The audio code and config deserve their credit, so if anyone knows who created something similar needs to tell me so I can credit them.

## Prerequisites

- A [Discord App](https://discordapp.com/developers/applications/me) Token
- A [Twitter App](https://apps.twitter.com/) Consumer Key/Secret and Access Token/Token Secret
- For Linux: the .NET Core SDK ([Installation Guide](https://www.microsoft.com/net/download/linux-package-manager/rhel/sdk-current))

## Building & Running

### Linux

```
git clone https://github.com/TheLavrov/AkujiBot.git ~/AkujiBot
cd ~/AkujiBot/DiscordBotNetCore
dotnet build
dotnet run
```

## Configuration

The program will generate a `configuration.json` in `DiscordAppNetCore/bin/Debug`.

```
"Owners": Adding your Discord ID to this list will allow you to use admin commands.
"Prefix": What symbol you type before a command's name.
"Token": Where your bot token goes.
"Volume": Volume for audio-related commands.
"consumerKey", "consumerSecret", "userAccessToken", "userAccessSecret": Used for Twitter commands like "live". If you don't care about Twitter LEAVE THESE BLANK.
```
