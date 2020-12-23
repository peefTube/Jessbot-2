using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Rest;

using System;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Numerics;

using Microsoft.Extensions.DependencyInjection;

using Jessbot.Data;
using Jessbot.IO;

namespace Jessbot
{
    /* This is the central bot code.
    -* This will probably contain calls to other files which handle things such as:
    -* Commands
    -* File saving
    -* File loading
    -* Item databases
    -*/
    class Jessbot
    {
        #region VARIABLES

        // Version information
        #region VERSION DATA

        static readonly string JessConstantVersion = "v1.99.0.000";
        static readonly string JessConstantVersionDate = "December 22, 2020";
        static readonly string JessConstantVersionName = "The Rebirth Update (Pre-Release)";
        static readonly string JessConstantVersionInfo = $"This update ( **\"{JessConstantVersionName}\"** ), {JessConstantVersion}, updated on {JessConstantVersionDate}, " + "restarted development of JessBot on the Discordant placeholder bot to clean up code and hopefully make the bot more effective and updatable.";

        #endregion

        // Hardcoded, crucial data
        #region CRUCIAL DATA

        // The bot itself
        DiscordShardedClient jess = new DiscordShardedClient();

        // Fallbacks (determines program state)
        public static bool FALLBACK_UserLevelledUp = false;

        // Bot owner list
        static readonly ulong[] JessProgramOwnersRegistry = { 236738543387541507, 553420930244673536, 559942856249442305 };

        // Defaults & Constants
        static readonly string JessDefaultPrefix = "JR."; // Used to build new servers

        static uint CONST_MaximumLevel = 1337; // Caps off the maximum level here
        static uint CONST_MinExpThreshold = 5000; // Minimum experience threshold to reach level 1
        static uint CONST_ExpThresholdIncrement = 2500; // Every level goes up by this much minimum necessary experience
        public static Dictionary<uint, BigInteger> CONST_TotalLevels = new Dictionary<uint, BigInteger>(); // Stores numeric values of each threshold for reaching a particular level

        static readonly string JB = $"{ new Emoji("<:jessbucks:561818526923620353>").ToString() }"; // Jessbucks emote constant

        // Initialization
        static IOLibrary JessbotIO = new IOLibrary();

        #endregion

        #endregion

        #region PROGRAM

        #region MAIN

        // This is the main program. Everything should culminate in this.
        static void Main(string[] args)
        {
            // Write, to the console, the version number.
            Console.WriteLine($"Jessica Bot II");
            Console.WriteLine($"{JessConstantVersionName} [ {JessConstantVersion} ]");
            Console.WriteLine($"Session start: {DateTime.Now}");

            // Start the program through MainAsync().
            new Jessbot().MainAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region MAIN ASYNC

        public async Task MainAsync()
        {
            #region LOADING CYCLE

            // Initialize CONST_TotalLevels through offloading to the I/O system.
            await JessbotIO.InitializeLevels(CONST_MaximumLevel, CONST_MinExpThreshold, CONST_ExpThresholdIncrement);

            // Send database to the I/O system, offloading initialization there instead.
            await JessbotIO.InitialDatabaseLoad();

            #endregion

            #region SHARDS
            
            var JessShardConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 100
            };

            // Prepare the console for initialization of the shards.
            Console.WriteLine();
            Console.WriteLine("Initializing shards...");

            // Initialize the shards themselves using the service provider coded in below.
            using (var services = ConfigureServices(JessShardConfig))
            {
                jess = services.GetRequiredService<DiscordShardedClient>();

                // Handle incoming messages.
                jess.MessageReceived += ProgramCycle;

                // Initialize and load up each shard!
                // Set playing status!
                await jess.SetGameAsync($"TESTING");

                // Arm each shard.
                await jess.SetStatusAsync(UserStatus.Invisible);
                jess.ShardReady += BotArmed;

                // Once shards are armed, prepare them to execute code.
                for (int i = 0; i < jess.Shards.Count; i++)
                {
                    // For each shard, set the "playing" status.
                    var CurrentShard = jess.Shards.ToList().ElementAt(i);
                    await CurrentShard.SetGameAsync($"TESTING SHARD {CurrentShard.ShardId}");

                    // For each shard, acknowledge that it has been started.
                    Console.WriteLine($"Shard {CurrentShard.ShardId} was initialized at {DateTime.Now}");
                }

                // Login and start!
                await jess.LoginAsync(TokenType.Bot, File.ReadAllText("token.lgtx"));
                await jess.StartAsync();

                // This tells this Task to wait indefinitely for instructions.
                // The -1 delay value keeps the program in this state so it doesn't close on its own.
                await Task.Delay(-1);
            }

            #endregion
        }

        // Service provider for MainAsync() and its shard handling.
        // Cloned from: https://github.com/discord-net/Discord.Net/blob/dev/samples/03_sharded_client/Program.cs
        private ServiceProvider ConfigureServices(DiscordSocketConfig config)
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(config))
                // .AddSingleton<CommandService>()
                // .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
        }

        // Used to take each shard of the bot online when ready.
        private Task BotArmed(DiscordSocketClient currentShard)
        {
            // Log to console.
            Console.WriteLine();
            Console.WriteLine($"Shard {currentShard.ShardId} is fully armed. Going online.");
            Console.WriteLine($"Armed at {DateTime.Now}");

            // Sets the shard online.
            currentShard.SetStatusAsync(UserStatus.Online);
            return Task.CompletedTask;
        }

        #region PROGRAM CYCLE

        // Default functionality of the program after all initialization is completed.
        public async Task ProgramCycle(SocketMessage e)
        {
            // Initialize basic JDL data.
            var users = JessbotDatabaseLibrary.JessUsersList;
            var userInfo = JessbotDatabaseLibrary.JessUsersInfoList;

            // Log in the console that a message was received.
            Console.WriteLine();
            Console.WriteLine($"New message detected on shard {jess.GetShardIdFor((e.Channel as SocketGuildChannel).Guild)}. Analyzing...");

            // Before doing anything, was user registered?
            if (!users.Contains(e.Author.Id) && !e.Author.IsBot)
            {
                // User is not registered. Add them to the database.
                users.Add(e.Author.Id);
                userInfo.Add(new JessbotUserData(e.Author.Id));

                // Log to the console.
                Console.WriteLine("Whoops! Message author has not been registered. Adding to the database.");
                Console.WriteLine($"Database updated and now contains {users.Count} known users. [ Updated: {DateTime.Now} ]");
            }
        }

        #endregion

        #endregion

        #endregion
    }
}