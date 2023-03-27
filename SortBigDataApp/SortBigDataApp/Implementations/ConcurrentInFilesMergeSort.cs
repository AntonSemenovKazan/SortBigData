using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Text;

namespace SortBigDataApp.Implementations
{
    public class ConcurrentInFilesMergeSort : IInMemorySort
    {
        private const int WriteBufferInBytes = 65000;

        private readonly ICollection<string> initialFiles;
        private readonly int maxConcurrency;

        public ConcurrentInFilesMergeSort(ICollection<string> initialFiles, IConfiguration configuration)
        {
            this.initialFiles = initialFiles;
            maxConcurrency = int.Parse(configuration["Settings:InFilesConcurrencyNumber"]);
        }

        public ICollection<string> Sort()
        {
            var queue = new ConcurrentQueue<string>(initialFiles);

            var semaphore = new SemaphoreSlim(maxConcurrency);
            var resultIsFound = queue.Count <= 1;
            while (!resultIsFound)
            {
                var nextIterationQueue = new ConcurrentQueue<string>();
                var tasks = new List<Task>();
                Console.WriteLine($"Sort in {queue.Count} files started...");
                while (queue.Count > 1)
                {
                    queue.TryDequeue(out var leftFileName);
                    queue.TryDequeue(out var rightFileName);

                    semaphore.Wait();

                    var enqueuedTask = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var resultFileName = Guid.NewGuid().ToString();
                            var fullResultFileName = Path.Combine(Path.GetDirectoryName(leftFileName), resultFileName);

                            StreamReader leftFileReader = null, rightFileReader = null;
                            StreamWriter resultFileWriter = null;
                            try
                            {
                                leftFileReader = new StreamReader(leftFileName);
                                rightFileReader = new StreamReader(rightFileName);
                                resultFileWriter = new StreamWriter(fullResultFileName, false, Encoding.UTF8, WriteBufferInBytes);

                                var leftLine = leftFileReader.ReadLine();
                                var leftEntity = ConvertToEntityOrNull(leftLine);

                                var rightLine = rightFileReader.ReadLine();
                                var rightEntity = ConvertToEntityOrNull(rightLine);

                                while (leftLine != null && rightLine != null)
                                {
                                    if (Entity.IsLessOrEqual(leftEntity, rightEntity))
                                    {
                                        resultFileWriter.WriteLine(leftLine);
                                        leftLine = leftFileReader.ReadLine();
                                        leftEntity = ConvertToEntityOrNull(leftLine);
                                    }
                                    else
                                    {
                                        resultFileWriter.WriteLine(rightLine);
                                        rightLine = rightFileReader.ReadLine();
                                        rightEntity = ConvertToEntityOrNull(rightLine);
                                    }
                                }

                                while (leftLine != null)
                                {
                                    resultFileWriter.WriteLine(leftLine);
                                    leftLine = leftFileReader.ReadLine();
                                }

                                while (rightLine != null)
                                {
                                    resultFileWriter.WriteLine(rightLine);
                                    rightLine = rightFileReader.ReadLine();
                                }
                            }
                            finally
                            {
                                leftFileReader?.Dispose();
                                rightFileReader?.Dispose();
                                resultFileWriter?.Dispose();
                            }
                            File.Delete(leftFileName);
                            File.Delete(rightFileName);

                            nextIterationQueue.Enqueue(fullResultFileName);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    tasks.Add(enqueuedTask);
                }

                Task.WaitAll(tasks.ToArray());

                if (queue.Count > 1)
                {
                    throw new Exception("Not expected count in queue");
                }
                else if (queue.Count == 1)
                {
                    queue.TryDequeue(out var lastFile);
                    nextIterationQueue.Enqueue(lastFile);
                }
                resultIsFound = nextIterationQueue.Count <= 1;
                if (nextIterationQueue.Contains(null))
                {
                    throw new Exception();
                }
                queue = nextIterationQueue;
            }

            queue.TryDequeue(out var result);
            return new List<string>() {
                result
            };
        }

        private static Entity ConvertToEntityOrNull(string line)
        {
            if (line == null)
            {
                return null;
            }

            return new Entity(line);
        }

    }
}
