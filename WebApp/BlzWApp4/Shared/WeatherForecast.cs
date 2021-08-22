using System;
using System.Collections.Generic;
using System.Text;

namespace BlzWApp4.Shared
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public string date_value { get; set; }
        public DateTime dtdate { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public int confirmed { get; set; }
        public int deaths { get; set; }
        public string stringency_actual { get; set; }
        public string stringency { get; set; }
    }
}
