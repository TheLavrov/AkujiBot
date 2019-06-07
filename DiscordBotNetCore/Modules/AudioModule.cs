using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Linq;
using System;
using DiscordBot;

public class AudioModule : ModuleBase<ICommandContext>
{
    private readonly AudioService _service;

//    Remember to add an instance of the AudioService

//     to your IServiceCollection when you initialize your bot

    public AudioModule(AudioService service)
    {
        _service = service;
    }

    public static DateTimeOffset buptimeout = DateTimeOffset.UtcNow.AddMinutes(-0.5);

    //    You* MUST* mark these commands with 'RunMode.Async'

    //     otherwise the bot will not respond until the Task times out.


    [Command("join", RunMode = RunMode.Async)]
    public async Task JoinCmd()
    {
        await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
    }

    //    Remember to add preconditions to your commands,
    //	 this is merely the minimal amount necessary.
    //     Adding more commands of your own is also encouraged.


    //    [Command("leave", RunMode = RunMode.Async)]

    //    public async Task LeaveCmd()
    //    {
    //        await _service.LeaveAudio(Context.Guild);
    //    }

    //    [Command("play", RunMode = RunMode.Async)]
    //    public async Task PlayCmd([Remainder] string song)
    //    {
    //        await _service.SendAudioAsync(Context.Guild, Context.Channel, song);
    //    }

    [Command("bup", RunMode = RunMode.Async)]
[Summary("bup")]
public async Task Bup()
{
    if (DateTimeOffset.UtcNow - buptimeout >= TimeSpan.FromMinutes(0.25))
    {
        buptimeout = DateTimeOffset.UtcNow;
        await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        await _service.SendAudioAsync(Context.Guild, Context.Channel, "bup.mp3");
        await _service.LeaveAudio(Context.Guild);
    }
    else
    {
        var message = await ReplyAsync($"`You're bupping too hard.`");
        await Task.Delay(1500);
        await message.DeleteAsync();
    }
}

[Command("vol")]
[Remarks("vol [number]")]
[Summary("Changes the volume for audio related commands.")]
public async Task SetVolume(string vol)
{
    if (vol.All(Char.IsDigit))
    {
        double v = Convert.ToDouble(vol);
        if (v < 1 && v > 0)
        {
            Config c = Config.Load();
            c.Volume = Convert.ToInt32(v * 100);
            c.SaveJson();
        }
        else if (v < 0)
        {
            Config c = Config.Load();
            c.Volume = 0;
            c.SaveJson();
        }
        else if (v > 100)
        {
            Config c = Config.Load();
            c.Volume = 100;
            c.SaveJson();
        }
        else
        {
            Config c = Config.Load();
            c.Volume = Convert.ToInt32(v);
            c.SaveJson();
        }
        await ReplyAsync($"`Volume has been set to {Config.Load().Volume}%.`");
    }
    else
    {
        await ReplyAsync($"`Invalid volume.`");
    }
}
}