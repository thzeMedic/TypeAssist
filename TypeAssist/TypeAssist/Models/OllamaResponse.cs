using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TypeAssist.Models
{
    public class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}
