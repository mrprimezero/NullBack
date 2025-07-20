using System;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace NullBack
{
    class Program
    {
        static void Main(string[] args)
        {
            DisplayHeader();
            
            if (!IsAdministrator())
            {
                Console.WriteLine("[ERROR] Tool requires Administrator privileges.");
                return;
            }

            var engine = new RecoveryEngine();
            
            while (true)
            {
                Console.Write("\nEnter directory path to scan (or 'exit' to quit): ");
                string path = Console.ReadLine().Trim();
                
                if (path.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;
                
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("[ERROR] Directory does not exist.");
                    continue;
                }

                var deletedItems = engine.ScanForDeletedItems(path);
                
                if (!deletedItems.Any())
                {
                    Console.WriteLine("No deleted items found in this location.");
                    continue;
                }

                DisplayDeletedItems(deletedItems);
                ProcessUserActions(engine, deletedItems);
            }
        }

        static void DisplayHeader()
        {
            Console.WriteLine(@"
  _   _       _ _       ____             _    
 | \ | |_   _| | | __  | __ )  __ _  ___| | __
 |  \| | | | | | |/ /  |  _ \ / _` |/ __| |/ /
 | |\  | |_| | |   <   | |_) | (_| | (__|   < 
 |_| \_|\__,_|_|_|\_\  |____/ \__,_|\___|_|\_\
            ");
            Console.WriteLine("NTFS File Recovery Tool - © PrimeZero 2023\n");
        }

        static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void DisplayDeletedItems(List<DeletedFile> items)
        {
            Console.WriteLine("\nDeleted Items Found:");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("ID  | Name                 | Size     | Deleted On");
            Console.WriteLine("--------------------------------------------------");
            
            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine($"{i+1,-3} | {items[i].Name,-20} | {items[i].Size,-8} | {items[i].DeletedDate}");
            }
        }

        static void ProcessUserActions(RecoveryEngine engine, List<DeletedFile> items)
        {
            Console.WriteLine("\nActions: [R]ecover, [D]elete from MFT, [C]ancel");
            
            while (true)
            {
                Console.Write("\nEnter action (filename -action): ");
                string input = Console.ReadLine().Trim();
                
                if (input.Equals("C", StringComparison.OrdinalIgnoreCase))
                    break;
                
                try
                {
                    var parts = input.Split(new[] {' ', '-'}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                        throw new ArgumentException("Invalid format");
                    
                    string filename = parts[0];
                    string action = parts[1].ToUpper();
                    
                    var item = items.FirstOrDefault(f => f.Name.Equals(filename, StringComparison.OrdinalIgnoreCase));
                    if (item == null)
                    {
                        Console.WriteLine("File not found in deleted items list.");
                        continue;
                    }
                    
                    switch (action)
                    {
                        case "R":
                            Console.Write("Enter recovery path: ");
                            string recoveryPath = Console.ReadLine();
                            engine.RecoverFile(item, recoveryPath);
                            Console.WriteLine("File recovered successfully.");
                            break;
                        case "D":
                            engine.PurgeMftEntry(item);
                            Console.WriteLine("MFT entry purged successfully.");
                            break;
                        default:
                            Console.WriteLine("Invalid action. Use R or D.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}