using System.Text;

namespace SortBigDataApp
{
    public class InFilesMergeSort
    {
        public string Sort(Queue<string> queue, int maxConcurrency)
        {
            var semaphore = new SemaphoreSlim(maxConcurrency);

            var resultIsFound = queue.Count <= 1;
            while (!resultIsFound)
            {
                var nextIterationQueue = new Queue<string>();
                var tasks = new List<Task>();
                while (queue.Count > 1)
                {
                    var leftFileName = queue.Dequeue();
                    var rightFileName = queue.Dequeue();

                    semaphore.Wait();

                    var t = Task.Factory.StartNew(() =>
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
                                resultFileWriter = new StreamWriter(fullResultFileName, false, Encoding.UTF8, 65000);

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

                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());

                if (queue.Count > 1)
                {
                    throw new Exception("Not expected count in queue");
                }
                else if (queue.Count == 1)
                {
                    nextIterationQueue.Enqueue(queue.Dequeue());
                }
                resultIsFound = nextIterationQueue.Count <= 1;
                queue = nextIterationQueue;
            }

            return queue.Dequeue();
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
