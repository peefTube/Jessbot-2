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
    class MessageService
    {
        private readonly DiscordSocketClient _bot;

        private readonly DatabaseService _db;
        private readonly CommandService _cmd;
        private readonly CommandHandlingService _commandService;

        public MessageService(DiscordSocketClient bot, DatabaseService databaseService, CommandService cmd, CommandHandlingService commandHandlingService)
        {
            _bot = bot;

            _db = databaseService;
            _cmd = cmd;
            _commandService = commandHandlingService;
        }

        // Receives and handles incoming messages.
        // This will fork off into other services.
        public async Task Receiver(SocketMessage incoming)
        {
            // Prepare handling system.
            // This first definition is redundant but this is on purpose.
            SocketMessage msg = incoming;
            string prc = incoming.Content.ToLower();

            // Determine beforehand if message is from a bot.
            // This should simplify code.
            bool IsBot = msg.Author.IsBot;

            // Prepare prefix.
            // Not mandatory to make this definition
            // uppercase, but preferable specifically
            // for readability; this will have a
            // .ToLower() call on it during processing.
            string _prefix = "JR.";

            for (int i = 0; i < _db.GetGuilds().Count; i++)
            { if (_db.GetGuilds()[i].GuildId == (msg.Channel as SocketGuildChannel).Guild.Id) { _prefix = _db.GetGuilds()[i].Prefix; } }

            // Prefix prepared - it is now safe to log to console.
            Logger.MessageStep(MsgStep.Detection);

            // If user is a bot, then end functionality quickly!
            if (IsBot)
            { Logger.MessageStep(MsgStep.IsBot, IsBot); return; }
            // User is not a bot. Proceed as normal.
            else
            {
                Logger.MessageStep(MsgStep.IsBot, IsBot);
            }
        }
    }
}
