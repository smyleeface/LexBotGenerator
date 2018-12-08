

using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.LexModelBuildingService;
using Amazon.LexModelBuildingService.Model;

namespace LexBot.Generator {
    public class BaseLexBotDependencyProvider : ILexBotGeneratorDependencyProvider {
        
        private IAmazonLexModelBuildingService _lexBuildingClient;

        public BaseLexBotDependencyProvider() {
            _lexBuildingClient = new AmazonLexModelBuildingServiceClient();
        }

        Task<DeleteBotResponse> ILexBotGeneratorDependencyProvider.DeleteBotAsync(DeleteBotRequest request) => _lexBuildingClient.DeleteBotAsync(request);
        Task<DeleteIntentResponse> ILexBotGeneratorDependencyProvider.DeleteIntentAsync(DeleteIntentRequest request) => _lexBuildingClient.DeleteIntentAsync(request);
        Task<DeleteSlotTypeResponse> ILexBotGeneratorDependencyProvider.DeleteSlotTypeAsync(DeleteSlotTypeRequest request) => _lexBuildingClient.DeleteSlotTypeAsync(request);
        Task<GetBotResponse> ILexBotGeneratorDependencyProvider.GetBotAsync(GetBotRequest request) => _lexBuildingClient.GetBotAsync(request);
        Task<GetIntentResponse> ILexBotGeneratorDependencyProvider.GetIntentAsync(GetIntentRequest request) => _lexBuildingClient.GetIntentAsync(request);
        Task<GetSlotTypeResponse> ILexBotGeneratorDependencyProvider.GetSlotTypeAsync(GetSlotTypeRequest request) => _lexBuildingClient.GetSlotTypeAsync(request);
        Task<PutBotResponse> ILexBotGeneratorDependencyProvider.PutBotAsync(PutBotRequest request) => _lexBuildingClient.PutBotAsync(request);
        Task<PutIntentResponse> ILexBotGeneratorDependencyProvider.PutIntentAsync(PutIntentRequest request) => _lexBuildingClient.PutIntentAsync(request);
        Task<PutSlotTypeResponse> ILexBotGeneratorDependencyProvider.PutSlotTypeAsync(PutSlotTypeRequest request) => _lexBuildingClient.PutSlotTypeAsync(request);
    }
}