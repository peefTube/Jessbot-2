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
using Jessbot.Services;

namespace Jessbot.Commands.Modules
{
    public sealed partial class UserModule
    {
        [Command("utccode", true)]
        [Alias("utc", "timezone", "tz")]
        [Summary("Sets user's UTC code / timezone.")]
        public async Task SetUTC(string utc)
        {
            // To shorten code, hold the user value.
            UserProfile User = Database.GetUsers()[Context.User.Id];

            // Make sure the UTC code exists and that the string is such.
            if (Jessbot.CodesUTC.Keys.Contains(utc.ToUpper()))
            { User.UserUTC = utc.ToUpper(); }
            else // Something went wrong.
            { await ReplyAsync("Uh-oh! That's not a proper UTC code. Are you sure you typed your code in the format `UTC±HH:MM`?"); return; }

            // Build the reply embed and send.
            EmbedBuilder Popup = new EmbedBuilder { Color = User.PrefColor };
            Popup.AddField("Success!", $"Your UTC code has been changed. You can see it at work in the footer.");
            Popup.Footer = new EmbedFooterBuilder { Text = $"UTC Time: {DateTimeOffset.UtcNow}\n" +
                $"New Time: {DateTimeOffset.UtcNow.ToOffset(Jessbot.CodesUTC[User.UserUTC])}" };
            await ReplyAsync("", false, Popup.Build());
        }
    }
}
