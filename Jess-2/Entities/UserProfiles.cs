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

namespace Jessbot.Entities
{
    public class UserProfile
    {
        public ulong UserID = 0; // Discord-side identifier value for this user.

        public BigInteger Experience = 0; // Massive integer value for containing experience data.
        public BigInteger Balance = 0;    // Massive integer value for containing wallet balance data.

        public ulong Level = 0; // Unsigned long integer value for containing user's levelling data.

        public Color PrefColor = new Color(0, 0, 0); // User-designated preferred color. Defaults to black.
        public string UserUTC = "UTC-00:00"; // User-designated UTC code. Defaults to UTC itself.

        // Subprofiles.
        public ExpProfile ExpData = new ExpProfile();    // Experience subprofile.
        public EconProfile EconData = new EconProfile(); // Economy subprofile.
        public InvProfile Inventory = new InvProfile();  // Inventory subprofile.

        // Constructor for Database I/O. Users are automatically registered
        // so new profile information (Exp, Econ, Inv) will have to be handled
        // on the fly in that case; for the I/O this is passed in as an additional argument.
        public UserProfile(List<object> incoming)
        {
            // Each of these should be self-explanatory.
            UserID = (ulong)incoming[0];

            Experience = (BigInteger)incoming[1];
            Balance = (BigInteger)incoming[2];

            Level = (ulong)incoming[3];

            // This manages color input.
            int[] _tempColor = ((string)incoming[4]).Split(',').Select(_raw => int.Parse(_raw)).ToArray();
            PrefColor = new Color(_tempColor[0], _tempColor[1], _tempColor[2]);

            // This should be self-explanatory again.
            UserUTC = (string)incoming[5];

            // This loads the subprofiles in and replaces the defaults with the new ones.
            List<object> subprofiles = (List<object>)incoming[6];
            ExpData   = (ExpProfile)subprofiles[0];
            EconData  = (EconProfile)subprofiles[1];
            Inventory = (InvProfile)subprofiles[2];
        }

        // Constructor for auto-registration.
        public UserProfile(ulong id)
        {
            UserID = id; // You only need to pass in the user's Discord-side ID value.
                         // Everything else is automatically initialized and ready to go.
        }

        // Special enum-based getter for subprofiles.
        // Might be useless given these are set public...
        public object GetSubProf(SubProfs subprofile)
        {
            switch (subprofile)
            {
                case SubProfs.Exp:
                    return ExpData;
                case SubProfs.Econ:
                    return EconData;
                case SubProfs.Inv:
                    return Inventory;
                default:
                    return null;
            }
        }
    }

    public class ExpProfile
    {
        public Dictionary<ulong, bool> Properties = new Dictionary<ulong, bool>();
        public Dictionary<ulong, bool> Unlocks    = new Dictionary<ulong, bool>();

        // Empty constructor. Will initialize everything to its default value.
        public ExpProfile()
        {
            // All will be set to false since they will be set true over time or upon loading.
            // Initialize the properties.
            Properties.Add(00, false); // Boost
            Properties.Add(01, false);
            Properties.Add(02, false);
            Properties.Add(03, false);
            Properties.Add(04, false);
            Properties.Add(05, false);
            Properties.Add(06, false);
            Properties.Add(07, false);
            Properties.Add(08, false);
            Properties.Add(09, false);

            // Initialize some unlocks.
            Unlocks.Add(2, false); // Unlocks for level two.
            Unlocks.Add(5, false); // Unlocks for level five.
        }

        // I/O constructor. Will initialize everything from existing data.
        public ExpProfile(List<object> incoming)
        {
            // TODO: Write me!
        }
    }

    public class EconProfile
    {
        public Dictionary<ulong, bool> Properties = new Dictionary<ulong, bool>();
        public Dictionary<ulong, bool> Tiers      = new Dictionary<ulong, bool>();

        // Empty constructor. Will initialize everything to its default value.
        public EconProfile()
        {
            // All will be set to false since they will be set true over time or upon loading.
            // Initialize the properties.
            Properties.Add(00, false); // Boost
            Properties.Add(01, false);
            Properties.Add(02, false);
            Properties.Add(03, false);
            Properties.Add(04, false);
            Properties.Add(05, false);
            Properties.Add(06, false);
            Properties.Add(07, false);
            Properties.Add(08, false);
            Properties.Add(09, false);

            // Initialize some tiers.
            Tiers.Add(1, false); // Tier One
            Tiers.Add(2, false); // Tier Two
        }

        // I/O constructor. Will initialize everything from existing data.
        public EconProfile(List<object> incoming)
        {
            // TODO: Write me!
        }
    }

    public class InvProfile
    {
        public Dictionary<ulong, BigInteger> TL_TOOLS       = new Dictionary<ulong, BigInteger>();
        public Dictionary<ulong, BigInteger> FDS_FOODSTUFFS = new Dictionary<ulong, BigInteger>();
        public Dictionary<ulong, BigInteger> RM_RAWMATS     = new Dictionary<ulong, BigInteger>();

        // Empty constructor. Will initialize everything to its default value.
        public InvProfile()
        {
            // Initialize tools.
            TL_TOOLS.Add(00, 0); // Fishing Rod I (TL00)

            // Initialize foodstuffs.
            FDS_FOODSTUFFS.Add(00, 0); // Fish, Tiny (FDS00)
            FDS_FOODSTUFFS.Add(01, 0); // Fish, Small (FDS01)
            FDS_FOODSTUFFS.Add(02, 0); // Fish, Medium (FDS02)
            FDS_FOODSTUFFS.Add(03, 0); // Fish, Large (FDS03)
            FDS_FOODSTUFFS.Add(04, 0); // Fish, Huge (FDS04)

            // Initialize raw materials.
            RM_RAWMATS.Add(00, 0); // Gold Nugget, Tiny (RM00)
            RM_RAWMATS.Add(01, 0); // Gold Nugget, Small (RM01)
        }

        // I/O constructor. Will initialize everything from existing data.
        public InvProfile(List<object> incoming)
        {
            // TODO: Write me!
        }
    }

    public enum SubProfs
    {
        Exp,
        Econ,
        Inv
    }
}
