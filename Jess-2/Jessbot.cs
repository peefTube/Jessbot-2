using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Rest;

using Colorful;
using Console = Colorful.Console;

using Interactivity;

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
    public partial class Jessbot
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

        // Initialize list of bot owners.
        public static readonly ulong[] Owners = { 236738543387541507, 553420930244673536, 559942856249442305 };

        // Initialize command handling and service provider.
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        // Initialize a random number generator. This should always be public.
        public static readonly Random RNG = new Random();

        // Initialize constants.
        public static readonly string JB = $"{ new Emoji("<:jessbucks:561818526923620353>").ToString() }";
        public static readonly Dictionary<string, TimeSpan> CodesUTC = new Dictionary<string, TimeSpan>()
        {
                    // Zero and positives
                    { "UTC-00:00", TimeSpan.Zero },   { "UTC+01:00", new TimeSpan(1, 0, 0) },
            { "UTC+02:00", new TimeSpan(2, 0, 0) },   { "UTC+03:00", new TimeSpan(3, 0, 0) },
            { "UTC+03:30", new TimeSpan(3, 30, 0) },  { "UTC+04:00", new TimeSpan(4, 0, 0) },
            { "UTC+05:00", new TimeSpan(5, 0, 0) },   { "UTC+05:30", new TimeSpan(5, 30, 0) },
            { "UTC+05:45", new TimeSpan(5, 45, 0) },  { "UTC+06:00", new TimeSpan(6, 0, 0) },
            { "UTC+06:30", new TimeSpan(6, 30, 0) },  { "UTC+07:00", new TimeSpan(7, 0, 0) },
            { "UTC+08:00", new TimeSpan(8, 0, 0) },   { "UTC+08:45", new TimeSpan(8, 45, 0) },
            { "UTC+09:00", new TimeSpan(9, 0, 0) },   { "UTC+09:30", new TimeSpan(9, 30, 0) },
            { "UTC+10:00", new TimeSpan(10, 0, 0) },  { "UTC+10:30", new TimeSpan(10, 30, 0) },
            { "UTC+11:00", new TimeSpan(11, 0, 0) },  { "UTC+12:00", new TimeSpan(12, 0, 0) },
            { "UTC+12:45", new TimeSpan(12, 45, 0) }, { "UTC+13:00", new TimeSpan(13, 0, 0) },
            { "UTC+13:45", new TimeSpan(13, 45, 0) }, { "UTC+14:00", new TimeSpan(14, 0, 0) },

            // Negatives
            { "UTC-01:00", new TimeSpan(-1, 0, 0) },   { "UTC-02:00", new TimeSpan(-2, 0, 0) },
            { "UTC-02:30", new TimeSpan(-2, -30, 0) }, { "UTC-03:00", new TimeSpan(-3, 0, 0) },
            { "UTC-03:30", new TimeSpan(-3, -30, 0) }, { "UTC-04:00", new TimeSpan(-4, 0, 0) },
            { "UTC-04:30", new TimeSpan(-4, -30, 0) }, { "UTC-05:00", new TimeSpan(-5, 0, 0) },
            { "UTC-06:00", new TimeSpan(-6, 0, 0) },   { "UTC-07:00", new TimeSpan(-7, 0, 0) },
            { "UTC-08:00", new TimeSpan(-8, 0, 0) },   { "UTC-09:00", new TimeSpan(-9, 0, 0) },
            { "UTC-09:30", new TimeSpan(-9, -30, 0) }, { "UTC-10:00", new TimeSpan(-10, 0, 0) },
            { "UTC-11:00", new TimeSpan(-11, 0, 0) },  { "UTC-12:00", new TimeSpan(-12, 0, 0) },

            // Alternative zero values
            { "UTC±00:00", TimeSpan.Zero }, { "UTC+00:00", TimeSpan.Zero }
        };

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
            Logger.InitService(ServiceType.Converter);
            Logger.InitService(ServiceType.Experience);
            Logger.InitService(ServiceType.Economy);
            Logger.InitService(ServiceType.Inventory);

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

            // Only run if the bot should be loading information!
            if (_load)
            {
                // Log to console.
                Logger.AsyncStatus(false, MainAsyncS.Load);

                // Load database.
                _services.GetRequiredService<DatabaseService>().Load();

                // Log to console.
                Logger.AsyncStatus(true, MainAsyncS.Load);
            }

            // Log to console.
            Logger.AsyncStatus(false, MainAsyncS.MessagesInit);

            // Initialize message logic.
            await MessageLogicInit();

            // Log to console.
            Logger.AsyncStatus(true, MainAsyncS.MessagesInit);
            Logger.AsyncStatus(false, MainAsyncS.Login);

            // Initialize user-guild interfacing logic.
            UGInterfacingLogicInit();

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
                .AddSingleton(new InteractivityService(client, TimeSpan.FromSeconds(20)))
                .AddSingleton<DatabaseService>()
                .AddSingleton<MessageService>()
                .AddSingleton<ParserService>()
                .AddSingleton<RegistrationService>()
                .AddSingleton<ConversionService>()
                .AddSingleton<ExperienceService>()
                .AddSingleton<EconomyService>()
                .AddSingleton<InventoryService>()
                .AddSingleton<UserGuildInterfaceService>()
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

            // This will tell the bot to pass any completed command to a command error handling service.
            _commands.CommandExecuted += CommandResultPassAsync;
        }

        private void UGInterfacingLogicInit()
        {
            _jessbot.UserJoined += _services.GetRequiredService<UserGuildInterfaceService>().Joined;
            _jessbot.UserBanned += _services.GetRequiredService<UserGuildInterfaceService>().Banned;
            _jessbot.UserLeft += _services.GetRequiredService<UserGuildInterfaceService>().Left;
        }

        // This passes the message into its respective service.
        private async Task MessagePassAsync(SocketMessage msg)
        {
            await _services.GetRequiredService<MessageService>().Receiver(msg);
        }

        // This passes the command results into their respective service.
        private async Task CommandResultPassAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            await _services.GetRequiredService<ParserService>().PostExecutionAsync(command, context, result);
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
