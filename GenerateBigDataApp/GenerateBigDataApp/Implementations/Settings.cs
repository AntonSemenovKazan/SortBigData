using GenerateBigDataApp.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GenerateBigDataApp.Implementations
{
    public class Settings : ISettings
    {
        public Settings(IConfiguration configuration)
        {
            MaxNumber = int.Parse(configuration["Settings:MaxNumber"]);
            MinStringLength = int.Parse(configuration["Settings:MinStringLength"]);
            MaxStringLength = int.Parse(configuration["Settings:MaxStringLength"]);
            DuplicateChanceInPercents = int.Parse(configuration["Settings:DuplicateChanceInPercents"]);
            GeneratedStringsNumber = int.Parse(configuration["Settings:GeneratedStringsNumber"]);
            GeneratedFileName = configuration["Settings:GeneratedFileName"];

        }

        public int MaxNumber { get; }

        public int MinStringLength { get; }

        public int MaxStringLength { get; }

        public int DuplicateChanceInPercents { get; }

        public int GeneratedStringsNumber { get; }
        public string GeneratedFileName { get; }

    }
}