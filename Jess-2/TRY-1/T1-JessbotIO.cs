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

using Jessbot;
using Jessbot.Data;

namespace Jessbot.IO
{   
    /* This is the IO code.
    -* This will be used as a library for calls to:
    -* Load from text files
    -* Save to text files
    -* Transfer data between .cs files
    -*/
    class IOLibrary
    {
        public Task InitialDatabaseLoad()
        {
            // Ensure that a string for bot pathing is set up.
            string BotPathing = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;

            // Initializes shorthand variables to make code shorter.
            #region SHORTHANDS

            // Server data
            var guilds = JessbotDatabaseLibrary.JessGuildsList;
            var guildNames = JessbotDatabaseLibrary.JessGuildNamesList;

            // Server settings
            var guildWelcomes = JessbotDatabaseLibrary.JessGuildWelcomeChannels;
            var guildStartRoles = JessbotDatabaseLibrary.JessGuildWelcomeRoles;
            var guildModLogs = JessbotDatabaseLibrary.JessGuildModChannels;
            var guildPrefixes = JessbotDatabaseLibrary.JessGuildPrefixes;

            var guildInvites = JessbotDatabaseLibrary.JessGuildInviteToggles;
            var guildsPublic = JessbotDatabaseLibrary.JessGuildPublicToggles;

            var guildJoins = JessbotDatabaseLibrary.JessGuildJoinToggles;
            var guildBans = JessbotDatabaseLibrary.JessGuildBanToggles;

            // User data
            var users = JessbotDatabaseLibrary.JessUsersList;
            var userInfo = JessbotDatabaseLibrary.JessUsersInfoList;

            #endregion

            // Notify operator through console that loading process, and loading of servers, is starting.
            Console.WriteLine();
            Console.WriteLine("Initializing loading process...");
            Console.WriteLine("Guild database loading process initializing...");

            // Load in the server database.
            string DatabaseDirPath = Path.Combine(BotPathing, "Debug/ADV_HIERARCHY/DATABASES");
            string[] ServerDatabaseLoad = File.ReadAllLines($"{DatabaseDirPath}/JESSBOT_SERVER_DATABASE.ptsfx");
            for (int i = 0; i < ServerDatabaseLoad.Length; i += 22)
            {
                guilds.Add(ulong.Parse(ServerDatabaseLoad[i + 2]));
                if (i + 21 < ServerDatabaseLoad.Length)
                {
                    // =====REGISTERED SERVER=====
                    // SERVER ID:
                    // ALREADY GATHERED, NEXT STEP
                    // SERVER NAME:
                    guildNames.Add(ServerDatabaseLoad[i + 4]);
                    // ENTRY CHANNEL:
                    guildWelcomes.Add(ulong.Parse(ServerDatabaseLoad[i + 2]), ulong.Parse(ServerDatabaseLoad[i + 6]));
                    // ENTRY ROLE:
                    guildStartRoles.Add(ulong.Parse(ServerDatabaseLoad[i + 2]), ulong.Parse(ServerDatabaseLoad[i + 8]));
                    // MODERATION CHANNEL:
                    guildModLogs.Add(ulong.Parse(ServerDatabaseLoad[i + 2]), ulong.Parse(ServerDatabaseLoad[i + 10]));
                    // SERVER PREFIX:
                    guildPrefixes.Add(ulong.Parse(ServerDatabaseLoad[i + 2]), ServerDatabaseLoad[i + 12]);
                    // INVITES ALLOWED:
                    guildInvites.Add(ulong.Parse(ServerDatabaseLoad[i + 2]), bool.Parse(ServerDatabaseLoad[i + 14]));
                    // USERLIST CHANGE MESSAGE TOGGLING:
                    guildJoins.Add(ulong.Parse(ServerDatabaseLoad[i + 2]), bool.Parse(ServerDatabaseLoad[i + 16]));
                    // USERBAN MESSAGE TOGGLING:
                    guildBans.Add(ulong.Parse(ServerDatabaseLoad[i + 2]), bool.Parse(ServerDatabaseLoad[i + 18]));
                    // PRIVATE TOGGLING:
                    guildsPublic.Add(ulong.Parse(ServerDatabaseLoad[i + 2]), bool.Parse(ServerDatabaseLoad[i + 20]));
                }
            }

            // Notify operator through console that loading of servers has finished and that user loading is next.
            Console.WriteLine($"Guild database loading process completed. {guilds.Count} guilds were loaded in. [ Completed: {DateTime.Now} ]");
            Console.WriteLine("User database loading process initializing...");

            // Load in the user database.
            string[] UserDatabaseLoad = File.ReadAllLines($"{DatabaseDirPath}/JESSBOT_USER_DATABASE.ptsfx");
            for (int i = 0; i < UserDatabaseLoad.Length; i += 6)
            {
                users.Add(ulong.Parse(UserDatabaseLoad[i + 4]));
                if (i + 5 < UserDatabaseLoad.Length)
                {
                    // =====REGISTERED USER=====
                    // USER NAME:
                    // UNNECESSARY INFORMATION, CAN BE GATHERED BY CODE, BUT HELPS WITH READABILITY
                    // USER ID:
                    // WE ALREADY HAVE THIS
                    
                    // ===> MOVE ONTO ADDING THE USER'S FULL DATA TO THE DATABASE!
                    string UniqueDirPath = Path.Combine(BotPathing, "Debug/ADV_HIERARCHY/USERS/" + UserDatabaseLoad[i + 4].ToString());
                    string[] UserInfoLoad = File.ReadAllLines($"{UniqueDirPath}/{UserDatabaseLoad[i + 4]}.ptsfx");
                    for (int j = 0; j < UserInfoLoad.Length; j += 8)
                    {
                        userInfo.Add(new JessbotUserData(ulong.Parse(UserDatabaseLoad[i + 4])));
                        if (j + 7 < UserInfoLoad.Length)
                        {
                            // =====[ INSERT USERNAME # PIN HERE ]=====
                            // USER ID:
                            // UNNECESSARY. ALREADY HAVE THIS INFORMATION
                            // EXPERIENCE VALUE:
                            userInfo.ElementAt(userInfo.Count - 1).SetExperience(BigInteger.Parse(UserInfoLoad[j + 4]), false);
                            // MONETARY VALUE:
                            userInfo.ElementAt(userInfo.Count - 1).SetMonetaryValue(BigInteger.Parse(UserInfoLoad[j + 6]));
                        }
                    }

                    string ExpSubdirPath = Path.Combine(UniqueDirPath, "EXPERIENCE");
                    string[] UserExpPropsDataLoad = File.ReadAllLines($"{ExpSubdirPath}/exp_props_{UserDatabaseLoad[i + 4]}.ptsfx");
                    for (int j = 1; j < UserExpPropsDataLoad.Length; j += 2)
                    {
                        if (j + 1 < UserExpPropsDataLoad.Length)
                        {
                            userInfo.ElementAt(userInfo.Count - 1).GetJUXA().properties[ulong.Parse(UserExpPropsDataLoad[j])] = bool.Parse(UserExpPropsDataLoad[j + 1]);
                        }
                    }

                    string[] UserExpUnlocksDataLoad = File.ReadAllLines($"{ExpSubdirPath}/exp_unlocks_{UserDatabaseLoad[i + 4]}.ptsfx");
                    for (int j = 1; j < UserExpUnlocksDataLoad.Length; j += 2)
                    {
                        if (j + 1 < UserExpUnlocksDataLoad.Length)
                        {
                            userInfo.ElementAt(userInfo.Count - 1).GetJUXA().unlocks[ulong.Parse(UserExpUnlocksDataLoad[j])] = bool.Parse(UserExpUnlocksDataLoad[j + 1]);
                        }
                    }

                    string EconSubdirPath = Path.Combine(UniqueDirPath, "ECONOMY");
                    string[] UserEconPropsDataLoad = File.ReadAllLines($"{EconSubdirPath}/econ_props_{UserDatabaseLoad[i + 4]}.ptsfx");
                    for (int j = 1; j < UserEconPropsDataLoad.Length; j += 2)
                    {
                        if (j + 1 < UserEconPropsDataLoad.Length)
                        {
                            userInfo.ElementAt(userInfo.Count - 1).GetJUCA().properties[ulong.Parse(UserEconPropsDataLoad[j])] = bool.Parse(UserEconPropsDataLoad[j + 1]);
                        }
                    }

                    string[] UserEconTiersDataLoad = File.ReadAllLines($"{EconSubdirPath}/econ_tiers_{UserDatabaseLoad[i + 4]}.ptsfx");
                    for (int j = 1; j < UserEconTiersDataLoad.Length; j += 2)
                    {
                        if (j + 1 < UserEconTiersDataLoad.Length)
                        {
                            userInfo.ElementAt(userInfo.Count - 1).GetJUCA().tiers[ulong.Parse(UserEconTiersDataLoad[j])] = bool.Parse(UserEconTiersDataLoad[j + 1]);
                        }
                    }
                }
            }

            // Notify operator through console that loading process has completed.
            Console.WriteLine($"User database loading process completed. {users.Count} users were loaded in. [ Completed: {DateTime.Now} ]");
            Console.WriteLine($"Loading process successfully completed at {DateTime.Now}");

            return Task.CompletedTask;
        }

