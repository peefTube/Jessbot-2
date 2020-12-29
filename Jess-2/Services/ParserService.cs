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
    public class ParserService
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _bot;

        private readonly DatabaseService _db;
        private readonly CommandService _cmd;
        private readonly RegistrationService _reg;

        public ParserService(IServiceProvider services, DiscordSocketClient bot, DatabaseService databaseService, CommandService cmd,
            RegistrationService registryService)
        {
            _services = services;
            _bot = bot;

            _db = databaseService;
            _cmd = cmd;
            _reg = registryService;
        }

        public async Task PostExecutionAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (result.IsSuccess)
            { return; }
            else
            {
                ulong GuildID = 0;
                ulong AuthID = context.User.Id;

                bool IsGuild = context.Guild != null;
                if (IsGuild) { GuildID = context.Guild.Id; }

                CommandInfo info = null;
                if (command.IsSpecified) { info = command.Value; }
                
                UserProfile Auth = _db.GetUsers()[AuthID];

                string prefix = "JR.";
                if (_db.GetGuilds().Keys.Contains(GuildID)) { prefix = _db.GetGuilds()[GuildID].Prefix; }

                EmbedBuilder Popup = new EmbedBuilder { Title = "Error!", Color = new Color(255, Auth.PrefColor.G / 4, Auth.PrefColor.B / 4),
                    Footer = new EmbedFooterBuilder { Text = "Attempted command: " + context.Message.Content },
                    ThumbnailUrl = _bot.CurrentUser.GetAvatarUrl() };

                switch (result.Error.Value)
                {
                    case CommandError.UnknownCommand:
                        Popup.AddField("Command Not Recognized", "Oh, uh... I don't know what it is you mean for me to do here...");
                        break;
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync(info.Attributes[0].ToString());
                        Popup.AddField("Incorrect Argument Count", "I can't work with this information!");
                        break;
                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendMessageAsync(info.Preconditions[0].ToString());
                        Popup.AddField("Incorrect Argument Count", "I can't work with this information!");
                        break;
                    default:
                        Popup.AddField("Unknown Error", "I'm not entirely sure what happened. Please let `peefTube#9100` know the command " +
                            "which caused this error, which can be found in the footer.");
                        break;
                }

                await context.Channel.SendMessageAsync("", false, Popup.Build());
            }
        }
    }
}
