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

namespace JessbotINITIAL
{
    #region MAIN BOT CODE

    class DiscordBot
    {
        #region dataFields

        // Version information
        static string JESS_CONSTANT_VERSION = "v1.9.9.005";
        static string JESS_CONSTANT_VERSION_DATE = "June 10, 2020";
        static string JESS_CONSTANT_VERSION_NAME = "The Rebirth Update (Pre-Release)";
        static string JESS_CONSTANT_VERSION_INFO = $"This update ( **\"{JESS_CONSTANT_VERSION_NAME}\"** ), {JESS_CONSTANT_VERSION}, updated on {JESS_CONSTANT_VERSION_DATE}, " + "restarted development of JessBot on the Discordant placeholder bot to clean up code and hopefully make the bot more effective and updatable. Added admin overview functionality.";

        // CREATE RANDOMIZER
        Random RNG = new Random();

        // ASYNC HANDLING VARIABLES
        #region ASYNC HANDLER

        // BANNED?
        static bool ifBan = false;

        // FALLBACKS
        public static bool JESS_FALLBACK_USER_LEVELLED_UP = false;

        #endregion

        // IMPORTANT BOT INFORMATION
        #region CONTENT VARIABLES

        // Bot owner list
        static ulong[] JESS_BOT_OWNERSHIP_REGISTRY = { 236738543387541507, 553420930244673536, 559942856249442305 };

        // Defaults
        static string JESS_DEFAULT_PREFIX = "JR."; // Used to build new servers

        // Constants
        static uint CONST_JESS_MAX_LEVEL_NUM = 500; // Caps off the maximum level here
        static uint CONST_JESS_MIN_EXPERIENCE_THRESH = 5000; // Minimum experience threshold to reach level 1
        static uint CONST_JESS_EXPERIENCE_THRESH_INCREMENT = 2500; // Every level goes up by this much minimum necessary experience
        static uint CONST_JESS_EXPERIENCE_THRESH_EXPONENT = 50; // Base exponent for increasing how much the threshold per level is - strikingly high at higher levels
        public static Dictionary<uint, BigInteger> CONST_JESS_LEVEL_TOTAL_NUMS = new Dictionary<uint, BigInteger>(); // Stores numeric values of each threshold for reaching a particular level

        static string jb = $"{ new Emoji("<:jessbucks:561818526923620353>").ToString() }";

        // Data storage
        static List<ulong> JESS_REGISTERED_GUILDS = new List<ulong>(); // Stores registered guilds, important list
        static List<string> JESS_REGISTERED_GUILD_NAMES = new List<string>(); // Stores registered guilds in string format, important list

        static Dictionary<ulong, ulong> CUSTOM_ENTRY_CHANNELS = new Dictionary<ulong, ulong>(); // Stores guild ID and welcome channel ID, must be set by owner or administration

        static Dictionary<ulong, ulong> CUSTOM_ENTRY_ROLES = new Dictionary<ulong, ulong>(); // Stores guild ID and base role, must be set by owner or administration

        static Dictionary<ulong, ulong> CUSTOM_MOD_CHANNELS = new Dictionary<ulong, ulong>(); // Stores guild ID and mod channel ID, must be set by owner or administration

        static Dictionary<ulong, string> CUSTOM_GUILD_PREFIXES = new Dictionary<ulong, string>(); // Stores guild ID and prefix, must be set by owner or administration

        static Dictionary<ulong, bool> GUILD_ALLOWS_INVITES = new Dictionary<ulong, bool>(); // Stores guild ID and true/false for allowing invites, must be set by owner or administration

        static Dictionary<ulong, bool> GUILD_TOGGLE_JOINMSG = new Dictionary<ulong, bool>(); // Stores whether join messages are allowed in this server
        static Dictionary<ulong, bool> GUILD_TOGGLE_BANSMSG = new Dictionary<ulong, bool>(); // Stores whether ban messages are allowed in this server

        public static List<ulong> JESS_REGISTERED_USERS = new List<ulong>(); // Stores all users. This is a master list that is used to populate the user data.
        public static List<UserReferendum> JESS_REGISTERED_USERS_DATA = new List<UserReferendum>(); // Correlates with the above.

        #endregion

        // Main program
        static DiscordSocketClient jess;

        #endregion

        // Start program
        #region start

        // This is pretty normal. Start the actual .exe then start the bot.
        static void Main(string[] args)
        {
            Console.WriteLine($"Jessica Bot II - {JESS_CONSTANT_VERSION}");

            new DiscordBot().MainAsync().GetAwaiter().GetResult();
        }

        // Startup code
        public DiscordBot()
        {
            // Set up the levelling system
            for (uint i = 0; i <= CONST_JESS_MAX_LEVEL_NUM; ++i)
            {
                if (i == 0)
                { CONST_JESS_LEVEL_TOTAL_NUMS.Add(0, 0); } // This is the base starting level. No threshold. You will always be, at minimum, level 0.
                else
                {
                    BigInteger threshold_value = CONST_JESS_MIN_EXPERIENCE_THRESH; // Set the level base to 5000.
                    threshold_value += CONST_JESS_EXPERIENCE_THRESH_INCREMENT * (i - 1); // Add 2500 * (i - 1) to increase the valuation.

                    if (i >= 5)
                    { threshold_value += new BigInteger((int)Math.Pow(CONST_JESS_EXPERIENCE_THRESH_EXPONENT, i - 4)); } // Starting from level 5, add 50 to the power of (i - 4) to more dramatically increase the valuation.

                    // Save this level and proceed to the next
                    CONST_JESS_LEVEL_TOTAL_NUMS.Add(i, threshold_value);
                }
            }

            // Not quite sure EXACTLY what this does but it is definitely necessary
            jess = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 100
            });

            // Set playing status
            jess.SetGameAsync("TESTING");

