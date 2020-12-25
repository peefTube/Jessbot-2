using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Rest;

using Colorful;
using Console = Colorful.Console;

using System;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Windows.Forms;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Jessbot.Services;

/* ======================================== *
 *                                          *
 *    CODED BY PEEFTUBE (JEREMY PEIFER)     *
 *    =================================     *
 *                                          *
 *             JESSICA BOT V2               *
 *             FOR DISCORD.NET              *
 *                                          *
 * ======================================== */

namespace Jessbot
{
    class Jessbot
    {
        // Version information
        #region VERSION DATA

        static readonly string Version = "v1.99.0.000";
        static readonly string VersionDate = "December 23, 2020";
        static readonly string VersionName = "The Rebirth Update (Pre-Release)";
        static readonly string VersionInfo = $"This update ( **\"{VersionName}\"** ), {Version}, updated on {VersionDate}, " + "restarted development of JessBot on the Discordant placeholder bot to clean up code and hopefully make the bot more effective and updatable.";

        // NOTE: This should only be used while in the middle of working on heavy functionality.
        //       Has no real use outside of looking cool.
        public static readonly bool InDev = true;

        #endregion

        // Initialize core bot; this may later be replaced with shard clients.
        private readonly DiscordSocketClient _jessbot;

        // Initialize command handling and service provider.
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        #region PROGRAM

        #region MAIN

        // This is the main program. Everything should culminate in this.
        static void Main(string[] args)
        {
            // Write, to the console, the version number.
            Console.WriteLine($"============== [ Jessica Bot II ] ==============", Color.Blue);
            Console.WriteLine($"{VersionName} [ {Version} ]", Color.Blue);
            Console.WriteLine($"Session start: {DateTime.Now}", Color.Blue);

            // Regardless of the setting of InDev, run this method immediately.
            // There is no need for an if-then, the method already contains one.
            Logger.DevWarning();

            // Start the program through MainAsync().
            new Jessbot().MainAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region CONSTRUCT

        private Jessbot()
        {
            // Log to console.
            Logger.Initialize();
            Logger.InitStatus(true, false, InitType.Client);

            _jessbot = new DiscordSocketClient(new DiscordSocketConfig
            {
                // Set Logger severity level.
                LogLevel = LogSeverity.Info,

                // Sets the MessageCacheSize.
                MessageCacheSize = 50,
            });

            // Log to console.
            Logger.InitStatus(false, true, InitType.Client);
            Logger.InitStatus(true, false, InitType.Commands);

            _commands = new CommandService(new CommandServiceConfig
            {
                // Set Logger severity level.
                LogLevel = LogSeverity.Info,

                // Set no case sensitivity.
                CaseSensitiveCommands = false,
            });

            // Log to console.
            Logger.InitStatus(false, true, InitType.Commands);
            Logger.InitStatus(true, false, InitType.Inject);

            // Setup the DI (Dependency Injector).
            _services = ServiceInjector(_jessbot, _commands);

            // Log to console.
            #region SERVICE INJECTION NOTIFIER
            // Ensure this is kept up to date with the services added
            // within ServiceInjector.
            Logger.InitService(ServiceType.Database);
            Logger.InitService(ServiceType.Messaging);
            Logger.InitService(ServiceType.Registry);

            #endregion
            Logger.InitStatus(false, true, InitType.Inject);
            Logger.StageSuccess(Stages.Initialize);
        }

        #endregion

        #region MAIN ASYNC

        // Main cycle of the program.
        public async Task MainAsync()
        {
            // Log to console.
            Logger.AsyncStarted();
            Logger.AsyncStatus(false, MainAsyncS.Load);

            // Load database.
            _services.GetRequiredService<DatabaseService>().Load();

            // Log to console.
            Logger.AsyncStatus(true, MainAsyncS.Load);
            Logger.AsyncStatus(false, MainAsyncS.MessagesInit);

            // Initialize message logic.
            await MessageLogicInit();

            // Log to console.
            Logger.AsyncStatus(true, MainAsyncS.MessagesInit);
            Logger.AsyncStatus(false, MainAsyncS.Login);

            // Login and start!
            await _jessbot.LoginAsync(TokenType.Bot, File.ReadAllText("token.ptsfx"));
            await _jessbot.StartAsync();

            // Once bot is ready:
            _jessbot.Ready += ReadyStatus;
            
            // Log to console.
            Logger.AsyncStatus(true, MainAsyncS.Login);
            Logger.StageSuccess(Stages.Async);

            // Run indefinitely.
            await Task.Delay(-1);
        }

        #endregion

        #region SERVICE INJECTION

        // Cloned from https://github.com/discord-net/Discord.Net/blob/dev/samples/03_sharded_client/Program.cs
        // as well as  https://docs.stillu.cc/guides/getting_started/samples/first-bot/structure.cs
        // Modified heavily, to work as needed for this code.
        private static IServiceProvider ServiceInjector(DiscordSocketClient client, CommandService commandSys)
        {
            return new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commandSys)
                .AddSingleton<DatabaseService>()
                .AddSingleton<MessageService>()
                .AddSingleton<RegistrationService>()
                .BuildServiceProvider();
        }

        #endregion

        // Cloned from https://docs.stillu.cc/guides/getting_started/samples/first-bot/structure.cs
        // Modified as needed.
        private async Task MessageLogicInit()
        {
            // Either search the program and add all Module classes that can be found.
            // Module classes MUST be marked 'public' or they will be ignored.
            // You also need to pass your 'IServiceProvider' instance now,
            // so make sure that's done before you get here.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // This will tell the bot to pass the message to a message handling service.
            _jessbot.MessageReceived += MessagePassAsync;
        }

        // This passes the
        private async Task MessagePassAsync(SocketMessage msg)
        {
            await _services.GetRequiredService<MessageService>().Receiver(msg);
        }

        #region READY

        // Bot is online.
        private async Task ReadyStatus()
        {
            Logger.Post(""); // Post a blank line.

            // Wait!! Is the bot in development? If not, operate normally.
            if (!InDev)
            {
                // Go online!
                await _jessbot.SetStatusAsync(UserStatus.Online);
                await _jessbot.SetGameAsync("on her phone", null, ActivityType.Playing);

                // Log to console.
                Logger.Post($"Bot is ready to go. [ {DateTime.Now} ]", Color.Green);
            }
            else
            {
                // Go online, but in testing mode.
                await _jessbot.SetStatusAsync(UserStatus.DoNotDisturb);
                await _jessbot.SetGameAsync("w/ a test build", null, ActivityType.Playing);

                // Log to console.
                Logger.Post($"[DEV] Bot is ready to go. [ {DateTime.Now} ]", Color.Blue);
            }
        }

        #endregion

        #endregion

    }
}
