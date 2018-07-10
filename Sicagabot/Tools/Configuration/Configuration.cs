using Discord.Commands;
using Newtonsoft.Json;
using Sicagabot.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Discord.Addons.EmojiTools;

//Tool to be called on startup and when bot configuration needs to be changed.

/* TO DO:
 * Whitelist configuration
 * Move more configuration options into a single file
 * encapsulation.
 */

namespace SicagaBot.Tools.Configuration
{
    public class Config
    {
        public string version = "0.1";
        //Token for login
        public string Token = "";

        //IDs for Messages we are listening in on
        public List<ulong> rolesMessages;

        //Channels we want the bot to ignore
        public List<ulong> ignoredChannels = new List<ulong>();

        //optional list of channels to allow bot commands in
        public bool usingWhitelist = false;
        public List<ulong> allowedChannels = new List<ulong>();

        //dictionary for roles
        public List<EmoteRoleDTO> Roles = new List<EmoteRoleDTO>();
        public List<EmoteRoleDTO> SingleRoles = new List<EmoteRoleDTO>();

        public void init()
        {
           Token = GetToken();
            GetMessagesListeningTo(ref rolesMessages);
            GetRoles(ref Roles);
            GetSingleRoles(ref SingleRoles);
            GetMessagesListeningTo(ref rolesMessages);
            GetIgnoredChannels(ref ignoredChannels);
        }

        public string GetToken()
        {
            Console.WriteLine("\n-- Getting login Token");
            string s = "";
            try
                {
                    s = File.ReadAllText("Token.txt");
                Console.WriteLine("\n-- Login token is " + s);
                return s;
                }
                catch { Console.WriteLine("Failed to open Token.txt! Is the file missing and are permissions set?"); }
           
            return s;
        }

        //get all the messages that we are listening for reactions on.
        public void GetMessagesListeningTo(ref List<ulong> m)
        {
            Console.WriteLine("\n-- Getting list of messages we're listening to");
            try
            {
                string json = "";
                try
                {
                    json = File.ReadAllText("messagestolistento.json");
                }
                catch
                {
                    Console.WriteLine("Failed to open messagestolistento.json! Is the file missing and are permissions set?");
                }
                m = JsonConvert.DeserializeObject<List<ulong>>(json);
                }
                catch (Exception)
                {

                }
            foreach (ulong u in m)
            {
                Console.WriteLine("we are listening to message " + u);
            }
        }

        //populate the dictionary from file
        public void GetRoles(ref List<EmoteRoleDTO> Roles)
        {
            Console.WriteLine("\n-- Getting Emote/Role pairs");
            string json = "";
            try
            {
                try
                {
                    json = File.ReadAllText("EmoteRolePairs.json");
                }
                catch { Console.WriteLine("Failed to open EmoteRolePairs.json! Is the file missing and are permissions set?"); }
                Roles = JsonConvert.DeserializeObject<List<EmoteRoleDTO>>(json);
            }
            catch (Exception)
            {
                
            }
            foreach (var v in Roles)
            {
                Console.WriteLine("We are looking for " + v.Emote + "and applying role " + v.Role);
            }

        }

        public void GetSingleRoles(ref List<EmoteRoleDTO> SingleRoles)
        {
            Console.WriteLine("\n-- Getting Single Emote/Role pairs");
            string json = "";
            try
            {
                try
                {
                    json = File.ReadAllText("SingleEmoteRolePairs.json");
                }
                catch { Console.WriteLine("Failed to open SingleEmoteRolePairs.json! Is the file missing and are permissions set?"); }
                SingleRoles = JsonConvert.DeserializeObject<List<EmoteRoleDTO>>(json);
            }
            catch (Exception)
            {

            }
            foreach (var v in SingleRoles)
            {
                Console.WriteLine("We are looking for " + v.Emote + "and applying role " + v.Role);
            }
        }

        //get list of ignored channels
        public void GetIgnoredChannels(ref List<ulong> ic)
        {
            Console.WriteLine("\n-- Getting list of ignored channels");
            string json = "";
            try
            {
                try
                {
                    json = File.ReadAllText("ignoredchannels.json");
                }
                catch { Console.WriteLine("Failed to open ignoredchannels.json! Is the file missing and are permissions set?"); }
                ic = JsonConvert.DeserializeObject<List<ulong>>(json);
            }
            catch (Exception)
            {
                
            }
            foreach (ulong u in ic)
            {
                Console.WriteLine("we are ignoring channel " + u);
            }
        }


        //////////////////////////////////////////////////
        ////For modifying values while bot is running/////
        //////////////////////////////////////////////////

        public void AddRoles(string emote, string role, SocketCommandContext context)
        {

        }

        public void AddMessageToListenTo(ulong messageID)
        {

        }

        public void AddChannelToIgnore(ulong channelID)
        {

        }

        public void ReLoad()
        {
            Console.WriteLine("-- Reloading from files\n");
            GetMessagesListeningTo(ref rolesMessages);
            GetRoles(ref Roles);
            GetSingleRoles(ref SingleRoles);
            GetMessagesListeningTo(ref rolesMessages);
            GetIgnoredChannels(ref ignoredChannels);
        }

        public void DeleteEmoteRolePair(string e, string r, SocketCommandContext context)
        {

        }
    }

}
