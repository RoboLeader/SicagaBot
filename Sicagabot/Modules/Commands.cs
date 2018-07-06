using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SicagaBot;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using Discord.Rest;
using Discord;
using Discord.Addons.InteractiveCommands;
using System.Xml.Linq;
using System.Xml;
using System.Linq;
using System.Net;
using System.Net.Http;
using Discord.WebSocket;
using Sicagabot.DTO;

namespace SicagaBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        string version = "0.1";
        
        [Command("help")]
        [Alias("?")]
        public async Task help()
        {
            await Context.Channel.SendMessageAsync
                (
                "`.help` (Shows this message)" + Environment.NewLine
                + "`.roll` (roll a die. Add a number to roll multiple 6-sided dice, add what kind of die to roll to roll one of those. *EX: .roll 3 d20*)"
                );
        }
        
        //rolls a single d6 by default
        [Command("roll")]
        public async Task roll()
        {
            Random die = new Random();
            await Context.Channel.SendMessageAsync(Context.User.Mention + ", you rolled **" + die.Next(1, 6) + "**");
        }

        //overload for a bunch of d6's
        [Command("roll")]
        public async Task roll(int i)
        {
            Random die = new Random();
            int adder = die.Next(1, 6);
            int newadder = 0;
            string msg = ", you rolled **" + adder + "**, ";
            for (int count = 2; count < i; count++)
            {
                newadder = die.Next(1, 6);
                msg += "**" + newadder + "**, ";
                adder += newadder;
            }
            newadder = die.Next(1, 6);
            adder += newadder; 
            msg += "**" + newadder + "** for a total of **" + adder + "**";
            await Context.Channel.SendMessageAsync(Context.User.Mention + msg);
        }

        //overload for extra kinds of dice
        [Command("roll")]
        public async Task roll([Remainder]string s)
        {
            Random die = new Random();
            // if the die is a d8, d12 or d20
            if (s == "d8")
            await Context.Channel.SendMessageAsync(Context.User.Mention + ", you rolled a **" + die.Next(1, 8) + "**");
            else if (s == "d12")
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you rolled a **" + die.Next(1, 12) + "**");
            else if (s == "d20")
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you rolled a **" + die.Next(1, 20) + "**");
            else
             await Context.Channel.SendMessageAsync(Context.User.Mention + ", you rolled a **" + die.Next(1, 8) + "**");
            
        }

        //overload for MULTIPLE extra kinds of dice
        [Command("roll")]
        public async Task roll(int i, [Remainder]string s)
        {
            Random die = new Random();
            int total = 0;
            int adder = 0;
            // if the die is a d8, d12 or d20
            if (i <= 0)
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you can't roll zero dice :\\");
            else if (s == "d8") {
                adder = die.Next(1, 8);
                string msg = ", you rolled **" + adder + "**, ";
                total += adder;
                for (int count = 2; count < i; count++)
                {
                    adder = die.Next(1, 8);
                    msg += "**" + adder + "**, ";
                    total += adder;
                }
                adder = die.Next(1, 8);
                total += adder;
                msg += "**" + adder + "** for a total of **" + total + "**";
                await Context.Channel.SendMessageAsync(Context.User.Mention + msg);
            }
            else if (s == "d12")
            {
                adder = die.Next(1, 12);
                string msg = ", you rolled **" + adder + "**, ";
                total += adder;
                for (int count = 2; count < i; count++)
                {
                    adder = die.Next(1, 12);
                    msg += "**" + adder + "**, ";
                    total += adder;
                }
                adder = die.Next(1, 12);
                total += adder;
                msg += "**" + adder + "** for a total of **" + total + "**";
                await Context.Channel.SendMessageAsync(Context.User.Mention + msg);
            }
            else if (s == "d20")
            {
                adder = die.Next(1, 20);
                string msg = ", you rolled **" + adder + "**, ";
                total += adder;
                for (int count = 2; count < i; count++)
                {
                    adder = die.Next(1, 20);
                    msg += "**" + adder + "**, ";
                    total += adder;
                }
                adder = die.Next(1, 20);
                total += adder;
                msg += "**" + adder + "** for a total of **" + total + "**";
                await Context.Channel.SendMessageAsync(Context.User.Mention + msg);
            }
            else
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you rolled **" + die.Next(1, 8) + "**");

        }

        //about message.
        [Command("about")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task about()
        {
            await Context.Channel.SendMessageAsync("**SicagaBot version: " + version
                + "**\nCreated by <@156651495884849153> using Discord.NET for the Sicaga Discord server.\n");
        }

        //NOTE, this does not work properly on emotes with spaces in the name
        //Gotta fix that.
        [Command("addemoterolepair")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task AddEmoteRole(string e, [Remainder]string r)
        {
            var newrole = new EmoteRoleDTO();
            newrole.Emote = e;
            newrole.Role = r;
            Program.Roles.Add(newrole);
            await Context.Channel.SendMessageAsync("Adding Emote pair: " + e + " " + r);
            Console.WriteLine("Adding Emote pair: " + e + " " + r);
            try
            {
                File.WriteAllText("EmoteRolePairs.json", JsonConvert.SerializeObject(Program.Roles));
            }
            catch { await Context.Channel.SendMessageAsync("Failed to write to file! Are permissions set properly?"); }
        }

        [Command("showemoterolepairs")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task ShowEmoteRolePairs()
        {
            string msg = "";
            foreach (var kvp in Program.Roles)
            {
                msg += "emote: " + kvp.Emote + " role: " + kvp.Role + Environment.NewLine;
            }
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("deleteemoterolepair")]
        [Alias("removeemoterolepair")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task DeleteEmoteRolePair(string e, string r)
        {
            //Program._config.DeleteEmoteRolePair(e, r, Context);
            bool found = false;
            foreach (var kvp in Program.Roles)
            {
                if (kvp.Emote == e)
                {
                    if (kvp.Role == r)
                    {
                        Program.Roles.Remove(kvp);
                        await Context.Channel.SendMessageAsync("emote pair found, removing.");
                        Console.WriteLine("removing emote pair " + e + " " + r);
                        //save the new JSON file
                        File.WriteAllText("EmoteRolePairs.json", JsonConvert.SerializeObject(Program.Roles));
                        found = true;
                    }
                }
                if (!found)
                    await Context.Channel.SendMessageAsync("No matching pair found.");
            }
        }

        [Command("addignoredchannel")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task AddIgnoredChannel(ulong channelID)
        {

        }

        [Command("showignoredchannels")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task ShowIgnoredChannels()
        {
            string msg = "";
            foreach (var item in Program.ignoredChannels)
            {
                msg += "Ignored channel: " + item + Environment.NewLine;
            }
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("removeignoredchannel")]
        [Alias("deleteignoredchannel")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task DeleteIgnoredChannel(ulong channelID)
        {

        }

        [Command("addmessagetolisten")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task AddMessageToListen(ulong messageID)
        {

        }

        [Command("showmessages")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task ShowListeningMessages()
        {

        }

        [Command("deletemessagetolisten")]
        [Alias("removemessagetolisten")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task DeleteMessageToListen(ulong messageID)
        {

        }


        //NOTE, this does not work properly on emotes with spaces in the name
        //Gotta fix that.
        [Command("addsingleemoterolepair")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task AddSingleEmoteRole(string e, [Remainder]string r)
        {
            var newrole = new EmoteRoleDTO();
            newrole.Emote = e;
            newrole.Role = r;
            Program.SingleRoles.Add(newrole);
            await Context.Channel.SendMessageAsync("Adding Emote pair: " + e + " " + r);
            Console.WriteLine("Adding Emote pair: " + e + " " + r);
            try
            {
                File.WriteAllText("SingleEmoteRolePairs.json", JsonConvert.SerializeObject(Program.SingleRoles));
            }
            catch { await Context.Channel.SendMessageAsync("Failed to write to file! Are permissions set properly?"); }
        }

        [Command("showsingleemoterolepairs")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task ShowSingleEmoteRolePairs()
        {
            string msg = "";
            foreach (var kvp in Program.SingleRoles)
            {
                msg += "emote: " + kvp.Emote + " role: " + kvp.Role + Environment.NewLine;
            }
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("deletesingleemoterolepair")]
        [Alias("removesingleemoterolepair")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task DeleteSingleEmoteRolePair(string e, string r)
        {
            //Program._config.DeleteEmoteRolePair(e, r, Context);
            bool found = false;
            foreach (var kvp in Program.SingleRoles)
            {
                if (kvp.Emote == e)
                {
                    if (kvp.Role == r)
                    {
                        Program.SingleRoles.Remove(kvp);
                        await Context.Channel.SendMessageAsync("emote pair found, removing.");
                        Console.WriteLine("removing emote pair " + e + " " + r);
                        //save the new JSON file
                        File.WriteAllText("SingleEmoteRolePairs.json", JsonConvert.SerializeObject(Program.SingleRoles));
                        found = true;
                    }
                }
                if (!found)
                    await Context.Channel.SendMessageAsync("No matching pair found.");
            }
        }

        //✓ᵛᵉʳᶦᶠᶦᵉᵈ
    }
}