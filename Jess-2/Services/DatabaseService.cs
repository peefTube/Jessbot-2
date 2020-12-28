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

namespace Jessbot.Services
{
    public partial class DatabaseService
    {
        private Dictionary<ulong, ServerProfile> _guilds = new Dictionary<ulong, ServerProfile>();
        private Dictionary<ulong, UserProfile> _users    = new Dictionary<ulong, UserProfile>();

        private readonly DiscordSocketClient _bot;

        // Builds the database from the DI.
        public DatabaseService(DiscordSocketClient bot)
        {
            _bot = bot;
        }

        // Get full list of guilds on a whim.
        public Dictionary<ulong, ServerProfile> GetGuilds()
        { return _guilds; }

        // Get full list of users on a whim.
        public Dictionary<ulong, UserProfile> GetUsers()
        { return _users; }
    }
}
