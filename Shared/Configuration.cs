using Microsoft.Extensions.Configuration;
using System.IO;


namespace Shared
{
    public class Configuration
    {
        private static IConfiguration config;

        private Configuration()
        {
        }

        public static IConfiguration Get()
        {
            if (config != null) return config;
            config = BuildDefaultConfiguration();
            return config;
        }

        private static IConfigurationRoot BuildConfiguration(IConfigurationBuilder builder)
        {
            var shared = Path.Combine("..", Directory.GetCurrentDirectory());
            return
                builder
                    .SetBasePath(shared)
                    .AddJsonFile("appsettings.json", false, true)
                    .Build();
        }

        private static IConfiguration BuildDefaultConfiguration()
        {
            return BuildConfiguration(new ConfigurationBuilder());
        }
    }

}
