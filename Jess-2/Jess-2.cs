using Discord;
using Discord.WebSocket;
using Discord.Commands;

using System;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Jess
{
    class DiscordBot
    {
        // NEVER FORGET QFTXJ: 158208296837185546

        // USE THIS TO GIVE BOT OVERLORD STATUS TO USERS
        static ulong[] uAuth = { 236738543387541507, 553420930244673536, 559942856249442305, 527695211485462580 };

        // USE THIS TO TROLL USERS IN THE ECON
        static ulong[] uTroll = { 250993708470632450, 564848790478127139, 219330484243660800, 189897843480199168 };

        // Serverdata dictionaries
        static Dictionary<ulong, ulong> welcomeChannels = new Dictionary<ulong, ulong>(); // #doormat, for example
        static Dictionary<ulong, ulong> peefChannels = new Dictionary<ulong, ulong>(); // Deprecated
        static Dictionary<ulong, ulong> modChannels = new Dictionary<ulong, ulong>(); // Mod logs?

        static Dictionary<ulong, ulong> marionettes = new Dictionary<ulong, ulong>(); // ?? Forgot which channels these are
        static Dictionary<ulong, ulong> bindChannels = new Dictionary<ulong, ulong>(); // Channels to set bind
        static Dictionary<ulong, ulong> musicChannels = new Dictionary<ulong, ulong>(); // Post music here

        static Dictionary<ulong, ulong> baseRoles = new Dictionary<ulong, ulong>(); // Sets base roles

        static List<ulong> leftServers = new List<ulong>(); // For "has left server"

        // Hardcoded data
        static string cmdPrefix = "JR."; // Hardcoded prefix. You may wish to change to dictionary later, this is PREFERABLE.
        static string versNum = "v0.00.1.5"; // Version number
        static string versUpdateInfo = $"enhances the economy."; // Version desc.

        // Startup data + is say locked by default?
        bool hasAlreadyStarted = false;
        bool isDebugging = false;
        bool sendintro = false;
        bool saylocked = true;

        // For peef status changes
        bool alreadyOnline = true;
        bool alreadyIdle = false;
        bool alreadyDND = false;
        bool alreadyAFK = false;
        bool alreadyOffline = false;
        bool peefStateMsgSend = Convert.ToBoolean(File.ReadAllText("pSMS.txt"));

        // Emotion handling data
        #region emotionHandler
        //For Jessica emotion handling (previously intended to be added to TAIANA but was never implemented)
        static List<ulong> serverList = new List<ulong>();
        static int[] emoteState;
        static int[] emoteStateCalc; // Used by program to determine emotional state
        static int[] emoteStateTimer; // Simulates a timer. Runs only when the leave threshold is met. When timer runs out, Jessica will return to -10.
        static bool[] outOfRoom;
        const int TH_TIMER = 60;
        const int TH_HAPPY = 10; // Positive emotional state #1
        const int TH_GLAD = 55; // Positive emotional state #2 | JR.insult ineffective
        const int TH_ECSTATIC = 110; // Positive emotional state #3 | JR.mock ineffective | JR.insult fails
        const int TH_SAD = -10; // Negative emotional state #1 | JR.mock is more effective
        const int TH_SORROW = -55; // Negative emotional state #2 | JR.calm effective | JR.mock drops to depressed | JR.compliment counter-effective
        const int TH_DEPRESSED = -110; // Negative emotional state #3 | JR.mock passes leave threshold | JR.insult switches to the mock responses
        const int TH_TOLEAVE = -250; // Low enough emotional state that Jessica self-calls JR.leaveserver until JR.calm is used
        #endregion

        // Economy handling data
        #region currencyHandler
        static Dictionary<ulong, long> balanceOut = new Dictionary<ulong, long>();
        static Dictionary<ulong, double> ecBT = new Dictionary<ulong, double>();
        static Dictionary<ulong, bool> ecB1 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecB2 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecB3 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecB4 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecB5 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecB6 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecB7 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecB8 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecB9 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecB10 = new Dictionary<ulong, bool>();

        static Dictionary<ulong, double> ecSBT = new Dictionary<ulong, double>();
        static Dictionary<ulong, bool> ecSB = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecSB1 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecSB2 = new Dictionary<ulong, bool>();

        static Dictionary<ulong, double> ecART = new Dictionary<ulong, double>();
        static Dictionary<ulong, bool> ecAR = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecAR1 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecAR2 = new Dictionary<ulong, bool>();

        static Dictionary<ulong, double> ecGBT = new Dictionary<ulong, double>();
        static Dictionary<ulong, bool> ecGB = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecGB1 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecGB2 = new Dictionary<ulong, bool>();
        static Dictionary<ulong, bool> ecGB3 = new Dictionary<ulong, bool>();

        static Dictionary<ulong, ulong> ITEM_EC00 = new Dictionary<ulong, ulong>(); // Fishing rod
        static Dictionary<ulong, ulong> ITEM_EC01 = new Dictionary<ulong, ulong>(); // Small fish
        static Dictionary<ulong, ulong> ITEM_EC02 = new Dictionary<ulong, ulong>(); // Medium fish
        static Dictionary<ulong, ulong> ITEM_EC03 = new Dictionary<ulong, ulong>(); // Soggy boot
        static Dictionary<ulong, ulong> ITEM_EC04 = new Dictionary<ulong, ulong>(); // Small cooked fish
        static Dictionary<ulong, ulong> ITEM_EC05 = new Dictionary<ulong, ulong>(); // Medium cooked fish
        static Dictionary<ulong, ulong> ITEM_EC06 = new Dictionary<ulong, ulong>(); // Small gold chunk
        static Dictionary<ulong, ulong> ITEM_EC07 = new Dictionary<ulong, ulong>(); // Boot
        static Dictionary<ulong, ulong> ITEM_EC08 = new Dictionary<ulong, ulong>(); // Boots

        static List<int> econTimeOut = new List<int>();
        static List<bool> econUserBlock = new List<bool>();
        #endregion

        // Random
        #region randomized
        Random rand_num;
        string[] no_u;
        string[] hi;
        // Old response systems, maintain for neutral
        string[] poke_response;
        string[] tap_response;
        string[] call_response;
        string[] pat_response;
        string[] pet_response;
        // New emotion response systems
        #region happy responses
        string[] poke_rHPY;
        string[] pat_rHPY;
        string[] pet_rHPY;
        #endregion
        #region glad responses
        string[] poke_rGLD;
        string[] pet_rGLD;
        #endregion
        #region ecstatic responses
        string[] poke_rECS;
        string[] pet_rECS;
        #endregion
        #region sad responses
        string[] poke_rSAD;
        string[] tap_rSAD;
        string[] call_rSAD;
        string[] pat_rSAD;
        string[] pet_rSAD;
        #endregion
        #region sorrow responses
        string[] poke_rSRW;
        string[] pat_rSRW;
        string[] pet_rSRW;
        #endregion
        #region depressed responses
        string[] poke_rDPR;
        string[] tap_rDPR;
        string[] call_rDPR;
        string[] pat_rDPR;
        string[] pet_rDPR;
        #endregion
        // New emotion generals
        #region compliment
        string[] comp_effective;
        string[] comp_countereff;
        #endregion
        #region calm
        string[] calm_effective;
        string[] calm_noeffect;
        #endregion
        #region insult
        string[] insult_effective;
        string[] insult_ineffective;
        string[] insult_fail;
        #endregion
        #region mock
        string mock_toDepressed = $"*sniff* Why are you so mean?";
        string mock_forceLeave = $"*sobbing* G-get away from me! *runs out of room* \n\n\n\n\n***...You bastard.***";
        string[] mock_moreeffective;
        string[] mock_effective;
        string[] mock_ineffective;
        #endregion
        string emoteJessLeaves = $"*sniffling, crying* I-I'm gonna go... *leaves room sobbing*";
        string emoteJessRecover = $"*Jessica has recovered and re-entered the room*";
        string moodPOS3 = "Jessica is ecstatic.";
        string moodPOS2 = "Jessica is glad.";
        string moodPOS1 = "Jessica is happy.";
        string moodNEUT = "Jessica is contented.";
        string moodNEG1 = "Jessica is sad.";
        string moodNEG2 = "Jessica is sorrowful.";
        string moodNEG3 = "Jessica is depressed.";
        string moodLEFT = $"*Jessica is too upset to return to the room*";
        // Maintained response systems
        string[] peef_response;
        string[] musp_response;
        string[] memes_and_moments;
        string[] dnd_roll;
        string[] annoy_user;
        #endregion

        // Main program
        static DiscordSocketClient jess;

        // Start program
        static void Main(string[] args)
        {
            new DiscordBot().MainAsync().GetAwaiter().GetResult();
        }

        // Startup code
        public DiscordBot()
        {
            jess = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 100
            });

            jess.SetGameAsync("use JR.help");

            jess.LoginAsync(TokenType.Bot, File.ReadAllText("token.txt"));
            jess.StartAsync();
        }

        // Program run cycle
        private async Task MainAsync()
        {
            //HANDLE USERJOINS AND USERDROPS
            jess.UserJoined += PingMeOnUserJoin;
            jess.UserLeft += UserDropped;
            jess.UserBanned += UserBanned;

            //checks boolean value to see if peef wants messages sent, ever
            //currently relies on a text file without write/rewrite perms
            //will need to work with other coders to make this variable affected by a toggle command, permanently each time
            //will take lots of work but will hopefully make the bot work efficiently lol
            //UPDATE: 4/3/19: DEPRECATED
            if (peefStateMsgSend)
            {
                jess.GuildMemberUpdated += StatusChanged;
            }

            //MAIN FUNCTIONALITY AND COMMAND HANDLING
            jess.MessageReceived += ParseCommand;

            //UPTIME NOTIFICATIONS: DEPRECATED
            if (!hasAlreadyStarted && !isDebugging && sendintro)
            {
                jess.Ready += NotifyUsersUponUptime;
            }
            else if (!hasAlreadyStarted && isDebugging && sendintro)
            {
                jess.Ready += NotifyUsersUponUptimeDebug;
            }

            //LOADUP
            string[] loadData = File.ReadAllLines("serverinfo.txt");
            for (int i = 0; i < loadData.Length; i += 9)
            {
                serverList.Add(ulong.Parse(loadData[i]));
                if (i + 8 < loadData.Length)
                {
                    // USER ID
                    // USERNAME
                    welcomeChannels.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 2])); // SEND A WELCOME MESSAGE
                    marionettes.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 3])); // UNSURE WHAT THIS DOES
                    bindChannels.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 4])); // UNSURE WHAT THIS DOES
                    musicChannels.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 5])); // SENDS MUSIC HERE...?
                    baseRoles.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 6])); // ADD A ROLE TO NEW USERS
                    peefChannels.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 7])); // DEPRECATED
                    modChannels.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 8])); // MODERATOR CHANNELS
                }
            }
            
            string[] loadUserData = File.ReadAllLines("usersinfo.txt");
            for (int i = 0; i < loadUserData.Length; i += 41)
            {
                if (i + 1 < loadUserData.Length)
                {
                    // USER ID
                    // USERNAME
                    // "BALANCE"
                    balanceOut.Add(ulong.Parse(loadUserData[i]), long.Parse(loadUserData[i + 3]));
                    ecBT.Add(ulong.Parse(loadUserData[i]), double.Parse(loadUserData[i + 4]));
                    ecB1.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 5]));
                    ecB2.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 6]));
                    ecB3.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 7]));
                    ecB4.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 8]));
                    ecB5.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 9]));
                    ecB6.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 10]));
                    ecB7.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 11]));
                    ecB8.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 12]));
                    ecB9.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 13]));
                    ecB10.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 14]));
                    // "SIPHON"
                    ecSBT.Add(ulong.Parse(loadUserData[i]), double.Parse(loadUserData[i + 16]));
                    ecSB.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 17]));
                    ecSB1.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 18]));
                    ecSB2.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 19]));
                    // "ADDITIVE RANDOM"
                    ecART.Add(ulong.Parse(loadUserData[i]), double.Parse(loadUserData[i + 21]));
                    ecAR.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 22]));
                    ecAR1.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 23]));
                    ecAR2.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 24]));
                    // "GAMBLER"
                    ecGBT.Add(ulong.Parse(loadUserData[i]), double.Parse(loadUserData[i + 26]));
                    ecGB.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 27]));
                    ecGB1.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 28]));
                    ecGB2.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 29]));
                    ecGB3.Add(ulong.Parse(loadUserData[i]), bool.Parse(loadUserData[i + 30]));
                    // "INVENTORY"
                    ITEM_EC00.Add(ulong.Parse(loadUserData[i]), ulong.Parse(loadUserData[i + 32]));
                    ITEM_EC01.Add(ulong.Parse(loadUserData[i]), ulong.Parse(loadUserData[i + 33]));
                    ITEM_EC02.Add(ulong.Parse(loadUserData[i]), ulong.Parse(loadUserData[i + 34]));
                    ITEM_EC03.Add(ulong.Parse(loadUserData[i]), ulong.Parse(loadUserData[i + 35]));
                    ITEM_EC04.Add(ulong.Parse(loadUserData[i]), ulong.Parse(loadUserData[i + 36]));
                    ITEM_EC05.Add(ulong.Parse(loadUserData[i]), ulong.Parse(loadUserData[i + 37]));
                    ITEM_EC06.Add(ulong.Parse(loadUserData[i]), ulong.Parse(loadUserData[i + 38]));
                    ITEM_EC07.Add(ulong.Parse(loadUserData[i]), ulong.Parse(loadUserData[i + 39]));
                    ITEM_EC08.Add(ulong.Parse(loadUserData[i]), ulong.Parse(loadUserData[i + 40]));

                    econTimeOut.Add(0);
                    econUserBlock.Add(false);
                }
            }

            //LENGTH HANDLING!
            emoteStateCalc = new int[serverList.Count];
            emoteStateTimer = new int[serverList.Count];
            emoteState = new int[serverList.Count];
            outOfRoom = new bool[serverList.Count];

            await Task.Delay(-1);
            hasAlreadyStarted = true;
            sendintro = false;
            saylocked = true;
        }

        //MAIN FUNCTIONALITY!!
        public async Task ParseCommand(SocketMessage e)
        {
            // RANDOM MESSAGE SYSTEMS
            #region randomized message system
            no_u = new string[]
            {
                    "no u", "no me", "haha, no u", "oof, no u", "haha, no me", "oof, no me", "rip", "rip, no me", "rip, no u", "welp", "welp, no u", "welp, no me", "what", "lol, real mature buddy", "lol, no u", "lol, no me", "ur a no u", "lol, real mature buddy... \n \nreaaaaaaaaal mature"
            };
            hi = new string[]
            {
                    $"Hey there! How's it going, {e.Author.Mention}?", "Oh, hi!", "Haha, hi there!", "Hey, how you doing?", "Hello there!", $"Hey, {e.Author.Mention}, how're you doing?", "Hi!", "Hello!"
            };
            peef_response = new string[]
            {
                    $"Schmick.", $"lololoololo", $"Aw, shlupenmoten.", $"Nimgu.", $"Frickety frick, it's frick on a stick!", $"What is pi divided by 4 plus peef cubed?", $"...meep.", $"*ducks* MOTHER!", $"It's only a flesh wound, really.", $"wut", $"When I ask you to jump, you will reply with GUHHHH!", $"WHADDYA MEAN, YOU'RE **AT SOUP?!**", $"I perceived a line of gentlemen holding rifles! But I could not see the target.", $"https://www.youtube.com/watch?v=LCsGCpeoOJ0", $"What is the velocity of an unladen swallow?", $"A man sits at a table with a pile of mud. What comes is used on a table... To eat something that came out of a pile of mud!", "roflawlcoptr", "Oh dippleschmicks, what now", "Ripperoni", "nawwwwwwwwww", "'sup y'all", "y'all's a bunch of silly sons of strange stuff"
            };
            musp_response = new string[]
            {
                $"https://www.youtube.com/watch?v=R8fW_SeNm_8&list=PL7B392D9B49EAF881", $"https://www.youtube.com/watch?v=RJN19V9-8hs", $"https://www.youtube.com/watch?v=FdJxZ3ECwf8", $"https://www.youtube.com/watch?v=r-eMVfiT8_c", $"https://www.youtube.com/watch?v=lcFdB1lTIRA", $"https://www.youtube.com/watch?v=1Ne9KIcXnHQ", $"https://www.youtube.com/watch?v=RMR5zf1J1Hs", $"https://www.youtube.com/watch?v=96-NlOSNIfM", $"https://www.youtube.com/watch?v=wKf98TBrtIk", $"https://www.youtube.com/watch?v=Prl6vr6SLZc", $"https://www.youtube.com/watch?v=zzK4SCnwHuI", $"https://www.youtube.com/watch?v=cb922Sry_DI", $"https://www.youtube.com/watch?v=_z82dcrSqns", $"https://www.youtube.com/watch?v=0ePBmD1L5jk", $"https://www.youtube.com/watch?v=Q9EJmrAKVHM", $"https://www.youtube.com/watch?v=3oIpHvUPQaw", $"https://www.youtube.com/watch?v=K08L8Xsy8Hk", $"https://www.youtube.com/watch?v=I86TfGM2OIA", $"https://www.youtube.com/watch?v=mPOIX7g2kQE", $"https://soundcloud.com/peeftube-music/a-blank-slate-ice-blue-horizons-level-3-part-1-frontier-i-rewrite-v2-unmixed", $"https://www.youtube.com/watch?v=DDihK7YFa7o", $"https://www.youtube.com/watch?v=aK4JSwhdcdE", $"https://www.youtube.com/watch?v=IC0ighYOgSc", $"https://www.youtube.com/watch?v=y_gknRMZ-OU", $"https://www.youtube.com/watch?v=CM-CIvMEqUs", $"https://www.youtube.com/watch?v=q6-ZGAGcJrk", $"https://www.youtube.com/watch?v=guIaCocnNaw", $"https://www.youtube.com/watch?v=JoJiLOafuvk", $"https://www.youtube.com/watch?v=KiKPgmN5jTg", $"https://www.youtube.com/watch?v=TZG1eqPEYbY", $"https://www.youtube.com/watch?v=GNTtR6ZpUOo", $"https://www.youtube.com/watch?v=5jvqI3-H9t4", $"https://www.youtube.com/watch?v=9G58GeB1Q1E", $"https://www.youtube.com/watch?v=Zj5DWB5a2n8", $"https://www.youtube.com/watch?v=4iBBKt2offY", $"https://www.youtube.com/watch?v=OUaiZN4VQws", $"https://www.youtube.com/watch?v=HgzGwKwLmgM", $"https://www.youtube.com/watch?v=dRCymdP_tl8", $"https://www.youtube.com/watch?v=zt20_m1wJ18", $"https://www.youtube.com/watch?v=qpMvS1Q1sos", $"https://www.youtube.com/watch?v=FklUAoZ6KxY", $"https://www.youtube.com/watch?v=BEoGsr0WXVw", $"https://www.youtube.com/watch?v=15JCb6P60Vw", $"https://www.youtube.com/watch?v=8B6jOUzBKYc", $"https://www.youtube.com/watch?v=K3WgUUeSGdU", $"https://www.youtube.com/watch?v=NG-_CJzD1Lc", $"https://www.youtube.com/watch?v=9Xz4NV0zsbY", $"https://www.youtube.com/watch?v=sP-484X6wd4", $"https://www.youtube.com/watch?v=T1ge9YlEtxw", $"https://www.youtube.com/watch?v=HJKvoIvv6SE", $"https://www.youtube.com/watch?v=R-HojwcZJKA"
            };
            memes_and_moments = new string[]
            {
                "memes/mam_0.jpg", "memes/mam_1.jpg", "memes/mam_2.png", "memes/mam_3.jpg", "memes/mam_4.png", "memes/mam_5.jpg", "memes/mam_6.png", "memes/mam_7.png", "memes/mam_8.png", "memes/mam_9.png", "memes/mam_10.png", "memes/mam_11.png", "memes/mam_12.png", "memes/mam_13.png", "memes/mam_14.png", "memes/mam_15.png", "memes/mam_16.png", "memes/mam_17.png", "memes/mam_18.png", "memes/mam_19.jpg", "memes/mam_20.jpg", "memes/mam_21.jpg", "memes/mam_22.png", "memes/mam_23.png", "memes/mam_24.png"
            };
            dnd_roll = new string[]
            {
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20"
            };
            annoy_user = new string[]
            {
                "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20"
            };
            #endregion

            #region emotionally modified message system
            #region old_dyn
            #region neutral / original
            tap_response = new string[]
            {
                    "You've got my attention!", "Yeah-huh?", "Mmhmm?", "Yes?", "What's up?", "You need something?", "Yeah?", "Hm?", "Hmm?", "Uh-huh?", $"Yes, {e.Author.Mention}?", $"What do you need, {e.Author.Mention}?"
            };
            call_response = new string[]
            {
                     "You call?", "I'm here!", "You've got my attention!", "Yeah-huh?", "Mmhmm?", "Yes?", "What's up?", "You need something?", "Yeah?", "Hm?", "Hmm?", "Uh-huh?", $"Yes, {e.Author.Mention}?", $"What do you need, {e.Author.Mention}?"
            };
            poke_response = new string[]
            {
                    "Ow, that hurts, stop it!", "I don't like you right now.", "Go away!", "Ugggggghhhhhhhh!", "Can you not?", "Shouldn't you be doing this on Facebook?", "Do I look like a phone to you?", "Stop it!", "Ever heard of a thing called boundaries?", "Ack, for Pete's sake!"
            };
            pat_response = new string[]
            {
                    "Aw, thanks.", "Heh.", "Nice to know I'm appreciated.", "Thank you.", "Hehe, thank you.", "Heh... thanks.", "Glad to hear someone likes having me around."
            };
            pet_response = new string[]
            {
                    "Awwww, you like my hair, don't you?", "Pull my hair and I'll end you.", "My hair soft today or something?", "You're adorable when you do that, haha.", "Okay, but don't get any strange ideas...", "Okay, but don't you dare get weird with me.", "Awww, how adorable. Just be mindful of boundaries, please."
            };
            #endregion
            #region positive
            #region pos1
            poke_rHPY = new string[]
            {
                    "Ow.", "You know that's rude, right?", "Can you not?", "Shouldn't you be doing this on Facebook?", "Do I look like a phone to you?", "Ack, for Pete's sake!", "Not the best way to get my attention.", "Rude!"
            };
            pat_rHPY = new string[]
            {
                    "Aw, thanks.", "Nice to know I'm appreciated.", "Hehe, thank you.", "Heh... thanks.", "Glad to hear someone likes having me around.", "Haha, thank you.", "Aw, thanks, haha."
            };
            pet_rHPY = new string[]
            {
                    "Awwww, you like my hair, don't you?", "Heh, okay then.", "My hair must be soft today or something.", "You're adorable when you do that, haha.", "Okay, but don't get any strange ideas...", "Awww, how adorable. Just be mindful of boundaries, please.", "Wonder how it'd look braided...", "Heh, glad I didn't put it in a ponytail today."
            };
            #endregion
            #region pos2
            poke_rGLD = new string[]
            {
                    "Ow.", "Oof.", "Owww!", "I feel like you've associated me with a phone, haha.", "Ack!", "Gentle, jeez!", "Ouch.", "Ow!"
            };
            pet_rGLD = new string[]
            {
                    "Awwww, you like my hair, don't you?", "My hair must be soft today or something.", "You're adorable when you do that, haha.", "Heh. I feel like a cat.", "Heh. I feel like a dog.", "Wonder how it'd look braided...", "Heh, glad I didn't put it in a ponytail today.", "This is strangely very calming."
            };
            #endregion
            #region pos3
            poke_rECS = new string[]
            {
                    "Ow.", "Oof.", "Heh.", "Owww!", "I feel like you've associated me with a phone, haha.", "Ack!", "Gentle, jeez!", "Ouch.", "Ow!", "Haha, stop!", "Stop it, you silly!", "Poke me one more time, I'ma poke you!", "Yes, I exist, haha."
            };
            pet_rECS = new string[]
            {
                    "Awwww, you like my hair, don't you?", "My hair must be soft today or something.", "You're adorable when you do that, haha.", "Heh. I feel like a cat.", "Heh. I feel like a dog.", "Wonder how it'd look braided...", "Heh, glad I didn't put it in a ponytail today.", "This is strangely very calming.", $"Just *please* don't mess it up, I spent a good while getting it to look like this...", "You want to braid it up?", "You want to put it in a ponytail?"
            };
            #endregion
            #endregion
            #region negative
            #region neg1
            tap_rSAD = new string[]
            {
                    "Hmm...?", "Yeah...?", "Huh...?", "What?", "What's up...?", "Well...?", $"Yes, {e.Author.Mention}...?", $"Yes, {e.Author.Mention}?", $"What do you need, {e.Author.Mention}?"
            };
            call_rSAD = new string[]
            {
                     "Hmm...?", "Yeah...?", "Huh...?", "What?", "What's up...?", "Well...?", $"Yes, {e.Author.Mention}...?", $"You called...?", $"Yes, {e.Author.Mention}?", $"What do you need, {e.Author.Mention}?"
            };
            poke_rSAD = new string[]
            {
                    "Ow, that hurts, stop it!", "I don't like you right now.", "Go away!", "Ugggggghhhhhhhh!", "Can you not?!", "Stop it!", "Ever heard of a thing called boundaries?", "Not okay!", "I'm really not in the mood for this.", "Can you like, go away?"
            };
            pat_rSAD = new string[]
            {
                    "Thanks, I guess...", "Heh... ehh...", "Heh...", "Thank you...", "Heh... thanks....", "Heh... thanks... I guess..."
            };
            pet_rSAD = new string[]
            {
                    "Pull my hair and I'll end you.", "Okay, but don't get any strange ideas...", "Okay, but don't you dare get weird with me.", "Be mindful of boundaries, please.", "Well, might cheer me up a little..."
            };
            #endregion
            #region neg2
            poke_rSRW = new string[]
            {
                    "Owww... please, don't...", "I'm really not in the mood for this...", $"*sob* Why...", "Owwwwwww...", "You're a jerk...", "...", $"*sob*", "GO AWAY!", "Please, don't!", "Stop it, you jerk!", "Get the hell out of my face.", "LEAVE ME ALONE!"
            };
            pat_rSRW = new string[]
            {
                    "Thanks, I guess...", "Heh... ehh...", "Heh...", "Thank you...", "Heh... thanks....", "Heh... thanks... I guess...", $"*sigh*", "I appreciate you trying to cheer me up... *sigh*", $"Thank you... *sniff*", $"Heh... thanks.... *sniff*", $"Heh... thanks... *sniff* I guess...", "Meh..."
            };
            pet_rSRW = new string[]
            {
                    "*sigh* Okay...", "Don't get any strange ideas...", "Whatever...", "Be mindful of boundaries...", "Well, might cheer me up a little...", "...", "*sniff*", "Be mindful of boundaries... *sniff*", "Well, might cheer me up a little... *sniff*", "Maybe not today?", "If I was in a better mood... *sniff* ...maybe."
            };
            #endregion
            #region neg3
            tap_rDPR = new string[]
            {
                    "...", "What do you want?", "Tell me what you want or leave me alone.", $"What, {e.Author.Mention}?", "You have my attention. Speak fast before I go back to moping."
            };
            call_rDPR = new string[]
            {
                     "...", "What do you want?", "Tell me what you want or leave me alone.", $"What, {e.Author.Mention}?", "You have my attention. Speak fast before I go back to moping.", $"You have my attention, {e.Author.Mention}. What do you want?"
            };
            poke_rDPR = new string[]
            {
                    $"*sobbing* ...that hurts... please don't...", "Get away from me...", "Just leave me alone...", "Stop...", "Go bug someone else...", $"*sob* I'M NOT IN THE MOOD. I-I-I.. I... *sobbing*", $"*sobbing* Just go away... p-please..."
            };
            pat_rDPR = new string[]
            {
                    "...", "*sniff*", "*sobbing*", "*while sobbing* ...t-thank-k y-you...", "*while sobbing* ...t-thank-k y-you... I-I... ap-p-p-preciate i-it..."
            };
            pet_rDPR = new string[]
            {
                    "...", "I don't want this right now.", "Thank you, but I'm not in the mood...", "*sobbing* I'm really... not... in the mood..."
            };
            #endregion
            #endregion
            #endregion
            #region new_gen
            #region compliment
            comp_effective = new string[]
            {
                "Oh! Well! I'm flattered!", "Why, thank you!", "Aw, shucks.", "You're too kind.", "Aw, thanks!", "Oh, you're too nice, haha!", "Haha, thanks!", "You really think so?", "Thanks."
            };
            comp_countereff = new string[]
            {
                "W-what?! No! This isn't true!", "I just want to be left alone!", "Stop! Go away!", "Let me mope in peace, okay?", "Leave me alone!", $"Stay away from me, *please!*"
            };
            #endregion
            #region calm
            calm_effective = new string[]
            {
                $"*sniff* Thank you.", "Thanks for being here.", "Thank you..."
            };
            calm_noeffect = new string[]
            {
                "Thanks, but it's unnecessary, haha.", "You really don't need to worry about me, I'm totally fine.", "This is a bit awkward."
            };
            #endregion
            #region insult
            insult_effective = new string[]
            {
                $"What the actual hell? You **jerk!** Go away!", $"Get away from me, you jerk!", "Get out of here!", "Scram!", $"Get your sorry bum out of here before I *make* you get your sorry bum out of here!", "Don't know who taught you your manners, but they did a hell of a poor job!", "RUDE!", "Ugh, great, an insult. Just what I want to deal with right now."
            };
            insult_ineffective = new string[]
            {
                $"*{e.Author.Username.ToUpper()} used INSULT! It's not very effective...*", "Rude.", "Hey, that's rude.", "Heh, that insult was pathetic. Like I mean, my cousin's dog could insult better than you, and he can't even speak.", "Chill.", $"*What* did you just say about my mother?", $"*sigh* Tsk. Tsk."
            };
            insult_fail = new string[]
            {
                "Haha, good one!", "That was a nice burn.", "Nice one.", "Haha, you're not wrong!", $"*smirks in amusement*", $"*{e.Author.Username.ToUpper()} used INSULT! But it didn't do anything...*"
            };
            #endregion
            #region mock
            mock_moreeffective = new string[]
            {
                $"*sob* Y-you're so... so... **mean!** *sobbing*", $"**I _HATE_ YOU!**", $"Shut up, y-you... you... *sob*", $"*while sobbing* Why? Why? What did I do to deserve this?", $"Why must you be such a *jerk?*"
            };
            mock_effective = new string[]
            {
                $"What the actual hell? You **jerk!** Go away!", $"Get away from me, you jerk!", "Get out of here!", "Scram!", $"Get your sorry bum out of here before I *make* you get your sorry bum out of here!", "Don't know who taught you your manners, but they did a hell of a poor job!", "RUDE!", "Ugh, great. Just what I want to deal with right now."
            };
            mock_ineffective = new string[]
            {
                "Hah. Nice try.", "A tad insulting, but whatever.", "Curb your tongue there, buddy."
            };
            #endregion
            #endregion
            #endregion

            // READ DMS!
            if (e.Channel is IDMChannel)
            // if ((e.Channel as IChannel) is SocketDMChannel)
            {
                await cfix(588600079632564234).SendMessageAsync($"{e.Author.Username} ID:{e.Author.Id} | \"{e.Content}\"");

                string rMM = e.Content.ToLower(); //regexMessageMatch

                if (!e.Author.IsBot)
                {
                    if (Regex.IsMatch(rMM, @"\b(hi|hello)\b"))
                    {
                        int greetings_resp_num = rand_num.Next(hi.Length);
                        string greetings_resp_next = hi[greetings_resp_num];

                        await e.Channel.SendMessageAsync(greetings_resp_next);
                        // await e.Channel.SendMessageAsync($"Hi, {e.Author.Mention}!");
                    }

                    if (Regex.IsMatch(rMM, @"\bi'm doing (good|fine|ok|well|great|fantastic|amazing)\b"))
                    {
                        await e.Author.SendMessageAsync("That's good to hear!");
                    }

                    if ((e.Content.ToLower().Contains("no") && e.Content.ToLower().Contains("u")) ||
                        (e.Content.ToLower().Contains("n") && e.Content.ToLower().Contains("o") && e.Content.ToLower().Contains("u")))
                    {
                        string e2 = e.Content.ToLower();
                        string e3 = "";

                        for (int x = 0; x < e2.Length; ++x)
                        {
                            if (Char.IsLetter(e2.ElementAt(x)))
                            {
                                e3 = e3 + e2.ElementAt(x);
                            }

                            if (!Char.IsLetter(e2.ElementAt(x)))
                            {
                                e3 = e3 + " ";
                            }
                        }

                        /* if ((Regex.IsMatch(msg, @"\bno\b") && Regex.IsMatch(msg, @"\bu\b")) ||
                               (Regex.IsMatch(msg, @"\bn\b") && Regex.IsMatch(msg, @"\bo\b") && Regex.IsMatch(msg, @"\bu\b"))) */
                        if (Regex.IsMatch(e3, @"^([Nn]+)\s*([Oo]+)\s*([Uu]+)$"))
                        {
                            int no_u_num = rand_num.Next(no_u.Length);
                            string no_u_next = no_u[no_u_num];
                            await e.Channel.SendMessageAsync(no_u_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                    }
                }
            }
            
            // USER SAYS NO U
            #region no u response system

            #region new system

            #region try 1
            if (e.Author.Id != jess.CurrentUser.Id)
            {
                if ((e.Content.ToLower().Contains("no") && e.Content.ToLower().Contains("u")) ||
                        (e.Content.ToLower().Contains("n") && e.Content.ToLower().Contains("o") && e.Content.ToLower().Contains("u")))
                {
                    string msg = "";

                    for (int x = 0; x < e.Content.Length; ++x)
                    {
                        if (Char.IsLetter(e.Content.ToLower().ElementAt(x)))
                        {
                            msg = msg + e.Content.ToLower().ElementAt(x);
                        }

                        if (!Char.IsLetter(e.Content.ToLower().ElementAt(x)))
                        {
                            msg = msg + " ";
                        }
                    }

                    /* if ((Regex.IsMatch(msg, @"\bno\b") && Regex.IsMatch(msg, @"\bu\b")) ||
                       (Regex.IsMatch(msg, @"\bn\b") && Regex.IsMatch(msg, @"\bo\b") && Regex.IsMatch(msg, @"\bu\b"))) */
                    if (Regex.IsMatch(msg, @"^([Nn]+)\s*([Oo]+)\s*([Uu]+)$"))
                    {
                        int no_u_num = rand_num.Next(no_u.Length);
                        string no_u_next = no_u[no_u_num];
                        await e.Channel.SendMessageAsync(no_u_next);
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                    }
                }
            }
            #endregion

            #endregion

            // string etwo = "a";
            // bool checke = etwo.Any(x => Char.IsWhiteSpace(x)) || etwo.Contains(" ");

            #region base
            /* UNTIL FIXED
            if (checke && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                if (etwo.StartsWith($"N") && etwo.EndsWith($"U") && (e.Author.Id != 268937141101264899))
                {
                    await e.Channel.SendMessageAsync(no_u_next);
                    leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                }
                else if (etwo.StartsWith($"N") && etwo.EndsWith($"U") && (e.Author.Id == 268937141101264899) && !etwo.Contains(" "))
                {
                    await e.Channel.SendMessageAsync(no_u_next + "\nNice try, hackerman.");
                    leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                }
                else if (etwo.StartsWith($"N") && etwo.EndsWith($"U") && (e.Author.Id == 268937141101264899) && etwo.Contains(" "))
                {
                    await e.Channel.SendMessageAsync(no_u_next);
                    leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                }
            }
            
            string ethree = "";
            for (int i = 0; i < etwo.Length; ++i)
            {
                char C_IN = etwo.ElementAt(i);
                string C_OUT = "";
                if (Char.IsWhiteSpace(C_IN) || Char.IsSeparator(C_IN) || C_IN == ' ')
                {
                   C_OUT = " ";
                }
                else if (C_IN == '*')
                {
                   C_OUT = "";
                }
                else if (C_IN == '|')
                {
                   C_OUT = "";
                }
                else if (C_IN == '_')
                {
                   C_OUT = "";
                }
                else if (C_IN == '~')
                {
                    C_OUT = "";
                }
                else if (C_IN == '"')
                {
                    C_OUT = "";
                }
                else
                {
                    C_OUT = $"{C_IN}";
                }

                ethree = ($"{ethree}" + $"{C_OUT}");

                /* if (ethree.Contains("﻿⁠‍‌​᠎ ﻿⁠‍‌​᠎"))
                {
                    ethree = ethree.Replace("﻿⁠‍‌​᠎ ﻿⁠‍‌​᠎", " ");
                }
                if (ethree.Contains("﻿⁠‍‌​᠎"))
                {
                    ethree = ethree.Replace("﻿⁠‍‌​᠎", " ");
                }
            }

            OLD﻿⁠‍‌​᠎﻿⁠‍‌​᠎
            if (ethree.EndsWith($"NO U") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }
            if (ethree.EndsWith($"N O U") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }
            if (ethree.EndsWith($"NAH U") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }
            if (ethree.EndsWith($"NO﻿⁠‍‌​᠎ ﻿⁠‍‌​᠎U") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }
            if (ethree.EndsWith($"N﻿⁠‍‌​᠎O﻿⁠‍‌​᠎ ﻿⁠‍‌​᠎U") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }
            if (ethree.EndsWith($"NO EU") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }
            //
            if (ethree.Contains($" NO U ") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.EndsWith($"NO U.") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.EndsWith($"NO U!") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.EndsWith($"NO U?") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.EndsWith($"NOU") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.EndsWith($"NO.U") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.EndsWith($"NO_U") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.EndsWith($"NO-U") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            //THOMAS PREVENTION SECTION
            if (ethree.EndsWith($"NO U") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }
            #endregion

            #region special
            //BOLD
            if (ethree.EndsWith($"**NO U**") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.Contains($" **NO U** ") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.EndsWith($"**NOU**") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.EndsWith($"**NO.U**") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.EndsWith($"**NO_U**") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (ethree.EndsWith($"**NO-U**") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            //THOMAS PREVENTION SECTION
            if (ethree.EndsWith($"**NO U**") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            #endregion

            #region old
            /*
            if (e.Content.EndsWith($"no u") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (e.Content.Contains($" no u ") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (e.Content.EndsWith($"No u") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int no_u_num = rand_num.Next(no_u.Length);
                string no_u_next = no_u[no_u_num];
                await e.Channel.SendMessageAsync(no_u_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            } 
            */
            #endregion

            #endregion

            // USER SAYS HI: CURRENTLY INACTIVE
            #region greetings response system
            /*
            if (e.Content.EndsWith($"hi") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int hi_num = rand_num.Next(hi.Length);
                string hi_next = hi[hi_num];
                await e.Channel.SendMessageAsync(hi_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (e.Content.EndsWith($"Hi") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int hi_num = rand_num.Next(hi.Length);
                string hi_next = hi[hi_num];
                await e.Channel.SendMessageAsync(hi_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (e.Content.EndsWith($"Hi!") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int hi_num = rand_num.Next(hi.Length);
                string hi_next = hi[hi_num];
                await e.Channel.SendMessageAsync(hi_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (e.Content.EndsWith($"hello") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int hi_num = rand_num.Next(hi.Length);
                string hi_next = hi[hi_num];
                await e.Channel.SendMessageAsync(hi_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (e.Content.EndsWith($"Hello") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int hi_num = rand_num.Next(hi.Length);
                string hi_next = hi[hi_num];
                await e.Channel.SendMessageAsync(hi_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }

            if (e.Content.EndsWith($"Hello!") && (e.Author.Id != jess.CurrentUser.Id))
            {
                int hi_num = rand_num.Next(hi.Length);
                string hi_next = hi[hi_num];
                await e.Channel.SendMessageAsync(hi_next);
                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
            }
            */
            #endregion

            // Server ID handling
            ulong serverId = (e.Channel as SocketGuildChannel).Guild.Id;
            int a = 0;

            // ECON BALANCE CHECKING
            #region runBalanceCheck
            if (!e.Author.IsBot && !e.Content.ToUpper().StartsWith("JR.") && e.MentionedUsers.Count == 0)
            {
                int takeUIDindex = 0;
                ulong takeUID = 0;
                foreach (ulong uID in balanceOut.Keys)
                {
                    if (uID == e.Author.Id)
                    {
                        takeUIDindex = balanceOut.Keys.ToList().IndexOf(uID);
                        takeUID = uID;
                    }
                }

                if (!balanceOut.ContainsKey(e.Author.Id))
                {
                    balanceOut.Add(e.Author.Id, 0);
                    ecBT.Add(e.Author.Id, 1.00);
                    ecB1.Add(e.Author.Id, false);
                    ecB2.Add(e.Author.Id, false);
                    ecB3.Add(e.Author.Id, false);
                    ecB4.Add(e.Author.Id, false);
                    ecB5.Add(e.Author.Id, false);
                    ecB6.Add(e.Author.Id, false);
                    ecB7.Add(e.Author.Id, false);
                    ecB8.Add(e.Author.Id, false);
                    ecB9.Add(e.Author.Id, false);
                    ecB10.Add(e.Author.Id, false);

                    ecSBT.Add(e.Author.Id, 1.00);
                    ecSB.Add(e.Author.Id, false);
                    ecSB1.Add(e.Author.Id, false);
                    ecSB2.Add(e.Author.Id, false);

                    ecART.Add(e.Author.Id, 1.00);
                    ecAR.Add(e.Author.Id, false);
                    ecAR1.Add(e.Author.Id, false);
                    ecAR2.Add(e.Author.Id, false);

                    ecGBT.Add(e.Author.Id, 1.00);
                    ecGB.Add(e.Author.Id, false);
                    ecGB1.Add(e.Author.Id, false);
                    ecGB2.Add(e.Author.Id, false);
                    ecGB3.Add(e.Author.Id, false);
                    
                    ITEM_EC00.Add(e.Author.Id, 0);
                    ITEM_EC01.Add(e.Author.Id, 0);
                    ITEM_EC02.Add(e.Author.Id, 0);
                    ITEM_EC03.Add(e.Author.Id, 0);
                    ITEM_EC04.Add(e.Author.Id, 0);
                    ITEM_EC05.Add(e.Author.Id, 0);
                    ITEM_EC06.Add(e.Author.Id, 0);
                    ITEM_EC07.Add(e.Author.Id, 0);
                    ITEM_EC08.Add(e.Author.Id, 0);

                    econTimeOut.Add(0);
                    econUserBlock.Add(false);
                }

                const double ECBM_1 = 1.25;
                const double ECBM_2 = 1.25;
                const double ECBM_3 = 1.50;
                const double ECBM_4 = 1.50;
                const double ECBM_5 = 1.75;
                const double ECBM_6 = 1.75;
                const double ECBM_7 = 2.00;
                const double ECBM_8 = 2.00;
                const double ECBM_9 = 2.25;
                const double ECBM_10 = 2.25;

                const double ECSBM_1 = 1.25;
                const double ECSBM_2 = 1.50;

                const double ECARM_1 = 1.25;
                const double ECARM_2 = 1.50;

                const double ECGBM_1 = 2.00;

                long x1 = (long)(rand_num.Next(30) + 1);
                long x2 = (long)(rand_num.Next(60) + 1);
                long x3 = (long)(rand_num.Next(90) + 1);
                long x4 = (long)((5 + (long)Math.Log(rand_num.Next(100) + 1)) * 2);
                long x5 = (long)((5 + (long)Math.Log(rand_num.Next(200) + 1)) * 2);
                long xOP = (long)(rand_num.Next(200) + 1);
                long xTR = (long)(rand_num.Next(5) + 1);
                long xTOT = 0;

                long sVAL = 0;

                long arVAL = 0;

                long gVAL = 0;

                // CALCULATE TOTAL BOOST FOR ALL USERS
                foreach (ulong uID in balanceOut.Keys)
                {
                    // DEFAULT
                    ecBT[uID] = 1.00;
                    if (ecB1[uID]) { ecBT[uID] *= ECBM_1; }
                    if (ecB2[uID]) { ecBT[uID] *= ECBM_2; }
                    if (ecB3[uID]) { ecBT[uID] *= ECBM_3; }
                    if (ecB4[uID]) { ecBT[uID] *= ECBM_4; }
                    if (ecB5[uID]) { ecBT[uID] *= ECBM_5; }
                    if (ecB6[uID]) { ecBT[uID] *= ECBM_6; }
                    if (ecB7[uID]) { ecBT[uID] *= ECBM_7; }
                    if (ecB8[uID]) { ecBT[uID] *= ECBM_8; }
                    if (ecB9[uID]) { ecBT[uID] *= ECBM_9; }
                    if (ecB10[uID]) { ecBT[uID] *= ECBM_10; }

                    // SIPHON
                    ecSBT[uID] = 1.00;
                    if (ecSB1[uID]) { ecSBT[uID] *= ECSBM_1; }
                    if (ecSB2[uID]) { ecSBT[uID] *= ECSBM_2; }

                    // ADDRAND
                    ecART[uID] = 1.00;
                    if (ecAR1[uID]) { ecART[uID] *= ECARM_1; }
                    if (ecAR2[uID]) { ecART[uID] *= ECARM_2; }

                    // SIPHON
                    ecGBT[uID] = 1.00;
                    if (ecGB2[uID]) { ecGBT[uID] *= ECGBM_1; }
                }

                double ECBM_T = ecBT[takeUID];

                // KEEP DOWN UNTIL SWITCH IS ACTIVE!
                /* if (Array.IndexOf(uAuth, e.Author.Id) != -1)
                {
                    if (((ulong)balanceOut[e.Author.Id] + (ulong)xOP) > long.MaxValue) { balanceOut[e.Author.Id] = long.MaxValue; }
                    else { xTOT += (long)(xOP * ECBM_T); }
                } */

                /* if (Array.IndexOf(uTroll, e.Author.Id) != -1)
                {
                    if (((ulong)balanceOut[e.Author.Id] + (ulong)xTR) > long.MaxValue) { balanceOut[e.Author.Id] = long.MaxValue; }
                    else { xTOT += (long)(xTR * ECBM_T); }
                } */

                if (econTimeOut[takeUIDindex] >= 0 && !econUserBlock[takeUIDindex])
                {
                    long eL = e.Content.Length;
                    if (eL > 0 && eL < 20)
                        if (((ulong)balanceOut[e.Author.Id] + (ulong)x1) > long.MaxValue) { balanceOut[e.Author.Id] = long.MaxValue; }
                        else { xTOT += (long)(x1 * ECBM_T); }
                    if (eL >= 20 && eL < 50)
                        if (((ulong)balanceOut[e.Author.Id] + (ulong)x2) > long.MaxValue) { balanceOut[e.Author.Id] = long.MaxValue; }
                        else { xTOT += (long)(x2 * ECBM_T); }
                    if (eL >= 50 && eL < 100)
                        if (((ulong)balanceOut[e.Author.Id] + (ulong)x3) > long.MaxValue) { balanceOut[e.Author.Id] = long.MaxValue; }
                        else { xTOT += (long)(x3 * ECBM_T); }
                    if (eL >= 100 && eL < 500)
                        if (((ulong)balanceOut[e.Author.Id] + (ulong)x4) > long.MaxValue) { balanceOut[e.Author.Id] = long.MaxValue; }
                        else { xTOT += (long)(x4 * ECBM_T); }
                    if (eL >= 500)
                        if (((ulong)balanceOut[e.Author.Id] + (ulong)x4) > long.MaxValue) { balanceOut[e.Author.Id] = long.MaxValue; }
                        else { xTOT += (long)(x5 * ECBM_T); }
                }

                balanceOut[e.Author.Id] += xTOT;

                foreach (ulong uID in balanceOut.Keys)
                {
                    double ECSBM_T = ecSBT[uID];
                    sVAL = (long)(xTOT * 0.25 * ECSBM_T);
                    if (uID != e.Author.Id && ecSB[uID]) { balanceOut[uID] += sVAL; }
                }

                foreach (ulong uID in balanceOut.Keys)
                {
                    double ECARM_T = ecART[uID];
                    arVAL = 0;

                    if (rand_num.Next(0, 2) == 1)
                    {
                        arVAL = (long)(xTOT * 2.00 * ECARM_T);
                    }

                    if (uID == e.Author.Id && ecAR[uID]) { balanceOut[uID] += arVAL; }
                }

                foreach (ulong uID in balanceOut.Keys)
                {
                    double ECGBM_T = ecGBT[uID];
                    gVAL = 0;

                    int NEG_CHANCE = 25;
                    int POS_CHANCE = 95;
                    
                    if (ecGB1[uID])
                    {
                        POS_CHANCE -= 10;
                    }

                    if (ecGB3[uID])
                    {
                        POS_CHANCE -= 10;
                    }

                    if (ecGB[uID])
                    {
                        int GAMBLE_CHANCE = rand_num.Next(1, 101);

                        if (GAMBLE_CHANCE <= NEG_CHANCE)
                        {
                            balanceOut[uID] -= rand_num.Next(20, 100);
                            if (balanceOut[uID] < 0)
                            {
                                balanceOut[uID] = 0;
                            }
                        }
                        else if (GAMBLE_CHANCE >= POS_CHANCE)
                        {
                            gVAL = (long)((xTOT + rand_num.Next(20, 100)) * ECGBM_T);
                        }
                    }

                    if (uID == e.Author.Id && ecGB[uID]) { balanceOut[uID] += gVAL; }
                }

                econUserBlock[takeUIDindex] = false;

                /* (REMEMBER TO USE DOUBLE SLASH TWICE TO DEACTIVATE)
                if (Array.IndexOf(uAuth, e.Author.Id) == -1)
                {
                    econTimeOut[takeUIDindex] += 1;
                    if (econTimeOut[takeUIDindex] >= 2)
                    {
                        econTimeOut[takeUIDindex] = -10;
                        econUserBlock[takeUIDindex] = true;
                    }
                } */

                File.Delete("usersinfo.txt");
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"usersinfo.txt", true))
                {
                    foreach (ulong uID in balanceOut.Keys)
                    {
                        // MAIN INFO
                        file.WriteLine(uID);
                        file.WriteLine(tfix(uID).Username);

                        file.WriteLine("BALANCE");

                        file.WriteLine(balanceOut[uID]);
                        file.WriteLine(ecBT[uID]);
                        file.WriteLine(ecB1[uID]);
                        file.WriteLine(ecB2[uID]);
                        file.WriteLine(ecB3[uID]);
                        file.WriteLine(ecB4[uID]);
                        file.WriteLine(ecB5[uID]);
                        file.WriteLine(ecB6[uID]);
                        file.WriteLine(ecB7[uID]);
                        file.WriteLine(ecB8[uID]);
                        file.WriteLine(ecB9[uID]);
                        file.WriteLine(ecB10[uID]);

                        file.WriteLine("SIPHON");

                        file.WriteLine(ecSBT[uID]);
                        file.WriteLine(ecSB[uID]);
                        file.WriteLine(ecSB1[uID]);
                        file.WriteLine(ecSB2[uID]);

                        file.WriteLine("ADDITIVE RANDOM");

                        file.WriteLine(ecART[uID]);
                        file.WriteLine(ecAR[uID]);
                        file.WriteLine(ecAR1[uID]);
                        file.WriteLine(ecAR2[uID]);

                        file.WriteLine("GAMBLER");

                        file.WriteLine(ecGBT[uID]);
                        file.WriteLine(ecGB[uID]);
                        file.WriteLine(ecGB1[uID]);
                        file.WriteLine(ecGB2[uID]);
                        file.WriteLine(ecGB3[uID]);

                        // ALTERNATE INFO
                        file.WriteLine("INVENTORY");
                        
                        file.WriteLine(ITEM_EC00[uID]);
                        file.WriteLine(ITEM_EC01[uID]);
                        file.WriteLine(ITEM_EC02[uID]);
                        file.WriteLine(ITEM_EC03[uID]);
                        file.WriteLine(ITEM_EC04[uID]);
                        file.WriteLine(ITEM_EC05[uID]);
                        file.WriteLine(ITEM_EC06[uID]);
                        file.WriteLine(ITEM_EC07[uID]);
                        file.WriteLine(ITEM_EC08[uID]);
                    }
                }
            }
            #endregion

            // EMOTION CALCULATION
            if (serverList.Contains(serverId))
            {
                a = serverList.IndexOf(serverId);
                if (emoteState.ElementAt(a) < TH_HAPPY && emoteState.ElementAt(a) > TH_SAD)
                {
                    emoteStateCalc[a] = 0;
                }
                if (emoteState.ElementAt(a) >= TH_HAPPY)
                {
                    emoteStateCalc[a] = 1;
                }
                if (emoteState.ElementAt(a) >= TH_GLAD)
                {
                    emoteStateCalc[a] = 2;
                }
                if (emoteState.ElementAt(a) >= TH_ECSTATIC)
                {
                    emoteStateCalc[a] = 3;
                }
                if (emoteState.ElementAt(a) <= TH_SAD)
                {
                    emoteStateCalc[a] = -1;
                }
                if (emoteState.ElementAt(a) <= TH_SORROW)
                {
                    emoteStateCalc[a] = -2;
                }
                if (emoteState.ElementAt(a) <= TH_DEPRESSED)
                {
                    emoteStateCalc[a] = -3;
                }
                if (emoteState.ElementAt(a) <= TH_TOLEAVE)
                {
                    if (!outOfRoom[a])
                    {
                        outOfRoom[a] = true;
                        await e.Channel.SendMessageAsync(emoteJessLeaves);
                    }
                    emoteStateCalc[a] = -4;
                    leftServers.Add((e.Channel as SocketGuildChannel).Guild.Id);
                    if (emoteStateCalc[a] == -4 && emoteStateTimer[a] < 120)
                    {
                        emoteStateTimer[a]++;
                    }
                    else if (emoteStateCalc[a] == -4 && emoteStateTimer[a] == 120)
                    {
                        emoteState[a] = -5;
                        emoteStateTimer[a] = 0;
                        emoteStateCalc[a] = 0;
                        await e.Channel.SendMessageAsync(emoteJessRecover);
                    }
                }
            }

            // DATE AND RANDOM
            string timeGrab = DateTime.Now.ToString("h:mm:ss.fff tt");
            rand_num = new Random();

            // JESS IS CALLED BY USER
            string[] callIN = new string[50000];
            if (e.Content.Contains($"<@!{jess.CurrentUser.Id.ToString()}>"))
            {
                if (emoteStateCalc[a] != -4)
                {
                    if (emoteStateCalc[a] >= 0)
                    {
                        callIN = call_response;
                    }
                    if (emoteStateCalc[a] <= -1)
                    {
                        callIN = call_rSAD;
                    }
                    if (emoteStateCalc[a] <= -3)
                    {
                        callIN = call_rDPR;
                    }
                    int call_resp_num = rand_num.Next(callIN.Length);
                    string call_response_next = callIN[call_resp_num];
                    await e.Channel.SendMessageAsync(call_response_next);
                    leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                }
                else
                {
                    await e.Channel.SendMessageAsync(moodLEFT);
                }
            }

            // ANNOY USERS
            /*if (e.Author.Id == 248558529336705026)
            {
                int annoy_user_num = rand_num.Next(annoy_user.Length);
                if (annoy_user_num >= 15)
                {
                    await e.Channel.SendMessageAsync("no u");
                    leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                }
                if (annoy_user_num <= 5)
                {
                    await e.Author.SendMessageAsync("no u");
                    leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                }
            }*/

            // COMMAND HANDLING
            string command = e.Content;
            if (command.ToUpper().StartsWith(cmdPrefix))
            {
                string[] emoteIN = new string[50000];
                string[] cmdArgs;
                command = command.Remove(0, cmdPrefix.Length);
                cmdArgs = command.Split(' ');
                switch (cmdArgs[0])
                {
                    /* case "addServer":
                        if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                        {
                            string[] loadData = File.ReadAllLines("serverinfo.txt");
                            for (int i = 0; i < loadData.Length; i += 7)
                            {
                                if (i + 6 < loadData.Length)
                                {
                                    welcomeChannels.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 1]));
                                    marionettes.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 2]));
                                    bindChannels.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 3]));
                                    musicChannels.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 4]));
                                    baseRoles.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 5]));
                                    peefChannels.Add(ulong.Parse(loadData[i]), ulong.Parse(loadData[i + 6]));
                                }
                            }

                            string[] saveData = File.ReadAllLines("serverinfo.txt");
                            for (int i = 0; i < saveData.Length; i += 7)
                            {
                                if (i + 6 < saveData.Length)
                                {
                                    File.AppendAllText("serverinfo.txt", $"{(e.Channel as SocketGuildChannel).Guild.Id}");
                                }
                            }
                        }
                        break; */
                    case "prefix": // CHANGE PREFIX
                        if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                        {
                            cmdPrefix = "";
                            for (int i = 1; i < cmdArgs.Length; i++) cmdPrefix += cmdArgs[i] + " ";
                            cmdPrefix = cmdPrefix.Remove(cmdPrefix.Length - 1, 1);
                            await e.Channel.SendMessageAsync($"Prefix edited. Use {cmdPrefix} to tell me to perform tasks from here on, {(e.Author as SocketGuildUser).Nickname}");
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync($"Sorry, but I'm afraid you have to be an administrator to do that... {(e.Author as SocketGuildUser).Nickname}");
                        }
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "bind": // BIND BOT TO CHANNEL FOR SAY
                        /* OLD VERSION
                        if (e.Channel == cfix(marionettes[serverId]))
                        {
                            if (e.MentionedChannels.Count > 0)
                            {
                                bindChannels[serverId] = e.MentionedChannels.First().Id;
                                await e.Channel.SendMessageAsync($"Alright, I'll speak in {cfix(bindChannels[serverId]).Name}, {e.Author.Mention}!");
                            }
                            else
                            {
                                await e.Channel.SendMessageAsync($"Please tell me where to speak, {e.Author.Mention}.");
                            }
                        }
                        else
                        {
                            await cfix(marionettes[serverId]).SendMessageAsync($"{cmdPrefix}bind is something I can't do, since it was sent from {e.Channel.Name} by {e.Author.Mention}");
                        } */

                        // NEW VERSION
                        if (e.MentionedChannels.Count > 0)
                        {
                            bindChannels[serverId] = e.MentionedChannels.First().Id;
                            await e.Channel.SendMessageAsync($"Alright, I'll speak in {cfix(bindChannels[serverId]).Name}, {e.Author.Mention}!");
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync($"Please tell me where to speak, {e.Author.Mention}.");
                        }

                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        if (command.Contains("&save"))
                        {
                            File.Delete("serverinfo.txt");
                            StreamWriter rewriteFile = File.CreateText("serverinfo.txt");
                            foreach (ulong x in welcomeChannels.Keys)
                            {
                                using (rewriteFile)
                                {
                                    rewriteFile.WriteLine(x);
                                    rewriteFile.WriteLine(jess.GetGuild(x).Name.ToString());
                                    rewriteFile.WriteLine(welcomeChannels[x]);
                                    rewriteFile.WriteLine(marionettes[x]);
                                    rewriteFile.WriteLine(bindChannels[x]);
                                    rewriteFile.WriteLine(musicChannels[x]);
                                    rewriteFile.WriteLine(baseRoles[x]);
                                    rewriteFile.WriteLine(peefChannels[x]);
                                    rewriteFile.WriteLine(modChannels[x]);
                                }
                            }
                        }
                        break;
                    case "saylock": // LOCK SAY COMMAND TO UAUTH USERS
                        //OLD: if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                        if (Array.IndexOf(uAuth, e.Author.Id) != -1)
                        {
                            if (saylocked == true)
                            {
                                saylocked = false;
                                await e.Channel.SendMessageAsync("Yeah, sure! I can say some things for you!");
                            }
                            else if (saylocked == false)
                            {
                                saylocked = true;
                                await e.Channel.SendMessageAsync("I'm afraid I can't talk for you at the moment... sorry about that.");
                            }
                        }
                        if (!(Array.IndexOf(uAuth, e.Author.Id) != -1))
                        {
                            await e.Channel.SendMessageAsync("You're not allowed to lock/unlock functions, my apologies...");
                        }
                        break;
                    case "say": // JESSICA MAY SPEAK WITH THIS
                        if (saylocked == false)
                        {
                            await cfix(bindChannels[serverId]).SendMessageAsync(command.Remove(0, 4));
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        if (saylocked == true)
                        {
                            if (Array.IndexOf(uAuth, e.Author.Id) != -1)
                            {
                                await cfix(bindChannels[serverId]).SendMessageAsync(command.Remove(0, 4));
                                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                            }
                            if (!(Array.IndexOf(uAuth, e.Author.Id) != -1))
                            {
                                await e.Channel.SendMessageAsync("Sorry, you're not allowed to do that...");
                            }
                        }
                        break;
                    case "version": // RETURN VERSION
                        await e.Channel.SendMessageAsync($"{e.Author.Mention} - Jessica's bot code is at {versNum}. \n" + $"This update {versUpdateInfo}");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "tap": // TAP JESSICA (ATTENTION)
                        if (emoteStateCalc[a] != -4)
                        {
                            if (emoteStateCalc[a] >= 0)
                            {
                                emoteIN = tap_response;
                            }
                            if (emoteStateCalc[a] <= -1)
                            {
                                emoteIN = tap_rSAD;
                            }
                            if (emoteStateCalc[a] <= -3)
                            {
                                emoteIN = tap_rDPR;
                                emoteState[a] -= 1;
                            }
                            int tap_resp_num = rand_num.Next(emoteIN.Length);
                            string tap_response_next = emoteIN[tap_resp_num];
                            await e.Channel.SendMessageAsync(tap_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync(moodLEFT);
                        }
                        break;
                    case "poke": // POKE JESSICA (ANNOY)
                        if (emoteStateCalc[a] != -4)
                        {
                            if (emoteStateCalc[a] == 0)
                            {
                                emoteIN = poke_response;
                                emoteState[a] -= 1;
                            }
                            if (emoteStateCalc[a] == 1)
                            {
                                emoteIN = poke_rHPY;
                                emoteState[a] -= 1;
                            }
                            if (emoteStateCalc[a] == 2)
                            {
                                emoteIN = poke_rGLD;
                                emoteState[a] -= 1;
                            }
                            if (emoteStateCalc[a] == 3)
                            {
                                emoteIN = poke_rECS;
                                emoteState[a] -= 1;
                            }
                            if (emoteStateCalc[a] == -1)
                            {
                                emoteIN = poke_rSAD;
                                emoteState[a] -= 2;
                            }
                            if (emoteStateCalc[a] == -2)
                            {
                                emoteIN = poke_rSRW;
                                emoteState[a] -= 3;
                            }
                            if (emoteStateCalc[a] == -3)
                            {
                                emoteIN = poke_rDPR;
                                emoteState[a] -= 4;
                            }
                            int poke_resp_num = rand_num.Next(emoteIN.Length);
                            string poke_response_next = emoteIN[poke_resp_num];
                            await e.Channel.SendMessageAsync(poke_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync(moodLEFT);
                        }
                        break;
                    case "pat": // PAT JESSICA ON HEAD (COMFORTS)
                        if (emoteStateCalc[a] != -4)
                        {
                            if (emoteStateCalc[a] == 0)
                            {
                                emoteIN = pat_response;
                                emoteState[a] += 1;
                            }
                            if (emoteStateCalc[a] == 1)
                            {
                                emoteIN = pat_rHPY;
                                emoteState[a] += 2;
                            }
                            if (emoteStateCalc[a] == 2)
                            {
                                emoteIN = pat_rHPY;
                                emoteState[a] += 3;
                            }
                            if (emoteStateCalc[a] == 3)
                            {
                                emoteIN = pat_rHPY;
                                emoteState[a] += 5;
                                balanceOut[e.Author.Id] += 5;
                            }
                            if (emoteStateCalc[a] == -1)
                            {
                                emoteIN = pat_rSAD;
                                emoteState[a] += 1;
                            }
                            if (emoteStateCalc[a] == -2)
                            {
                                emoteIN = pat_rSRW;
                                emoteState[a] += 2;
                            }
                            if (emoteStateCalc[a] == -3)
                            {
                                emoteIN = pat_rDPR;
                                emoteState[a] += 3;
                            }
                            int pat_resp_num = rand_num.Next(emoteIN.Length);
                            string pat_response_next = emoteIN[pat_resp_num];
                            await e.Channel.SendMessageAsync(pat_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync(moodLEFT);
                        }
                        break;
                    case "pet": // PET JESSICA'S HAIR
                        int c = rand_num.Next(Math.Abs(emoteStateCalc[a]) + 1);
                        int d = rand_num.Next(3);
                        int k = (c * (Math.Sign(emoteStateCalc[a]))) + d + 1;
                        if (emoteStateCalc[a] != -4)
                        {
                            if (emoteStateCalc[a] == 0)
                            {
                                emoteIN = pet_response;
                                emoteState[a] += k;
                            }
                            if (emoteStateCalc[a] == 1)
                            {
                                emoteIN = pet_rHPY;
                                emoteState[a] += k;
                            }
                            if (emoteStateCalc[a] == 2)
                            {
                                emoteIN = pet_rGLD;
                                emoteState[a] += k;
                            }
                            if (emoteStateCalc[a] == 3)
                            {
                                emoteIN = pet_rECS;
                                emoteState[a] += k + d;
                            }
                            if (emoteStateCalc[a] == -1)
                            {
                                emoteIN = pet_rSAD;
                                emoteState[a] += k;
                            }
                            if (emoteStateCalc[a] == -2)
                            {
                                emoteIN = pet_rSRW;
                                emoteState[a] += k - 1;
                                if (d == 2)
                                {
                                    emoteState[a] += 1;
                                }
                                if (k == 0)
                                {
                                    emoteState[a] -= d;
                                }
                            }
                            if (emoteStateCalc[a] == -3)
                            {
                                emoteIN = pet_rDPR;
                                emoteState[a] -= k - 2;
                                if (d == 2)
                                {
                                    emoteState[a] += 2;
                                }
                                if (k <= 0)
                                {
                                    emoteState[a] -= d * 2;
                                }
                            }
                            int pet_resp_num = rand_num.Next(emoteIN.Length);
                            string pet_response_next = emoteIN[pet_resp_num];
                            await e.Channel.SendMessageAsync(pet_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync(moodLEFT);
                        }
                        break;
                    case "calm": // CALM JESSICA DOWN
                        if (emoteStateCalc[a] >= 0)
                        {
                            emoteIN = calm_noeffect;
                            int calm_resp_num = rand_num.Next(emoteIN.Length);
                            string calm_response_next = emoteIN[calm_resp_num];
                            await e.Channel.SendMessageAsync(calm_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                            balanceOut[e.Author.Id] += 5;
                        }
                        if (emoteStateCalc[a] == -1)
                        {
                            emoteIN = calm_effective;
                            int calm_resp_num = rand_num.Next(emoteIN.Length);
                            string calm_response_next = emoteIN[calm_resp_num];
                            await e.Channel.SendMessageAsync(calm_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                            emoteState[a] += 10;
                            balanceOut[e.Author.Id] += 10;
                        }
                        if (emoteStateCalc[a] <= -2 && emoteStateCalc[a] > -4)
                        {
                            emoteIN = calm_effective;
                            int calm_resp_num = rand_num.Next(emoteIN.Length);
                            string calm_response_next = emoteIN[calm_resp_num];
                            await e.Channel.SendMessageAsync(calm_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                            emoteState[a] += 30;
                            balanceOut[e.Author.Id] += 25;
                        }
                        if (emoteStateCalc[a] <= -4)
                        {
                            emoteIN = calm_effective;
                            int calm_resp_num = rand_num.Next(emoteIN.Length);
                            string calm_response_next = emoteIN[calm_resp_num];
                            await e.Channel.SendMessageAsync(calm_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                            await e.Channel.SendMessageAsync(emoteJessRecover);
                            balanceOut[e.Author.Id] += 50;
                            emoteState[a] = 5;
                            emoteStateCalc[a] = 0;
                            emoteStateTimer[a] = 0;
                            break;
                        }
                        break;
                    case "compliment": // BE NICE TO JESSICA
                        if (emoteStateCalc[a] != -4)
                        {
                            if (emoteStateCalc[a] >= 2)
                            {
                                emoteIN = comp_effective;
                                emoteState[a] += 20;
                                balanceOut[e.Author.Id] += 25;
                            }
                            if (emoteStateCalc[a] >= 0)
                            {
                                emoteIN = comp_effective;
                                emoteState[a] += 8;
                                balanceOut[e.Author.Id] += 10;
                            }
                            if (emoteStateCalc[a] == -1)
                            {
                                emoteIN = comp_effective;
                                emoteState[a] += 3;
                                balanceOut[e.Author.Id] += 5;
                            }
                            if (emoteStateCalc[a] == -2)
                            {
                                emoteIN = comp_countereff;
                                emoteState[a] -= 8;
                            }
                            if (emoteStateCalc[a] <= -3)
                            {
                                emoteIN = comp_countereff;
                                emoteState[a] -= 20;
                            }
                            int cmplt_resp_num = rand_num.Next(emoteIN.Length);
                            string cmplt_response_next = emoteIN[cmplt_resp_num];
                            await e.Channel.SendMessageAsync(cmplt_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync(moodLEFT);
                        }
                        break;
                    case "mock": // MAKE FUN OF JESSICA
                        if (emoteStateCalc[a] != -4)
                        {
                            if (emoteStateCalc[a] == 3)
                            {
                                emoteIN = mock_ineffective;
                                int mock_resp_num = rand_num.Next(emoteIN.Length);
                                string mock_response_next = emoteIN[mock_resp_num];
                                await e.Channel.SendMessageAsync(mock_response_next);
                                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                                emoteState[a] -= 5;
                            }
                            if (emoteStateCalc[a] == 2)
                            {
                                emoteIN = mock_effective;
                                int mock_resp_num = rand_num.Next(emoteIN.Length);
                                string mock_response_next = emoteIN[mock_resp_num];
                                await e.Channel.SendMessageAsync(mock_response_next);
                                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                                emoteState[a] -= 10;
                            }
                            if (emoteStateCalc[a] == 1)
                            {
                                emoteIN = mock_effective;
                                int mock_resp_num = rand_num.Next(emoteIN.Length);
                                string mock_response_next = emoteIN[mock_resp_num];
                                await e.Channel.SendMessageAsync(mock_response_next);
                                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                                emoteState[a] -= 15;
                            }
                            if (emoteStateCalc[a] == 0)
                            {
                                emoteIN = mock_effective;
                                int mock_resp_num = rand_num.Next(emoteIN.Length);
                                string mock_response_next = emoteIN[mock_resp_num];
                                await e.Channel.SendMessageAsync(mock_response_next);
                                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                                emoteState[a] -= 20;
                            }
                            if (emoteStateCalc[a] == -1)
                            {
                                emoteIN = mock_moreeffective;
                                int mock_resp_num = rand_num.Next(emoteIN.Length);
                                string mock_response_next = emoteIN[mock_resp_num];
                                await e.Channel.SendMessageAsync(mock_response_next);
                                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                                emoteState[a] -= 40;
                            }
                            if (emoteStateCalc[a] == -2)
                            {
                                await e.Channel.SendMessageAsync(mock_toDepressed);
                                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                                emoteState[a] = -155;
                            }
                            if (emoteStateCalc[a] == -3)
                            {
                                await e.Channel.SendMessageAsync(mock_forceLeave);
                                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                                emoteState[a] = -550;
                                outOfRoom[a] = true;
                                emoteStateTimer[a] = -60;
                            }
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync(moodLEFT + $"\nBesides, why would you want to do this to someone who's already crying, *you sick, demented bastard?*");
                            emoteStateTimer[a] -= 20;
                        }
                        break;
                    case "insult": // INSULT JESSICA
                        if (emoteStateCalc[a] != -4)
                        {
                            int signRand = Math.Sign(rand_num.Next(11) - 5);
                            int insultFails = signRand * rand_num.Next(3);
                            if (emoteStateCalc[a] == 0)
                            {
                                emoteIN = insult_ineffective;
                                emoteState[a] -= 3;
                            }
                            if (emoteStateCalc[a] == 1)
                            {
                                emoteIN = insult_ineffective;
                                emoteState[a] -= 2;
                            }
                            if (emoteStateCalc[a] == 2)
                            {
                                emoteIN = insult_ineffective;
                                emoteState[a] -= 1;
                            }
                            if (emoteStateCalc[a] == 3)
                            {
                                emoteIN = insult_fail;
                                emoteState[a] += insultFails;
                            }
                            if (emoteStateCalc[a] == -1)
                            {
                                emoteIN = insult_effective;
                                emoteState[a] -= 8;
                            }
                            if (emoteStateCalc[a] == -2)
                            {
                                emoteIN = insult_effective;
                                emoteState[a] -= 14;
                            }
                            if (emoteStateCalc[a] == -3)
                            {
                                emoteIN = mock_moreeffective;
                                emoteState[a] -= 20;
                            }
                            int inslt_resp_num = rand_num.Next(emoteIN.Length);
                            string inslt_response_next = emoteIN[inslt_resp_num];
                            await e.Channel.SendMessageAsync(inslt_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync(moodLEFT + $"\nBesides, why would you want to do this to someone who's already crying, *you sick, demented bastard?*");
                            emoteStateTimer[a] -= 10;
                        }
                        break;
                    case "mood": // GET JESSICA'S MOOD, WITH ASSOCIATED EMOTION IMAGE
                        string beginImgPath = $"Pictures/";
                        int eSSval = emoteState[a];
                        int thrshval;
                        if (emoteStateCalc[a] != -4)
                        {
                            if (emoteStateCalc[a] == 0)
                            {
                                thrshval = TH_HAPPY;
                                string emoteStateScore = $" | {eSSval}/{thrshval}";
                                await e.Channel.SendFileAsync($"{beginImgPath}mood/neut.png", (moodNEUT + emoteStateScore));
                            }
                            if (emoteStateCalc[a] == 1)
                            {
                                thrshval = TH_GLAD;
                                string emoteStateScore = $" | {eSSval}/{thrshval}";
                                await e.Channel.SendFileAsync($"{beginImgPath}mood/pos1.png", (moodPOS1 + emoteStateScore));
                            }
                            if (emoteStateCalc[a] == 2)
                            {
                                thrshval = TH_ECSTATIC;
                                string emoteStateScore = $" | {eSSval}/{thrshval}";
                                await e.Channel.SendFileAsync($"{beginImgPath}mood/pos2.png", (moodPOS2 + emoteStateScore));
                            }
                            if (emoteStateCalc[a] == 3)
                            {
                                thrshval = TH_ECSTATIC;
                                string emoteStateScore = $" | {eSSval}/{thrshval}";
                                await e.Channel.SendFileAsync($"{beginImgPath}mood/pos3.png", (moodPOS3 + emoteStateScore));
                            }
                            if (emoteStateCalc[a] == -1)
                            {
                                thrshval = TH_SAD;
                                string emoteStateScore = $" | {eSSval}/{thrshval}";
                                await e.Channel.SendFileAsync($"{beginImgPath}mood/neg1.png", (moodNEG1 + emoteStateScore));
                            }
                            if (emoteStateCalc[a] == -2)
                            {
                                thrshval = TH_SORROW;
                                string emoteStateScore = $" | {eSSval}/{thrshval}";
                                await e.Channel.SendFileAsync($"{beginImgPath}mood/neg2.png", (moodNEG2 + emoteStateScore));
                            }
                            if (emoteStateCalc[a] == -3)
                            {
                                thrshval = TH_DEPRESSED;
                                string emoteStateScore = $" | {eSSval}/{thrshval}";
                                await e.Channel.SendFileAsync($"{beginImgPath}mood/neg3.png", (moodNEG3 + emoteStateScore));
                            }
                        }
                        else
                        {
                            thrshval = TH_TOLEAVE;
                            string emoteStateScore = $" | {eSSval}/{thrshval}";
                            await e.Channel.SendFileAsync($"{beginImgPath}mood/neg4.png", (moodLEFT + emoteStateScore));
                        }
                        break;
                    case "moodreset": // RESETS JESSICA'S MOOD
                        if (Array.IndexOf(uAuth, e.Author.Id) != -1)
                        {
                            beginImgPath = $"Pictures/";

                            emoteState[a] = 0;
                            emoteStateCalc[a] = 0;
                            emoteStateTimer[a] = 0;
                            IEnumerable<IMessage> clearMessages = await e.Channel.GetMessagesAsync(1).FlattenAsync();
                            foreach (IMessage m in clearMessages) await m.DeleteAsync();

                            eSSval = emoteState[a];
                            thrshval = TH_HAPPY;
                            string emoteStateScore = $" | {eSSval}/{thrshval}";
                            await e.Channel.SendFileAsync($"{beginImgPath}mood/neut.png", ($"**Mood reset.** \n" + moodNEUT + emoteStateScore));
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync($"*You are not allowed to perform this task.*");
                        }
                        break;
                    case "twitter": // RETURNS PEEF'S TWITTER
                        await e.Channel.SendMessageAsync($"Here you go! https://twitter.com/peeftube");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "email_peef": // GETS PEEF'S EMAIL
                        await e.Channel.SendMessageAsync($"{e.Author.Mention}, peef's email address is peefTube@gmail.com.");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "youtube": // GETS THE PEEFTUBE YT CHAN
                        await e.Channel.SendMessageAsync($"I'm afraid I don't have this on hand at the moment.");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "join": // JOINS VC [FINISH ME]
                        // IMPLEMENT CHANNEL DATA COLLECTION (GRAB FROM BIND)
                        await e.Channel.SendMessageAsync($"I can't join voice channels yet.");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "play": // PLAY MUSIC IN VC [FINISH ME]
                        await e.Channel.SendMessageAsync($"I can't play music in voice channels yet.");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "time": // RETURN LOCAL TIME FROM PEEF'S COMPUTER
                        await e.Channel.SendMessageAsync($"It is currently, in PST time, {timeGrab}");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "msg_rmv": // REMOVE MESSAGES
                        if ((e.Author as SocketGuildUser).GuildPermissions.ManageMessages)
                        {
                            if (cmdArgs.Length == 1)
                            {
                                await e.Channel.SendMessageAsync($"I can't delete 'I don't know' messages!");
                                break;
                            }
                            int msgCount = int.Parse(cmdArgs[1]) + 1;
                            while (msgCount > 0)
                            {
                                IEnumerable<IMessage> clearMessages = await e.Channel.GetMessagesAsync(msgCount).FlattenAsync();
                                foreach (IMessage m in clearMessages) await m.DeleteAsync();
                                msgCount = Math.Max(0, msgCount - 500);
                            }
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync($"Apologies, but you're not allowed to do that...");
                        }
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "peef": // IMPERSONATE PEEF [UPDATE]
                        if (cgfix(e.Channel.Id).Guild.Id == 402720603846606858)
                        {
                            int peef_resp_num = rand_num.Next(peef_response.Length);
                            string peef_response_next = peef_response[peef_resp_num];
                            await e.Channel.SendMessageAsync(peef_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync("Sorry, this command is limited to PCR only. There is an invite link available via JR.help! You may also contact the creator of this bot, peef, using JR.dmpeef.");
                        }
                        break;
                    case "musicpost": // POST MUSIC [UPDATE]
                        IMessageChannel music_post = cfix(musicChannels[serverId]);
                        int musp_resp_num = rand_num.Next(musp_response.Length);
                        string musp_response_next = musp_response[musp_resp_num];
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        await music_post.SendMessageAsync($"{musp_response_next}" + $"\n" + "Enjoy!");
                        break;
                    case "shutdown": // TELL USERS JESSICA IS SHUTTING DOWN
                        if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                        {
                            await cfix(402721696605274112).SendMessageAsync($"I'm gonna go offline for a bit.");
                            await cfix(550395914364387370).SendMessageAsync($"I'm gonna go offline for a bit.");
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        break;
                    case "sd_r": // GIVE REASON FOR ABOVE [PLEASE COMBINE!]
                        if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                        {
                            await cfix(402721696605274112).SendMessageAsync("Reason: " + command.Remove(0, 5));
                            await cfix(550395914364387370).SendMessageAsync("Reason: " + command.Remove(0, 5));
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        break;
                    case "srvr_alert": // ALERT ALL USERS SOMETHING IS HAPPENING
                        if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                        {
                            await cfix(402721696605274112).SendMessageAsync(command.Remove(0, 11));
                            await cfix(550395914364387370).SendMessageAsync(command.Remove(0, 11));
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        break;
                    case "leaveserver": // LEAVE ROOM
                        if (!leftServers.Contains((e.Channel as SocketGuildChannel).Guild.Id))
                        {
                            leftServers.Add((e.Channel as SocketGuildChannel).Guild.Id);
                            await e.Channel.SendMessageAsync($"*leaves the room*");
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync($"*no response, Jessica has already left the room*");
                        }
                        break;
                    case "callback": // REENTER ROOM
                        int call_resp_num = rand_num.Next(call_response.Length);
                        string call_response_next = call_response[call_resp_num];

                        if (leftServers.Contains((e.Channel as SocketGuildChannel).Guild.Id))
                        {
                            await e.Channel.SendMessageAsync($"*enters the room* \n" + call_response_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync($"I'm right here! \n" + $"Anyways... " + call_response_next);
                        }
                        break;
                    case "mnm": // POST MEME
                        if (cgfix(e.Channel.Id).Guild.Id == 402720603846606858)
                        {
                            int mnm_num = rand_num.Next(memes_and_moments.Length);
                            string mnm_next = memes_and_moments[mnm_num];
                            await e.Channel.SendFileAsync($"Pictures/" + mnm_next);
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync("Sorry, this command is limited to PCR only. There is an invite link available via JR.help! You may also contact the creator of this bot, peef, using JR.dmpeef.");
                        }
                        break;
                    case "inv_frontier": // INVITE TO FRONTIER: DO NOT USE
                        await e.Channel.SendMessageAsync($"This Discord sadly no longer exists.");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "inv_pcr": // INVITE TO PCR
                        await e.Channel.SendMessageAsync($"https://discord.gg/USgKFeW");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "freemansmind": // FREEMAN'S MIND
                        if (cgfix(e.Channel.Id).Guild.Id == 402720603846606858)
                        {
                            await e.Channel.SendMessageAsync($"https://www.youtube.com/watch?v=bdfszx8y23Y");
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync("Sorry, this command is limited to PCR only. There is an invite link available via JR.help! You may also contact the creator of this bot, peef, using JR.dmpeef.");
                        }
                        break;
                    case "dnd_roll": // DND DIE ROLL [UPDATE]
                        int dnd_roll_num = rand_num.Next(dnd_roll.Length);
                        string dnd_roll_next = dnd_roll[dnd_roll_num];
                        await e.Channel.SendMessageAsync(dnd_roll_next); leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "stm_peef_frnds": // GET PEEF'S STEAM FRIENDS LIST
                        await e.Channel.SendMessageAsync($"http://steamcommunity.com/id/peef_profile_site_thing/friends/");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "saydm": // SAY BUT IN DMS
                        SocketUser usertomention = tfix(e.MentionedUsers.First().Id);
                        await usertomention.SendMessageAsync(command.Remove(0, e.MentionedUsers.First().Mention.Length + 6));
                        await e.Channel.SendMessageAsync($"{e.Author.Mention}, I sent them your message!");
                        break;
                    case "saydmID": // SAY BUT IN DMS (USES ID!)
                        SocketUser usertomention_ID = tfix(ulong.Parse(cmdArgs[1]));
                        await usertomention_ID.SendMessageAsync(command.Remove(0, cmdArgs[1].Length + 8));
                        await e.Channel.SendMessageAsync($"{e.Author.Mention}, I sent them your message!");
                        break;
                    case "warn": // WARN USER
                        if ((e.Author as SocketGuildUser).GuildPermissions.Administrator)
                        {
                            SocketUser usertowarn = e.MentionedUsers.First();
                            await e.Channel.SendMessageAsync($"{e.Author.Mention}, you warned {usertowarn.Mention}.");
                            await cfix(modChannels[serverId]).SendMessageAsync($"{e.Author.Mention} (usertag {e.Author.Username}#{e.Author.Discriminator}) warned user {usertowarn.Username}#{usertowarn.Discriminator} with message:" + command.Remove(0, e.MentionedUsers.First().Mention.Length + 5));
                            await cfix(peefChannels[serverId]).SendMessageAsync($"{usertowarn.Mention}, you have been warned by {e.Author.Mention}:\n" + command.Remove(0, e.MentionedUsers.First().Mention.Length + 5));
                            await usertowarn.SendMessageAsync(command.Remove(0, e.MentionedUsers.First().Mention.Length + 5) + $"\n**You have been warned.**");
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync($"I'm afraid only administrators can do that...");
                        }
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "ban": // BAN USER [IMPLEMENT]
                        await e.Channel.SendMessageAsync($"I can't do this yet...");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "dmpeef": // SAYDM BUT DIRECTLY TO PEEF
                        SocketUser peef = tfix(236738543387541507);
                        await peef.SendMessageAsync($"{e.Author.Mention} wanted to say something to you: " + command.Remove(0, 7));
                        await e.Channel.SendMessageAsync($"{e.Author.Mention}, I sent them your message!");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "shut": // SHUT YOUR PIEHOLE
                        await e.Channel.SendMessageAsync($"Hey, shut it!");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "hey": // KICK DOWN A DOOR YOU FOOL
                        if (cgfix(e.Channel.Id).Guild.Id == 402720603846606858)
                        {
                            await e.Channel.SendMessageAsync($"https://www.youtube.com/watch?v=Cb6F14AGrvI");
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync("Sorry, this command is limited to PCR only. There is an invite link available via JR.help! You may also contact the creator of this bot, peef, using JR.dmpeef.");
                        }
                        break;
                    case "immortalbirb": // THAT BIRD WILL NEVER DIE
                        if (cgfix(e.Channel.Id).Guild.Id == 402720603846606858)
                        {
                            await e.Channel.SendMessageAsync($"https://www.youtube.com/watch?v=z4rJe-cVfOI");
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync("Sorry, this command is limited to PCR only. There is an invite link available via JR.help! You may also contact the creator of this bot, peef, using JR.dmpeef.");
                        }
                        break;
                    case "cocainum": // TF2 SHENANIGANS
                        if (cgfix(e.Channel.Id).Guild.Id == 402720603846606858)
                        {
                            await e.Channel.SendMessageAsync($"https://youtu.be/weMpv14cW9U");
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync("Sorry, this command is limited to PCR only. There is an invite link available via JR.help! You may also contact the creator of this bot, peef, using JR.dmpeef.");
                        }
                        break;
                    case "srvrcrtdt": // WHEN WAS THE SERVER MADE
                        await e.Channel.SendMessageAsync($"This server was created: {(e.Author as SocketGuildUser).Guild.CreatedAt}");
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "srvr_ecreg": // REGISTER ENTIRE DISCORD WITH ECONOMY SYSTEM!
                        if ((e.Author as SocketGuildUser).GuildPermissions.Administrator || (Array.IndexOf(uAuth, e.Author.Id) != -1))
                        {
                            int iterator = 0;
                            int track_added = 0;

                            while (iterator < cgfix(e.Channel.Id).Guild.Users.Count)
                            {
                                ulong uCheck = cgfix(e.Channel.Id).Guild.Users.ElementAt(iterator).Id;

                                if (!balanceOut.ContainsKey(uCheck) && !tfix(uCheck).IsBot)
                                {
                                    balanceOut.Add(uCheck, 0);
                                    ecBT.Add(uCheck, 1.00);
                                    ecB1.Add(uCheck, false);
                                    ecB2.Add(uCheck, false);
                                    ecB3.Add(uCheck, false);
                                    ecB4.Add(uCheck, false);
                                    ecB5.Add(uCheck, false);
                                    ecB6.Add(uCheck, false);
                                    ecB7.Add(uCheck, false);
                                    ecB8.Add(uCheck, false);
                                    ecB9.Add(uCheck, false);
                                    ecB10.Add(uCheck, false);

                                    ecSBT.Add(uCheck, 1.00);
                                    ecSB.Add(uCheck, false);
                                    ecSB1.Add(uCheck, false);
                                    ecSB2.Add(uCheck, false);

                                    ecART.Add(uCheck, 1.00);
                                    ecAR.Add(uCheck, false);
                                    ecAR1.Add(uCheck, false);
                                    ecAR2.Add(uCheck, false);

                                    ecGBT.Add(uCheck, 1.00);
                                    ecGB.Add(uCheck, false);
                                    ecGB1.Add(uCheck, false);
                                    ecGB2.Add(uCheck, false);
                                    ecGB3.Add(uCheck, false);

                                    ITEM_EC00.Add(uCheck, 0);
                                    ITEM_EC01.Add(uCheck, 0);
                                    ITEM_EC02.Add(uCheck, 0);
                                    ITEM_EC03.Add(uCheck, 0);
                                    ITEM_EC04.Add(uCheck, 0);
                                    ITEM_EC05.Add(uCheck, 0);
                                    ITEM_EC06.Add(uCheck, 0);
                                    ITEM_EC07.Add(uCheck, 0);
                                    ITEM_EC08.Add(uCheck, 0);

                                    econTimeOut.Add(0);
                                    econUserBlock.Add(false);

                                    ++track_added;
                                }

                                ++iterator;
                            }
                            await e.Channel.SendMessageAsync($"{track_added} users were added to the economy database.");
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync($"You must be an administrator to add users to the database!");
                        }
                        break;
                    case "bal": // RETURN USER'S BALANCE FOR THE ECONOMY
                        if (cmdArgs.Length == 1)
                        {
                            await e.Channel.SendMessageAsync($"Your balance is: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[e.Author.Id]}. \nYou have a total boost of {ecBT[e.Author.Id]}x.");
                            leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        }
                        else if (cmdArgs.Length > 1)
                        {
                            ulong balCheckID = e.MentionedUsers.First().Id;
                            if (balCheckID == e.Author.Id)
                            {
                                await e.Channel.SendMessageAsync($"Your balance is: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[e.Author.Id]}. \nYou have a total boost of {ecBT[e.Author.Id]}x.");
                                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                            }
                            else
                            {
                                await e.Channel.SendMessageAsync($"Their balance is: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[balCheckID]}. \nThey have a total boost of {ecBT[balCheckID]}x.");
                                leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                            }
                        }
                        break;
                    case "store": // OPERATES STORE
                        const string doNotOwn = "You do not own this.";
                        const string doOwn = "You own this.";
                        const string notExist = "This is not yet available.";
                        string jb = new Emoji("<:jessbucks:561818526923620353>").ToString();

                        ulong cID = e.Author.Id;
                        long cBal = balanceOut[cID];
                        string[] thisCustomerOwns = new string[5001];

                        #region boostermessages
                        if (ecB1[cID]) { thisCustomerOwns[1] = doOwn; }
                        if (!ecB1[cID]) { thisCustomerOwns[1] = doNotOwn; }
                        if (ecB2[cID]) { thisCustomerOwns[2] = doOwn; }
                        if (!ecB2[cID]) { thisCustomerOwns[2] = doNotOwn; }
                        if (ecB3[cID]) { thisCustomerOwns[3] = doOwn; }
                        if (!ecB3[cID]) { thisCustomerOwns[3] = doNotOwn; }
                        if (ecB4[cID]) { thisCustomerOwns[4] = doOwn; }
                        if (!ecB4[cID]) { thisCustomerOwns[4] = doNotOwn; }
                        if (ecB5[cID]) { thisCustomerOwns[5] = doOwn; }
                        if (!ecB5[cID]) { thisCustomerOwns[5] = doNotOwn; }
                        if (ecB6[cID]) { thisCustomerOwns[6] = doOwn; }
                        if (!ecB6[cID]) { thisCustomerOwns[6] = doNotOwn; }
                        if (ecB7[cID]) { thisCustomerOwns[7] = doOwn; }
                        if (!ecB7[cID]) { thisCustomerOwns[7] = doNotOwn; }
                        if (ecB8[cID]) { thisCustomerOwns[8] = doOwn; }
                        if (!ecB8[cID]) { thisCustomerOwns[8] = doNotOwn; }
                        if (ecB9[cID]) { thisCustomerOwns[9] = doOwn; }
                        if (!ecB9[cID]) { thisCustomerOwns[9] = doNotOwn; }
                        if (ecB10[cID]) { thisCustomerOwns[10] = doOwn; }
                        if (!ecB10[cID]) { thisCustomerOwns[10] = doNotOwn; }
                        #endregion

                        #region siphonmessages
                        if (ecSB[cID]) { thisCustomerOwns[100] = doOwn; }
                        if (!ecSB[cID]) { thisCustomerOwns[100] = doNotOwn; }
                        if (ecSB1[cID]) { thisCustomerOwns[101] = doOwn; }
                        if (!ecSB1[cID]) { thisCustomerOwns[101] = doNotOwn; }
                        if (ecSB2[cID]) { thisCustomerOwns[102] = doOwn; }
                        if (!ecSB2[cID]) { thisCustomerOwns[102] = doNotOwn; }
                        #endregion

                        #region addrandmessages
                        if (ecAR[cID]) { thisCustomerOwns[200] = doOwn; }
                        if (!ecAR[cID]) { thisCustomerOwns[200] = doNotOwn; }
                        if (ecAR1[cID]) { thisCustomerOwns[201] = doOwn; }
                        if (!ecAR1[cID]) { thisCustomerOwns[201] = doNotOwn; }
                        if (ecAR2[cID]) { thisCustomerOwns[202] = doOwn; }
                        if (!ecAR2[cID]) { thisCustomerOwns[202] = doNotOwn; }
                        #endregion

                        #region gamblemessages
                        if (ecGB[cID]) { thisCustomerOwns[300] = doOwn; }
                        if (!ecGB[cID]) { thisCustomerOwns[300] = doNotOwn; }
                        if (ecGB1[cID]) { thisCustomerOwns[301] = doOwn; }
                        if (!ecGB1[cID]) { thisCustomerOwns[301] = doNotOwn; }
                        if (ecGB2[cID]) { thisCustomerOwns[302] = doOwn; }
                        if (!ecGB2[cID]) { thisCustomerOwns[302] = doNotOwn; }
                        if (ecGB3[cID]) { thisCustomerOwns[303] = doOwn; }
                        if (!ecGB3[cID]) { thisCustomerOwns[303] = doNotOwn; }
                        #endregion

                        string ecStoreMsg = ($"**CENTRAL STORE** \n" + $"Your balance is {jb}{balanceOut[e.Author.Id]}. \n");
                        string ecBuyMsg = ($"Use JR.buy # to buy an item!");
                        if (cmdArgs.Length == 1)
                        {
                            await e.Channel.SendMessageAsync($"{ecStoreMsg}"
                                + $"Page 1: Boosters \n"
                                + $"Page 2: Powerups \n"
                                + $"Page 3: Purchases \n"
                                + $"Page 4: Game Items, Page 1 \n"
                                + $"`Use JR.store # to access a part of the store!`");
                        }
                        else if (cmdArgs.Length >= 2)
                        {
                            int pageNum = int.Parse(cmdArgs[1]);
                            switch (pageNum)
                            {
                                case 1:
                                    await e.Channel.SendMessageAsync($"{ecStoreMsg}" +
                                    $"**BOOSTERS** \n" +
                                    $"**0001:** 1.25x Boost, {jb}1000 [{thisCustomerOwns[1]}] \n" +
                                    $"**0002:** 1.25x Boost, {jb}3500 [{thisCustomerOwns[2]}] \n" +
                                    $"**0003:** 1.50x Boost, {jb}5000 [{thisCustomerOwns[3]}] \n" +
                                    $"**0004:** 1.50x Boost, {jb}11500 [{thisCustomerOwns[4]}] \n" +
                                    $"**0005:** 1.75x Boost, {jb}22500 [{thisCustomerOwns[5]}] \n" +
                                    $"**0006:** 1.75x Boost, {jb}35000 [{thisCustomerOwns[6]}] \n" +
                                    $"**0007:** 2.00x Boost, {jb}50000 [{thisCustomerOwns[7]}] \n" +
                                    $"**0008:** 2.00x Boost, {jb}85000 [{thisCustomerOwns[8]}] \n" +
                                    $"**0009:** 2.25x Boost, {jb}115000 [{thisCustomerOwns[9]}] \n" +
                                    $"**0010:** 2.25x Boost, {jb}165000 [{thisCustomerOwns[10]}] \n" +
                                    $"{ecBuyMsg}");
                                    break;
                                case 2:
                                    await e.Channel.SendMessageAsync($"{ecStoreMsg}" +
                                        $"**POWERUPS** \n" +
                                        $"**0100:** Siphon, {jb}95000 [{thisCustomerOwns[100]}] | **`25% of every message's income is also given to you.`** \n" +
                                        $"**0101:** Siphon 1.25x Boost, {jb}125000 [{thisCustomerOwns[101]}] \n" +
                                        $"**0102:** Siphon 1.50x Boost, {jb}185000 [{thisCustomerOwns[102]}] \n" +
                                        $"**0200:** Additive Random, {jb}25000 [{thisCustomerOwns[200]}] | **`50/50 chance for extra income.`** \n" +
                                        $"**0201:** AR 1.25x Boost, {jb}125000 [{thisCustomerOwns[201]}] \n" +
                                        $"**0202:** AR 1.50x Boost, {jb}225000 [{thisCustomerOwns[202]}] \n" +
                                        $"**0300:** Gambler Random, {jb}25000 [{thisCustomerOwns[300]}] | **`25% to lose a lot of money, 5% to make even more.`** \n" +
                                        $"**0301:** Gambler 10% Pos. Chance Increase, {jb}125000 [{thisCustomerOwns[301]}] \n" +
                                        $"**0302:** Gambler 2.00x Boost, {jb}275000 [{thisCustomerOwns[302]}] \n" +
                                        $"**0303:** Gambler 10% Pos. Chance Increase, {jb}325000 [{thisCustomerOwns[303]}] \n" +
                                        $"{ecBuyMsg} \n");
                                    break;
                                case 3:
                                    await e.Channel.SendMessageAsync($"{ecStoreMsg}" +
                                        $"**PURCHASES** \n" +
                                        $"**1000:** Commission {jess.GetUser(236738543387541507).Username}#{jess.GetUser(236738543387541507).Discriminator}, {jb}125000000 | **~~`ONLY 2 PER MONTH`~~`| MUST PAY, SORRY :(`** \n" +
                                        $"**1001:** Secret Purchase, {jb}50000 \n" +
                                        $"{ecBuyMsg}");
                                    break;
                                case 4:
                                    await e.Channel.SendMessageAsync($"{ecStoreMsg}" +
                                        $"**GAME ITEMS** \n" +
                                        $"**1100:** Fishing Rod {jb}20 | **`Very cheap, but useful for food. Occasionally produces rare items.`** \n" +
                                        $"Owned: {ITEM_EC00[cID]}\n" +
                                        $"{ecBuyMsg}");
                                    break;
                                default:
                                    await e.Channel.SendMessageAsync($"{ecStoreMsg}"
                                    + $"Page 1: Boosters \n"
                                    + $"Page 2: Powerups \n"
                                    + $"Page 3: Purchases \n"
                                    + $"Page 4: Game Items, Page 1 \n"
                                    + $"`Use JR.store # to access a part of the store!`");
                                    break;
                            }
                        }
                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "buy": // OPERATES STORE PURCHASES
                        jb = new Emoji("<:jessbucks:561818526923620353>").ToString();
                        cID = e.Author.Id;
                        cBal = balanceOut[cID];
                        ecBuyMsg = ($"Use JR.buy # to buy an item!");

                        ulong numToBuy = 1;

                        if (cmdArgs.Length == 1)
                        {
                            await e.Channel.SendMessageAsync(ecBuyMsg);
                        }
                        else if (cmdArgs.Length > 1)
                        {
                            int purchaseNum = 0;

                            if (cmdArgs[1].ToUpper().Contains("EC"))
                            {
                                switch (cmdArgs[1].ToUpper())
                                {
                                    case "EC00":
                                        purchaseNum = 1100;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                purchaseNum = int.Parse(cmdArgs[1]);
                            }
                            
                            if (cmdArgs.Length > 2 && cmdArgs[2].ToUpper().Contains("X"))
                            {
                                numToBuy = ulong.Parse(cmdArgs[2].ToUpper().Replace("X", ""));
                            }

                            switch (purchaseNum)
                            {
                                case 1:
                                    #region code
                                    if (!ecB1[cID])
                                    {
                                        if (cBal >= 1000)
                                        {
                                            ecB1[cID] = true;
                                            balanceOut[cID] -= 1000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}1000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecB1[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 2:
                                    #region code
                                    if (!ecB2[cID])
                                    {
                                        if (cBal >= 3500)
                                        {
                                            ecB2[cID] = true;
                                            balanceOut[cID] -= 3500;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}3500 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecB2[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 3:
                                    #region code
                                    if (!ecB3[cID])
                                    {
                                        if (cBal >= 5000)
                                        {
                                            ecB3[cID] = true;
                                            balanceOut[cID] -= 5000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}5000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecB3[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 4:
                                    #region code
                                    if (!ecB4[cID])
                                    {
                                        if (cBal >= 11500)
                                        {
                                            ecB4[cID] = true;
                                            balanceOut[cID] -= 11500;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}11500 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecB4[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 5:
                                    #region code
                                    if (!ecB5[cID])
                                    {
                                        if (cBal >= 22500)
                                        {
                                            ecB5[cID] = true;
                                            balanceOut[cID] -= 22500;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}22500 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecB5[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 6:
                                    #region code
                                    if (!ecB6[cID])
                                    {
                                        if (cBal >= 35000)
                                        {
                                            ecB6[cID] = true;
                                            balanceOut[cID] -= 35000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}35000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecB6[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 7:
                                    #region code
                                    if (!ecB7[cID])
                                    {
                                        if (cBal >= 50000)
                                        {
                                            ecB7[cID] = true;
                                            balanceOut[cID] -= 50000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}50000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecB7[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 8:
                                    #region code
                                    if (!ecB8[cID])
                                    {
                                        if (cBal >= 85000)
                                        {
                                            ecB8[cID] = true;
                                            balanceOut[cID] -= 85000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}85000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecB8[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 9:
                                    #region code
                                    if (!ecB9[cID])
                                    {
                                        if (cBal >= 115000)
                                        {
                                            ecB9[cID] = true;
                                            balanceOut[cID] -= 115000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}115000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecB9[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 10:
                                    #region code
                                    if (!ecB10[cID])
                                    {
                                        if (cBal >= 165000)
                                        {
                                            ecB10[cID] = true;
                                            balanceOut[cID] -= 165000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}165000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecB10[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 100:
                                    #region code
                                    if (!ecSB[cID])
                                    {
                                        if (cBal >= 95000)
                                        {
                                            ecSB[cID] = true;
                                            balanceOut[cID] -= 95000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}95000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecSB[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 101:
                                    #region code
                                    if (!ecSB1[cID])
                                    {
                                        if (cBal >= 125000)
                                        {
                                            ecSB1[cID] = true;
                                            balanceOut[cID] -= 125000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}125000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecSB1[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 102:
                                    #region code
                                    if (!ecSB2[cID])
                                    {
                                        if (cBal >= 185000)
                                        {
                                            ecSB2[cID] = true;
                                            balanceOut[cID] -= 185000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}185000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecSB2[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 200:
                                    #region code
                                    if (!ecAR[cID])
                                    {
                                        if (cBal >= 25000)
                                        {
                                            ecAR[cID] = true;
                                            balanceOut[cID] -= 25000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}25000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecAR[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 201:
                                    #region code
                                    if (!ecAR1[cID])
                                    {
                                        if (cBal >= 125000)
                                        {
                                            ecAR1[cID] = true;
                                            balanceOut[cID] -= 125000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}125000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecAR1[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 202:
                                    #region code
                                    if (!ecAR2[cID])
                                    {
                                        if (cBal >= 225000)
                                        {
                                            ecAR2[cID] = true;
                                            balanceOut[cID] -= 225000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}225000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecAR2[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 300:
                                    #region code
                                    if (!ecGB[cID])
                                    {
                                        if (cBal >= 25000)
                                        {
                                            ecGB[cID] = true;
                                            balanceOut[cID] -= 25000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}25000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecGB[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 301:
                                    #region code
                                    if (!ecGB1[cID])
                                    {
                                        if (cBal >= 125000)
                                        {
                                            ecGB1[cID] = true;
                                            balanceOut[cID] -= 125000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}125000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecGB1[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 302:
                                    #region code
                                    if (!ecGB2[cID])
                                    {
                                        if (cBal >= 275000)
                                        {
                                            ecGB2[cID] = true;
                                            balanceOut[cID] -= 275000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}275000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecGB2[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 303:
                                    #region code
                                    if (!ecGB3[cID])
                                    {
                                        if (cBal >= 325000)
                                        {
                                            ecGB3[cID] = true;
                                            balanceOut[cID] -= 325000;
                                            await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}325000 was removed from your account.");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                        }
                                        break;
                                    }
                                    if (ecGB3[cID])
                                    {
                                        await e.Channel.SendMessageAsync($"You already own this!");
                                    }
                                    break;
                                #endregion
                                case 1000:
                                    if (cBal >= 125000000)
                                    {
                                        balanceOut[cID] -= 125000000;
                                        await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}125000000 was removed from your account. Expect a DM from {jess.GetUser(236738543387541507).Username}#{jess.GetUser(236738543387541507).Discriminator} regarding your commission.");
                                        await jess.GetUser(236738543387541507).SendMessageAsync($"{e.Author.Username}#{e.Author.Discriminator} has purchased a commission!");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                    }
                                    break;
                                case 1001:
                                    if (cBal >= 50000)
                                    {
                                        balanceOut[cID] -= 50000;
                                        await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}50000 was removed from your account.");
                                        await e.Author.SendMessageAsync($"Here, take this: ||https://www.youtube.com/watch?v=1Fx7u57KIrE||");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                    }
                                    break;
                                case 1100:
                                    #region code
                                    if (cBal >= (long)(20 * numToBuy))
                                    {
                                        ITEM_EC00[cID] += (1 * numToBuy);
                                        balanceOut[cID] -= (long)(20 * numToBuy);
                                        await e.Channel.SendMessageAsync($"Your purchase was successful. {jb}{20 * numToBuy} was removed from your account. \n"
                                            + $"{1 * numToBuy} fishing rod(s) was/were added to your inventory, giving you a total of {ITEM_EC00[cID]}.");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessageAsync($"Your purchase failed, insufficient funds.");
                                    }
                                    break;
                                #endregion
                                default:
                                    await e.Channel.SendMessageAsync($"Your purchase failed. This item is unavailable or does not exist on the market. Try again later.");
                                    break;
                            }
                        }

                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "sell": // SELL ITEMS
                        jb = new Emoji("<:jessbucks:561818526923620353>").ToString();
                        string input0 = "";

                        ulong numToSell = 1;

                        if (cmdArgs.Length == 1)
                        {
                            await e.Channel.SendMessageAsync("No item code provided. Aborting. \n" +
                                "**Please ensure you provide an item code in format `EC##`.**");
                            break;
                        }
                        else if (cmdArgs.Length >= 2)
                        {
                            input0 = cmdArgs[1].ToString().ToUpper();
                            
                            if (cmdArgs.Length >= 3 && cmdArgs[2].ToUpper().Contains("X"))
                            {
                                numToSell = ulong.Parse(cmdArgs[2].ToUpper().Replace("X", ""));
                            }
                        }

                        ulong userInfTrack = 0;
                        bool isSafeToRun = true;

                        switch (input0)
                        {
                            case "EC00":
                                userInfTrack = ITEM_EC00[e.Author.Id];
                                break;
                            case "EC01":
                                userInfTrack = ITEM_EC01[e.Author.Id];
                                break;
                            case "EC02":
                                userInfTrack = ITEM_EC02[e.Author.Id];
                                break;
                            case "EC03":
                                userInfTrack = ITEM_EC03[e.Author.Id];
                                break;
                            case "EC04":
                                userInfTrack = ITEM_EC04[e.Author.Id];
                                break;
                            case "EC05":
                                userInfTrack = ITEM_EC05[e.Author.Id];
                                break;
                            case "EC06":
                                userInfTrack = ITEM_EC06[e.Author.Id];
                                break;
                            case "EC07":
                                userInfTrack = ITEM_EC07[e.Author.Id];
                                break;
                            case "EC08":
                                userInfTrack = ITEM_EC08[e.Author.Id];
                                break;
                        }

                        for (ulong i = 0; i < numToSell; ++i)
                        {
                            if (!((userInfTrack - 1) < 0) && !((userInfTrack - 1) >= ulong.MaxValue) && isSafeToRun)
                            {
                                userInfTrack -= 1;
                            }
                            else
                            {
                                isSafeToRun = false;
                                break;
                            }
                        }

                        switch (input0)
                        {
                            case "EC00":
                                if (ITEM_EC00[e.Author.Id] != 0 && !((ITEM_EC00[e.Author.Id] - numToSell) < 0) && !((ITEM_EC00[e.Author.Id] - numToSell) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC00[e.Author.Id] -= (1 * numToSell);
                                    balanceOut[e.Author.Id] += (long)(10 * numToSell);
                                    await e.Channel.SendMessageAsync($"{numToSell} fishing rod(s) sold for {jb}{10 * numToSell}. Your balance is now {jb}{balanceOut[e.Author.Id]}");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough fishing rods!");
                                    break;
                                }
                            case "EC01":
                                if (ITEM_EC01[e.Author.Id] != 0 && !((ITEM_EC01[e.Author.Id] - numToSell) < 0) && !((ITEM_EC01[e.Author.Id] - numToSell) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC01[e.Author.Id] -= (1 * numToSell);
                                    balanceOut[e.Author.Id] += (long)(8 * numToSell);
                                    await e.Channel.SendMessageAsync($"{numToSell} small fish sold for {jb}{8 * numToSell}. Your balance is now {jb}{balanceOut[e.Author.Id]}");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough small fish!");
                                    break;
                                }
                            case "EC02":
                                if (ITEM_EC02[e.Author.Id] != 0 && !((ITEM_EC02[e.Author.Id] - numToSell) < 0) && !((ITEM_EC02[e.Author.Id] - numToSell) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC02[e.Author.Id] -= (1 * numToSell);
                                    balanceOut[e.Author.Id] += (long)(17 * numToSell);
                                    await e.Channel.SendMessageAsync($"{numToSell} fish sold for {jb}{17 * numToSell}. Your balance is now {jb}{balanceOut[e.Author.Id]}");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough fish!");
                                    break;
                                }
                            case "EC03":
                                if (ITEM_EC03[e.Author.Id] != 0 && !((ITEM_EC03[e.Author.Id] - numToSell) < 0) && !((ITEM_EC03[e.Author.Id] - numToSell) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC03[e.Author.Id] -= (1 * numToSell);
                                    balanceOut[e.Author.Id] += (long)(4 * numToSell);
                                    await e.Channel.SendMessageAsync($"{numToSell} soggy boot(s) sold for {jb}{4 * numToSell}. Your balance is now {jb}{balanceOut[e.Author.Id]}");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough soggy boots!");
                                    break;
                                }
                            case "EC04":
                                if (ITEM_EC04[e.Author.Id] != 0 && !((ITEM_EC04[e.Author.Id] - numToSell) < 0) && !((ITEM_EC04[e.Author.Id] - numToSell) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC04[e.Author.Id] -= (1 * numToSell);
                                    balanceOut[e.Author.Id] += (long)(22 * numToSell);
                                    await e.Channel.SendMessageAsync($"{numToSell} small cooked fish sold for {jb}{22 * numToSell}. Your balance is now {jb}{balanceOut[e.Author.Id]}");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough small cooked fish!");
                                    break;
                                }
                            case "EC05":
                                if (ITEM_EC05[e.Author.Id] != 0 && !((ITEM_EC05[e.Author.Id] - numToSell) < 0) && !((ITEM_EC05[e.Author.Id] - numToSell) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC05[e.Author.Id] -= (1 * numToSell);
                                    balanceOut[e.Author.Id] += (long)(40 * numToSell);
                                    await e.Channel.SendMessageAsync($"{numToSell} cooked fish sold for {jb}{40 * numToSell}. Your balance is now {jb}{balanceOut[e.Author.Id]}");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough cooked fish!");
                                    break;
                                }
                            case "EC06":
                                if (ITEM_EC06[e.Author.Id] != 0 && !((ITEM_EC06[e.Author.Id] - numToSell) < 0) && !((ITEM_EC06[e.Author.Id] - numToSell) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC06[e.Author.Id] -= (1 * numToSell);
                                    long incomeIncrease = (long)(250 * numToSell);

                                    int activateBoost = rand_num.Next(0, 10);
                                    bool boostActivated = false;

                                    for (int i = 0; (ulong)i < numToSell; ++i)
                                    {
                                        if (activateBoost == 1)
                                        {
                                            long smallGoldBonus = balanceOut[e.Author.Id] / 5;
                                            if (smallGoldBonus > 500) smallGoldBonus = 500;
                                            incomeIncrease += smallGoldBonus;
                                            boostActivated = true;
                                        }

                                        activateBoost = rand_num.Next(0, 10);
                                    }

                                    balanceOut[e.Author.Id] += incomeIncrease;

                                    if (!boostActivated)
                                    {
                                        await e.Channel.SendMessageAsync($"{numToSell} small gold chunk(s) sold for {jb}{incomeIncrease}. Your balance is now {jb}{balanceOut[e.Author.Id]}");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessageAsync($"{numToSell} small gold chunk(s) sold for {jb}{incomeIncrease}. Your balance is now {jb}{balanceOut[e.Author.Id]}\n" +
                                            "**GOLD BOOST ACTIVATED BEFORE SALE!!!**");
                                    }
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough small gold chunks!");
                                    break;
                                }
                            case "EC07":
                                if (ITEM_EC07[e.Author.Id] != 0 && !((ITEM_EC07[e.Author.Id] - numToSell) < 0) && !((ITEM_EC07[e.Author.Id] - numToSell) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC07[e.Author.Id] -= (1 * numToSell);
                                    balanceOut[e.Author.Id] += (long)(25 * numToSell);
                                    await e.Channel.SendMessageAsync($"{numToSell} boot(s) sold for {jb}{25 * numToSell}. Your balance is now {jb}{balanceOut[e.Author.Id]}");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough boots!");
                                    break;
                                }
                            case "EC08":
                                if ((ITEM_EC08[e.Author.Id] != 0) && !((ITEM_EC08[e.Author.Id] - numToSell) < 0) && !((ITEM_EC08[e.Author.Id] - numToSell) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC08[e.Author.Id] -= (1 * numToSell);
                                    balanceOut[e.Author.Id] += (long)(50 * numToSell);
                                    await e.Channel.SendMessageAsync($"{numToSell} pair(s) of boots sold for {jb}{50 * numToSell}. Your balance is now {jb}{balanceOut[e.Author.Id]}");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough pairs of boots!");
                                    break;
                                }
                            default:
                                await e.Channel.SendMessageAsync("**Please ensure you provide an item code in format `EC##`.**");
                                break;
                        }
                        break;
                    case "use": // USE AN ITEM!
                        jb = new Emoji("<:jessbucks:561818526923620353>").ToString();
                        input0 = "";
                        
                        ulong numToUse = 1;

                        int percentage = rand_num.Next(0, 10001);

                        if (cmdArgs.Length == 1)
                        {
                            await e.Channel.SendMessageAsync("No item code provided. Aborting. \n" +
                                "**Please ensure you provide an item code in format `EC##`.**");
                            break;
                        }
                        else if (cmdArgs.Length >= 2)
                        {
                            input0 = cmdArgs[1].ToString().ToUpper();
                            
                            if (cmdArgs.Length >= 3 && cmdArgs[2].ToUpper().Contains("X"))
                            {
                                numToUse = ulong.Parse(cmdArgs[2].ToUpper().Replace("X", ""));
                            }
                        }

                        userInfTrack = 0;
                        isSafeToRun = true;

                        switch (input0)
                        {
                            case "EC00":
                                userInfTrack = ITEM_EC00[e.Author.Id];
                                break;
                            case "EC01":
                                userInfTrack = ITEM_EC01[e.Author.Id];
                                break;
                            case "EC02":
                                userInfTrack = ITEM_EC02[e.Author.Id];
                                break;
                            case "EC03":
                                userInfTrack = ITEM_EC03[e.Author.Id];
                                break;
                            case "EC04":
                                userInfTrack = ITEM_EC04[e.Author.Id];
                                break;
                            case "EC05":
                                userInfTrack = ITEM_EC05[e.Author.Id];
                                break;
                            case "EC06":
                                userInfTrack = ITEM_EC06[e.Author.Id];
                                break;
                            case "EC07":
                                userInfTrack = ITEM_EC07[e.Author.Id];
                                break;
                            case "EC08":
                                userInfTrack = ITEM_EC08[e.Author.Id];
                                break;
                        }
                        
                        for (ulong i = 0; i < numToUse; ++i)
                        {
                            if (!((userInfTrack - 1) < 0) && !((userInfTrack - 1) >= ulong.MaxValue) && isSafeToRun)
                            {
                                userInfTrack -= 1;
                            }
                            else
                            {
                                isSafeToRun = false;
                                break;
                            }
                        }

                        switch (input0)
                        {
                            case "EC00":
                                if (ITEM_EC00[e.Author.Id] != 0 && !((ITEM_EC00[e.Author.Id] - numToUse) < 0) && !((ITEM_EC00[e.Author.Id] - numToUse) >= ulong.MaxValue) && isSafeToRun && isSafeToRun)
                                {
                                    if (numToUse == 1)
                                    {
                                        ITEM_EC00[e.Author.Id] -= 1;

                                        if (percentage < 5500)
                                        {
                                            ITEM_EC01[e.Author.Id] += 1;
                                            await e.Channel.SendMessageAsync("Fishing rod used! You caught a **small fish (EC01)!**");
                                        }
                                        else if (percentage < 8000)
                                        {
                                            ITEM_EC02[e.Author.Id] += 1;
                                            await e.Channel.SendMessageAsync("Fishing rod used! You caught a **medium fish (EC02)!**");
                                        }
                                        else if (percentage < 8750)
                                        {
                                            ITEM_EC03[e.Author.Id] += 1;
                                            await e.Channel.SendMessageAsync("Fishing rod used! You caught a **soggy boot (EC03)!**");
                                        }
                                        else if (percentage < 9650)
                                        {
                                            ITEM_EC06[e.Author.Id] += 1;
                                            await e.Channel.SendMessageAsync("Fishing rod used! You caught a **small gold chunk (EC06)!**");
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync("Fishing rod used! Sadly, nothing was caught.");
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        int EC01_catch = 0;
                                        int EC02_catch = 0;
                                        int EC03_catch = 0;
                                        int EC06_catch = 0;

                                        for (int i = 0; (ulong)i < numToUse; ++i)
                                        {
                                            ITEM_EC00[e.Author.Id] -= 1;

                                            if (percentage < 5500)
                                            {
                                                ITEM_EC01[e.Author.Id] += 1;
                                                EC01_catch += 1;
                                            }
                                            else if (percentage < 8000)
                                            {
                                                ITEM_EC02[e.Author.Id] += 1;
                                                EC02_catch += 1;
                                            }
                                            else if (percentage < 8750)
                                            {
                                                ITEM_EC03[e.Author.Id] += 1;
                                                EC03_catch += 1;
                                            }
                                            else if (percentage < 9650)
                                            {
                                                ITEM_EC06[e.Author.Id] += 1;
                                                EC06_catch += 1;
                                            }
                                            else
                                            {
                                            }
                                            
                                            percentage = rand_num.Next(0, 10001);
                                        }

                                        if (EC01_catch == 0 && EC02_catch == 0 && EC03_catch == 0 && EC06_catch == 0)
                                        {
                                            await e.Channel.SendMessageAsync($"{numToUse} fishing rods used! Sadly, nothing was caught.");
                                            break;
                                        }
                                        else
                                        {
                                            string EC01_catch_str = $"**{EC01_catch} small fish (EC01)**";
                                            string EC02_catch_str = $"**{EC02_catch} fish (EC02)**";
                                            string EC03_catch_str = $"**{EC03_catch} soggy boot(s) (EC03)**";
                                            string EC06_catch_str = $"**{EC06_catch} small gold chunk(s) (EC06)**";

                                            string main_str = $"{numToUse} fishing rods used! You caught: \n";

                                            if (EC01_catch != 0) { main_str += (EC01_catch_str + "\n"); }
                                            if (EC02_catch != 0) { main_str += (EC02_catch_str + "\n"); }
                                            if (EC03_catch != 0) { main_str += (EC03_catch_str + "\n"); }
                                            if (EC06_catch != 0) { main_str += (EC06_catch_str); }

                                            await e.Channel.SendMessageAsync(main_str);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough fishing rods!");
                                    break;
                                }
                            case "EC01":
                                if (ITEM_EC01[e.Author.Id] != 0 && !((ITEM_EC01[e.Author.Id] - numToUse) < 0) && !((ITEM_EC01[e.Author.Id] - numToUse) >= ulong.MaxValue) && isSafeToRun && isSafeToRun)
                                {
                                    ITEM_EC01[e.Author.Id] -= (1 * numToUse);
                                    ITEM_EC04[e.Author.Id] += (1 * numToUse);
                                    await e.Channel.SendMessageAsync($"You have **cooked {numToUse} small fish (EC04)!**");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough small fish!");
                                    break;
                                }
                            case "EC02":
                                if (ITEM_EC02[e.Author.Id] != 0 && !((ITEM_EC02[e.Author.Id] - numToUse) < 0) && !((ITEM_EC02[e.Author.Id] - numToUse) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC02[e.Author.Id] -= (1 * numToUse);
                                    ITEM_EC05[e.Author.Id] += (1 * numToUse);
                                    await e.Channel.SendMessageAsync($"You have **cooked {numToUse} fish (EC05)!**");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough fish!");
                                    break;
                                }
                            case "EC03":
                                if (ITEM_EC03[e.Author.Id] != 0 && !((ITEM_EC03[e.Author.Id] - numToUse) < 0) && !((ITEM_EC03[e.Author.Id] - numToUse) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC03[e.Author.Id] -= (1 * numToUse);
                                    ITEM_EC07[e.Author.Id] += (1 * numToUse);
                                    await e.Channel.SendMessageAsync($"You have dried {numToUse} **boot(s) (EC07)!**");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough soggy boots!");
                                    break;
                                }
                            case "EC04":
                                if (ITEM_EC04[e.Author.Id] != 0 && !((ITEM_EC04[e.Author.Id] - numToUse) < 0) && !((ITEM_EC04[e.Author.Id] - numToUse) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    if (numToUse == 1)
                                    {
                                        ITEM_EC04[e.Author.Id] -= 1;

                                        int smallgold_chance = rand_num.Next(0, 10);

                                        if (smallgold_chance == 0)
                                        {
                                            ITEM_EC06[e.Author.Id] += 1;
                                            await e.Channel.SendMessageAsync("You have eaten a small cooked fish and found a **small chunk of gold (EC06)!**");
                                            break;
                                        }
                                        else if (smallgold_chance == 1 || smallgold_chance == 5)
                                        {
                                            balanceOut[e.Author.Id] += rand_num.Next(9, 28);
                                            await e.Channel.SendMessageAsync("You have eaten a small cooked fish and found a small amount of money!");
                                            break;
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync("You have eaten a small cooked fish.");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        long toAddToBal = 0;
                                        ulong EC06_found = 0;

                                        for (ulong i = 0; i < numToUse; ++i)
                                        {
                                            ITEM_EC04[e.Author.Id] -= 1;

                                            int smallgold_chance = rand_num.Next(0, 10);

                                            if (smallgold_chance == 0)
                                            {
                                                EC06_found += 1;
                                            }
                                            else if (smallgold_chance == 1 || smallgold_chance == 5)
                                            {
                                                toAddToBal += rand_num.Next(9, 28);
                                            }
                                            else
                                            {
                                            }
                                        }

                                        if (toAddToBal == 0 && EC06_found == 0)
                                        {
                                            await e.Channel.SendMessageAsync($"You have eaten {numToUse} small cooked fish.");
                                            break;
                                        }
                                        else
                                        {
                                            ITEM_EC06[e.Author.Id] += EC06_found;
                                            balanceOut[e.Author.Id] += toAddToBal;

                                            string bal_str = $"{jb}{toAddToBal} in money";
                                            string EC06_found_str = $"**{EC06_found} small gold chunk(s) (EC06)**";

                                            string main_str = $"You have eaten {numToUse} small cooked fish and found: \n";

                                            if (toAddToBal != 0) { main_str += (bal_str + "\n"); }
                                            if (EC06_found != 0) { main_str += (EC06_found_str); }

                                            await e.Channel.SendMessageAsync(main_str);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough small cooked fish!");
                                    break;
                                }
                            case "EC05":
                                if (ITEM_EC05[e.Author.Id] != 0 && !((ITEM_EC05[e.Author.Id] - numToUse) < 0) && !((ITEM_EC05[e.Author.Id] - numToUse) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    if (numToUse == 1)
                                    {
                                        ITEM_EC05[e.Author.Id] -= 1;

                                        int smallgold_chance = rand_num.Next(0, 20);

                                        if (smallgold_chance == 0 || smallgold_chance == 5 || smallgold_chance == 10)
                                        {
                                            ITEM_EC06[e.Author.Id] += 1;
                                            await e.Channel.SendMessageAsync("You have eaten a cooked fish and found a **small chunk of gold (EC06)!**");
                                            break;
                                        }
                                        else if (smallgold_chance == 1 || smallgold_chance == 6)
                                        {
                                            ITEM_EC06[e.Author.Id] += (ulong)rand_num.Next(2, 5);
                                            await e.Channel.SendMessageAsync("You have eaten a cooked fish and found a **few small chunks of gold (EC06)!**");
                                            break;
                                        }
                                        else if (smallgold_chance == 2 || smallgold_chance == 14 || smallgold_chance == 11)
                                        {
                                            balanceOut[e.Author.Id] += rand_num.Next(9, 28);
                                            await e.Channel.SendMessageAsync("You have eaten a cooked fish and found a small amount of money!");
                                            break;
                                        }
                                        else if (smallgold_chance == 3 || smallgold_chance == 13)
                                        {
                                            balanceOut[e.Author.Id] += rand_num.Next(29, 77);
                                            await e.Channel.SendMessageAsync("You have eaten a cooked fish and found some money!");
                                            break;
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessageAsync("You have eaten a cooked fish.");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        long toAddToBal = 0;
                                        ulong EC06_found = 0;

                                        for (ulong i = 0; i < numToUse; ++i)
                                        {
                                            ITEM_EC05[e.Author.Id] -= 1;

                                            int smallgold_chance = rand_num.Next(0, 20);

                                            if (smallgold_chance == 0 || smallgold_chance == 5 || smallgold_chance == 10)
                                            {
                                                EC06_found += 1;
                                            }
                                            else if (smallgold_chance == 1 || smallgold_chance == 6)
                                            {
                                                EC06_found += (ulong)rand_num.Next(2, 5);
                                            }
                                            else if (smallgold_chance == 2 || smallgold_chance == 14 || smallgold_chance == 11)
                                            {
                                                toAddToBal += rand_num.Next(9, 28);
                                            }
                                            else if (smallgold_chance == 3 || smallgold_chance == 13)
                                            {
                                                toAddToBal += rand_num.Next(29, 77);
                                            }
                                            else
                                            {
                                            }
                                        }

                                        if (toAddToBal == 0 && EC06_found == 0)
                                        {
                                            await e.Channel.SendMessageAsync($"You have eaten {numToUse} cooked fish.");
                                            break;
                                        }
                                        else
                                        {
                                            ITEM_EC06[e.Author.Id] += EC06_found;
                                            balanceOut[e.Author.Id] += toAddToBal;

                                            string bal_str = $"{jb}{toAddToBal} in money";
                                            string EC06_found_str = $"**{EC06_found} small gold chunk(s) (EC06)**";

                                            string main_str = $"You have eaten {numToUse} cooked fish and found: \n";

                                            if (toAddToBal != 0) { main_str += (bal_str + "\n"); }
                                            if (EC06_found != 0) { main_str += (EC06_found_str); }

                                            await e.Channel.SendMessageAsync(main_str);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough cooked fish!");
                                    break;
                                }
                            case "EC06":
                                if (ITEM_EC06[e.Author.Id] != 0 && !((ITEM_EC06[e.Author.Id] - numToUse) < 0) && !((ITEM_EC06[e.Author.Id] - numToUse) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    if (numToUse == 1)
                                    {
                                        ITEM_EC06[e.Author.Id] -= 1;
                                        long smallGoldBonus = balanceOut[e.Author.Id] / 5;
                                        if (smallGoldBonus > 500) smallGoldBonus = 500;
                                        balanceOut[e.Author.Id] += smallGoldBonus;
                                        await e.Channel.SendMessageAsync("You have used your small gold chunk's bonus! Your balance has increased.");
                                        break;
                                    }
                                    else
                                    {
                                        ITEM_EC06[e.Author.Id] -= numToUse;
                                        long smallGoldBonus = balanceOut[e.Author.Id] / 5;
                                        if (smallGoldBonus > 500) smallGoldBonus = 500;
                                        balanceOut[e.Author.Id] += smallGoldBonus * (long)numToUse;
                                        await e.Channel.SendMessageAsync($"You have used the bonus for {numToUse} small gold chunks! Your balance has increased.");
                                        break;
                                    }
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("You don't have enough gold chunks.");
                                    break;
                                }
                            case "EC07":
                                if (ITEM_EC07[e.Author.Id] >= 2 && !((ITEM_EC05[e.Author.Id] - numToUse) < 2) && !((ITEM_EC05[e.Author.Id] - numToUse) >= ulong.MaxValue) && isSafeToRun)
                                {
                                    ITEM_EC07[e.Author.Id] -= (2 * numToUse);
                                    ITEM_EC08[e.Author.Id] += (1 * numToUse);
                                    await e.Channel.SendMessageAsync($"You have **combined {numToUse} pair(s) of boots (EC08)!**");
                                    break;
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync($"You have too few boots to combine them into {numToUse} pair(s).");
                                    break;
                                }
                            case "EC08":
                                await e.Channel.SendMessageAsync("Not much you can do with these boots right now.");
                                break;
                            default:
                                await e.Channel.SendMessageAsync("**Please ensure you provide an item code in format `EC##`.**");
                                break;
                        }
                        break;
                    case "inv": // GET INVENTORY
                        await e.Channel.SendMessageAsync($"**INVENTORY OF {e.Author.Username.ToUpper()}#{e.Author.Discriminator}**\n"
                        + $"EC00, Fishing Rod x{ITEM_EC00[e.Author.Id]} \n"
                        + $"EC01, Small Fish x{ITEM_EC01[e.Author.Id]} \n"
                        + $"EC02, Medium Fish x{ITEM_EC02[e.Author.Id]} \n"
                        + $"EC03, Soggy Boot x{ITEM_EC03[e.Author.Id]} \n"
                        + $"EC04, Cooked Fish (Small) x{ITEM_EC04[e.Author.Id]} \n"
                        + $"EC05, Cooked Fish (Medium) x{ITEM_EC05[e.Author.Id]} \n"
                        + $"EC06, Small Gold Chunk x{ITEM_EC06[e.Author.Id]} \n"
                        + $"EC07, Boot x{ITEM_EC07[e.Author.Id]} \n"
                        + $"EC08, Boots x{ITEM_EC08[e.Author.Id]} \n");
                        break;
                    case "leads": // LIST OF LEADERS IN THE ECONOMY
                        long currentTop = 0;
                        ulong trackID = 0;
                        List<ulong> economyLeaders = new List<ulong>();

                        if (balanceOut.Keys.ToArray().Length >= 10)
                        {
                            while (economyLeaders.Count < 10)
                            {
                                currentTop = 0;
                                trackID = 0;

                                foreach (ulong uID in balanceOut.Keys)
                                {
                                    if (!economyLeaders.Contains(uID) && economyLeaders.Count < 10)
                                    {
                                        if (balanceOut[uID] >= currentTop)
                                        {
                                            currentTop = balanceOut[uID];
                                            trackID = uID;
                                        }
                                    }
                                }

                                economyLeaders.Add(trackID);
                            }
                        }
                        else
                        {
                            await e.Channel.SendMessageAsync("Unfortunately, there's not enough people to generate the leaderboards. My apologies." + balanceOut.Keys.ToArray().Length);
                            break;
                        }

                        string leader0 = $"1. **{tfix(economyLeaders[0]).Username}#{tfix(economyLeaders[0]).Discriminator}**, with {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[economyLeaders[0]]} \n";
                        string leader1 = $"2. **{tfix(economyLeaders[1]).Username}#{tfix(economyLeaders[1]).Discriminator}**, with {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[economyLeaders[1]]} \n";
                        string leader2 = $"3. **{tfix(economyLeaders[2]).Username}#{tfix(economyLeaders[2]).Discriminator}**, with {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[economyLeaders[2]]} \n";
                        string leader3 = $"4. **{tfix(economyLeaders[3]).Username}#{tfix(economyLeaders[3]).Discriminator}**, with {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[economyLeaders[3]]} \n";
                        string leader4 = $"5. **{tfix(economyLeaders[4]).Username}#{tfix(economyLeaders[4]).Discriminator}**, with {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[economyLeaders[4]]} \n";
                        string leader5 = $"6. **{tfix(economyLeaders[5]).Username}#{tfix(economyLeaders[5]).Discriminator}**, with {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[economyLeaders[5]]} \n";
                        string leader6 = $"7. **{tfix(economyLeaders[6]).Username}#{tfix(economyLeaders[6]).Discriminator}**, with {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[economyLeaders[6]]} \n";
                        string leader7 = $"8. **{tfix(economyLeaders[7]).Username}#{tfix(economyLeaders[7]).Discriminator}**, with {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[economyLeaders[7]]} \n";
                        string leader8 = $"9. **{tfix(economyLeaders[8]).Username}#{tfix(economyLeaders[8]).Discriminator}**, with {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[economyLeaders[8]]} \n";
                        string leader9 = $"10. **{tfix(economyLeaders[9]).Username}#{tfix(economyLeaders[9]).Discriminator}**, with {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[economyLeaders[9]]}";

                        string printLeaders = "**ECONOMIC LEADERBOARD** \n" + leader0 + leader1 + leader2 + leader3 + leader4 + leader5 + leader6 + leader7 + leader8 + leader9;

                        await e.Channel.SendMessageAsync(printLeaders);
                        break;
                    case "gvauth": // ECONGIVE FOR THE UAUTH LIST
                        if (Array.IndexOf(uAuth, e.Author.Id) != -1)
                        {
                            // IEnumerable<IMessage> clearMessages = await e.Channel.GetMessagesAsync(1).FlattenAsync();
                            // foreach (IMessage m in clearMessages) await m.DeleteAsync();

                            if (cmdArgs.Length == 1)
                            {
                                await e.Channel.SendMessageAsync($"You have failed to provide any parameters.");
                                break;
                            }

                            if (cmdArgs[1].Length > 19)
                            {
                                await e.Channel.SendMessageAsync($"This value is too big for the system to handle. Aborting transfer.");
                                break;
                            }

                            if (ulong.Parse(cmdArgs[1]) > long.MaxValue)
                            {
                                await e.Channel.SendMessageAsync($"This value is too big for the system to handle. Aborting transfer.");
                                break;
                            }

                            long ecG_AUTH = long.Parse(cmdArgs[1]);
                            long eG_bak_AUTH = ecG_AUTH;
                            SocketUser eRec_AUTH = e.MentionedUsers.First();
                            string giftMsg_AUTH = e.Content.Remove(0, eRec_AUTH.Mention.Length + cmdArgs[1].Length + 10);

                            if (ecG_AUTH < 1)
                            {
                                await e.Channel.SendMessageAsync($"You have failed to provide a positive amount of money.");
                                break;
                            }

                            if (eRec_AUTH.Id == e.Author.Id)
                            {
                                await e.Channel.SendMessageAsync($"It is absurd to try to send yourself money.");
                                break;
                            }

                            await e.Author.SendMessageAsync($"Your balance is: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[e.Author.Id]}");

                            if (ecG_AUTH > balanceOut[e.Author.Id])
                            {
                                await e.Author.SendMessageAsync($"Insufficient funds to send {eRec_AUTH.Username} {new Emoji("<:jessbucks:561818526923620353>").ToString()}{ecG_AUTH} -- you need: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{ecG_AUTH - balanceOut[e.Author.Id]}");
                                break;
                            }
                            else
                            {
                                balanceOut[e.Author.Id] -= ecG_AUTH;
                                await e.Author.SendMessageAsync($"Your balance is now: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[e.Author.Id]}");
                            }

                            if (!balanceOut.ContainsKey(eRec_AUTH.Id))
                            {
                                if (!eRec_AUTH.IsBot)
                                {
                                    balanceOut.Add(eRec_AUTH.Id, ecG_AUTH);
                                    ecBT.Add(eRec_AUTH.Id, 1.00);
                                    ecB1.Add(eRec_AUTH.Id, false);
                                    ecB2.Add(eRec_AUTH.Id, false);
                                    ecB3.Add(eRec_AUTH.Id, false);
                                    ecB4.Add(eRec_AUTH.Id, false);
                                    ecB5.Add(eRec_AUTH.Id, false);
                                    ecB6.Add(eRec_AUTH.Id, false);
                                    ecB7.Add(eRec_AUTH.Id, false);
                                    ecB8.Add(eRec_AUTH.Id, false);
                                    ecB9.Add(eRec_AUTH.Id, false);
                                    ecB10.Add(eRec_AUTH.Id, false);

                                    ecSBT.Add(eRec_AUTH.Id, 1.00);
                                    ecSB.Add(eRec_AUTH.Id, false);
                                    ecSB1.Add(eRec_AUTH.Id, false);
                                    ecSB2.Add(eRec_AUTH.Id, false);

                                    ecART.Add(eRec_AUTH.Id, 1.00);
                                    ecAR.Add(eRec_AUTH.Id, false);
                                    ecAR1.Add(eRec_AUTH.Id, false);
                                    ecAR2.Add(eRec_AUTH.Id, false);

                                    ecGBT.Add(eRec_AUTH.Id, 1.00);
                                    ecGB.Add(eRec_AUTH.Id, false);
                                    ecGB1.Add(eRec_AUTH.Id, false);
                                    ecGB2.Add(eRec_AUTH.Id, false);
                                    ecGB3.Add(eRec_AUTH.Id, false);

                                    ITEM_EC00.Add(eRec_AUTH.Id, 0);
                                    ITEM_EC01.Add(eRec_AUTH.Id, 0);
                                    ITEM_EC02.Add(eRec_AUTH.Id, 0);
                                    ITEM_EC03.Add(eRec_AUTH.Id, 0);
                                    ITEM_EC04.Add(eRec_AUTH.Id, 0);
                                    ITEM_EC05.Add(eRec_AUTH.Id, 0);
                                    ITEM_EC06.Add(eRec_AUTH.Id, 0);
                                    ITEM_EC07.Add(eRec_AUTH.Id, 0);
                                    ITEM_EC08.Add(eRec_AUTH.Id, 0);

                                    econTimeOut.Add(0);
                                    econUserBlock.Add(false);
                                }
                                else
                                {
                                    await e.Channel.SendMessageAsync("This is a bot user!");
                                }
                            }
                            else
                            {
                                if (((ulong)balanceOut[eRec_AUTH.Id] + (ulong)ecG_AUTH) > long.MaxValue)
                                {
                                    balanceOut[eRec_AUTH.Id] = long.MaxValue;
                                    balanceOut[e.Author.Id] = (long.MaxValue - eG_bak_AUTH);
                                }
                                else
                                {
                                    balanceOut[eRec_AUTH.Id] += ecG_AUTH;
                                }
                            }

                            await e.Author.SendMessageAsync($"Your gift to {eRec_AUTH.Username + "#" + eRec_AUTH.Discriminator} of {new Emoji("<:jessbucks:561818526923620353>").ToString()}{ecG_AUTH} was successfully received. Your balance is now: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[e.Author.Id]}");
                            await eRec_AUTH.SendMessageAsync($"You have received a gift from {e.Author.Username + "#" + e.Author.Discriminator} of {new Emoji("<:jessbucks:561818526923620353>").ToString()}{ecG_AUTH}! Your balance is now: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[eRec_AUTH.Id]}\n\nAdded message:{giftMsg_AUTH}");
                            await e.Channel.SendMessageAsync("Transfer completed!");

                            File.Delete("usersinfo.txt");
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"usersinfo.txt", true))
                            {
                                foreach (ulong uID in balanceOut.Keys)
                                {
                                    // MAIN INFO
                                    file.WriteLine(uID);
                                    file.WriteLine(tfix(uID).Username);

                                    file.WriteLine("BALANCE");

                                    file.WriteLine(balanceOut[uID]);
                                    file.WriteLine(ecBT[uID]);
                                    file.WriteLine(ecB1[uID]);
                                    file.WriteLine(ecB2[uID]);
                                    file.WriteLine(ecB3[uID]);
                                    file.WriteLine(ecB4[uID]);
                                    file.WriteLine(ecB5[uID]);
                                    file.WriteLine(ecB6[uID]);
                                    file.WriteLine(ecB7[uID]);
                                    file.WriteLine(ecB8[uID]);
                                    file.WriteLine(ecB9[uID]);
                                    file.WriteLine(ecB10[uID]);

                                    file.WriteLine("SIPHON");

                                    file.WriteLine(ecSBT[uID]);
                                    file.WriteLine(ecSB[uID]);
                                    file.WriteLine(ecSB1[uID]);
                                    file.WriteLine(ecSB2[uID]);

                                    file.WriteLine("ADDITIVE RANDOM");

                                    file.WriteLine(ecART[uID]);
                                    file.WriteLine(ecAR[uID]);
                                    file.WriteLine(ecAR1[uID]);
                                    file.WriteLine(ecAR2[uID]);

                                    file.WriteLine("GAMBLER");

                                    file.WriteLine(ecGBT[uID]);
                                    file.WriteLine(ecGB[uID]);
                                    file.WriteLine(ecGB1[uID]);
                                    file.WriteLine(ecGB2[uID]);
                                    file.WriteLine(ecGB3[uID]);

                                    // ALTERNATE INFO
                                    file.WriteLine("INVENTORY");

                                    file.WriteLine(ITEM_EC00[uID]);
                                    file.WriteLine(ITEM_EC01[uID]);
                                    file.WriteLine(ITEM_EC02[uID]);
                                    file.WriteLine(ITEM_EC03[uID]);
                                    file.WriteLine(ITEM_EC04[uID]);
                                    file.WriteLine(ITEM_EC05[uID]);
                                    file.WriteLine(ITEM_EC06[uID]);
                                    file.WriteLine(ITEM_EC07[uID]);
                                    file.WriteLine(ITEM_EC08[uID]);
                                }
                            }
                        }

                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "gamble": // GAMBLE JESSBUCKS, THIS IS VERY RISKY!
                        if (cmdArgs.Length == 1)
                        {
                            await e.Channel.SendMessageAsync($"You have failed to provide any parameters.");
                            break;
                        }

                        if (cmdArgs[1].Length > 19)
                        {
                            await e.Channel.SendMessageAsync($"This value is too big for the system to handle. Aborting transfer.");
                            break;
                        }

                        if (ulong.Parse(cmdArgs[1]) > long.MaxValue)
                        {
                            await e.Channel.SendMessageAsync($"This value is too big for the system to handle. Aborting.");
                            break;
                        }

                        if (ulong.Parse(cmdArgs[1]) <= (ulong)balanceOut[e.Author.Id] && ulong.Parse(cmdArgs[1]) >= 500)
                        {
                            long prevBal = balanceOut[e.Author.Id];
                            string jbG = $"{new Emoji("<:jessbucks:561818526923620353>").ToString()}";

                            int heads_tails = rand_num.Next(0, 2);
                            if (heads_tails > 0)
                            {
                                balanceOut[e.Author.Id] += long.Parse(cmdArgs[1]);
                                await e.Channel.SendMessageAsync($"Your balance is: {jbG}{prevBal}\n" + 
                                    $"**You win!** {new Emoji("<:jess:595121644641583104>").ToString()} \n" +
                                    $"Your balance is now: {jbG}{balanceOut[e.Author.Id]}");
                            }
                            else
                            {
                                balanceOut[e.Author.Id] -= long.Parse(cmdArgs[1]);
                                await e.Channel.SendMessageAsync($"Your balance is: {jbG}{prevBal}\n" +
                                    $"**You lose.** \n" +
                                    $"Your balance is now: {jbG}{balanceOut[e.Author.Id]}");
                            }

                            if (balanceOut[e.Author.Id] < 0)
                            {
                                balanceOut[e.Author.Id] = 0;
                            }
                        }
                        else
                        {
                            if (ulong.Parse(cmdArgs[1]) > (ulong)balanceOut[e.Author.Id])
                            {
                                await e.Channel.SendMessageAsync($"Insufficient funds to gamble {new Emoji("<:jessbucks:561818526923620353>").ToString()}{long.Parse(cmdArgs[1])} -- you need: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{long.Parse(cmdArgs[1]) - balanceOut[e.Author.Id]}");
                            }
                            else if (ulong.Parse(cmdArgs[1]) < 500)
                            {
                                await e.Channel.SendMessageAsync($"You **must** gamble a minimum of {new Emoji("<:jessbucks:561818526923620353>").ToString()}500.");
                            }
                            break;
                        }

                        break;
                    case "give": // GIVE USER JESSBUCKS
                        if (cmdArgs.Length == 1)
                        {
                            await e.Channel.SendMessageAsync($"You have failed to provide any parameters.");
                            break;
                        }

                        if (cmdArgs[1].Length > 19)
                        {
                            await e.Channel.SendMessageAsync($"This value is too big for the system to handle. Aborting transfer.");
                            break;
                        }

                        if (ulong.Parse(cmdArgs[1]) > long.MaxValue)
                        {
                            await e.Channel.SendMessageAsync($"This value is too big for the system to handle. Aborting transfer.");
                            break;
                        }

                        long Gift = long.Parse(cmdArgs[1]);
                        long eG_bak = Gift;
                        SocketUser eRec = e.MentionedUsers.First();
                        string giftMsg = e.Content.Remove(0, eRec.Mention.Length + cmdArgs[1].Length + 8);

                        if (Gift < 1)
                        {
                            await e.Channel.SendMessageAsync($"You have failed to provide a positive amount of money.");
                            break;
                        }

                        if (Array.IndexOf(uAuth, e.Author.Id) == -1)
                        {
                            if (eRec.Id == e.Author.Id)
                            {
                                await e.Channel.SendMessageAsync($"It is absurd to try to send yourself money.");
                                break;
                            }

                            await e.Author.SendMessageAsync($"Your balance is: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[e.Author.Id]}");
                            
                            if (Gift > balanceOut[e.Author.Id])
                            {
                                await e.Author.SendMessageAsync($"Insufficient funds to send {eRec.Username} {new Emoji("<:jessbucks:561818526923620353>").ToString()}{Gift} -- you need: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{Gift - balanceOut[e.Author.Id]}");
                                break;
                            }
                            else
                            {
                                balanceOut[e.Author.Id] -= Gift;
                                await e.Author.SendMessageAsync($"Your balance is now: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[e.Author.Id]}");
                            }
                        }

                        if (!balanceOut.ContainsKey(eRec.Id))
                        {
                            if (!eRec.IsBot)
                            {
                                balanceOut.Add(eRec.Id, Gift);
                                ecBT.Add(eRec.Id, 1.00);
                                ecB1.Add(eRec.Id, false);
                                ecB2.Add(eRec.Id, false);
                                ecB3.Add(eRec.Id, false);
                                ecB4.Add(eRec.Id, false);
                                ecB5.Add(eRec.Id, false);
                                ecB6.Add(eRec.Id, false);
                                ecB7.Add(eRec.Id, false);
                                ecB8.Add(eRec.Id, false);
                                ecB9.Add(eRec.Id, false);
                                ecB10.Add(eRec.Id, false);

                                ecSBT.Add(eRec.Id, 1.00);
                                ecSB.Add(eRec.Id, false);
                                ecSB1.Add(eRec.Id, false);
                                ecSB2.Add(eRec.Id, false);

                                ecART.Add(eRec.Id, 1.00);
                                ecAR.Add(eRec.Id, false);
                                ecAR1.Add(eRec.Id, false);
                                ecAR2.Add(eRec.Id, false);

                                ecGBT.Add(eRec.Id, 1.00);
                                ecGB.Add(eRec.Id, false);
                                ecGB1.Add(eRec.Id, false);
                                ecGB2.Add(eRec.Id, false);
                                ecGB3.Add(eRec.Id, false);
                                
                                ITEM_EC00.Add(eRec.Id, 0);
                                ITEM_EC01.Add(eRec.Id, 0);
                                ITEM_EC02.Add(eRec.Id, 0);
                                ITEM_EC03.Add(eRec.Id, 0);
                                ITEM_EC04.Add(eRec.Id, 0);
                                ITEM_EC05.Add(eRec.Id, 0);
                                ITEM_EC06.Add(eRec.Id, 0);
                                ITEM_EC07.Add(eRec.Id, 0);
                                ITEM_EC08.Add(eRec.Id, 0);

                                econTimeOut.Add(0);
                                econUserBlock.Add(false);
                            }
                            else
                            {
                                await e.Channel.SendMessageAsync("This is a bot user!");
                            }
                        }
                        else
                        {
                            if (((ulong)balanceOut[eRec.Id] + (ulong)Gift) > long.MaxValue)
                            {
                                balanceOut[eRec.Id] = long.MaxValue;
                                balanceOut[e.Author.Id] = (long.MaxValue - eG_bak);
                            }
                            else
                            {
                                balanceOut[eRec.Id] += Gift;
                            }
                        }

                        await e.Author.SendMessageAsync($"Your gift to {eRec.Username + "#" + eRec.Discriminator} of {new Emoji("<:jessbucks:561818526923620353>").ToString()}{Gift} was successfully received. Your balance is now: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[e.Author.Id]}");
                        await eRec.SendMessageAsync($"You have received a gift from {e.Author.Username + "#" + e.Author.Discriminator} of {new Emoji("<:jessbucks:561818526923620353>").ToString()}{Gift}! Your balance is now: {new Emoji("<:jessbucks:561818526923620353>").ToString()}{balanceOut[eRec.Id]}\n\nAdded message:{giftMsg}");
                        await e.Channel.SendMessageAsync("Transfer completed!");

                        File.Delete("usersinfo.txt");
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"usersinfo.txt", true))
                        {
                            foreach (ulong uID in balanceOut.Keys)
                            {
                                // MAIN INFO
                                file.WriteLine(uID);
                                file.WriteLine(tfix(uID).Username);

                                file.WriteLine("BALANCE");

                                file.WriteLine(balanceOut[uID]);
                                file.WriteLine(ecBT[uID]);
                                file.WriteLine(ecB1[uID]);
                                file.WriteLine(ecB2[uID]);
                                file.WriteLine(ecB3[uID]);
                                file.WriteLine(ecB4[uID]);
                                file.WriteLine(ecB5[uID]);
                                file.WriteLine(ecB6[uID]);
                                file.WriteLine(ecB7[uID]);
                                file.WriteLine(ecB8[uID]);
                                file.WriteLine(ecB9[uID]);
                                file.WriteLine(ecB10[uID]);

                                file.WriteLine("SIPHON");

                                file.WriteLine(ecSBT[uID]);
                                file.WriteLine(ecSB[uID]);
                                file.WriteLine(ecSB1[uID]);
                                file.WriteLine(ecSB2[uID]);

                                file.WriteLine("ADDITIVE RANDOM");

                                file.WriteLine(ecART[uID]);
                                file.WriteLine(ecAR[uID]);
                                file.WriteLine(ecAR1[uID]);
                                file.WriteLine(ecAR2[uID]);

                                file.WriteLine("GAMBLER");

                                file.WriteLine(ecGBT[uID]);
                                file.WriteLine(ecGB[uID]);
                                file.WriteLine(ecGB1[uID]);
                                file.WriteLine(ecGB2[uID]);
                                file.WriteLine(ecGB3[uID]);

                                // ALTERNATE INFO
                                file.WriteLine("INVENTORY");

                                file.WriteLine(ITEM_EC00[uID]);
                                file.WriteLine(ITEM_EC01[uID]);
                                file.WriteLine(ITEM_EC02[uID]);
                                file.WriteLine(ITEM_EC03[uID]);
                                file.WriteLine(ITEM_EC04[uID]);
                                file.WriteLine(ITEM_EC05[uID]);
                                file.WriteLine(ITEM_EC06[uID]);
                                file.WriteLine(ITEM_EC07[uID]);
                                file.WriteLine(ITEM_EC08[uID]);
                            }
                        }

                        leftServers.Remove((e.Channel as SocketGuildChannel).Guild.Id);
                        break;
                    case "ecreset": // RESET ECONOMY [DISCORD-WIDE!]
                        bool resettingEcon = false;
                        if (Array.IndexOf(uAuth, e.Author.Id) != -1 && cmdArgs.Length == 1)
                        {
                            resettingEcon = true;
                            
                            if (resettingEcon)
                            {
                                foreach (ulong uID in balanceOut.Keys.ToList())
                                {
                                    balanceOut[uID] = 0;
                                    ecBT[uID] = 1;
                                    ecB1[uID] = false;
                                    ecB2[uID] = false;
                                    ecB3[uID] = false;
                                    ecB4[uID] = false;
                                    ecB5[uID] = false;
                                    ecB6[uID] = false;
                                    ecB7[uID] = false;
                                    ecB8[uID] = false;
                                    ecB9[uID] = false;
                                    ecB10[uID] = false;

                                    ecSBT[uID] = 1;
                                    ecSB[uID] = false;
                                    ecSB1[uID] = false;
                                    ecSB2[uID] = false;

                                    ecART[uID] = 1;
                                    ecAR[uID] = false;
                                    ecAR1[uID] = false;
                                    ecAR2[uID] = false;

                                    ecGBT[uID] = 1;
                                    ecGB[uID] = false;
                                    ecGB1[uID] = false;
                                    ecGB2[uID] = false;
                                    ecGB3[uID] = false;

                                    ITEM_EC00[uID] = 0;
                                    ITEM_EC01[uID] = 0;
                                    ITEM_EC02[uID] = 0;
                                    ITEM_EC03[uID] = 0;
                                    ITEM_EC04[uID] = 0;
                                    ITEM_EC05[uID] = 0;
                                    ITEM_EC06[uID] = 0;
                                    ITEM_EC07[uID] = 0;
                                    ITEM_EC08[uID] = 0;
                                }
                                resettingEcon = false;
                            }

                            string HasReset = $"**The economy has been reset. All users' economy values are now at zero.**";
                            await cfix(402721696605274112).SendMessageAsync("" + HasReset);
                            await cfix(550395914364387370).SendMessageAsync("" + HasReset);
                            await cfix(558710378477912073).SendMessageAsync("" + HasReset);
                            await cfix(629177210569359410).SendMessageAsync("" + HasReset);
                        }

                        if (Array.IndexOf(uAuth, e.Author.Id) != -1 && cmdArgs.Length == 2)
                        {
                            resettingEcon = true;
                            ulong tarID = e.MentionedUsers.First().Id;

                            if (resettingEcon)
                            {
                                if (balanceOut.Keys.ToList().Contains(tarID))
                                {
                                    balanceOut[tarID] = 0;
                                    ecBT[tarID] = 1;
                                    ecB1[tarID] = false;
                                    ecB2[tarID] = false;
                                    ecB3[tarID] = false;
                                    ecB4[tarID] = false;
                                    ecB5[tarID] = false;
                                    ecB6[tarID] = false;
                                    ecB7[tarID] = false;
                                    ecB8[tarID] = false;
                                    ecB9[tarID] = false;
                                    ecB10[tarID] = false;

                                    ecSBT[tarID] = 1;
                                    ecSB[tarID] = false;
                                    ecSB1[tarID] = false;
                                    ecSB2[tarID] = false;

                                    ecART[tarID] = 1;
                                    ecAR[tarID] = false;
                                    ecAR1[tarID] = false;
                                    ecAR2[tarID] = false;

                                    ecGBT[tarID] = 1;
                                    ecGB[tarID] = false;
                                    ecGB1[tarID] = false;
                                    ecGB2[tarID] = false;
                                    ecGB3[tarID] = false;

                                    ITEM_EC00[tarID] = 0;
                                    ITEM_EC01[tarID] = 0;
                                    ITEM_EC02[tarID] = 0;
                                    ITEM_EC03[tarID] = 0;
                                    ITEM_EC04[tarID] = 0;
                                    ITEM_EC05[tarID] = 0;
                                    ITEM_EC06[tarID] = 0;
                                    ITEM_EC07[tarID] = 0;
                                    ITEM_EC08[tarID] = 0;
                                }
                                resettingEcon = false;
                            }

                            File.Delete("usersinfo.txt");
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"usersinfo.txt", true))
                            {
                                foreach (ulong uID in balanceOut.Keys)
                                {
                                    // MAIN INFO
                                    file.WriteLine(uID);
                                    file.WriteLine(tfix(uID).Username);

                                    file.WriteLine("BALANCE");

                                    file.WriteLine(balanceOut[uID]);
                                    file.WriteLine(ecBT[uID]);
                                    file.WriteLine(ecB1[uID]);
                                    file.WriteLine(ecB2[uID]);
                                    file.WriteLine(ecB3[uID]);
                                    file.WriteLine(ecB4[uID]);
                                    file.WriteLine(ecB5[uID]);
                                    file.WriteLine(ecB6[uID]);
                                    file.WriteLine(ecB7[uID]);
                                    file.WriteLine(ecB8[uID]);
                                    file.WriteLine(ecB9[uID]);
                                    file.WriteLine(ecB10[uID]);

                                    file.WriteLine("SIPHON");

                                    file.WriteLine(ecSBT[uID]);
                                    file.WriteLine(ecSB[uID]);
                                    file.WriteLine(ecSB1[uID]);
                                    file.WriteLine(ecSB2[uID]);

                                    file.WriteLine("ADDITIVE RANDOM");

                                    file.WriteLine(ecART[uID]);
                                    file.WriteLine(ecAR[uID]);
                                    file.WriteLine(ecAR1[uID]);
                                    file.WriteLine(ecAR2[uID]);
                                    
                                    file.WriteLine("GAMBLER");

                                    file.WriteLine(ecGBT[uID]);
                                    file.WriteLine(ecGB[uID]);
                                    file.WriteLine(ecGB1[uID]);
                                    file.WriteLine(ecGB2[uID]);
                                    file.WriteLine(ecGB3[uID]);

                                    // ALTERNATE INFO
                                    file.WriteLine("INVENTORY");

                                    file.WriteLine(ITEM_EC00[uID]);
                                    file.WriteLine(ITEM_EC01[uID]);
                                    file.WriteLine(ITEM_EC02[uID]);
                                    file.WriteLine(ITEM_EC03[uID]);
                                    file.WriteLine(ITEM_EC04[uID]);
                                    file.WriteLine(ITEM_EC05[uID]);
                                    file.WriteLine(ITEM_EC06[uID]);
                                    file.WriteLine(ITEM_EC07[uID]);
                                    file.WriteLine(ITEM_EC08[uID]);
                                }
                            }
                        }

                        if (!(Array.IndexOf(uAuth, e.Author.Id) != -1))
                        {
                            await e.Channel.SendMessageAsync("You are not permitted to destroy nor to damage the economy.");
                        }
                        break;
                    case "ecreset_hard": // HARD ECONOMIC RESET!
                        resettingEcon = false;

                        if (Array.IndexOf(uAuth, e.Author.Id) != -1)
                        {
                            resettingEcon = true;

                            if (resettingEcon)
                            {
                                balanceOut.Clear();
                                ecBT.Clear();
                                ecB1.Clear();
                                ecB2.Clear();
                                ecB3.Clear();
                                ecB4.Clear();
                                ecB5.Clear();
                                ecB6.Clear();
                                ecB7.Clear();
                                ecB8.Clear();
                                ecB9.Clear();
                                ecB10.Clear();

                                ecSBT.Clear();
                                ecSB.Clear();
                                ecSB1.Clear();
                                ecSB2.Clear();

                                ecART.Clear();
                                ecAR.Clear();
                                ecAR1.Clear();
                                ecAR2.Clear();

                                ecGBT.Clear();
                                ecGB.Clear();
                                ecGB1.Clear();
                                ecGB2.Clear();
                                ecGB3.Clear();

                                ITEM_EC00.Clear();
                                ITEM_EC01.Clear();
                                ITEM_EC02.Clear();
                                ITEM_EC03.Clear();
                                ITEM_EC04.Clear();
                                ITEM_EC05.Clear();
                                ITEM_EC06.Clear();
                                ITEM_EC07.Clear();
                                ITEM_EC08.Clear();

                                resettingEcon = false;
                            }

                            string HasReset = $"**The economy has been __HARD RESET__. All users have been removed from the economic database.**";
                            await cfix(402721696605274112).SendMessageAsync("" + HasReset);
                            await cfix(550395914364387370).SendMessageAsync("" + HasReset);
                            await cfix(558710378477912073).SendMessageAsync("" + HasReset);
                            await cfix(629177210569359410).SendMessageAsync("" + HasReset);

                            File.Delete("usersinfo.txt");
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"usersinfo.txt", true))
                            {
                                file.WriteLine("");
                            }
                        }

                        if (!(Array.IndexOf(uAuth, e.Author.Id) != -1))
                        {
                            await e.Channel.SendMessageAsync("You are not permitted to destroy nor to damage the economy.");
                        }
                        break;
                    case "playstat": // CHANGE GAME STATUS
                        if (Array.IndexOf(uAuth, e.Author.Id) != -1)
                        {
                            IEnumerable<IMessage> clearMessages = await e.Channel.GetMessagesAsync(1).FlattenAsync();
                            foreach (IMessage m in clearMessages) await m.DeleteAsync();
                            await jess.SetGameAsync(command.Remove(0, 9));
                        }
                        break;
                    case "econstat": // CHANGE GAME STATUS TO USER ECONOMY STATUS
                        if (Array.IndexOf(uAuth, e.Author.Id) != -1)
                        {
                            IEnumerable<IMessage> clearMessages = await e.Channel.GetMessagesAsync(1).FlattenAsync();
                            foreach (IMessage m in clearMessages) await m.DeleteAsync();
                            await jess.SetGameAsync($"{balanceOut.Count} users registered");
                        }
                        break;
                    case "help": // HELP MEH
                        string helpMsg = $"{e.Author.Mention}, here's the list of tasks I can perform for you right now. Use {cmdPrefix} before each of these to let me know! \n";
                        string helpPageMsg = $"`Use JR.help # to access a page.`";

                        if (cmdArgs.Length == 1)
                        {
                            await e.Channel.SendMessageAsync($"{helpMsg}" +
                            $"**Page 1 / 4** \n" +
                            $"help - you're reading it, haha! Usage: JR.help (page number) \n" +
                            $"srvrcrtdt - I can get the creation date of this server! \n" +
                            $"version - provides version number. \n" +
                            $"tap - hmm? \n" +
                            $"poke - *sigh* people sometimes... \n" +
                            $"pat - I enjoy this from time to time. \n" +
                            $"pet - ...like my hair, eh? Just don't pull anything weird, or I'll end you. \n" +
                            $"calm - if I'm upset, this will calm me down. \n" +
                            $"compliment - I really appreciate it when people do this. \n" +
                            $"insult - Ugh, **rude!** \n" +
                            $"{helpPageMsg}");
                        }
                        else if (cmdArgs.Length > 1)
                        {
                            int helpPageNum = int.Parse(cmdArgs[1]);
                            switch (helpPageNum)
                            {
                                case 1:
                                    await e.Channel.SendMessageAsync($"{helpMsg}" +
                                    $"**Page 1 / 4** \n" +
                                    $"help - you're reading it, haha! Usage: JR.help (page number) \n" +
                                    $"srvrcrtdt - I can get the creation date of this server! \n" +
                                    $"version - provides version number. \n" +
                                    $"tap - hmm? \n" +
                                    $"poke - *sigh* people sometimes... \n" +
                                    $"pat - I enjoy this from time to time. \n" +
                                    $"pet - ...like my hair, eh? Just don't pull anything weird, or I'll end you. \n" +
                                    $"calm - if I'm upset, this will calm me down. \n" +
                                    $"compliment - I really appreciate it when people do this. \n" +
                                    $"insult - Ugh, **rude!** \n" +
                                    $"{helpPageMsg}");
                                    break;
                                case 2:
                                    await e.Channel.SendMessageAsync($"{helpMsg}" +
                                    $"**Page 2 / 4** \n" +
                                    $"mock - ...you're sick. \n" +
                                    $"mood - returns current mood. \n" +
                                    $"bal - I can check your balance! \n" +
                                    $"gamble - I don't recommend gambling your money... Usage: JR.gamble (amount to gamble) \n" +
                                    $"give - give other users a chunk of your balance! Usage: JR.give (value) (ping recipient) (message) \n" +
                                    $"store - access the store! \n" +
                                    $"buy - buy an item from the store! \n" +
                                    $"leads - check the economic leaderboards and try not to get jealous. \n" +
                                    $"peef - do you *really* want to see my young peef impression...? **(PCR ONLY)**\n" +
                                    $"time - you probably don't need it, but I can tell you the time. \n" +
                                    $"msg_rmv - you *do* realize I'm an admin, right? \n" +
                                    $"musicpost - I'll share some of peef's favorite music with you if you want.\n" +
                                    $"{helpPageMsg}");
                                    break;
                                case 3:
                                    await e.Channel.SendMessageAsync($"{helpMsg}" +
                                    $"**Page 3 / 4** \n" +
                                    $"leaveserver - aw, you really want me gone? ...shame... \n" +
                                    $"callback - I'll come back if you want. \n" +
                                    $"mnm - some old dumb memes and moments peef gave me to share around if you want... **(PCR ONLY)**\n" +
                                    $"inv_pcr - oh sure, I can invite you to PCR! \n" +
                                    $"freemansmind - this is a link to a 'Best of Freeman's Mind' compilation. **(PCR ONLY)**\n" +
                                    $"dnd_roll - I can roll dice for DnD if you'd like! \n" +
                                    $"twitter - this is a link to the Twitter of {jess.GetUser(236738543387541507).Username}#{jess.GetUser(236738543387541507).Discriminator}. \n" +
                                    $"youtube - this is a link to the YouTube of {jess.GetUser(236738543387541507).Username}#{jess.GetUser(236738543387541507).Discriminator}. \n" +
                                    $"stm_peef_frnds - gets the Steam friends list of {jess.GetUser(236738543387541507).Username}#{jess.GetUser(236738543387541507).Discriminator}. \n" +
                                    $"{helpPageMsg}");
                                    break;
                                case 4:
                                    await e.Channel.SendMessageAsync($"{helpMsg}" +
                                    $"**Page 4 / 4** \n" +
                                    $"email_peef - gets the email of {jess.GetUser(236738543387541507).Username}#{jess.GetUser(236738543387541507).Discriminator} so you can talk. \n" +
                                    $"dmpeef - use this so I can DM {jess.GetUser(236738543387541507).Username}#{jess.GetUser(236738543387541507).Discriminator} something you want to tell him. \n" +
                                    $"shut - SHUT. UP. \n" +
                                    $"hey - A video link. **(PCR ONLY)**\n" +
                                    $"immortalbirb - A video link. **(PCR ONLY)**\n" +
                                    $"cocainum - A video link. **(PCR ONLY)**\n" +
                                    $"join - I can't perform this task right now. \n" +
                                    $"play - I can't perform this task right now. \n" +
                                    $"{helpPageMsg}");
                                    break;
                                default:
                                    await e.Channel.SendMessageAsync($"{helpMsg}" +
                                    $"**Page 1 / 4** \n" +
                                    $"help - you're reading it, haha! Usage: JR.help (page number) \n" +
                                    $"srvrcrtdt - I can get the creation date of this server! \n" +
                                    $"version - provides version number. \n" +
                                    $"tap - hmm? \n" +
                                    $"poke - *sigh* people sometimes... \n" +
                                    $"pat - I enjoy this from time to time. \n" +
                                    $"pet - ...like my hair, eh? Just don't pull anything weird, or I'll end you. \n" +
                                    $"calm - if I'm upset, this will calm me down. \n" +
                                    $"compliment - I really appreciate it when people do this. \n" +
                                    $"insult - Ugh, **rude!** \n" +
                                    $"{helpPageMsg}");
                                    break;
                            }
                        }
                        break;
                }

                File.Delete("usersinfo.txt");
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"usersinfo.txt", true))
                {
                    foreach (ulong uID in balanceOut.Keys)
                    {
                        // MAIN INFO
                        file.WriteLine(uID);
                        file.WriteLine(tfix(uID).Username);

                        file.WriteLine("BALANCE");

                        file.WriteLine(balanceOut[uID]);
                        file.WriteLine(ecBT[uID]);
                        file.WriteLine(ecB1[uID]);
                        file.WriteLine(ecB2[uID]);
                        file.WriteLine(ecB3[uID]);
                        file.WriteLine(ecB4[uID]);
                        file.WriteLine(ecB5[uID]);
                        file.WriteLine(ecB6[uID]);
                        file.WriteLine(ecB7[uID]);
                        file.WriteLine(ecB8[uID]);
                        file.WriteLine(ecB9[uID]);
                        file.WriteLine(ecB10[uID]);

                        file.WriteLine("SIPHON");

                        file.WriteLine(ecSBT[uID]);
                        file.WriteLine(ecSB[uID]);
                        file.WriteLine(ecSB1[uID]);
                        file.WriteLine(ecSB2[uID]);

                        file.WriteLine("ADDITIVE RANDOM");

                        file.WriteLine(ecART[uID]);
                        file.WriteLine(ecAR[uID]);
                        file.WriteLine(ecAR1[uID]);
                        file.WriteLine(ecAR2[uID]);

                        file.WriteLine("GAMBLER");

                        file.WriteLine(ecGBT[uID]);
                        file.WriteLine(ecGB[uID]);
                        file.WriteLine(ecGB1[uID]);
                        file.WriteLine(ecGB2[uID]);
                        file.WriteLine(ecGB3[uID]);

                        // ALTERNATE INFO
                        file.WriteLine("INVENTORY");

                        file.WriteLine(ITEM_EC00[uID]);
                        file.WriteLine(ITEM_EC01[uID]);
                        file.WriteLine(ITEM_EC02[uID]);
                        file.WriteLine(ITEM_EC03[uID]);
                        file.WriteLine(ITEM_EC04[uID]);
                        file.WriteLine(ITEM_EC05[uID]);
                        file.WriteLine(ITEM_EC06[uID]);
                        file.WriteLine(ITEM_EC07[uID]);
                        file.WriteLine(ITEM_EC08[uID]);
                    }
                }
            }
        }

        // PING ME ON USER JOIN
        public async Task PingMeOnUserJoin(SocketGuildUser e)
        {
            IRole base_role = e.Guild.GetRole(baseRoles[e.Guild.Id]);
            IMessageChannel sendChannel = cfix(welcomeChannels[e.Guild.Id]);
            IMessageChannel modChannel = cfix(modChannels[e.Guild.Id]);

            if (e.Guild.Id != 402720603846606858)
            {
                await sendChannel.SendMessageAsync(new Emoji("<:jessbullet:550433651444285460>").ToString() + $" {e.Username}, welcome to the Discord! Please check the rules channel :D");
            }
            else
            {
                await sendChannel.SendMessageAsync(new Emoji("<:jessbullet:550433651444285460>").ToString() + $" {e.Username}, welcome to the Discord! Please check the rules and introduce yourself in entry so a moderator can let you in :)");
            }
            await e.AddRoleAsync(base_role);

            await modChannel.SendMessageAsync($"{e.Username}#{e.Discriminator} joined at {DateTime.Now.ToString("h: mm:ss.fff tt")} PST.");

            return;
        }

        // A USER DROPPED
        public async Task UserDropped(SocketGuildUser e)
        {
            IMessageChannel sendChannel = cfix(welcomeChannels[e.Guild.Id]);
            IMessageChannel modChannel = cfix(modChannels[e.Guild.Id]);
            if (e.Nickname == null)
            {
                await sendChannel.SendMessageAsync(new Emoji("<:jessbullet:550433651444285460>").ToString() + $" {e.Mention} (aka {e.Username}) has dropped from the server.");
            }
            else
            {
                await sendChannel.SendMessageAsync(new Emoji("<:jessbullet:550433651444285460>").ToString() + $" {e.Mention} (aka {e.Username} with nickname {e.Nickname}) has dropped from the server.");
            }

            await modChannel.SendMessageAsync($"{e.Username}#{e.Discriminator} left at {DateTime.Now.ToString("h: mm:ss.fff tt")} PST.");

            return;
        }

        // WELL A USER WAS BANNED
        public async Task UserBanned(SocketUser x, SocketGuild g)
        {
            SocketGuildUser e = g.GetUser(x.Id);
            IMessageChannel sendChannel = cfix(welcomeChannels[e.Guild.Id]);
            IMessageChannel modChannel = cfix(modChannels[e.Guild.Id]);
            if (e.Nickname == null)
            {
                await sendChannel.SendMessageAsync(new Emoji("<:jessbullet:550433651444285460>").ToString() + $" {e.Mention} (aka {e.Username}) was banned. Reason: {(await g.GetBanAsync(e)).Reason}");
            }
            else
            {
                await sendChannel.SendMessageAsync(new Emoji("<:jessbullet:550433651444285460>").ToString() + $" {e.Mention} (aka {e.Username} with nickname {e.Nickname}) was banned. Reason: {(await g.GetBanAsync(e)).Reason}");
            }

            await modChannel.SendMessageAsync($"{e.Username}#{e.Discriminator} was banned at {DateTime.Now.ToString("h: mm:ss.fff tt")} PST. Reason: {(await g.GetBanAsync(e)).Reason}");

            return;
        }

        // DEPRECATED
        public async Task NotifyUsersUponUptime()
        {
            IMessageChannel introChannel = cfix(236771856831479809);
            IMessageChannel introChannel2 = cfix(402721696605274112);
            await introChannel.SendMessageAsync($"Hi, everyone! I'm online again! 😄\nMy AI version number is currently {versNum}. \nThis update: {versUpdateInfo}");
            await introChannel2.SendMessageAsync($"Hi, everyone! I'm online again! 😄\nMy AI version number is currently {versNum}. \nThis update: {versUpdateInfo}");
            return;
        }

        // DEPRECATED
        public async Task NotifyUsersUponUptimeDebug()
        {
            IMessageChannel introChannel = cfix(236771856831479809);
            IMessageChannel introChannel2 = cfix(402721696605274112);
            await introChannel.SendMessageAsync($"Hi, everyone! I'm online again! 😄\nMy AI version number is currently {versNum}. \nI may go offline soon due to debugging.");
            await introChannel2.SendMessageAsync($"Hi, everyone! I'm online again! 😄\nMy AI version number is currently {versNum}. \nI may go offline soon due to debugging.");
            return;
        }

        // CHANNEL ID TO CHANNEL OBJECT
        ISocketMessageChannel cfix(ulong id)
        {
            return jess.GetChannel(id) as ISocketMessageChannel;
        }

        SocketGuildChannel cgfix(ulong id)
        {
            return jess.GetChannel(id) as SocketGuildChannel;
        }

        // USER ID TO USER OBJECT
        SocketUser tfix(ulong id)
        {
            return jess.GetUser(id) as SocketUser;
        }

        // DEPRECATED
        public async Task StatusChanged(SocketGuildUser a, SocketGuildUser b)
        {
            IMessageChannel sendChannel = cfix(peefChannels[b.Guild.Id]);
            IMessageChannel sendChannel1 = cfix(peefChannels[236771856831479809]);
            IMessageChannel sendChannel2 = cfix(peefChannels[402720603846606858]);
            if (b.Id == 236738543387541507)
            {
                switch (b.Status)
                {
                    case UserStatus.Online:
                        if (!alreadyOnline)
                        {
                            //await sendChannel.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} is online! :D");
                            await sendChannel1.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} is online! :D");
                            await sendChannel2.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} is online! :D");
                            alreadyOnline = true;
                            alreadyIdle = false;
                            alreadyDND = false;
                            alreadyAFK = false;
                            alreadyOffline = false;
                        }
                        if (alreadyOnline)
                        {
                        }
                        break;
                    case UserStatus.Idle:
                        if (!alreadyIdle)
                        {
                            //await sendChannel.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} is online! (Idle)");
                            await sendChannel1.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} is online! (Idle)");
                            await sendChannel2.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} is online! (Idle)");
                            alreadyIdle = true;
                            alreadyOnline = false;
                            alreadyDND = false;
                            alreadyAFK = false;
                            alreadyOffline = false;
                        }
                        if (alreadyIdle)
                        {
                        }
                        break;
                    case UserStatus.DoNotDisturb:
                        if (!alreadyDND)
                        {
                            //await sendChannel.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} is set to 'Do Not Disturb.'");
                            await sendChannel1.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} is set to 'Do Not Disturb.'");
                            await sendChannel2.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} is set to 'Do Not Disturb.'");
                            alreadyDND = true;
                            alreadyIdle = false;
                            alreadyOnline = false;
                            alreadyAFK = false;
                            alreadyOffline = false;
                        }
                        if (alreadyDND)
                        {
                        }
                        break;
                    case UserStatus.AFK:
                        if (!alreadyAFK)
                        {
                            //await sendChannel.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} went AFK.");
                            await sendChannel1.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} went AFK.");
                            await sendChannel2.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} went AFK.");
                            alreadyAFK = true;
                            alreadyOnline = false;
                            alreadyIdle = false;
                            alreadyDND = false;
                            alreadyOffline = false;
                        }
                        if (alreadyAFK)
                        {
                        }
                        break;
                    case UserStatus.Invisible:
                        if (!alreadyOffline)
                        {
                            //await sendChannel.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} went offline.");
                            await sendChannel1.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} went offline.");
                            await sendChannel2.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} went offline.");
                            alreadyOffline = true;
                            alreadyOnline = false;
                            alreadyDND = false;
                            alreadyAFK = false;
                            alreadyIdle = false;
                        }
                        if (alreadyOffline)
                        {
                        }
                        break;
                    case UserStatus.Offline:
                        if (!alreadyOffline)
                        {
                            //await sendChannel.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} went offline.");
                            await sendChannel1.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} went offline.");
                            await sendChannel2.SendMessageAsync($"{jess.GetUser(236738543387541507).Mention} went offline.");
                            alreadyOffline = true;
                            alreadyOnline = false;
                            alreadyDND = false;
                            alreadyAFK = false;
                            alreadyIdle = false;
                        }
                        if (alreadyOffline)
                        {
                        }
                        break;
                }
            }
            return;
        }
    }
}