﻿using Discord;
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
using Jessbot.Services;

namespace Jessbot.Commands
{
    // This will establish a base module. You will need to pass the ENTIRETY of the Dependency Injector into this.
    public abstract class CoreModule : ModuleBase<CoreContext>
    {
        // DI passes these in automatically.
        public DiscordSocketClient Bot { get; set; }
        public DatabaseService Database { get; set; }
        public MessageService MessageService { get; set; }
        public RegistrationService RegService { get; set; }
        public ConversionService Converter { get; set; }
        public InteractivityService Interaction { get; set; }
    }
}
