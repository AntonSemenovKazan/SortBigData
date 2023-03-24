using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace SortBigDataApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // todo: put in config
            string fileName = @"input.txt";

            // todo: clean all prev results

            var outputDirectory = "output";

            DoSortIteration(fileName, outputDirectory);

            WriteOutput(outputDirectory);
        }

        static void WriteOutput(string startDirectory)
        {
            var outputFileName = "output.txt";
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }

            using (FileStream stream = new FileStream(outputFileName, FileMode.OpenOrCreate))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    var context = new TraverseTreeContext("tempOutput.txt");
                    context.InitTempStream();
                    context.Writer = writer;

                    GoThroughTreeWithTempWriter(context, startDirectory);

                    context.DisposeTempStream();
                    DoSortIteration(context.TempFileName, "tempOutput");
                    SimpleGoThroughTree(context.Writer, "tempOutput");
                }
            }
        }

        static void SimpleGoThroughTree(StreamWriter writer, string currentDirectory)
        {
            var leftDirectory = Path.Combine(currentDirectory, "l");
            if (Directory.Exists(leftDirectory))
            {
                SimpleGoThroughTree(writer, leftDirectory);
            }

            var valueFile = Path.Combine(currentDirectory, "v.txt");
            if (File.Exists(valueFile))
            {
                writer.WriteLine(File.ReadAllText(valueFile));
            }

            var rightDirectory = Path.Combine(currentDirectory, "r");
            if (Directory.Exists(rightDirectory))
            {
                SimpleGoThroughTree(writer, rightDirectory);
            }
        }

        static void GoThroughTreeWithTempWriter(TraverseTreeContext context, string currentDirectory)
        {
            var leftDirectory = Path.Combine(currentDirectory, "l");
            if (Directory.Exists(leftDirectory))
            {
                GoThroughTreeWithTempWriter(context, leftDirectory);
            }

            var valueFile = Path.Combine(currentDirectory, "v.txt");
            if (File.Exists(valueFile))
            {
                var value = File.ReadAllText(valueFile);

                if (context.PrevValue == null)
                {
                    context.PrevValue = value;
                }

                if (context.PrevValue != value)
                {
                    // sort and write temp
                    context.DisposeTempStream();

                    DoSortIteration(context.TempFileName, "tempOutput");
                    SimpleGoThroughTree(context.Writer, "tempOutput");
                    // clear temp
                    context.InitTempStream();
                    // update prevValue
                    context.PrevValue = value;
                }

                context.TempWriter.WriteLine(value);
            }

            var rightDirectory = Path.Combine(currentDirectory, "r");
            if (Directory.Exists(rightDirectory))
            {
                GoThroughTreeWithTempWriter(context, rightDirectory);
            }
        }

        static void DoSortIteration(string fileName, string startDirectory = null)
        {
            string leftFileName, rightFileName;

            if (!File.Exists(fileName))
            {
                Console.WriteLine($"File does not exist: {fileName}");
                return;
            }

            using (StreamReader reader = new StreamReader(fileName))
            {
                var line = reader.ReadLine();

                if (line == null)
                {
                    Console.WriteLine($"File is empty: {fileName}");
                    return;
                }

                var pivotItem = line;

                var currentDirectory = Path.GetDirectoryName(fileName);
                if (startDirectory != null)
                {
                    currentDirectory = Path.Combine(currentDirectory, startDirectory);

                    if (Directory.Exists(currentDirectory))
                    {
                        Directory.Delete(currentDirectory, true);
                    }

                    Directory.CreateDirectory(currentDirectory);
                }

                File.WriteAllText(Path.Combine(currentDirectory, "v.txt"), line);

                line = reader.ReadLine();

                if (line == null)
                {
                    return;
                }

                var leftDirectory = Path.Combine(currentDirectory, "l");
                var rightDirectory = Path.Combine(currentDirectory, "r");

                leftFileName = Path.Combine(leftDirectory, "s.txt");
                rightFileName = Path.Combine(rightDirectory, "s.txt");

                if (!Directory.Exists(leftDirectory))
                {
                    Directory.CreateDirectory(leftDirectory);
                }

                if (!Directory.Exists(rightDirectory))
                {
                    Directory.CreateDirectory(rightDirectory);
                }

                using (FileStream leftStream = new FileStream(leftFileName, FileMode.OpenOrCreate))
                {
                    using (StreamWriter leftWriter = new StreamWriter(leftStream))
                    {
                        using (FileStream rightStream = new FileStream(rightFileName, FileMode.OpenOrCreate))
                        {
                            using (StreamWriter rightWriter = new StreamWriter(rightStream))
                            {
                                do
                                {
                                    if (string.Compare(line, pivotItem) <= 0)
                                    {
                                        leftWriter.WriteLine(line);
                                    }
                                    else
                                    {
                                        rightWriter.WriteLine(line);
                                    }
                                } while ((line = reader.ReadLine()) != null);
                            }
                        }
                    }
                }
            }

            if (startDirectory == null)
            {
                File.Delete(fileName);
            }

            if (leftFileName != null)
            {
                DoSortIteration(leftFileName);
            }

            if (rightFileName != null)
            {
                DoSortIteration(rightFileName);
            }
        }
    }

    public class TraverseTreeContext : IDisposable
    {
        public TraverseTreeContext(string tempFileName)
        {
            TempFileName = tempFileName;
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
        }


        public string TempFileName { get; set; }

        private FileStream TempFileStream { get; set; }

        public StreamWriter Writer { get; set; }

        public StreamWriter TempWriter { get; set; }

        public string PrevValue { get; set; }
    }
}