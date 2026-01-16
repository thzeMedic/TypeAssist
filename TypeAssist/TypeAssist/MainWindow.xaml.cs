using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


namespace TypeAssist
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);
        private CancellationTokenSource _currentCts;
        
        private List<char> buffer = new List<char>();
        
        private static List<string> processes = new List<string>();

        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public MainWindow()
        {
            InitializeComponent();
            Debug.WriteLine("MainWindow: Initialized component");

            if (config.Sections["ModeSettings"] is null)
            {
                config.Sections.Add("ModeSettings", new ModeSettings());
                config.Save();
            }

            KeyboardSubscribe();
            Debug.WriteLine("MainWindow: Subscribed to InputListenerService");

            SettingService.ApplySuggestionPosition(testPopup);

            SettingService.SettingsUpdated += () =>
            {
                SettingService.ApplySuggestionPosition(testPopup);
                Debug.WriteLine("MainWindow: Applied new suggestion position from settings");
            };
        }

        private async Task HandleNewInputAsync(string currentText)
        {
            Debug.WriteLine("HandleNewInputAsync: Enter");
            _currentCts?.Cancel();
            _currentCts = new CancellationTokenSource();
            var token = _currentCts.Token;

            try
            {
                Dispatcher.Invoke(() => testblock.Text = currentText);
                Debug.WriteLine($"HandleNewInputAsync: Updated UI textblock: '{currentText}'");

                var sw = Stopwatch.StartNew();
                Debug.WriteLine("HandleNewInputAsync: Requesting suggestion from LlmService...");



                var rawSuggestion = await App.LlmService.GetNextWordAsync(currentText, token);

                sw.Stop();
                Debug.WriteLine($"HandleNewInputAsync: LLM request finished in {sw.ElapsedMilliseconds} ms. Suggestion='{rawSuggestion}'");


                if (!token.IsCancellationRequested && !string.IsNullOrEmpty(rawSuggestion))
                {
                    var suggestions = rawSuggestion.Split('|', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => s.Trim())
                                           .ToArray();

                    Dispatcher.Invoke(() =>
                    {
                        ListBox listBox = GenerateListBox(suggestions);
                        testPopup.Child = listBox;
                        testPopup.IsOpen = true;

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
                        Debug.WriteLine("HandleNewInputAsync: Popup opened with suggestion");

                    });
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

        private void KeyboardSubscribe()
        {
            InputListenerService.Subscribe(buffer, processes, async (currentText) =>
            {
                Debug.WriteLine($"MainWindow: Received input callback. CurrentText='{currentText}'");
                await HandleNewInputAsync(currentText);
            });
            InputListenerService.SubscribeSetting();
        }

        private ListBox GenerateListBox(string[] data)
        {
            Debug.WriteLine($"GenerateListBox: Creating ListBox with {data?.Length ?? 0} items");
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

        private void ConfirmSelection(string selectedOption) {
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

            Debug.WriteLine($"Recommendation selected: {selectedOption}");
            testblock.Text = selectedOption;
            Debug.WriteLine($"RecommendationList_SelectionChanged: Setting foreground to process handle {handleEmulation}");
            SetForegroundWindow(handleEmulation);
            Debug.WriteLine("RecommendationList_SelectionChanged: Calling CompletionService.EmulateKeys");
            CompletionService.EmulateKeys(selectedOption, buffer);
            buffer.Add(' ');
            testPopup.IsOpen = false;
            Debug.WriteLine("RecommendationList_SelectionChanged: Emulation complete and popup closed");
        }

        private static void TypeAssistForeground()
        {
            Process[] typeAssistProcess = Process.GetProcessesByName("TypeAssist");
            IntPtr typeAssistHandle = typeAssistProcess[0].MainWindowHandle;

            SetForegroundWindow(typeAssistHandle);
        }
    }
}