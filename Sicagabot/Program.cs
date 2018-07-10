using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SicagaBot.Tools.Configuration;
using System.Collections.Generic;
using System.Linq;
using Sicagabot.DTO;
using Discord.Rest;

/*
 * To Do:
 * 
 * Move all variables, lists and objects that will be needed by the program to Configuration, 
 * there's no reason it should be in Program.
 * 
 * Downloadfile can be updated to take advantage of Dotnet Core 2.0
 * 
 */


namespace SicagaBot
{
    class Program
    {
        private readonly DiscordSocketClient _client;

        public static Config _config = new Config(); //program config

        // Keep the CommandService and IServiceCollection around for use with commands.
        private readonly IServiceCollection _map = new ServiceCollection();
        private readonly CommandService _commands = new CommandService();

        // Program entry point
        static void Main(string[] args)
        {
            // Call the Program constructor, followed by the 
            // MainAsync method and wait until it finishes (which should be never).
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                // How much logging do you want to see?
                LogLevel = LogSeverity.Info,

                // If you or another service needs to do anything with messages
                // (eg. checking Reactions, checking the content of edited/deleted messages),
                // you must set the MessageCacheSize. You may adjust the number as needed.
                //MessageCacheSize = 50,

                // If your platform doesn't have native websockets,
                // add Discord.Net.Providers.WS4Net from NuGet,
                // add the `using` at the top, and uncomment this line:
                //WebSocketProvider = WS4NetProvider.Instance
            });

            //set "playing" message
            _client.SetGameAsync("with art supplies");

