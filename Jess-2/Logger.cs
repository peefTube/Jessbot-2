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

using Microsoft.Extensions.DependencyInjection;

using Jessbot.Services;

namespace Jessbot
{
    class Logger
    {
        // Directly post to the Console.
        // It is advised NOT to use this unless absolutely necessary.
        public static void Post(string input)
        {
            Console.WriteLine(input, Color.DarkerGrey);
        }

        // Alternative to Post with additional parameter for specific usage points.
        public static void Post(string input, Color colorState)
        {
            Console.WriteLine(input, colorState);
        }

        // Check against the InDev constant in Jessbot.cs
        public static void DevWarning()
        {
            if (Jessbot.InDev)
            {
                Console.WriteLine();
                Console.WriteLine($"================== [ WARNING ] =================", Color.Gold);
                Console.WriteLine("The current client build is in development.", Color.Gold);
                Console.WriteLine("Some functionality may be incomplete or broken.", Color.Gold);
            }
        }

        // Initialization segment
        public static void Initialize()
        {
            Console.WriteLine();
            Console.WriteLine($"============== [ Initialization ] ==============", Color.LightGrey);
        }

        // Initialization status
        public static void InitStatus(bool newChunk, bool isComplete, InitType phase)
        {
            // NOTE: newChunk should always be set to true for the first line of a section.
            //       However, the if-then for it should be linked to the phase of that section.
            //       Phase zero should always be below the section statement, thus requiring
            //       no new line at the beginning, as the section header (LogInitialize() in this
            //       case) will do this task instead.

            // Splitter, adds whitespace
            // DO NOT RUN ON FIRST PHASE
            if (newChunk && phase != 0) { Console.WriteLine(); }

            // Switch case for phase of initialization
            switch (phase)
            {
                // Client initialization
                case InitType.Client:
                    if (!isComplete) { Console.WriteLine("Initializing client...", Color.DarkerGrey); }
                    else { Console.WriteLine($"Client initialized. [ {DateTime.Now} ]", Color.DarkerGrey); }
                    break;
                // Commands initialization
                case InitType.Commands:
                    if (!isComplete) { Console.WriteLine("Initializing commands logic...", Color.DarkerGrey); }
                    else { Console.WriteLine($"Commands initialized. [ {DateTime.Now} ]", Color.DarkerGrey); }
                    break;
                // DI initialization
                case InitType.Inject:
                    if (!isComplete) { Console.WriteLine("Initializing dependency injector...", Color.DarkerGrey); }
                    else { Console.WriteLine($"DI initialized. [ {DateTime.Now} ]", Color.DarkerGrey); }
                    break;
            }
        }

        // Service initialization
        public static void InitService(ServiceType service)
        {
            string serviceName = "";

            // Which service is this being run by?
            switch (service)
            {
                // The command handling service is being initialized.
                case ServiceType.CommandHandling:
                    serviceName = "Command Handling Service";
                    Console.WriteLine($"Initializing {serviceName}...", Color.DarkBlue);
                    break;
                // The database services are being initialized.
                case ServiceType.Database:
                    serviceName = "Database Services";
                    Console.WriteLine($"Initializing {serviceName}...", Color.DarkBlue);
                    break;
                // The message service is being initialized.
                case ServiceType.Messaging:
                    serviceName = "Message Service";
                    Console.WriteLine($"Initializing {serviceName}...", Color.DarkBlue);
                    break;
            }
        }
        
        // MainAsync() segment
        public static void AsyncStarted()
        {
            Console.WriteLine();
            Console.WriteLine($"================ [ Main Async ] ================", Color.Blue);
            Console.WriteLine($"The MainAsync() operation has several stages.", Color.DarkBlue);
            Console.WriteLine($"Each stage will have its own unique subsection.", Color.DarkBlue);
            Console.WriteLine($"The first stage should begin shortly.", Color.DarkBlue);
        }

