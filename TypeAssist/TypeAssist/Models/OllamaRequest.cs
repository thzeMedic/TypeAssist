using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TypeAssist.Models
{
    // Diese Klasse definiert, was wir an Ollama senden
    public class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "qwen2.5:0.5b";

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = "";

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        // WICHTIG: Damit das Modell im RAM bleibt (-1)
        [JsonPropertyName("keep_alive")]
        public int KeepAlive { get; set; } = -1;

        [JsonPropertyName("options")]
        public OllamaOptions Options { get; set; }
    }

    public class OllamaOptions
    {
        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; } = 5;

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.0;

        [JsonPropertyName("stop")]
        public string[] Stop { get; set; } = new[] { "\n", ".", "!", "Text:" };
    }
}
