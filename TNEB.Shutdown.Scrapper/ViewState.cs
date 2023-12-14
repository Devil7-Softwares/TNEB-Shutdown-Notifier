using System.Text.Json.Serialization;

namespace TNEB.Shutdown.Scrapper
{
    public class ViewState
    {
        /// <summary>
        /// ID of the view state.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// State of the view state.
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        public ViewState(string id, string state)
        {
            Id = id;
            State = state;
        }

        public ViewState()
        {
            Id = string.Empty;
            State = string.Empty;
        }
    }
}
