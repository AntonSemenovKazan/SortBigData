using Microsoft.Extensions.Configuration;
using SortBigDataApp;
using System.Diagnostics;

internal class Program
{

    const int InMemoryBytesMax = 1024 * 1024 * 1024;

    static void Main(string[] args)
    {
        IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var fileName = Configuration["Settings:InputFileName"];
        var parentDirectory = Path.GetDirectoryName(fileName);

        var fullSw = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();
        var inMemoryQueue = new Queue<List<Entity>>();
        var fileNamesQueue = new Queue<string>();
        var readBytesNumber = 0;
        using (StreamReader reader = new StreamReader(fileName))
        {
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                inMemoryQueue.Enqueue(new List<Entity>() { new Entity(line) });
                readBytesNumber += line.Length;

                if (readBytesNumber > InMemoryBytesMax)
                {
                    readBytesNumber = 0;

                    var tempFullFileName = SortInMemoryAndWriteToFile(sw, parentDirectory, inMemoryQueue);
                    fileNamesQueue.Enqueue(tempFullFileName);
                }
            }
        }

        if (inMemoryQueue.Count > 0)
        {
            var tempFullFileName = SortInMemoryAndWriteToFile(sw, parentDirectory, inMemoryQueue);
            fileNamesQueue.Enqueue(tempFullFileName);
        }

        Console.WriteLine($"Full read and in memory sort and print took {fullSw.Elapsed}");
        fullSw.Restart();

        Console.WriteLine($"Files number = {fileNamesQueue.Count}");

        var inFileMergeSort = new InFilesMergeSort();
        inFileMergeSort.Sort(fileNamesQueue);

        Console.WriteLine($"In files sort took {fullSw.Elapsed}");
        fullSw.Restart();

        var resultFileName = fileNamesQueue.Dequeue();
        var outputPath = Path.Combine(parentDirectory, "output.txt");
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }
        System.IO.File.Move(resultFileName, outputPath);

        Console.WriteLine($"Sorted result in {resultFileName}");
    }

    private static string SortInMemoryAndWriteToFile(Stopwatch sw, string parentDirectory, Queue<List<Entity>> inMemoryQueue)
    {
        Console.WriteLine($"Read all data for {sw.Elapsed}");
        sw.Restart();

        var sort = new InMemoryMergeSort();
        sort.Sort(inMemoryQueue);

        Console.WriteLine($"Sort all data for {sw.Elapsed}");
        sw.Restart();

        var result = inMemoryQueue.Dequeue();

        var resultFileName = Guid.NewGuid().ToString();
        var outputPath = Path.Combine(parentDirectory, resultFileName);
        using (StreamWriter writer = new StreamWriter(outputPath))
        {
            foreach (var entity in result)
            {
                writer.WriteLine($"{entity.NumberPart}. {entity.StringPart}");
            }
        }

        Console.WriteLine($"Printed all data for {sw.Elapsed}");
        sw.Restart();

        return outputPath;
    }
}