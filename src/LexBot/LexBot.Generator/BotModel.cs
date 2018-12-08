using System.Collections.Generic;
using Amazon.LexModelBuildingService.Model;

namespace LexBot.Generator {
    public class BotYamlModel {
        public string BotName { get; set; }
        public List<PutSlotTypeRequest> Slots { get; set; }
        public List<PutIntentRequest> Intents { get; set; }
        public Prompt ClarificationPrompt { get; set; }
        public Statement AbortStatement { get; set; }
        public List<Intent> IntentsForBot { get; set; }
        public PutBotRequest PutBotRequest { get; set; }
    }
}