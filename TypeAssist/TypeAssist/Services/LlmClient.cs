using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json; 
using System.Threading.Tasks;
using TypeAssist.Models; 

namespace TypeAssist.Services
{
    public class LlmClient
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "http://localhost:11434/api/generate";

        public LlmClient()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(5); // Nicht ewig warten
        }

        // Diese Methode rufen wir beim Start auf (dein Warmup)
        public async Task WarmupAsync()
        {
            Debug.WriteLine("--- Starting Ollama Warmup ---");
            var sw = Stopwatch.StartNew();

            // Leerer Request zum Laden
            await GetNextWordAsync("Warmup");

            sw.Stop();
            Debug.WriteLine($"--- Warmup finished in {sw.ElapsedMilliseconds} ms ---");
        }

        public async Task<string> GetNextWordAsync(string context)
        {
            try
            {
                // Hier nutzen wir jetzt unsere saubere Klasse statt "new { ... }"
                var payload = new OllamaRequest
                {
                    Prompt = $"Text: {context}\nFortsetzung:",
                    Options = new OllamaOptions() // Nutzt die Defaults aus der Klasse
                };

                // Der C# Weg: PostAsJsonAsync serialisiert automatisch
                var response = await _httpClient.PostAsJsonAsync(ApiUrl, payload);

                if (!response.IsSuccessStatusCode) return null;

                // Antwort lesen und in unser Model wandeln
                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

                return result?.Response?.Trim();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LLM Error: {ex.Message}");
                return null;
            }
        }
    }
}
