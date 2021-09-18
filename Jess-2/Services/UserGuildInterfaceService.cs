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
using Jessbot.Commands.Modules;
using Jessbot.Commands;

namespace Jessbot.Services
{
    class UserGuildInterfaceService
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _bot;

        private readonly DatabaseService _db;
        private readonly CommandService _cmd;
        private readonly RegistrationService _reg;
        private readonly ParserService _parse;
        private readonly ConversionService _convert;

        private readonly ExperienceService _exp;
        private readonly EconomyService _econ;
        private readonly InventoryService _inv;

        public UserGuildInterfaceService(IServiceProvider services, DiscordSocketClient bot, DatabaseService databaseService, CommandService cmd,
            RegistrationService registryService, ParserService parser, ConversionService converter, ExperienceService exp, EconomyService econ,
            InventoryService inv)
        {
            _services = services;
            _bot = bot;

            _db = databaseService;
            _cmd = cmd;
            _reg = registryService;
            _parse = parser;
            _convert = converter;

            _exp = exp;
            _econ = econ;
            _inv = inv;
        }

        // User has joined.
        public async Task Joined(SocketGuildUser user)
        {
            ServerProfile _guild;
            if (_db.GetGuilds().ContainsKey(user.Guild.Id))
            { _guild = _db.GetGuilds()[user.Guild.Id]; }
            else
            { return; }
            ulong _msgChannelRaw = _guild.WelcomeChannel;
            var RNG = Jessbot.RNG;

            // First, check if the user is a bot.
            // If they are, skip user registration functionality.
            if (user.IsBot)
            {
                if (_msgChannelRaw != 0 && _bot.GetGuild(user.Guild.Id).TextChannels.Contains(_bot.GetChannel(_msgChannelRaw)) && _guild.AllowingJoinMsg)
                {
                    SocketTextChannel _msgChannel = _bot.GetGuild(user.Guild.Id).GetTextChannel(_msgChannelRaw);

                    // Set up embed.
                    EmbedBuilder Entry = new EmbedBuilder
                    {
                        Title = $"New Bot: {user.Username}#{user.Discriminator}!",
                        Color = new Color(128, 128, 128),
                        Footer = new EmbedFooterBuilder { Text = $"Joined: {DateTimeOffset.UtcNow}" }
                    };
                    Entry.AddField("Hello!", $"Welcome to **{user.Guild.Name}**! Please enjoy your stay - we are eager to use your services!");
                    await _msgChannel.SendMessageAsync("", false, Entry.Build());
                }
                else
                {
                    // Set up embed.
                    EmbedBuilder Entry = new EmbedBuilder
                    {
                        Title = $"New Bot: {user.Username}#{user.Discriminator}!",
                        Color = new Color(128, 128, 128),
                        Footer = new EmbedFooterBuilder { Text = $"Joined: {DateTimeOffset.UtcNow}" }
                    };
                    Entry.AddField("Hello!", $"Welcome to **{user.Guild.Name}**! Please enjoy your stay - we are eager to use your services!");
                    await user.SendMessageAsync("", false, Entry.Build());
                }
            }
            // User is not a bot.
            else
            {
                // Make sure this user is registered to avoid issues.
                if (!_db.GetUsers().Keys.Contains(user.Id))
                { _reg.GenUserProfile(user.Id, "JR."); }

                // Set up embed.
                EmbedBuilder Entry = new EmbedBuilder
                {
                    Title = $"Now Entering: {user.Username}#{user.Discriminator}!",
                    Color = _db.GetUsers()[user.Id].PrefColor,
                    Footer = new EmbedFooterBuilder { Text = $"Joined: {DateTimeOffset.UtcNow}" }
                };
                Entry.AddField("Hello!", $"Welcome to **{user.Guild.Name}**! Please enjoy your stay.");
                if (_guild.RequiredReads.Count != 0)
                {
                    string _reqread = "";

                    for (int i = 0; i < _guild.RequiredReads.Count; i++)
                    {
                        _reqread += $"{user.Guild.GetChannel(_guild.RequiredReads[i])}\n";
                    }

                    Entry.AddField("Required Reading", "Server administration requires that you read the following channels:\n" +
                    _reqread);
                }

                if (_msgChannelRaw != 0 && _bot.GetGuild(user.Guild.Id).TextChannels.Contains(_bot.GetChannel(_msgChannelRaw)) && _guild.AllowingJoinMsg)
                {
                    SocketTextChannel _msgChannel = _bot.GetGuild(user.Guild.Id).GetTextChannel(_msgChannelRaw);
                    await _msgChannel.SendMessageAsync("", false, Entry.Build());
                }
                else
                {
                    await user.SendMessageAsync("", false, Entry.Build());
                }
            }

            // Send a message to the moderation channel, if the server has one.
            if (_guild.ModChannel != 0 && _bot.GetGuild(user.Guild.Id).TextChannels.Contains(_bot.GetChannel(_guild.ModChannel)))
            {
                SocketTextChannel _msgChannel = _bot.GetGuild(user.Guild.Id).GetTextChannel(_guild.ModChannel);
                await _msgChannel.SendMessageAsync($"{user.Username}#{user.Discriminator} joined at {DateTimeOffset.UtcNow}.");
            }
        }

