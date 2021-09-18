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
    }

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
}
