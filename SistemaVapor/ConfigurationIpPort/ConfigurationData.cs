using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Client
{
    public class ConfigurationData
    {

        public static string ip;

        public static string IpAddress
        {
            get 
            {   ip = Get().GetValue<string>("Ip");
                return ip;
            }
        }

        private static int port;

        public static int Port
        {
            get
            {
                port = Get().GetValue<int>("Port");
                return port;
            }
       
        }

        private static IConfiguration config;

        private  ConfigurationData() { }

        public static int ConfigPort()
        {
            return Port;
        }

        public static string ConfigIP()
        {
            return IpAddress;
        }


        private static IConfiguration Get()
        {
            
            config = BuildDefaultConfiguration();
            return config;
        }

        private static IConfigurationRoot BuildConfiguration(IConfigurationBuilder builder)
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false).Build();
            return config;
                
        }

        private static IConfiguration BuildDefaultConfiguration()
        {
            return BuildConfiguration(new ConfigurationBuilder());
        }

    }
}
