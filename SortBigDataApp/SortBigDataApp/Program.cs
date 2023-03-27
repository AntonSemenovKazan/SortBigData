using Microsoft.Extensions.Configuration;
using SortBigDataApp;
using SortBigDataApp.Implementations;
using System.Diagnostics;
using System.Text;

internal class Program
{
    static void Main(string[] args)
    {
        IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var fileName = Configuration["Settings:InputFileName"];
        var inputFileInfo = new FileInfo(fileName);
        var parentDirectory = Path.GetDirectoryName(fileName);
        var outputPath = Path.Combine(parentDirectory, "output.txt");
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        var fullStopWatch = Stopwatch.StartNew();
        Console.WriteLine($"Sorting {inputFileInfo.Length / 1024} KB file started...");

        var sortInMemory = new ConcurrentInMemorySort(Configuration);
        var sortInMemoryResult = sortInMemory.Sort();

        var inFilesMergeSort = new ConcurrentInFilesMergeSort(sortInMemoryResult, Configuration);
        var resultFileName = inFilesMergeSort.Sort().Single();
        File.Move(resultFileName, outputPath);

        Console.WriteLine($"Sorting finished took {fullStopWatch.Elapsed}");
    }


}