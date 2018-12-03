using System;
using System.Linq;

namespace poc_failover
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var cluster = ClusterFactory.Create(2);

            while (true) 
            {
                Console.WriteLine(cluster);
                Console.WriteLine();
                Console.WriteLine("Please enter a command :");
                var commandText = Console.ReadLine();
                if (TryParse(commandText, out var runCommand))
                {
                    try 
                    {
                        runCommand(cluster);
                    } 
                    catch (Exception ex) 
                    {
                        Console.Error.WriteLine("Error: " + ex.Message);
                    }
                }
            }
        }

        private static bool TryParse(string commandText, out Action<Cluster> runCommand)
        {
            var parts = commandText.Split();
            string cmdText = parts.Length > 0 ? parts[0] : string.Empty;
            string name = parts.Length > 1 ? parts[1] : string.Empty;
            switch (cmdText.ToLowerInvariant())
            {
                case "start":
                    runCommand = (Cluster c) => c.StartNode(name);
                    return true;
                case "stop":
                    runCommand = (Cluster c) => c.StopNode(name);
                    return true;
                case "add":
                    runCommand = (Cluster c) => c.AddNode(name);
                    return true;
                case "remove":
                    runCommand = (Cluster c) => c.RemoveNode(name);
                    return true;
                case "quit":
                    runCommand = (Cluster c) => c.Dispose();
                    return true;
                default:
                     runCommand = null;
                     Console.Error.WriteLine($"Command '{cmdText}' is not understood");
                    return false;
            }
        }
    }
}
