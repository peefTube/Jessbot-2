using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Rest;

using Colorful;
using Console = Colorful.Console;

using Interactivity;

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
    public class MessageService
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

        public MessageService(IServiceProvider services, DiscordSocketClient bot, DatabaseService databaseService, CommandService cmd,
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

        // Receives and handles incoming messages.
        // This will fork off into other services.
        public async Task Receiver(SocketMessage msg)
        {
            // Determine beforehand if message is from a bot.
            // This should simplify code.
            bool IsBot = msg.Author.IsBot;

            // Determine beforehand if message is from the system.
            // This should simplify code.
            bool IsSystem = false;
            if (msg is SocketSystemMessage)
            { IsSystem = true; }

            // Prepare prefix!
            // Not mandatory to make this definition
            // uppercase, but preferable specifically
            // for readability; this will have a
            // .ToLower() call on it during processing.
            // This is done early on purpose. You will
            // want to have it ready to go.
            string _prefix = "JR.";
            int _pos = 3;
            
            // Also ensure first that this is a guild message.
            // You will only use the boolean ONCE.
            bool IsGuild = false;
            if (msg.Channel is SocketGuildChannel)
            { IsGuild = true; }

            // If this is a guild, attempt to get a custom prefix.
            if (IsGuild && _db.GetGuilds().ContainsKey((msg.Channel as SocketGuildChannel).Guild.Id))
            {
                _prefix = _db.GetGuilds()[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix;
                _pos = _prefix.Length;
            }

            // Prefix prepared - it is now safe to log to console.
            Logger.MessageStep(MsgStep.Detection);

            // If user is a bot, then end functionality quickly!
            if (IsBot)
            { Logger.MessageStep(MsgStep.IsBot, IsBot); return; } // TODO: Instead of an immediate return call, have a message success call first.
            // User is not a bot. Proceed as normal.
            else
            {
                // Log to console.
                Logger.MessageStep(MsgStep.IsBot, IsBot);

                // If message is a system message, end functionality now!
                if (IsSystem)
                { Logger.MessageStep(MsgStep.IsSystem, IsSystem); return; }
                // Message is a normal message. Proceed as normal.
                else
                {
                    // Log to console.
                    Logger.MessageStep(MsgStep.IsSystem, IsSystem);
                    Logger.MessageStep(MsgStep.CheckReg, true); // This signifies that the check has started.

                    // We should check if the user is registered.
                    // If they aren't, offload that to the registry service.
                    // You will need to set up a flag, if it is true you may proceed.
                    bool IsUserReg = false;

                    #region DEPRECATED: REGISTRATION CHECK
                    /* OLD HANDLING: THIS IS WHAT **NOT** TO DO!!
                    // This loop should only run while the registration check flag
                    // is returning false. If it returns true, you will proceed normally.
                    // If it returns false, you will register the user first.
                    for (int i = 0; i < _db.GetUsers().Count && IsUserReg == false; i++)
                    { if (_db.GetUsers()[i].UserID == msg.Author.Id) { IsUserReg = true; } } */
                    #endregion

                    // Now check if the user is registered.
                    if (_db.GetUsers().ContainsKey(msg.Author.Id))
                    { IsUserReg = true; } // User is registered, they exist in the database!

                    // If the user is NOT registered, you will need to register them.
                    // Offload that to the Registration Service, then set IsUserReg true.
                    // This will use the message's author ID.
                    if (!IsUserReg)
                    { _reg.GenUserProfile(msg, _prefix) ; IsUserReg = true; }
                    // Otherwise:
                    // Log to the console that the user WAS registered. You'll have to use .Post().
                    else { Logger.Post("User was already registered."); }

                    // Log again to console.
                    Logger.MessageStep(MsgStep.CheckReg, false); // This signifies that the check has finished.

                    // Now that the user is definitely registered, offload the message functionality
                    // as necessary.
                    // Offload the message to the experience handler.
                    await _exp.CalculateFromMessage(msg, msg.Author.Id);

                    // TODO: Create an economy service.

                    // Offload the message to the command handler.
                    if ((msg as SocketUserMessage).HasStringPrefix(_prefix, ref _pos, StringComparison.OrdinalIgnoreCase))
                    {
                        var context = new CoreContext(_bot, msg as SocketUserMessage, _prefix);
                        var result = await _cmd.ExecuteAsync(context, _pos, _services);
                    }

                    // Everything is finished! Set any necessary values, 
                    // then save the database to prevent any issues.
                    _db.Save();
                }
            }
        }
    }
}
