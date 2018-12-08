using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.LexModelBuildingService.Model;

namespace LexBot.Generator {
    public class ManageSlots {
        private ILexBotGeneratorDependencyProvider _provider;
        public BotYamlModel botYamlData;

        public ManageSlots(ILexBotGeneratorDependencyProvider provider, BotYamlModel lexYamlData) {
            _provider = provider;
            botYamlData = lexYamlData;
        }

        public async Task DeleteAll() {
            foreach (var slot in botYamlData.Slots) {
                await DeleteSingle(slot);
                Thread.Sleep(3000);
            }
        }

        public async Task DeleteSingle(PutSlotTypeRequest slot) {
                var response = await DoesSlotExist(slot);
                if (response != null) {
                    await DeleteLexSlot(slot.Name);
                }
        }
        
        private async Task DeleteLexSlot(string slotName) {
            await _provider.DeleteSlotTypeAsync(new DeleteSlotTypeRequest {
                Name = slotName
            });
        }

        public async Task RunUpdate() {
            foreach (var slot in botYamlData.Slots) {
                var response = await DoesSlotExist(slot);
                if (response != null) {
                    await UpdateLexSlot(slot, response.Checksum);
                }
                else {
                    slot.Checksum = null;
                    await PutLexSlot(slot);
                }
            }
        }

        private async Task PutLexSlot(PutSlotTypeRequest slot) {
            await _provider.PutSlotTypeAsync(slot);
        }
        
        private async Task UpdateLexSlot(PutSlotTypeRequest slot, string checksum = null) {
            if (checksum != null) {
                slot.Checksum = checksum;
            }
            await PutLexSlot(slot);
        }
        
        public async Task<GetSlotTypeResponse> DoesSlotExist(PutSlotTypeRequest slot) {
            try {
                var response = await _provider.GetSlotTypeAsync(new GetSlotTypeRequest {
                    Name = slot.Name,
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