        // User was banned.
        public async Task Banned(SocketUser user, SocketGuild guild)
        {
            ServerProfile _guild;
            if (_db.GetGuilds().ContainsKey(guild.Id))
            { _guild = _db.GetGuilds()[guild.Id]; }
            else
            { return; }
            ulong _msgChannelRaw = _guild.WelcomeChannel;
            
            // No longer works!
            // SocketGuildUser fulluser = guild.GetUser(user.Id);
            
            string presentAlias = null;
            try
            { presentAlias = _db.GetUsers()[user.Id].AliasList[guild.Id]; }
            catch (Exception)
            { presentAlias = null; }

            // Set up embed.
            EmbedBuilder Banhammer = new EmbedBuilder
            {
                Title = $"Banned: {user.Username}#{user.Discriminator}",
                Color = new Color(255, 0, 0),
                Footer = new EmbedFooterBuilder { Text = $"Banned: {DateTimeOffset.UtcNow}" }
            };

            if (presentAlias != null)
            { Banhammer.AddField("Hammer Time!", $"**{presentAlias}** has been... let's put it this way: forcibly, but justifiably, removed."); }
            else
            { Banhammer.AddField("Hammer Time!", $"**{user.Username}** has been... let's put it this way: forcibly, but justifiably, removed."); }

            if (_msgChannelRaw != 0 && _bot.GetGuild(guild.Id).TextChannels.Contains(_bot.GetChannel(_msgChannelRaw)) && _guild.AllowingBansMsg)
            {
                SocketTextChannel _msgChannel = _bot.GetGuild(guild.Id).GetTextChannel(_msgChannelRaw);
                await _msgChannel.SendMessageAsync("", false, Banhammer.Build());
            }

            if (!user.IsBot)
            { await user.SendMessageAsync($"You were banned from **{guild.Name}**. Reason for ban: {(await guild.GetBanAsync(user)).Reason}"); }

            // Send a message to the moderation channel, if the server has one.
            if (_guild.ModChannel != 0 && _bot.GetGuild(guild.Id).TextChannels.Contains(_bot.GetChannel(_guild.ModChannel)))
            {
                SocketTextChannel _msgChannel = _bot.GetGuild(guild.Id).GetTextChannel(_guild.ModChannel);
                await _msgChannel.SendMessageAsync($"**{user.Username}#{user.Discriminator} was banned! Please read the below messages for further details.**\n" +
                    $"Reason for ban: {(await guild.GetBanAsync(user)).Reason}");
            }
        }

        // User has voluntarily left.
        public async Task Left(SocketGuildUser user)
        {
            ServerProfile _guild;
            if (_db.GetGuilds().ContainsKey(user.Guild.Id))
            { _guild = _db.GetGuilds()[user.Guild.Id]; }
            else
            { return; }
            ulong _msgChannelRaw = _guild.WelcomeChannel;

            string presentAlias = null;
            try
            { presentAlias = _db.GetUsers()[user.Id].AliasList[user.Guild.Id]; }
            catch (Exception)
            { presentAlias = null; }

            var isBanned = user.Guild.GetBanAsync(user).Result;
            if (isBanned == null)
            {
                Color embedColor;
                if (user.IsBot)
                { embedColor = new Color(128, 128, 128); }
                else
                { embedColor = _db.GetUsers()[user.Id].PrefColor; }

                // Set up embed.
                EmbedBuilder Leaving = new EmbedBuilder
                {
                    Title = $"Leaving: {user.Username}#{user.Discriminator}",
                    Color = embedColor,
                    Footer = new EmbedFooterBuilder { Text = $"Left: {DateTimeOffset.UtcNow}" }
                };

                if (presentAlias != null)
                { Leaving.AddField("Farewell!", $"**{presentAlias}** has decided to leave."); }
                else
                { Leaving.AddField("Farewell!", $"**{user.Username}** has decided to leave."); }

                if (_msgChannelRaw != 0 && _bot.GetGuild(user.Guild.Id).TextChannels.Contains(_bot.GetChannel(_msgChannelRaw)) && _guild.AllowingJoinMsg)
                {
                    SocketTextChannel _msgChannel = _bot.GetGuild(user.Guild.Id).GetTextChannel(_msgChannelRaw);
                    await _msgChannel.SendMessageAsync("", false, Leaving.Build());
                }
            }

            // Send a message to the moderation channel, if the server has one.
            if (_guild.ModChannel != 0 && _bot.GetGuild(user.Guild.Id).TextChannels.Contains(_bot.GetChannel(_guild.ModChannel)))
            {
                SocketTextChannel _msgChannel = _bot.GetGuild(user.Guild.Id).GetTextChannel(_guild.ModChannel);
                if (presentAlias == null)
                { await _msgChannel.SendMessageAsync($"{user.Username}#{user.Discriminator} left at {DateTimeOffset.UtcNow}."); }
                else
                {
                    await _msgChannel.SendMessageAsync($"{user.Username}#{user.Discriminator} left at {DateTimeOffset.UtcNow}.\n" +
                        $"They had the nickname '{presentAlias}'.");
                }
            }
        }
    }
}
