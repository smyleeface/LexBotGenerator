using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.LexModelBuildingService;
using Amazon.LexModelBuildingService.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.RepresentationModel;

namespace LexBot.SlackBot {
    public class ManageIntents {
        private IAmazonLexModelBuildingService _lexBuildingClient;
        private BotYamlModel _lexYamlData;

        public ManageIntents(BotYamlModel lexYamlData) {
            _lexBuildingClient = new AmazonLexModelBuildingServiceClient();
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
            await _lexBuildingClient.DeleteIntentAsync(new DeleteIntentRequest {
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
            await _lexBuildingClient.PutIntentAsync(intent);
        }       
        
        private async Task UpdateLexIntent(PutIntentRequest intent, string checksum = null) {
            if (checksum != null) {
                intent.Checksum = checksum;
            }
            await PutLexIntent(intent);
        }

        private async Task<GetIntentResponse> DoesIntentExist(PutIntentRequest intent) {
            try {
                var response = await _lexBuildingClient.GetIntentAsync(new GetIntentRequest {
                    Name = intent.Name,
                    Version = "$LATEST"
                });
                return response;
            }
            catch (Exception e) {
                return null;
            }
        }
    }
}
