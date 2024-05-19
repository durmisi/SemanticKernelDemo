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


        /// <summary>
        /// The idea is to sample multiple, diverse reasoning paths through few-shot CoT, and use the generations to select the most consistent answer. 
        /// This helps to boost the performance of CoT prompting on tasks involving arithmetic and commonsense reasoning.
        /// https://www.promptingguide.ai/techniques/consistency
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Self_Consistency_Prompting_demo_works_as_expected()
        {
            //Arrange
            var builder = CreateBuilder();
            var kernel = builder.Build();

            //Act
            string prompt = @"
                Q: There are 15 trees in the grove. Grove workers will plant trees in the grove today. After they are done,
                there will be 21 trees. How many trees did the grove workers plant today?
                A: We start with 15 trees. Later we have 21 trees. The difference must be the number of trees they planted.
                So, they must have planted 21 - 15 = 6 trees. The answer is 6.

                Q: If there are 3 cars in the parking lot and 2 more cars arrive, how many cars are in the parking lot?
                A: There are 3 cars in the parking lot already. 2 more arrive. Now there are 3 + 2 = 5 cars. The answer is 5.

                Q: Leah had 32 chocolates and her sister had 42. If they ate 35, how many pieces do they have left in total?
                A: Leah had 32 chocolates and Leah’s sister had 42. That means there were originally 32 + 42 = 74
                chocolates. 35 have been eaten. So in total they still have 74 - 35 = 39 chocolates. The answer is 39.

                Q: Jason had 20 lollipops. He gave Denny some lollipops. Now Jason has 12 lollipops. How many lollipops
                did Jason give to Denny?
                A: Jason had 20 lollipops. Since he only has 12 now, he must have given the rest to Denny. The number of
                lollipops he has given to Denny must have been 20 - 12 = 8 lollipops. The answer is 8.

                Q: Shawn has five toys. For Christmas, he got two toys each from his mom and dad. How many toys does
                he have now?
                A: He has 5 toys. He got 2 from mom, so after that he has 5 + 2 = 7 toys. Then he got 2 more from dad, so
                in total he has 7 + 2 = 9 toys. The answer is 9.

                Q: There were nine computers in the server room. Five more computers were installed each day, from
                monday to thursday. How many computers are now in the server room?
                A: There are 4 days from monday to thursday. 5 computers were added each day. That means in total 4 * 5 =
                20 computers were added. There were 9 computers in the beginning, so now there are 9 + 20 = 29 computers.
                The answer is 29.

                Q: Michael had 58 golf balls. On tuesday, he lost 23 golf balls. On wednesday, he lost 2 more. How many
                golf balls did he have at the end of wednesday?
                A: Michael initially had 58 balls. He lost 23 on Tuesday, so after that he has 58 - 23 = 35 balls. On
                Wednesday he lost 2 more so now he has 35 - 2 = 33 balls. The answer is 33.

                Q: Olivia has $23. She bought five bagels for $3 each. How much money does she have left?
                A: She bought 5 bagels for $3 each. This means she spent $15. She has $8 left.

                Q: When I was 6 my sister was half my age. Now I’m 70 how old is my sister?
                A:
            ";

            outputHelper.WriteLine(prompt);

            var result = await kernel.InvokePromptAsync(prompt);
            var response = result.GetValue<string>();

            outputHelper.WriteLine(response);

            //Assert
            Assert.Contains("67", response);
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