using System.IO;

namespace SortBigDataApp
{


    public class QuickSort<TPivotItem>
    {
        public QuickSort(string fileName, Func<string, TPivotItem> getPivotItem, Func<string, TPivotItem, bool> compareFunc, string startTreeDirectory)
        {
            FileName = fileName;
            GetPivotItem = getPivotItem;
            CompareFunc = compareFunc;
            StartTreeDirectory = startTreeDirectory;
        }

        public string FileName { get; }

        public Func<string, TPivotItem> GetPivotItem { get; }

        public Func<string, TPivotItem, bool> CompareFunc { get; }

        public string StartTreeDirectory { get; }

        public void Sort()
        {
            Sort(FileName, GetPivotItem, CompareFunc, StartTreeDirectory);
        }

        static void Sort<TPivotItem>(string fileName, Func<string, TPivotItem> getPivotItem, Func<string, TPivotItem, bool> compareFunc, string startTreeDirectory = null)
        {
            string leftFileName = null, rightFileName = null;

            //if (!File.Exists(fileName))
            //{
            //    Console.WriteLine($"File does not exist: {fileName}");
            //    return;
            //}

            using (StreamReader reader = new StreamReader(fileName))
            {
                var line = reader.ReadLine();

                if (line == null)
                {
                    //Console.WriteLine($"File is empty: {fileName}");
                    return;
                }

                var pivotItem = getPivotItem(line);

                var currentDirectory = Path.GetDirectoryName(fileName);
                if (startTreeDirectory != null)
                {
                    currentDirectory = Path.Combine(currentDirectory, startTreeDirectory);

                    if (Directory.Exists(currentDirectory))
                    {
                        Directory.Delete(currentDirectory, true);
                    }

                    Directory.CreateDirectory(currentDirectory);
                }

                File.WriteAllText(Path.Combine(currentDirectory, "v"), line);

                line = reader.ReadLine();

                if (line != null)
                {
                    StreamWriter leftWriter = null, rightWriter = null;
                    try
                    {

                        do
                        {
                            if (compareFunc(line, pivotItem))
                            {
                                if (leftWriter == null)
                                {
                                    var leftDirectory = Path.Combine(currentDirectory, "l");
                                    Directory.CreateDirectory(leftDirectory);
                                    leftFileName = Path.Combine(leftDirectory, "s");
                                    leftWriter = new StreamWriter(leftFileName);
                                }

                                leftWriter.WriteLine(line);
                            }
                            else
                            {
                                if (rightWriter == null)
                                {
                                    var rightDirectory = Path.Combine(currentDirectory, "r");
                                    Directory.CreateDirectory(rightDirectory);

                                    rightFileName = Path.Combine(rightDirectory, "s");

                                    rightWriter = new StreamWriter(rightFileName);
                                }

                                rightWriter.WriteLine(line);
                            }
                        } while ((line = reader.ReadLine()) != null);
                    }
                    finally
                    {
                        leftWriter?.Dispose();
                        rightWriter?.Dispose();
                    }
                }
            }

            if (startTreeDirectory == null)
            {
                File.Delete(fileName);
            }

            if (leftFileName != null)
            {
                Sort(leftFileName, getPivotItem, compareFunc);
            }

            if (rightFileName != null)
            {
                Sort(rightFileName, getPivotItem, compareFunc);
            }
        }
    }
}
