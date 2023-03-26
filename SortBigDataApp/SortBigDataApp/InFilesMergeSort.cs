namespace SortBigDataApp
{
    public class InFilesMergeSort
    {
        public void Sort(Queue<string> queue)
        {
            while (queue.Count > 1)
            {
                var leftFileName = queue.Dequeue();
                var rightFileName = queue.Dequeue();

                var resultFileName = Guid.NewGuid().ToString();
                var fullResultFileName = Path.Combine(Path.GetDirectoryName(leftFileName), resultFileName);

                StreamReader leftFileReader = null, rightFileReader = null;
                StreamWriter resultFileWriter = null;
                try
                {
                    leftFileReader = new StreamReader(leftFileName);
                    rightFileReader = new StreamReader(rightFileName);
                    resultFileWriter = new StreamWriter(fullResultFileName);

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

                queue.Enqueue(fullResultFileName);
            }
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
