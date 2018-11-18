using System.Collections;
using System.Collections.Generic;
using Amazon.LexModelBuildingService.Model;
using Newtonsoft.Json;

namespace LexBot.SlackBot {
    public class BotYamlModel {
        public string BotName { get; set; }
        public IEnumerable<PutSlotTypeRequest> Slots { get; set; }
        public IEnumerable<PutIntentRequest> Intents { get; set; }
        public Prompt ClarificationPrompt { get; set; }
        public Statement AbortStatement { get; set; }
        public IEnumerable<Intent> IntentsForBot { get; set; }
        public PutBotRequest PutBotRequest { get; set; }
    }
}