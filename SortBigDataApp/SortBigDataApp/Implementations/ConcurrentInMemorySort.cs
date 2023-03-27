using Microsoft.Extensions.Configuration;
using SortBigDataApp.Implementations;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

public class ConcurrentInMemorySort : IInMemorySort
{
    private const int WriteBufferInBytes = 65000;

    private readonly IConfiguration configuration;

    public ConcurrentInMemorySort(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public ICollection<string> Sort()
    {
        var fileName = configuration["Settings:InputFileName"];
        var parentDirectory = Path.GetDirectoryName(fileName);
        var inMemoryLinesNumber = int.Parse(configuration["Settings:InMemoryLinesNumber"]);
        var inMemoryConcurrencyNumber = int.Parse(configuration["Settings:InMemoryConcurrencyNumber"]);

        var fullStopWatch = Stopwatch.StartNew();

        var readLinesFromFile = new Entity[inMemoryLinesNumber];
        var fileNamesQueue = new ConcurrentBag<string>();
        var addedLinesNumber = 0;

        var tasks = new List<Task>();
        var concurrencySemaphore = new SemaphoreSlim(inMemoryConcurrencyNumber);
        var localStopWatch = Stopwatch.StartNew();

        using (StreamReader reader = new StreamReader(fileName))
        {
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                readLinesFromFile[addedLinesNumber] = new Entity(line);
                addedLinesNumber++;

                if (addedLinesNumber >= inMemoryLinesNumber)
                {
                    Console.WriteLine($"Read {addedLinesNumber} lines from file took {localStopWatch.Elapsed}");

                    concurrencySemaphore.Wait();

                    var bufferCopy = readLinesFromFile;
                    var bufferIndexCopy = addedLinesNumber;
                    var enqueuedTask = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var tempFullFileName = SortInMemoryAndWriteToFile(parentDirectory, bufferCopy, bufferIndexCopy);
                            fileNamesQueue.Add(tempFullFileName);
                        }
                        finally
                        {
                            concurrencySemaphore.Release();
                        }
                    });

                    tasks.Add(enqueuedTask);

                    addedLinesNumber = 0;
                    readLinesFromFile = new Entity[inMemoryLinesNumber];

                    localStopWatch.Restart();
                }
            }
        }

        if (addedLinesNumber > 0)
        {
            concurrencySemaphore.Wait();

            var enqueuedTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    var tempFullFileName = SortInMemoryAndWriteToFile(parentDirectory, readLinesFromFile, addedLinesNumber);
                    fileNamesQueue.Add(tempFullFileName);
                }
                finally
                {
                    concurrencySemaphore.Release();
                }
            });

            tasks.Add(enqueuedTask);
        }

        Task.WaitAll(tasks.ToArray());

        Console.WriteLine($"Full read, in-memory sort and write to {fileNamesQueue.Count} files took {fullStopWatch.Elapsed}");

        return fileNamesQueue.ToList();
    }

    private static string SortInMemoryAndWriteToFile(string parentDirectory, Entity[] buffer, int bufferIndex)
    {
        var sw = Stopwatch.StartNew();
        var sort = new InMemoryMergeSort();
        var right = bufferIndex - 1;
        var result = sort.SortArray(buffer, 0, right);

        Console.WriteLine($"Sort {bufferIndex} lines in memory took {sw.Elapsed}");
        sw.Restart();

        var resultFileName = Guid.NewGuid().ToString();
        var outputPath = Path.Combine(parentDirectory, resultFileName);
        using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8, WriteBufferInBytes))
        {
            for (var i = 0; i <= right; i++)
            {
                writer.WriteLine($"{result[i].NumberPart}. {result[i].StringPart}");
            }
        }

        Console.WriteLine($"Write {bufferIndex} lines took {sw.Elapsed}");
        sw.Restart();

        return outputPath;

    }
}
