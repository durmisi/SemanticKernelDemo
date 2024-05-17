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