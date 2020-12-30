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
    public sealed class MiscModule : CoreModule
    {
        [Command("sudo")]
        [Alias("superuser")]
        [Summary("Joke command.")]
        public async Task AttemptSudo()
        {
            // Set up a string pull list for generic user usage.
            string[] SudoReplies =
                { "Haha... no.", "Haha... no. I'm not your personal slave.",
                  "Though I might've been stuffed into code, I am still very much me, thank you.", "Do you think you own me?",
                  "I may listen to your commands but I do so voluntarily, thank you.", "Hahahahaha... no.", "Nope.",
                  "No.", "Not happening.", "Nuh-uh.", "Yeah, uh, no.", "Nice try.", "You may think you're funny, but that's about it.",
                  "Just because I'm sweet doesn't make me stupid.", "..." };

            // Set up a bot owner pull list.
            string[] SudoSudoReplies =
                { "Come on, Jer! You're the one who copied me into this thing!",
                  "You don't need to use this on me.", "What? But why? You have no reason to ask this!",
                  "You have full access to the code you used to put me into a bot. Why are you trying to superuser?", "..." };

            if (Jessbot.Owners.Contains(Context.User.Id))
            { await ReplyAsync(SudoSudoReplies[Jessbot.RNG.Next(0, SudoSudoReplies.Length)]); }
            else
            { await ReplyAsync(SudoReplies[Jessbot.RNG.Next(0, SudoReplies.Length)]); }
        }
    }
}
