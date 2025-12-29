using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace TypeAssist
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        protected override void OnStartup(StartupEventArgs e)
        {
            // Start the stopwatch here so the first measurement includes the startup time.
            var startupSw = Stopwatch.StartNew();

            // Fire-and-forget the async work so the UI startup isn't blocked.
            _ = SendTwoOllamaRequestsAsync(startupSw);

            base.OnStartup(e);
        }

        private static async Task SendTwoOllamaRequestsAsync(Stopwatch startupSw)
        {

            String initialPromptInstructionsGerman = "Du bist eine Autovervollständigung. Ergänze den Text mit genau EINEM Wort.\r\nAchte strikt auf korrekte Groß- und Kleinschreibung im Deutschen.\r\nAntworte nur mit dem Wort, ohne Satzzeichen oder Erklärungen.";
            String promptGerman = "Setze diesen Text fort:";
            String bufferTest = "Heute ist ein schöner";
            try
            {
                var requestUri = "http://localhost:11434/api/generate";

                var payload = new
                {
                    model = "qwen2.5:0.5b",
                    system = "Du bist ein Autocomplete-System. Antworte NUR mit dem nächsten Wort. Keine Sätze, keine Punkte.",
                    // WICHTIG: Kein Leerzeichen am Ende von "Fortsetzung:"
                    prompt = $"Text: {bufferTest}\nFortsetzung:",
                    stream = false,
                    options = new
                    {
                        num_predict = 10,
                        temperature = 0.0, // 0.0 ist bei 0.5b Modellen oft stabiler für Logik
                                           // Wir nehmen das Leerzeichen aus der Stop-Liste, 
                                           // damit es das erste Leerzeichen machen darf, aber beim zweiten stoppt.
                        stop = new[] { "\n", ". ", "!", "Text:" }
                    }
                };

                // First request: measure from OnStartup -> first response
                var response1 = await _httpClient.PostAsJsonAsync(requestUri, payload);
                response1.EnsureSuccessStatusCode();
                var text1 = await response1.Content.ReadAsStringAsync();

                startupSw.Stop();
                Debug.WriteLine($"First call (startup + request) elapsed: {startupSw.ElapsedMilliseconds} ms");
                Debug.WriteLine("Ollama response 1 (raw): " + text1);

                // Second request: measure request-only time
                var reqSw = Stopwatch.StartNew();
                var response2 = await _httpClient.PostAsJsonAsync(requestUri, payload);
                response2.EnsureSuccessStatusCode();
                var text2 = await response2.Content.ReadAsStringAsync();
                reqSw.Stop();

                Debug.WriteLine($"Second call (only request) elapsed: {reqSw.ElapsedMilliseconds} ms");
                Debug.WriteLine("Ollama response 2 (raw): " + text2);
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine("Ollama request failed: " + httpEx.Message);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Unexpected error during Ollama request: " + ex.Message);
            }
        }
    }

}
