using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

//Loads startup config from file

namespace SicagaBot.Services.Configuration
{
    public class Config
    {
        public string Token = "";
        
        //Called by Program at startup, get config values from file to avoid hardcoding.
        public void Initialize()
        {
            //Get file
            Console.WriteLine("-- Config startup, getting XML file");
            XmlDocument config = new XmlDocument();
            try
            {
                string file = System.IO.File.ReadAllText(@"botconfig.xml");
                
                config.LoadXml(file);
            }catch { Console.WriteLine("failed to load config!"); }

            //Get token. There should only ever be one of these so we can get get element 0.
            try
            {
                Console.WriteLine("-- Getting token...");
                XmlNodeList XMLtoken = config.GetElementsByTagName("token");
                Token = XMLtoken[0].InnerText;
                Console.WriteLine("Token is " + Token);
            }
            catch { Console.WriteLine("failed to find token!"); }
        }
    }

}
