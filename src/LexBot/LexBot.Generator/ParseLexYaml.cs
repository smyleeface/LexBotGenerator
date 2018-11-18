using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.LexModelBuildingService;
using Amazon.LexModelBuildingService.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.RepresentationModel;

namespace LexBot.SlackBot {
    public class ParseLexYaml {
       
        public BotYamlModel Run() {
            return ReadFromYamlToObject();            
        }

        private BotYamlModel ReadFromYamlToObject() {
            var lexYamlData = new BotYamlModel();
            
            // open the file
            //TODO: parameterize the filename
            string filePath = "/Users/pattyr/Projects/SMYLEECOM/SmyleeGitHubEventHub/src/LexBots/LexBot.SlackBot/Lex.yml";
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            StreamReader streamReader = new StreamReader(fileStream);
            StringReader yamlString = new StringReader(streamReader.ReadToEnd());
            var yaml = new YamlStream();
            yaml.Load(yamlString);
            var yamlMappingNode = (YamlMappingNode) yaml.Documents[0].RootNode;
            fileStream.Close();

            // get the bot name
            lexYamlData.BotName = ParseBotName(yamlMappingNode);
            
            // get the slots
            lexYamlData.Slots = ParseSlots(yamlMappingNode);
            
            // get the intents
            lexYamlData.Intents = ParseIntents(yamlMappingNode);
            
            // get the error handling information
            var errorHandling = (YamlMappingNode) yamlMappingNode.Children[new YamlScalarNode("ErrorHandling")];
            
            // get clarification prompt
            lexYamlData.ClarificationPrompt = ParseClarificationPrompt(errorHandling);
            
            // get abort statements
            lexYamlData.AbortStatement = ParseAbortStatements(errorHandling);
            
            // get intents for bot
            lexYamlData.IntentsForBot = ParseIntentsForBot(yamlMappingNode);
                
            // put bot request
            lexYamlData.PutBotRequest = GeneratePutBotRequest(lexYamlData); 
            return lexYamlData;
        }

        private PutBotRequest GeneratePutBotRequest(BotYamlModel lexYamlData) {
            return new PutBotRequest {
                Name = lexYamlData.BotName,
                Locale = "en-US",
                ChildDirected = false,
                ClarificationPrompt = lexYamlData.ClarificationPrompt,
                AbortStatement = lexYamlData.AbortStatement,
                Intents = lexYamlData.IntentsForBot.ToList()
            };

        }

        private IEnumerable<Intent> ParseIntentsForBot(YamlMappingNode yamlMappingNode) {
            var parsedIntentsForBot = new List<Intent>();
            var intents = (YamlSequenceNode) yamlMappingNode.Children[new YamlScalarNode("Intents")];
            foreach (var intent in intents.Children.ToList()) {
                var yamlIntentMapping = (YamlMappingNode) intent;
                var intentName = yamlIntentMapping.Children[new YamlScalarNode("Intent")].ToString();
                var intentForBot = new Intent {
                    IntentName = intentName,
                    IntentVersion = "$LATEST"
                };
                parsedIntentsForBot.Add(intentForBot);
            }

            return parsedIntentsForBot;
        }

        private IEnumerable<PutIntentRequest> ParseIntents(YamlMappingNode yamlMappingNode) {
            var intents = (YamlSequenceNode) yamlMappingNode.Children[new YamlScalarNode("Intents")];
            var listIntentRequests = new List<PutIntentRequest>();
            foreach (var intent in intents.Children.ToList()) {
                var yamlIntentMapping = (YamlMappingNode) intent;
                var intentName = yamlIntentMapping.Children[new YamlScalarNode("Intent")].ToString();
                var listOfIntentSlots = ParseIntentSlots(yamlIntentMapping, intentName);
                var listOfSampleUtterances = ParseSampleUtterances(yamlIntentMapping);
                var conclusionStatements = ParseConclusionStatement(yamlIntentMapping);
                var fulfillmentActivity = ParseFulfilmentActivity(yamlIntentMapping);
                var putIntentRequest = new PutIntentRequest {
                    Name = intentName,
                    Slots = listOfIntentSlots.ToList(),
                    SampleUtterances = listOfSampleUtterances.ToList(),
                    ConclusionStatement = conclusionStatements,
                    FulfillmentActivity = fulfillmentActivity
                };
                listIntentRequests.Add(putIntentRequest);
            }
            return listIntentRequests;
        }

