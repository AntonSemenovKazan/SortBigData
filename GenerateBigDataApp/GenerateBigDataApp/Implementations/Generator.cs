using GenerateBigDataApp.Interfaces;

namespace GenerateBigDataApp.Implementations
{
    public class Generator : IGenerator
    {
        private readonly Random random = new Random();
        private readonly ISettings settings;

        private string lastStringPart;

        public Generator(ISettings settings)
        {
            this.settings = settings;
            lastStringPart = GenerateStringPart();
        }

        public string Generate()
        {
            var number = random.Next(settings.MaxNumber);
            lastStringPart = GenerateStringPartWithChance();

            return $"{number}. {lastStringPart}";
        }

        private string GenerateStringPart() => RandomString(random.Next(settings.MinStringLength, settings.MaxStringLength));

        private string GenerateStringPartWithChance()
        {
            if (settings.DuplicateChanceInPercents == 0)
            {
                return GenerateStringPart();
            }

            if (settings.DuplicateChanceInPercents == 100)
            {
                return lastStringPart;
            }

            var randomChance = random.Next(100);

            if (settings.DuplicateChanceInPercents >= randomChance)
            {
                return lastStringPart;
            }

            return GenerateStringPart();
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}