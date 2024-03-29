﻿using Discord;
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
using Jessbot.Commands.Modules;
using Jessbot.Commands;

namespace Jessbot.Services
{
    public class NamingService
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _bot;

        private readonly DatabaseService _db;
        private readonly CommandService _cmd;
        private readonly RegistrationService _reg;
        private readonly ParserService _parse;
        private readonly ConversionService _convert;

        public NamingService(IServiceProvider services, DiscordSocketClient bot, DatabaseService databaseService, CommandService cmd,
            RegistrationService registryService, ParserService parser, ConversionService converter)
        {
            _services = services;
            _bot = bot;

            _db = databaseService;
            _cmd = cmd;
            _reg = registryService;
            _parse = parser;
            _convert = converter;
        }

        public void RefreshNameData()
        {
            Dictionary<ulong, UserProfile> userlist = _db.GetUsers();
            foreach (ulong quid in userlist.Keys)
            {
                try
                { userlist[quid].Username = _bot.GetUser(quid).Username + "#" + _bot.GetUser(quid).Discriminator; }
                catch (Exception)
                { userlist[quid].Username = "unidentified#0000"; }
            }
        }

        public void RefreshAliasList()
        {
            // Userlist.
            Dictionary<ulong, UserProfile> userlist = _db.GetUsers();

            // Run through all registered users.
            foreach (ulong quid in userlist.Keys)
            {
                // Temporary nickname list.
                Dictionary<ulong, string> tempAliasList = new Dictionary<ulong, string>();

                // Run through all guilds.
                // Definition: qg is "query guild"
                foreach (SocketGuild qg in _bot.Guilds)
                {
                    SocketGuildUser thisuser = null;

                    foreach (SocketGuildUser user in qg.Users)
                    {
                        if (user.Id == quid)
                        { thisuser = user; break; }
                        else
                        { thisuser = null; }
                    }

                    if (thisuser != null)
                    {
                        try
                        { tempAliasList.Add(qg.Id, thisuser.Nickname); }
                        catch (Exception)
                        { tempAliasList.Add(qg.Id, null); }
                    }
                }

                // Set the alias list for this user.
                userlist[quid].AliasList = tempAliasList;
            }
        }
    }
}
