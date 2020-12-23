using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Rest;

using System;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Numerics;

using Microsoft.Extensions.DependencyInjection;

/* ======================================== *
 *                                          *
 *    CODED BY PEEFTUBE (JEREMY PEIFER)     *
 *    =================================     *
 *                                          *
 *             JESSICA BOT V2               *
 *             FOR DISCORD.NET              *
 *                                          *
 * ======================================== */

namespace Jessbot
{
    class Jessbot
    {
        // Version information
        #region VERSION DATA

        static readonly string JessConstantVersion = "v1.99.0.000";
        static readonly string JessConstantVersionDate = "December 23, 2020";
        static readonly string JessConstantVersionName = "The Rebirth Update (Pre-Release)";
        static readonly string JessConstantVersionInfo = $"This update ( **\"{JessConstantVersionName}\"** ), {JessConstantVersion}, updated on {JessConstantVersionDate}, " + "restarted development of JessBot on the Discordant placeholder bot to clean up code and hopefully make the bot more effective and updatable.";

        #endregion
    }
}
