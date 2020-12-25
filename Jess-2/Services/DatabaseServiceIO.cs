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
    public partial class DatabaseService
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
            Logger.DataLoadLog(false, LoadFuncs.User, null); // No input. Set to null.

            // Load in the user database.
            string[] UserDatabaseLoad = File.ReadAllLines($"{DatabaseDirPath}/JESSBOT_USER_DATABASE.ptsfx");
            for (int i = 0; i < UserDatabaseLoad.Length; i += 6)
            {
                // Set up a collector list to pass into a new guild profile.
                List<object> DataPass = new List<object>();

                // Set up another collector list, plus three specific collectors for:
                // 1. User Experience Profile
                // 2. User Economy Profile
                // 3. User Inventory Profile
                // These specific collectors will be used to complete the user profile
                // by passing them in through the collector list defined here, which
                // is then passed into DataPass.
                List<object> SubProfiles = new List<object>();
                List<object> ExpProfPass = new List<object>();
                List<object> EcnProfPass = new List<object>();
                List<object> InvProfPass = new List<object>();

                // Handle the collector.
                DataPass.Add(ulong.Parse(UserDatabaseLoad[i + 4]));
                if (i + 5 < UserDatabaseLoad.Length)
                {
                    // =====REGISTERED USER=====
                    // USER NAME:
                    // UNNECESSARY INFORMATION, CAN BE GATHERED BY CODE, BUT HELPS WITH READABILITY
                    // USER ID:
                    // WE ALREADY HAVE THIS
                    
                    // Handle the subprofile collectors.
                    // Start with the main user profile.
                    string UserDir = Path.Combine(BotPathing, "Debug/ADV_HIERARCHY/USERS/" + UserDatabaseLoad[i + 4].ToString());
                    string[] UserDataLoad = File.ReadAllLines($"{UserDir}/{UserDatabaseLoad[i + 4]}.ptsfx");
                    for (int j = 0; j < UserDataLoad.Length; j += 10)
                    {
                        // You do not need to create a new user profile like in Jess2-INITIALVERSION.cs.
                        // This is handled instead with a single data pass, which is far more efficient.
                        if (j + 9 < UserDataLoad.Length)
                        {
                            // =====[ INSERT USERNAME # PIN HERE ]=====
                            // USER ID:
                            // UNNECESSARY. ALREADY HAVE THIS INFORMATION
                            // EXPERIENCE VALUE:
                            DataPass.Add(BigInteger.Parse(UserDataLoad[j + 4]));
                            // MONETARY VALUE:
                            DataPass.Add(BigInteger.Parse(UserDataLoad[j + 6]));
                            // EXPERIENCE LEVEL:
                            DataPass.Add(ulong.Parse(UserDataLoad[j + 8]));
                        }
                    }

                    // ---------------------- WHITESPACE FOR READABILITY ---------------------- 

                    // Handling experience subprofile.
                    // Be ready to pass this into the SubProfiles collector!
                    // Create two subcollectors first. This will make it easier
                    // to process the profile data.
                    List<bool> ExpPrfPrprts = new List<bool>(); // Properties
                    List<bool> ExpPrfUnloks = new List<bool>(); // Unlocks

                    // First handle properties of the experience subprofile.
                    string ExpSubDir = Path.Combine(UserDir, "EXPERIENCE");
                    string[] ExpPropsLoad = File.ReadAllLines($"{ExpSubDir}/exp_props_{UserDatabaseLoad[i + 4]}.ptsfx");
                    for (int j = 1; j < ExpPropsLoad.Length; j += 2)
                    {
                        if (j + 1 < ExpPropsLoad.Length)
                        {
                            ExpPrfPrprts.Add(bool.Parse(ExpPropsLoad[j + 1]));
                        }
                    }

                    // Now handle unlocks of the experience subprofile.
                    string[] ExpUnlocksLoad = File.ReadAllLines($"{ExpSubDir}/exp_unlocks_{UserDatabaseLoad[i + 4]}.ptsfx");
                    for (int j = 1; j < ExpUnlocksLoad.Length; j += 2)
                    {
                        if (j + 1 < ExpUnlocksLoad.Length)
                        {
                            ExpPrfUnloks.Add(bool.Parse(ExpUnlocksLoad[j + 1]));
                        }
                    }

                    // Passer data is ready, prepare passer and use it!
                    ExpProfPass.Add(ExpPrfPrprts);
                    ExpProfPass.Add(ExpPrfUnloks);
                    SubProfiles.Add(new ExpProfile(ExpProfPass));

                    // Now that the passer has been used, clear its data.
                    ExpProfPass.Clear(); ExpPrfPrprts.Clear(); ExpPrfUnloks.Clear();

                    // ---------------------- WHITESPACE FOR READABILITY ---------------------- 

                    // Handling economy subprofile.
                    // Be ready to pass this into the SubProfiles collector!
                    // Create two subcollectors first. This will make it easier
                    // to process the profile data.
                    List<bool> EcnPrfPrprts = new List<bool>(); // Properties
                    List<bool> EcnProfTiers = new List<bool>(); // Tiers

                    // First handle properties of the economy subprofile.
                    string EcnSubDir = Path.Combine(UserDir, "ECONOMY");
                    string[] EcnPropsLoad = File.ReadAllLines($"{EcnSubDir}/econ_props_{UserDatabaseLoad[i + 4]}.ptsfx");
                    for (int j = 1; j < EcnPropsLoad.Length; j += 2)
                    {
                        if (j + 1 < EcnPropsLoad.Length)
                        {
                            EcnPrfPrprts.Add(bool.Parse(EcnPropsLoad[j + 1]));
                        }
                    }

                    // Now handle unlocks of the economy subprofile.
                    string[] EcnTiersLoad = File.ReadAllLines($"{EcnSubDir}/econ_unlocks_{UserDatabaseLoad[i + 4]}.ptsfx");
                    for (int j = 1; j < EcnTiersLoad.Length; j += 2)
                    {
                        if (j + 1 < EcnTiersLoad.Length)
                        {
                            EcnProfTiers.Add(bool.Parse(EcnTiersLoad[j + 1]));
                        }
                    }

                    // Passer data is ready, prepare passer and use it!
                    EcnProfPass.Add(EcnPrfPrprts);
                    EcnProfPass.Add(EcnProfTiers);
                    SubProfiles.Add(new EconProfile(EcnProfPass));

                    // Now that the passer has been used, clear its data.
                    EcnProfPass.Clear(); EcnPrfPrprts.Clear(); EcnProfTiers.Clear();

                    // ---------------------- WHITESPACE FOR READABILITY ---------------------- 

                    // Handling inventory subprofile.
                    // Be ready to pass this into the SubProfiles collector!
                    // Create multiple subcollectors first. This will make it easier
                    // to process the profile data.
                    // TODO: Prepare inventory data collectors

                    // TODO: Collect inventory from file

                    // Passer data is ready, prepare passer and use it!
                    // TODO: Place inventory data in passer
                    SubProfiles.Add(new InvProfile(InvProfPass));

                    // Now that the passer has been used, clear its data.
                    InvProfPass.Clear();

                    // ---------------------- WHITESPACE FOR READABILITY ---------------------- 

                    // SubProfile list has been completed.
                    // Pass it into the main passer as your final value.
                    DataPass.Add(SubProfiles);
                }

                // Add to the guild database and prepare to start over.
                _users.Add(new UserProfile(DataPass));
                DataPass.Clear();
                SubProfiles.Clear();
            }

            // Log to console.
            Logger.DataLoadLog(true, LoadFuncs.User, _users.Count); // Number of users.
        }
    }
}
