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
using Jessbot.Services;

namespace Jessbot.Commands.Modules
{
    public sealed partial class UserModule
    {
        [Command("color", true)]
        [Summary("Sets preferred color.")]
        public async Task SetColor(int r, int g, int b)
        {
            // This is a bit of a worthless move, why did I do this
            await SetColor(Converter.ColorToHex(new Color(r, g, b)));
        }

        [Command("color", true)]
        [Summary("Sets preferred color.")]
        public async Task SetColor(string hex)
        {
            List<object> Receipt = Converter.HexToColor(hex);
            bool DidIFail = (bool)Receipt[1]; // Store the flag for if this command failed.

            if (DidIFail) { await ReplyAsync("Oops! Something went wrong. Are you sure you gave me a proper color value?"); return; }
            else
            { Database.GetUsers()[Context.User.Id].PrefColor = (Color)Receipt[0]; }

            EmbedBuilder Popup = new EmbedBuilder { Color = Database.GetUsers()[Context.User.Id].PrefColor };
            Popup.AddField("Success!", $"Your color has been changed! You can see it on the sidebar of this embed.");
            await ReplyAsync("", false, Popup.Build());
        }
    }
}
