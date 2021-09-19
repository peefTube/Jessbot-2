using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Rest;

using Colorful;
using Console = Colorful.Console;

using Interactivity;
using Interactivity.Confirmation;

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
    public sealed partial class AdminModule
    {
        [Command("regserver", true, RunMode = RunMode.Async)]
        [Alias("serverreg", "svreg", "regsv")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Server registration, registers this particular server.")]
        public async Task RegServer()
        {
            // Log to console.
            Logger.Post("[COMMAND] Command acknowledged. Running server registry command.", Color.Teal);

            int Stage = 1;  // Iterate through all stages of server setup.
            int Stages = 8; // Set the maximum number of stages to iterate through.
            int IsEditing = 0; // 0 = Not editing; 1 = Primed to edit; 2 = Editing

            bool Mistake = false; // Use to determine if there was user error.

            ulong IDPass = 0;     // Prepare an ID passer.
            string TextPass = ""; // Prepare a text passer.
            bool TFPass = false;  // Prepare a flag passer.

            List<object> Passes = new List<object>(); // Prepare a container for all passer values.

            // Pre-generate a new profile to work from.
            ServerProfile ServerPass = new ServerProfile(Context.Guild.Id, Context.Guild.Name);
            
            // Log to console.
            Logger.Post("[COMMAND:SVREG] Prepped. Sending messages.", Color.Teal);

            await ReplyAsync("Hello! I can indeed register your server. Just let me check a few things first.");
            await Task.Delay(700);
            await ReplyAsync("*Okay... is it in here? I can't tell. Maybe this one? No... Ah! Maybe this is it! ..no...*");
            await Task.Delay(1300);
            if (Database.GetGuilds().Keys.Contains(Context.Guild.Id))
            {
                await ReplyAsync("Hmm... well... it would seem this server is already registered. However, I can allow you to make edits!");
                IsEditing = 1;
                ServerPass = new ServerProfile(Database.GetGuilds()[Context.Guild.Id].DataPass());
                
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] Server is already registered. Querying user {Context.User.Username.ToUpper()}#{Context.User.Discriminator} about edits.", Color.Teal);
            }
            else
            {
                await ReplyAsync("Hmm... let's see here... okay, your server isn't registered yet, so let's get started!");

                // Log to console.
                Logger.Post($"[COMMAND:SVREG] Server is not registered. Starting editing procedure.", Color.Teal);
            }

            if (IsEditing == 1)
            {
                var request = new ConfirmationBuilder()
                .WithContent(new PageBuilder().WithText("Would you like to make edits?"))
                .Build();

                var confirm = await Interaction.SendConfirmationAsync(request, Context.Channel);

                if (confirm.Value == true)
                {
                    await Context.Channel.SendMessageAsync("Okay! We can get started on editing now. :thumbsup:"); IsEditing = 2;

                    // Log to console.
                    Logger.Post($"[COMMAND:SVREG] Editing query completed with result TRUE.", Color.Teal);
                }
                else
                {
                    // Log to console.
                    Logger.Post($"[COMMAND:SVREG] Editing query completed with result FALSE.", Color.Teal);

                    await Context.Channel.SendMessageAsync("Fair enough. I'll be here when you need me."); return;
                }
            }

            EmbedBuilder Popup = new EmbedBuilder { Color = Database.GetUsers()[Context.User.Id].PrefColor,
                Title = $"Registering {Context.Guild.Name}...", Footer = new EmbedFooterBuilder { Text = $"Step {Stage} of {Stages}" } };
            if (IsEditing == 2)
            { Popup.Title = $"Editing {Context.Guild.Name}..."; }

            EmbedBuilder PopupMid = new EmbedBuilder { Color = Database.GetUsers()[Context.User.Id].PrefColor };

            while (Stage == 1)
            {
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] STEP 1: WELCOME CHANNEL", Color.Teal);
                Logger.Post($"[COMMAND:SVREG] Awaiting proper channel input.", Color.Teal);

                // Stage 1
                Popup.AddField("Welcome Channel", "Please write either a Discord channel tag or a channel's numeric ID.\n" +
                    "If you wish to skip this step, please write `skip`.");
                Popup.AddField("Current", $"<#{ServerPass.WelcomeChannel}> ({ServerPass.WelcomeChannel.ToString("D18")})");
                Popup.Footer.Text = $"Step {Stage} of {Stages}";
                await ReplyAsync("", false, Popup.Build());

                IDPass = ServerPass.WelcomeChannel; // Prepare passer!

                Mistake = false;

                var first = await Interaction.NextMessageAsync(x => x.Author == Context.User);

                if (first.IsSuccess == true)
                {
                    if (first.Value.Content.ToLower() == "skip")
                    {
                        PopupMid.AddField("Skipping...", "Okay, we'll skip this step, then.");
                        await ReplyAsync("", false, PopupMid.Build());

                        Popup.Fields.Clear();
                        PopupMid.Fields.Clear();

                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] User opted to skip this step.", Color.Teal);

                        Stage = 2;
                        break;
                    }
                    else
                    {
                        if (first.Value.MentionedChannels.Count == 0)
                        {
                            try
                            {
                                if (Context.Guild.TextChannels.Contains(Bot.GetChannel(ulong.Parse(first.Value.Content)))
                                    || (ulong.Parse(first.Value.Content) == 0))
                                {
                                    IDPass = ulong.Parse(first.Value.Content);

                                    // Log to console.
                                    Logger.Post($"[COMMAND:SVREG] User provided a channel in the form of an ID number.", Color.Teal);
                                }
                                else
                                {
                                    PopupMid.AddField("Incorrect Input", "Oops! That's not something I can use. Try again.");
                                    await ReplyAsync("", false, PopupMid.Build());

                                    Popup.Fields.Clear();
                                    PopupMid.Fields.Clear();

                                    Mistake = true;
                                    
                                    // Log to console.
                                    Logger.Post($"[COMMAND:SVREG] User made an error.", Color.Teal);
                                }
                            }
                            catch
                            {
                                PopupMid.AddField("Incorrect Input", "Oops! That's not something I can use. Try again.");
                                await ReplyAsync("", false, PopupMid.Build());

                                Popup.Fields.Clear();
                                PopupMid.Fields.Clear();

                                Mistake = true;

                                // Log to console.
                                Logger.Post($"[COMMAND:SVREG] User made an error.", Color.Teal);
                            }
                        }
                        else
                        {
                            IDPass = first.Value.MentionedChannels.ElementAt(0).Id;
                            
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User provided a channel.", Color.Teal);
                        }
                    }

                    if (!Mistake)
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] Asking user to confirm choice.", Color.Teal);

                        var request = new ConfirmationBuilder()
                        .WithContent(new PageBuilder().WithText($"Would you like to set **Welcome Channel** to <#{IDPass}> ({IDPass.ToString("D18")})?"))
                        .Build();

                        var confirm = await Interaction.SendConfirmationAsync(request, Context.Channel);

                        if (confirm.Value == true)
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmed their choice. Proceeding.", Color.Teal);

                            await Context.Channel.SendMessageAsync("Okay! We can continue. :thumbsup:"); Stage = 2;
                            break;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("Alright. Let's try again, then. Please say `skip` if you don't want to " +
                                  "fill in this value.");
                            
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmation failed or timed out. Trying again.", Color.Teal);
                        }
                    }

                    // Just in case...
                    Popup.Fields.Clear();
                    PopupMid.Fields.Clear();
                }
            }

            // Prepare for next stage.
            Passes.Add(IDPass);
            Popup.Fields.Clear();
            PopupMid.Fields.Clear();
            
            while (Stage == 2)
            {
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] STEP 2: WELCOME ROLE", Color.Teal);
                Logger.Post($"[COMMAND:SVREG] Awaiting proper role input.", Color.Teal);

                // Stage 2
                Popup.AddField("Welcome Role", "Please write either a Discord role tag or a role's numeric ID.\n" +
                    "If you wish to skip this step, please write `skip`.");
                Popup.AddField("Current", $"<@&{ServerPass.WelcomeRole}> ({ServerPass.WelcomeRole.ToString("D18")})");
                Popup.Footer.Text = $"Step {Stage} of {Stages}";
                await ReplyAsync("", false, Popup.Build());

                IDPass = ServerPass.WelcomeRole; // Prepare passer!

                var second = await Interaction.NextMessageAsync(x => x.Author == Context.User);

                if (second.IsSuccess == true)
                {
                    if (second.Value.Content.ToLower() == "skip")
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] User opted to skip this step.", Color.Teal);

                        PopupMid.AddField("Skipping...", "Okay, we'll skip this step, then.");
                        await ReplyAsync("", false, PopupMid.Build());

                        Popup.Fields.Clear();
                        PopupMid.Fields.Clear();

                        Stage = 3;
                        break;
                    }
                    else
                    {
                        if (second.Value.MentionedRoles.Count == 0)
                        {
                            try
                            {
                                if (Context.Guild.Users.Contains(Bot.GetUser(ulong.Parse(second.Value.Content)))
                                    || (ulong.Parse(second.Value.Content) == 0))
                                {
                                    IDPass = ulong.Parse(second.Value.Content);

                                    // Log to console.
                                    Logger.Post($"[COMMAND:SVREG] User provided a role in the form of an ID number.", Color.Teal);
                                }
                                else
                                {
                                    // Log to console.
                                    Logger.Post($"[COMMAND:SVREG] User made an error.", Color.Teal);

                                    PopupMid.AddField("Incorrect Input", "Oops! That's not something I can use. Try again.");
                                    await ReplyAsync("", false, PopupMid.Build());

                                    Popup.Fields.Clear();
                                    PopupMid.Fields.Clear();

                                    Mistake = true;
                                }
                            }
                            catch
                            {
                                // Log to console.
                                Logger.Post($"[COMMAND:SVREG] User made an error.", Color.Teal);

                                PopupMid.AddField("Incorrect Input", "Oops! That's not something I can use. Try again.");
                                await ReplyAsync("", false, PopupMid.Build());

                                Popup.Fields.Clear();
                                PopupMid.Fields.Clear();

                                Mistake = true;
                            }
                        }
                        else
                        {
                            IDPass = second.Value.MentionedRoles.ElementAt(0).Id;

                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User provided a role.", Color.Teal);
                        }
                    }

                    if (!Mistake)
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] Asking user to confirm choice.", Color.Teal);

                        var request = new ConfirmationBuilder()
                        .WithContent(new PageBuilder().WithText($"Would you like to set **Welcome Role** to <#{IDPass}> ({IDPass.ToString("D18")})?"))
                        .Build();

                        var confirm = await Interaction.SendConfirmationAsync(request, Context.Channel);

                        if (confirm.Value == true)
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmed their choice. Proceeding.", Color.Teal);

                            await Context.Channel.SendMessageAsync("Okay! We can continue. :thumbsup:"); Stage = 3; break;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("Alright. Let's try again, then. Please say `skip` if you don't want to " +
                                  "fill in this value.");
                            
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmation failed or timed out. Trying again.", Color.Teal);
                        }
                    }

                    // Just in case...
                    Popup.Fields.Clear();
                    PopupMid.Fields.Clear();
                }
            }

            // Prepare for next stage.
            Passes.Add(IDPass);
            Popup.Fields.Clear();
            PopupMid.Fields.Clear();

            while (Stage == 3)
            {
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] STEP 3: MOD CHANNEL", Color.Teal);
                Logger.Post($"[COMMAND:SVREG] Awaiting proper channel input.", Color.Teal);

                // Stage 3
                Popup.AddField("Mod Channel", "Please write either a Discord channel tag or a channel's numeric ID.\n" +
                    "If you wish to skip this step, please write `skip`.");
                Popup.AddField("Current", $"<#{ServerPass.ModChannel}> ({ServerPass.ModChannel.ToString("D18")})");
                Popup.Footer.Text = $"Step {Stage} of {Stages}";
                await ReplyAsync("", false, Popup.Build());

                IDPass = ServerPass.ModChannel; // Prepare passer!

                Mistake = false;

                var third = await Interaction.NextMessageAsync(x => x.Author == Context.User);

                if (third.IsSuccess == true)
                {
                    if (third.Value.Content.ToLower() == "skip")
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] User opted to skip this step.", Color.Teal);

                        PopupMid.AddField("Skipping...", "Okay, we'll skip this step, then.");
                        await ReplyAsync("", false, PopupMid.Build());

                        Popup.Fields.Clear();
                        PopupMid.Fields.Clear();

                        Stage = 4;
                        break;
                    }
                    else
                    {
                        if (third.Value.MentionedChannels.Count == 0)
                        {
                            try
                            {
                                if (Context.Guild.TextChannels.Contains(Bot.GetChannel(ulong.Parse(third.Value.Content)))
                                    || (ulong.Parse(third.Value.Content) == 0))
                                {
                                    IDPass = ulong.Parse(third.Value.Content);

                                    // Log to console.
                                    Logger.Post($"[COMMAND:SVREG] User provided a channel in the form of an ID number.", Color.Teal);

                                }
                                else
                                {
                                    PopupMid.AddField("Incorrect Input", "Oops! That's not something I can use. Try again.");
                                    await ReplyAsync("", false, PopupMid.Build());

                                    Popup.Fields.Clear();
                                    PopupMid.Fields.Clear();

                                    Mistake = true;

                                    // Log to console.
                                    Logger.Post($"[COMMAND:SVREG] User made an error.", Color.Teal);
                                }
                            }
                            catch
                            {
                                PopupMid.AddField("Incorrect Input", "Oops! That's not something I can use. Try again.");
                                await ReplyAsync("", false, PopupMid.Build());

                                Popup.Fields.Clear();
                                PopupMid.Fields.Clear();

                                Mistake = true;

                                // Log to console.
                                Logger.Post($"[COMMAND:SVREG] User made an error.", Color.Teal);
                            }
                        }
                        else
                        {
                            IDPass = third.Value.MentionedChannels.ElementAt(0).Id;
                            
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User provided a channel.", Color.Teal);
                        }
                    }

                    if (!Mistake)
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] Asking user to confirm choice.", Color.Teal);

                        var request = new ConfirmationBuilder()
                        .WithContent(new PageBuilder().WithText($"Would you like to set **Mod Channel** to <#{IDPass}> ({IDPass.ToString("D18")})?"))
                        .Build();

                        var confirm = await Interaction.SendConfirmationAsync(request, Context.Channel);

                        if (confirm.Value == true)
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmed their choice. Proceeding.", Color.Teal);

                            await Context.Channel.SendMessageAsync("Okay! We can continue. :thumbsup:"); Stage = 4; break;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("Alright. Let's try again, then. Please say `skip` if you don't want to " +
                                    "fill in this value.");

                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmation failed or timed out. Trying again.", Color.Teal);
                        }
                    }

                    // Just in case...
                    Popup.Fields.Clear();
                    PopupMid.Fields.Clear();
                }
            }

            // Prepare for next stage.
            Passes.Add(IDPass);
            Popup.Fields.Clear();
            PopupMid.Fields.Clear();

            while (Stage == 4)
            {
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] STEP 4: PREFIX", Color.Teal);
                Logger.Post($"[COMMAND:SVREG] Awaiting proper prefix input.", Color.Teal);

                // Stage 4
                Popup.AddField("Prefix", "Please write a new prefix.\n" +
                    "If you wish to skip this step, please write `skip`.");
                Popup.AddField("Current", ServerPass.Prefix);
                Popup.Footer.Text = $"Step {Stage} of {Stages}";
                await ReplyAsync("", false, Popup.Build());

                TextPass = ServerPass.Prefix; // Prepare passer!

                Mistake = false;

                var fourth = await Interaction.NextMessageAsync(x => x.Author == Context.User);

                if (fourth.IsSuccess == true)
                {
                    if (fourth.Value.Content.ToLower() == "skip")
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] User opted to skip this step.", Color.Teal);
                        
                        PopupMid.AddField("Skipping...", "Okay, we'll skip this step, then.");
                        await ReplyAsync("", false, PopupMid.Build());

                        Popup.Fields.Clear();
                        PopupMid.Fields.Clear();

                        Stage = 5;
                        break;
                    }
                    else
                    {
                        if (fourth.Value.Content.StartsWith('"'.ToString()) && fourth.Value.Content.EndsWith('"'.ToString()))
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User provided a valid prefix.", Color.Teal);

                            TextPass = fourth.Value.Content.Substring(1, fourth.Value.Content.Length - 2);
                        }
                        else
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User made an error.", Color.Teal);

                            PopupMid.AddField("Incorrect Input", "Oops! That's not something I can use. Try again.");
                            await ReplyAsync("", false, PopupMid.Build());

                            Popup.Fields.Clear();
                            PopupMid.Fields.Clear();

                            Mistake = true;
                        }
                    }

                    if (!Mistake)
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] Asking user to confirm choice.", Color.Teal);

                        var request = new ConfirmationBuilder()
                        .WithContent(new PageBuilder().WithText($"Would you like to set **Prefix** to {TextPass}?"))
                        .Build();

                        var confirm = await Interaction.SendConfirmationAsync(request, Context.Channel);

                        if (confirm.Value == true)
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmed their choice. Proceeding.", Color.Teal);

                            await Context.Channel.SendMessageAsync("Okay! We can continue. :thumbsup:"); Stage = 5; break;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("Alright. Let's try again, then. Please say `skip` if you don't want to " +
                                    "fill in this value.");

                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmation failed or timed out. Trying again.", Color.Teal);
                        }
                    }

                    // Just in case...
                    Popup.Fields.Clear();
                    PopupMid.Fields.Clear();
                }
            }

            // Prepare for next stage.
            Passes.Add(TextPass);
            Popup.Fields.Clear();
            PopupMid.Fields.Clear();

            while (Stage == 5)
            {
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] STEP 5: INVITES", Color.Teal);
                Logger.Post($"[COMMAND:SVREG] Awaiting proper boolean input.", Color.Teal);

                // Stage 5
                Popup.AddField("Allow Invites", "Please write either `true` or `false`.\n" +
                    "If you wish to skip this step, please write `skip`.");
                Popup.AddField("Current", $"{ServerPass.AllowingInvites}");
                Popup.Footer.Text = $"Step {Stage} of {Stages}";
                await ReplyAsync("", false, Popup.Build());

                TFPass = ServerPass.AllowingInvites; // Prepare passer!

                Mistake = false;

                var fifth = await Interaction.NextMessageAsync(x => x.Author == Context.User);

                if (fifth.IsSuccess == true)
                {
                    if (fifth.Value.Content.ToLower() == "skip")
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] User opted to skip this step.", Color.Teal);

                        PopupMid.AddField("Skipping...", "Okay, we'll skip this step, then.");
                        await ReplyAsync("", false, PopupMid.Build());

                        Popup.Fields.Clear();
                        PopupMid.Fields.Clear();

                        Stage = 6;
                        break;
                    }
                    else
                    {
                        try
                        {
                            TFPass = bool.Parse(fifth.Value.Content);
                            
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User provided a valid value {TFPass.ToString().ToUpper()}.", Color.Teal);
                        }
                        catch
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User made an error.", Color.Teal);

                            PopupMid.AddField("Incorrect Input", "Oops! That's not something I can use. Try again.");
                            await ReplyAsync("", false, PopupMid.Build());

                            Popup.Fields.Clear();
                            PopupMid.Fields.Clear();

                            Mistake = true;
                        }
                    }

                    if (!Mistake)
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] Asking user to confirm choice.", Color.Teal);

                        var request = new ConfirmationBuilder()
                        .WithContent(new PageBuilder().WithText($"Would you like to set **Allow Invites** to {TFPass}?"))
                        .Build();

                        var confirm = await Interaction.SendConfirmationAsync(request, Context.Channel);

                        if (confirm.Value == true)
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmed their choice. Proceeding.", Color.Teal);

                            await Context.Channel.SendMessageAsync("Okay! We can continue. :thumbsup:"); Stage = 6; break;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("Alright. Let's try again, then. Please say `skip` if you don't want to " +
                                    "fill in this value.");

                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmation failed or timed out. Trying again.", Color.Teal);
                        }
                    }

                    // Just in case...
                    Popup.Fields.Clear();
                    PopupMid.Fields.Clear();
                }
            }

            // Prepare for next stage.
            Passes.Add(TFPass);
            Popup.Fields.Clear();
            PopupMid.Fields.Clear();

            while (Stage == 6)
            {
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] STEP 6: JOINS", Color.Teal);
                Logger.Post($"[COMMAND:SVREG] Awaiting proper boolean input.", Color.Teal);

                // Stage 6
                Popup.AddField("Allow Join Messages", "Please write either `true` or `false`.\n" +
                    "If you wish to skip this step, please write `skip`.");
                Popup.AddField("Current", $"{ServerPass.AllowingJoinMsg}");
                Popup.Footer.Text = $"Step {Stage} of {Stages}";
                await ReplyAsync("", false, Popup.Build());

                TFPass = ServerPass.AllowingJoinMsg; // Prepare passer!

                Mistake = false;

                var sixth = await Interaction.NextMessageAsync(x => x.Author == Context.User);

                if (sixth.IsSuccess == true)
                {
                    if (sixth.Value.Content.ToLower() == "skip")
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] User opted to skip this step.", Color.Teal);

                        PopupMid.AddField("Skipping...", "Okay, we'll skip this step, then.");
                        await ReplyAsync("", false, PopupMid.Build());

                        Popup.Fields.Clear();
                        PopupMid.Fields.Clear();

                        Stage = 7;
                        break;
                    }
                    else
                    {
                        try
                        {
                            TFPass = bool.Parse(sixth.Value.Content);

                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User provided a valid value {TFPass.ToString().ToUpper()}.", Color.Teal);
                        }
                        catch
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User made an error.", Color.Teal);
                            
                            PopupMid.AddField("Incorrect Input", "Oops! That's not something I can use. Try again.");
                            await ReplyAsync("", false, PopupMid.Build());

                            Popup.Fields.Clear();
                            PopupMid.Fields.Clear();

                            Mistake = true;
                        }
                    }

                    if (!Mistake)
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] Asking user to confirm choice.", Color.Teal);

                        var request = new ConfirmationBuilder()
                        .WithContent(new PageBuilder().WithText($"Would you like to set **Allow Join Messages** to {TFPass}?"))
                        .Build();

                        var confirm = await Interaction.SendConfirmationAsync(request, Context.Channel);

                        if (confirm.Value == true)
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmed their choice. Proceeding.", Color.Teal);

                            await Context.Channel.SendMessageAsync("Okay! We can continue. :thumbsup:"); Stage = 7; break;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("Alright. Let's try again, then. Please say `skip` if you don't want to " +
                                    "fill in this value.");

                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmation failed or timed out. Trying again.", Color.Teal);
                        }
                    }

                    // Just in case...
                    Popup.Fields.Clear();
                    PopupMid.Fields.Clear();
                }
            }

            // Prepare for next stage.
            Passes.Add(TFPass);
            Popup.Fields.Clear();
            PopupMid.Fields.Clear();

            while (Stage == 7)
            {
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] STEP 7: BANS", Color.Teal);
                Logger.Post($"[COMMAND:SVREG] Awaiting proper boolean input.", Color.Teal);

                // Stage 7
                Popup.AddField("Allow Ban Messages", "Please write either `true` or `false`.\n" +
                    "If you wish to skip this step, please write `skip`.");
                Popup.AddField("Current", $"{ServerPass.AllowingBansMsg}");
                Popup.Footer.Text = $"Step {Stage} of {Stages}";
                await ReplyAsync("", false, Popup.Build());

                TFPass = ServerPass.AllowingBansMsg; // Prepare passer!

                Mistake = false;

                var seventh = await Interaction.NextMessageAsync(x => x.Author == Context.User);

                if (seventh.IsSuccess == true)
                {
                    if (seventh.Value.Content.ToLower() == "skip")
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] User opted to skip this step.", Color.Teal);

                        PopupMid.AddField("Skipping...", "Okay, we'll skip this step, then.");
                        await ReplyAsync("", false, PopupMid.Build());

                        Popup.Fields.Clear();
                        PopupMid.Fields.Clear();

                        Stage = 8;
                        break;
                    }
                    else
                    {
                        try
                        {
                            TFPass = bool.Parse(seventh.Value.Content);

                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User provided a valid value {TFPass.ToString().ToUpper()}.", Color.Teal);
                        }
                        catch
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User made an error.", Color.Teal);

                            PopupMid.AddField("Incorrect Input", "Oops! That's not something I can use. Try again.");
                            await ReplyAsync("", false, PopupMid.Build());

                            Popup.Fields.Clear();
                            PopupMid.Fields.Clear();

                            Mistake = true;
                        }
                    }

                    if (!Mistake)
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] Asking user to confirm choice.", Color.Teal);

                        var request = new ConfirmationBuilder()
                        .WithContent(new PageBuilder().WithText($"Would you like to set **Allow Ban Messages** to {TFPass}?"))
                        .Build();

                        var confirm = await Interaction.SendConfirmationAsync(request, Context.Channel);

                        if (confirm.Value == true)
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmed their choice. Proceeding.", Color.Teal);

                            await Context.Channel.SendMessageAsync("Okay! We can continue. :thumbsup:"); Stage = 8; break;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("Alright. Let's try again, then. Please say `skip` if you don't want to " +
                                    "fill in this value.");

                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmation failed or timed out. Trying again.", Color.Teal);
                        }
                    }

                    // Just in case...
                    Popup.Fields.Clear();
                    PopupMid.Fields.Clear();
                }
            }

            // Prepare for next stage.
            Passes.Add(TFPass);
            Popup.Fields.Clear();
            PopupMid.Fields.Clear();

            while (Stage == 8)
            {
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] STEP 8: PUBLICITY", Color.Teal);
                Logger.Post($"[COMMAND:SVREG] Awaiting proper boolean input.", Color.Teal);

                // Stage 8
                Popup.AddField("Publicity", "Please write either `true` or `false`.\n" +
                    "If you wish to skip this step, please write `skip`.");
                Popup.AddField("Current", $"{ServerPass.AllowingVisible}");
                Popup.Footer.Text = $"Step {Stage} of {Stages}";
                await ReplyAsync("", false, Popup.Build());

                TFPass = ServerPass.AllowingVisible; // Prepare passer!

                Mistake = false;

                var eighth = await Interaction.NextMessageAsync(x => x.Author == Context.User);

                if (eighth.IsSuccess == true)
                {
                    if (eighth.Value.Content.ToLower() == "skip")
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] User opted to skip this step.", Color.Teal);

                        PopupMid.AddField("Skipping...", "Okay, we'll skip this step, then.");
                        await ReplyAsync("", false, PopupMid.Build());

                        Popup.Fields.Clear();
                        PopupMid.Fields.Clear();

                        Stage = 9;
                        break;
                    }
                    else
                    {
                        try
                        {
                            TFPass = bool.Parse(eighth.Value.Content);

                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User provided a valid value {TFPass.ToString().ToUpper()}.", Color.Teal);
                        }
                        catch
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User made an error.", Color.Teal);

                            PopupMid.AddField("Incorrect Input", "Oops! That's not something I can use. Try again.");
                            await ReplyAsync("", false, PopupMid.Build());

                            Popup.Fields.Clear();
                            PopupMid.Fields.Clear();

                            Mistake = true;
                        }
                    }

                    if (!Mistake)
                    {
                        // Log to console.
                        Logger.Post($"[COMMAND:SVREG] Asking user to confirm choice.", Color.Teal);

                        var request = new ConfirmationBuilder()
                        .WithContent(new PageBuilder().WithText($"Would you like to set **Publicity** to {TFPass}?"))
                        .Build();

                        var confirm = await Interaction.SendConfirmationAsync(request, Context.Channel);

                        if (confirm.Value == true)
                        {
                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmed their choice. Proceeding.", Color.Teal);

                            await Context.Channel.SendMessageAsync("Okay! We can continue. :thumbsup:"); Stage = 9; break;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("Alright. Let's try again, then. Please say `skip` if you don't want to " +
                                    "fill in this value.");

                            // Log to console.
                            Logger.Post($"[COMMAND:SVREG] User confirmation failed or timed out. Trying again.", Color.Teal);
                        }
                    }

                    // Just in case...
                    Popup.Fields.Clear();
                    PopupMid.Fields.Clear();
                }
            }

            // Prepare for next stage.
            Passes.Add(TFPass);
            Popup.Fields.Clear();
            PopupMid.Fields.Clear();

            // Additional details which may not have been checked.
            Passes.Add(ServerPass.RequiredReads);

            // Finalize the new serverdata, and hold the old data before passing the new into the passer.
            List<object> ServerData = new List<object> { Context.Guild.Id, Context.Guild.Name };
            foreach (object o in Passes)
            { ServerData.Add(o); }
            ServerProfile ServerHold = ServerPass;
            ServerPass = new ServerProfile(ServerData);

            // Create the new embed that allows the user to see the changes they've made.
            Popup = new EmbedBuilder
            {
                Color = Database.GetUsers()[Context.User.Id].PrefColor,
                Title = $"Reviewing New Profile: {Context.Guild.Name}",
                Footer = new EmbedFooterBuilder { Text = $"Pending approval." },
                ThumbnailUrl = Context.Guild.IconUrl
            };
            // First element
            Popup.AddField("Server ID", $"{Context.Guild.Id}");

            // Second element
            Popup.AddField("Welcome Channel", "The channel which users are welcomed in, and which is used to generate new invites " +
                "(assuming this permission is set.)");
            Popup.AddField("Old", $"<#{ServerHold.WelcomeChannel}> ({ServerHold.WelcomeChannel.ToString("D18")})", true);
            Popup.AddField("New", $"<#{ServerPass.WelcomeChannel}> ({ServerPass.WelcomeChannel.ToString("D18")})", true);

            // Third element
            Popup.AddField("Welcome Role", "The role which new joins to the server are given.");
            Popup.AddField("Old", $"<@&{ServerHold.WelcomeRole}> ({ServerHold.WelcomeRole.ToString("D18")})", true);
            Popup.AddField("New", $"<@&{ServerPass.WelcomeRole}> ({ServerPass.WelcomeRole.ToString("D18")})", true);

            // Fourth element
            Popup.AddField("Moderator Channel", "The channel used to generate a custom audit log. Should be a hidden channel.");
            Popup.AddField("Old", $"<#{ServerHold.ModChannel}> ({ServerHold.ModChannel.ToString("D18")})", true);
            Popup.AddField("New", $"<#{ServerPass.ModChannel}> ({ServerPass.ModChannel.ToString("D18")})", true);

            // Fifth element
            Popup.AddField("Commands Prefix", "The custom prefix for this server. Defaults to `JR.`.");
            Popup.AddField("Old", ServerHold.Prefix, true);
            Popup.AddField("New", ServerPass.Prefix, true);

            // Sixth element
            Popup.AddField("Invites Allowed?", "Are you allowing invites to this server through this bot?");
            Popup.AddField("Old", $"{ServerHold.AllowingInvites}", true);
            Popup.AddField("New", $"{ServerPass.AllowingInvites}", true);

            // Seventh element
            Popup.AddField("Join/Leave Messages", "Have you enabled join and leave messages in the welcome channel?");
            Popup.AddField("Old", $"{ServerHold.AllowingJoinMsg}", true);
            Popup.AddField("New", $"{ServerPass.AllowingJoinMsg}", true);

            // Eighth element
            Popup.AddField("Ban Messages", "Have you enabled kick and ban messages in the welcome channel?");
            Popup.AddField("Old", $"{ServerHold.AllowingBansMsg}", true);
            Popup.AddField("New", $"{ServerPass.AllowingBansMsg}", true);

            // Ninth element
            Popup.AddField("Public?", "Are you allowing other servers to see yours?");
            Popup.AddField("Old", $"{ServerHold.AllowingVisible}", true);
            Popup.AddField("New", $"{ServerPass.AllowingVisible}", true);

            // Build and show user.
            await ReplyAsync("", false, Popup.Build());
            
            // Log to console.
            Logger.Post($"[COMMAND:SVREG] Asking user to review.", Color.Teal);

            // Confirm the user's edits and exit.
            var passConfirm = new ConfirmationBuilder()
            .WithContent(new PageBuilder().WithText("Are you ready to save these changes and register / save your edits?"))
            .Build();

            var passConfirmResult = await Interaction.SendConfirmationAsync(passConfirm, Context.Channel);

            if (passConfirmResult.Value == true)
            {
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] User has reviewed and desires these changes.", Color.Teal);

                await Context.Channel.SendMessageAsync("Alright! Just let me file this real quiiiiick...");
                await Task.Delay(700);
                await Context.Channel.SendMessageAsync("*alright, ok, aaaaaaand...*");
                await Task.Delay(700);
                if (Database.GetGuilds().ContainsKey(Context.Guild.Id))
                {
                    await Context.Channel.SendMessageAsync("*hmm-hmm-hmm, ah, here we go, and leeeet's swap that out here... and...*");
                    Database.GetGuilds()[Context.Guild.Id] = ServerPass;
                    await Task.Delay(900);
                    
                    // Log to console.
                    Logger.Post($"[COMMAND:SVREG] Finalizing edits.", Color.Teal);
                }
                else
                {
                    // Log to console.
                    Logger.Post($"[COMMAND:SVREG] Registering server.", Color.Teal);

                    await Context.Channel.SendMessageAsync("*alright, that's not in here, so leeeet's sort that out... aha!\n" +
                        $"filed under **{Context.Guild.Name[0].ToString().ToUpper()}**! let's just put this in heeeere... and...*");
                    Database.GetGuilds().Add(ServerPass.GuildId, ServerPass);
                    await Task.Delay(1600);
                }
                await Context.Channel.SendMessageAsync("Done! Your server has been filed anew! Have a good day!");
                await Task.Delay(700);

                PopupMid.AddField("Successful!", "Your server's profile and settings have been updated; " +
                    "your server will now be registered if it was not already.");
                await ReplyAsync("", false, PopupMid.Build());
                
                // Ensure the database saves the new information.
                Database.Save();
                
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] Database saved.", Color.Teal);

                return;
            }
            else
            {
                // Log to console.
                Logger.Post($"[COMMAND:SVREG] User cancelled operation.", Color.Teal);

                await Context.Channel.SendMessageAsync("Oh. Okay. Well, when you are ready to register or make some edits to the " +
                    "server profile, please don't hesitate to let me know!");

                await Task.Delay(1000);

                PopupMid.AddField("Changes Not Saved", "Your server's profile and settings have not been saved; " +
                    "if your server was not already registered, it has not been now.");
                await ReplyAsync("", false, PopupMid.Build());

                return;
            }
        }
    }
}
