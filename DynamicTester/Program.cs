using System;
using System.Linq;
using ImageProcessing.SharpShader;

namespace DynamicTester
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintHelp();
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Initializing ...");
            var processor = new SharpShader(args.Skip(1).ToArray());
            var start = DateTime.UtcNow;
            if (processor.LoadFromFile(args[0]))
                Console.WriteLine("Read shader in {0:F} msecs", (DateTime.UtcNow-start).TotalMilliseconds);
            else
            {
                Console.WriteLine("Reading shader failed");
                Console.WriteLine(processor.LastLoadMessages);
                Console.ReadLine();
                return;
            }

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
                    Console.ReadLine();
                    break;
                }
                Console.WriteLine("OK");
            }
        }
        private static void PrintHelp()
        {
            Console.WriteLine("== Usage =====================");
            Console.WriteLine(" DynamicTester.exe <shaderFileName> [args]");
            Console.WriteLine("==============================");
        }
    }
}
