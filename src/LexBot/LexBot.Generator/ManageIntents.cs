using System;
using System.Threading.Tasks;
using Amazon.LexModelBuildingService.Model;

namespace LexBot.Generator {
    public class ManageIntents {
        private ILexBotGeneratorDependencyProvider _provider;
        private BotYamlModel _lexYamlData;

        public ManageIntents(ILexBotGeneratorDependencyProvider provider, BotYamlModel lexYamlData) {
            _provider = provider;
            _lexYamlData = lexYamlData;
        }

        public async Task DeleteAll() {
            foreach (var intent in _lexYamlData.Intents) {
                await DeleteSingle(intent);
            }
        }

        public async Task DeleteSingle(PutIntentRequest intent) {
            var response = await DoesIntentExist(intent);
            if (response != null) {
                await DeleteLexIntent(intent.Name);
            }
        }
        
        private async Task DeleteLexIntent(string intentName) {
            await _provider.DeleteIntentAsync(new DeleteIntentRequest {
                Name = intentName
            });
        }
        
        public async Task RunUpdate() {
            foreach (var intent in _lexYamlData.Intents) {
                var response = await DoesIntentExist(intent);
                if (response != null) {
                    await UpdateLexIntent(intent, response.Checksum);
                }
                else {
                    await PutLexIntent(intent);
                }
            }
        }

        private async Task PutLexIntent(PutIntentRequest intent) {
            await _provider.PutIntentAsync(intent);
        }       
        
        private async Task UpdateLexIntent(PutIntentRequest intent, string checksum = null) {
            if (checksum != null) {
                intent.Checksum = checksum;
            }
            await PutLexIntent(intent);
        }

        private async Task<GetIntentResponse> DoesIntentExist(PutIntentRequest intent) {
            try {
                var response = await _provider.GetIntentAsync(new GetIntentRequest {
                    Name = intent.Name,
                    Version = "$LATEST"
                });
                return response;
            }
            catch {
                return null;
            }
        }
    }
}
