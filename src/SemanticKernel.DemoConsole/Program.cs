using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using SemanticKernelDemo.Plugins;
using System.Reflection;
using System.Reflection.PortableExecutable;

var kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug().AddConsole());


kernelBuilder.AddAzureOpenAIChatCompletion(
         "gpt-35-turbo-0613",                      // Azure OpenAI Deployment Name
         "https://{instance}.openai.azure.com/", // Azure OpenAI Endpoint
         "-----------------------------",
         "AzureOpenAI");      // Azure OpenAI Key


//kernelBuilder.Plugins.AddFromType<AuthorEmailPlanner>();
//kernelBuilder.Plugins.AddFromType<EmailPlugin>();


var kernel = kernelBuilder.Build();


KernelFunction dynaFunc = kernel.CreateFunctionFromPrompt(new PromptTemplateConfig()
{
    Name = "GenerateStory",
    Template = "Tell a story about {{$topic}} that is {{$length}} sentences long.",
    InputVariables = new List<InputVariable> {
        new InputVariable()
        {
            Name ="topic",
            IsRequired = true,
            Description="The topic of the story."
        },

        new InputVariable()
        {
            Name ="length",
            IsRequired = true,
            Description="The number of sentences in the story."
        }
    },
    OutputVariable = new OutputVariable()
    {
        Description = "The generated story.",
    },
    ExecutionSettings = new Dictionary<string, PromptExecutionSettings>()
    {
        {"AzureOpenAI", new OpenAIPromptExecutionSettings() { MaxTokens = 100, Temperature = 0.4, TopP = 1 } }
    }
});

kernel.ImportPluginFromFunctions("Dynamic", new[] { dynaFunc });

#pragma warning disable SKEXP0004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
void MyPreHandler(object? sender, FunctionInvokingEventArgs e)
{
    Console.WriteLine($"{e.Function.Name} : Pre Execution Handler - Triggered");
}

void MyPostExecutionHandler(object? sender, FunctionInvokedEventArgs e)
{
    Console.WriteLine($"{e.Function.Name} : Post Execution Handler - Usage: {e.Result.Metadata?["Usage"]?.ToString()}");
}

kernel.FunctionInvoking += MyPreHandler;
kernel.FunctionInvoked += MyPostExecutionHandler;
#pragma warning restore SKEXP0004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


// Retrieve the chat completion service from the kernel
IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Create the chat history
ChatHistory chatMessages = new ChatHistory();

// Start the conversation
while (true)
{
    // Get user input
    System.Console.Write("User > ");
    chatMessages.AddUserMessage(Console.ReadLine()!);

    // Get the chat completions
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
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