        public Task InitializeLevels(uint maxLevel, uint minThresh, uint threshI)
        {
            // Makes code easier to read.
            var levels = Jessbot.CONST_TotalLevels;

            // Notify operator through console that this process is starting.
            Console.WriteLine();
            Console.WriteLine("Initializing levelling system...");

            // Set up the levelling system. Ensure it is not already initialized first, just in case.
            if (levels.Count != 0)
            { levels.Clear(); }
            for (uint i = 0; i <= maxLevel; ++i)
            {
                if (i == 0)
                { levels.Add(0, 0); } // This is the base starting level. No threshold. You will always be, at minimum, level 0.
                else
                {
                    // Calculate an exponential curve!
                    uint byTwos = i / 2;
                    uint byThrees = i / 3;
                    uint byFives = i / 5;
                    uint byTens = i / 10;
                    uint byTwentyFives = i / 25;
                    uint byFifties = i / 50;
                    uint byHundreds = i / 100;
                    BigInteger exponentialCurve = (byTwos + byThrees + byFives + byTens + byTwentyFives + byFifties + byHundreds);
                    
                    // Exponential curve must not fall below one.
                    if (exponentialCurve <= 0)
                    { exponentialCurve = 1; }

                    // Finally make the curve fully exponential! Start from level 5 for this.
                    if (i >= 5)
                    {
                        // Normal increment calculator.
                        BigInteger exponentialIncrementation = (BigInteger)Math.Pow(1.02, i - 4);

                        // If the level is currently below 25, ease into the exponent a little bit more.
                        if (i < 25)
                        { exponentialIncrementation -= (BigInteger)Math.Pow(1.015, 25 - i); }

                        // Finally, increment the curve!
                        exponentialCurve += exponentialIncrementation;
                    }
                    else // The curve should not begin yet.
                    { exponentialCurve = 1; }

                    // Perform the calculations for this level.
                    BigInteger ThreshValue = minThresh; // Set the level base to 5000.
                    ThreshValue += threshI * (i - 1) * exponentialCurve; // Add 2500 * (i - 1) * exponentialCurve to increase the valuation.

                    // Save this level and proceed to the next.
                    levels.Add(i, ThreshValue);
                }
            }

            // Notify operator through console that this process has completed.
            Console.WriteLine($"Maximum level is level {maxLevel}.");
            Console.WriteLine($"{levels[(uint)levels.Count - 1].ToString()} experience needed to reach.");
            Console.WriteLine($"Levelling system successfully initialized at {DateTime.Now}");

            return Task.CompletedTask;
        }

        public Task DatabaseSave()
        {
            return Task.CompletedTask;
        }
    }
}
