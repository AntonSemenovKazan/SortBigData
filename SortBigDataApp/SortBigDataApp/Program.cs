using Microsoft.Extensions.Configuration;
using SortBigDataApp;
using System.Diagnostics;

internal class Program
{
    const string startTreeDirectory = "tree";
    const string outputFileName = "output.txt";
    static string tempTreeDirectory = "tempTree";

    static bool FirstLevelSortAsc = true;
    static bool SecondLevelSortAsc = true;

    static void Main(string[] args)
    {
        IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var inputFileName = Configuration["Settings:InputFileName"];
        var rootDirectory = Path.GetDirectoryName(inputFileName);
        tempTreeDirectory = Path.Combine(rootDirectory, tempTreeDirectory);

        FirstLevelSortAsc = bool.Parse(Configuration["Settings:FirstLevelSortAsc"]);
        Func<string, string, bool> compareStringValues = FirstLevelSortAsc ? CompareStringValues : (a, b) => !CompareStringValues(a, b);

        SecondLevelSortAsc = bool.Parse(Configuration["Settings:SecondLevelSortAsc"]);

        var sw = Stopwatch.StartNew();
        Console.WriteLine("First tree building...");
        var quickSort = new QuickSort<string>(inputFileName, GetStringPivotItem, compareStringValues, startTreeDirectory);
        quickSort.Sort();
        sw.Stop();
        Console.WriteLine($"First tree is built for {sw.Elapsed}");

        sw.Restart();
        WriteOutput(rootDirectory);
        sw.Stop();
        Console.WriteLine($"Output generated for {sw.Elapsed}");
    }

    static string GetStringPivotItem(string line)
    {
        return Parse(line)[1];
    }

    static int GetIntPivotItem(string line)
    {
        return int.Parse(Parse(line)[0]);
    }

    static bool CompareStringValues(string line, string pivotItem)
    {
        var parsedA = Parse(line);
        return string.Compare(parsedA[1], pivotItem) <= 0;
    }

    static bool CompareIntValues(string a, int pivotItem)
    {
        var parsedA = Parse(a);
        return int.Parse(parsedA[0]) <= pivotItem;
    }

    static void WriteOutput(string outputDirectory)
    {
        using var context = new TraverseTreeContext(outputDirectory, outputFileName, startTreeDirectory);

        GoThroughTreeWithTempWriter(context, context.TreeDirectory);

        context.DisposeTempStream();

        Func<string, int, bool> compareIntValues = SecondLevelSortAsc ? CompareIntValues : (a, b) => !CompareIntValues(a, b);

        var quickSort = new QuickSort<int>(context.TempFileName, GetIntPivotItem, compareIntValues, tempTreeDirectory);
        quickSort.Sort();

        SimpleGoThroughTree(context.OutputWriter, tempTreeDirectory);
    }

    static void SimpleGoThroughTree(StreamWriter writer, string currentDirectory)
    {
        var leftDirectory = Path.Combine(currentDirectory, "l");
        if (Directory.Exists(leftDirectory))
        {
            SimpleGoThroughTree(writer, leftDirectory);
        }

        var valueFile = Path.Combine(currentDirectory, "v");
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


    static void GoThroughTreeWithTempWriter(TraverseTreeContext context, string currentTreeDirectory)
    {
        var leftDirectory = Path.Combine(currentTreeDirectory, "l");
        if (Directory.Exists(leftDirectory))
        {
            GoThroughTreeWithTempWriter(context, leftDirectory);
        }

        var valueFile = Path.Combine(currentTreeDirectory, "v");
        if (File.Exists(valueFile))
        {
            var value = File.ReadAllText(valueFile);
            var stringValue = Parse(value)[1];

            if (context.PrevValue == null)
            {
                context.PrevValue = stringValue;
            }

            if (context.PrevValue != stringValue)
            {
                // sort and write temp
                context.DisposeTempStream();

                Func<string, int, bool> compareIntValues = SecondLevelSortAsc ? CompareIntValues : (a, b) => !CompareIntValues(a, b);
                var quickSort = new QuickSort<int>(context.TempFileName, GetIntPivotItem, compareIntValues, tempTreeDirectory);
                quickSort.Sort();

                SimpleGoThroughTree(context.OutputWriter, tempTreeDirectory);
                // clear temp
                context.InitTempStream();
                // update prevValue
                context.PrevValue = stringValue;
            }

            context.TempWriter.WriteLine(value);
        }

        var rightDirectory = Path.Combine(currentTreeDirectory, "r");
        if (Directory.Exists(rightDirectory))
        {
            GoThroughTreeWithTempWriter(context, rightDirectory);
        }
    }

    static string[] Parse(string input)
    {
        return input.Split(". ");
    }
}