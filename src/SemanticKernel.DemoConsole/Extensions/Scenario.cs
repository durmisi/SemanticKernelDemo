using Microsoft.SemanticKernel;

namespace SemanticKernelDemo.Extensions
{
    internal class Scenario
    {
        public string Name { get; set; }

        public string Template { get; set; }


        public List<InputVariable> InputVariables { get; set; }

        public OutputVariable OutputVariable { get; set; }

        public PromptExecutionSettings ExecutionSettings { get; set; }
    }
}
