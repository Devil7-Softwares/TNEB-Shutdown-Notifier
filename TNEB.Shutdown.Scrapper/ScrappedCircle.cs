﻿using System.Text.Json.Serialization;

namespace TNEB.Shutdown.Scrapper
{
    public interface ICircle
    {
        public string Value { get; set; }
        public string Name { get; set; }
    }

    public class ScrappedCircle : ICircle
    {
        /// <summary>
        /// Circle ID from TNEB
        /// </summary>
        /// <example>
        /// "0402"
        /// </example>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// Circle Name from TNEB
        /// </summary>
        /// <example>
        /// "CHENNAI - CENTRAL"
        /// </example>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        public ScrappedCircle(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public ScrappedCircle()
        {
            Name = string.Empty;
            Value = string.Empty;
        }
    }
}
