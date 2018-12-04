using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Configuration;

namespace Knnithyanand.Demo.WebJobs.PdfConverter
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var loggerFactory = new LoggerFactory();
            var instrumentationKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
            config.DashboardConnectionString = "";
            config.LoggerFactory = loggerFactory
                .AddApplicationInsights(instrumentationKey, null)
                .AddConsole();

            var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }
}