        private FulfillmentActivity ParseFulfilmentActivity(YamlMappingNode yamlIntentMapping) {
            try {
                var fulfillmentActivity = yamlIntentMapping.Children[new YamlScalarNode("FulfillmentActivity")].ToString();
                
                // check that it is a lambda arn
                var LAMBDA_ARN_REGEX = @"arn:(aws[a-zA-Z-]*)?:lambda:[a-z]{2}((-gov)|(-iso(b?)))?-[a-z]+-\d{1}:\d{12}:function:[a-zA-Z0-9-_]+(:(\$LATEST|[a-zA-Z0-9-_]+))?";
                var regex = new Regex(LAMBDA_ARN_REGEX);
                var matchArn = regex.Matches(fulfillmentActivity);
                if (matchArn.Count > 0) {
                    return new FulfillmentActivity {
                        Type = FulfillmentActivityType.CodeHook,
                        CodeHook = new CodeHook {
                            Uri = fulfillmentActivity
                        }
                    };
                }
            }
            catch (Exception e) {
                // ignored
            }
            return new FulfillmentActivity {
                Type = FulfillmentActivityType.ReturnIntent
            };            
        }

        private Statement ParseConclusionStatement(YamlMappingNode yamlIntentMapping) {
            var conclusionStatementPrompt = (YamlSequenceNode) yamlIntentMapping.Children[new YamlScalarNode("ConclusionStatement")];
            var listOfConclusionStatementPromptMessages = ParseMessagesList(conclusionStatementPrompt);
            return new Statement {
                Messages = listOfConclusionStatementPromptMessages.ToList()
            };
        }

        private IEnumerable<string> ParseSampleUtterances(YamlMappingNode yamlIntentMapping) {
            var listOfSampleUtterances = new List<string>();
            var sampleUtterances = ((YamlSequenceNode) yamlIntentMapping.Children[new YamlScalarNode("SampleUtterances")]).Children;
            foreach (var sampleUtterance in sampleUtterances) {
                listOfSampleUtterances.Add(sampleUtterance.ToString());
            }
            return listOfSampleUtterances;
        }

        private IEnumerable<Slot> ParseIntentSlots(YamlMappingNode yamlIntentMapping, string intentName) {
            var listOfIntentSlots = new List<Slot>();
            var listOfSlots = (YamlSequenceNode) yamlIntentMapping.Children[new YamlScalarNode("Slots")];
            foreach (var slotData in listOfSlots) {
                var slot = (YamlMappingNode) slotData;
                var parsedIntentSlots = ParseIntentSlot(intentName, slot);
                listOfIntentSlots.Add(parsedIntentSlots);
            }

            return listOfIntentSlots;
        }

        private string ParseBotName(YamlMappingNode yamlMappingNode) {
            return yamlMappingNode.Children[new YamlScalarNode("Bot")].ToString();
        }

        public Message ParseMessage(YamlMappingNode messageYaml) {
            var message = messageYaml.Children[new YamlScalarNode("Message")].ToString();
            string contentType;
            try {
                contentType = messageYaml.Children[new YamlScalarNode("ContentType")].ToString();
            }
            catch (Exception e) {
                contentType = "";
            }
            return GenerateMessage (message, contentType);
        }

        public IEnumerable<Message> ParseMessagesList(YamlSequenceNode messagesYamlList) {
            var listOfValueElicitationPromptMessages = new List<Message>();
            foreach (var messageYaml in messagesYamlList) {
                var abortStatementMessageNode = (YamlMappingNode) messageYaml;
                var statementMessage = ParseMessage(abortStatementMessageNode);
                listOfValueElicitationPromptMessages.Add(statementMessage);
            }
            return listOfValueElicitationPromptMessages;
        }

