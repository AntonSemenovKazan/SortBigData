using GenerateBigDataApp.Interfaces;

namespace GenerateBigDataApp.Implementations
{
    public class FileWriter : IWriter
    {
        private readonly StreamWriter writer;

        public FileWriter(string fileName)
        {
            writer = new StreamWriter(fileName);
        }

        public void Dispose()
        {
            writer?.Dispose();
        }

        public void Write(string value)
        {
            writer.WriteLine(value);
        }
    }
}