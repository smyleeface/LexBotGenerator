using System;
using System.Linq;
using System.Threading.Tasks;

namespace LexBot.SlackBot {
    class Program {
        static async Task Main(string[] args) {
            var parseLexYaml = new ParseLexYaml();
            var lexYamlData = parseLexYaml.Run();
            var manageBots = new ManageBots(lexYamlData);
            var manageIntents = new ManageIntents(lexYamlData);
            var manageSlots = new ManageSlots(lexYamlData);
            var action = args.FirstOrDefault();
            switch (action) {
                case "setup":
                    await manageSlots.RunUpdate();
                    await manageIntents.RunUpdate();            
                    await manageBots.RunUpdate();            
                    break;
                case "teardown":
                    await manageBots.DeleteSingle();
                    await manageIntents.DeleteAll();
                    await manageSlots.DeleteAll();
                    break;
            }

            Console.WriteLine("LexBoxSetupComplete");
        }
    }
}