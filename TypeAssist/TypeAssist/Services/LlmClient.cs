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
            await GetNextWordAsync("Warmup", new CancellationToken());

            sw.Stop();
            Debug.WriteLine($"--- Warmup finished in {sw.ElapsedMilliseconds} ms ---");
        }

        public async Task<string> GetNextWordAsync(string context, CancellationToken cancellationToken)
        {
            try
            {
                var strictPrompt = $@"<|im_start|>system
                    Du bist ein Code-Editor und Autocomplete-Tool. 
                    Deine Aufgabe ist es, den Text des Users zu vervollständigen.
                    Antworte NUR mit dem nächsten Wort. 
                    Keine Sätze. Keine Erklärungen. Keine Anführungszeichen.
                    <|im_end|>
                    <|im_start|>user
                    {context}<|im_end|>
                    <|im_start|>assistant
                    ";

                var payload = new OllamaRequest
                {
                    // Wir senden den ganzen Prompt als "prompt" Feld, nicht "system" extra
                    Prompt = strictPrompt,

                    // Wichtig: Verhindert, dass er "Hier ist das Wort:" schreibt
                    Options = new OllamaOptions
                    {
                        NumPredict = 10, // Nur max 5 Token generieren
                        Stop = new[] { "\n", ".", "<|im_end|>", "!", "?" }, // Bei Leerzeichen sofort stoppen!
                        Temperature = 0.1 // Weniger Kreativität, mehr Präzision
                    },
                    // ... keep_alive ...
                };

                // Der C# Weg: PostAsJsonAsync serialisiert automatisch
                var response = await _httpClient.PostAsJsonAsync(ApiUrl, payload, cancellationToken);

                if (!response.IsSuccessStatusCode) return null;

                // Antwort lesen und in unser Model wandeln
                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);

                return result?.Response?.Trim();
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Request cancelled because user kept typing.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LLM Error: {ex.Message}");
                return null;
            }
        }
    }
}