            //init config
            _config.init();
        }
        

        // Example of a logging handler. This can be re-used by addons
        // that ask for a Func<LogMessage, Task>.
        private static Task Logger(LogMessage message)
        {
            var cc = Console.ForegroundColor;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = cc;

            // If you get an error saying 'CompletedTask' doesn't exist,
            // your project is targeting .NET 4.5.2 or lower. You'll need
            // to adjust your project's target framework to 4.6 or higher
            // (instructions for this are easily Googled).
            // If you *need* to run on .NET 4.5 for compat/other reasons,
            // the alternative is to 'return Task.Delay(0);' instead.
            return Task.CompletedTask;
        }

        private async Task MainAsync()
        {
            // Subscribe the logging handler.
            _client.Log += Logger;

            // Centralize the logic for commands into a seperate method.
            await InitCommands();

            // Login and connect.
            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(-1);
        }

        private IServiceProvider _services;

        private async Task InitCommands()
        {
            // Repeat this for all the service classes
            // and other dependencies that your commands might need.
            //   _map.AddSingleton(new SomeServiceClass());

            // When all your required services are in the collection, build the container.
            // Tip: There's an overload taking in a 'validateScopes' bool to make sure
            // you haven't made any mistakes in your dependency graph.
            _services = _map.BuildServiceProvider();
            
            // Either search the program and add all Module classes that can be found.
            // Module classes *must* be marked 'public' or they will be ignored.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            // Or add Modules manually if you prefer to be a little more explicit:
            //await _commands.AddModuleAsync<SomeModule>();

            // Subscribe a handler to see if a message invokes a command.
            _client.MessageReceived += HandleCommandAsync;
            // 
            _client.ReactionAdded += OnAddReaction;
            _client.ReactionRemoved += OnRemovedReaction;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Bail out if it's a System Message.
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            // Create a number to track where the prefix ends and the command begins
            int pos = 0;

            //prefix
            if (msg.HasCharPrefix('.', ref pos))
            {
                // Create a Command Context.
                var context = new SocketCommandContext(_client, msg);

                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed succesfully).

                if (context.Channel.Id == 464627270829735936) //only execute if in the bot channel. Temporary.

                {
                    var result = await _commands.ExecuteAsync(context, pos, _services);

                    //TO-DO: add logging.
                }



            }
            ////////////////////////////////////////////////////////////////////////
            ////////        THE JOKES BEGIN HERE         ///////////////////////////
            ////////////////////////////////////////////////////////////////////////

            //for things that aren't a command, but we still want the bot to respond to it.
            //for the previous bot it had a few "joke" responses to common user messages like "lol"
            else
            {
                //wave if mentioned directly
                if (arg.Content.Contains("<@455932886097461249>"))
                {
                    IEmote emote = new Emoji("\U0001f44b");
                    await msg.AddReactionAsync(emote);
                }
            }
        }
        
        //when reactions are added to a message
        private async Task OnAddReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            //Console.WriteLine("Reaction Added: " + reaction.Emote.Name);//DEBUG
            foreach (ulong messageID in _config.rolesMessages)
            {
                if (messageID == message.Id)
                {
                    Console.WriteLine("Found reaction on listening message");
                    string rolename = "";
                    //Check to see if it matches an emote we're looking for
                    foreach (var kvp in _config.Roles)
                    {
                        //Console.WriteLine("checking regular roles");
                        if (kvp.Emote == reaction.Emote.Name)
                        {
                            rolename = kvp.Role;
                            var role = ((SocketGuildUser)reaction.User).Guild.Roles.Where(has => has.Name.ToUpper() == (rolename).ToUpper());
                            await ((SocketGuildUser)reaction.User).AddRolesAsync(role);
                            Console.WriteLine("Adding role " + rolename + " to user " + reaction.User.ToString());
                            return;
                        }
                    }

                    foreach (var kvp in _config.SingleRoles)
                    {
                        //Console.WriteLine("in SingleRoles, this should play 21 times");
                        if (kvp.Emote == reaction.Emote.Name)
                        {
                            //  Console.WriteLine("emote found");
                            rolename = kvp.Role;
                            var role = ((SocketGuildUser)reaction.User).Guild.Roles.Where(has => has.Name.ToUpper() == (rolename).ToUpper());
                            await ((SocketGuildUser)reaction.User).AddRolesAsync(role);
                            Console.WriteLine("Adding single role " + rolename + " to user " + reaction.User.ToString());

                            //remove the emote
                            var m = (RestUserMessage)await channel.GetMessageAsync(message.Id);
                            var e2 = reaction.Emote;
                            await m.RemoveReactionAsync(e2, reaction.User.Value);

                            //create a list of all this user's roles.
                            List<SocketRole> usersRoles = new List<SocketRole>(((SocketGuildUser)reaction.User).Roles);
                            //iterate through the list of singleroles, remove all matching roles from user
                            foreach (var i in _config.SingleRoles)
                            {
                                foreach (var o in usersRoles)
                                {
                                    if (i.Role == o.Name)
                                    {
                                        // if (usersRoles.Exists(x => x.Name.ToLower() == i.Role.ToLower())){
                                        role = ((SocketGuildUser)reaction.User).Guild.Roles.Where(has => has.Name.ToUpper() == o.Name.ToUpper());
                                        await ((SocketGuildUser)reaction.User).RemoveRolesAsync(role);
                                        Console.WriteLine("removing role " + o.Name + " from user " + reaction.User.ToString());
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                else { }
            }
        }

        //Get unused roles to remove
        private Dictionary<string, string> GetUnusedRoles(KeyValuePair<string, string> removepair)
        {
            Dictionary<string, string> allroles = new Dictionary<string, string>();
            foreach (var kvp in _config.SingleRoles)
            {
                allroles.Add(kvp.Emote, kvp.Role);
                //Console.WriteLine("adding role " + a.Value + " to dictionary");
            }
            allroles.Remove(removepair.Key);
            return allroles;
        }

        private async Task OnRemovedReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            //Console.WriteLine("Reaction Removed: " + reaction.Emote.Name);//DEBUG

            try
            {
                //If the ID is the message that the bot is listening for reactions on
                foreach (ulong messageID in _config.rolesMessages)
                {
                    if (reaction.MessageId == messageID)
                    {
                        //if the emote matches one in the dictionary
                        string rolename = "";
                        //if (Roles.TryGetValue(reaction.Emote.Name, out rolename))
                        foreach (var kvp in _config.Roles) {
                                if (kvp.Emote == reaction.Emote.Name)
                                {
                                    rolename = kvp.Role;
                                    var role = ((SocketGuildUser)reaction.User).Guild.Roles.Where(has => has.Name.ToUpper() == (rolename).ToUpper());
                                    await ((SocketGuildUser)reaction.User).RemoveRolesAsync(role);
                                    Console.WriteLine("removing role " + rolename + " from user " + reaction.User.ToString());
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
    }
}
