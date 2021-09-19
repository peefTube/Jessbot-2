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
        public void GenUserProfile(SocketMessage incoming, string prefix)
        {
            // Handle the incoming message's properties and store
            // them as temporary variables.
            var Auth = incoming.Author;
            var Name = Auth.Username;

            // Set some additional temporary variables
            // and renames of existing objects to make life easier.
            var RNG  = Jessbot.RNG;

            // Generate a profile for the user (incoming value).
            _db.GetUsers().Add(Auth.Id, new UserProfile(Auth.Id));

            // Once the profile is generated, let them know in DMs!
            string UserDBCount = _db.GetUsers().Count.ToString("D4");
            EmbedBuilder Notifier = new EmbedBuilder
            {
                Title = "Registered!",
                // Color = new Color(RNG.Next(128, 256), RNG.Next(128, 256), RNG.Next(128, 256)),
                Color = _db.GetUsers()[Auth.Id].PrefColor,
                Footer = new EmbedFooterBuilder { Text = $"Registered: {DateTime.Now}" }
            };
            Notifier.AddField("Hello!", $"Welcome to Jessica Bot II! You have been registered as **user #{UserDBCount}**!");
            Notifier.AddField("Where to Start", $"Here are some commands to get you started with setting up your profile!\n" +
                $"`{prefix}color <#hhhhhh>` - `#hhhhhh` should be replaced with a hexadecimal color code.\n" +
                $"`{prefix}utc <utc-00:00>` - `utc-00:00` should be replaced with your timezone's offset code.\n");
            Auth.SendMessageAsync("", false, Notifier.Build());
            
            // Log to the console.
            // This happens AFTER the profile generation to simplify code.
            Logger.UserRegistration((ulong)_db.GetUsers().Count);
        }

        // Tells the database to generate a new user profile for an already identified user.
        // Runs through this service for ease of discovery.
        public void GenUserProfile(ulong incoming, string prefix)
        {
            // Handle the incoming message's properties and store
            // them as temporary variables.
            var Auth = _bot.GetUser(incoming);
            var Name = Auth.Username;

            // Set some additional temporary variables
            // and renames of existing objects to make life easier.
            var RNG = Jessbot.RNG;

            // Generate a profile for the user (incoming value).
            _db.GetUsers().Add(Auth.Id, new UserProfile(Auth.Id));

            // Once the profile is generated, let them know in DMs!
            string UserDBCount = _db.GetUsers().Count.ToString("D4");
            EmbedBuilder Notifier = new EmbedBuilder
            {
                Title = "Registered!",
                // Color = new Color(RNG.Next(128, 256), RNG.Next(128, 256), RNG.Next(128, 256)),
                Color = _db.GetUsers()[Auth.Id].PrefColor,
                Footer = new EmbedFooterBuilder { Text = $"Registered: {DateTime.Now}" }
            };
            Notifier.AddField("Hello!", $"Welcome to Jessica Bot II! You have been registered as **user #{UserDBCount}**!");
            Notifier.AddField("Where to Start", $"Here are some commands to get you started with setting up your profile!\n" +
                $"`{prefix}color <#hhhhhh>` - `#hhhhhh` should be replaced with a hexadecimal color code.\n" +
                $"`{prefix}utc <utc-00:00>` - `utc-00:00` should be replaced with your timezone's offset code.\n");
            Auth.SendMessageAsync("", false, Notifier.Build());

            // Log to the console.
            // This happens AFTER the profile generation to simplify code.
            Logger.UserRegistration((ulong)_db.GetUsers().Count);
        }

        // Tells the database to generate a new user profile from an id.
        // Runs through this service for ease of discovery.
        public void GenUserProfile(SocketMessage incoming, string prefix, ulong target)
        {
            // Handle the incoming message's properties and store
            // them as temporary variables.
            var Auth = incoming.Author;
            var Name = Auth.Username;

            // Set some additional temporary variables
            // and renames of existing objects to make life easier.
            var RNG = Jessbot.RNG;

            // Generate a profile for the user (incoming value).
            _db.GetUsers().Add(target, new UserProfile(target));

            // Once the profile is generated, let them know in DMs!
            string UserDBCount = _db.GetUsers().Count.ToString("D4");
            EmbedBuilder Notifier = new EmbedBuilder
            {
                Title = "Registered!",
                // Color = new Color(RNG.Next(128, 256), RNG.Next(128, 256), RNG.Next(128, 256)),
                Color = _db.GetUsers()[target].PrefColor,
                Footer = new EmbedFooterBuilder { Text = $"Registered: {DateTime.Now}" }
            };
            Notifier.AddField("Hello!", $"Welcome to Jessica Bot II! You have been registered as **user #{UserDBCount}** by " +
                $"**{Auth.Username}#{Auth.Discriminator}**!");
            Notifier.AddField("Where to Start", $"Here are some commands to get you started with setting up your profile!\n" +
                $"`{prefix}color <#hhhhhh>` - `#hhhhhh` should be replaced with a hexadecimal color code.\n" +
                $"`{prefix}utc <utc-00:00>` - `utc-00:00` should be replaced with your timezone's offset code.\n");
            _bot.GetUser(target).SendMessageAsync("", false, Notifier.Build());

            // Log to the console.
            // This happens AFTER the profile generation to simplify code.
            Logger.UserRegistration((ulong)_db.GetUsers().Count);
        }
    }
}
