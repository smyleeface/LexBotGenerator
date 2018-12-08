using System;
using System.Threading;

namespace LexBot.Generator {
    class Program {
        
        static void Main(string[] args) {

            string action;
            if (args.Length > 0) {
                action = args[0];
            }
            else {
                Console.WriteLine("!!! run `dotnet run setup` or `dotnet run teardown` !!!");
                return;
            }
            Console.WriteLine($"!!! LexBot {action} Begin");
            Console.WriteLine($">>> reading lex yaml definition");
            var parseYaml = ReadLocalFile.Run("Tests.Fixtures.LexDefinition.yml");
            var lexYamlData = parseYaml.Run();
            var baseLexbotProvider = new BaseLexBotDependencyProvider();
            Console.WriteLine($">>> parsing bot data from yaml");
            var manageBots = new ManageBots(baseLexbotProvider, lexYamlData);
            Console.WriteLine($">>> parsing intent data from yaml");
            var manageIntents = new ManageIntents(baseLexbotProvider, lexYamlData);
            Console.WriteLine($">>> parsing slot data from yaml");
            var manageSlots = new ManageSlots(baseLexbotProvider, lexYamlData);
            switch (action) {
                case "setup":
                    Console.WriteLine($">>> running setup for slots");
                    manageSlots.RunUpdate().Wait();
                    
                    // wait for aws to finish processing requests
                    Thread.Sleep(5000);
                    Console.WriteLine($">>> running setup for intents");
                    manageIntents.RunUpdate().Wait();
                    
                    // wait for aws to finish processing requests
                    Thread.Sleep(5000);
                    Console.WriteLine($">>> running setup for bot");
                    manageBots.RunUpdate().Wait();            
                    break;
                case "teardown":
                    Console.WriteLine($">>> running teardown for bot");
                    manageBots.DeleteSingle().Wait();
                    
                    // wait for aws to finish processing requests
                    Thread.Sleep(5000);
                    Console.WriteLine($">>> running teardown for intents");
                    manageIntents.DeleteAll().Wait();
                    
                    // wait for aws to finish processing requests
                    Thread.Sleep(5000);
                    Console.WriteLine($">>> running teardown for slots");
                    manageSlots.DeleteAll().Wait();
                    break;
            }
            Console.WriteLine($"LexBox {action} Complete !!!");
        }
    }
}