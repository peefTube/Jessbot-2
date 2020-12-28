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
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Jessbot.Services;

namespace Jessbot
{
    // A list of hardcoded startup settings.
    // These are meant to be used explicitly to control bot function on startup.
    // WARNING: These WILL cause overwriting and damage of internal files.
    //          Use sparingly.
    public partial class Jessbot
    {
        private static bool _load = true; // Should I load normally?

        // _load subtypes, meant specifically to control certain load functions.
        public static bool _loadGuilds = true;  // Should I load guilds in?
        public static bool _loadUsers  = true;  // Should I load users in?
    }
}
