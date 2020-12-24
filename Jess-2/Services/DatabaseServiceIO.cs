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

using Jessbot;
using Jessbot.Entities;

namespace Jessbot.Services
{
    partial class DatabaseService
    {
        public void Load()
        {
            // Log to console.
            Logger.DataLoadLog(false, LoadFuncs.Pathing, null); // No input. Set to null.

            // Ensure that a string for bot pathing is set up.
            string BotPathing = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;

            // Log to console.
            Logger.DataLoadLog(true, LoadFuncs.Pathing, null); // Directly post the path.
            Logger.DataLoadLog(false, LoadFuncs.Guild, null); // No input. Set to null.

            // Load in the server database.
            string DatabaseDirPath = Path.Combine(BotPathing, "Debug/ADV_HIERARCHY/DATABASES");
            string[] ServerDatabaseLoad = File.ReadAllLines($"{DatabaseDirPath}/JESSBOT_SERVER_DATABASE.ptsfx");
            for (int i = 0; i < ServerDatabaseLoad.Length; i += 22)
            {
                // Set up a collector list to pass into a new guild profile.
                List<object> DataPass = new List<object>();

                // Handle the collector.
                DataPass.Add(ulong.Parse(ServerDatabaseLoad[i + 2]));
                if (i + 21 < ServerDatabaseLoad.Length)
                {
                    // =====REGISTERED SERVER=====
                    // SERVER ID:
                    // ALREADY GATHERED, NEXT STEP
                    // SERVER NAME:
                    DataPass.Add(ServerDatabaseLoad[i + 4]);
                    // ENTRY CHANNEL:
                    DataPass.Add(ulong.Parse(ServerDatabaseLoad[i + 6]));
                    // ENTRY ROLE:
                    DataPass.Add(ulong.Parse(ServerDatabaseLoad[i + 8]));
                    // MODERATION CHANNEL:
                    DataPass.Add(ulong.Parse(ServerDatabaseLoad[i + 10]));
                    // SERVER PREFIX:
                    DataPass.Add(ServerDatabaseLoad[i + 12]);
                    // INVITES ALLOWED:
                    DataPass.Add(bool.Parse(ServerDatabaseLoad[i + 14]));
                    // USERLIST CHANGE MESSAGE TOGGLING:
                    DataPass.Add(bool.Parse(ServerDatabaseLoad[i + 16]));
                    // USERBAN MESSAGE TOGGLING:
                    DataPass.Add(bool.Parse(ServerDatabaseLoad[i + 18]));
                    // PRIVATE TOGGLING:
                    DataPass.Add(bool.Parse(ServerDatabaseLoad[i + 20]));
                }

                // Add to the guild database and prepare to start over.
                _guilds.Add(new ServerProfile(DataPass));
                DataPass.Clear();
            }

            // Log to console.
            Logger.DataLoadLog(true, LoadFuncs.Guild, _guilds.Count); // Number of guilds.
        }
    }
}
