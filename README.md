# LexBotGenerator

Create, Update, or Delete Lex Bot from a YAML file.
The LexBot Generator can be used to initialize a LexBot.
It can also be used as a library to manage slot values from external sources.

# How to use

* dotnet core 2.0x
* aws account and aws cli credentials configured with elevated permission to manage the lex bot service.

## LexBot File Definition

Example of the LexBot Yaml File Definition can be found at `src/LexBot/LexBot.Generator/Tests/Fixtures/LexDefinition.yml`
This is the default file used in the test and in the program run demo.

## Run Demo from Local

From the `LexBot.Generator` directory, run the following:

### Setup LexBot

```sh
cd src/LexBot/LexBot.Generator/
dotnet run setup
```

### Teardown LexBot

```sh
cd src/LexBot/LexBot.Generator/
dotnet run teardown
```

## Update Slot Values after existing

- Copy the `src/LexBot/LexBot.Generator/Tests/Fixtures/LexDefinition.yml` from this repo and include in your application.
- Add `LexBot.Generator` to the project.
- Update slot data
    ```
    TextReader reader;
    
    // Load data from yaml
    var parseLexYaml = new ParseLexYaml(reader);
    var lexYamlData = parseLexYaml.Run();
    
    // Use LexBotDependencyProvider or create own
    var lexbotProvider = new BaseLexBotDependencyProvider();
    var manageSlots = new ManageSlots(lexbotProvider, lexYamlData);
    
    // Gather data, put in list of `EnumerationValues`
    ...
    
    // Update the slot
    await UpdateSlot(manageSlot.Name, listOfEnumerationValues);
    ```