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
using Jessbot.IO;

namespace Jessbot.Data
{
    /* This is the database code.
    -* This will be used as a library for:
    -* Server data
    -* User data
    -* Other bot-crucial data
    -*/

    // A special library class for all of the separate databases.
    class JessbotDatabaseLibrary
    {
        // Data storage

        // Guilds data
        #region GUILDS

        public static List<ulong> JessGuildsList = new List<ulong>(); // Stores registered guilds, important list
        public static List<string> JessGuildNamesList = new List<string>(); // Stores registered guilds in string format, important list

        public static Dictionary<ulong, ulong> JessGuildWelcomeChannels = new Dictionary<ulong, ulong>(); // Stores guild ID and welcome channel ID, must be set by owner or administration

        public static Dictionary<ulong, ulong> JessGuildWelcomeRoles = new Dictionary<ulong, ulong>(); // Stores guild ID and base role, must be set by owner or administration

        public static Dictionary<ulong, ulong> JessGuildModChannels = new Dictionary<ulong, ulong>(); // Stores guild ID and mod channel ID, must be set by owner or administration

        public static Dictionary<ulong, string> JessGuildPrefixes = new Dictionary<ulong, string>(); // Stores guild ID and prefix, must be set by owner or administration

        public static Dictionary<ulong, bool> JessGuildInviteToggles = new Dictionary<ulong, bool>(); // Stores guild ID and true/false for allowing invites, must be set by owner or administration

        public static Dictionary<ulong, bool> JessGuildPublicToggles = new Dictionary<ulong, bool>(); // Stores guild ID and true/false for showing in the directory, must be set by owner or administration

        public static Dictionary<ulong, bool> JessGuildJoinToggles = new Dictionary<ulong, bool>(); // Stores whether join messages are allowed in this server
        public static Dictionary<ulong, bool> JessGuildBanToggles = new Dictionary<ulong, bool>(); // Stores whether ban messages are allowed in this server

        #endregion

        // User data
        #region USERS

        public static List<ulong> JessUsersList = new List<ulong>(); // Stores all users. This is a master list that is used to populate the user data.
        public static List<JessbotUserData> JessUsersInfoList = new List<JessbotUserData>(); // Correlates with the above.

        #endregion
        
    }

    // This is an object specifically meant to store an individual user's information.
    // Experience, economy, and inventory tracking will take place through this class.
    // Serves as a cleaner, hierarchical alternative to storing everything in a single, master user list.
    class JessbotUserData
    {
        ulong DiscordID = 0; // Stores the numerical ID of this user.

        BigInteger UserExpValuation = new BigInteger(); // Used to store the user's experience points.
        BigInteger UserBalValuation = new BigInteger(); // Used to store the user's balance.

        ulong UserExpLevel = 0; // Used to store the user's experience level.

        UserExpAttributes JUXA = new UserExpAttributes(); // Used to track what has happened with a user's experience level.

        UserEconAttributes JUCA = new UserEconAttributes(); // Used to track what has happened with this user's specific store purchases.

        UserInventory UserInventory = new UserInventory(); // Used to manage the user's inventory.

        // Initializes a user so they may be registered.
        public JessbotUserData(ulong ID)
        {
            DiscordID = ID;
        }

        #region GETTERS

        // Gets the JUXA object for this user.
        public UserExpAttributes GetJUXA()
        { return JUXA; }

        // Gets the JUCA object for this user.
        public UserEconAttributes GetJUCA()
        { return JUCA; }

        public BigInteger getUserExpValue()
        { return UserExpValuation; }

        public BigInteger getUserExpLevel()
        { return UserExpLevel; }

        #endregion

        #region SETTERS

        // Sets the experience for this user.
        public void SetExperience(BigInteger experience)
        {
            ulong OldLevel = UserExpLevel;
            UserExpValuation = experience;

            for (uint i = 0; i < Jessbot.CONST_TotalLevels.Count; ++i)
            {
                if (Jessbot.CONST_TotalLevels[i] <= UserExpValuation)
                {
                    UserExpLevel = i;

                    if (OldLevel < UserExpLevel)
                    {
                        Jessbot.FALLBACK_UserLevelledUp = true;
                    }
                }
                else
                { break; }
            }
        }

        // Sets the experience for this user but determines whether or not they should have a level-up message.
        public void SetExperience(BigInteger experience, bool doIRunLevelUp)
        {
            ulong OldLevel = UserExpLevel;
            UserExpValuation = experience;

            for (uint i = 0; i < Jessbot.CONST_TotalLevels.Count; ++i)
            {
                if (Jessbot.CONST_TotalLevels[i] <= UserExpValuation)
                {
                    UserExpLevel = i;

                    if (OldLevel < UserExpLevel && doIRunLevelUp)
                    {
                        Jessbot.FALLBACK_UserLevelledUp = true;
                    }
                }
                else
                { break; }
            }
        }

        public void SetMonetaryValue(BigInteger cash)
        {
            UserBalValuation = cash;
        }

        #endregion
    }

    // The "inventory" of a user's experience boosts and unlocks.
    class UserExpAttributes
    {
        // Make sure these Dictionaries are public. They will have some advanced modifications so it is good to know what these are.
        public Dictionary<ulong, bool> properties = new Dictionary<ulong, bool>();

        public Dictionary<ulong, bool> unlocks = new Dictionary<ulong, bool>();

        // This will initialize the class. 
        public UserExpAttributes()
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

    // The "inventory" of a user's economy boosts and unlocks.
    class UserEconAttributes
    {
        // Make sure these Dictionaries are public. They will have some advanced modifications so it is good to know what these are.
        public Dictionary<ulong, bool> properties = new Dictionary<ulong, bool>();

        public Dictionary<ulong, bool> tiers = new Dictionary<ulong, bool>();

        // This will initialize the class. 
        public UserEconAttributes()
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

    class UserInventory
    {
        // Make sure these Dictionaries are public. They will have some advanced modifications so it is good to know what these are.
        public Dictionary<ulong, BigInteger> TL_Tools = new Dictionary<ulong, BigInteger>();

        public Dictionary<ulong, BigInteger> FDS_Foodstuffs = new Dictionary<ulong, BigInteger>();

        public Dictionary<ulong, BigInteger> RM_Rawmats = new Dictionary<ulong, BigInteger>();

        // This will initialize the class. 
        public UserInventory()
        {
            // Initialize tools.
            TL_Tools.Add(00, 0); // Fishing Rod I (TL00)

            // Initialize foodstuffs.
            FDS_Foodstuffs.Add(00, 0); // Fish, Tiny (FDS00)
            FDS_Foodstuffs.Add(01, 0); // Fish, Small (FDS01)
            FDS_Foodstuffs.Add(02, 0); // Fish, Medium (FDS02)
            FDS_Foodstuffs.Add(03, 0); // Fish, Large (FDS03)
            FDS_Foodstuffs.Add(04, 0); // Fish, Huge (FDS04)

            // Initialize raw materials.
            RM_Rawmats.Add(00, 0); // Gold Nugget, Tiny (RM00)
            RM_Rawmats.Add(01, 0); // Gold Nugget, Small (RM01)
        }
    }
}