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
    public class ManageSlots {
        private IAmazonLexModelBuildingService _lexBuildingClient;
        private BotYamlModel _lexYamlData;

        public ManageSlots(BotYamlModel lexYamlData) {
            _lexBuildingClient = new AmazonLexModelBuildingServiceClient();
            _lexYamlData = lexYamlData;
        }

        public async Task DeleteAll() {
            foreach (var slot in _lexYamlData.Slots) {
                await DeleteSingle(slot);
            }
        }

        public async Task DeleteSingle(PutSlotTypeRequest slot) {
                var response = await DoesSlotExist(slot);
                if (response != null) {
                    await DeleteLexSlot(slot.Name);
                }
        }
        
        private async Task DeleteLexSlot(string slotName) {
            await _lexBuildingClient.DeleteSlotTypeAsync(new DeleteSlotTypeRequest {
                Name = slotName
            });
        }

        public async Task RunUpdate() {
            foreach (var slot in _lexYamlData.Slots) {
                var response = await DoesSlotExist(slot);
                if (response != null) {
                    await UpdateLexSlot(slot, response.Checksum);
                }
                else {
                    await PutLexSlot(slot);
                }
            }
        }

        private async Task PutLexSlot(PutSlotTypeRequest slot) {
            await _lexBuildingClient.PutSlotTypeAsync(slot);
        }
        
        private async Task UpdateLexSlot(PutSlotTypeRequest slot, string checksum = null) {
            if (checksum != null) {
                slot.Checksum = checksum;
            }
            await PutLexSlot(slot);
        }

        private async Task<GetSlotTypeResponse> DoesSlotExist(PutSlotTypeRequest slot) {
            try {
                var response = await _lexBuildingClient.GetSlotTypeAsync(new GetSlotTypeRequest {
                    Name = slot.Name,
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
