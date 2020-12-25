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

        BigInteger _experience = 0; // Massive integer value for containing experience data.
        BigInteger _balance = 0;    // Massive integer value for containing wallet balance data.

        ulong _level = 0; // Unsigned long integer value for containing user's levelling data.

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

            _experience = (BigInteger)incoming[1];
            _balance    = (BigInteger)incoming[2];

            _level = (ulong)incoming[3];

            // This loads the subprofiles in and replaces the defaults with the new ones.
            List<object> subprofiles = (List<object>)incoming[4];
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
    }

    public class ExpProfile
    {

        // Empty constructor. Will initialize everything to its default value.
        public ExpProfile()
        {
            // TODO: Write me!
        }

        // I/O constructor. Will initialize everything from existing data.
        public ExpProfile(List<object> incoming)
        {
            // TODO: Write me!
        }
    }

    public class EconProfile
    {

        // Empty constructor. Will initialize everything to its default value.
        public EconProfile()
        {
            // TODO: Write me!
        }

        // I/O constructor. Will initialize everything from existing data.
        public EconProfile(List<object> incoming)
        {
            // TODO: Write me!
        }
    }

    public class InvProfile
    {

        // Empty constructor. Will initialize everything to its default value.
        public InvProfile()
        {
            // TODO: Write me!
        }

        // I/O constructor. Will initialize everything from existing data.
        public InvProfile(List<object> incoming)
        {
            // TODO: Write me!
        }
    }
}
