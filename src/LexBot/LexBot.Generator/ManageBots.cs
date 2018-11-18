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
    public class ManageBots {
        private IAmazonLexModelBuildingService _lexBuildingClient;
        private BotYamlModel _lexYamlData;

        public ManageBots(BotYamlModel lexYamlData) {
            _lexBuildingClient = new AmazonLexModelBuildingServiceClient();
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
            await _lexBuildingClient.DeleteBotAsync(new DeleteBotRequest {
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
            await _lexBuildingClient.PutBotAsync(putBotRequest);
        }

        private async Task<GetBotResponse> DoesBotExist(string botName) {
            try {
                var response = await _lexBuildingClient.GetBotAsync(new GetBotRequest {
                    Name = botName,
                    VersionOrAlias = "$LATEST"
                });
                return response;
            }
            catch (Exception e) {
                return null;
            }
        }
    }
}
