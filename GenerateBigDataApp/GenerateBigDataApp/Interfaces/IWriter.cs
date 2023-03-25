namespace GenerateBigDataApp.Interfaces
{
    public interface IWriter : IDisposable
    {
        void Write(string value);
    }
}