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
        string _name = "";        // The public guild name.

        ulong _welcomeChannel = 0; // Discord-side identifier value for a particular channel.
        ulong _welcomeRole = 0;    // Discord-side identifier value for a particular role.

        ulong _modChannel = 0;     // Discord-side identifier value for a particular channel.

        public string Prefix = "JR.";  // The prefix for commands. Defaults to this value if not set.

        bool _allowingInvites = false; // Toggles invite creation functionality.
        bool _allowingJoinMsg = true;  // Toggles join and leave messages.
        bool _allowingBansMsg = true;  // Toggles ban messages.
        bool _allowingVisible = true;  // Toggles visibility in the server directory.

        // Constructor for Database I/O or complete registration process.
        public ServerProfile(List<object> incoming)
        {
            // Each of these should be self-explanatory.
            GuildId = (ulong)incoming[0];
            _name   = (string)incoming[1];

            _welcomeChannel = (ulong)incoming[2];
            _welcomeRole    = (ulong)incoming[3];

            _modChannel = (ulong)incoming[4];

            Prefix = (string)incoming[5];

            // These are boolean attributes.
            _allowingInvites = (bool)incoming[6];
            _allowingJoinMsg = (bool)incoming[7];
            _allowingBansMsg = (bool)incoming[8];
            _allowingVisible = (bool)incoming[9];
        }

        // Data pass to the Database I/O. Acts as an inverse of the DB I/O's specified constructor.
        public List<object> DataPass()
        {
            // Need to create the inverse of "incoming" from the constructor here, unfortunately.
            List<object> outgoing = new List<object>();

            // Add all values.
            outgoing.Add(GuildId);
            outgoing.Add(_name);
            outgoing.Add(_welcomeChannel);
            outgoing.Add(_welcomeRole);
            outgoing.Add(_modChannel);
            outgoing.Add(Prefix);
            outgoing.Add(_allowingInvites);
            outgoing.Add(_allowingJoinMsg);
            outgoing.Add(_allowingBansMsg);
            outgoing.Add(_allowingVisible);

            // Package is ready. Send.
            return outgoing;
        }
    }
}
