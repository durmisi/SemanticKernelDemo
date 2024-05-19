using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Xunit.Abstractions;

namespace SemanticKernel.Demo.Tests
{
    public class PromptsTests
    {
        private readonly ITestOutputHelper outputHelper;

        public PromptsTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }


        /// <summary>
        /// Zero-shot prompting means that the prompt used to interact with the model won't contain examples or demonstrations. 
        /// The zero-shot prompt directly instructs the model to perform a task without any additional examples to steer it.
        /// https://www.promptingguide.ai/techniques/zeroshot
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Zero_Shot_Prompting_demo_works_as_expected()
        {
            //Arrange
            var builder = CreateBuilder();
            var kernel = builder.Build();

            //Act
            string prompt = @"
                    Classify the text into neutral, negative or positive. 
                    Text: I think the vacation is okay.
                    Sentiment:
            ";

            outputHelper.WriteLine(prompt);

            var result = await kernel.InvokePromptAsync(prompt);
            var response = result.GetValue<string>();

            outputHelper.WriteLine(response);

            //Assert
            Assert.Equal("Neutral", response);
        }

        /// <summary>
        ///  Few-shot prompting can be used as a technique to enable in-context learning 
        ///  where we provide demonstrations in the prompt to steer the model to better performance. 
        ///  https://www.promptingguide.ai/techniques/fewshot
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Few_Shot_Prompting_demo_works_as_expected()
        {
            //Arrange
            var builder = CreateBuilder();
            var kernel = builder.Build();

            //Act
            string prompt = @"
                    Positive This is awesome! 
                    This is bad! Negative
                    Wow that movie was rad!
                    Positive
                    What a horrible show! --
            ";

            outputHelper.WriteLine(prompt);

            var result = await kernel.InvokePromptAsync(prompt);
            var response = result.GetValue<string>();

            outputHelper.WriteLine(response);

            //Assert
            Assert.Equal("Negative", response);
        }


        /// <summary>
        /// chain-of-thought (CoT) prompting enables complex reasoning capabilities through intermediate reasoning steps. 
        /// You can combine it with few-shot prompting to get better results on more complex tasks 
        /// that require reasoning before responding.
        /// https://www.promptingguide.ai/techniques/cot
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Chain_of_Thought_Prompting_demo_works_as_expected()
        {
            //Arrange
            var builder = CreateBuilder();
            var kernel = builder.Build();
                
            //Act
            string prompt = @"
                The odd numbers in this group add up to an even number: 4, 8, 9, 15, 12, 2, 1.
                A: Adding all the odd numbers (9, 15, 1) gives 25. The answer is False.
                The odd numbers in this group add up to an even number: 15, 32, 5, 13, 82, 7, 1. 
                A:
            ";

            outputHelper.WriteLine(prompt);

            var result = await kernel.InvokePromptAsync(prompt);
            var response = result.GetValue<string>();

            outputHelper.WriteLine(response);

            //Assert
            Assert.Equal("Adding all the odd numbers (15, 5, 13, 7, 1) gives 41. The answer is False.", response);
        }

        private static IKernelBuilder CreateBuilder(Action<IKernelBuilder>? configure = null)
        {
            var builder = Kernel.CreateBuilder();

            if (configure != null)
            {
                configure(builder);
            }
            else
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                string apiKey = config.GetValue<string>("AzureOpenAI:ApiKey")!;
                string endpoint = config.GetValue<string>("AzureOpenAI:Endpoint")!;
                string deploymentName = config.GetValue<string>("AzureOpenAI:DeploymentName")!;

                builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

            }

            return builder;

        }
    }
}