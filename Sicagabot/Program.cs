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

namespace SicagaBot
{
    class Program
    {
        private readonly DiscordSocketClient _client;

        public static Config _config = new Config(); //startup config

        //Token for login
        private string Token = "";

        //IDs for Messages we are listening in on
        private List<ulong> rolesMessages;

        //Channels we want the bot to ignore
        private List<ulong> ignoredChannels = new List<ulong>();

        //dictionary for roles
        public static List<EmoteRoleDTO> Roles = new List<EmoteRoleDTO>();
       // private Dictionary<string, string> Roles = new Dictionary<string, string>();


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

            //get the login token from the config file
            Token = _config.GetToken();
            Console.WriteLine(Token);
            //populate the dictionary of Roles and corresponding emotes
            _config.GetRoles(ref Roles);
            foreach (var v in Roles)
            {
                Console.WriteLine("We are looking for " + v.Emote + "and applying role " + v.Role);
            }
            //Get messages we are listening to
            _config.GetMessagesListeningTo(ref rolesMessages);
            foreach (ulong u in rolesMessages)
            {
                Console.WriteLine("we are listening to message " + u);
            }
            //get list of ignored channels
            _config.GetIgnoredChannels(ref ignoredChannels);
            foreach (ulong u in ignoredChannels)
            {
                Console.WriteLine("we are ignoring channel " + u);
            }

            //set "playing" message
            _client.SetGameAsync(".? for commands!");
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
            await _client.LoginAsync(TokenType.Bot, Token);
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
                var result = await _commands.ExecuteAsync(context, pos, _services);

                //TO-DO: add logging.

            }
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
            foreach (ulong messageID in rolesMessages)
            {
                string rolename = "";
                bool found = false;//if we found a matching emote, no need to check anymore.
                //Check to see if it matches an emote we're looking for
                foreach (var kvp in Roles)
                {
                    if (kvp.Emote == reaction.Emote.Name)
                    {
                        rolename = kvp.Role;
                        var role = ((SocketGuildUser)reaction.User).Guild.Roles.Where(has => has.Name.ToUpper() == (rolename).ToUpper());
                        await ((SocketGuildUser)reaction.User).AddRolesAsync(role);
                        Console.WriteLine("Adding role " + rolename + " to user " + reaction.User.ToString());
                        found = true;
                    }
                }
                if (found)//emote is found, break out.
                    break;
            }
        }

        private async Task OnRemovedReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            //Console.WriteLine("Reaction Removed: " + reaction.Emote.Name);//DEBUG

            try
            {
                //If the ID is the message that the bot is listening for reactions on
                foreach (ulong messageID in rolesMessages)
                {
                    if (reaction.MessageId == messageID)
                    {
                        //if the emote matches one in the dictionary
                        string rolename = "";
                        //if (Roles.TryGetValue(reaction.Emote.Name, out rolename))
                        foreach (var kvp in Roles) {
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
