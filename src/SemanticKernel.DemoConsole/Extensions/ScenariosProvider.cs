using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

namespace SemanticKernelDemo.Extensions
{
    internal class ScenariosProvider
    {
        internal IEnumerable<Scenario> Get()
        {
            return new Scenario[]
            {
                new Scenario()
                {
                    Name="GenerateStory",
                    Template = "Tell a story about {{$topic}} that is {{$length}} sentences long.",
                    InputVariables = new List<InputVariable> {
                        new()
                        {
                            Name ="topic",
                            IsRequired = true,
                            Description="The topic of the story."
                        },

                        new()
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
                    ExecutionSettings = new OpenAIPromptExecutionSettings()
                    {
                        MaxTokens = 100,
                        Temperature = 0.4,
                        TopP = 1
                    }
                },

                new Scenario()
                {
                    Name="CustomerSuport",
                    Template = @"Act like a customer support service. If the user ask/want to report a problem or create a ticket, always ask question for each input variable until you get a valid value and collect all required input. Always collect {{$firstName}}, {{$lastName}} and {{$problemDescription}} ",
                    InputVariables = new List<InputVariable> {
                        new()
                        {
                            Name ="firstName",
                            IsRequired = true,
                            Description="First name of the user reporting the problem",
                            Default=""
                        },

                        new()
                        {
                            Name ="lastName",
                            IsRequired = true,
                            Description="Last name of the user reporting the problem",
                            Default=""
                        },
                        new()
                        {
                            Name ="emailAdddress",
                            IsRequired = true,
                            Description="Email address of the user reporting the problem",
                            Default=""
                        },
                        new()
                        {
                            Name ="phoneNumber",
                            IsRequired = true,
                            Description="Phone number (Ex: +389 xx xxx xxx)",
                            Default=""
                        },
                        new()
                        {
                            Name ="problemDescription",
                            IsRequired = true,
                            Description="Problem description (max length 400)",
                            Default=""
                        },
                        new()
                        {
                            Name ="confirm",
                            IsRequired = true,
                            Description="Print the data you have collected and ask the user for confirmation before until you get a positive confirmation.",
                            Default=""
                        }
                    },
                    OutputVariable = new OutputVariable()
                    {
                        Description = "The generated object in json format.",
                    },
                    ExecutionSettings = new OpenAIPromptExecutionSettings()
                    {
                        MaxTokens = 100,
                        Temperature = 0.4,
                        TopP = 1
                    }
                }
            };
        }
    }
}
