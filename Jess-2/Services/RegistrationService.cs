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
    public class RegistrationService
    {
        private readonly DiscordSocketClient _bot;

        private readonly DatabaseService _db;
        private readonly CommandService _cmd;

        public RegistrationService(DiscordSocketClient bot, DatabaseService databaseService, CommandService cmd)
        {
            _bot = bot;

            _db = databaseService;
            _cmd = cmd;
        }

        // Tells the database to generate a new user profile.
        // Runs through this service for ease of discovery.
        public void GenUserProfile(ulong id)
        {
            _db.GetUsers().Add(new UserProfile(id));

            // Log to the console.
            // This happens AFTER the profile generation to simplify code.
            Logger.UserRegistration((ulong)_db.GetUsers().Count);
        }
    }
}
