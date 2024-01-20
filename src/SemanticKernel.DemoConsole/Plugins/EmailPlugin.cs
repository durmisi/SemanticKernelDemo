using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SemanticKernelDemo.Plugins
{
    /// <summary>
    /// A plugin that returns the current time.
    /// </summary>
    public class EmailPlugin
    {
        [KernelFunction]
        [Description("Sends an email to support.")]
        public async Task SendEmailAsync(
            Kernel kernel,
            [Description("Semicolon delimitated list of emails of the recipients")] string recipientEmails,
            string subject,
            string body
        )
        {
            // Add logic to send an email using the recipientEmails, subject, and body
            // For now, we'll just print out a success message to the console
            Console.WriteLine("Email sent!");
        }
    }
}
