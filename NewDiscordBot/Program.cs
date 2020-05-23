using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NewDiscordBot.Modules;

namespace NewDiscordBot
{
  public class Program
  {
    static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

    private DiscordSocketClient client;
    private CommandService commands;
    private IServiceProvider serviceProvider;
    
    public async Task RunBotAsync()
    {
      client = new DiscordSocketClient();
      commands = new CommandService();
      serviceProvider = new ServiceCollection()
        .AddSingleton(client)
        .AddSingleton(commands)
        .AddSingleton(new CompanionState(Companions.Navi, string.Empty, string.Empty, 0, DateTime.MinValue))
        .BuildServiceProvider();

      var token = "NzEzMDIzNzc1MjYzODE3NzI5.XscpgQ.BQRK7UnuqbBqe6GH4e8YsMMVI5o";

      client.Log += Client_Log;
      await RegisterCommandsAsync();
      await client.LoginAsync(Discord.TokenType.Bot, token);
      await client.StartAsync();
      await Task.Delay(-1);
    }

    private async Task Commands_CommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
      switch (result)
      {
        case StateResponse stateResponse:
          UpdateServiceProvider(stateResponse);
          break;
        default:
          break;
      }
    }

    private void UpdateServiceProvider(StateResponse stateResponse)
    {
      (serviceProvider.GetService(typeof(CompanionState)) as CompanionState).Companion = stateResponse.CompanionState.Companion;
      (serviceProvider.GetService(typeof(CompanionState)) as CompanionState).Greeting = stateResponse.CompanionState.Greeting;
      (serviceProvider.GetService(typeof(CompanionState)) as CompanionState).LastReply = stateResponse.CompanionState.LastReply;
      (serviceProvider.GetService(typeof(CompanionState)) as CompanionState).NumberOfChanges = stateResponse.CompanionState.NumberOfChanges;
    }

    private Task Client_Log(LogMessage arg)
    {
      Console.WriteLine(arg);
      return Task.CompletedTask;
    }

    public async Task RegisterCommandsAsync()
    {
      client.MessageReceived += HandleCommandAsync;
      await commands.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
      commands.CommandExecuted += Commands_CommandExecuted;

    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
      var message = arg as SocketUserMessage;
      var context = new SocketCommandContext(client, message);
      if (message.Author.IsBot)
        return;

      var argPos = 0;

      var serv = serviceProvider.GetService(typeof(CompanionState));

      if (message.HasStringPrefix("!", ref argPos))
      {
        var result = await commands.ExecuteAsync(context, argPos, serviceProvider);
        if (!result.IsSuccess)
          Console.WriteLine(result.ErrorReason);
      }
    }
  }

  public enum Companions
  {
    Navi = 0,
    Tatl,
    Ezlo,
    Midna,
    Fi
  }

  public class CompanionState
  {
    public Companions Companion { get; set; }
    public string Greeting { get; set; }
    public string LastReply { get; set; }
    public int NumberOfChanges { get; set; }
    public DateTime LastChangeTime { get; set; }
    public CompanionState(Companions companion, string greeting, string lastReply, int numberOfChanges, DateTime lastChangeTime)
    {
      Companion = companion;
      Greeting = greeting;
      LastReply = lastReply;
      NumberOfChanges = numberOfChanges;
      LastChangeTime = lastChangeTime;
    }
  }
}
