using Build;
using Common;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (new PerformanceSection("Crayon"))
            {
#if DEBUG
                // First chance exceptions should crash in debug builds.
                ExecuteProgramUnchecked(args);
#else
                try
                {
                    ExecuteProgramUnchecked(args);
                }
                catch (InvalidOperationException e)
                {
                    System.Console.WriteLine(e.Message);
                }
                catch (MultiParserException e)
                {
                    System.Console.WriteLine(e.Message);
                }
                catch (ParserException e)
                {
                    System.Console.WriteLine(e.Message);
                }
#endif

#if DEBUG
                // Crash if there were any graphics contexts that weren't cleaned up.
                // This is okay on Windows, but on OSX this is a problem, so ensure that a
                // regressions are quickly noticed.
                SystemBitmap.Graphics.EnsureCleanedUp();
#endif
            }
        }

        private static void ExecuteProgramUnchecked(string[] args)
        {
#if DEBUG
            args = GetEffectiveArgs(args);
#endif
            new CrayonPipeline().Run(args);

#if DEBUG
            /*
            if (command.ShowPerformanceMarkers)
            {
                string summary = PerformanceTimer.GetSummary();
                Console.WriteLine(summary);
            }
            */
#endif
        }

        private static string[] GetEffectiveArgs(string[] actualArgs)
        {
#if DEBUG
            if (actualArgs.Length == 0)
            {
                string crayonHome = System.Environment.GetEnvironmentVariable("CRAYON_HOME");
                if (crayonHome != null)
                {
                    string debugArgsFile = FileUtil.JoinPath(crayonHome, "DEBUG_ARGS.txt");
                    if (FileUtil.FileExists(debugArgsFile))
                    {
                        string[] debugArgs = FileUtil.ReadFileText(debugArgsFile).Trim().Split('\n');
                        string lastArgSet = debugArgs[debugArgs.Length - 1].Trim();
                        if (lastArgSet.Length > 0)
                        {
                            return lastArgSet.Split(' ');
                        }
                    }
                }
            }
#endif
            return actualArgs;
        }

        internal static Platform.AbstractPlatform GetPlatformInstance(BuildContext buildContext)
        {
            string platformId = buildContext.Platform.ToLowerInvariant();
            return platformProvider.GetPlatform(platformId);
        }

        internal static PlatformProvider platformProvider = new PlatformProvider();
    }
}
