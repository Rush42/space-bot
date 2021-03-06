﻿using System;
using System.Diagnostics;
using CharonBot.Properties;

namespace CharonBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
#if DEBUG
           // Tests.RunTests();
#endif
            var stopwatch = Stopwatch.StartNew();
            RunBot(args);

            stopwatch.Stop();
            Console.WriteLine("[BOT]\tBot finished in {0} ms.", stopwatch.ElapsedMilliseconds);
        }

        private static void RunBot(string[] args)
        {
            if (args.Length != 1)
            {
                PrintUsage();
                return;
            }

            var outputPath = GetOutputPathFromArguments(args);

            var bot = new CharonBot(outputPath);
            bot.Execute();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("C# CharonBot usage: CharonBot.exe <outputFilename>");
            Console.WriteLine();
            Console.WriteLine("\toutputPath\tThe output folder where the match runner will output map and state files and look for the move file.");
        }

        private static string GetOutputPathFromArguments(string[] args)
        {
            if (!string.IsNullOrEmpty(args[0]))
            {
                return args[0];
            }

            Console.WriteLine("Invalid output filename default to: " + Settings.Default.OutputFile);
            return Settings.Default.OutputFile;
        }
    }
}