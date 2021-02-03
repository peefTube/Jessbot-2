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
    public class ServerProfile
    {
        public ulong GuildId = 0; // Discord-side identifier value for this guild.
        public string Name = "";  // The public guild name.

        public ulong WelcomeChannel = 0; // Discord-side identifier value for a particular channel.
        public ulong WelcomeRole = 0;    // Discord-side identifier value for a particular role.

        public ulong ModChannel = 0;     // Discord-side identifier value for a particular channel.

        public string Prefix = "JR.";  // The prefix for commands. Defaults to this value if not set.

        public List<ulong> RequiredReads = new List<ulong>(); // Required reading for this guild.

        public bool AllowingInvites = false; // Toggles invite creation functionality.
        public bool AllowingJoinMsg = true;  // Toggles join and leave messages.
        public bool AllowingBansMsg = true;  // Toggles ban messages.
        public bool AllowingVisible = true;  // Toggles visibility in the server directory.
        
        // Default, empty constructor. Everything is pre-set, minus server ID and name. Pass these in.
        public ServerProfile(ulong ID, string NamePass)
        {
            GuildId = ID;
            Name = NamePass;
        }

        // Constructor for Database I/O or complete registration process.
        public ServerProfile(List<object> incoming)
        {
            // Each of these should be self-explanatory.
            GuildId = (ulong)incoming[0];
            Name = (string)incoming[1];

            WelcomeChannel = (ulong)incoming[2];
            WelcomeRole    = (ulong)incoming[3];

            ModChannel = (ulong)incoming[4];

            Prefix = (string)incoming[5];

            // These are boolean attributes.
            AllowingInvites = (bool)incoming[6];
            AllowingJoinMsg = (bool)incoming[7];
            AllowingBansMsg = (bool)incoming[8];
            AllowingVisible = (bool)incoming[9];
        }

        // Data pass to the Database I/O. Acts as an inverse of the DB I/O's specified constructor.
        public List<object> DataPass()
        {
            // Need to create the inverse of "incoming" from the constructor here, unfortunately.
            List<object> outgoing = new List<object>();

            // Add all values.
            outgoing.Add(GuildId);
            outgoing.Add(Name);
            outgoing.Add(WelcomeChannel);
            outgoing.Add(WelcomeRole);
            outgoing.Add(ModChannel);
            outgoing.Add(Prefix);
            outgoing.Add(AllowingInvites);
            outgoing.Add(AllowingJoinMsg);
            outgoing.Add(AllowingBansMsg);
            outgoing.Add(AllowingVisible);
            outgoing.Add(RequiredReads);

            // Package is ready. Send.
            return outgoing;
        }
    }
}
