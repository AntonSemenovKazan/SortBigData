using Microsoft.Extensions.Configuration;
using SortBigDataApp;
using System.Diagnostics;
using System.Text;

internal class Program
{

    const int BufferSize = 1000 * 1000 * 5;
    //const int BufferSize = 100;

    static void Main(string[] args)
    {
        IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var fileName = Configuration["Settings:InputFileName"];
        var parentDirectory = Path.GetDirectoryName(fileName);

        var fullSw = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();
        var buffer = new Entity[BufferSize];
        var fileNamesQueue = new Queue<string>();
        var bufferIndex = 0;

        int maxConcurrency = 10;
        List<Task> tasks = new List<Task>();
        var concurrencySemaphore = new SemaphoreSlim(maxConcurrency);
        using (StreamReader reader = new StreamReader(fileName))
        {
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                buffer[bufferIndex] = new Entity(line);
                bufferIndex++;

                if (bufferIndex >= BufferSize)
                {
                    concurrencySemaphore.Wait();

                    var bufferCopy = buffer;
                    var bufferIndexCopy = bufferIndex;
                    var t = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var tempFullFileName = SortInMemoryAndWriteToFile(sw, parentDirectory, bufferCopy, bufferIndexCopy);
                            fileNamesQueue.Enqueue(tempFullFileName);
                        }
                        finally
                        {
                            concurrencySemaphore.Release();
                        }
                    });

                    tasks.Add(t);

                    bufferIndex = 0;
                    buffer = new Entity[BufferSize];
                }
            }
        }

        if (bufferIndex > 0)
        {
            var tempFullFileName = SortInMemoryAndWriteToFile(sw, parentDirectory, buffer, bufferIndex);
            fileNamesQueue.Enqueue(tempFullFileName);
        }

        Task.WaitAll(tasks.ToArray());

        Console.WriteLine($"Full read and in memory sort and print took {fullSw.Elapsed}");
        fullSw.Restart();

        Console.WriteLine($"Files number = {fileNamesQueue.Count}");

        var inFileMergeSort = new InFilesMergeSort();
        var resultFileName = inFileMergeSort.Sort(fileNamesQueue, 10);

        Console.WriteLine($"In files sort took {fullSw.Elapsed}");
        fullSw.Restart();

        var outputPath = Path.Combine(parentDirectory, "output.txt");
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }
        System.IO.File.Move(resultFileName, outputPath);

        Console.WriteLine($"Sorted result in {resultFileName}");
    }

    private static string SortInMemoryAndWriteToFile(Stopwatch sw, string parentDirectory, Entity[] buffer, int bufferIndex)
    {
        Console.WriteLine($"Read all data for {sw.Elapsed}");
        sw.Restart();

        var sort = new ArrayInMemoryMergeSort();
        var right = bufferIndex - 1;
        var result = sort.SortArray(buffer, 0, right);

        Console.WriteLine($"Sort all data for {sw.Elapsed}");
        sw.Restart();

        var resultFileName = Guid.NewGuid().ToString();
        var outputPath = Path.Combine(parentDirectory, resultFileName);
        using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8, 65000))
        {
            for (var i = 0; i <= right; i++)
            {
                writer.WriteLine($"{result[i].NumberPart}. {result[i].StringPart}");
            }
        }

        Console.WriteLine($"Printed all data for {sw.Elapsed}");
        sw.Restart();

        return outputPath;
    }
}