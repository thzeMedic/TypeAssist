using OpenAI.Containers;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using TypeAssist.Services;
using static System.Net.Mime.MediaTypeNames;


namespace TypeAssist
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);
        private CancellationTokenSource? _currentCts;

        private List<char> buffer = new List<char>();

        private static List<string> processes = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            ConfigService.CheckConfigSection();

            KeyboardSubscribe();

            UpdatePopupPosition();

            ConfigService.SettingsUpdated += UpdatePopupPosition;
        }

        private async Task HandleNewInputAsync(string currentText)
        {

            if (string.IsNullOrWhiteSpace(currentText))
            {
                Debug.WriteLine("[HandleNewInputAsync] Input empty/whitespace. Skipping LLM request.");
                Dispatcher.Invoke(() =>
                {
                    recommendations.IsOpen = false;
                });
                return;
            }
                Debug.WriteLine($"buffer: {currentText}");
            _currentCts?.Cancel();
            _currentCts = new CancellationTokenSource();
            var token = _currentCts.Token;
            bool useTravelDistance = ConfigService.GetSettings().UseTravelDistance;
            //bool limitContext = ConfigService.GetSettings().UseLimitedContext;
            // int contextSize = ConfigService.GetSettings().ContextLength;
            try
            {
                var sw = Stopwatch.StartNew();

                string contextToSend = currentText;

                /*if (contextToSend.Length > contextSize && limitContext)
                {
                    contextToSend = contextToSend.Substring(contextToSend.Length - 200);
                }
                */

                if (contextToSend.Length > 200)
                {
                    contextToSend = contextToSend.Substring(contextToSend.Length - 200);
                }

                var rawSuggestion = await App.LlmService.GetNextWordAsyncRemote(contextToSend, token);

                sw.Stop();

                if (!token.IsCancellationRequested && !string.IsNullOrEmpty(rawSuggestion))
                {
                        
                    List<string> suggestions = rawSuggestion.Split('|', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(s => s.Trim())
                                                   .ToList();

                    string lastWord = currentText.Split(' ').LastOrDefault() ?? "";
                    string prefix = "";

                    if (currentText.Length >= lastWord.Length)
                    {
                        prefix = currentText.Substring(0, currentText.Length - lastWord.Length);
                    }

                    // Only strip if we actually have a prefix (prevent stripping empty strings)
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        for (int i = 0; i < suggestions.Count; i++)
                        {
                            // Check if the suggestion starts with the prefix (case-insensitive)
                            if (suggestions[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            {
                                // Replace the suggestion with just the completion part
                                // Example: "table it" -> "it"
                                suggestions[i] = suggestions[i].Substring(prefix.Length);
                            }
                        }
                    }

                    List<string> filteredSuggestions; 

                    if (useTravelDistance)
                    {
                        // 2. Status vor dem Filtern
                        Debug.WriteLine("--- Travel Distance Active ---");
                        Debug.WriteLine($"[TypeAssist] Current Input: '{currentText}' (Length: {currentText.Length})");
                        Debug.WriteLine($"[TypeAssist] Candidates ({suggestions.Count}): {String.Join(", ", suggestions)}");
                        Debug.WriteLine($"last word was: {lastWord}");

                        // 3. Filter aufrufen
                        filteredSuggestions = Traveldistance.GetTravelDistanceAdjustedSuggestions(lastWord, suggestions);

                        Debug.WriteLine($"[TypeAssist] FINAL RESULT: {String.Join(", ", filteredSuggestions)}");
                        Debug.WriteLine("--------------------------------------------------");
                    } else
                    {
                        Debug.WriteLine("--- Travel Distance Deactivated ---");
                        Debug.WriteLine($"[TypeAssist] Current Input: '{currentText}'");

                        // Optional: Trim punctuation from the end of the last word to allow predictions like "snow." -> "snowy"
                        // string cleanLastWord = lastWord.TrimEnd('.', ',', '!', '?'); 
                        // For now, we stick to the standard logic:

                        Debug.WriteLine($"[TypeAssist] Last Word: '{lastWord}'");

                        filteredSuggestions = suggestions
                            // FIX: Check against 'lastWord', NOT 'currentText'
                            .Where(s => s.StartsWith(lastWord, StringComparison.OrdinalIgnoreCase))
                            // Ensure the suggestion is actually longer than what we have
                            .Where(s => s.Length > lastWord.Length)
                            .ToList();

                        Debug.WriteLine($"[TypeAssist] Candidates ({suggestions.Count}): {String.Join(", ", suggestions)}");
                        Debug.WriteLine($"[TypeAssist] FINAL RESULT: {String.Join(", ", filteredSuggestions)}");
                    }


                    if (filteredSuggestions.Count > 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ListBox listBox = GenerateListBox(filteredSuggestions.ToArray());
                            recommendations.Child = listBox;
                            recommendations.IsOpen = true;

                            TypeAssistForeground();

                            listBox.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                            {
                                if (listBox.Items.Count > 0)
                                {
                                    listBox.SelectedIndex = 0;

                                    var item = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(0);
                                    item?.Focus();
                                }
                                else
                                {
                                    listBox.Focus();
                                }
                            }));
                        });
                    }

                    
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("HandleNewInputAsync: Canceled because user was faster.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleNewInputAsync: Unexpected exception: {ex.Message}");
            }
            finally
            {
                Debug.WriteLine("HandleNewInputAsync: Exit");
            }
        }

        /// <summary>
        /// Subscribes to keyboard input events and settings hotkey using the input listener service.
        /// </summary>
        /// <remarks>This method enables listening for keyboard input and hotkey triggers by registering
        /// appropriate event handlers with the input listener service. It should be called to initialize input
        /// monitoring for the current buffer and process context.</remarks>
        private void KeyboardSubscribe()
        {
            InputListenerService.Subscribe(buffer, processes, async (currentText) =>
            {
                await HandleNewInputAsync(currentText);
            });
            InputListenerService.SubscribeSettingsHotkey();
        }

        /// <summary>
        /// Updates the position and placement of the popup control based on the current suggestion position settings.
        /// </summary>
        /// <remarks>This method retrieves the user's suggestion position preference from the settings and
        /// adjusts the popup's placement accordingly. It should be called whenever the suggestion position setting
        /// changes to ensure the popup appears in the correct location.</remarks>
        private void UpdatePopupPosition()
        {
            var settings = ConfigService.GetSettings();

            switch (settings.SuggestionPosition)
            {
                case "rechts":
                    recommendations.PlacementTarget = AnchorRight;
                    recommendations.Placement = PlacementMode.Left;
                    break;
                case "links":
                    recommendations.PlacementTarget = AnchorLeft;
                    recommendations.Placement = PlacementMode.Right;
                    break;
                case "oben":
                    recommendations.PlacementTarget = AnchorTop;
                    recommendations.Placement = PlacementMode.Bottom;
                    break;
                case "unten":
                    recommendations.PlacementTarget = AnchorBottom;
                    recommendations.Placement = PlacementMode.Top;
                    break;
                case "mitte":
                    recommendations.PlacementTarget = null;
                    recommendations.Placement = PlacementMode.Center;
                    break;
                default:
                    recommendations.PlacementTarget = null;
                    recommendations.Placement = PlacementMode.MousePoint;
                    break;
            }
        }

        /// <summary>
        /// Creates a new ListBox control populated with the specified data and configured to confirm selection on Tab
        /// or mouse click.
        /// </summary>
        /// <remarks>The returned ListBox raises selection confirmation when the user presses the Tab key
        /// or clicks an item with the mouse. The caller is responsible for adding the ListBox to the visual tree and
        /// managing its lifecycle.</remarks>
        /// <param name="data">An array of strings to display as items in the ListBox. Can be null or empty to create an empty ListBox.</param>
        /// <returns>A ListBox control with its ItemsSource set to the provided data and event handlers for confirming selection.</returns>
        private ListBox GenerateListBox(string[] data)
        {
            var listbox = new ListBox
            {
                ItemsSource = data
            };

            listbox.PreviewKeyDown += (sender, e) =>
            {
                var lb = sender as ListBox;

                if (e.Key == Key.Tab)
                {
                    if (lb.SelectedItem != null)
                    {
                        ConfirmSelection(lb.SelectedItem.ToString());
                        e.Handled = true;
                    }
                }
            };

            listbox.PreviewMouseLeftButtonUp += (sender, e) =>
            {
                var lb = sender as ListBox;
                if (lb.SelectedItem != null)
                {
                    ConfirmSelection(lb.SelectedItem.ToString());
                }
            };

            return listbox;
        }

        /// <summary>
        /// Confirms the user's selection by updating the UI, bringing the target process window to the foreground, and
        /// emulating key input based on the selected option.
        /// </summary>
        /// <remarks>This method sets the selected option in the UI, brings the relevant process window to
        /// the foreground, and sends the selected text as emulated keystrokes. The method also updates the input buffer
        /// and closes the associated popup. Ensure that the target process is running and accessible before calling
        /// this method.</remarks>
        /// <param name="selectedOption">The option selected by the user to be confirmed and sent as emulated input. Cannot be null or empty.</param>
        private async void ConfirmSelection(string selectedOption)
        {
            try
            {
                // 1. PAUSE LISTENING to prevent double-entry of emulated keys
                InputListenerService.IgnoreInput = true;

                IntPtr handleEmulation;

                if (processes.Count == 1)
                {
                    Process[] process = Process.GetProcessesByName(processes[0]);
                    handleEmulation = process[0].MainWindowHandle;
                }
                else
                {
                    Process[] process = Process.GetProcessesByName(processes[processes.Count - 1]);
                    handleEmulation = process[0].MainWindowHandle;
                }

                SetForegroundWindow(handleEmulation);

                // 2. Emulate the keystrokes (types the difference between buffer and suggestion)
                CompletionService.EmulateKeys(selectedOption, buffer);

                // 3. Manually update the buffer to the correct state
                UpdateBufferWithSelection(selectedOption);

                recommendations.IsOpen = false;
            }
            finally
            {
                // 4. Wait a short moment for OS hooks to clear, then UNPAUSE
                await Task.Delay(50);
                InputListenerService.IgnoreInput = false;
            }
        }


        private void UpdateBufferWithSelection(string selectedOption)
        {
            // Remove the partial word currently at the end of the buffer
            // Example Buffer: ['H', 'e', 'l']
            // Selected: "Hello"

            // Convert buffer to string to find the last word boundary
            string currentText = new string(buffer.ToArray());

            // Simple logic: remove characters until we hit a space or empty
            while (buffer.Count > 0 && buffer.Last() != ' ')
            {
                buffer.RemoveAt(buffer.Count - 1);
            }

            // Append the full selected option
            buffer.AddRange(selectedOption.ToCharArray());

            // Check Mode for Spacing
            var settings = ConfigService.GetSettings();

            // Only add space in default (Word) mode. 
            // Silben (Syllables) and Buchstaben (Characters) continue the word immediately.
            if (settings.Mode != "Silben" && settings.Mode != "Buchstaben")
            {
                buffer.Add(' ');
            }

            // Debug output to verify sync
            Debug.WriteLine($"[Sync] Buffer updated manually to: '{new string(buffer.ToArray())}'");
        }

        private static void TypeAssistForeground()
        {
            Process[] typeAssistProcess = Process.GetProcessesByName("TypeAssist");
            IntPtr typeAssistHandle = typeAssistProcess[0].MainWindowHandle;

            SetForegroundWindow(typeAssistHandle);
        }
    }
}