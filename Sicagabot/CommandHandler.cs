using Discord;
using Discord.Addons.EmojiTools;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Discord.Addons.InteractiveCommands;
using Discord.Rest;

namespace SicagaBot
{
    public class CommandHandler
    {

        private readonly IServiceProvider _provider;
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;

        //dictionary for simple roles
        Dictionary<string, string> characters = new Dictionary<string, string>();
        //dictionary for regions
        Dictionary<string, string> regions = new Dictionary<string, string>();

        //Messages to listen on
        ulong characterRolesMessage = 419612906448093195;

        //Channels to ignore
        ulong ignoredChannels = 259279873414135808;

        //RNG for fun stuff
        public Random rand = new Random();

        //Queue for Copypasta protection
        //

        public CommandHandler(IServiceProvider provider)
        {



            _provider = provider;
            _client = _provider.GetService<DiscordSocketClient>();
            _client.MessageReceived += _client_MessageReceived;
            //reactions code
            _client.ReactionAdded += OnAddReaction;
            _client.ReactionRemoved += ReactionRemoved;

            _commands = _provider.GetService<CommandService>();
            _client.SetGameAsync(".? for commands!");

            //filling up the dictionary for characters
            //Pair the name of the custom emote or unicode emoji we are listening to with the role
            //EX: characters.Add("Olaf", "Olaf Tyson");
            //EX: characters.Add("🖥", "PC");

            //Entries for region information
            /*
            regions.Add("SA", "SOUTH AMERICA");
            regions.Add("AF", "AFRICA");
            regions.Add("AU", "AUSTRALIA");
            regions.Add("NA", "NORTH AMERICA");
            regions.Add("EU", "EUROPE");
            regions.Add("AS", "ASIA");
            */

            ////////////////////////////////////////////////////
            //////////////role assignment code here/////////////
            ////////////////////////////////////////////////////

            //Role assignments
            async Task OnAddReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
            {
                try
                {
                    //If the ID is the message that the bot is listening for reactions on
                    if (reaction.MessageId == characterRolesMessage)
                    {
                        Console.WriteLine("reactionadded " + reaction.Emote.Name);
                        //if the emote matches one in the dictionary
                        string rolename = "";
                        //Check to see if it matches a character or platform emote
                        foreach (KeyValuePair<string, string> kvp in characters)
                        {
                            if (kvp.Key == reaction.Emote.Name)
                            {
                                rolename = kvp.Value;
                                var role = ((SocketGuildUser)reaction.User).Guild.Roles.Where(has => has.Name.ToUpper() == (rolename).ToUpper());
                                await ((SocketGuildUser)reaction.User).AddRolesAsync(role);
                                Console.WriteLine("Adding role " + rolename + " to user " + reaction.User.ToString());
                            }
                        }
                        //check to see if it matches a region
                        foreach (KeyValuePair<string, string> kvp in regions)
                        {
                            if (kvp.Key == reaction.Emote.Name)
                            {
                                //remove the emote
                                var m = (RestUserMessage)await channel.GetMessageAsync(message.Id);
                                var e2 = reaction.Emote;
                                await m.RemoveReactionAsync(e2, reaction.User.Value);
                                //add the role for the emote selected
                                rolename = kvp.Value;
                                var role = ((SocketGuildUser)reaction.User).Guild.Roles.Where(has => has.Name.ToUpper() == (rolename).ToUpper());
                                await ((SocketGuildUser)reaction.User).AddRolesAsync(role);
                                Console.WriteLine("Adding role " + rolename + " to user " + reaction.User.ToString());

                                //remove all other region roles
                                Dictionary<string, string> unusedroles = GetUnusedRegions(kvp);
                                //if the emote matches one in the dictionary

                                foreach (KeyValuePair<string, string> regionsToRemove in unusedroles)
                                {

                                    role = ((SocketGuildUser)reaction.User).Guild.Roles.Where(has => has.Name.ToUpper() == (regionsToRemove.Value).ToUpper());
                                    await ((SocketGuildUser)reaction.User).RemoveRolesAsync(role);
                                    Console.WriteLine("removing role " + regionsToRemove.Value + " from user " + reaction.User.ToString());
                                }
                            }
                        }
                    }
                }

                catch (Exception)
                {
                    Console.WriteLine("Failure in role assignment script.");
                }

            }

            //Get unused regions to get a list of roles to remove
            Dictionary<string, string> GetUnusedRegions(KeyValuePair<string, string> removepair)
            {
                Dictionary<string, string> allregions = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> a in regions)
                {
                    allregions.Add(a.Key, a.Value);
                    //Console.WriteLine("adding role " + a.Value + " to dictionary");
                }
                allregions.Remove(removepair.Key);
                return allregions;
            }


