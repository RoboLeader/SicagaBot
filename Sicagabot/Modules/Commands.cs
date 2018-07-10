using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Sicagabot.DTO;
using Discord.WebSocket;
using System.Collections.Generic;

namespace SicagaBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Alias("?")]
        public async Task help()
        {
            await Context.Channel.SendMessageAsync
                (
                "`.help` (Shows this message)" + Environment.NewLine
                + "`.roll` (roll a die. Add a number to roll multiple 6-sided dice, add what kind of die to roll to roll one of those. *EX: .roll 3 d20*)"
                + "\n\n **Admin commands:**"
                + "`addmessagetolisten` (add a message for the bot to listen to reactions on)" +
                "\n`showmessages` (show what messages the bot is listening on)" +
                "\n`addemoterolepair` (adds a new emote role pair, format is `.addemoterolepair Emote Role`)" +
                "\n`showemoterolepairs` (displays current emote role pairs)" +
                "\n`addsingleemoterolepair` (add an exclusive emote role pair. Users can only have one of these roles at a time.)"
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
            await Context.Channel.SendMessageAsync("**SicagaBot version: " + Program._config.version
                + "**\nCreated by Craig Hauser (<@156651495884849153>) using Discord.NET for the Sicaga Discord server.\n");
        }

        //Todo:
        // - support for roles and emotes with spaces
        // - validation on if the role even exists
        [Command("addemoterolepair")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task AddEmoteRole(string e, [Remainder]string r)
        {
            var newrole = new EmoteRoleDTO();
            newrole.Emote = e;
            newrole.Role = r;
            Program._config.Roles.Add(newrole);
            await Context.Channel.SendMessageAsync("Adding Emote pair: " + e + " " + r);
            Console.WriteLine("Adding Emote pair: " + e + " " + r);
            try
            {
                File.WriteAllText("EmoteRolePairs.json", JsonConvert.SerializeObject(Program._config.Roles));
            }
            catch { await Context.Channel.SendMessageAsync("Failed to write to file! Are permissions set properly?"); }
        }

        [Command("showemoterolepairs")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task ShowEmoteRolePairs()
        {
            string msg = "";
            foreach (var kvp in Program._config.Roles)
            {
                msg += "emote: " + kvp.Emote + " role: " + kvp.Role + Environment.NewLine;
            }
            await Context.Channel.SendMessageAsync(msg);
        }

        //TODO: simplify command
        [Command("deleteemoterolepair")]
        [Alias("removeemoterolepair")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task DeleteEmoteRolePair(string e, string r)
        {
            //Program._config._config.DeleteEmoteRolePair(e, r, Context);
            bool found = false;
            foreach (var kvp in Program._config.Roles)
            {
                if (kvp.Emote == e)
                {
                    if (kvp.Role == r)
                    {
                        Program._config.Roles.Remove(kvp);
                        await Context.Channel.SendMessageAsync("emote pair found, removing.");
                        Console.WriteLine("removing emote pair " + e + " " + r);
                        //save the new JSON file
                        File.WriteAllText("EmoteRolePairs.json", JsonConvert.SerializeObject(Program._config.Roles));
                        found = true;
                    }
                }
                
            }
            if (!found)
                await Context.Channel.SendMessageAsync("No matching pair found.");
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
            foreach (var item in Program._config.ignoredChannels)
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
            Program._config.rolesMessages.Add(messageID);
            try
            {
                File.WriteAllText("messagestolistento.json", JsonConvert.SerializeObject(Program._config.rolesMessages));
            }
            catch { await Context.Channel.SendMessageAsync("Failed to write to file! Are permissions set properly?"); }
        }

        [Command("showmessages")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task ShowListeningMessages()
        {
            string messages = "";
            foreach (ulong i in Program._config.rolesMessages)
            {
                messages += i + "\n";
            }
            await Context.Channel.SendMessageAsync(messages);
        }

        [Command("deletemessagetolisten")]
        [Alias("removemessagetolisten")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task DeleteMessageToListen(ulong messageID)
        {

        }


        //Todo:
        // - support for roles and emotes with spaces
        // - validation on if the role even exists
        [Command("addsingleemoterolepair")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task AddSingleEmoteRole(string e, [Remainder]string r)
        {
            var newrole = new EmoteRoleDTO();
            newrole.Emote = e;
            newrole.Role = r;
            Program._config.SingleRoles.Add(newrole);
            await Context.Channel.SendMessageAsync("Adding Emote pair: " + e + " " + r);
            Console.WriteLine("Adding Emote pair: " + e + " " + r);
            try
            {
                File.WriteAllText("SingleEmoteRolePairs.json", JsonConvert.SerializeObject(Program._config.SingleRoles));
            }
            catch { await Context.Channel.SendMessageAsync("Failed to write to file! Are permissions set properly?"); }
        }

        [Command("showsingleemoterolepairs")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task ShowSingleEmoteRolePairs()
        {
            string msg = "";
            foreach (var kvp in Program._config.SingleRoles)
            {
                msg += "emote: " + kvp.Emote + " role: " + kvp.Role + Environment.NewLine;
            }
            await Context.Channel.SendMessageAsync(msg);
        }

        //TODO: Simplify command
        [Command("deletesingleemoterolepair")]
        [Alias("removesingleemoterolepair")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task DeleteSingleEmoteRolePair(string e, string r)
        {
            //Program._config._config.DeleteEmoteRolePair(e, r, Context);
            bool found = false;
            foreach (var kvp in Program._config.SingleRoles)
            {
                if (kvp.Emote == e)
                {
                    if (kvp.Role == r)
                    {
                        Program._config.SingleRoles.Remove(kvp);
                        await Context.Channel.SendMessageAsync("emote pair found, removing.");
                        Console.WriteLine("removing emote pair " + e + " " + r);
                        //save the new JSON file
                        File.WriteAllText("SingleEmoteRolePairs.json", JsonConvert.SerializeObject(Program._config.SingleRoles));
                        found = true;
                    }
                }
                
            }
            if (!found)
                await Context.Channel.SendMessageAsync("No matching pair found.");
        }

        //✓ᵛᵉʳᶦᶠᶦᵉᵈ
    }
}