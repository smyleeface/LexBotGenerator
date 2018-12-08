using System;
using System.Threading.Tasks;
using Amazon.LexModelBuildingService.Model;

namespace LexBot.Generator {
    public class ManageBots : BaseLexBotDependencyProvider {
        private ILexBotGeneratorDependencyProvider _provider;
        private BotYamlModel _lexYamlData;

        public ManageBots(ILexBotGeneratorDependencyProvider provider, BotYamlModel lexYamlData) {
            _provider = provider;
            _lexYamlData = lexYamlData;
        }

        public async Task DeleteSingle() {
            var botName = _lexYamlData.BotName;
            var response = await DoesBotExist(botName);
            if (response != null) {
                await DeleteLexBot(botName);
            }
        }

        public async Task RunUpdate() {
            var response = await DoesBotExist(_lexYamlData.BotName);
            if (response != null) {
                await UpdateLexBot(_lexYamlData.PutBotRequest, response.Checksum);
            }
            else {
                await PutLexBot(_lexYamlData.PutBotRequest);
            }
        }

        private async Task DeleteLexBot(string botName) {
            await _provider.DeleteBotAsync(new DeleteBotRequest {
                Name = botName
            });
        }
        
        private async Task UpdateLexBot(PutBotRequest putBotRequest, string checksum = null) {
            if (checksum != null) {
                putBotRequest.Checksum = checksum;                
            }
            await PutLexBot(putBotRequest);
        }
        
        private async Task PutLexBot(PutBotRequest putBotRequest) {
            await _provider.PutBotAsync(putBotRequest);
        }

        private async Task<GetBotResponse> DoesBotExist(string botName) {
            try {
                var response = await _provider.GetBotAsync(new GetBotRequest {
                    Name = botName,
                    VersionOrAlias = "$LATEST"
                });
                return response;
            }
            catch {
                return null;
            }
        }
    }
}
