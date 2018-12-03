using System;
using System.Linq;

namespace poc_failover
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var cluster = new Cluster(5);

            while (true) {
                Console.WriteLine(cluster);
                Console.WriteLine();
                Console.WriteLine("Please enter a command :");
                var commandText = Console.ReadLine();
                if (TryParse(commandText, out var runCommand, out var id)) {
                    var process = cluster.FindProcess(id);
                    runCommand(process);
                }
            }
        }

        private static bool TryParse(string commandText, out Action<Node> runCommand, out string id)
        {
            var parts = commandText.Split();
            string cmdText = parts.Length > 0 ? parts[0] : string.Empty;
            id = parts.Length > 1 ? parts[1] : string.Empty;
            switch (cmdText.ToLowerInvariant())
            {
                case "start":
                    runCommand = (Node p) => p.Start();
                    return true;
                case "stop":
                    runCommand = (Node p) => p.Stop();
                    return true;
                default:
                     runCommand = null;
                     Console.Error.WriteLine($"Command '{cmdText}' is not understood");
                    return false;
            }
        }
    }
}