            //Role removal
            async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
            {

                try
                {
                    //If the ID is the message that the bot is listening for reactions on
                    if (reaction.MessageId == characterRolesMessage)
                    {
                        //if the emote matches one in the dictionary
                        string rolename = "";
                        if (characters.TryGetValue(reaction.Emote.Name, out rolename))
                        {
                            var role = ((SocketGuildUser)reaction.User).Guild.Roles.Where(has => has.Name.ToUpper() == (rolename).ToUpper());
                            await ((SocketGuildUser)reaction.User).RemoveRolesAsync(role);
                            Console.WriteLine("removing role " + rolename + " from user " + reaction.User.ToString());
                        }

                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failure in role assignment script.");
                }

            }







            async Task _client_MessageReceived(SocketMessage arg)
            {
                var message = arg as SocketUserMessage;
                if (message == null) return; //ignore system messages

                var context = new SocketCommandContext(_client, message);
               // int argPos = 0; //we are ignoring the prefix for now (Craig - 6/11/18)
                if (arg.Channel.Id != ignoredChannels && !arg.Author.IsBot)
                {
                    await context.Channel.SendMessageAsync("I could respond to anything but right now I respond to everything cause I'm still kinda dumb.");
                    //Console.WriteLine(message.Content.ToString());

                    /*
                    try
                    {

                        ////////////////////////////////////////////////////
                        //////////////insert the jokes heres////////////////
                        ////////////////////////////////////////////////////

                        if (arg.Content.ToLower().Contains("volta instinct"))
                        {
                            Random rand = new Random();
                            int i = rand.Next(1, 50);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("spot dodge the free fall\nspot dodge the read\nSp0t DodGe tHe sp0t dOdgE lImIt.\nspot dodge literally everything");
                            }
                        }


                        if (arg.Content.ToLower().Contains("snek") || arg.Content.ToLower().Contains("snake lady"))
                        {
                            int i = rand.Next(1, 30);
                            if (i == 1)
                            {
                                try
                                {
                                    Emoji e1 = new Emoji("🐍");
                                    await message.AddReactionAsync(e1);
                                }
                                catch { Console.WriteLine("Failed to respond to snake prompt"); }
                            }
                        }

                        if (arg.Content.ToLower().Contains("spot dodge") || arg.Content.ToLower().Contains("spotdodge"))
                        {
                            int i = rand.Next(1, 420);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("spot dodge the free fall"
                                    +" \nspot dodge the read\nSp0t DodGe tHe sp0t dOdgE lImIt." +
                                    "\nspot dodge literally everything");
                            }
                        }

                        if (arg.Content.Contains("<@420684032045744128>"))
                        {
                            var e1 = Emote.Parse("<:panicFeathers:368564239129903104>");
                            await message.AddReactionAsync(e1);
                        }

                        if (arg.Content.ToLower().Equals("lol"))
                        {
                            int i = rand.Next(1, 50);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("Huehuehue");
                                if (i == 2)
                                {
                                    await context.Channel.SendMessageAsync("lol");
                                }
                            }
                        }
                        if (arg.Content.ToLower().Equals("whodat"))
                        {
                            int i = rand.Next(1, 15);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("whodat");
                            }
                        }

                        if (arg.Content.ToLower().Contains("feathers") && arg.Content.ToLower().Contains("chicken") && arg.Content.ToLower().Contains("is"))
                        {
                            int i = rand.Next(1, 2);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("Do not call me a chicken.");
                            }
                        }

                        if (arg.Content.ToLower().Contains("switch update"))
                        {
                            int i = rand.Next(1, 20);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("But when does it come out on Xbox?");
                            }
                        }

                        if (arg.Content.ToLower().Contains("pc update"))
                        {
                            int i = rand.Next(1, 20);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("But when does it come out on Xbox?");
                            }
                        }

                        //no u
                        if (arg.Content.ToLower().Equals("no u"))
                        {
                            int i = rand.Next(1, 15);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("no u");
                            }
                        }
                        //meme right
                        if (arg.Content.ToLower().Contains("meme"))
                        {

                            int i = rand.Next(1, 30);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("if ur gonna meme\nMEME RIGHT");
                            }
                        }
                        //beats
                        if (arg.Content.ToLower().Contains("beats") || arg.Content.ToLower().Contains("volt") || arg.Content.ToLower().Contains("drumvolt") || arg.Content.ToLower().Contains("dash danc"))
                        {
                            int i = rand.Next(1, 15);
                            if (i == 1)
                            {
                                try
                                {
                                    var e1 = Emote.Parse("<a:drumvolt:411637794184757259>");
                                    await message.AddReactionAsync(e1);
                                }
                                catch
                                {
                                    Console.WriteLine("attempted to apply animated DrumVolt reaction, failed.");
                                }

                            }
                        }
                        //pacosmug responses
                        if (arg.Content.Contains("<:pacoSmug:359864036873207810>"))
                        {
                            int i = rand.Next(1, 10);
                            if (i == 1)
                            {
                                try
                                {
                                    var e1 = Emote.Parse("<:pacoSmug:359864036873207810>");
                                    await message.AddReactionAsync(e1);
                                }
                                catch
                                {
                                    Console.WriteLine("Failed to add pacoSmug reaction");
                                }
                            }
                        }
                        //various "what" responses
                        if (arg.Content.ToLower().Equals("wat"))
                        {
                            int i = rand.Next(1, 15);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("wat");
                            }
                        }
                        if (arg.Content.ToLower().Equals("what"))
                        {
                            int i = rand.Next(1, 15);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("what");
                            }
                        }
                        if (arg.Content.ToLower().Equals("wut"))
                        {
                            int i = rand.Next(1, 15);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("wut");
                            }
                        }
                        if (arg.Content.ToLower().Contains("waitwhat"))
                        {
                            int i = rand.Next(1, 10);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("***C O N F U S I O N***");
                            }
                        }
                        //i guess your just a brawlout pro

                        //side b spam
                        if (arg.Content.ToLower().Contains("meme"))
                        {
                            int i = rand.Next(1, 30);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("if ur gonna meme\nMEME RIGHT");
                            }
                        }
                        //data mining
                        if (arg.Content.ToLower().Contains("data mining") || arg.Content.ToLower().Contains("data mine"))
                        {
                            int i = rand.Next(1, 15);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("***S E C R E T S***");
                            }
                        }

                        //respond with "Salt" emotes
                        //SD's like a pro
                        if (arg.Content.ToLower().Contains("ozumi") || arg.Content.ToLower().Contains("ozu"))
                        {
                            int i = rand.Next(1, 60);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("Are we talking about Ozumi? I hear that guy SDs like a pro.");
                            }
                        }
                        //Various XD responses
                        if (arg.Content.ToLower().Contains("xd"))
                        {
                            int i = rand.Next(1, 50);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("eggsDEEE");
                            }
                            if (i == 2)
                            {
                                await context.Channel.SendMessageAsync("ECKS\n***sharp inhale***");
                                Thread.Sleep(6000);
                                await context.Channel.SendMessageAsync("DEEEEEEEEEEEEEEEEEEEEEEEEEE");
                            }
                        }
                        //Various responses to character names being said
                        //data mining
                        if (arg.Content.ToLower().Contains("Natura") || arg.Content.ToLower().Contains("natu'ra"))
                        {
                            int i = rand.Next(1, 30);
                            if (i == 1)
                            {
                                await context.Channel.SendMessageAsync("***SASSY GRASSY GAL***");
                            }
                        }

        */
                    ////////////////////////////////////////////////////
                    //////////////do serious stuff heres////////////////
                    ////////////////////////////////////////////////////


                }
            }
        }
        public async Task ConfigureAsync(IServiceProvider s)
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(),s);
        }
    }

}

