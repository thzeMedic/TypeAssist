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
            _httpClient.Timeout = TimeSpan.FromSeconds(5); 
        }

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
                var prompt = SettingService.ApplyMode(context);

                var payload = new OllamaRequest
                {
                    Model = "qwen2.5:0.5b",
                    Prompt = prompt,
                    Options = new OllamaOptions
                    {
                        NumPredict = 10,      
                        Temperature = 0.2,   
                        TopK = 40,
                        RepeatPenalty=1.1,
                        Stop = new[] { " ", "\n", ".", ",", "!", "?" }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(ApiUrl, payload, cancellationToken);

                if (!response.IsSuccessStatusCode) return null;

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