        // Main Async status
        public static void AsyncStatus(bool isComplete, MainAsyncS stage)
        {
            // Which stage are we on?
            switch (stage)
            {
                // We are loading the database..
                case MainAsyncS.Load:
                    // Splitter.
                    if (!isComplete)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"=============== [ Database Load ] ==============", Color.LightGrey);
                        Console.WriteLine($"Initializing load process...", Color.DarkGrey);
                    }
                    // Async stage complete, don't split.
                    else
                    { Console.WriteLine($"Database loading completed. [ {DateTime.Now} ]", Color.DarkGreen); }
                    break;
                // We are registering message functionality..
                case MainAsyncS.MessagesInit:
                    // Splitter.
                    if (!isComplete)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"================ [ Message Init ] ==============", Color.LightGrey);
                        Console.WriteLine($"Initializing message logic...", Color.DarkGrey);
                    }
                    // Async stage complete, don't split.
                    else
                    { Console.WriteLine($"Message logic initialized.", Color.DarkGreen); }
                    break;
                // We are finalizing the login process.
                case MainAsyncS.Login:
                    // Splitter.
                    if (!isComplete)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"================= [ Logging In ] ===============", Color.LightGrey);
                        Console.WriteLine($"Logging in...", Color.DarkGrey);
                    }
                    // Async stage complete, don't split.
                    else
                    { Console.WriteLine($"Bot is logged in.", Color.DarkGreen); }
                    break;
            }
        }

        // Loading process logging
        public static void DataLoadLog(bool isComplete, LoadFuncs step, object input)
        {
            // Which step are we on?
            switch (step)
            {
                // We are pathing.
                case LoadFuncs.Pathing:
                    if (!isComplete)
                    { Console.WriteLine($"[LOAD:PATH] Determining base path...", Color.DarkerGrey); }
                    else
                    { Console.WriteLine($"[LOAD:PATH] Path determined. Assuming next sequence.", Color.DarkerGrey); }
                    break;
                // We are loading guilds.
                case LoadFuncs.Guild:
                    if (!isComplete)
                    { Console.WriteLine($"[LOAD:GUILD] Loading guilds...", Color.DarkerGrey); }
                    else
                    {
                        if (input != null)
                        { Console.WriteLine($"[LOAD:GUILD] Guild load complete. [ {input} GUILDS ]", Color.DarkerGrey); }
                        else
                        { Console.WriteLine($"[LOAD:GUILD] Error loading guilds. [ NO GUILDS ]", Color.DarkRed); }
                        Console.WriteLine($"[LOAD:GUILD] Assuming next sequence.", Color.DarkerGrey);
                    }
                    break;
            }
        }

        // Successful stage completion
        public static void StageSuccess(Stages stage)
        {
            // Which stage are we on?
            switch (stage)
            {
                // We are initializing.
                case Stages.Initialize:
                    Console.WriteLine();
                    Console.WriteLine($"[ STAGE SUCCESS : INIT ]", Color.Green);
                    Console.WriteLine($">> Initialization complete. [ {DateTime.Now} ]", Color.Green);
                    break;
                // We are on MainAsync().
                case Stages.Async:
                    Console.WriteLine();
                    Console.WriteLine($"[ STAGE SUCCESS : ASYNC ]", Color.Green);
                    Console.WriteLine($">> Async setup complete. [ {DateTime.Now} ]", Color.Green);
                    break;
            }
        }

        // Message processor functionality
        public static void MessageStep(MsgStep step, bool isBot = false)
        {
            // Which step are we on?
            switch (step)
            {
                // Message was detected.
                case MsgStep.Detection:
                    Console.WriteLine();
                    Console.WriteLine($"============= [ Message Detected ] =============", Color.LightGrey);
                    Console.WriteLine($"New message detected. Analyzing...", Color.DarkerGrey);
                    break;
                // We are checking if the message was written by a bot.
                case MsgStep.IsBot:
                    Console.WriteLine($"Determining if author is a bot...", Color.DarkerGrey);
                    // Message was written by a bot.
                    if (isBot)
                    { Console.WriteLine($"Message author is a bot. Analysis concluded.", Color.DarkTeal); }
                    // Message was written by a user.
                    else
                    { Console.WriteLine($"Message author is not a bot. Proceeding...", Color.Teal); }
                    break;
            }
        }
    }

    #region ENUMS

    public enum Stages
    {
        Initialize,
        Async,
    }

    public enum MainAsyncS
    {
        Load,
        MessagesInit,
        Login,
    }

    public enum LoadFuncs
    {
        Pathing,
        Guild,
        User,
        UExp,
        UEcon,
        UInv,
    }

    public enum ServiceType
    {
        CommandHandling,
        Database,
        Messaging,
    }

    public enum InitType
    {
        Client,
        Commands,
        Inject
    }

    public enum MsgStep
    {
        Detection,
        IsBot,
    }

    #endregion
}
