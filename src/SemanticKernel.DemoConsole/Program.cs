using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelDemo.Extensions;

var kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug().AddConsole());


kernelBuilder.AddAzureOpenAIChatCompletion(
         "gpt-35-turbo-0613",                      // Azure OpenAI Deployment Name
         "https://{instance}.openai.azure.com/", // Azure OpenAI Endpoint
         "{key}",
         "AzureOpenAI");      // Azure OpenAI Key


var kernel = kernelBuilder.Build();


var dynaFuncions = new List<KernelFunction>();

foreach (var scenario in new ScenariosProvider().Get())
{

    var dynaFunc = kernel.CreateFunctionFromPrompt(new PromptTemplateConfig()
    {
        Name = scenario.Name,
        Template = scenario.Template,
        InputVariables = scenario.InputVariables,
        OutputVariable = scenario.OutputVariable,
        ExecutionSettings = new Dictionary<string, PromptExecutionSettings>()
        {
            {"AzureOpenAI", scenario.ExecutionSettings }
        }
    });

    dynaFuncions.Add(dynaFunc);
}

kernel.ImportPluginFromFunctions("DynaFunctions", dynaFuncions);

#pragma warning disable SKEXP0004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
void MyPreHandler(object? sender, FunctionInvokingEventArgs e)
{
    Console.WriteLine($"{e.Function.Name} : Pre Execution Handler - Triggered");

    foreach (var item in e.Arguments)
    {
        if (item.Key == "confim" && item.Value == null || item.Value?.ToString() != "yes")
        {
            //TODO: find a way to ask for confirmation again
        }
    }



}

void MyPostExecutionHandler(object? sender, FunctionInvokedEventArgs e)
{
    Console.WriteLine($"{e.Function.Name} : Post Execution Handler - Usage: {e.Result.Metadata?["Usage"]?.ToString()}");

    foreach (var item in e.Arguments)
    {
        Console.WriteLine($"{item.Key}:{item.Value}");
    }

    //TODO: invoke the scenario handler
}

kernel.FunctionInvoking += MyPreHandler;
kernel.FunctionInvoked += MyPostExecutionHandler;
#pragma warning restore SKEXP0004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


// Retrieve the chat completion service from the kernel
IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Create the chat history

var systemMessage = new List<ChatMessageContent> {

    new (AuthorRole.System, @"
        Please don`t answer on question outside of your tools knowledge! 
        Always clear your history when start a new session.
    ")
};

ChatHistory chatMessages = new ChatHistory(systemMessage);


// Start the conversation

Console.WriteLine("Hi, Welcome to your virtual chat assistant, type \"quit\" if you want to leave.");

while (true)
{
    // Get user input
    Console.Write("User: ");
    string question = Console.ReadLine() ?? string.Empty;
    if (question == "quit")
    {
        break;
    }

    if (string.IsNullOrEmpty(question))
    {
        continue;
    }

    chatMessages.AddUserMessage(question!);

    // Get the chat completions
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        Temperature = 0,
        MaxTokens = 200
    };

    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
        chatMessages,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Stream the results
    string fullMessage = "";
    await foreach (var content in result)
    {

        if (content.Content == null)
        {
            continue;
        }

        if (content.Role.HasValue)
        {
            System.Console.Write("Assistant > ");
        }
        System.Console.Write(content.Content);
        fullMessage += content.Content;
    }
    System.Console.WriteLine();

    // Add the message from the agent to the chat history
    chatMessages.AddAssistantMessage(fullMessage);
}