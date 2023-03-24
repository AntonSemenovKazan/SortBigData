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
            using (FileStream stream = new FileStream("output.txt", FileMode.OpenOrCreate))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    GoThroughTree(writer, startDirectory);
                }
            }
        }

        static void GoThroughTree(StreamWriter writer, string currentDirectory)
        {
            var leftDirectory = Path.Combine(currentDirectory, "l");
            if (Directory.Exists(leftDirectory))
            {
                GoThroughTree(writer, leftDirectory);
            }

            var valueFile = Path.Combine(currentDirectory, "v.txt");
            if (File.Exists(valueFile))
            {
                writer.WriteLine(File.ReadAllText(valueFile));
            }

            var rightDirectory = Path.Combine(currentDirectory, "r");
            if (Directory.Exists(rightDirectory))
            {
                GoThroughTree(writer, rightDirectory);
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
}