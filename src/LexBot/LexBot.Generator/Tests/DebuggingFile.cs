using System.Threading.Tasks;
using Xunit;

namespace LexBot.SlackBot.Tests {
    public class DebuggingFile {
        
        [Fact]
        public async Task parse_lex_yaml() {
            var parseYaml = new ParseLexYaml();
            var lexYamlData = parseYaml.Run();
        }
        
        [Fact]
        public async Task create_or_update_lex_bot() {
            var parseYaml = new ParseLexYaml();
            var lexYamlData = parseYaml.Run();
            var updateSlots = new ManageSlots(lexYamlData);
            await updateSlots.RunUpdate();
            var manageIntents = new ManageIntents(lexYamlData);
            await manageIntents.RunUpdate();            
            var manageBots = new ManageBots(lexYamlData);
            await manageBots.RunUpdate();
        }

        [Fact]
        public async Task delete_lex_bot() {
            var parseYaml = new ParseLexYaml();
            var lexYamlData = parseYaml.Run();  
            var manageBots = new ManageBots(lexYamlData);
            await manageBots.DeleteSingle();
            var manageIntents = new ManageIntents(lexYamlData);
            await manageIntents.DeleteAll();
            var manageSlots = new ManageSlots(lexYamlData);
            await manageSlots.DeleteAll();
        }
    }
}