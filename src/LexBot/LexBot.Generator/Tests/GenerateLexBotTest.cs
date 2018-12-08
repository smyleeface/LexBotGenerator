using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.LexModelBuildingService;
using Amazon.LexModelBuildingService.Model;
using Moq;
using Xunit;

namespace LexBot.Generator.Tests {
    public class GenerateLexBotTest {
        
        private GetSlotTypeRequest _environmentGetSlotRequest = new GetSlotTypeRequest {
            Name = "EnvironmentA",
            Version = "$LATEST"
        };

        private GetSlotTypeRequest _branchGetSlotRequest = new GetSlotTypeRequest {
            Name = "BranchA",
            Version = "$LATEST"
        };
        
        private PutSlotTypeRequest _environmentPutSlotRequest = new PutSlotTypeRequest {
            Name = "EnvironmentA",
            Checksum = null
        };
        
        private PutSlotTypeRequest _branchPutSlotRequest = new PutSlotTypeRequest {
            Name = "BranchA",
            Checksum = "fghijk"
        };
        
        private GetIntentRequest _deployGetIntent = new GetIntentRequest {
            Name = "DeployA",
            Version = "$LATEST"
        };
        
        private PutIntentRequest _deployPutIntent = new PutIntentRequest {
            Name = "DeployA",
            Checksum = "zyxwvu",
            SampleUtterances = new List<string> {
                "deploy {Branch}",
                "deploy {Environment}"
            },
            ConclusionStatement = new Statement {
                Messages = new List<Message> {
                    new Message {
                        Content = "Bye",
                        ContentType = "PlainText"
                    }
                }
            },
            FulfillmentActivity = new FulfillmentActivity {
                Type = new FulfillmentActivityType("ReturnIntent")
            },
            Slots = new List<Slot> {
                new Slot {
                    Name = "Branch",
                    SlotType = "BranchA",
                    SlotConstraint = new SlotConstraint("Required"),
                    SlotTypeVersion = "$LATEST",
                    Priority = 1,
                    ValueElicitationPrompt = new Prompt {
                        MaxAttempts = 3,
                        Messages = new List<Message> {
                            new Message {
                                Content = "Which Branch?",
                                ContentType = new ContentType("PlainText")
                            }
                        }
                    }
                },
                new Slot {
                    Name = "Environment",
                    SlotType = "EnvironmentA",
                    SlotConstraint = new SlotConstraint("Required"),
                    SlotTypeVersion = "$LATEST",
                    Priority = 2,
                    ValueElicitationPrompt = new Prompt {
                        MaxAttempts = 3,
                        Messages = new List<Message> {
                            new Message {
                                Content = "Which Environment?",
                                ContentType = new ContentType("PlainText")
                            }
                        }
                    }
                }
            }
        };

        private PutBotRequest _putBotRequest = new PutBotRequest {
            Name = "SampleTestCreateBot",
            Intents = new List<Intent> {
                new Intent {
                    IntentName = "DeployA",
                    IntentVersion = "$LATEST"
                }
            },
            AbortStatement = new Statement {
                Messages = new List<Message> {
                    new Message {
                        Content = "Good Bye"
                    }
                }
            },
            ClarificationPrompt = new Prompt {
                Messages = new List<Message> {
                    new Message {
                        Content = "I didn't understand, please try again"
                    }
                },
                MaxAttempts = 3
            }
        };

        private string _localFilePath = "Tests.Fixtures.LexDefinition.yml";
        
        [Fact]
        public void parse_lex_yaml() {
            
            // Arrange
            var parseYaml = ReadLocalFile.Run(_localFilePath);
            
            // Act
            var lexYamlData = parseYaml.Run();
            
            // Assert
            Assert.Equal("SampleTestCreateBot", lexYamlData.BotName);
            Assert.Equal("Good Bye", lexYamlData.AbortStatement.Messages[0].Content);
            Assert.Equal("PlainText", lexYamlData.AbortStatement.Messages[0].ContentType.ToString());
            Assert.Equal(3, lexYamlData.ClarificationPrompt.MaxAttempts);
            Assert.Equal("I didn't understand, please try again", lexYamlData.ClarificationPrompt.Messages[0].Content);
            Assert.Equal("PlainText", lexYamlData.ClarificationPrompt.Messages[0].ContentType.ToString());
            Assert.Equal("DeployA", lexYamlData.Intents[0].Name);
            Assert.Equal("Branch", lexYamlData.Intents[0].Slots[0].Name);
            Assert.Equal("EnvironmentA", lexYamlData.Slots[0].Name);
            Assert.Equal("smyleeface", lexYamlData.Slots[0].EnumerationValues[0].Value);
        }
        
