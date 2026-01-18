using OpenAI.Chat;
using OpenAI.Conversations;
using System;
using System.ClientModel;
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
        private readonly ChatClient _client;
        public LlmClient()
        {
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
            _client = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);
            _httpClient = new HttpClient();
            // Keep a short timeout so we don't block too long waiting for a model when the user keeps typing
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        public async Task WarmupAsync()
        {
            try
            {
                Debug.WriteLine("Warmup: starting remote warmup calls");
                await GetNextWordAsyncRemote("Warmup", CancellationToken.None);

                Debug.WriteLine("Warmup: second remote warmup call");
                await GetNextWordAsyncRemote("Warmup", CancellationToken.None);
            }
            catch { }
        }

        public async Task<string?> GetNextWordAsyncRemote(string context, CancellationToken cancellationToken)
        {
            try
            {
                var swRemote = Stopwatch.StartNew();
                
                var messages = new ChatMessage[]
                {
                    new SystemChatMessage(
                        "You are a precise autocomplete engine. " +
                        "Your task: complete the user's input exactly 5 suggestions, where each suggestion is one word and separated by this delimiter: '|' " +
                        "Format (Suggestion): word1|word2|word3|word4|word5 " +
                        "Rules: No explanations. No punctuation. No polite conversation. Only the words in the specified format."),

                    new UserChatMessage($"Input: {context}")
                };

                var options = new ChatCompletionOptions
                {
                    Temperature = 0.1f,
                    MaxOutputTokenCount = 40,   
                    TopP = 1.0f,
                    StopSequences = { "\n", "Input:" }
                };

                ChatCompletion completion = await _client.CompleteChatAsync(messages, options, cancellationToken);

                string result = completion.Content[0].Text.Trim();

                swRemote.Stop();
                Debug.WriteLine($"OpenAI: {result} (elapsed {swRemote.ElapsedMilliseconds} ms)");

                return result;
            }
            catch (ClientResultException ex) when (ex.Status == 401)
            {
                Debug.WriteLine("OpenAI API Key expired/wrong/missing!");
                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OpenAI Error: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetNextWordAsyncLocal(string context, CancellationToken cancellationToken)
        {
            try
            {
                var prompt = $@"
                    You are a very smart autocomplete program. 
                    Rules:
                    - Provide 3 suggestions that complete the last word.
                    - Each suggestion must be ONE WORD only.
                    - Format: word1|word2|word3
                    - No explanations, no punctuation, no extra text.

                    Examples:
                    - Input: 'I like appl' -> Output: 'apple|apples|applied'
                    - Input: 'The weather is be' -> Output: 'beautiful|better|best'
                    - Input: 'Wait for the applic' -> Output: 'application|applicant|applicable'
                    
                    Task: Complete the last unfinished word of the UserInput.
                    UserInput: '{context}' -> Output:";

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

                Debug.WriteLine($"Local LLM: posting to {ApiUrl}");
                Debug.WriteLine($"Local LLM: payload model={payload.Model}, num_predict={payload.Options.NumPredict}, temp={payload.Options.Temperature}");

                var sw = Stopwatch.StartNew();
                var response = await _httpClient.PostAsJsonAsync(ApiUrl, payload, cancellationToken);
                sw.Stop();

                Debug.WriteLine($"Local LLM: response status {response.StatusCode} (elapsed {sw.ElapsedMilliseconds} ms)");

                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                Debug.WriteLine($"Local LLM raw response: {raw}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Local LLM: non-success status code returned");
                    return null;
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<OllamaResponse>(raw);

                if (result == null)
                {
                    Debug.WriteLine("Local LLM: failed to deserialize response into OllamaResponse");
                    return null;
                }

                Debug.WriteLine($"Local LLM: parsed response='{result.Response}' done={result.Done}");

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
