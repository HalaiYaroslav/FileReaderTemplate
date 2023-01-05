using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KioskScreensIdsInLogsTranslator
{
    class Program
    {
        private static List<TemplateScreenIdToNameMap> _templateScreensNames;
        private static int _waitingTicks = 0;

        static async Task Main(string[] args)
        {
            try
            {
                await LoadJson();

                Start:
                Console.WriteLine("Please, enter path where log files are need to be translated:");
                string logFilesSourcePath = Console.ReadLine();

                if (!Directory.Exists(logFilesSourcePath))
                {
                    Console.WriteLine("The path doesn't reference to an existing directory");
                    goto Start;
                }

                Console.WriteLine("\nStarting translation");

                string[] logFilesPaths = Directory.GetFiles(logFilesSourcePath, "*.log");
                var translatedLogFiles = new List<(string, List<string>)>();

                foreach (string logFilePath in logFilesPaths)
                {
                    var translatedLogFileRows = new List<string>();

                    await using FileStream logFile = File.OpenRead(logFilePath);
                    using var logFileStream = new StreamReader(logFile);

                    Console.WriteLine($"\nTranslating \"{Path.GetFileNameWithoutExtension(logFilePath)}\"");

                    string row;
                    while ((row = await logFileStream.ReadLineAsync()) != null)
                    {
                        VisualizeWaiting();

                        if (row.Contains("NextSelectedScreenId="))
                        {
                            string templateScreenIdPart = row.Substring(row.LastIndexOf("NextSelectedScreenId=", StringComparison.Ordinal) + "NextSelectedScreenId=".Length, 36);

                            var translation = _templateScreensNames.FirstOrDefault(x => x.Id == templateScreenIdPart)?.HumanReadableName;
                            if (translation != null)
                            {
                                row = row.Replace(templateScreenIdPart, $"{translation} - {templateScreenIdPart}");
                            }
                            else
                            {
                                Console.WriteLine("\n\tTHE FOLLOWING SCREEN ID IS NOT REGISTERED IN APPLICATION");
                                Console.WriteLine($"\t\t{templateScreenIdPart}");
                                Console.WriteLine("\tPLEASE, CALL 911 OR SIMPLY DROP A MESSAGE TO YARIK\n");
                                await Task.Delay(2000);
                                Console.WriteLine("\tDON'T FORGET YOUR BILLS ;-)\n");

                                row = row.Replace(templateScreenIdPart, $"ALOHA - {templateScreenIdPart}");
                            }

                        }

                        translatedLogFileRows.Add(row);
                    }

                    translatedLogFiles.Add((logFilePath, translatedLogFileRows));

                    Console.WriteLine($"\nEnd translating \"{Path.GetFileNameWithoutExtension(logFilePath)}\"\n");
                    _waitingTicks = 0;
                }

                Console.Beep();
                Console.WriteLine("\nTranslation successfully finished\n");

                Console.WriteLine("Please, enter path where you want translated copies to be pasted:");
                string translatedLogFilesTargetPath = Console.ReadLine();

                if (!Directory.Exists(translatedLogFilesTargetPath))
                {
                    Directory.CreateDirectory(translatedLogFilesTargetPath);
                    Console.WriteLine($"The path doesn't reference to an existing directory. Created new directory for the path \"{translatedLogFilesTargetPath}\"");
                }

                foreach ((string, List<string>) translatedLogFile in translatedLogFiles)
                {
                    await File.WriteAllLinesAsync($"{translatedLogFilesTargetPath}\\Translated_{Path.GetFileName(translatedLogFile.Item1)}", translatedLogFile.Item2);
                }

                Console.WriteLine($"\nFinished translations saving, find them in \"{translatedLogFilesTargetPath}\"\n");

                goto Start;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected exception: {e.Message}");
                Console.ReadKey();
            }
        }

        private static async Task LoadJson()
        {
            string executingExecutingAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException();
            string pathToTemplateScreenIdsToNamesMapFile = Path.Join(executingExecutingAssemblyDirectory , @"\..\..\..\..\TemplateScreenIdsToNamesMap.json");

            using var reader = new StreamReader(pathToTemplateScreenIdsToNamesMapFile);

            var jsonContent = await reader.ReadToEndAsync();

            _templateScreensNames = JsonConvert.DeserializeObject<List<TemplateScreenIdToNameMap>>(jsonContent);
        }

        private static void VisualizeWaiting()
        {
            const string informationalMessage = "Please wait, translating the shit";

            if (_waitingTicks == 0)
            {
                Console.Write(informationalMessage);
            }

            if (_waitingTicks == 3)
            {
                ClearCurrentConsoleLine(informationalMessage.Length);

                _waitingTicks = 1;
            }

            if (_waitingTicks++ <= 3)
            {
                Console.Write(".");
            }
        }

        private static void ClearCurrentConsoleLine(int informationalMessageLength)
        {
            int currentLineCursor = Console.CursorTop;

            Console.SetCursorPosition(informationalMessageLength, Console.CursorTop);

            Console.Write(new string(' ', Console.WindowWidth));

            Console.SetCursorPosition(informationalMessageLength, currentLineCursor);
        }
    }

    public class TemplateScreenIdToNameMap
    {
        public string Id { get; set; }

        public string HumanReadableName { get; set; }
    }
}