using Azure;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BlzWApp4.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlzWApp4.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private List<WeatherForecast> WeatherForecastList = new List<WeatherForecast>();

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;

            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                 }
            };

            string mi_clientId = "d42926b7-9465-4672-b69d-1b31f8f9f6bf";
            //string keyvault_name = "kvblobtbldiplom1";
            string secret_name = "storageAccountKey";

            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = mi_clientId });
            string keyVaultUrl = @"https://kvblobtbldiplom1.vault.azure.net/";
            var kvclient = new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: credential, options: options);

            logger.LogInformation("SecretClient is ready");

            KeyVaultSecret secret = kvclient.GetSecret(secret_name);
            string storageAccountKey = secret.Value;

            logger.LogInformation("storageAccountKey is OK");

            string storageUri = @"https://staccblobtbldiplom2.table.core.windows.net/tableabc1";
            string tableName = "tableabc1";
            string accountName = "staccblobtbldiplom2";

            var tableClient = new TableClient(new Uri(storageUri),
                tableName,
                new TableSharedKeyCredential(accountName, storageAccountKey)
            );

            logger.LogInformation("TableClient is ready");

            Pageable<TableEntity> queryResultsFilter = tableClient.Query<TableEntity>();

            logger.LogInformation("queryResultsFilter is ready");

            foreach (TableEntity qEntity in queryResultsFilter)
            {
                WeatherForecastList.Add(new WeatherForecast
                {
                    PartitionKey = qEntity.GetString("PartitionKey"),
                    RowKey = qEntity.GetString("RowKey"),
                    Date = Convert.ToDateTime(qEntity.GetString("date_value")),

                    country_name = qEntity.GetString("country_name"),
                    confirmed = (int)qEntity.GetInt32("confirmed"),
                    deaths = (int)qEntity.GetInt32("deaths"),
                    stringency_actual = ((double)qEntity.GetDouble("stringency_actual")).ToString("0.##"),
                    stringency = ((double)qEntity.GetDouble("stringency")).ToString("0.##")

                });
            }

        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            /*
                        var rng = new Random();
                        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                        {
                            Date = DateTime.Now.AddDays(index),
                            //TemperatureC = rng.Next(-20, 55),
                            //Summary = Summaries[rng.Next(Summaries.Length)]
                        })
                        .ToArray();
            */
            return WeatherForecastList.OrderBy(d => d.Date).ThenBy(d => d.country_name); //.Take(50);
        }
    }
}
