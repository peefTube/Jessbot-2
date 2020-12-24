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
    partial class DatabaseService
    {
        private List<ServerProfile> _guilds = new List<ServerProfile>();
        private List<UserProfile> _users = new List<UserProfile>();

        // Get full list of guilds on a whim.
        public List<ServerProfile> GetGuilds()
        { return _guilds; }

        // Get full list of users on a whim.
        public List<UserProfile> GetUsers()
        { return _users; }
    }
}
