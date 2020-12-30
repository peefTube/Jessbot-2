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
    public class ExperienceService
    {
        // Establish some constants! You will need these for message calculations...
        // Leveling
        private readonly ulong LevelNumber = 1500; // This is the number of levels that will be generated AFTER level 0.
        private readonly Dictionary<ulong, BigInteger> LevelThresholds = new Dictionary<ulong, BigInteger>();
        private readonly BigInteger LevelBase     = 1000;
        private readonly BigInteger LevelBigAdd   = 500;
        private readonly BigInteger LevelSmallAdd = 250;

        // Base experience gain
        private readonly int GainBaseCap = 10; // Base cap. Each "tiny" message can hit up to this much EXP.
        private readonly int GainAddtCap = 5; // Additive cap. Each message size can hit up to this much added EXP.
        private readonly int GainLevlAdd = 2; // Level additive cap. Each level shifts the EXP gain by this much.

        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _bot;

        private readonly DatabaseService _db;
        private readonly CommandService _cmd;
        private readonly RegistrationService _reg;
        private readonly ParserService _parse;
        private readonly ConversionService _convert;
        
        private readonly InteractivityService _interact;

        public ExperienceService(IServiceProvider services, DiscordSocketClient bot, DatabaseService databaseService, CommandService cmd,
            RegistrationService registryService, ParserService parser, ConversionService converter, InteractivityService interactive)
        {
            _services = services;
            _bot = bot;

            _db = databaseService;
            _cmd = cmd;
            _reg = registryService;
            _parse = parser;
            _convert = converter;

            _interact = interactive;

            // Set up the level thresholds dictionary.
            LevelThresholds.Add(0, 0);
            for (ulong L = 0; L < LevelNumber; L++)
            {
                ulong LAct = L + 1; // This is the actual level number.
                
                if (LAct == 1) // If the level number is 1, set the threshold to the base increment.
                { LevelThresholds.Add(LAct, LevelBase); }
                else // The level number is greater than 1. Perform math.
                {
                    // Do the calculations...
                    BigInteger export = LevelThresholds[L]      // Grab the last level's experience threshold.
                        + LevelBase                             // Add the base increment.
                        + (LevelBigAdd * L)                     // Add the big increment multiplied by the last level number.
                        + (LevelSmallAdd * (L - 1));            // Add the small increment multiplied by two level numbers ago.
                    
                    // Calculations after level 10:
                    BigInteger exponential = 0;
                    if (LAct > 10)
                    {
                        exponential += LevelSmallAdd * (LAct / 3); // Every third level, add the small increment to the exponential increment.
                        exponential += LevelBigAdd * (LAct / 5); // Every fifth level, add big increment to the exponential increment.
                        exponential += LevelBase * (LAct / 10); // Every tenth level, add the base increment to the exponential increment.

                        exponential *= 1 + (LAct / 20); // Every twentieth level, multiply the exponential by the intervals of 20 that have passed.
                        exponential *= 1 + (LAct / 50); // Every fiftieth level, multiply the exponential by the intervals of 50 that have passed.
                    }

                    // Add the exponential to the export.
                    export += exponential;

                    // Calculation for this level complete, export to dictionary.
                    LevelThresholds.Add(LAct, export);
                }
            }
        }

        public async Task CalculateFromMessage(SocketMessage msg, ulong DBgrab)
        {
            // Grab the user profile from the database, you will need this.
            UserProfile User = _db.GetUsers()[DBgrab];

            // Get the message length, you will need this as well.
            int Length = msg.Content.Length;

            // Perform the calculations. Add 1 to all caps to include the caps.
            // All if statements add on top of the existing calculations done. Be aware of this.
            BigInteger Gain = Jessbot.RNG.Next(1, GainBaseCap + 1); // For "tiny" messages only.            (Length < 10)
            if (Length >= 10)   { Gain += Jessbot.RNG.Next(0, GainAddtCap + 1); } // "Little" messages.     (Length >= 10)
            if (Length >= 25)   { Gain += Jessbot.RNG.Next(0, GainAddtCap + 1); } // "Small" messages.      (Length >= 25)
            if (Length >= 50)   { Gain += Jessbot.RNG.Next(0, GainAddtCap + 1); } // "Medium" messages.     (Length >= 50)
            if (Length >= 100)  { Gain += Jessbot.RNG.Next(0, GainAddtCap + 1); } // "Long" messages.       (Length >= 100)
            if (Length >= 250)  { Gain += Jessbot.RNG.Next(0, GainAddtCap + 1); } // "Huge" messages.       (Length >= 250)

            // Based on user level, shift the experience gain by the cap.
            Gain += User.Level * (ulong)GainLevlAdd;

            // Calculate the level, and then complete the task.
            CalculateLevel(User, Gain); await Task.CompletedTask;
        }

        private void CalculateLevel(UserProfile User, BigInteger Gain)
        {
            // Grab the user's current level.
            ulong StartLevel = User.Level;

            // Grab current experience, then set and grab the new experience.
            BigInteger OldExp = User.Experience;
            User.Experience += Gain; BigInteger NewExp = User.Experience;

            // Check the experience values against the current level.
            if (OldExp >= LevelThresholds[StartLevel])
            { } // You are safe to proceed.
            else if (NewExp >= LevelThresholds[StartLevel])
            { } // Odd. Calculations must have changed. Proceed regardless.
                // NOTE: This comment is regarding the old experience value vs. the starting level.
            else
            {
                // Uh-oh! Something is wrong! Recalculate the level until things are within bounds.
                while (NewExp < LevelThresholds[StartLevel] && StartLevel > 0)
                { StartLevel -= 1; }

                // Now that things are within bounds, pass the new level in and finish.
                User.Level = StartLevel; return;
            }

            // Now that you are sure you are clear, run a quick while loop to ensure that the old experience
            // value and the old level value match up so you can properly analyze the information being passed in.
            while (OldExp < LevelThresholds[StartLevel] && StartLevel > 0)
            { StartLevel -= 1; }
            
            // Establish the user's current level.
            ulong NewLevel = StartLevel;

            // Calculate the actual new level information.
            while (!(NewExp < LevelThresholds[NewLevel + 1]) && NewExp >= LevelThresholds[NewLevel] && 
                (NewLevel + 1) <= (ulong)LevelThresholds.Count) // L + 1 > EXP >= L
            { NewLevel += 1; }

            // Now that the level has been calculated, pass the new level in.
            User.Level = NewLevel;

            // If, and only if, the new level is higher than the previous:
            if (NewLevel > StartLevel)
            {
                // Build a small embed showing that the user has leveled up, and send it!
                EmbedBuilder Popup = new EmbedBuilder { Color = User.PrefColor };
                Popup.AddField("Level Up!", $"You have leveled up to **Level {User.Level}**!");

                Popup.AddField("Old Stats", $"Here is an overview of your old statistics:");
                Popup.AddField("Exp", $"{OldExp}", true); Popup.AddField("Level", $"Level {StartLevel}", true);

                Popup.AddField("New Stats", $"Here is an overview of your current statistics:");
                Popup.AddField("Exp", $"{NewExp}", true); Popup.AddField("Level", $"Level {NewLevel}", true);

                _bot.GetUser(User.UserID).SendMessageAsync("", false, Popup.Build());
            }

            // Side task complete.
            return;
        }
    }
}
