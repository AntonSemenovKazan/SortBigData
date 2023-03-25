
using System.IO;

public class TraverseTreeContext : IDisposable
{
    const string tempOutputFileName = "tempOutput.txt";

    public TraverseTreeContext(string outputDirectory, string outputFileName, string startTreeDirectory)
    {
        var fullOutputFileName = Path.Combine(outputDirectory, outputFileName);
        var fullTempFileName = Path.Combine(outputDirectory, tempOutputFileName);
        TreeDirectory = Path.Combine(outputDirectory, startTreeDirectory);

        TempFileName = fullTempFileName;

        OutputFileStream = new FileStream(fullOutputFileName, FileMode.Create);
        OutputWriter = new StreamWriter(OutputFileStream);

        InitTempStream();
    }

    public void InitTempStream()
    {
        if (File.Exists(TempFileName))
        {
            File.Delete(TempFileName);
        }


        TempFileStream = new FileStream(TempFileName, FileMode.OpenOrCreate);
        TempWriter = new StreamWriter(TempFileStream);
    }

    public void DisposeTempStream()
    {
        TempWriter?.Dispose();
        TempFileStream?.Dispose();
    }


    public void Dispose()
    {
        DisposeTempStream();

        OutputWriter?.Dispose();
        OutputFileStream?.Dispose();
    }

    public string TreeDirectory { get; set; }

    public string TempFileName { get; set; }

    private FileStream OutputFileStream { get; set; }


    public StreamWriter OutputWriter { get; set; }



    private FileStream TempFileStream { get; set; }



    public StreamWriter TempWriter { get; set; }

    public string PrevValue { get; set; }
}