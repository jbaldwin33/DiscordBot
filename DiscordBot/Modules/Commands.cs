using Discord.Commands;
using Discord.Net;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace NewDiscordBot.Modules
{
  

  public static class Constants
  {
    public static int MaxNumberOfChanges = 2;

    public static string[] NaviQuotes = new[]
    {
      "The Great Deku Tree asked me to be your partner from now on. Nice to meet you!",
      "Watch out!",
      "Look!",
      "Listen!",
      "Hello!"
    };

    public static string[] TatlQuotes = new[]
    {
      "My name's Tatl. So, uh, it's nice to meet you or whatever.",
      "Ohhhh, Tael... I wonder if that child will be all right on his own?",
      "I'll be your partner... or at least, until we catch that Skull Kid."
    };

    public static string[] EzloQuotes = new[]
    {
      "Well, then you have found yourself a companion, my boy! My name is Ezlo. It's a pleasure to make your acquaintance.",
      "You know, you and I have a lot in common. You see, I, too, am on a quest to break a curse of Vaati's.",
      "You're wearing a Minish person on your head, you oaf! I certainly hope you believe in us now!"
    };

    public static string[] MidnaQuotes = new[]
    {
      "You humans are obedient to a fault, aren’t you?",
      "While you’re here dawdling, the twilight continues to expand. Come on! Hurry it up!",
      "Don’t blame me for your world’s fate if you don’t hurry up and find that light! Come on! Snap to it!"
    };

    public static string[] FiQuotes = new[]
    {
      "I am detecting other users in this server.",
      "The Master Sword is known as the Sword of Evil's Bane. There is a high probability that if you find it you will be able to slay Demise."
    };

    public static string MidnaChange = "Link... I... See you later.";

    public static string Reverse = "W-What just happened?! Everything has... started over... Wha... What are you, anyway? That song you played...";
    public static string[] OcarinaQuotes = new[]
    {
      "Would you like to talk to Saria?",
      "Huh? When did you get that instrument?!",
      "I'm a hat not an instrument!",
      "Trying to call your horse?",
      "The harp you hold is known as the Goddess's Harp. It is a divine instrument of the goddess who once watched over this land."
    };

    public static string NaviPlay2 = "Really? Would you like to talk to me instead?";
    public static string NaviPlay3 = "Should we believe what Sheik told us and go to Kakariko village?";
    public static string SariaReply = "Great! Great! Please don't forget this song! Do you promise? When you want to hear my voice, play Saria's Song. You can talk with me anytime...";

    public static string[] ChangeReply = new[]
    {
      "It looks like this item doesn't work here...",
      "Hey, wait for me! Don't leave me behind!",
      "What do you think you're doing!",
      "It’s only for a little bit longer… Do you mind if I continue to hide as your shadow while you’re in human form? I’m sorry...",
      "Master, there is a 100% chance that you will not be able to change companions again until exactly 1 hour has passed."
    };

  }

  public class Commands : ModuleBase<SocketCommandContext>
  {
    public CompanionState CompanionConvo { get; set; }

    public Commands(CompanionState companionConvo)
    {
      CompanionConvo = companionConvo;
    }

    public readonly static List<KeyValuePair<Companions, string>> CompanionGreetings = new List<KeyValuePair<Companions, string>>
    {
      new KeyValuePair<Companions, string>(Companions.Navi, "Hey!"),
      new KeyValuePair<Companions, string>(Companions.Tatl, "You rang?"),
      new KeyValuePair<Companions, string>(Companions.Ezlo, "What is it?"),
      new KeyValuePair<Companions, string>(Companions.Midna, "Did you need something?"),
      new KeyValuePair<Companions, string>(Companions.Fi, "There was a 20% chance that I would respond. How can I help you?")
    };

    public readonly static List<KeyValuePair<Companions, string[]>> CompanionQuotes = new List<KeyValuePair<Companions, string[]>>
    {
      new KeyValuePair<Companions, string[]>(Companions.Navi, Constants.NaviQuotes),
      new KeyValuePair<Companions, string[]>(Companions.Tatl,  Constants.TatlQuotes),
      new KeyValuePair<Companions, string[]>(Companions.Ezlo,  Constants.EzloQuotes),
      new KeyValuePair<Companions, string[]>(Companions.Midna, Constants.MidnaQuotes),
      new KeyValuePair<Companions, string[]>(Companions.Fi,    Constants.FiQuotes)
    };


    [Command("change")]
    public async Task<RuntimeResult> Change()
    {
      if (CompanionConvo.NumberOfChanges > Constants.MaxNumberOfChanges && CompanionConvo.LastChangeTime.AddMinutes(60) > DateTime.Now)
      {
        CompanionConvo.LastReply = Constants.ChangeReply[(int)CompanionConvo.Companion];
        await ReplyAsync(CompanionConvo.LastReply);
      }
      else
      {
        if (CompanionConvo.NumberOfChanges == Constants.MaxNumberOfChanges)
          CompanionConvo.NumberOfChanges = 0;

        if (CompanionConvo.Companion == Companions.Midna)
        {
          await ReplyAsync(Constants.MidnaChange);
          await Task.Delay(1000);
        }
        await ChangeCompanion();

        CompanionConvo.LastReply = CompanionConvo.Greeting;
        await ReplyAsync(CompanionConvo.LastReply);
      }
      return StateResponse.FromSuccess(CompanionConvo);
    }

    public async Task ChangeCompanion()
    {
      var num = new Random().Next(0, CompanionGreetings.Count);
      CompanionConvo.Companion = CompanionGreetings[num].Key;
      CompanionConvo.Greeting = CompanionGreetings[num].Value;
      string picPath = string.Empty;
      string name = string.Empty;
      switch (CompanionConvo.Companion)
      {
        case Companions.Navi:
          picPath = "navi.png";
          name = "Navi";
          break;
        case Companions.Tatl:
          picPath = "tatl.png";
          name = "Tatl";
          break;
        case Companions.Ezlo:
          picPath = "ezlo.png";
          name = "Ezlo";
          break;
        case Companions.Midna:
          picPath = "midna.png";
          name = "Midna";
          break;
        case Companions.Fi:
          picPath = "fi.png";
          name = "Fi";
          break;
        default:
          picPath = "navi.png";
          name = "Navi";
          break;
      }
      var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), picPath);
      var updateFailed = false;
      try
      {
        await Context.Client.CurrentUser.ModifyAsync(u =>
        {
          u.Avatar = new Discord.Image(path);
          u.Username = name;
        });
        updateFailed = false;
      }
      catch (HttpException hex) when (hex.HttpCode == System.Net.HttpStatusCode.BadRequest)
      {
        updateFailed = true;
        CompanionConvo.NumberOfChanges = 2;
      }
      CompanionConvo.LastChangeTime = DateTime.Now;
      if (updateFailed)
        CompanionConvo.NumberOfChanges++;
    }

    [Command("talk")]
    public async Task<RuntimeResult> Talk()
    {
      var quotes = CompanionQuotes[(int)CompanionConvo.Companion];
      var num = new Random().Next(0, quotes.Value.Length);
      CompanionConvo.LastReply = quotes.Value[num];
      await ReplyAsync(CompanionConvo.LastReply);
      return StateResponse.FromSuccess(CompanionConvo);
    }

    [Command("play")]
    public async Task<RuntimeResult> Play()
    {
      CompanionConvo.LastReply = Constants.OcarinaQuotes[(int)CompanionConvo.Companion];
      await ReplyAsync(CompanionConvo.LastReply);
      return StateResponse.FromSuccess(CompanionConvo);
    }

    [Command("yes")]
    public async Task<RuntimeResult> Yes()
    {
      if (CompanionConvo.LastReply.Contains("Saria"))
      {
        CompanionConvo.LastReply = Constants.SariaReply;
        await ReplyAsync(CompanionConvo.LastReply);
      }
      else if (CompanionConvo.LastReply.Contains("instead"))
      {
        CompanionConvo.LastReply = Constants.NaviPlay3;
        await ReplyAsync(CompanionConvo.LastReply);
      }
      return StateResponse.FromSuccess(CompanionConvo);
    }

    [Command("no")]
    public async Task<RuntimeResult> No()
    {
      if (CompanionConvo.LastReply.Contains("Saria"))
      {
        CompanionConvo.LastReply = Constants.NaviPlay2;
        await ReplyAsync(CompanionConvo.LastReply);
      }
      return StateResponse.FromSuccess(CompanionConvo);
    }

    [Command("reverse")]
    public async Task<RuntimeResult> Reverse()
    {
      if (CompanionConvo.LastReply.Contains("Huh"))
      {
        CompanionConvo.LastReply = Constants.Reverse;
        await ReplyAsync(CompanionConvo.LastReply);
      }
      return StateResponse.FromSuccess(CompanionConvo);
    }
  }

  public class StateResponse : RuntimeResult
  {
    public CompanionState CompanionState { get; set; }
    public StateResponse(CommandError? error, string reason, CompanionState companion) : base(error, reason)
    {
      CompanionState = companion;
    }

    public static StateResponse FromError(string reason) => new StateResponse(CommandError.Unsuccessful, reason, null);
    public static StateResponse FromSuccess(CompanionState companionConvo) => new StateResponse(null, null, companionConvo);
  }
}