            // Login and start
            jess.LoginAsync(TokenType.Bot, File.ReadAllText("token.lgtx"));
            jess.StartAsync();
        }

        #endregion

        // Program run cycle
        #region MainAsync()

        private async Task MainAsync()
        {
            // If a message is received, send it to the parser for commands handling and message processing
            jess.MessageReceived += ParseCommand;

            #region USER HANDLING

            // If a user joins, handle properly
            jess.UserJoined += ProcessUserJoin;

            // If a user leaves, handle properly
            ifBan = false; // Make sure this is set before running the commands

            jess.UserBanned += ProcessUserBan; // Ban must be run first

            if (!ifBan)
            {
                // User wasn't already banned. Go ahead and run
                jess.UserLeft += ProcessUserDrop;
            }

            jess.JoinedGuild += ProcessGuildJoin;

            #endregion

            // Load cycle
            #region LOAD_IN

            string[] SERVER_DB_LOADIN = File.ReadAllLines("JESSBOT_SERVER_DATABASE.lgtx");
            for (int i = 0; i < SERVER_DB_LOADIN.Length; i += 20)
            {
                JESS_REGISTERED_GUILDS.Add(ulong.Parse(SERVER_DB_LOADIN[i + 2]));
                if (i + 19 < SERVER_DB_LOADIN.Length)
                {
                    // =====REGISTERED SERVER=====
                    // SERVER ID:
                    // ALREADY GATHERED, NEXT STEP
                    // SERVER NAME:
                    JESS_REGISTERED_GUILD_NAMES.Add(SERVER_DB_LOADIN[i + 4]);
                    // ENTRY CHANNEL:
                    CUSTOM_ENTRY_CHANNELS.Add(ulong.Parse(SERVER_DB_LOADIN[i + 2]), ulong.Parse(SERVER_DB_LOADIN[i + 6]));
                    // ENTRY ROLE:
                    CUSTOM_ENTRY_ROLES.Add(ulong.Parse(SERVER_DB_LOADIN[i + 2]), ulong.Parse(SERVER_DB_LOADIN[i + 8]));
                    // MODERATION CHANNEL:
                    CUSTOM_MOD_CHANNELS.Add(ulong.Parse(SERVER_DB_LOADIN[i + 2]), ulong.Parse(SERVER_DB_LOADIN[i + 10]));
                    // SERVER PREFIX:
                    CUSTOM_GUILD_PREFIXES.Add(ulong.Parse(SERVER_DB_LOADIN[i + 2]), SERVER_DB_LOADIN[i + 12]);
                    // INVITES ALLOWED:
                    GUILD_ALLOWS_INVITES.Add(ulong.Parse(SERVER_DB_LOADIN[i + 2]), bool.Parse(SERVER_DB_LOADIN[i + 14]));
                    // USERLIST CHANGE MESSAGE TOGGLING:
                    GUILD_TOGGLE_JOINMSG.Add(ulong.Parse(SERVER_DB_LOADIN[i + 2]), bool.Parse(SERVER_DB_LOADIN[i + 16]));
                    // USERBAN MESSAGE TOGGLING:
                    GUILD_TOGGLE_BANSMSG.Add(ulong.Parse(SERVER_DB_LOADIN[i + 2]), bool.Parse(SERVER_DB_LOADIN[i + 18]));
                }
            }

            // CHECK DIRECTORY
            string botPathing = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;

            string[] USER_DB_LOADIN = File.ReadAllLines("JESSBOT_USER_DATABASE.lgtx");
            for (int i = 0; i < USER_DB_LOADIN.Length; i += 6)
            {
                JESS_REGISTERED_USERS.Add(ulong.Parse(USER_DB_LOADIN[i + 4]));
                if (i + 5 < USER_DB_LOADIN.Length)
                {
                    // =====REGISTERED USER=====
                    // USER NAME:
                    // UNNECESSARY INFORMATION, CAN BE GATHERED BY CODE, BUT HELPS WITH READABILITY
                    // USER ID:
                    // WE ALREADY HAVE THIS

                    // ===> MOVE ONTO ADDING THE USER'S FULL DATA TO THE DATABASE!
                    string uniqueDirectory = Path.Combine(botPathing, "Debug/ADV_HIERARCHY/USERS/" + USER_DB_LOADIN[i + 4].ToString());
                    string[] USER_DATA_LOADIN = File.ReadAllLines($"{uniqueDirectory}/{USER_DB_LOADIN[i + 4]}.lgtx");
                    for (int j = 0; j < USER_DATA_LOADIN.Length; j += 8)
                    {
                        JESS_REGISTERED_USERS_DATA.Add(new UserReferendum(ulong.Parse(USER_DB_LOADIN[i + 4])));
                        if (j + 7 < USER_DATA_LOADIN.Length)
                        {
                            // =====[ INSERT USERNAME # PIN HERE ]=====
                            // USER ID:
                            // UNNECESSARY. ALREADY HAVE THIS INFORMATION
                            // EXPERIENCE VALUE:
                            JESS_REGISTERED_USERS_DATA.ElementAt(JESS_REGISTERED_USERS_DATA.Count - 1).SetExperience(BigInteger.Parse(USER_DATA_LOADIN[j + 4]), false);
                            // MONETARY VALUE:
                            JESS_REGISTERED_USERS_DATA.ElementAt(JESS_REGISTERED_USERS_DATA.Count - 1).SetMonetaryValue(BigInteger.Parse(USER_DATA_LOADIN[j + 6]));
                        }
                    }

                    string expSubDirectory = Path.Combine(uniqueDirectory, "EXPERIENCE");
                    string[] USER_EXPDATA_PROPS_LOADIN = File.ReadAllLines($"{expSubDirectory}/exp_props_{USER_DB_LOADIN[i + 4]}.lgtx");
                    for (int j = 1; j < USER_EXPDATA_PROPS_LOADIN.Length; j += 2)
                    {
                        if (j + 1 < USER_EXPDATA_PROPS_LOADIN.Length)
                        {
                            JESS_REGISTERED_USERS_DATA.ElementAt(JESS_REGISTERED_USERS_DATA.Count - 1).userExperienceProperties.properties[ulong.Parse(USER_EXPDATA_PROPS_LOADIN[j])] = bool.Parse(USER_EXPDATA_PROPS_LOADIN[j + 1]);
                        }
                    }

                    string[] USER_EXPDATA_UNLOCKS_LOADIN = File.ReadAllLines($"{expSubDirectory}/exp_unlocks_{USER_DB_LOADIN[i + 4]}.lgtx");
                    for (int j = 1; j < USER_EXPDATA_UNLOCKS_LOADIN.Length; j += 2)
                    {
                        if (j + 1 < USER_EXPDATA_UNLOCKS_LOADIN.Length)
                        {
                            JESS_REGISTERED_USERS_DATA.ElementAt(JESS_REGISTERED_USERS_DATA.Count - 1).userExperienceProperties.unlocks[ulong.Parse(USER_EXPDATA_UNLOCKS_LOADIN[j])] = bool.Parse(USER_EXPDATA_UNLOCKS_LOADIN[j + 1]);
                        }
                    }

                    string econSubDirectory = Path.Combine(uniqueDirectory, "ECONOMY");
                    string[] USER_ECONDATA_PROPS_LOADIN = File.ReadAllLines($"{econSubDirectory}/econ_props_{USER_DB_LOADIN[i + 4]}.lgtx");
                    for (int j = 1; j < USER_ECONDATA_PROPS_LOADIN.Length; j += 2)
                    {
                        if (j + 1 < USER_ECONDATA_PROPS_LOADIN.Length)
                        {
                            JESS_REGISTERED_USERS_DATA.ElementAt(JESS_REGISTERED_USERS_DATA.Count - 1).userEconomyProperties.properties[ulong.Parse(USER_ECONDATA_PROPS_LOADIN[j])] = bool.Parse(USER_ECONDATA_PROPS_LOADIN[j + 1]);
                        }
                    }

                    string[] USER_ECONDATA_TIERS_LOADIN = File.ReadAllLines($"{econSubDirectory}/econ_tiers_{USER_DB_LOADIN[i + 4]}.lgtx");
                    for (int j = 1; j < USER_ECONDATA_TIERS_LOADIN.Length; j += 2)
                    {
                        if (j + 1 < USER_ECONDATA_TIERS_LOADIN.Length)
                        {
                            JESS_REGISTERED_USERS_DATA.ElementAt(JESS_REGISTERED_USERS_DATA.Count - 1).userEconomyProperties.tiers[ulong.Parse(USER_ECONDATA_TIERS_LOADIN[j])] = bool.Parse(USER_ECONDATA_TIERS_LOADIN[j + 1]);
                        }
                    }
                }
            }

            #endregion

            // Unsure what this does
            await Task.Delay(-1);
        }

        #endregion

        //MAIN FUNCTIONALITY!!
        #region mainProgram

        // Parser is necessary for handling text and commands.
        public async Task ParseCommand(SocketMessage e)
        {
            // Before doing anything, was user registered?
            if (!JESS_REGISTERED_USERS.Contains(e.Author.Id) && !e.Author.IsBot)
            {
                JESS_REGISTERED_USERS.Add(e.Author.Id);
                JESS_REGISTERED_USERS_DATA.Add(new UserReferendum(e.Author.Id));
            }

            // Recognize commands!
            // Look for the prefix.
            string customPrefix = JESS_DEFAULT_PREFIX.ToLower();

            for (int i = 0; i < CUSTOM_GUILD_PREFIXES.Count; i++)
            {
                if ((e.Channel as SocketGuildChannel).Guild.Id.Equals(JESS_REGISTERED_GUILDS.ElementAt(i)))
                {
                    // FOUND THE PREFIX
                    customPrefix = CUSTOM_GUILD_PREFIXES[JESS_REGISTERED_GUILDS.ElementAt(i)].ToLower();

                    break;
                }
            }

            // Now that user handling was completed, and a custom prefix is found, handle the economy and experience values here as they need to be updated before commands are run. Make sure no command was run for the economy.
            // Handling of money and experience.
            #region ECONOMY AND EXPERIENCE

            // Check if commands were run. If not, it is safe to run income functionality.
            if (!e.Content.ToLower().StartsWith(customPrefix) && !e.Author.IsBot)
            {
                for (uint i = 0; i < JESS_REGISTERED_USERS.Count; ++i)
                {
                    if (JESS_REGISTERED_USERS[(int)i] == e.Author.Id)
                    {
                        ulong random_income_gain = 0;

                        #region BOOSTER HANDLING

                        // No booster
                        if (!JESS_REGISTERED_USERS_DATA[(int)i].userEconomyProperties.properties[0])
                        { random_income_gain = (ulong)RNG.Next(0, 3); }
                        
                        // Booster 1 active
                        else if (JESS_REGISTERED_USERS_DATA[(int)i].userEconomyProperties.properties[0] && !JESS_REGISTERED_USERS_DATA[(int)i].userEconomyProperties.properties[1])
                        { random_income_gain = (ulong)RNG.Next(1, 6); }

                        #endregion

                        JESS_REGISTERED_USERS_DATA[(int)i].SetMonetaryValue(JESS_REGISTERED_USERS_DATA[(int)i].GetMonetaryValue() + random_income_gain);
                    }
                }
            }

            // This code handles experience functionality.
            // Before doing anything, is user registered?
            if (JESS_REGISTERED_USERS.Contains(e.Author.Id) && !e.Author.IsBot)
            {
                // Yes. It is safe to update experience points.
                int random_experience_gain = 0;
                UserReferendum userBeingGivenExperience = null;

                for (uint i = 0; i < JESS_REGISTERED_USERS.Count; ++i)
                {
                    if (JESS_REGISTERED_USERS[(int)i] == e.Author.Id)
                    {
                        userBeingGivenExperience = JESS_REGISTERED_USERS_DATA[(int)i];
                    }
                }

                #region BOOSTER HANDLING

                // No booster
                if (!userBeingGivenExperience.userExperienceProperties.properties[0])
                {
                    random_experience_gain = RNG.Next(1, 6);

                    // Check for message sizes.
                    if (e.Content.Length >= 50)
                    { random_experience_gain += RNG.Next(0, 11); }
                    if (e.Content.Length >= 125)
                    { random_experience_gain += RNG.Next(0, 26); }
                    if (e.Content.Length >= 350)
                    { random_experience_gain += RNG.Next(0, 51); }
                    if (e.Content.Length >= 600)
                    { random_experience_gain += RNG.Next(0, 101); }
                    if (e.Content.Length >= 1250)
                    { random_experience_gain += RNG.Next(0, 201); }
                }

                // Booster 1 active
                else if (userBeingGivenExperience.userExperienceProperties.properties[0] && !userBeingGivenExperience.userExperienceProperties.properties[1])
                {
                    random_experience_gain = RNG.Next(5, 11);

                    // Check for message sizes.
                    if (e.Content.Length >= 50)
                    { random_experience_gain += RNG.Next(0, 26); }
                    if (e.Content.Length >= 125)
                    { random_experience_gain += RNG.Next(0, 51); }
                    if (e.Content.Length >= 350)
                    { random_experience_gain += RNG.Next(0, 101); }
                    if (e.Content.Length >= 600)
                    { random_experience_gain += RNG.Next(0, 151); }
                    if (e.Content.Length >= 1250)
                    { random_experience_gain += RNG.Next(0, 301); }
                }

                #endregion

                for (uint i = 0; i < JESS_REGISTERED_USERS.Count; ++i)
                {
                    if (JESS_REGISTERED_USERS[(int)i] == e.Author.Id)
                    {
                        JESS_REGISTERED_USERS_DATA[(int)i].SetExperience(JESS_REGISTERED_USERS_DATA[(int)i].GetExperience() + random_experience_gain);
                    }

                    if (JESS_FALLBACK_USER_LEVELLED_UP)
                    {
                        // OLD HANDLING
                        // await e.Channel.SendMessageAsync($"Congratulations, {e.Author.Mention}! You have levelled up to level **{JESS_REGISTERED_USERS_DATA[(int)i].GetExperienceLevel()}**");

                        EmbedBuilder POPUP_LEVELLED_UP = new EmbedBuilder();

                        POPUP_LEVELLED_UP.WithTitle("LEVELED UP!");
                        POPUP_LEVELLED_UP.AddField("NEW LEVEL!", $"Congratulations, <@{JESS_REGISTERED_USERS[(int)i]}>! Your new level is: **{JESS_REGISTERED_USERS_DATA[(int)i].GetExperienceLevel()}**. Keep going, you got this!", false);

                        if (JESS_REGISTERED_USERS_DATA[(int)i].GetExperienceLevel() == 2)
                        {
                            POPUP_LEVELLED_UP.AddField("UNLOCK!", $"Congratulations! You have reached **level 2** and **you are now able to access the store.**", true);

                            JESS_REGISTERED_USERS_DATA[(int)i].userExperienceProperties.unlocks[2] = true;
                        }

                        POPUP_LEVELLED_UP.WithColor(new Color(RNG.Next(100, 256), RNG.Next(110, 256), RNG.Next(110, 256)));
                        await jess.GetUser(JESS_REGISTERED_USERS[(int)i]).SendMessageAsync("", false, POPUP_LEVELLED_UP.Build());
                    }

                    JESS_FALLBACK_USER_LEVELLED_UP = false;

                    // Just in case, we should check the experience level of the user one more time and modify the unlocks values. We may have missed something.
                    if (JESS_REGISTERED_USERS_DATA[(int)i].GetExperienceLevel() >= 2)
                    {
                        JESS_REGISTERED_USERS_DATA[(int)i].userExperienceProperties.unlocks[2] = true;
                    }
                }
            }
            else
            {
                // No. Do absolutely nothing.
            }

            #endregion

            // SHOULD NEVER RUN FOR BOTS
            if (!e.Author.IsBot)
            {
                // Keep this collapsible.
                #region COMMANDS

                if (e.Content.ToLower().StartsWith(customPrefix))
                {

                    // If prefix is found, remove it from the message and proceed to check for which command was run
                    // Also ensure arguments can be handled and have a method for finding said arguments
                    string[] cmdArgs;
                    string command = e.Content.Remove(0, customPrefix.Length);
                    cmdArgs = command.Split(' ');
                    switch (cmdArgs[0])
                    {
                        case "db_users_in":
                            // USED BY BOT OWNER ONLY
                            if (JESS_BOT_OWNERSHIP_REGISTRY.Contains(e.Author.Id))
                            {
                                ulong usersAdded = 0;

                                // Run through all guilds JessBot is in and check its userlist against the recognized userlist
                                for (int x = 0; x < jess.Guilds.Count; x++)
                                {
                                    for (int y = 0; y < jess.Guilds.ElementAt(x).Users.Count; y++)
                                    {
                                        if (!JESS_REGISTERED_USERS.Contains(jess.Guilds.ElementAt(x).Users.ElementAt(y).Id) &&
                                            !jess.Guilds.ElementAt(x).Users.ElementAt(y).IsBot)
                                        {
                                            // Safe to add to userlist
                                            JESS_REGISTERED_USERS.Add(jess.Guilds.ElementAt(x).Users.ElementAt(y).Id);
                                            JESS_REGISTERED_USERS_DATA.Add(new UserReferendum(jess.Guilds.ElementAt(x).Users.ElementAt(y).Id));

                                            ++usersAdded; // Keep track of the number of users added!
                                        }
                                    }
                                }

                                // Notify command sender of how many users were added to the userlist.
                                await e.Channel.SendMessageAsync($"{usersAdded} users were added to the userlist.");
                            }
                            else
                            {
                                // User is not considered the bot owner, bar from using command
                                await e.Channel.SendMessageAsync("I'm afraid I am under no obligation to do that for you.");
                            }
                            break;

                        case "version":
                            // User is checking for current version!
                            await e.Channel.SendMessageAsync(JESS_CONSTANT_VERSION_INFO);
                            break;

                        #region SERVER REGISTRY COMMANDS

                        case "admin_introduction":
                            // Administrator-only command. Tells Jessica to introduce herself in the mentioned channel.
                            if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                            {
                                // Check to make sure the user has specified a channel
                                if (e.MentionedChannels.Count == 1)
                                {
                                    await (e.MentionedChannels.First() as ISocketMessageChannel).SendMessageAsync("Hello, everyone! I'm Jessica! I'm here to help assist with server moderation and provide some (hopefully) fun activities for y'all!");
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("Oops! I need a specific channel to work with!");
                                }
                            }
                            else
                            {
                                // User is not administrator, bar from using command
                                await e.Channel.SendMessageAsync("I'm afraid I am under no obligation to do that for you.");
                            }
                            break;

                        case "admin_registerserver":
                            // Administrator-only command. Registers the Discord with JessBot and runs the savedata
                            if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                            {
                                // User is administrator, run command successfully
                                bool serverPreviouslyRegistered = SV_CheckThenSave((e.Channel as SocketGuildChannel).Guild.Id);

                                if (serverPreviouslyRegistered)
                                {
                                    await e.Channel.SendMessageAsync("Oops! It looks like you already registered this server. I'll make sure to keep better tabs on that for you!");
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("Okay! I've added this server to my list of registered servers. Thank you!");
                                }
                            }
                            else
                            {
                                // User is not administrator, bar from using command
                                await e.Channel.SendMessageAsync("I'm afraid I am under no obligation to do that for you.");
                            }
                            break;

                        case "admin_setentrychannel":
                            // Administrator-only command. Sets the greeting channel to the current channel and runs the savedata
                            if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                            {
                                // User is administrator, run command successfully
                                bool serverPreviouslyRegistered = false;

                                // Check to make sure the user has specified a channel
                                if (e.MentionedChannels.Count == 0)
                                {
                                    // No channel specified, run normally
                                    serverPreviouslyRegistered = SV_CheckThenSave((e.Channel as SocketGuildChannel).Guild.Id, e.Channel.Id, "channel");
                                }
                                else if (e.MentionedChannels.Count == 1)
                                {
                                    // A channel was specified, run normally with added condition
                                    serverPreviouslyRegistered = SV_CheckThenSave((e.Channel as SocketGuildChannel).Guild.Id, e.MentionedChannels.ElementAt(0).Id, "channel");
                                }
                                else
                                {
                                    // Too many channels specified, display error and break
                                    await e.Channel.SendMessageAsync("I'm sorry, you've confused me! Can you be a bit more specific?");
                                    break;
                                }

                                if (serverPreviouslyRegistered)
                                {
                                    await e.Channel.SendMessageAsync("I've updated that information for you. Thank you!");
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("Oops! You haven't registered this server yet, but that's okay - I did it for you!");
                                }
                            }
                            else
                            {
                                // User is not administrator, bar from using command
                                await e.Channel.SendMessageAsync("I'm afraid I am under no obligation to do that for you.");
                            }
                            break;

                        case "admin_setmodchannel":
                            // Administrator-only command. Sets the moderator channel to the current channel and runs the savedata
                            if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                            {
                                // User is administrator, run command successfully
                                bool serverPreviouslyRegistered = false;

                                // Check to make sure the user has specified a channel
                                if (e.MentionedChannels.Count == 0)
                                {
                                    // No channel specified, run normally
                                    serverPreviouslyRegistered = SV_CheckThenSave((e.Channel as SocketGuildChannel).Guild.Id, e.Channel.Id, "channelMOD");
                                }
                                else if (e.MentionedChannels.Count == 1)
                                {
                                    // A channel was specified, run normally with added condition
                                    serverPreviouslyRegistered = SV_CheckThenSave((e.Channel as SocketGuildChannel).Guild.Id, e.MentionedChannels.ElementAt(0).Id, "channelMOD");
                                }
                                else
                                {
                                    // Too many channels specified, display error and break
                                    await e.Channel.SendMessageAsync("I'm sorry, you've confused me! Can you be a bit more specific?");
                                    break;
                                }

                                if (serverPreviouslyRegistered)
                                {
                                    await e.Channel.SendMessageAsync("I've updated that information for you. Thank you!");
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("Oops! You haven't registered this server yet, but that's okay - I did it for you!");
                                }
                            }
                            else
                            {
                                // User is not administrator, bar from using command
                                await e.Channel.SendMessageAsync("I'm afraid I am under no obligation to do that for you.");
                            }
                            break;

                        case "admin_setentryrole":
                            // Administrator-only command. Sets the entry role to the specified role and runs the savedata
                            if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                            {
                                // User is administrator, run command successfully
                                bool serverPreviouslyRegistered = false;

                                // Check to make sure the user has specified a role
                                if (e.MentionedRoles.Count == 0)
                                {
                                    // No roles mentioned, cancel action
                                    await e.Channel.SendMessageAsync("You've not given me any useful information!");
                                    break;
                                }
                                else if (e.MentionedRoles.Count == 1)
                                {
                                    // Role is mentioned, perform action
                                    serverPreviouslyRegistered = SV_CheckThenSave((e.Channel as SocketGuildChannel).Guild.Id, e.MentionedRoles.ElementAt(0).Id, "role");
                                }
                                else
                                {
                                    // User provided too many roles, cancel action
                                    await e.Channel.SendMessageAsync("I'm sorry, you've confused me! Can you be a bit more specific?");
                                    break;
                                }

                                if (serverPreviouslyRegistered)
                                {
                                    await e.Channel.SendMessageAsync("I've updated that information for you. Thank you!");
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("Oops! You haven't registered this server yet, but that's okay - I did it for you!");
                                }
                            }
                            else
                            {
                                // User is not administrator, bar from using command
                                await e.Channel.SendMessageAsync("I'm afraid I am under no obligation to do that for you.");
                            }
                            break;

                        case "admin_setprefix":
                            // Administrator-only command. Sets the prefix to the specified string and runs the savedata
                            if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                            {
                                // User is administrator, run command successfully
                                bool serverPreviouslyRegistered = false;

                                // Check to make sure the user has specified a prefix
                                if (cmdArgs.Length == 1)
                                {
                                    // No string given, cancel action
                                    await e.Channel.SendMessageAsync("You've not given me any useful information!");
                                    break;
                                }
                                else
                                {
                                    // String given, perform action
                                    serverPreviouslyRegistered = SV_CheckThenSave((e.Channel as SocketGuildChannel).Guild.Id, cmdArgs[1]);
                                }

                                if (serverPreviouslyRegistered)
                                {
                                    await e.Channel.SendMessageAsync($"I've updated that information for you. Thank you! (The current prefix is now: {CUSTOM_GUILD_PREFIXES[(e.Channel as SocketGuildChannel).Guild.Id]})");
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("Oops! You haven't registered this server yet, but that's okay - I did it for you!");
                                }
                            }
                            else
                            {
                                // User is not administrator, bar from using command
                                await e.Channel.SendMessageAsync("I'm afraid I am under no obligation to do that for you.");
                            }
                            break;

                        case "admin_openserver":
                            // Administrator-only command. Checks to see if the server is already open, and if not, ensures it is opened
                            if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                            {
                                // User is administrator, run command successfully
                                bool serverPreviouslyRegistered = false;
                                bool isAlreadyOpen = false;
                                bool openState = false;

                                // Check to see if the server is already open
                                bool[] serverOpenCheck = SV_CheckIfServerOpened((e.Channel as SocketGuildChannel).Guild.Id);

                                // If server doesn't exist, warn user and cut functionality
                                if (serverOpenCheck[0] == false)
                                {
                                    await e.Channel.SendMessageAsync("Oops! You haven't registered this server yet. In this circumstance, you will have to do that before you can continue.");
                                    break;
                                }

                                // Server does indeed exist, continue
                                isAlreadyOpen = serverOpenCheck[1];

                                // Perform action
                                bool[] advancedCheckAndSave = SV_CheckThenSave((e.Channel as SocketGuildChannel).Guild.Id, isAlreadyOpen, e);
                                serverPreviouslyRegistered = advancedCheckAndSave[0];
                                openState = advancedCheckAndSave[1];

                                if (serverPreviouslyRegistered)
                                {
                                    await e.Channel.SendMessageAsync($"I've updated that information for you. Thank you! (Server open status: {openState})");
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("Oops! You haven't registered this server yet, but that's okay - I did it for you!");
                                }
                            }
                            else
                            {
                                // User is not administrator, bar from using command
                                await e.Channel.SendMessageAsync("I'm afraid I am under no obligation to do that for you.");
                            }
                            break;

                        case "create_invite":
                            // Must be in an opened server. This necessitates use of the entry/greeting channel.
                            // NECESSARY! Make sure the user has put in an ID argument.
                            if (cmdArgs.Length == 2)
                            {
                                // Good! User has put in an ID argument. Now make sure it is VALID (ulong)
                                bool ID_validDefinition = false;
                                ulong ID_serverInviteLink = 0;

                                if (cmdArgs[1].ToLower().ElementAt(0) >= '0' && cmdArgs[1].ToLower().ElementAt(0) <= '9')
                                {
                                    ID_validDefinition = true;
                                }

                                // You have checked if the ID is valid.
                                if (ID_validDefinition && (cmdArgs[1].Length > 10))
                                {
                                    // Good! ID is valid!
                                    ID_serverInviteLink = ulong.Parse(cmdArgs[1]);
                                }
                                else
                                {
                                    // ID is invalid. Notify user.
                                    if (cmdArgs[1].ToLower().ElementAt(0) >= 'a' && cmdArgs[1].ToLower().ElementAt(0) <= 'z')
                                    {
                                        await e.Channel.SendMessageAsync("This isn't even a number! What are you thinking??");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessageAsync("I'm... not seeing this in my list. Maybe check the numbers again?");
                                    }
                                    break;
                                }

                                // ID is valid. Make sure it is in the list.
                                bool foundIDinList = false;

                                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                                {
                                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == ID_serverInviteLink)
                                    {
                                        // Found ID! Create (if not already existing) and post link!
                                        foundIDinList = true;

                                        // However, first make sure the server is even open - will appear broken otherwise!
                                        if (GUILD_ALLOWS_INVITES[JESS_REGISTERED_GUILDS.ElementAt(i)])
                                        {
                                            // SERVER IS INDEED OPEN!
                                            IInviteMetadata newInvite = await jess.GetGuild(ID_serverInviteLink).GetTextChannel(CUSTOM_ENTRY_CHANNELS[JESS_REGISTERED_GUILDS.ElementAt(i)]).CreateInviteAsync(null, null, false, false, null);
                                            await e.Channel.SendMessageAsync($"Here's your invite to {newInvite.GuildName}, {e.Author.Username}: {newInvite.Url}");
                                            break;
                                        }
                                        else
                                        {
                                            // SERVER NOT OPENED! Cancel action
                                            await e.Channel.SendMessageAsync("Ah... uh-oh. Well, uh. Bad news. Yes, that server is in the directory. However, I don't have permission to invite you. I apologize.");
                                            break;
                                        }
                                    }
                                }

                                // Didn't find the ID in the list.
                                if (!foundIDinList)
                                {
                                    await e.Channel.SendMessageAsync("Oops! It appears that this server is not in the directory. Sorry!");
                                }
                            }
                            else
                            {
                                // User failed to provide any information.
                                await e.Channel.SendMessageAsync("I don't have anything I can work with! Please give me a valid server ID.");
                            }
                            break;

                        #endregion

                        #region ADMINISTRATIVE COMMANDS

                        case "admin_overview":
                            // Provides an overview of the server
                            if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                            {
                                // User is administrator, run command successfully
                                bool serverPreviouslyRegistered = false;
                                serverPreviouslyRegistered = SV_CheckThenSave((e.Channel as SocketGuildChannel).Guild.Id);

                                // Continue?
                                if (serverPreviouslyRegistered)
                                {
                                    // Server is registered so it is okay to continue
                                    // Create and display AOS!
                                    EmbedBuilder SV_AOSBuilder = new EmbedBuilder();

                                    SV_AOSBuilder.WithTitle("Administrative Overview System");

                                    SV_AOSBuilder.AddField("Current Server", $"{(e.Channel as SocketGuildChannel).Guild.Name} **( {(e.Channel as SocketGuildChannel).Guild.Id} )**\nOwner: {(e.Channel as SocketGuildChannel).Guild.Owner.Username}#{(e.Channel as SocketGuildChannel).Guild.Owner.Discriminator}", false);

                                    SV_AOSBuilder.AddField("Server Prefix", CUSTOM_GUILD_PREFIXES[(e.Channel as SocketGuildChannel).Guild.Id], true);
                                    SV_AOSBuilder.AddField("Registered", "Yes", false);

                                    // User counts
                                    int humanUserCount = 0;
                                    int botUserCount = 0;
                                    for (int i = 0; i < (e.Channel as SocketGuildChannel).Guild.Users.Count; i++)
                                    {
                                        if (!(e.Channel as SocketGuildChannel).Guild.Users.ElementAt(i).IsBot)
                                        { humanUserCount++; }
                                        else
                                        { botUserCount++; }
                                    }

                                    SV_AOSBuilder.AddField("Total Users", (e.Channel as SocketGuildChannel).Guild.Users.Count, true);
                                    SV_AOSBuilder.AddField("Human Users", humanUserCount, true);
                                    SV_AOSBuilder.AddField("Bot Users", botUserCount, true);

                                    // Values
                                    bool entrySet = false;
                                    bool modSet = false;
                                    bool entryRoleSet = false;
                                    bool openedToPublic = false;

                                    ulong entryID = 0;
                                    ulong modID = 0;
                                    ulong entryRoleID = 0;

                                    string openedToPublicReadable = "Closed";

                                    bool userMsgOn = false;
                                    bool banMsgOn = false;

                                    // Check all values
                                    for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                                    {
                                        // Server is already registered, this is known - make sure you find the right server first though
                                        if (JESS_REGISTERED_GUILDS.ElementAt(i) == (e.Channel as SocketGuildChannel).Guild.Id)
                                        {
                                            // Entry channel
                                            entryID = CUSTOM_ENTRY_CHANNELS[JESS_REGISTERED_GUILDS.ElementAt(i)];
                                            if (entryID != 0) { entrySet = true; }

                                            // Mod channel
                                            modID = CUSTOM_MOD_CHANNELS[JESS_REGISTERED_GUILDS.ElementAt(i)];
                                            if (entryID != 0) { modSet = true; }

                                            // Entry role
                                            entryRoleID = CUSTOM_ENTRY_ROLES[JESS_REGISTERED_GUILDS.ElementAt(i)];
                                            if (entryID != 0) { entryRoleSet = true; }

                                            // Check if open
                                            openedToPublic = GUILD_ALLOWS_INVITES[JESS_REGISTERED_GUILDS.ElementAt(i)];
                                            if (openedToPublic) { openedToPublicReadable = "Open"; }

                                            // Togglers
                                            userMsgOn = GUILD_TOGGLE_JOINMSG[JESS_REGISTERED_GUILDS.ElementAt(i)];
                                            banMsgOn = GUILD_TOGGLE_BANSMSG[JESS_REGISTERED_GUILDS.ElementAt(i)];

                                            break;
                                        }
                                    }

                                    // Display values
                                    if (entrySet) { SV_AOSBuilder.AddField("Entry Channel", entryID, false); }
                                    else { SV_AOSBuilder.AddField("Entry Channel", "N/A", false); }

                                    if (modSet) { SV_AOSBuilder.AddField("Mod Channel", modID, false); }
                                    else { SV_AOSBuilder.AddField("Mod Channel", "N/A", false); }

                                    if (entryRoleSet) { SV_AOSBuilder.AddField("Entry Role", entryRoleID, false); }
                                    else { SV_AOSBuilder.AddField("Entry Role", "N/A", false); }

                                    if (openedToPublic) { SV_AOSBuilder.AddField("Publicity", openedToPublicReadable, false); }
                                    else { SV_AOSBuilder.AddField("Publicity", openedToPublicReadable, false); }

                                    if (userMsgOn) { SV_AOSBuilder.AddField("Userjoin Messages", "On", true); }
                                    else { SV_AOSBuilder.AddField("Userjoin Messages", "Off", true); }

                                    if (banMsgOn) { SV_AOSBuilder.AddField("Ban Messages", "On", true); }
                                    else { SV_AOSBuilder.AddField("Ban Messages", "Off", true); }

                                    SV_AOSBuilder.WithColor(Color.Gold);
                                    await e.Channel.SendMessageAsync("", false, SV_AOSBuilder.Build());
                                }
                                else
                                {
                                    // Server is not registered, however the system is still usable, albeit limited
                                    // Create and display AOS!
                                    EmbedBuilder SV_AOSBuilder = new EmbedBuilder();

                                    SV_AOSBuilder.WithTitle("Administrative Overview System");

                                    SV_AOSBuilder.AddField("Current Server", $"{(e.Channel as SocketGuildChannel).Guild.Name} **( {(e.Channel as SocketGuildChannel).Guild.Id} )**\nOwner: {(e.Channel as SocketGuildChannel).Guild.Owner.Username}#{(e.Channel as SocketGuildChannel).Guild.Owner.Discriminator}", false);

                                    SV_AOSBuilder.AddField("Server Prefix", JESS_DEFAULT_PREFIX, true);
                                    SV_AOSBuilder.AddField("Registered", "No", false);

                                    // User counts
                                    int humanUserCount = 0;
                                    int botUserCount = 0;
                                    for (int i = 0; i < (e.Channel as SocketGuildChannel).Guild.Users.Count; i++)
                                    {
                                        if (!(e.Channel as SocketGuildChannel).Guild.Users.ElementAt(i).IsBot)
                                        { humanUserCount++; }
                                        else
                                        { botUserCount++; }
                                    }

                                    SV_AOSBuilder.AddField("Total Users", (e.Channel as SocketGuildChannel).Guild.Users.Count, true);
                                    SV_AOSBuilder.AddField("Human Users", humanUserCount, true);
                                    SV_AOSBuilder.AddField("Bot Users", botUserCount, true);

                                    SV_AOSBuilder.WithColor(Color.Orange);
                                    await e.Channel.SendMessageAsync("", false, SV_AOSBuilder.Build());
                                }
                            }
                            else
                            {
                                // User is not administrator, bar from using command
                                await e.Channel.SendMessageAsync("I'm afraid I am under no obligation to do that for you.");
                            }
                            break;

                        #region TOGGLERS

                        case "admin_togglejoinmessages":
                            // Administrative command. Toggles user handling messages for the server.
                            if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                            {
                                // User is administrator, run command successfully
                                bool serverPreviouslyRegistered = false;
                                serverPreviouslyRegistered = ADM_CheckToggleThenSave((e.Channel as SocketGuildChannel).Guild.Id, "gen_userjoin");

                                // Continue functionality
                                if (serverPreviouslyRegistered)
                                {
                                    await e.Channel.SendMessageAsync($"I've updated that information for you. Thank you! Current status of userjoin message toggle is: {GUILD_TOGGLE_JOINMSG[(e.Channel as SocketGuildChannel).Guild.Id]}");
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync($"Oops! You haven't registered this server yet, but that's okay - I did it for you! Current status of userjoin message toggle is: {GUILD_TOGGLE_JOINMSG[(e.Channel as SocketGuildChannel).Guild.Id]}");
                                }
                            }
                            else
                            {
                                // User is not administrator, bar from using command
                                await e.Channel.SendMessageAsync("I'm afraid I am under no obligation to do that for you.");
                            }
                            break;

                        case "admin_togglebanmessages":
                            // Administrative command. Toggles user handling messages for the server.
                            if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                            {
                                // User is administrator, run command successfully
                                bool serverPreviouslyRegistered = false;
                                serverPreviouslyRegistered = ADM_CheckToggleThenSave((e.Channel as SocketGuildChannel).Guild.Id, "gen_userbans");

                                // Continue functionality
                                if (serverPreviouslyRegistered)
                                {
                                    await e.Channel.SendMessageAsync($"I've updated that information for you. Thank you! Current status of userban message toggle is: {GUILD_TOGGLE_BANSMSG[(e.Channel as SocketGuildChannel).Guild.Id]}");
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync($"Oops! You haven't registered this server yet, but that's okay - I did it for you! Current status of userban message toggle is: {GUILD_TOGGLE_BANSMSG[(e.Channel as SocketGuildChannel).Guild.Id]}");
                                }
                            }
                            else
                            {
                                // User is not administrator, bar from using command
                                await e.Channel.SendMessageAsync("I'm afraid I am under no obligation to do that for you.");
                            }
                            break;

                        #endregion

                        #endregion

                        #region SERVER DIRECTORY COMMAND

                        case "server_directory":
                            // Displays the list of registered servers with their IDs.
                            int dirIndexer = 0;
                            int dirPageIncrement = 0;
                            int pageValue = 0;
                            int totalPages = 0;
                            string directoryPage = "";

                            // What page?
                            if (cmdArgs.Length > 1)
                            {
                                pageValue = int.Parse(cmdArgs[1]);
                                totalPages = JESS_REGISTERED_GUILDS.Count / 20;

                                dirIndexer = 0 + (19 * pageValue);
                            }

                            // Lay page out
                            for (int i = dirIndexer; i < JESS_REGISTERED_GUILDS.Count && dirPageIncrement < 20; i++)
                            {
                                directoryPage += $"{JESS_REGISTERED_GUILD_NAMES.ElementAt(i)} **( {JESS_REGISTERED_GUILDS.ElementAt(i)} )** - {jess.GetGuild(JESS_REGISTERED_GUILDS.ElementAt(i)).MemberCount} Members and Bots\n";
                                dirPageIncrement++;
                            }

                            // Finalize
                            directoryPage += $"\n**Page {pageValue + 1} of {totalPages + 1}**";

                            // Create and display directory!
                            EmbedBuilder embedDirectory = new EmbedBuilder();

                            embedDirectory.WithTitle("Server Directory");

                            embedDirectory.AddField("Current Server", $"{(e.Channel as SocketGuildChannel).Guild.Name} **( {(e.Channel as SocketGuildChannel).Guild.Id} )**", false);

                            embedDirectory.AddField("List", directoryPage, false);

                            embedDirectory.WithColor(Color.Blue);
                            await e.Channel.SendMessageAsync("", false, embedDirectory.Build());

                            break;

                        #endregion

                        #region HELP COMMAND

                        case "help":
                            // Displays the help section
                            int commandsDisplayedPerPage = 8; // Used to handle math without having to set several values. DO NOT SET TO TEN.
                            int helpIndexer = 0;
                            int helpPageIncrement = 0;
                            pageValue = 0;
                            totalPages = 0;

                            bool userAttemptedIllegalAccess = false;

                            int helpPageType = 0;
                            int commandsRegistered = 0;

                            string helpPage = "";
                            string prefix = "";

                            List<string> commandsList = new List<string>();

                            // Get the prefix
                            if (JESS_REGISTERED_GUILDS.Count != 0)
                            {
                                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                                {
                                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == (e.Channel as SocketGuildChannel).Guild.Id)
                                    {
                                        // SERVER IS REGISTERED, GET PREFIX
                                        prefix = CUSTOM_GUILD_PREFIXES[JESS_REGISTERED_GUILDS.ElementAt(i)];
                                        break;
                                    }
                                }
                            }

                            // If prefix is still empty
                            if (prefix.Equals(""))
                            {
                                prefix = JESS_DEFAULT_PREFIX;
                            }

                            // Set up prefix help message
                            string prefixTooltip = $"This server's prefix is: `{prefix}`\nPlease use this prefix before any and all commands, else I won't know you're asking for my help!";

                            // Default
                            if (cmdArgs.Length == 1)
                            {
                                // Is user an admin?
                                if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                                {
                                    // Yes. Allow access to the administration section.
                                    helpPage += $"**Administrative Commands** - Access with `{prefix}help admin <page>`.\n";
                                }

                                // Regardless of administrative powers, allow access to the following:
                                helpPage += $"**General Commands** - Access with `{prefix}help general <page>`.\n";
                                helpPage += $"**Economy Commands** - Access with `{prefix}help economy <page>`.\n";
                            }

                            // Check for help function type
                            if (cmdArgs.Length > 1)
                            {
                                string caseValue = cmdArgs[1];
                                switch (caseValue)
                                {
                                    case "admin":
                                        if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                                        {
                                            // User is an admin and is capable of accessing administrative commands
                                            helpPageType = 1;
                                            commandsRegistered = 10;

                                            commandsList.Add("`admin_introduction <channel>` - use this to introduce me to your server!");
                                            commandsList.Add("`admin_registerserver` - use this to register your server with the directory!");
                                            commandsList.Add("`admin_setentrychannel <channel>` - use this to tell me where I should greet new users!");
                                            commandsList.Add("`admin_setmodchannel <channel>` - use this to tell me where I should do my own personal audit log tracking!");
                                            commandsList.Add("`admin_setentryrole <role id>` - use this to tell me which role new users should automatically be given!");
                                            commandsList.Add("`admin_setprefix <prefix>` - use this to set your server's prefix! This lets me know when I'm being talked to.");
                                            commandsList.Add("`admin_openserver` - use this to open your server to the public! Make sure you've already set the entry channel and role first, otherwise I can't help you. This will require your server to be registered.");
                                            commandsList.Add("`admin_togglejoinmessages` - use this to tell me whether or not I should greet new users and notify people when they've left.");
                                            commandsList.Add("`admin_togglebanmessages` - use this to tell me whether or not I should discuss when users are banned.");
                                            commandsList.Add("`admin_overview` - use this to get an overview of the server settings. This will only work in its entirety if the server has been registered.");
                                        }
                                        else
                                        {
                                            // User is not an admin. Display an error message and break functionality
                                            userAttemptedIllegalAccess = true;
                                        }

                                        break;
                                    case "general":
                                        helpPageType = 2;
                                        commandsRegistered = 5;

                                        commandsList.Add("`help <section> <page>` - you are currently using this!");
                                        commandsList.Add("`create_invite <server id>` - use this in combination with the directory to generate invites to opened Discord servers! If their administrative team has kept their server closed to the public, I will be unable to invite you, however.");
                                        commandsList.Add("`server_directory <page>` - use this to find all registered servers and their user counts! It's a big wide world out there...");
                                        commandsList.Add("`balance | bal` - use this to look at your balance.");
                                        commandsList.Add("`experience | exp` - use this to see your current experience and level, as well as what you'll have to reach for the next level.");

                                        break;
                                    case "economy":
                                        helpPageType = 3;
                                        commandsRegistered = 1;

                                        commandsList.Add("`store` - use this view the store. Must be experience level 2 or higher.");

                                        break;
                                }

                                if (userAttemptedIllegalAccess)
                                {
                                    await e.Channel.SendMessageAsync("Sorry, I'm afraid you don't have permission to view those commands.");
                                    break;
                                }

                                pageValue = 0;
                                totalPages = commandsRegistered / commandsDisplayedPerPage;

                                if (Math.IEEERemainder(commandsRegistered, commandsDisplayedPerPage) == 0 && totalPages != 0)
                                {
                                    totalPages -= 1;
                                }

                                helpIndexer = 0 + (commandsDisplayedPerPage * pageValue);
                            }

                            // What page?
                            if (cmdArgs.Length > 2)
                            {
                                pageValue = int.Parse(cmdArgs[2]) - 1;
                                totalPages = commandsRegistered / commandsDisplayedPerPage;

                                if (Math.IEEERemainder(commandsRegistered, commandsDisplayedPerPage) == 0 && totalPages != 0)
                                {
                                    totalPages -= 1;
                                }

                                if (pageValue > totalPages)
                                {
                                    await e.Channel.SendMessageAsync("Oops! That page does not exist!");
                                    break;
                                }

                                helpIndexer = 0 + (commandsDisplayedPerPage * pageValue);
                            }

                            // Lay page out
                            if (helpPageType != 0)
                            {
                                for (int i = helpIndexer; i < commandsRegistered && helpPageIncrement < commandsDisplayedPerPage; i++)
                                {
                                    helpPage += $"{commandsList.ElementAt(i)}\n";
                                    helpPageIncrement++;
                                }
                            }

                            // Finalize
                            if (helpPageType != 0) { helpPage += $"\n**Page {pageValue + 1} of {totalPages + 1}**"; }

                            // Create and display directory!
                            EmbedBuilder embedHelp = new EmbedBuilder();

                            switch (helpPageType)
                            {
                                // Sections/Default
                                case 0:
                                    embedHelp.WithTitle("Help Menu - Sections List");
                                    embedHelp.AddField("Available Sections", helpPage, false);
                                    embedHelp.WithColor(new Color(RNG.Next(100, 256), RNG.Next(110, 256), RNG.Next(110, 256)));
                                    break;
                                case 1:
                                    embedHelp.WithTitle("Help Menu - Administrative Commands");
                                    embedHelp.AddField("Note", prefixTooltip, false);
                                    embedHelp.AddField("Available Commands", helpPage, false);
                                    embedHelp.WithColor(Color.Gold);
                                    break;
                                case 2:
                                    embedHelp.WithTitle("Help Menu - General Commands");
                                    embedHelp.AddField("Note", prefixTooltip, false);
                                    embedHelp.AddField("Available Commands", helpPage, false);
                                    embedHelp.WithColor(new Color(0, 212, 255));
                                    break;
                                case 3:
                                    embedHelp.WithTitle("Help Menu - Economy Commands");
                                    embedHelp.AddField("Note", prefixTooltip, false);
                                    embedHelp.AddField("Available Commands", helpPage, false);
                                    embedHelp.WithColor(new Color(RNG.Next(20, 51), RNG.Next(120, 256), RNG.Next(80, 121)));
                                    break;
                            }

                            await e.Channel.SendMessageAsync("", false, embedHelp.Build());

                            commandsList.Clear();
                            break;

                        #endregion

                        #region USER COMMANDS

                        case "exp":
                        case "experience":
                            for (uint i = 0; i < JESS_REGISTERED_USERS.Count; ++i)
                            {
                                if (JESS_REGISTERED_USERS[(int)i] == e.Author.Id)
                                {
                                    // Basic debug handler
                                    // await e.Channel.SendMessageAsync(JESS_REGISTERED_USERS_DATA[(int)i].GetExperience().ToString());

                                    // Create and display UEV!
                                    EmbedBuilder SV_UEVBuilder = new EmbedBuilder();

                                    SV_UEVBuilder.WithTitle("User Experience Viewer");

                                    SV_UEVBuilder.AddField("Your EXP", JESS_REGISTERED_USERS_DATA[(int)i].GetExperience().ToString() + " EXP", true);
                                    SV_UEVBuilder.AddField("Current Level", JESS_REGISTERED_USERS_DATA[(int)i].GetExperienceLevel(), true);

                                    SV_UEVBuilder.AddField("===[ NEXT LEVEL ]===", "You are approaching:", false);

                                    SV_UEVBuilder.AddField("Required EXP", CONST_JESS_LEVEL_TOTAL_NUMS[(uint)JESS_REGISTERED_USERS_DATA[(int)i].GetExperienceLevel() + 1].ToString() + " EXP", true);
                                    SV_UEVBuilder.AddField("Next Level", JESS_REGISTERED_USERS_DATA[(int)i].GetExperienceLevel() + 1, true);

                                    SV_UEVBuilder.WithColor(new Color(0, 212, 255));
                                    await e.Channel.SendMessageAsync("", false, SV_UEVBuilder.Build());
                                }
                            }
                            break;

                        case "bal":
                        case "balance":
                            for (uint i = 0; i < JESS_REGISTERED_USERS.Count; ++i)
                            {
                                if (JESS_REGISTERED_USERS[(int)i] == e.Author.Id)
                                {
                                    // Basic debug handler
                                    // await e.Channel.SendMessageAsync(JESS_REGISTERED_USERS_DATA[(int)i].GetMonetaryValue().ToString());

                                    // Create and display UEV!
                                    EmbedBuilder SV_UBVBuilder = new EmbedBuilder();

                                    SV_UBVBuilder.WithTitle("User Balance Viewer");

                                    SV_UBVBuilder.AddField("Your Balance", $"{new Emoji("<:jessbucks:561818526923620353>").ToString()}" + JESS_REGISTERED_USERS_DATA[(int)i].GetMonetaryValue().ToString(), true);

                                    SV_UBVBuilder.WithColor(new Color(RNG.Next(20, 51), RNG.Next(120, 256), RNG.Next(80, 121)));
                                    await e.Channel.SendMessageAsync("", false, SV_UBVBuilder.Build());
                                }
                            }
                            break;

                        #endregion

                        #region ECONOMY COMMANDS

                        case "store":
                            // Displays the store
                            int itemsDisplayedPerPage = 8; // Used to handle math without having to set several values. DO NOT SET TO TEN.
                            int storeIndexer = 0;
                            int storePageIncrement = 0;
                            pageValue = 0;
                            totalPages = 0;

                            int storePageType = 0;
                            int itemsRegistered = 0;

                            string storePage = "";
                            prefix = "";
                            string itemPrefix = "";

                            ulong viewerLevel = 0;
                            UserReferendum viewer = null;

                            List<string> itemsList = new List<string>();

                            // Get necessary information about the user. This will be used to decide if the user can see anything, or purchase certain things.
                            for (uint i = 0; i < JESS_REGISTERED_USERS.Count; ++i)
                            {
                                if (JESS_REGISTERED_USERS[(int)i] == e.Author.Id)
                                {
                                    viewerLevel = JESS_REGISTERED_USERS_DATA[(int)i].GetExperienceLevel();
                                    viewer = JESS_REGISTERED_USERS_DATA[(int)i];
                                }
                            }

                            // Get the prefix
                            if (JESS_REGISTERED_GUILDS.Count != 0)
                            {
                                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                                {
                                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == (e.Channel as SocketGuildChannel).Guild.Id)
                                    {
                                        // SERVER IS REGISTERED, GET PREFIX
                                        prefix = CUSTOM_GUILD_PREFIXES[JESS_REGISTERED_GUILDS.ElementAt(i)];
                                        break;
                                    }
                                }
                            }

                            // If prefix is still empty
                            if (prefix.Equals(""))
                            {
                                prefix = JESS_DEFAULT_PREFIX;
                            }

                            // Set up prefix help message
                            prefixTooltip = $"This server's prefix is: `{prefix}`\nPlease use this prefix before any and all commands, else I won't know you're asking for my help!";

                            // Default
                            if (cmdArgs.Length == 1)
                            {
                                storePage += $"**EXP Boosters** - single-purchase EXP boosters. Access with `{prefix}store exp <page>`.\n";
                                storePage += $"**Cash Boosters** - single-purchase cash boosters. Access with `{prefix}store cash <page>`.\n";
                            }

                            // Check for help function type
                            if (cmdArgs.Length > 1)
                            {
                                string caseValue = cmdArgs[1];
                                switch (caseValue)
                                {
                                    case "experience":
                                    case "exp":
                                        storePageType = 1;
                                        itemsRegistered = 1;
                                        itemPrefix = "XB";

                                        itemsList.Add($"`{itemPrefix}00` [Cost: {jb} 500] - guarantees a minimum of 5 EXP per message. *Owned? {viewer.userExperienceProperties.properties[0].ToString().ToUpper()}*");

                                        break;
                                    case "cash":
                                        storePageType = 2;
                                        itemsRegistered = 1;
                                        itemPrefix = "CB";

                                        itemsList.Add($"`{itemPrefix}00` [Cost: {jb} 250] - guarantees a minimum of {jb}1 per message and raises maximum cash gain. *Owned? {viewer.userEconomyProperties.properties[0].ToString().ToUpper()}*");

                                        break;
                                }

                                pageValue = 0;
                                totalPages = itemsRegistered / itemsDisplayedPerPage;

                                if (Math.IEEERemainder(itemsRegistered, itemsDisplayedPerPage) == 0 && totalPages != 0)
                                {
                                    totalPages -= 1;
                                }

                                helpIndexer = 0 + (itemsDisplayedPerPage * pageValue);
                            }

                            // Set up item prefix help message
                            itemPrefix = itemPrefix.ToUpper();
                            string itemPrefixTooltip = $"This section's item prefix is: `{itemPrefix}`\nPlease use this prefix before any and all purchases of this sort! For these items, use: `{prefix}buy {itemPrefix}##`";

                            // What page?
                            if (cmdArgs.Length > 2)
                            {
                                pageValue = int.Parse(cmdArgs[2]) - 1;
                                totalPages = itemsRegistered / itemsDisplayedPerPage;

                                if (Math.IEEERemainder(itemsRegistered, itemsDisplayedPerPage) == 0 && totalPages != 0)
                                {
                                    totalPages -= 1;
                                }

                                if (pageValue > totalPages)
                                {
                                    await e.Channel.SendMessageAsync("Oops! That page does not exist!");
                                    break;
                                }

                                helpIndexer = 0 + (itemsDisplayedPerPage * pageValue);
                            }

                            // Lay page out
                            if (storePageType != 0)
                            {
                                for (int i = storeIndexer; i < itemsRegistered && storePageIncrement < itemsDisplayedPerPage; i++)
                                {
                                    storePage += $"{itemsList.ElementAt(i)}\n";

                                    storePage = storePage.Replace("TRUE", "**Yes.**");
                                    storePage = storePage.Replace("FALSE", "**No.**");

                                    storePageIncrement++;
                                }
                            }

                            // Finalize
                            if (storePageType != 0) { storePage += $"\n**Page {pageValue + 1} of {totalPages + 1}**"; }

                            // Create and display directory!
                            EmbedBuilder ECON_storeBuilder = new EmbedBuilder();

                            if (viewerLevel >= 2 || viewer.userExperienceProperties.unlocks[2])
                            {
                                switch (storePageType)
                                {
                                    // Sections
                                    case 0:
                                        ECON_storeBuilder.WithTitle("Store Menu");
                                        ECON_storeBuilder.AddField("Available Sections", storePage, false);
                                        ECON_storeBuilder.WithColor(new Color(60, 250, RNG.Next(100, 181)));
                                        break;
                                    case 1:
                                        ECON_storeBuilder.WithTitle("Store Menu - EXP Boosters");
                                        ECON_storeBuilder.AddField("Note", prefixTooltip, false);
                                        ECON_storeBuilder.AddField("Note", itemPrefixTooltip, false);
                                        ECON_storeBuilder.AddField("Available Items", storePage, false);
                                        ECON_storeBuilder.WithColor(new Color(0, 200, RNG.Next(80, 151)));
                                        break;
                                    case 2:
                                        ECON_storeBuilder.WithTitle("Store Menu - Cash Boosters");
                                        ECON_storeBuilder.AddField("Note", prefixTooltip, false);
                                        ECON_storeBuilder.AddField("Note", itemPrefixTooltip, false);
                                        ECON_storeBuilder.AddField("Available Items", storePage, false);
                                        ECON_storeBuilder.WithColor(new Color(0, 200, RNG.Next(80, 151)));
                                        break;
                                }

                                await e.Channel.SendMessageAsync("", false, ECON_storeBuilder.Build());
                            }
                            else
                            {
                                ECON_storeBuilder.WithTitle("No Access");
                                ECON_storeBuilder.AddField("EXP Level Too Low", "Unfortunately, your experience level is too low, and you are not permitted to access the store yet.", false);
                                ECON_storeBuilder.WithColor(new Color(255, 0, 0));

                                await e.Channel.SendMessageAsync("", false, ECON_storeBuilder.Build());
                            }

                            itemsList.Clear();
                            break;

                        case "buy":
                        case "purchase":
                            // This runs when the user wishes to buy something.
                            ulong purchaserLevel = 0;
                            UserReferendum purchaser = null;

                            string returnedPurchaseMessage = "";

                            BigInteger cost = 0;

                            bool successfulPurchase = false;

                            // Get necessary information about the user. This will be used to decide if the user can see anything, or purchase certain things.
                            for (uint i = 0; i < JESS_REGISTERED_USERS.Count; ++i)
                            {
                                if (JESS_REGISTERED_USERS[(int)i] == e.Author.Id)
                                {
                                    purchaserLevel = JESS_REGISTERED_USERS_DATA[(int)i].GetExperienceLevel();
                                    purchaser = JESS_REGISTERED_USERS_DATA[(int)i];
                                }
                            }

                            // Can user actually purchase anything?
                            if (purchaserLevel >= 2 || purchaser.userExperienceProperties.unlocks[2])
                            {
                                List<string> internalArgs = new List<string>();

                                // Yes.
                                if (cmdArgs.Length >= 2)
                                {
                                    // Did user put in a second argument?
                                    if (cmdArgs.Length == 2)
                                    {
                                        // User didn't put in a second argument. Assume they meant to purchase one of the item.
                                        internalArgs.Add(cmdArgs[0]);
                                        internalArgs.Add(cmdArgs[1]);
                                        internalArgs.Add("1");
                                    }
                                    else
                                    {
                                        // User put in a second argument. Read the arguments as such.
                                        internalArgs.Add(cmdArgs[0]);
                                        internalArgs.Add(cmdArgs[1]);
                                        internalArgs.Add(cmdArgs[2]);
                                    }

                                    string caseValueFull = internalArgs[1].ToUpper();
                                    string caseValuePre = "";

                                    string countFull = internalArgs[2];
                                    string countPre = "";
                                    BigInteger count = 0;

                                    for (int i = 0; i < caseValueFull.Length && Char.IsLetter(caseValueFull[i]); ++i)
                                    {
                                        caseValuePre += caseValueFull[i];
                                    }

                                    for (int i = 0; i < countFull.Length && Char.IsLetter(countFull[i]); ++i)
                                    {
                                        countPre += countFull[i];
                                    }

                                    ulong caseValueID = ulong.Parse(caseValueFull.Remove(0, caseValuePre.Length));
                                    count = BigInteger.Parse(countFull.Remove(0, countPre.Length));

                                    switch (caseValuePre)
                                    {
                                        case "XB":
                                            switch (caseValueID)
                                            {
                                                case 0:
                                                    cost = 500;
                                                    
                                                    if (!purchaser.userExperienceProperties.properties[0])
                                                    {
                                                        if (cost <= purchaser.GetMonetaryValue())
                                                        {
                                                            purchaser.SetMonetaryValue(purchaser.GetMonetaryValue() - cost);
                                                            purchaser.userExperienceProperties.properties[0] = true;

                                                            returnedPurchaseMessage = $"You have bought `XB00` for {jb}{cost}. Your balance is now: {jb}{purchaser.GetMonetaryValue()}! Hope you enjoy this purchase!";

                                                            successfulPurchase = true;
                                                        }
                                                        else
                                                        {
                                                            returnedPurchaseMessage = $"Oops, I'm sorry, you don't have enough to purchase `XB00`! You need {jb}{cost - purchaser.GetMonetaryValue()} more.";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        returnedPurchaseMessage = $"Oops, I'm sorry, you already own `XB00`!";
                                                    }
                                                    break;
                                            }
                                            break;

                                        case "CB":
                                            switch (caseValueID)
                                            {
                                                case 0:
                                                    cost = 250;

                                                    if (!purchaser.userEconomyProperties.properties[0])
                                                    {
                                                        if (cost <= purchaser.GetMonetaryValue())
                                                        {
                                                            purchaser.SetMonetaryValue(purchaser.GetMonetaryValue() - cost);
                                                            purchaser.userEconomyProperties.properties[0] = true;

                                                            returnedPurchaseMessage = $"You have bought `CB00` for {jb}{cost}. Your balance is now: {jb}{purchaser.GetMonetaryValue()}! Hope you enjoy this purchase!";

                                                            successfulPurchase = true;
                                                        }
                                                        else
                                                        {
                                                            returnedPurchaseMessage = $"Oops, I'm sorry, you don't have enough to purchase `CB00`! You need {jb}{cost - purchaser.GetMonetaryValue()} more.";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        returnedPurchaseMessage = $"Oops, I'm sorry, you already own `CB00`!";
                                                    }
                                                    break;
                                            }
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                // No.
                                returnedPurchaseMessage = $"I'm sorry, but your experience level isn't high enough yet to access the store or purchase anything. However, you do have {jb}{purchaser.GetMonetaryValue()} in your balance, and will certainly have more than that by that time!";
                            }

                            EmbedBuilder ECON_PAM_POPUP = new EmbedBuilder();

                            if (successfulPurchase)
                            {
                                ECON_PAM_POPUP.WithTitle("Purchase");
                                ECON_PAM_POPUP.AddField("Information", returnedPurchaseMessage, false);

                                ECON_PAM_POPUP.WithColor(new Color(0, 255, 0));
                            }
                            else
                            {
                                if (purchaserLevel >= 2 || purchaser.userExperienceProperties.unlocks[2])
                                {
                                    ECON_PAM_POPUP.WithTitle("Purchase Failed");
                                    ECON_PAM_POPUP.AddField("Insufficient Funds / Already Owned", returnedPurchaseMessage, false);
                                }
                                else
                                {
                                    ECON_PAM_POPUP.WithTitle("No Access");
                                    ECON_PAM_POPUP.AddField("EXP Level Too Low", returnedPurchaseMessage, false);
                                }
                                
                                ECON_PAM_POPUP.WithColor(new Color(255, 0, 0));
                            }

                            await e.Channel.SendMessageAsync("", false, ECON_PAM_POPUP.Build());

                            break;

                        case "statistics":
                        case "statistic":
                        case "stat":
                        case "stats":
                            // Displays a general overview for this user
                            EmbedBuilder USER_STATS_POPUP = new EmbedBuilder();

                            USER_STATS_POPUP.WithTitle($"User Statistics: {e.Author.Username}");

                            BigInteger currentTop = 0;
                            ulong currentTopTrackedID = 0;
                            List<ulong> TOP_SORTED_REGISTERED_USERS = new List<ulong>();

                            while (TOP_SORTED_REGISTERED_USERS.Count < JESS_REGISTERED_USERS.Count)
                            {
                                currentTop = 0;
                                currentTopTrackedID = 0;

                                uint i = 0;

                                foreach (ulong uID in JESS_REGISTERED_USERS)
                                {
                                    if (!TOP_SORTED_REGISTERED_USERS.Contains(uID) && TOP_SORTED_REGISTERED_USERS.Count < JESS_REGISTERED_USERS.Count)
                                    {
                                        BigInteger comparatorValue = JESS_REGISTERED_USERS_DATA[(int)i].GetExperience() + (JESS_REGISTERED_USERS_DATA[(int)i].GetMonetaryValue() * 10);

                                        if (comparatorValue > currentTop)
                                        {
                                            currentTop = comparatorValue;
                                            currentTopTrackedID = JESS_REGISTERED_USERS[(int)i];
                                        }
                                        else if (comparatorValue == currentTop)
                                        {
                                            if (currentTopTrackedID < JESS_REGISTERED_USERS[(int)i])
                                            { currentTopTrackedID = JESS_REGISTERED_USERS[(int)i]; }
                                        }
                                    }

                                    i++;
                                }

                                TOP_SORTED_REGISTERED_USERS.Add(currentTopTrackedID);
                            }

                            for (uint i = 0; i < JESS_REGISTERED_USERS.Count; ++i)
                            {
                                if (JESS_REGISTERED_USERS[(int)i] == e.Author.Id)
                                {
                                    USER_STATS_POPUP.AddField("Your EXP", JESS_REGISTERED_USERS_DATA[(int)i].GetExperience().ToString() + " EXP", true);
                                    USER_STATS_POPUP.AddField("Current Level", JESS_REGISTERED_USERS_DATA[(int)i].GetExperienceLevel(), true);

                                    for (uint j = 0; j < TOP_SORTED_REGISTERED_USERS.Count; j++)
                                    {
                                        if (TOP_SORTED_REGISTERED_USERS[(int)j] == e.Author.Id)
                                        {
                                            USER_STATS_POPUP.AddField("Your Rank", $"#{j + 1}", true);
                                        }
                                    }

                                    USER_STATS_POPUP.AddField("Your Balance", $"{new Emoji("<:jessbucks:561818526923620353>").ToString()}" + JESS_REGISTERED_USERS_DATA[(int)i].GetMonetaryValue().ToString(), false);
                                }
                            }

                            List<SocketRole> roles = (e.Author as SocketGuildUser).Roles.ToList();
                            List<IRole> roles_mid = new List<IRole>();

                            for (int i = 0; i < roles.Count; i++)
                            {
                                roles_mid.Add(roles.ElementAt(i) as IRole);
                            }

                            int currentTopRole = 0;

                            List<IRole> roles_sorted = new List<IRole>();

                            while (roles_sorted.Count < roles_mid.Count)
                            {
                                currentTopRole = 0;
                                uint savedPosition = 0;

                                for (uint i = 0; i < roles_mid.Count; i++)
                                {
                                    if (roles_mid[(int)i].Position > currentTopRole && !roles_sorted.Contains(roles_mid[(int)i]))
                                    {
                                        currentTopRole = roles_mid[(int)i].Position;
                                        savedPosition = i;
                                    }
                                }
                                
                                roles_sorted.Add(roles_mid[(int)savedPosition]);
                            }
                            
                            for (int i = 0; i < roles_sorted.Count; i++)
                            {
                                if (roles_sorted[i].Color.RawValue > 0)
                                {
                                    USER_STATS_POPUP.WithColor(roles_sorted[i].Color);
                                    break;
                                }
                            }

                            await e.Channel.SendMessageAsync("", false, USER_STATS_POPUP.Build());

                            break;

                        case "inventory":
                        case "inv":
                            // Displays a general overview for this user's inventory
                            EmbedBuilder USER_INV_POPUP = new EmbedBuilder();

                            USER_INV_POPUP.WithTitle($"Inventory: {e.Author.Username}");

                            UserReferendum user = null;

                            for (uint i = 0; i < JESS_REGISTERED_USERS.Count; ++i)
                            {
                                if (JESS_REGISTERED_USERS[(int)i] == e.Author.Id)
                                {
                                    user = JESS_REGISTERED_USERS_DATA[(int)i];
                                }
                            }

                            // Calculate user's total worth.
                            BigInteger totalWorth = user.GetMonetaryValue();

                            // Additional values for storing sectional total worth
                            BigInteger totalWorth_TL = 0;
                            BigInteger totalWorth_FDS = 0;
                            BigInteger totalWorth_RM = 0;

                            #region TOTAL WORTH CALCULATION

                            // Tools
                            totalWorth += user.userInventory.TL_tools[00] * 15;
                            
                            totalWorth_TL += user.userInventory.TL_tools[00] * 15;

                            // Foodstuffs
                            totalWorth += user.userInventory.FDS_foodstuffs[00] * 8;
                            totalWorth += user.userInventory.FDS_foodstuffs[01] * 11;
                            totalWorth += user.userInventory.FDS_foodstuffs[02] * 16;
                            totalWorth += user.userInventory.FDS_foodstuffs[03] * 20;
                            totalWorth += user.userInventory.FDS_foodstuffs[04] * 25;
                            
                            totalWorth_FDS += user.userInventory.FDS_foodstuffs[00] * 8;
                            totalWorth_FDS += user.userInventory.FDS_foodstuffs[01] * 11;
                            totalWorth_FDS += user.userInventory.FDS_foodstuffs[02] * 16;
                            totalWorth_FDS += user.userInventory.FDS_foodstuffs[03] * 20;
                            totalWorth_FDS += user.userInventory.FDS_foodstuffs[04] * 25;

                            // Raw Materials
                            totalWorth += user.userInventory.RM_rawmats[00] * 75;
                            totalWorth += user.userInventory.RM_rawmats[01] * 150;

                            totalWorth_RM += user.userInventory.RM_rawmats[00] * 75;
                            totalWorth_RM += user.userInventory.RM_rawmats[01] * 150;

                            #endregion

                            USER_INV_POPUP.AddField($"Balance", jb + user.GetMonetaryValue(), true);
                            USER_INV_POPUP.AddField($"Total Valuation", jb + totalWorth, true);

                            BigInteger cyclicalItemCount = 0;
                            string sectionalDetailPrompt_template = "\n";
                            string sectionalDetailPrompt = sectionalDetailPrompt_template;

                            // Default
                            if (cmdArgs.Length == 1)
                            {
                                for (int i = 0; i < user.userInventory.TL_tools.Count; i++)
                                { cyclicalItemCount += user.userInventory.TL_tools[(ulong)i]; }

                                sectionalDetailPrompt += $"`Use {customPrefix}inv TL <page> or {customPrefix}inv tools <page> to get more detailed information.`";
                                USER_INV_POPUP.AddField($"TOOLS (TL)", cyclicalItemCount + " items" + sectionalDetailPrompt, false);

                                cyclicalItemCount = 0;
                                sectionalDetailPrompt = sectionalDetailPrompt_template;

                                for (int i = 0; i < user.userInventory.FDS_foodstuffs.Count; i++)
                                { cyclicalItemCount += user.userInventory.FDS_foodstuffs[(ulong)i]; }

                                sectionalDetailPrompt += $"`Use {customPrefix}inv FDS <page> or {customPrefix}inv foodstuffs <page> to get more detailed information.`";
                                USER_INV_POPUP.AddField($"FOODSTUFFS (FDS)", cyclicalItemCount + " items" + sectionalDetailPrompt, false);

                                cyclicalItemCount = 0;
                                sectionalDetailPrompt = sectionalDetailPrompt_template;

                                for (int i = 0; i < user.userInventory.RM_rawmats.Count; i++)
                                { cyclicalItemCount += user.userInventory.RM_rawmats[(ulong)i]; }

                                sectionalDetailPrompt += $"`Use {customPrefix}inv RM <page> or {customPrefix}inv rawmats <page> to get more detailed information.`";
                                USER_INV_POPUP.AddField($"RAW MATERIALS (RM)", cyclicalItemCount + " items" + sectionalDetailPrompt, false);

                                cyclicalItemCount = 0;
                                sectionalDetailPrompt = sectionalDetailPrompt_template;
                            }

                            // Argument for section
                            if (cmdArgs.Length > 1)
                            {
                                switch (cmdArgs[1].ToLower())
                                {
                                    case "tools":
                                    case "tl":
                                    case "tool":
                                        for (int i = 0; i < user.userInventory.TL_tools.Count; i++)
                                        { cyclicalItemCount += user.userInventory.TL_tools[(ulong)i]; }
                                        
                                        USER_INV_POPUP.AddField($"TOOLS", cyclicalItemCount + " total items in category, valuation: " + jb + totalWorth_TL);
                                        USER_INV_POPUP.AddField($"Fishing Rod (TL00)", user.userInventory.TL_tools[00] + " items");

                                        cyclicalItemCount = 0;

                                        break;

                                    case "foodstuffs":
                                    case "fds":
                                    case "foods":
                                    case "food":
                                        for (int i = 0; i < user.userInventory.FDS_foodstuffs.Count; i++)
                                        { cyclicalItemCount += user.userInventory.FDS_foodstuffs[(ulong)i]; }

                                        USER_INV_POPUP.AddField($"FOODSTUFFS", cyclicalItemCount + " total items in category, valuation: " + jb + totalWorth_FDS);
                                        USER_INV_POPUP.AddField($"Fish, Tiny (FDS00)", user.userInventory.FDS_foodstuffs[00] + " items");
                                        USER_INV_POPUP.AddField($"Fish, Small (FDS01)", user.userInventory.FDS_foodstuffs[01] + " items");
                                        USER_INV_POPUP.AddField($"Fish, Medium (FDS02)", user.userInventory.FDS_foodstuffs[02] + " items");
                                        USER_INV_POPUP.AddField($"Fish, Large (FDS03)", user.userInventory.FDS_foodstuffs[03] + " items");
                                        USER_INV_POPUP.AddField($"Fish, Huge (FDS04)", user.userInventory.FDS_foodstuffs[04] + " items");

                                        cyclicalItemCount = 0;

                                        break;

                                    case "rawmats":
                                    case "rm":
                                    case "rawmaterials":
                                    case "rawmat":
                                        for (int i = 0; i < user.userInventory.RM_rawmats.Count; i++)
                                        { cyclicalItemCount += user.userInventory.RM_rawmats[(ulong)i]; }

                                        USER_INV_POPUP.AddField($"RAW MATERIALS", cyclicalItemCount + " total items in category, valuation: " + jb + totalWorth_RM);
                                        USER_INV_POPUP.AddField($"Gold Nugget, Tiny (RM00)", user.userInventory.RM_rawmats[00] + " items");
                                        USER_INV_POPUP.AddField($"Gold Nugget, Small (RM01)", user.userInventory.RM_rawmats[01] + " items");

                                        cyclicalItemCount = 0;

                                        break;
                                }
                            }

                            USER_INV_POPUP.WithColor(new Color(RNG.Next(55, 256), RNG.Next(105, 256), RNG.Next(55, 256)));

                            await e.Channel.SendMessageAsync($"", false, USER_INV_POPUP.Build());

                            break;

                        #endregion

                        default:
                            // Nothing was input for a command. Treat as an error.
                            if (!command.Equals(""))
                                await e.Channel.SendMessageAsync("Uh, heh, I uh... don't quite know what it is you want me to do.");
                            break;
                    }
                }

                #endregion

                // RUN A FINAL SAVE ON ALL USER DATA AS IT HAS MOST DEFINITELY BEEN MODIFIED
                SV_SaveMasterUserList(); // Save the master list.
                SV_SaveUserDataFiles(); // Save all user data files.
            }
        }

        #endregion

        #region FUNCTIONS

        #region SERVER OPERATIONS

        // HANDLE ALL SERVER OPERATIONS, SUCH AS USERJOIN, USERDROP, ETC.

        // UPON JOINING GUILD
        public async Task ProcessGuildJoin(SocketGuild g)
        {
            await g.Owner.SendMessageAsync($"Hi! I'd like to introduce myself in **{g.Name}**, but I don't know where I should begin! Please, use the command `JR.admin_introduction < channel >` so I can introduce myself!");
        }

        // UPON USERJOIN
        public async Task ProcessUserJoin(SocketGuildUser u)
        {
            // ASSUME NOT REGISTERED AND MOD CHANNEL NOT SET
            bool exists = false;
            bool MOD_set = false;

            // CHECK TO SEE IF IT MAY BE
            if (JESS_REGISTERED_GUILDS.Count != 0)
            {
                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                {
                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == u.Guild.Id)
                    {
                        if (CUSTOM_ENTRY_CHANNELS[u.Guild.Id] != 0 && CUSTOM_ENTRY_ROLES[u.Guild.Id] != 0 && GUILD_TOGGLE_JOINMSG[u.Guild.Id])
                        {
                            // DO THE NECESSARY CRITERIA EXIST? IF SO, PROCEED:
                            exists = true;

                            if (CUSTOM_MOD_CHANNELS[u.Guild.Id] != 0)
                            {
                                // MOD CHANNEL IS SET!
                                MOD_set = true;
                            }

                            break;
                        }
                    }
                }
            }

            // IS IT REGISTERED?
            if (exists)
            {
                // EXISTS, CONTINUE
                await (jess.GetChannel(CUSTOM_ENTRY_CHANNELS[u.Guild.Id]) as ISocketMessageChannel).SendMessageAsync($"Hello, {u.Username}, and welcome to **{u.Guild.Name}**! Please enjoy your stay, and make sure to read through the rules! If you are currently unable to see primary chat channels, please make sure to go through whatever security measures the moderators have instituted. Thank you!");
                await u.AddRoleAsync(jess.GetGuild(u.Guild.Id).GetRole(CUSTOM_ENTRY_ROLES[u.Guild.Id]));

                // SEND VALUE TO MOD CHANNEL IF IT IS SET!
                if (MOD_set)
                {
                    await (jess.GetChannel(CUSTOM_MOD_CHANNELS[u.Guild.Id]) as ISocketMessageChannel).SendMessageAsync($"{u.Username}#{u.Discriminator} joined the server at {DateTime.Now.ToString("h:mm:ss.fff tt")} PST (UTC + 8).");
                }
            }
            else
            {
                // DOES NOT EXIST, PROCEED ANYWAYS THROUGH DM
                await u.SendMessageAsync($"Hello, {u.Username}! Unfortunately, server moderation in **{u.Guild.Name}** haven't quite set things up as needed, so I have to send this to you in DMs. Regardless, please enjoy your stay, and make sure to read through the server rules! If you are currently unable to see the server's primary chat channels, please make sure to go through whatever security measures the moderators have instituted. Thank you!");
                await u.AddRoleAsync(jess.GetGuild(u.Guild.Id).GetRole(CUSTOM_ENTRY_ROLES[u.Guild.Id]));
            }
        }

        // UPON USERDROP
        public async Task ProcessUserDrop(SocketGuildUser u)
        {
            // ASSUME NOT REGISTERED AND MOD CHANNEL NOT SET
            bool exists = false;
            bool MOD_set = false;

            // CHECK TO SEE IF IT MAY BE
            if (JESS_REGISTERED_GUILDS.Count != 0)
            {
                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                {
                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == u.Guild.Id)
                    {
                        if (CUSTOM_ENTRY_CHANNELS[u.Guild.Id] != 0 && GUILD_TOGGLE_JOINMSG[u.Guild.Id])
                        {
                            // DO THE NECESSARY CRITERIA EXIST? IF SO, PROCEED:
                            exists = true;

                            if (CUSTOM_MOD_CHANNELS[u.Guild.Id] != 0)
                            {
                                // MOD CHANNEL IS SET!
                                MOD_set = true;
                            }

                            break;
                        }
                    }
                }
            }

            // IS IT REGISTERED?
            if (exists)
            {
                // EXISTS, CONTINUE
                await (jess.GetChannel(CUSTOM_ENTRY_CHANNELS[u.Guild.Id]) as ISocketMessageChannel).SendMessageAsync($"Unfortunately, {u.Username} has left **{u.Guild.Name}**. You may have known them by '{u.Nickname}'.");

                // SEND VALUE TO MOD CHANNEL IF IT IS SET!
                if (MOD_set)
                {
                    await (jess.GetChannel(CUSTOM_MOD_CHANNELS[u.Guild.Id]) as ISocketMessageChannel).SendMessageAsync($"{u.Username}#{u.Discriminator} left the server at {DateTime.Now.ToString("h:mm:ss.fff tt")} PST (UTC + 8). Their nickname was '{u.Nickname}'.");
                }
            }
            else
            {
                // DOES NOT EXIST, PROCEED ANYWAYS THROUGH DM
                await u.Guild.Owner.SendMessageAsync($"{u.Username}#{u.Discriminator} left {u.Guild.Name}.");
            }
        }

        // UPON BAN
        public async Task ProcessUserBan(SocketUser u, SocketGuild g)
        {
            SocketGuildUser gu = g.GetUser(u.Id);

            // ASSUME NOT REGISTERED AND MOD CHANNEL NOT SET
            bool exists = false;
            bool MOD_set = false;

            // CHECK TO SEE IF IT MAY BE
            if (JESS_REGISTERED_GUILDS.Count != 0)
            {
                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                {
                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == g.Id)
                    {
                        if (CUSTOM_ENTRY_CHANNELS[g.Id] != 0 && GUILD_TOGGLE_BANSMSG[g.Id])
                        {
                            // DO THE NECESSARY CRITERIA EXIST? IF SO, PROCEED:
                            exists = true;

                            if (CUSTOM_MOD_CHANNELS[g.Id] != 0)
                            {
                                // MOD CHANNEL IS SET!
                                MOD_set = true;
                            }

                            break;
                        }
                    }
                }
            }

            // IS IT REGISTERED?
            if (exists)
            {
                // EXISTS, CONTINUE
                await (jess.GetChannel(CUSTOM_ENTRY_CHANNELS[g.Id]) as ISocketMessageChannel).SendMessageAsync($"Unfortunately, {gu.Username} has been banned from **{g.Name}**. You may have known them by '{gu.Nickname}'. Please ask server moderation if curious for details, but note they are not obligated to tell you anything.");

                // TELL USER WHY THEY WERE BANNED, IF A USER AT ALL
                if (!gu.IsBot)
                {
                    await gu.SendMessageAsync($"You have been banned from {g.Name}. Reason provided: {(await g.GetBanAsync(gu)).Reason}");
                }

                // SEND VALUE TO MOD CHANNEL IF IT IS SET!
                if (MOD_set)
                {
                    await (jess.GetChannel(CUSTOM_MOD_CHANNELS[g.Id]) as ISocketMessageChannel).SendMessageAsync($"{gu.Username}#{gu.Discriminator} was banned at {DateTime.Now.ToString("h:mm:ss.fff tt")} PST (UTC + 8) for reason: {(await g.GetBanAsync(gu)).Reason} Their nickname was '{gu.Nickname}'.");
                }

                // ENSURE USERLEAVE MESSAGE DOESN'T DISPLAY
                ifBan = true;
            }
            else
            {
                // DOES NOT EXIST, PROCEED ANYWAYS THROUGH DM
                await g.Owner.SendMessageAsync($"{u.Username}#{u.Discriminator} was banned from {g.Name} for reason: {(await g.GetBanAsync(u as SocketGuildUser)).Reason}.");

                // ENSURE USERLEAVE MESSAGE DOESN'T DISPLAY
                ifBan = true;
            }
        }

        #endregion

        #region SAVE SERVER DATA

        // CHECK INPUT, AND IF EVERYTHING MATCHES UP, SAVE SERVER DATA

        // USE TO CHECK IF THE SERVER IS ALREADY OPEN - SINGLE INPUT, SERVER ID
        bool[] SV_CheckIfServerOpened(ulong serverID)
        {
            // ASSUME THE SERVER IS NOT REGISTERED AND NOT OPEN
            bool exists = false;
            bool open = false;

            // CHECK TO SEE IF IT MIGHT BE REGISTERED
            if (JESS_REGISTERED_GUILDS.Count != 0)
            {
                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                {
                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == serverID)
                    {
                        // SERVER IS REGISTERED, CHECK IF SERVER IS OPEN
                        exists = true;
                        open = GUILD_ALLOWS_INVITES[JESS_REGISTERED_GUILDS.ElementAt(i)];
                        break;
                    }
                }
            }

            bool[] existsAndOpen = { exists, open };

            // RETURN RESULT
            return existsAndOpen;
        }

        // ONE INPUT, IS SERVER
        bool SV_CheckThenSave(ulong serverID)
        {
            // ASSUME THE SERVER IS NOT REGISTERED
            bool exists = false;

            // CHECK TO SEE IF IT MIGHT BE
            if (JESS_REGISTERED_GUILDS.Count != 0)
            {
                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                {
                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == serverID)
                    {
                        // SERVER IS REGISTERED, NO NEED TO DO ANYTHING
                        exists = true;
                        break;
                    }
                }
            }

            // IF SERVER IS NOT REGISTERED, REGISTER
            if (!exists)
            {
                JESS_REGISTERED_GUILDS.Add(serverID);
                JESS_REGISTERED_GUILD_NAMES.Add(jess.GetGuild(serverID).Name);

                // SINCE THIS DATA WAS NOT INPUT AND SERVER DOESN'T EXIST, SET THESE VALUES AHEAD OF TIME
                CUSTOM_ENTRY_CHANNELS.Add(serverID, 0);
                CUSTOM_ENTRY_ROLES.Add(serverID, 0);
                CUSTOM_MOD_CHANNELS.Add(serverID, 0);
                CUSTOM_GUILD_PREFIXES.Add(serverID, JESS_DEFAULT_PREFIX);
                GUILD_ALLOWS_INVITES.Add(serverID, false);
                GUILD_TOGGLE_JOINMSG.Add(serverID, true);
                GUILD_TOGGLE_BANSMSG.Add(serverID, true);
            }
            // IF SERVER IS REGISTERED, UPDATE INSTEAD
            else
            {
                if (JESS_REGISTERED_GUILDS.Count != 0)
                {
                    for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                    {
                        if (JESS_REGISTERED_GUILDS.ElementAt(i) == serverID)
                        {
                            // FOUND SERVER, UPDATING
                            JESS_REGISTERED_GUILDS[i] = serverID;
                            JESS_REGISTERED_GUILD_NAMES[i] = jess.GetGuild(serverID).Name;
                        }
                    }
                }
            }

            // RUN THE SAVE FUNCTION THEN EXIT
            SV_Save();
            return exists;
        }

        // THREE INPUTS, SERVER, OPEN STATE, AND SOCKETMESSAGE IN CASE ERROR NEEDS DISPLAYING
        bool[] SV_CheckThenSave(ulong serverID, bool opened, SocketMessage e)
        {
            // ASSUME THE SERVER IS NOT REGISTERED AND NOT OPEN
            bool exists = false;
            bool open = false;

            // CHECK TO SEE IF IT MIGHT BE
            if (JESS_REGISTERED_GUILDS.Count != 0)
            {
                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                {
                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == serverID)
                    {
                        // SERVER IS REGISTERED, NO NEED TO DO ANYTHING
                        exists = true;
                        break;
                    }
                }
            }

            // IF SERVER IS NOT REGISTERED, REGISTER
            if (!exists)
            {
                JESS_REGISTERED_GUILDS.Add(serverID);
                JESS_REGISTERED_GUILD_NAMES.Add(jess.GetGuild(serverID).Name);

                // SINCE THIS DATA WAS NOT INPUT AND SERVER DOESN'T EXIST, SET THESE VALUES AHEAD OF TIME
                CUSTOM_ENTRY_CHANNELS.Add(serverID, 0);
                CUSTOM_ENTRY_ROLES.Add(serverID, 0);
                CUSTOM_MOD_CHANNELS.Add(serverID, 0);
                CUSTOM_GUILD_PREFIXES.Add(serverID, JESS_DEFAULT_PREFIX);
                GUILD_ALLOWS_INVITES.Add(serverID, false);
                GUILD_TOGGLE_JOINMSG.Add(serverID, true);
                GUILD_TOGGLE_BANSMSG.Add(serverID, true);
            }
            // IF SERVER IS REGISTERED, UPDATE INSTEAD
            else
            {
                if (JESS_REGISTERED_GUILDS.Count != 0)
                {
                    for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                    {
                        if (JESS_REGISTERED_GUILDS.ElementAt(i) == serverID)
                        {
                            // FOUND SERVER, UPDATING
                            JESS_REGISTERED_GUILDS[i] = serverID;
                            JESS_REGISTERED_GUILD_NAMES[i] = jess.GetGuild(serverID).Name;

                            // TOGGLE OPENED STATE IF THERE IS AN ENTRY CHANNEL
                            if (CUSTOM_ENTRY_CHANNELS[JESS_REGISTERED_GUILDS.ElementAt(i)] != 0)
                            {
                                GUILD_ALLOWS_INVITES[JESS_REGISTERED_GUILDS.ElementAt(i)] = !opened;
                                open = !opened;
                            }
                            else
                            {
                                e.Channel.SendMessageAsync("Oops! You haven't set an entry channel.");
                                open = false;
                                break;
                            }
                        }
                    }
                }
            }

            bool[] existsAndOpen = { exists, open };

            // RUN THE SAVE FUNCTION THEN EXIT
            SV_Save();
            return existsAndOpen;
        }

        // TWO INPUTS, SERVER AND PREFIX
        bool SV_CheckThenSave(ulong serverID, string prefixValue)
        {
            // ASSUME THE SERVER IS NOT REGISTERED
            bool exists = false;

            // CHECK TO SEE IF IT MIGHT BE
            if (JESS_REGISTERED_GUILDS.Count != 0)
            {
                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                {
                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == serverID)
                    {
                        // SERVER IS REGISTERED, NO NEED TO DO ANYTHING
                        exists = true;
                        break;
                    }
                }
            }

            // IF SERVER IS NOT REGISTERED, REGISTER
            if (!exists)
            {
                JESS_REGISTERED_GUILDS.Add(serverID);
                JESS_REGISTERED_GUILD_NAMES.Add(jess.GetGuild(serverID).Name);

                // SINCE THIS DATA WAS NOT INPUT AND SERVER DOESN'T EXIST, SET THESE VALUES AHEAD OF TIME
                CUSTOM_ENTRY_CHANNELS.Add(serverID, 0);
                CUSTOM_ENTRY_ROLES.Add(serverID, 0);
                CUSTOM_MOD_CHANNELS.Add(serverID, 0);
                CUSTOM_GUILD_PREFIXES.Add(serverID, prefixValue);
                GUILD_ALLOWS_INVITES.Add(serverID, false);
                GUILD_TOGGLE_JOINMSG.Add(serverID, true);
                GUILD_TOGGLE_BANSMSG.Add(serverID, true);
            }
            // IF SERVER IS REGISTERED, UPDATE INSTEAD
            else
            {
                if (JESS_REGISTERED_GUILDS.Count != 0)
                {
                    for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                    {
                        if (JESS_REGISTERED_GUILDS.ElementAt(i) == serverID)
                        {
                            // FOUND SERVER, UPDATING
                            JESS_REGISTERED_GUILDS[i] = serverID;
                            JESS_REGISTERED_GUILD_NAMES[i] = jess.GetGuild(serverID).Name;

                            CUSTOM_GUILD_PREFIXES[JESS_REGISTERED_GUILDS.ElementAt(i)] = prefixValue;
                        }
                    }
                }
            }

            // RUN THE SAVE FUNCTION THEN EXIT
            SV_Save();
            return exists;
        }

        // THREE INPUTS, SERVER, ADDITIONAL ID, AND NAME/CHANNEL MODIFIER
        bool SV_CheckThenSave(ulong serverID, ulong inputID, string caseValue)
        {
            // ASSUME THE SERVER IS NOT REGISTERED
            bool exists = false;

            // SET UP CASEVALUE
            bool forRole = false;
            bool forModChannel = false;
            switch (caseValue)
            {
                case "channel":
                    forRole = false;
                    forModChannel = false;
                    break;
                case "channelMOD":
                    forRole = false;
                    forModChannel = true;
                    break;
                case "role":
                    forRole = true;
                    forModChannel = false;
                    break;
            }

            // CHECK TO SEE IF IT MIGHT BE
            if (JESS_REGISTERED_GUILDS.Count != 0)
            {
                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                {
                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == serverID)
                    {
                        // SERVER IS REGISTERED, NO NEED TO DO ANYTHING
                        exists = true;
                        break;
                    }
                }
            }

            // IF SERVER IS NOT REGISTERED, REGISTER
            if (!exists)
            {
                JESS_REGISTERED_GUILDS.Add(serverID);
                JESS_REGISTERED_GUILD_NAMES.Add(jess.GetGuild(serverID).Name);

                // SINCE THIS DATA WAS NOT INPUT AND SERVER DOESN'T EXIST, SET THESE VALUES AHEAD OF TIME
                if (!forRole && !forModChannel)
                {
                    CUSTOM_ENTRY_CHANNELS.Add(serverID, inputID);
                    CUSTOM_ENTRY_ROLES.Add(serverID, 0);
                    CUSTOM_MOD_CHANNELS.Add(serverID, 0);
                    CUSTOM_GUILD_PREFIXES.Add(serverID, JESS_DEFAULT_PREFIX);
                    GUILD_ALLOWS_INVITES.Add(serverID, false);
                    GUILD_TOGGLE_JOINMSG.Add(serverID, true);
                    GUILD_TOGGLE_BANSMSG.Add(serverID, true);
                }
                else if (!forRole && forModChannel)
                {
                    CUSTOM_ENTRY_CHANNELS.Add(serverID, 0);
                    CUSTOM_ENTRY_ROLES.Add(serverID, 0);
                    CUSTOM_MOD_CHANNELS.Add(serverID, inputID);
                    CUSTOM_GUILD_PREFIXES.Add(serverID, JESS_DEFAULT_PREFIX);
                    GUILD_ALLOWS_INVITES.Add(serverID, false);
                    GUILD_TOGGLE_JOINMSG.Add(serverID, true);
                    GUILD_TOGGLE_BANSMSG.Add(serverID, true);
                }
                else
                {
                    CUSTOM_ENTRY_CHANNELS.Add(serverID, 0);
                    CUSTOM_ENTRY_ROLES.Add(serverID, inputID);
                    CUSTOM_MOD_CHANNELS.Add(serverID, 0);
                    CUSTOM_GUILD_PREFIXES.Add(serverID, JESS_DEFAULT_PREFIX);
                    GUILD_ALLOWS_INVITES.Add(serverID, false);
                    GUILD_TOGGLE_JOINMSG.Add(serverID, true);
                    GUILD_TOGGLE_BANSMSG.Add(serverID, true);
                }
            }
            // IF SERVER IS REGISTERED, UPDATE INSTEAD
            else
            {
                if (JESS_REGISTERED_GUILDS.Count != 0)
                {
                    for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                    {
                        if (JESS_REGISTERED_GUILDS.ElementAt(i) == serverID)
                        {
                            // FOUND SERVER, UPDATING
                            JESS_REGISTERED_GUILDS[i] = serverID;
                            JESS_REGISTERED_GUILD_NAMES[i] = jess.GetGuild(serverID).Name;

                            if (!forRole && !forModChannel)
                            {
                                CUSTOM_ENTRY_CHANNELS[JESS_REGISTERED_GUILDS.ElementAt(i)] = inputID;
                            }
                            else if (!forRole && forModChannel)
                            {
                                CUSTOM_MOD_CHANNELS[JESS_REGISTERED_GUILDS.ElementAt(i)] = inputID;
                            }
                            else
                            {
                                CUSTOM_ENTRY_ROLES[JESS_REGISTERED_GUILDS.ElementAt(i)] = inputID;
                            }
                        }
                    }
                }
            }

            // RUN THE SAVE FUNCTION THEN EXIT
            SV_Save();
            return exists;
        }

        // SAVE SERVER DATA
        void SV_Save()
        {
            // DELETE THE EXISTING FILE (.lgtx is used to hide data just in case but is easily readable)
            File.Delete("JESSBOT_SERVER_DATABASE.lgtx");

            // CREATE NEW FILE
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"JESSBOT_SERVER_DATABASE.lgtx", true))
            {
                foreach (ulong SDBi in CUSTOM_ENTRY_CHANNELS.Keys)
                {
                    file.WriteLine("=====REGISTERED SERVER=====");          // i = 0
                    file.WriteLine("SERVER ID:");                           // i = 1
                    file.WriteLine(SDBi);                                   // i = 2
                    file.WriteLine("SERVER NAME:");                         // i = 3
                    file.WriteLine(jess.GetGuild(SDBi).Name);               // i = 4
                    file.WriteLine("ENTRY CHANNEL:");                       // i = 5
                    file.WriteLine(CUSTOM_ENTRY_CHANNELS[SDBi]);            // i = 6
                    file.WriteLine("ENTRY ROLE:");                          // i = 7
                    file.WriteLine(CUSTOM_ENTRY_ROLES[SDBi]);               // i = 8
                    file.WriteLine("MODERATION CHANNEL:");                  // i = 9
                    file.WriteLine(CUSTOM_MOD_CHANNELS[SDBi]);              // i = 10
                    file.WriteLine("SERVER PREFIX:");                       // i = 11
                    file.WriteLine(CUSTOM_GUILD_PREFIXES[SDBi]);            // i = 12
                    file.WriteLine("INVITES ALLOWED:");                     // i = 13
                    file.WriteLine(GUILD_ALLOWS_INVITES[SDBi]);             // i = 14
                    file.WriteLine("USERLIST CHANGE MESSAGE TOGGLING:");    // i = 15
                    file.WriteLine(GUILD_TOGGLE_JOINMSG[SDBi]);             // i = 16
                    file.WriteLine("USERBAN MESSAGE TOGGLING:");            // i = 17
                    file.WriteLine(GUILD_TOGGLE_BANSMSG[SDBi]);             // i = 18
                    file.WriteLine("");                                     // i = 19
                }
            }
        }

        #endregion

        #region SAVE USER DATA

        // USED TO EFFICIENTLY SAVE USER DATA

        // SAVE THE MASTER LIST
        void SV_SaveMasterUserList()
        {
            // DELETE THE EXISTING FILE (.lgtx is used to hide data just in case but is easily readable)
            File.Delete("JESSBOT_USER_DATABASE.lgtx");

            // CREATE NEW FILE
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"JESSBOT_USER_DATABASE.lgtx", true))
            {
                foreach (ulong SDBi in JESS_REGISTERED_USERS)
                {
                    file.WriteLine("=====REGISTERED USER=====");                                            // i = 0
                    file.WriteLine("USER NAME:");                                                           // i = 1

                    try
                    { file.WriteLine($"{jess.GetUser(SDBi).Username}#{jess.GetUser(SDBi).Discriminator}"); }// i = 2
                    catch (Exception)
                    { file.WriteLine($"COULD NOT FIND"); }                                                  // i = 2

                    file.WriteLine("USER ID:");                                                             // i = 3
                    file.WriteLine(SDBi);                                                                   // i = 4
                    file.WriteLine();                                                                       // i = 5
                }
            }
        }

        // SAVE A USER'S DATA FILES
        void SV_SaveUserDataFiles()
        {
            for (uint i = 0; i < JESS_REGISTERED_USERS.Count; ++i)
            {
                ulong userID = JESS_REGISTERED_USERS[(int)i];

                // CREATE DIRECTORY
                string botPathing = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
                string uniqueDirectory = Path.Combine(botPathing, "Debug/ADV_HIERARCHY/USERS/" + userID.ToString());
                System.IO.Directory.CreateDirectory(uniqueDirectory);

                // DELETE ANY EXISTING FILE BY THIS NAME!
                File.Delete($@"{uniqueDirectory}/{userID}.lgtx");

                // CREATE NEW FILE
                using (System.IO.StreamWriter file = new System.IO.StreamWriter($@"{uniqueDirectory}/{userID}.lgtx", true))
                {
                    string usernameValue = "";

                    try
                    { usernameValue = $"{jess.GetUser(userID).Username}#{jess.GetUser(userID).Discriminator}"; }
                    catch (Exception)
                    { usernameValue = "COULD NOT FIND"; }

                    usernameValue = usernameValue.ToUpper();

                    file.WriteLine($"====={usernameValue}=====");                                       // i = 0
                    file.WriteLine("USER ID:");                                                         // i = 1
                    file.WriteLine(userID);                                                             // i = 2
                    file.WriteLine("EXPERIENCE VALUE:");                                                // i = 3
                    file.WriteLine(JESS_REGISTERED_USERS_DATA[(int)i].GetExperience().ToString());      // i = 4
                    file.WriteLine("MONETARY VALUE:");                                                  // i = 5
                    file.WriteLine(JESS_REGISTERED_USERS_DATA[(int)i].GetMonetaryValue().ToString());   // i = 6
                    file.WriteLine();                                                                   // i = 7

                    
                }

                // CREATE EXPERIENCE SUBDIRECTORY
                string EXP_subDirectory = Path.Combine(uniqueDirectory, "EXPERIENCE");
                System.IO.Directory.CreateDirectory(EXP_subDirectory);

                // DELETE ANY EXISTING FILES BY THESE NAMES!
                File.Delete($@"{EXP_subDirectory}/exp_props_{userID}.lgtx");
                File.Delete($@"{EXP_subDirectory}/exp_unlocks_{userID}.lgtx");
                File.Delete($@"{EXP_subDirectory}/readme.txt");

                // CREATE NEW FILES
                using (System.IO.StreamWriter file = new System.IO.StreamWriter($@"{EXP_subDirectory}/readme.txt", true))
                {
                    // Write a readme so users understand what each file does.
                    file.WriteLine($"The \"exp_props_{userID}.lgtx\" is set up in such a way that the ExperienceProperties file's properties Dictionary can be stored. Each numeric value represents a property value that is set either true or false for this specific user, which is represented immediately beneath it. It is not a user-friendly read, but its storage mechanism is not user-friendly, either, which is why this readme is provided.");
                    file.WriteLine();
                    file.WriteLine($"The \"exp_unlocks_{userID}.lgtx\" is set up in such a way that the ExperienceProperties file's unlocks Dictionary can be stored. Each numeric value represents an experience level that is set true or false for this specific user - based on whether they have attained that level - which is represented immediately beneath it. It is not a user-friendly read, but its storage mechanism is not user-friendly, either, which is why this readme is provided.");
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter($@"{EXP_subDirectory}/exp_props_{userID}.lgtx", true))
                {
                    string usernameValue = "";

                    try
                    { usernameValue = $"{jess.GetUser(userID).Username}#{jess.GetUser(userID).Discriminator}"; }
                    catch (Exception)
                    { usernameValue = "COULD NOT FIND"; }

                    usernameValue = usernameValue.ToUpper();

                    file.WriteLine($"====={usernameValue} EXP PROPERTIES=====");    // i = 0

                    for (ulong p = 0; p < (ulong)JESS_REGISTERED_USERS_DATA[(int)i].userExperienceProperties.properties.Count; p++)
                    {
                        ulong property = JESS_REGISTERED_USERS_DATA[(int)i].userExperienceProperties.properties.Keys.ElementAt((int)p);
                        bool isActive = JESS_REGISTERED_USERS_DATA[(int)i].userExperienceProperties.properties.Values.ElementAt((int)p);

                        file.WriteLine(property);                                   // i = 1 + (2 * p)
                        file.WriteLine(isActive);                                   // i = 2 + (2 * p)
                    }
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter($@"{EXP_subDirectory}/exp_unlocks_{userID}.lgtx", true))
                {
                    string usernameValue = "";

                    try
                    { usernameValue = $"{jess.GetUser(userID).Username}#{jess.GetUser(userID).Discriminator}"; }
                    catch (Exception)
                    { usernameValue = "COULD NOT FIND"; }

                    usernameValue = usernameValue.ToUpper();

                    file.WriteLine($"====={usernameValue} EXP UNLOCKS=====");       // i = 0

                    for (ulong u = 0; u < (ulong)JESS_REGISTERED_USERS_DATA[(int)i].userExperienceProperties.unlocks.Count; u++)
                    {
                        ulong unlock = JESS_REGISTERED_USERS_DATA[(int)i].userExperienceProperties.unlocks.Keys.ElementAt((int)u);
                        bool isActive = JESS_REGISTERED_USERS_DATA[(int)i].userExperienceProperties.unlocks.Values.ElementAt((int)u);

                        file.WriteLine(unlock);                                     // i = 1 + (2 * u)
                        file.WriteLine(isActive);                                   // i = 2 + (2 * u)
                    }
                }

                // CREATE ECONOMY SUBDIRECTORY
                string ECON_subDirectory = Path.Combine(uniqueDirectory, "ECONOMY");
                System.IO.Directory.CreateDirectory(ECON_subDirectory);

                // DELETE ANY EXISTING FILES BY THESE NAMES!
                File.Delete($@"{ECON_subDirectory}/econ_tiers_{userID}.lgtx");
                File.Delete($@"{ECON_subDirectory}/econ_props_{userID}.lgtx");
                File.Delete($@"{ECON_subDirectory}/readme.txt");

                // CREATE NEW FILES
                using (System.IO.StreamWriter file = new System.IO.StreamWriter($@"{ECON_subDirectory}/readme.txt", true))
                {
                    // Write a readme so users understand what each file does.
                    file.WriteLine($"The \"econ_props_{userID}.lgtx\" is set up in such a way that the EconomyProperties file's properties Dictionary can be stored. Each numeric value represents a property value that is set either true or false for this specific user, which is represented immediately beneath it. It is not a user-friendly read, but its storage mechanism is not user-friendly, either, which is why this readme is provided.");
                    file.WriteLine();
                    file.WriteLine($"The \"econ_tiers_{userID}.lgtx\" is set up in such a way that the EconomyProperties file's unlocks Dictionary can be stored. Each numeric value represents an economy tier that is set true or false for this specific user - based on whether they have purchased that tier - which is represented immediately beneath it. It is not a user-friendly read, but its storage mechanism is not user-friendly, either, which is why this readme is provided.");
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter($@"{ECON_subDirectory}/econ_props_{userID}.lgtx", true))
                {
                    string usernameValue = "";

                    try
                    { usernameValue = $"{jess.GetUser(userID).Username}#{jess.GetUser(userID).Discriminator}"; }
                    catch (Exception)
                    { usernameValue = "COULD NOT FIND"; }

                    usernameValue = usernameValue.ToUpper();

                    file.WriteLine($"====={usernameValue} ECON PROPERTIES=====");   // i = 0

                    for (ulong p = 0; p < (ulong)JESS_REGISTERED_USERS_DATA[(int)i].userEconomyProperties.properties.Count; p++)
                    {
                        ulong property = JESS_REGISTERED_USERS_DATA[(int)i].userEconomyProperties.properties.Keys.ElementAt((int)p);
                        bool isActive = JESS_REGISTERED_USERS_DATA[(int)i].userEconomyProperties.properties.Values.ElementAt((int)p);

                        file.WriteLine(property);                                   // i = 1 + (2 * p)
                        file.WriteLine(isActive);                                   // i = 2 + (2 * p)
                    }
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter($@"{ECON_subDirectory}/econ_tiers_{userID}.lgtx", true))
                {
                    string usernameValue = "";

                    try
                    { usernameValue = $"{jess.GetUser(userID).Username}#{jess.GetUser(userID).Discriminator}"; }
                    catch (Exception)
                    { usernameValue = "COULD NOT FIND"; }

                    usernameValue = usernameValue.ToUpper();

                    file.WriteLine($"====={usernameValue} ECON TIERS=====");        // i = 0

                    for (ulong u = 0; u < (ulong)JESS_REGISTERED_USERS_DATA[(int)i].userEconomyProperties.tiers.Count; u++)
                    {
                        ulong tier = JESS_REGISTERED_USERS_DATA[(int)i].userEconomyProperties.tiers.Keys.ElementAt((int)u);
                        bool isActive = JESS_REGISTERED_USERS_DATA[(int)i].userEconomyProperties.tiers.Values.ElementAt((int)u);

                        file.WriteLine(tier);                                       // i = 1 + (2 * u)
                        file.WriteLine(isActive);                                   // i = 2 + (2 * u)
                    }
                }
            }
        }

        #endregion

        #region ADMIN OPERATIONS

        // HANDLE ALL ADMINISTRATIVE OPERATIONS

        #region TOGGLERS

        // TOGGLER FUNCTIONS

        // TWO INPUTS, SERVER ID AND CASE VALUE
        bool ADM_CheckToggleThenSave(ulong serverID, string caseValue)
        {
            // ASSUME THE SERVER IS NOT REGISTERED
            bool exists = false;

            // SET UP CASEVALUE
            bool isForBans = false;
            switch (caseValue)
            {
                case "gen_userjoin":
                    isForBans = false;
                    break;
                case "gen_userbans":
                    isForBans = true;
                    break;
            }

            // CHECK TO SEE IF IT MIGHT BE
            if (JESS_REGISTERED_GUILDS.Count != 0)
            {
                for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                {
                    if (JESS_REGISTERED_GUILDS.ElementAt(i) == serverID)
                    {
                        // SERVER IS REGISTERED, NO NEED TO DO ANYTHING
                        exists = true;
                        break;
                    }
                }
            }

            // IF SERVER IS NOT REGISTERED, REGISTER
            if (!exists)
            {
                JESS_REGISTERED_GUILDS.Add(serverID);
                JESS_REGISTERED_GUILD_NAMES.Add(jess.GetGuild(serverID).Name);

                // SINCE THIS DATA WAS NOT INPUT AND SERVER DOESN'T EXIST, SET THESE VALUES AHEAD OF TIME
                if (!isForBans)
                {
                    CUSTOM_ENTRY_CHANNELS.Add(serverID, 0);
                    CUSTOM_ENTRY_ROLES.Add(serverID, 0);
                    CUSTOM_MOD_CHANNELS.Add(serverID, 0);
                    CUSTOM_GUILD_PREFIXES.Add(serverID, JESS_DEFAULT_PREFIX);
                    GUILD_ALLOWS_INVITES.Add(serverID, false);
                    GUILD_TOGGLE_JOINMSG.Add(serverID, false);
                    GUILD_TOGGLE_BANSMSG.Add(serverID, true);
                }
                else
                {
                    CUSTOM_ENTRY_CHANNELS.Add(serverID, 0);
                    CUSTOM_ENTRY_ROLES.Add(serverID, 0);
                    CUSTOM_MOD_CHANNELS.Add(serverID, 0);
                    CUSTOM_GUILD_PREFIXES.Add(serverID, JESS_DEFAULT_PREFIX);
                    GUILD_ALLOWS_INVITES.Add(serverID, false);
                    GUILD_TOGGLE_JOINMSG.Add(serverID, true);
                    GUILD_TOGGLE_BANSMSG.Add(serverID, false);
                }
            }
            // IF SERVER IS REGISTERED, UPDATE INSTEAD
            else
            {
                if (JESS_REGISTERED_GUILDS.Count != 0)
                {
                    for (int i = 0; i < JESS_REGISTERED_GUILDS.Count; i++)
                    {
                        if (JESS_REGISTERED_GUILDS.ElementAt(i) == serverID)
                        {
                            // FOUND SERVER, UPDATING
                            JESS_REGISTERED_GUILDS[i] = serverID;
                            JESS_REGISTERED_GUILD_NAMES[i] = jess.GetGuild(serverID).Name;

                            if (!isForBans)
                            {
                                // SET APPROPRIATE TOGGLED VALUES
                                if (GUILD_TOGGLE_JOINMSG[JESS_REGISTERED_GUILDS.ElementAt(i)])
                                {
                                    (GUILD_TOGGLE_JOINMSG[JESS_REGISTERED_GUILDS.ElementAt(i)]) = false;
                                }
                                else
                                {
                                    (GUILD_TOGGLE_JOINMSG[JESS_REGISTERED_GUILDS.ElementAt(i)]) = true;
                                }
                            }
                            else
                            {
                                // SET APPROPRIATE TOGGLED VALUES
                                if (GUILD_TOGGLE_BANSMSG[JESS_REGISTERED_GUILDS.ElementAt(i)])
                                {
                                    (GUILD_TOGGLE_BANSMSG[JESS_REGISTERED_GUILDS.ElementAt(i)]) = false;
                                }
                                else
                                {
                                    (GUILD_TOGGLE_BANSMSG[JESS_REGISTERED_GUILDS.ElementAt(i)]) = true;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            // RUN THE SAVE FUNCTION THEN EXIT
            SV_Save();
            return exists;
        }

        #endregion

        #endregion

        #endregion
    }

    #endregion

    // A custom library embedded into the bot. It is mandatory that this is embedded into the bot as it requires things unique to the bot itself.
    #region JESSBOT2.0LIB

    // Things that don't quite fit in any other section, or are used by multiple sections
    #region GENERICS

    // This is used for a new user. The user referendum is logged with a combination of a ulong list containing all user IDs (for accessing a user's referendum) and the referendum file itself, which contains references to other files that are used to keep track of everything the user does and owns. It is designed to be a cleaner alternative to a master user list.
    class UserReferendum
    {
        ulong discordIdentifier = 0; // Used in conjunction with the master user list file.

        BigInteger userExperienceValue = new BigInteger(); // Used to store the user's experience points.
        BigInteger userMonetaryValue = new BigInteger(); // Used to store the user's balance.

        ulong userExperienceLevel = 0; // Used to store the user's experience level.

        public ExperienceProperties userExperienceProperties = new ExperienceProperties(); // Used to track what has happened with a user's experience level.

        public EconomyProperties userEconomyProperties = new EconomyProperties(); // Used to track what has happened with this user's specific store purchases. Also used to hold their inventory.

        public Inventory userInventory = new Inventory(); // Used to manage the user's inventory.

        // Initializes a user so they may be registered.
        public UserReferendum(ulong identification)
        {
            discordIdentifier = identification;
        }

        public void SetExperience(BigInteger experience)
        {
            ulong oldLevel = userExperienceLevel;
            userExperienceValue = experience;

            for (uint i = 0; i < DiscordBot.CONST_JESS_LEVEL_TOTAL_NUMS.Count; ++i)
            {
                if (DiscordBot.CONST_JESS_LEVEL_TOTAL_NUMS[i] <= userExperienceValue)
                {
                    userExperienceLevel = i;

                    if (oldLevel < userExperienceLevel)
                    {
                        DiscordBot.JESS_FALLBACK_USER_LEVELLED_UP = true;
                    }
                }
                else
                { break; }
            }
        }

        public void SetExperience(BigInteger experience, bool doIRunLevelUp)
        {
            ulong oldLevel = userExperienceLevel;
            userExperienceValue = experience;

            for (uint i = 0; i < DiscordBot.CONST_JESS_LEVEL_TOTAL_NUMS.Count; ++i)
            {
                if (DiscordBot.CONST_JESS_LEVEL_TOTAL_NUMS[i] <= userExperienceValue)
                {
                    userExperienceLevel = i;

                    if (oldLevel < userExperienceLevel && doIRunLevelUp)
                    {
                        DiscordBot.JESS_FALLBACK_USER_LEVELLED_UP = true;
                    }
                }
                else
                { break; }
            }
        }

        public void SetMonetaryValue(BigInteger cash)
        {
            userMonetaryValue = cash;
        }

        public BigInteger GetExperience()
        {
            return userExperienceValue;
        }

        public BigInteger GetMonetaryValue()
        {
            return userMonetaryValue;
        }

        public ulong GetExperienceLevel()
        {
            return userExperienceLevel;
        }
    }

    #endregion

    // Things pertaining to experience levelling and such
    #region EXPERIENCE

    // Used by the bot to determine if any additional modifications should be made
    class ExperienceProperties
    {
        // Make sure these Dictionaries are public. They will have some advanced modifications so it is good to know what these are.
        public Dictionary<ulong, bool> properties = new Dictionary<ulong, bool>();

        public Dictionary<ulong, bool> unlocks = new Dictionary<ulong, bool>();

        // This will initialize the class. 
        public ExperienceProperties()
        {
            // All will be set to false since they will be set true over time or upon loading.
            // Initialize the properties.
            properties.Add(00, false); // Boost
            properties.Add(01, false);
            properties.Add(02, false);
            properties.Add(03, false);
            properties.Add(04, false);
            properties.Add(05, false);
            properties.Add(06, false);
            properties.Add(07, false);
            properties.Add(08, false);
            properties.Add(09, false);

            // Initialize some unlocks.
            unlocks.Add(2, false); // Unlocks for level two.
            unlocks.Add(5, false); // Unlocks for level five.
        }
    }

    #endregion

    // Things pertaining to the economy and such
    #region ECONOMY

    // Used by the bot to determine if any additional modifications should be made
    class EconomyProperties
    {
        // Make sure these Dictionaries are public. They will have some advanced modifications so it is good to know what these are.
        public Dictionary<ulong, bool> properties = new Dictionary<ulong, bool>();

        public Dictionary<ulong, bool> tiers = new Dictionary<ulong, bool>();

        // This will initialize the class. 
        public EconomyProperties()
        {
            // All will be set to false since they will be set true over time or upon loading.
            // Initialize the properties.
            properties.Add(00, false); // Boost
            properties.Add(01, false);
            properties.Add(02, false);
            properties.Add(03, false);
            properties.Add(04, false);
            properties.Add(05, false);
            properties.Add(06, false);
            properties.Add(07, false);
            properties.Add(08, false);
            properties.Add(09, false);

            // Initialize some tiers.
            tiers.Add(1, false); // Used in determining what the user can see and buy in the store. Unlocks Tier 1 items.
            tiers.Add(2, false); // Used in determining what the user can see and buy in the store. Unlocks Tier 2 items.
        }
    }

    // Used by the bot to determine if any additional modifications should be made
    class Inventory
    {
        // Make sure these Dictionaries are public. They will have some advanced modifications so it is good to know what these are.
        public Dictionary<ulong, BigInteger> TL_tools = new Dictionary<ulong, BigInteger>();

        public Dictionary<ulong, BigInteger> FDS_foodstuffs = new Dictionary<ulong, BigInteger>();

        public Dictionary<ulong, BigInteger> RM_rawmats = new Dictionary<ulong, BigInteger>();

        // This will initialize the class. 
        public Inventory()
        {
            // Initialize tools.
            TL_tools.Add(00, 0); // Fishing Rod I (TL00)

            // Initialize foodstuffs.
            FDS_foodstuffs.Add(00, 0); // Fish, Tiny (FDS00)
            FDS_foodstuffs.Add(01, 0); // Fish, Small (FDS01)
            FDS_foodstuffs.Add(02, 0); // Fish, Medium (FDS02)
            FDS_foodstuffs.Add(03, 0); // Fish, Large (FDS03)
            FDS_foodstuffs.Add(04, 0); // Fish, Huge (FDS04)

            // Initialize raw materials.
            RM_rawmats.Add(00, 0); // Gold Nugget, Tiny (RM00)
            RM_rawmats.Add(01, 0); // Gold Nugget, Small (RM01)
        }
    }

    #endregion

    #endregion
}