using System.Threading.Tasks;
using Amazon.LexModelBuildingService.Model;

namespace LexBot.Generator {
    public interface ILexBotGeneratorDependencyProvider {
        Task<DeleteBotResponse> DeleteBotAsync(DeleteBotRequest request);
        Task<DeleteIntentResponse> DeleteIntentAsync(DeleteIntentRequest request);
        Task<DeleteSlotTypeResponse> DeleteSlotTypeAsync(DeleteSlotTypeRequest request);
        Task<GetBotResponse> GetBotAsync(GetBotRequest request);
        Task<GetIntentResponse> GetIntentAsync(GetIntentRequest request);
        Task<GetSlotTypeResponse> GetSlotTypeAsync(GetSlotTypeRequest request);
        Task<PutBotResponse> PutBotAsync(PutBotRequest request);
        Task<PutIntentResponse> PutIntentAsync(PutIntentRequest request);
        Task<PutSlotTypeResponse> PutSlotTypeAsync(PutSlotTypeRequest request);
    }
}