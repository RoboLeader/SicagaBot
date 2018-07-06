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

namespace SicagaBot.Tools.Configuration
{
    public class Config
    {
        //get login token, nothing fancy here, just keeping it out of the code.
        public string GetToken()
        {
            Console.WriteLine("\n-- Getting login Token");
            string s = "";
            try
                {
                    s = File.ReadAllText("Token.txt");
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
        }

        //get list of ignored channels
        public void GetIgnoredChannels(ref List<ulong> ignoredchannels)
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
                ignoredchannels = JsonConvert.DeserializeObject<List<ulong>>(json);
            }
            catch (Exception)
            {
                
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

        }

        public void DeleteEmoteRolePair(string e, string r, SocketCommandContext context)
        {

        }
    }

}
