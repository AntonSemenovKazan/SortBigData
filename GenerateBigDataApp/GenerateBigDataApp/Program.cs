using GenerateBigDataApp.Implementations;
using Microsoft.Extensions.Configuration;

namespace GenerateBigDataApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // init start settings
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var settings = new Settings(Configuration);

            // init generator
            var generator = new Generator(settings);

            // init writer
            using var writer = new FileWriter(settings.GeneratedFileName);

            // generate N times
            for (var i = 1; i <= settings.GeneratedStringsNumber; i++)
            {
                var newString = generator.Generate();
                writer.Write(newString);
            }
        }
    }
}