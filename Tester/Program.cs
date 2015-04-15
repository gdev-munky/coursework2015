using System;
using ImageProcessing.AntiStamp_v1;

namespace Tester
{
    class Program
    {
        private static string SourceFileName;
        private static string TargetFileName;
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintHelp();
                return;
            }
            SourceFileName = args[0];
            TargetFileName = SourceFileName + ".processed.png";
            for (var i = 1; i < args.Length-1; i++)
            {
                if (!AnalyzeArg(args[i], args[i + 1]))
                    return;
            }

            Console.WriteLine("Initializing ...");
            var processor = new AntiStampImageProcessor(SourceFileName, TargetFileName);
            TimeSpan timeSpent;
            var result = processor.Run(out timeSpent);
            Console.WriteLine("Done in {0:F} msecs", timeSpent.TotalMilliseconds);
            if (!result)
                Console.WriteLine("Result: Failed!");
            
            foreach (var t in processor.Tasks)
            {
                Console.Write(">> {0} : done in {1:F} msecs, result : ", t, t.TimeSpentOnExecution.TotalMilliseconds);
                if (!string.IsNullOrWhiteSpace(t.ExecutionMessage))
                {
                    Console.WriteLine("Failed; message : {0}", t.ExecutionMessage);
                    break;
                }
                Console.WriteLine("OK");
            }
            //Console.ReadLine();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("== Usage =====================");
            Console.WriteLine(" Tester.exe <fileName> [/t <targetFileName>]");
            Console.WriteLine("==============================");
        }

        private static bool AnalyzeArg(string arg, string nextArg)
        {
            if (!arg.StartsWith("/"))
                return true;
            switch (arg)
            {
                case "/t":
                {
                    TargetFileName = nextArg;
                    break;   
                }
            }
            return true;
        }
    }
}
