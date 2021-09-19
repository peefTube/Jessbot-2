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
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jessbot.Services
{
    // This file designates complex I/O functions that are primarily beneficial for JSON files.

    public partial class DatabaseService
    {
        #region SERVER DATA
        // Prep for serverdata reading.
        private Dictionary<ulong, ServerNucleus> ServerNuclei = new Dictionary<ulong, ServerNucleus>();

        public Dictionary<ulong, ServerProfile> ReadServerDatabase(string source)
        {
            return ConvertServerNucleiToProfiles(RSD_RAW(source));
        }

        public Dictionary<ulong, ServerNucleus> RSD_RAW(string source)
        {
            ServerNuclei = JsonConvert.DeserializeObject<Dictionary<ulong, ServerNucleus>>(File.ReadAllText(source));
            return ServerNuclei;
        }

        public Dictionary<ulong, ServerProfile> ConvertServerNucleiToProfiles(Dictionary<ulong, ServerNucleus> nuclei)
        {
            Dictionary<ulong, ServerProfile> ParsedProfileList = new Dictionary<ulong, ServerProfile>();

            foreach (ulong i in nuclei.Keys)
            {
                // Conversion!
                ServerNucleus Curr = nuclei[i];
                ServerProfile TempGuild = new ServerProfile(i, Curr.Name)
                {
                    WelcomeChannel  = Curr.WelcomeChannel,
                    WelcomeRole     = Curr.WelcomeRole,
                    ModChannel      = Curr.ModChannel,

                    Prefix          = Curr.Prefix,

                    RequiredReads   = Curr.RequiredReads,

                    AllowingInvites = Curr.AllowingInvites,
                    AllowingJoinMsg = Curr.AllowingJoinMsg,
                    AllowingBansMsg = Curr.AllowingBansMsg,
                    AllowingVisible = Curr.AllowingVisible,
                };

                ParsedProfileList.Add(i, TempGuild);

                Curr = null;
                TempGuild = null;
            }

            return ParsedProfileList;
        }

        #endregion

        #region USER DATA
        // Prep for userdata reading.
        private Dictionary<ulong, UserNucleus> UserNuclei = new Dictionary<ulong, UserNucleus>();

        public Dictionary<ulong, UserProfile> ReadUserDatabase(string source)
        {
            return ConvertUserNucleiToProfiles(RUD_RAW(source));
        }

        public Dictionary<ulong, UserNucleus> RUD_RAW(string source)
        {
            UserNuclei = JsonConvert.DeserializeObject<Dictionary<ulong, UserNucleus>>(File.ReadAllText(source));
            return UserNuclei;
        }

        public Dictionary<ulong, UserProfile> ConvertUserNucleiToProfiles(Dictionary<ulong, UserNucleus> nuclei)
        {
            Dictionary<ulong, UserProfile> ParsedProfileList = new Dictionary<ulong, UserProfile>();

            foreach (ulong i in nuclei.Keys)
            {
                // Conversion!
                UserNucleus Curr = nuclei[i];
                UserProfile TempUser = new UserProfile(i)
                {
                    Username = Curr.Username,
                    Experience = Curr.Experience,
                    Balance = Curr.Balance,
                    Level = Curr.Level,
                    UserUTC = Curr.UserUTC,

                    // Known nicknames!
                    AliasList = Curr.AliasList,

                    // Preferred color!
                    PrefColor = Curr.PrefColor.ToColor(),

                    // User profiles!
                    ExpData = Curr.ExpData.ToProfile(),
                    EconData = Curr.EconData.ToProfile(),
                    Inventory = Curr.Inventory.ToProfile()
                };

                ParsedProfileList.Add(i, TempUser);

                Curr = null;
                TempUser = null;
            }

            return ParsedProfileList;
        }

        #endregion
    }

    #region SERVER NUCLEI
    // This is used to parse a new server from JSON code.
    public class ServerNucleus
    {
        // Structured as if the JSON file.
        [JsonPropertyName("GuildId")]
        public ulong GuildId                { get; set; }   // internal ID
        [JsonPropertyName("Name")]
        public string Name                  { get; set; }   // internal guild name - this 'floats'

        [JsonPropertyName("WelcomeChannel")]
        public ulong WelcomeChannel         { get; set; }   // internal ID for desired channel
        [JsonPropertyName("WelcomeRole")]
        public ulong WelcomeRole            { get; set; }   // internal ID for desired role
        [JsonPropertyName("ModChannel")]
        public ulong ModChannel             { get; set; }   // internal ID for desired channel

        [JsonPropertyName("Prefix")]
        public string Prefix                { get; set; }   // internal string for server's command prefix

        [JsonPropertyName("RequiredReads")]
        public List<ulong> RequiredReads    { get; set; }   // List object for holding internal channel IDs

        // These should explain themselves.
        [JsonPropertyName("AllowingInvites")]
        public bool AllowingInvites         { get; set; }
        [JsonPropertyName("AllowingJoinMsg")]
        public bool AllowingJoinMsg         { get; set; }
        [JsonPropertyName("AllowingBansMsg")]
        public bool AllowingBansMsg         { get; set; }
        [JsonPropertyName("AllowingVisible")]
        public bool AllowingVisible         { get; set; }
    }

    #endregion

    #region USER NUCLEI
    // This is used to parse a new user from JSON code.
    public class UserNucleus
    {
        [JsonPropertyName("Username")]
        public string Username { get; set; }

        [JsonPropertyName("UserID")]
        public ulong Identifier { get; set; }

        [JsonPropertyName("Experience")]
        public BigInteger Experience { get; set; }

        [JsonPropertyName("Balance")]
        public BigInteger Balance { get; set; }

        [JsonPropertyName("Level")]
        public ulong Level { get; set; }
        
        [JsonPropertyName("AliasList")]
        public Dictionary<ulong, string> AliasList { get; set; }

        [JsonPropertyName("PrefColor")]
        public ColorNucleus PrefColor { get; set; }

        [JsonPropertyName("UserUTC")]
        public string UserUTC { get; set; }

        [JsonPropertyName("ExpData")]
        public ExpNucleus ExpData { get; set; }

        [JsonPropertyName("EconData")]
        public EconNucleus EconData { get; set; }

        [JsonPropertyName("Inventory")]
        public InvNucleus Inventory { get; set; }
    }

    // This is used to parse a user's experience data from JSON code.
    public class ExpNucleus
    {
        [JsonPropertyName("Properties")]
        public Dictionary<ulong, bool> Properties { get; set; }
        
        [JsonPropertyName("Unlocks")]
        public Dictionary<ulong, bool> Unlocks { get; set; }

        public ExpProfile ToProfile()
        {
            ExpProfile EXPORT = new ExpProfile()
            {
                Properties = this.Properties,
                Unlocks = this.Unlocks
            };

            return EXPORT;
        }
    }

    // This is used to parse a user's economy data from JSON code.
    public class EconNucleus
    {
        [JsonPropertyName("Properties")]
        public Dictionary<ulong, bool> Properties { get; set; }

        [JsonPropertyName("Tiers")]
        public Dictionary<ulong, bool> Tiers { get; set; }

        public EconProfile ToProfile()
        {
            EconProfile EXPORT = new EconProfile()
            {
                Properties = this.Properties,
                Tiers = this.Tiers
            };

            return EXPORT;
        }
    }

    // This is used to parse a user's inventory data from JSON code.
    public class InvNucleus
    {
        [JsonPropertyName("TL_TOOLS")]
        public Dictionary<ulong, BigInteger> TL_TOOLS { get; set; }

        [JsonPropertyName("FDS_FOODSTUFFS")]
        public Dictionary<ulong, BigInteger> FDS_FOODSTUFFS { get; set; }

        [JsonPropertyName("RM_RAWMATS")]
        public Dictionary<ulong, BigInteger> RM_RAWMATS { get; set; }

        public InvProfile ToProfile()
        {
            InvProfile EXPORT = new InvProfile()
            {
                TL_TOOLS = this.TL_TOOLS,
                FDS_FOODSTUFFS = this.FDS_FOODSTUFFS,
                RM_RAWMATS = this.RM_RAWMATS
            };

            return EXPORT;
        }
    }

    // This is used to parse color data from JSON code.
    public class ColorNucleus
    {
        [JsonPropertyName("RawValue")]
        public int RawValue { get; set; }

        [JsonPropertyName("R")]
        public int R { get; set; }

        [JsonPropertyName("G")]
        public int G { get; set; }

        [JsonPropertyName("B")]
        public int B { get; set; }

        public Color ToColor()
        {
            return new Color(this.R, this.G, this.B);
        }
    }

    #endregion
}
