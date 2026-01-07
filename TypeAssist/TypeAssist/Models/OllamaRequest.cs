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
        // Empfehlung: mindestens 3B für bessere deutsche Grammatik
        public string Model { get; set; } = "qwen2.5:0.5b";

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = "";

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        // WICHTIG: Damit das Modell im RAM bleibt (-1)
        [JsonPropertyName("keep_alive")]
        public int KeepAlive { get; set; } = -1;

        [JsonPropertyName("options")]
        public OllamaOptions Options { get; set; } = new OllamaOptions();
    }

    public class OllamaOptions
    {
        // 🔥 Der Gamechanger für Autocomplete: nur EIN Token erzeugen
        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; } = 1;

        // Wenig Kreativität → stabilere Vorhersagen
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.1;

        // Optional, aber hilfreich gegen „Halluzinationen“
        [JsonPropertyName("top_p")]
        public double TopP { get; set; } = 0.9;

        [JsonPropertyName("top_k")]
        public int TopK { get; set; } = 40;

        [JsonPropertyName("repeat_penalty")]
        public double RepeatPenalty { get; set; } = 1.1;

        // Stoppt die Ausgabe nach dem ersten Wort
        [JsonPropertyName("stop")]
        public string[] Stop { get; set; } = new[] { " ", "\n", ".", ",", "!", "?" };
    }
}
