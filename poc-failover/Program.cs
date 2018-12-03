using System;
using System.Linq;

namespace poc_failover
{

    

    public class Program
    {
        public static void Main(string[] args)
        {
            var processes = Enumerable.Range(1, 5).Select(idx => new Process(idx.ToString())).ToArray();

            while (true) {
                PrintCurrentState(processes);

                Console.WriteLine("Please enter a command :");
                var commandText = Console.ReadLine();
                if (TryParse(commandText, out var runCommand, out var id)) {
                    var process = processes.SingleOrDefault(p => p.Id == id);
                    if (process == null) {
                        Console.Error.WriteLine($"Process '{id}' does not exist");
                    } else {
                        runCommand(process);
                    }
                }
            }
        }

        private static bool TryParse(string commandText, out Action<Process> runCommand, out string id)
        {
            var parts = commandText.Split();
            string cmdText = parts.Length > 0 ? parts[0] : string.Empty;
            id = parts.Length > 1 ? parts[1] : string.Empty;
            switch (cmdText.ToLowerInvariant())
            {
                case "start":
                    runCommand = (Process p) => p.Start();
                    return true;
                case "stop":
                    runCommand = (Process p) => p.Stop();
                    return true;
                default:
                     runCommand = null;
                     Console.Error.WriteLine($"Command '{cmdText}' is not understood");
                    return false;
            }
        }

        private static void PrintCurrentState(Process[] processes)
        {
            Console.WriteLine("\nCurrent state :");
            foreach (var proc in processes)
            {
                Console.WriteLine($"\t{proc}");
            }
        }
    }
}
