namespace GenerateBigDataApp.Interfaces
{
    public interface ISettings
    {
        public int MaxNumber { get; }

        public int MinStringLength { get; }

        public int MaxStringLength { get; }

        public int DuplicateChanceInPercents { get; }

        public int GeneratedStringsNumber { get; }

        public string GeneratedFileName { get; }
    }
}