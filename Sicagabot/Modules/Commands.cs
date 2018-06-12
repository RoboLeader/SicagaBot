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
                + "`roll` Rolls a 6-sided die"
                );
        }
        

        [Command("roll")]
        public async Task roll()
        {
            Random die = new Random();
            await Context.Channel.SendMessageAsync(Context.User.Mention + ", you rolled a **" + die.Next(1, 6) + "**");
        }

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

        //about message.
        [Command("about")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task about()
        {
            await Context.Channel.SendMessageAsync("**SicagaBot version: " + version
                + "**\nCreated by <@156651495884849153> using Discord.NET for Sicaga.\n");
        }

        /* moving file downloads to service

        //download file code
        //uses HttpClient to maintain compatability with dotnet Core 1.0
        public static async Task<byte[]> DownloadAsByteArray(string url)
        {
            using (var client = new HttpClient())
            {

                using (var result = await client.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return await result.Content.ReadAsByteArrayAsync();
                    }

                }
            }
            return null;
        }
        */

        //✓ᵛᵉʳᶦᶠᶦᵉᵈ
    }
}