        public Slot ParseIntentSlot(string intentName, YamlMappingNode slotData) {
            var slotName = slotData.Children[new YamlScalarNode("Name")].ToString();
            var slotType = slotData.Children[new YamlScalarNode("SlotType")].ToString();
            if (!Int32.TryParse(slotData.Children[new YamlScalarNode("Priority")].ToString(), out var priority)) {
                priority = 0;
            }
            var slotTypeVersion = slotData.Children[new YamlScalarNode("SlotTypeVersion")].ToString();
            var valueElicitationPrompt = (YamlMappingNode) slotData.Children[new YamlScalarNode("ValueElicitationPrompt")];
            var valueElicitationPromptMaxRetries = Int32.Parse(valueElicitationPrompt.Children[new YamlScalarNode("MaxAttempts")].ToString());
            var valueElicitationPromptMessages = (YamlSequenceNode) valueElicitationPrompt.Children[new YamlScalarNode("Messages")];
            var listOfValueElicitationPromptMessages = ParseMessagesList(valueElicitationPromptMessages);
            return new Slot {
                Name = slotName,
                SlotType = slotType,
                SlotConstraint = SlotConstraint.Required,
                SlotTypeVersion = slotTypeVersion,
                ValueElicitationPrompt = new Prompt {
                    Messages = listOfValueElicitationPromptMessages.ToList(),
                    MaxAttempts = valueElicitationPromptMaxRetries
                },
                Priority = priority
            };
        }

        public Message GenerateMessage(string content, string sourceContentType) {
            var contentType = ContentType.PlainText;
            switch (sourceContentType) {
                case "SSML":
                    contentType = ContentType.SSML;
                    break;
                case "CustomPayload":
                    contentType = ContentType.CustomPayload;
                    break;
                case "PlainText":
                case null:
                case "":
                    break;
                default:
                    throw new Exception($"ContentType {sourceContentType} is not valid.");
            }
            return new Message {
                Content = content,
                ContentType = contentType
            };
        }

        public Statement ParseAbortStatements(YamlMappingNode yamlErrorHandling) {
            var messages = new List<Message>();
            var abortStatementMessages = ((YamlSequenceNode) yamlErrorHandling.Children[new YamlScalarNode("AbortStatements")]).Children.ToList();
            foreach (var abortStatementMessage in abortStatementMessages) {
                var message = ((YamlMappingNode) abortStatementMessage).Children[new YamlScalarNode("Message")].ToString();
                string contentType;
                try {
                    contentType = ((YamlMappingNode) abortStatementMessage).Children[new YamlScalarNode("ContentType")].ToString();
                }
                catch (Exception e) {
                    contentType = "";
                }
                var statementMessage = GenerateMessage (message, contentType);
                messages.Add(statementMessage);
            }
            return new Statement {
                Messages = messages
            };
        }
        
        public Prompt ParseClarificationPrompt(YamlMappingNode yamlErrorHandling) {
            var messages = new List<Message>();
            var maxRetries = Int32.Parse(yamlErrorHandling.Children[new YamlScalarNode("MaxRetries")].ToString());
            var clarificationPromptMessages = ((YamlSequenceNode) yamlErrorHandling.Children[new YamlScalarNode("ClarificationPrompts")]).Children.ToList();
            foreach (var clarificationPromptMessage in clarificationPromptMessages) {
                var message = ((YamlMappingNode) clarificationPromptMessage).Children[new YamlScalarNode("Message")].ToString();
                var contentType = "";
                try {
                    contentType = ((YamlMappingNode) clarificationPromptMessage).Children[new YamlScalarNode("ContentType")].ToString();
                }
                catch (Exception e) {
                    continue;
                }
                var promptMessage = GenerateMessage (message, contentType);
                messages.Add(promptMessage);
            }
            return new Prompt {
                Messages = messages,
                MaxAttempts = maxRetries
            };
        }
        
        public IEnumerable<PutSlotTypeRequest> ParseSlots(YamlMappingNode yamlMappingNode) {
            
            var putSlotTypeRequests = new List<PutSlotTypeRequest>();
            var yamlSlots = (YamlSequenceNode) yamlMappingNode.Children[new YamlScalarNode("Slots")];
            foreach (var yamlSlot in yamlSlots) {
                var yamlSlotMapping = (YamlMappingNode) yamlSlot;
                var slotObject = ParseSlot(yamlSlotMapping);
                putSlotTypeRequests.Add(slotObject);
            }
            return putSlotTypeRequests;
        }

        public PutSlotTypeRequest ParseSlot(YamlMappingNode yamlSlotMapping) {
            var enumerationValues = ((YamlSequenceNode) yamlSlotMapping.Children[new YamlScalarNode("EnumerationValues")]).Children;
            var listOfEnumerationValues = enumerationValues
                .Select(enumerationValue => new EnumerationValue {Value = enumerationValue.ToString()})
                .ToList();
            return new PutSlotTypeRequest {
                Name = yamlSlotMapping.Children[new YamlScalarNode("Slot")].ToString(),
                EnumerationValues = listOfEnumerationValues,
                Checksum = "$LATEST"
            };
        }
    }
}
