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
        bool _profileERtoDEF = false;

        [Command("profile", true)]
        [Alias("prof", "p")]
        [Summary("Profile viewing tool.")]
        public async Task ProfileViewer(string page = null)
        {
            // Set up some constants!
            string JB = Jessbot.JB;

            // Set up user details!
            // NOTE: This is for the message author.
            UserProfile User = Database.GetUsers()[Context.User.Id];
            ulong UserID = User.UserID;
            Color UserPrefColor = User.PrefColor;
            SocketUser RawUser = Bot.GetUser(UserID);
            string UserName = RawUser.Username;
            string UserNameFull = RawUser.Username + "#" + RawUser.Discriminator;
            string UserUTC = User.UserUTC;
            DateTimeOffset UserTime = DateTimeOffset.UtcNow.ToOffset(Jessbot.CodesUTC[UserUTC]);

            // Make an EmbedBuilder to send the profile through!
            EmbedBuilder ProfEmbed = new EmbedBuilder();

            // Before generating the embed through a switch case,
            // you should ensure that page first has a value, and is passed in the lowercase.
            if (page == null) { page = ""; }
            switch (page.ToLower())
            {
                case "experience":
                case "exp":
                    ExpProfile UserExpData = (ExpProfile)User.GetSubProf(SubProfs.Exp);

                    ProfEmbed.Title = $"User Profile: {UserName} | Experience";

                    // Get all the experience data.
                    Dictionary<ulong, bool> ExpProperties = UserExpData.Properties;
                    Dictionary<ulong, bool> ExpUnlocks    = UserExpData.Unlocks;

                    // First line.
                    ProfEmbed.AddField("Exp", $"{User.Experience} EXP", true);
                    ProfEmbed.AddField("Level", $"Level {User.Level}", true);

                    // Prepare the profile itself.
                    string PropertiesString = "";
                    string UnlocksString    = "";

                    foreach (ulong p in ExpProperties.Keys)
                    {
                        // Prepare the string.
                        string has = "You do not own this.";
                        if (ExpProperties[p])
                        { has = "You own this."; }

                        PropertiesString += $"Booster #{(p + 1).ToString("D2")} `XB{p.ToString("D2")}` - `{has}`\n";
                    }

                    foreach (ulong u in ExpUnlocks.Keys)
                    {
                        // Prepare the string.
                        string has = "You have not unlocked this.";
                        if (ExpUnlocks[u])
                        { has = "You have unlocked this."; }

                        UnlocksString += $"Unlock: Level {u} - `{has}`\n";
                    }

                    ProfEmbed.AddField("Boosters", PropertiesString, false);
                    ProfEmbed.AddField("Unlocks", UnlocksString, false);

                    // Last lines, these are necessary to perform here.
                    ProfEmbed.AddField("Main Pages", $"**Overview\nExperience\nEconomy\nInventory**");
                    ProfEmbed.AddField("Subpages", $"N/A");
                    break;
                case "economy":
                case "econ":
                    EconProfile UserEconData = (EconProfile)User.GetSubProf(SubProfs.Econ);

                    ProfEmbed.Title = $"User Profile: {UserName} | Economy";

                    // Get all the economy data.
                    Dictionary<ulong, bool> EconProperties = UserEconData.Properties;
                    Dictionary<ulong, bool> EconTiers      = UserEconData.Tiers;

                    // First line.
                    ProfEmbed.AddField("Balance", $"{JB}{User.Balance}", true);

                    // Prepare the profile itself.
                    PropertiesString   = "";
                    string TiersString = "";

                    foreach (ulong p in EconProperties.Keys)
                    {
                        // Prepare the string.
                        string has = "You do not own this.";
                        if (EconProperties[p])
                        { has = "You own this."; }

                        PropertiesString += $"Booster #{(p + 1).ToString("D2")} `CB{p.ToString("D2")}` - `{has}`\n";
                    }

                    foreach (ulong t in EconTiers.Keys)
                    {
                        // Prepare the string.
                        string has = "You have not unlocked this.";
                        if (EconTiers[t])
                        { has = "You have unlocked this."; }

                        TiersString += $"Tier {t} - `{has}`\n";
                    }

                    ProfEmbed.AddField("Boosters", PropertiesString, false);
                    ProfEmbed.AddField("Tiers", TiersString, false);

                    // Last lines, these are necessary to perform here.
                    ProfEmbed.AddField("Main Pages", $"**Overview\nExperience\nEconomy\nInventory**");
                    ProfEmbed.AddField("Subpages", $"N/A");
                    break;
                case "inventory":
                case "inv":
                    InvProfile UserInventory = (InvProfile)User.GetSubProf(SubProfs.Inv);

                    ProfEmbed.Title = $"User Profile: {UserName} | Inventory";

                    // Last lines, these are necessary to perform here.
                    ProfEmbed.AddField("Main Pages", $"**Overview\nExperience\nEconomy\nInventory**");
                    ProfEmbed.AddField("Subpages", $"N/A");
                    break;
                case "overview":
                case "default":
                case "base":
                case "home":
                case "":
                    ProfEmbed.Title = $"User Profile: {UserName} | Overview";

                    // First line.
                    ProfEmbed.AddField("Exp", $"{User.Experience} EXP", true);
                    ProfEmbed.AddField("Level", $"Level {User.Level}", true);
                    ProfEmbed.AddField("Balance", $"{JB}{User.Balance}", true);

                    // Middle lines.
                    ProfEmbed.AddField("Preferred Color", $"{Converter.ColorToHex(UserPrefColor)}", false);
                    ProfEmbed.AddField("UTC Code", $"{UserUTC}", true); ProfEmbed.AddField("Your Time", $"{UserTime}", true);

                    // Last line, this is necessary to perform here.
                    ProfEmbed.AddField("Main Pages", $"**Overview\nExperience\nEconomy\nInventory**");
                    break;
                default:
                    // User did a dum-dum
                    ProfEmbed.Title = $"User Profile: {UserName} | Unknown";
                    ProfEmbed.AddField("User Error", $"Uh-oh! I don't know of any section called **{page.ToLower()}**. " +
                        $"Please select from the pages below.");

                    // Last line, this is necessary to perform here.
                    ProfEmbed.AddField("Main Pages", $"**Overview\nExperience\nEconomy\nInventory**");
                    break;
            }

            // Quick, check if the command has errored into this Task!
            string DidIError = "";
            if (_profileERtoDEF)
            { DidIError = "Uh-oh! You did something wrong. That's okay, though, I'll show you your profile instead."; _profileERtoDEF = false; }

            // Final elements.
            ProfEmbed.ThumbnailUrl = RawUser.GetAvatarUrl();
            ProfEmbed.Color = UserPrefColor;
            ProfEmbed.Author = new EmbedAuthorBuilder { Name = $"Data request by: {UserNameFull}", IconUrl = RawUser.GetAvatarUrl() };
            ProfEmbed.Footer = new EmbedFooterBuilder { Text = $"Data collected at {UserTime}" };
            await ReplyAsync(DidIError, false, ProfEmbed.Build());
        }

        [Command("profile", true)]
        [Alias("prof", "p")]
        [RequireContext(ContextType.Guild)]
        [Summary("Profile viewing tool.")]
        public async Task ProfileViewer(SocketGuildUser target = null, string page = null)
        {
            // Set up some constants!
            string JB = Jessbot.JB;

            // Set up user details!
            // NOTE: This is for the message author.
            //       If you are grabbing someone else's profile,
            //       you will be using "TargetUser" data instead.
            UserProfile User = Database.GetUsers()[Context.User.Id];
            ulong UserID = User.UserID;
            Color UserPrefColor = User.PrefColor;
            SocketUser RawUser = Bot.GetUser(UserID);
            string UserName = RawUser.Username;
            string UserNameFull = RawUser.Username + "#" + RawUser.Discriminator;
            string UserUTC = User.UserUTC;
            DateTimeOffset UserTime = DateTimeOffset.UtcNow.ToOffset(Jessbot.CodesUTC[UserUTC]);

            // Set up target user details!
            // NOTE: This is for the message author's target.
            //       If you are grabbing your own profile,
            //       you will be defaulting to another task instead.
            // NOTE: Default to null. This will (hopefully) not break.
            UserProfile TargetUser = null;
            ulong TargetUserID = 0;
            Color TargetUserPrefColor = new Color(0, 0, 0);
            SocketUser RawTargetUser = null;
            string TargetUserName = null;
            string TargetUserNameFull = null;
            string TargetUserUTC = null;
            DateTimeOffset TargetUserTime = DateTimeOffset.UtcNow;

            if (target == null || target.IsBot)
            { _profileERtoDEF = true; await ProfileViewer(page); return; }
            else
            {
                // Make sure the TargetUser exists in the database!
                if (!Database.GetUsers().Keys.Contains(target.Id))
                { RegService.GenUserProfile(Context.Message, Context.Prefix, target.Id); }

                TargetUser = Database.GetUsers()[target.Id];
                TargetUserID = TargetUser.UserID;
                TargetUserPrefColor = TargetUser.PrefColor;
                RawTargetUser = Bot.GetUser(TargetUserID);
                TargetUserName = RawTargetUser.Username;
                TargetUserNameFull = RawTargetUser.Username + "#" + RawTargetUser.Discriminator;
                TargetUserUTC = TargetUser.UserUTC;
                TargetUserTime = DateTimeOffset.UtcNow.ToOffset(Jessbot.CodesUTC[TargetUserUTC]);
            }

            // Make sure this is not the author.
            if (TargetUserID == UserID)
            { await ProfileViewer(page); return; }

            // Make an EmbedBuilder to send the profile through!
            EmbedBuilder ProfEmbed = new EmbedBuilder();            
            
            // Before generating the embed through a switch case,
            // you should ensure that page first has a value, and is passed in the lowercase.
            if (page == null) { page = ""; }
            switch (page.ToLower())
            {
                case "experience":
                case "exp":
                    ExpProfile TargetUserExpData = (ExpProfile)TargetUser.GetSubProf(SubProfs.Exp);

                    ProfEmbed.Title = $"User Profile: {TargetUserName} | Experience";

                    // Get all the experience data.
                    Dictionary<ulong, bool> ExpProperties = TargetUserExpData.Properties;
                    Dictionary<ulong, bool> ExpUnlocks = TargetUserExpData.Unlocks;

                    // First line.
                    ProfEmbed.AddField("Exp", $"{TargetUser.Experience} EXP", true);
                    ProfEmbed.AddField("Level", $"Level {TargetUser.Level}", true);

                    // Prepare the profile itself.
                    string PropertiesString = "";
                    string UnlocksString = "";

                    foreach (ulong p in ExpProperties.Keys)
                    {
                        // Prepare the string.
                        string has = "You do not own this.";
                        if (ExpProperties[p])
                        { has = "You own this."; }

                        PropertiesString += $"Booster #{(p + 1).ToString("D2")} `XB{p.ToString("D2")}` - `{has}`\n";
                    }

                    foreach (ulong u in ExpUnlocks.Keys)
                    {
                        // Prepare the string.
                        string has = "You have not unlocked this.";
                        if (ExpUnlocks[u])
                        { has = "You have unlocked this."; }

                        UnlocksString += $"Unlock: Level {u} - `{has}`\n";
                    }

                    ProfEmbed.AddField("Boosters", PropertiesString, false);
                    ProfEmbed.AddField("Unlocks", UnlocksString, false);

                    // Last lines, these are necessary to perform here.
                    ProfEmbed.AddField("Main Pages", $"**Overview\nExperience\nEconomy\nInventory**");
                    ProfEmbed.AddField("Subpages", $"N/A");
                    break;
                case "economy":
                case "econ":
                    EconProfile TargetUserEconData = (EconProfile)TargetUser.GetSubProf(SubProfs.Econ);

                    ProfEmbed.Title = $"User Profile: {TargetUserName} | Economy";

                    // Get all the economy data.
                    Dictionary<ulong, bool> EconProperties = TargetUserEconData.Properties;
                    Dictionary<ulong, bool> EconTiers      = TargetUserEconData.Tiers;

                    // First line.
                    ProfEmbed.AddField("Balance", $"{JB}{TargetUser.Balance}", true);

                    // Prepare the profile itself.
                    PropertiesString   = "";
                    string TiersString = "";

                    foreach (ulong p in EconProperties.Keys)
                    {
                        // Prepare the string.
                        string has = "You do not own this.";
                        if (EconProperties[p])
                        { has = "You own this."; }

                        PropertiesString += $"Booster #{(p + 1).ToString("D2")} `CB{p.ToString("D2")}` - `{has}`\n";
                    }

                    foreach (ulong t in EconTiers.Keys)
                    {
                        // Prepare the string.
                        string has = "You have not unlocked this.";
                        if (EconTiers[t])
                        { has = "You have unlocked this."; }

                        TiersString += $"Tier {t} - `{has}`\n";
                    }

                    ProfEmbed.AddField("Boosters", PropertiesString, false);
                    ProfEmbed.AddField("Tiers", TiersString, false);

                    // Last lines, these are necessary to perform here.
                    ProfEmbed.AddField("Main Pages", $"**Overview\nExperience\nEconomy\nInventory**");
                    ProfEmbed.AddField("Subpages", $"N/A");
                    break;
                case "inventory":
                case "inv":
                    InvProfile TargetUserInventory = (InvProfile)TargetUser.GetSubProf(SubProfs.Inv);

                    ProfEmbed.Title = $"User Profile: {TargetUserName} | Inventory";

                    // Last lines, these are necessary to perform here.
                    ProfEmbed.AddField("Main Pages", $"**Overview\nExperience\nEconomy\nInventory**");
                    ProfEmbed.AddField("Subpages", $"N/A");
                    break;
                case "overview":
                case "default":
                case "base":
                case "home":
                case "":
                    ProfEmbed.Title = $"User Profile: {TargetUserName} | Overview";

                    // First line.
                    ProfEmbed.AddField("Exp", $"{TargetUser.Experience} EXP", true);
                    ProfEmbed.AddField("Level", $"Level {TargetUser.Level}", true);
                    ProfEmbed.AddField("Balance", $"{JB}{TargetUser.Balance}", true);

                    // Middle lines.
                    ProfEmbed.AddField("Preferred Color", $"{Converter.ColorToHex(TargetUserPrefColor)}", false);
                    ProfEmbed.AddField("UTC Code", $"{TargetUserUTC}", true); ProfEmbed.AddField("Their Time", $"{TargetUserTime}", true);

                    // Last line, this is necessary to perform here.
                    ProfEmbed.AddField("Main Pages", $"**Overview\nExperience\nEconomy\nInventory**");
                    break;
                default:
                    // User did a dum-dum
                    ProfEmbed.Title = $"User Profile: {TargetUserName} | Unknown";
                    ProfEmbed.AddField("User Error", $"Uh-oh! I don't know of any section called **{page.ToLower()}**. " +
                        $"Please select from the pages below.");

                    // Last line, this is necessary to perform here.
                    ProfEmbed.AddField("Main Pages", $"**Overview\nExperience\nEconomy\nInventory**");
                    break;
            }
          
            // Final elements.
            ProfEmbed.ThumbnailUrl = RawTargetUser.GetAvatarUrl();
            ProfEmbed.Color = TargetUserPrefColor;
            ProfEmbed.Author = new EmbedAuthorBuilder { Name = $"Data request by: {UserNameFull}", IconUrl = RawUser.GetAvatarUrl() };
            ProfEmbed.Footer = new EmbedFooterBuilder { Text = $"Data collected at {UserTime}" };
            await ReplyAsync("", false, ProfEmbed.Build());
        }
    }
}
