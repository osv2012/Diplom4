using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzFunc5
{
    public class DayDataTableEntity : ITableEntity
    {
        public DayDataTableEntity() { }
        public DayDataTableEntity(string partitionKey, string rowKey) {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string date_value { get; set; }
        public DateTime dtdate { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public int confirmed { get; set; }
        public int deaths { get; set; }
        public float stringency_actual { get; set; }
        public float stringency { get; set; }
    }

    public class DayDataRepository
    {
        public TableClient DayDataTableCient = null;

        public DayDataRepository()
        {
        }
        
        public DayDataRepository(ILogger log)
        {
            string mi_clientId = "";
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = mi_clientId });

            log.LogInformation("DefaultAzureCredential is OK");

            string keyvault_name = "";
            string keyVaultUrl = "https://kvblobtbldiplom1.vault.azure.net/";
            string secret_name = "";

            var kvclient = new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: credential);
            log.LogInformation("SecretClient is OK");

            var secret = (kvclient.GetSecret(secret_name)).Value.Value;

            string storageUri = "https://staccblobtbldiplom2.table.core.windows.net/tableabc1";
            string tableName = "";
            string accountName = "";
            string storageAccountKey = secret; 
            DayDataTableCient = new TableClient(new Uri(storageUri),
                tableName,
                new TableSharedKeyCredential(accountName, storageAccountKey)
            );
            log.LogInformation("TableClient is OK");
        }

        internal void AllDelete(ILogger log)
        {
            log.LogInformation("AllDelete started");
            Pageable<TableEntity> queryResultsFilter = DayDataTableCient.Query<TableEntity>();
            log.LogInformation("queryResultsFilter is OK");

            int nCount = 0;
            foreach (TableEntity qEntity in queryResultsFilter)
            {
                //Console.WriteLine($"{qEntity.GetString("PartitionKey")}: {qEntity.GetString("RowKey")}: ");

                string sPartitionKey = qEntity.GetString("PartitionKey");
                string sRowKey = qEntity.GetString("RowKey");

                DayDataTableCient.DeleteEntity(sPartitionKey, sRowKey);
                nCount++;
            }

            Console.WriteLine("AllDeleted! :{0}", nCount.ToString());
            log.LogInformation("AllDelete finished");
        }

        internal void AllLoad(ILogger log)
        {
            log.LogInformation("AllLoad started");
            var countries = new List<Country>(
                new[]
                {
                        new Country("ITA", "Italy"),
                        new Country("ISR", "Israel"),
                        new Country("FRA", "France"),
                        new Country("CHE", "Switzerland"),
                        new Country("AUT", "Austria"),
                        new Country("CHN", "China"),
                        new Country("GRC", "Greece"),
                        new Country("IND", "India"),
                        new Country("USA", "United States"),
                        new Country("RUS", "Russian Federation")
                }
            );

            DateTime Now = DateTime.Now;
            int nDays = Now.DayOfYear;
            DateTime startDate = new DateTime(Now.Year, 1, 1);
            string serviceBaseUrl = @"https://covidtrackerapi.bsg.ox.ac.uk/api/v2/stringency/actions/";

            Parallel.ForEach(countries, country =>
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    for (int i = 0; i < 10; i++) // nDays 150
                    {
                        string sUrl = serviceBaseUrl + country.CountryCode + @"/" + startDate.AddDays(i).ToString("yyyy-MM-dd") + @"/";

                        try
                        {
                            var response = client.GetAsync(sUrl).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                string responseBody = response.Content.ReadAsStringAsync().Result;

                                if (!string.IsNullOrEmpty(responseBody))
                                {

                                    var dData = JsonSerializer.Deserialize<StringencyData>(responseBody);

                                    if (null != dData && null != dData.stringencyData && !string.IsNullOrEmpty(dData.stringencyData.date_value))
                                    {

                                        var entity = new TableEntity(country.CountryCode, i.ToString())
                                        {
                                            { "date_value", dData.stringencyData.date_value },
                                            { "dtdate", startDate.AddDays(i).ToString("yyyy-MM-dd") },
                                            { "country_name", country.CountryName },
                                            { "confirmed", dData.stringencyData.confirmed },
                                            { "deaths", dData.stringencyData.deaths },
                                            { "stringency_actual", (double)(dData.stringencyData.stringency_actual) },
                                            { "stringency", (double)(dData.stringencyData.stringency) }
                                        };

                                        if (!string.IsNullOrEmpty((string)entity["date_value"]))
                                        {
                                            DayDataTableCient.AddEntity(entity);
                                        }
                                        else
                                        {
                                            Console.WriteLine($"{entity.GetString("PartitionKey")}: {entity.GetString("RowKey")}: date_value is empty");
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            break;
                        }

                    }
                }


            });
            Console.WriteLine("AllLoaded!");
            log.LogInformation("AllLoad finished");
        }
    }

    struct Country
    {
        private readonly string countryCode;
        private readonly string countryName;

        public Country(string code, string name)
        {
            this.countryCode = code;
            this.countryName = name;
        }

        public string CountryCode { get { return countryCode; } }
        public string CountryName { get { return countryName; } }

    }
    public class DayData
    {
        public string date_value { get; set; }
        public string country_code { get; set; }
        public int confirmed { get; set; }
        public int deaths { get; set; }
        public float stringency_actual { get; set; }
        public float stringency { get; set; }
    }

    public class CountryDayData
    {
        public string date_value { get; set; }
        public DateTime dtdate { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public int confirmed { get; set; }
        public int deaths { get; set; }
        public float stringency_actual { get; set; }
        public float stringency { get; set; }
    }

    public class StringencyData
    {
        public DayData stringencyData { get; set; }
    }
}
