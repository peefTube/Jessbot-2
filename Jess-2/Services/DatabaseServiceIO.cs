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

            // Load in the server database, but only if told.
            string DatabaseDirPath = Path.Combine(BotPathing, "Debug/ADV_HIERARCHY/DATABASES"); // Establish a directory path.
            if (Jessbot._loadGuilds)
            {
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
                    _guilds.Add((ulong)DataPass[0], new ServerProfile(DataPass));
                    DataPass.Clear();
                }
            }

            // Log to console.
            Logger.DataLoadLog(true, LoadFuncs.Guild, _guilds.Count); // Number of guilds.
            Logger.DataLoadLog(false, LoadFuncs.User, null); // No input. Set to null.

            // Load in the user database, but only if told.
            if (Jessbot._loadUsers)
            {
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
                        for (int j = 0; j < UserDataLoad.Length; j += 14)
                        {
                            // You do not need to create a new user profile like in Jess2-INITIALVERSION.cs.
                            // This is handled instead with a single data pass, which is far more efficient.
                            if (j + 13 < UserDataLoad.Length)
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
                                // COLOR PREFERENCE:
                                DataPass.Add(UserDataLoad[j + 10]);
                                // UTC CODE:
                                DataPass.Add(UserDataLoad[j + 12]);
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
                        string[] ExpPropsLoad = File.ReadAllLines($"{ExpSubDir}/EXP_P_{UserDatabaseLoad[i + 4]}.ptsfx");
                        for (int j = 1; j < ExpPropsLoad.Length; j += 2)
                        {
                            if (j + 1 < ExpPropsLoad.Length)
                            {
                                ExpPrfPrprts.Add(bool.Parse(ExpPropsLoad[j + 1]));
                            }
                        }

                        // Now handle unlocks of the experience subprofile.
                        string[] ExpUnlocksLoad = File.ReadAllLines($"{ExpSubDir}/EXP_U_{UserDatabaseLoad[i + 4]}.ptsfx");
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
                        string[] EcnPropsLoad = File.ReadAllLines($"{EcnSubDir}/ECON_P_{UserDatabaseLoad[i + 4]}.ptsfx");
                        for (int j = 1; j < EcnPropsLoad.Length; j += 2)
                        {
                            if (j + 1 < EcnPropsLoad.Length)
                            {
                                EcnPrfPrprts.Add(bool.Parse(EcnPropsLoad[j + 1]));
                            }
                        }

                        // Now handle unlocks of the economy subprofile.
                        string[] EcnTiersLoad = File.ReadAllLines($"{EcnSubDir}/ECON_T_{UserDatabaseLoad[i + 4]}.ptsfx");
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
                    _users.Add((ulong)DataPass[0], new UserProfile(DataPass));
                    DataPass.Clear();
                    SubProfiles.Clear();
                }
            }

            // Log to console.
            Logger.DataLoadLog(true, LoadFuncs.User, _users.Count); // Number of users.
        }

        public void Save()
        {
            // Ensure that a string for bot pathing is set up.
            string BotPathing = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;

            // Make sure the directory path is saved.
            string DatabaseDirPath = Path.Combine(BotPathing, "Debug/ADV_HIERARCHY/DATABASES");
            System.IO.Directory.CreateDirectory(DatabaseDirPath); // Create the database directory if it doesn't exist.

            // Server data saving.
            // Delete the file beforehand.
            File.Delete($"{DatabaseDirPath}/JESSBOT_SERVER_DATABASE.ptsfx");

            // Write the server data to a .ptsfx "database."
            using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{DatabaseDirPath}/JESSBOT_SERVER_DATABASE.ptsfx", true))
            {
                // Note that .DataPass() is used here and not for users.
                // User data is very complex and needs special treatment, whereas this can
                // be passed back pretty simply.
                foreach (ulong i in _guilds.Keys)
                {
                    file.WriteLine("=====REGISTERED SERVER=====");          // i = 0
                    file.WriteLine("SERVER ID:");                           // i = 1
                    file.WriteLine((ulong)_guilds[i].DataPass()[0]);        // i = 2
                    file.WriteLine("SERVER NAME:");                         // i = 3
                    file.WriteLine((string)_guilds[i].DataPass()[1]);       // i = 4
                    file.WriteLine("ENTRY CHANNEL:");                       // i = 5
                    file.WriteLine((ulong)_guilds[i].DataPass()[2]);        // i = 6
                    file.WriteLine("ENTRY ROLE:");                          // i = 7
                    file.WriteLine((ulong)_guilds[i].DataPass()[3]);        // i = 8
                    file.WriteLine("MODERATION CHANNEL:");                  // i = 9
                    file.WriteLine((ulong)_guilds[i].DataPass()[4]);        // i = 10
                    file.WriteLine("SERVER PREFIX:");                       // i = 11
                    file.WriteLine((string)_guilds[i].DataPass()[5]);       // i = 12
                    file.WriteLine("INVITES ALLOWED:");                     // i = 13
                    file.WriteLine((bool)_guilds[i].DataPass()[6]);         // i = 14
                    file.WriteLine("USERLIST CHANGE MESSAGE TOGGLING:");    // i = 15
                    file.WriteLine((bool)_guilds[i].DataPass()[7]);         // i = 16
                    file.WriteLine("USERBAN MESSAGE TOGGLING:");            // i = 17
                    file.WriteLine((bool)_guilds[i].DataPass()[8]);         // i = 18
                    file.WriteLine("PRIVATE TOGGLING:");                    // i = 19
                    file.WriteLine((bool)_guilds[i].DataPass()[9]);         // i = 20
                    file.WriteLine("");                                     // i = 21
                }
            }

            // User data saving.
            // Delete the file beforehand.
            File.Delete($"{DatabaseDirPath}/JESSBOT_USER_DATABASE.ptsfx");

            // Write the server data to a .ptsfx "database."
            using (System.IO.StreamWriter dbFile = new System.IO.StreamWriter($"{DatabaseDirPath}/JESSBOT_USER_DATABASE.ptsfx", true))
            {
                // Note: you're going to be writing several files here.
                foreach (ulong i in _users.Keys)
                {
                    // DO THIS FIRST!
                    // It is possible the bot cannot find this user.
                    // In this situation, ensure saving doesn't break.
                    // Store the username information in a string.
                    string username = "";
                    string usernameTitle = "";
                    try
                    { username = $"{_bot.GetUser(i).Username}#{_bot.GetUser(i).Discriminator}"; }
                    catch (Exception)
                    { username = $"COULD NOT FIND"; }

                    // Keep with the convention of the database.
                    usernameTitle = username.ToUpper();

                    // Write the file.
                    dbFile.WriteLine("=====REGISTERED USER=====");                                            // i = 0
                    dbFile.WriteLine("USER NAME:");                                                           // i = 1
                    dbFile.WriteLine(username);                                                               // i = 2
                    dbFile.WriteLine("USER ID:");                                                             // i = 3
                    dbFile.WriteLine(i);                                                                      // i = 4
                    dbFile.WriteLine();                                                                       // i = 5

                    // Write the user's file.
                    string UserDir = Path.Combine(BotPathing, "Debug/ADV_HIERARCHY/USERS/" + i);
                    System.IO.Directory.CreateDirectory(UserDir); // Create the user's directory if it doesn't exist.
                    File.Delete($"{UserDir}/{i}.ptsfx"); // Delete beforehand, otherwise you'll append to an existing file.
                    using (System.IO.StreamWriter uFile = new System.IO.StreamWriter($"{UserDir}/{i}.ptsfx", true))
                    {
                        // Before writing the file, handle any multi-value items, such as RGB.
                        string RGB = _users[i].PrefColor.R + "," + _users[i].PrefColor.G + "," + _users[i].PrefColor.B;

                        // Write the file.
                        uFile.WriteLine($"====={usernameTitle}=====");                                            // i = 0
                        uFile.WriteLine("USER ID:");                                                         // i = 1
                        uFile.WriteLine(i);                                                                  // i = 2
                        uFile.WriteLine("EXPERIENCE VALUE:");                                                // i = 3
                        uFile.WriteLine(_users[i].Experience.ToString());                                    // i = 4
                        uFile.WriteLine("MONETARY VALUE:");                                                  // i = 5
                        uFile.WriteLine(_users[i].Balance.ToString());                                       // i = 6
                        uFile.WriteLine("EXPERIENCE LEVEL:");                                                // i = 7
                        uFile.WriteLine(_users[i].Level.ToString());                                         // i = 8
                        uFile.WriteLine("COLOR PREFERENCE:");                                                // i = 9
                        uFile.WriteLine(RGB);                                                                // i = 10
                        uFile.WriteLine("UTC CODE:");                                                        // i = 11
                        uFile.WriteLine(_users[i].UserUTC);                                                  // i = 12
                        uFile.WriteLine();                                                                   // i = 13
                    }

                    // Create the subdirectory for experience files.
                    string ExpSubDir = Path.Combine(UserDir, "EXPERIENCE");
                    System.IO.Directory.CreateDirectory(ExpSubDir);

                    // Delete the existing files so you don't append onto them.
                    File.Delete($@"{ExpSubDir}/EXP_P_{i}.ptsfx");
                    File.Delete($@"{ExpSubDir}/EXP_U_{i}.ptsfx");
                    File.Delete($@"{ExpSubDir}/readme.txt");

                    // Rebuild the files.
                    using (System.IO.StreamWriter readme = new System.IO.StreamWriter($@"{ExpSubDir}/readme.txt", true))
                    {
                        // Write a readme so users understand what each file does.
                        readme.WriteLine($"EXPERIENCE PROFILE DATA FOR USER WITH ID {i} AND USERNAME \"{username}\"");
                        readme.WriteLine($"The .ptsfx files in this directory correlate to the experience profile of this user by " +
                            $"their ID. Here is, per file, what each is meant to contain:");
                        readme.WriteLine($"\"EXP_P_{i}.PTSFX\": This file contains the purchases boosting the user's experience gain.");
                        readme.WriteLine($"\"EXP_U_{i}.PTSFX\": This file contains the unlocks the user has earned by their experience level.");
                    }

                    using (System.IO.StreamWriter expProperties = new System.IO.StreamWriter($@"{ExpSubDir}/EXP_P_{i}.ptsfx", true))
                    {
                        expProperties.WriteLine($"====={usernameTitle} EXP PROPERTIES=====");    // i = 0

                        for (ulong p = 0; p < (ulong)_users[i].ExpData.Properties.Count; p++)
                        {
                            ulong property = _users[i].ExpData.Properties.Keys.ElementAt((int)p);
                            bool isActive  = _users[i].ExpData.Properties.Values.ElementAt((int)p);

                            expProperties.WriteLine(property);                              // i = 1 + (2 * p)
                            expProperties.WriteLine(isActive);                              // i = 2 + (2 * p)
                        }
                    }

                    using (System.IO.StreamWriter expUnlocks = new System.IO.StreamWriter($@"{ExpSubDir}/EXP_U_{i}.ptsfx", true))
                    {
                        expUnlocks.WriteLine($"====={usernameTitle} EXP UNLOCKS=====");    // i = 0

                        for (ulong p = 0; p < (ulong)_users[i].ExpData.Unlocks.Count; p++)
                        {
                            ulong unlock  = _users[i].ExpData.Unlocks.Keys.ElementAt((int)p);
                            bool isActive = _users[i].ExpData.Unlocks.Values.ElementAt((int)p);

                            expUnlocks.WriteLine(unlock);                                // i = 1 + (2 * p)
                            expUnlocks.WriteLine(isActive);                              // i = 2 + (2 * p)
                        }
                    }

                    // Create the subdirectory for economy files.
                    string EconSubDir = Path.Combine(UserDir, "ECONOMY");
                    System.IO.Directory.CreateDirectory(EconSubDir);

                    // Delete the existing files so you don't append onto them.
                    File.Delete($@"{EconSubDir}/ECON_P_{i}.ptsfx");
                    File.Delete($@"{EconSubDir}/ECON_T_{i}.ptsfx");
                    File.Delete($@"{EconSubDir}/readme.txt");

                    // Rebuild the files.
                    using (System.IO.StreamWriter readme = new System.IO.StreamWriter($@"{EconSubDir}/readme.txt", true))
                    {
                        // Write a readme so users understand what each file does.
                        readme.WriteLine($"ECONOMY PROFILE DATA FOR USER WITH ID {i} AND USERNAME \"{username}\"");
                        readme.WriteLine($"The .ptsfx files in this directory correlate to the economy profile of this user by " +
                            $"their ID. Here is, per file, what each is meant to contain:");
                        readme.WriteLine($"\"ECON_P_{i}.PTSFX\": This file contains the purchases boosting the user's balance gain.");
                        readme.WriteLine($"\"ECON_T_{i}.PTSFX\": This file contains the tiers the user has unlocked.");
                    }

                    using (System.IO.StreamWriter econProperties = new System.IO.StreamWriter($@"{EconSubDir}/ECON_P_{i}.ptsfx", true))
                    {
                        econProperties.WriteLine($"====={usernameTitle} ECON PROPERTIES=====");    // i = 0

                        for (ulong p = 0; p < (ulong)_users[i].EconData.Properties.Count; p++)
                        {
                            ulong property = _users[i].EconData.Properties.Keys.ElementAt((int)p);
                            bool isActive  = _users[i].EconData.Properties.Values.ElementAt((int)p);

                            econProperties.WriteLine(property);                              // i = 1 + (2 * p)
                            econProperties.WriteLine(isActive);                              // i = 2 + (2 * p)
                        }
                    }

                    using (System.IO.StreamWriter econTiers = new System.IO.StreamWriter($@"{EconSubDir}/ECON_T_{i}.ptsfx", true))
                    {
                        econTiers.WriteLine($"====={usernameTitle} ECON TIERS=====");    // i = 0

                        for (ulong p = 0; p < (ulong)_users[i].EconData.Tiers.Count; p++)
                        {
                            ulong tier    = _users[i].EconData.Tiers.Keys.ElementAt((int)p);
                            bool isActive = _users[i].EconData.Tiers.Values.ElementAt((int)p);

                            econTiers.WriteLine(tier);                                  // i = 1 + (2 * p)
                            econTiers.WriteLine(isActive);                              // i = 2 + (2 * p)
                        }
                    }

                    // Create the subdirectory for inventory files
                    string InvSubDir = Path.Combine(UserDir, "INVENTORY");
                    System.IO.Directory.CreateDirectory(InvSubDir);

                    // Delete the existing files so you don't append onto them.
                    File.Delete($@"{InvSubDir}/INV_TL_{i}.ptsfx");
                    File.Delete($@"{InvSubDir}/INV_FDS_{i}.ptsfx");
                    File.Delete($@"{InvSubDir}/INV_RM_{i}.ptsfx");
                    File.Delete($@"{InvSubDir}/readme.txt");

                    // Rebuild the files.
                    using (System.IO.StreamWriter readme = new System.IO.StreamWriter($@"{InvSubDir}/readme.txt", true))
                    {
                        // Write a readme so users understand what each file does.
                        readme.WriteLine($"INVENTORY PROFILE DATA FOR USER WITH ID {i} AND USERNAME \"{username}\"");
                        readme.WriteLine($"The .ptsfx files in this directory correlate to the inventory profile of this user by " +
                            $"their ID. Here is, per file, what each is meant to contain:");
                        readme.WriteLine($"\"INV_TL_{i}.PTSFX\": This file contains the tools section of the user's inventory.");
                        readme.WriteLine($"\"INV_FDS_{i}.PTSFX\": This file contains the foodstuff section of the user's inventory.");
                        readme.WriteLine($"\"INV_RM_{i}.PTSFX\": This file contains the raw materials section of the user's inventory.");
                    }

                    using (System.IO.StreamWriter tools = new System.IO.StreamWriter($@"{InvSubDir}/INV_TL_{i}.ptsfx", true))
                    {
                        tools.WriteLine($"====={usernameTitle} TOOL AMOUNTS=====");    // i = 0

                        for (ulong p = 0; p < (ulong)_users[i].Inventory.TL_TOOLS.Count; p++)
                        {
                            ulong tool          = _users[i].Inventory.TL_TOOLS.Keys.ElementAt((int)p);
                            BigInteger amount   = _users[i].Inventory.TL_TOOLS.Values.ElementAt((int)p);

                            tools.WriteLine(tool);                                // i = 1 + (2 * p)
                            tools.WriteLine(amount);                              // i = 2 + (2 * p)
                        }
                    }

                    using (System.IO.StreamWriter foods = new System.IO.StreamWriter($@"{InvSubDir}/INV_FDS_{i}.ptsfx", true))
                    {
                        foods.WriteLine($"====={usernameTitle} FOODSTUFF AMOUNTS=====");    // i = 0

                        for (ulong p = 0; p < (ulong)_users[i].Inventory.FDS_FOODSTUFFS.Count; p++)
                        {
                            ulong food        = _users[i].Inventory.FDS_FOODSTUFFS.Keys.ElementAt((int)p);
                            BigInteger amount = _users[i].Inventory.FDS_FOODSTUFFS.Values.ElementAt((int)p);

                            foods.WriteLine(food);                                // i = 1 + (2 * p)
                            foods.WriteLine(amount);                              // i = 2 + (2 * p)
                        }
                    }

                    using (System.IO.StreamWriter mats = new System.IO.StreamWriter($@"{InvSubDir}/INV_RM_{i}.ptsfx", true))
                    {
                        mats.WriteLine($"====={usernameTitle} RAWMAT AMOUNTS=====");    // i = 0

                        for (ulong p = 0; p < (ulong)_users[i].Inventory.RM_RAWMATS.Count; p++)
                        {
                            ulong mat         = _users[i].Inventory.RM_RAWMATS.Keys.ElementAt((int)p);
                            BigInteger amount = _users[i].Inventory.RM_RAWMATS.Values.ElementAt((int)p);

                            mats.WriteLine(mat);                                 // i = 1 + (2 * p)
                            mats.WriteLine(amount);                              // i = 2 + (2 * p)
                        }
                    }
                }
            }
        }
    }
}
