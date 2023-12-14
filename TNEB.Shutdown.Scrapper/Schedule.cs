using System.Text.Json.Serialization;

namespace TNEB.Shutdown.Scrapper
{
    public class Schedule
    {
        /// <summary>
        /// The date of outage.
        /// </summary>
        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// The date and time of outage start.
        /// </summary>
        [JsonPropertyName("from")]
        public DateTimeOffset From { get; set; }

        /// <summary>
        /// The date and time of outage end.
        /// </summary>
        [JsonPropertyName("to")]
        public DateTimeOffset To { get; set; }

        /// <summary>
        /// The town where outage is happening.
        /// </summary>
        [JsonPropertyName("town")]
        public string Town { get; set; }

        /// <summary>
        /// The substation where outage is happening.
        /// </summary>
        [JsonPropertyName("subStation")]
        public string SubStation { get; set; }

        /// <summary>
        /// The feeder where the type of work is happening.
        /// </summary>
        [JsonPropertyName("feeder")]
        public string Feeder { get; set; }

        /// <summary>
        /// Affected locations.
        /// </summary>
        [JsonPropertyName("location")]
        public string Location { get; set; }

        /// <summary>
        /// The type of work being done.
        /// </summary>
        [JsonPropertyName("typeOfWork")]
        public string TypeOfWork { get; set; }

        public Schedule(DateTimeOffset date, DateTimeOffset from, DateTimeOffset to, string town, string subStation, string feeder, string location, string typeOfWork)
        {
            Date = date;
            From = from;
            To = to;
            Town = town;
            SubStation = subStation;
            Feeder = feeder;
            Location = location;
            TypeOfWork = typeOfWork;
        }

        public Schedule()
        {
            Date = DateTime.Now;
            From = DateTime.Now;
            To = DateTime.Now;
            Town = string.Empty;
            SubStation = string.Empty;
            Feeder = string.Empty;
            Location = string.Empty;
            TypeOfWork = string.Empty;
        }
    }
}