        [Fact]
        public async Task create_or_update_lex_slot() {
            
            // Arrange
            var parseYaml = ReadLocalFile.Run(_localFilePath);
            var lexYamlData = parseYaml.Run();
            Mock<ILexBotGeneratorDependencyProvider> provider = new Mock<ILexBotGeneratorDependencyProvider>(MockBehavior.Strict);
            
            // EnvironmentA get slot
            provider.Setup(x => x.GetSlotTypeAsync(It.Is<GetSlotTypeRequest>(y =>
                y.Name == _environmentGetSlotRequest.Name && y.Version == "$LATEST"
            ))).Returns(Task.FromException<GetSlotTypeResponse>(new Exception("error")));

            // Branch get slot
            provider.Setup(x => x.GetSlotTypeAsync(It.Is<GetSlotTypeRequest>(y =>
                y.Name == _branchGetSlotRequest.Name && y.Version == "$LATEST"
            ))).Returns(Task.FromResult(new GetSlotTypeResponse {
                Name = "BranchA",
                Checksum = "fghijk"
            }));
            
            // EnvironmentA put slot
            provider.Setup(x => x.PutSlotTypeAsync(It.Is<PutSlotTypeRequest>(y =>
                y.Name == _environmentPutSlotRequest.Name && y.Checksum == _environmentPutSlotRequest.Checksum
            ))).Returns(Task.FromResult(new PutSlotTypeResponse {
                Name = "EnvironmentA",
                Checksum = "abcde"
            }));
            
            // Branch put slot
            provider.Setup(x => x.PutSlotTypeAsync(It.Is<PutSlotTypeRequest>(y =>
                y.Name == _branchPutSlotRequest.Name && y.Checksum == _branchPutSlotRequest.Checksum
            ))).Returns(Task.FromResult(new PutSlotTypeResponse {
                Name = "BranchA",
                Checksum = "lmnob"
            }));
            var manageSlots = new ManageSlots(provider.Object, lexYamlData);
            
            // Act
            // Assert
            await manageSlots.RunUpdate();            
        }

        [Fact]
        public async Task create_or_update_lex_intent() {
            
            // Arrange
            var parseYaml = ReadLocalFile.Run(_localFilePath);
            var lexYamlData = parseYaml.Run();
            Mock<ILexBotGeneratorDependencyProvider> provider = new Mock<ILexBotGeneratorDependencyProvider>(MockBehavior.Strict);

            // DeployA get intent
            provider.Setup(x => x.GetIntentAsync(It.Is<GetIntentRequest>(y => 
                y.Name == _deployGetIntent.Name && 
                y.Version == _deployGetIntent.Version
            ))).Returns(Task.FromResult(new GetIntentResponse {
                Name = "DeployA",
                Checksum = "zyxwvu"
            }));
            provider.Setup(x => x.PutIntentAsync(It.Is<PutIntentRequest>(y => 
                y.Name == _deployPutIntent.Name && 
                y.Checksum == _deployPutIntent.Checksum &&
                y.SampleUtterances[0] == _deployPutIntent.SampleUtterances[0] &&
                y.SampleUtterances[1] == _deployPutIntent.SampleUtterances[1] &&
                y.ConclusionStatement.Messages[0].Content == _deployPutIntent.ConclusionStatement.Messages[0].Content && 
                y.FulfillmentActivity.Type.Value == _deployPutIntent.FulfillmentActivity.Type.Value &&
                y.Slots[0].Name == _deployPutIntent.Slots[0].Name &&
                y.Slots[1].Name == _deployPutIntent.Slots[1].Name &&
                y.Slots[0].SlotType == _deployPutIntent.Slots[0].SlotType
            ))).Returns(Task.FromResult(new PutIntentResponse {
                Name = "DeployA",
                Checksum = "qwert"
            }));
            var manageIntents = new ManageIntents(provider.Object, lexYamlData);
            
            // Act
            // Assert
            await manageIntents.RunUpdate();            
        }

        [Fact]
        public async Task create_or_update_lex_bot() {
            
            // Arrange
            var parseYaml = ReadLocalFile.Run(_localFilePath);
            var lexYamlData = parseYaml.Run();  
            Mock<ILexBotGeneratorDependencyProvider> provider = new Mock<ILexBotGeneratorDependencyProvider>(MockBehavior.Strict);
            provider.Setup(x => x.PutBotAsync(It.Is<PutBotRequest>(y => 
                y.Name == _putBotRequest.Name &&
                y.Intents[0].IntentName == _putBotRequest.Intents[0].IntentName &&
                y.ClarificationPrompt.MaxAttempts == _putBotRequest.ClarificationPrompt.MaxAttempts &&
                y.ClarificationPrompt.Messages[0].Content == _putBotRequest.ClarificationPrompt.Messages[0].Content &&
                y.AbortStatement.Messages[0].Content == _putBotRequest.AbortStatement.Messages[0].Content
            ))).Returns(Task.FromResult(new PutBotResponse {
                Name = "DeployA",
                Checksum = "qwert"
            }));
            var manageBots = new ManageBots(provider.Object, lexYamlData);
            
            // Act
            // Assert
            await manageBots.RunUpdate();
        }
    }
}