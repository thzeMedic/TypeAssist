using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace TypeAssist
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);
        private CancellationTokenSource _currentCts;
        
        private List<char> buffer = new List<char>();
        
        private static List<string> processes = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            Debug.WriteLine("MainWindow: Initialized component");
            InputListenerService.Subscribe(buffer, processes, async (currentText) => 
                {
                    Debug.WriteLine($"MainWindow: Received input callback. CurrentText='{currentText}'");
                    await HandleNewInputAsync(currentText);
                }
            );
            Debug.WriteLine("MainWindow: Subscribed to InputListenerService");
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

                var suggestion = await App.LlmService.GetNextWordAsync(currentText, token);

                sw.Stop();
                Debug.WriteLine($"HandleNewInputAsync: LLM request finished in {sw.ElapsedMilliseconds} ms. Suggestion='{suggestion}'");

                if (!token.IsCancellationRequested && !string.IsNullOrEmpty(suggestion))
                {
                    Dispatcher.Invoke(() =>
                    {
                        var suggestions = new string[] { suggestion };
                        testPopup.Child = GenerateListBox(suggestions, RecommendationList_SelectionChanged);
                        testPopup.IsOpen = true;
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

        private static ListBox GenerateListBox(string[] data, SelectionChangedEventHandler e)
        {
            Debug.WriteLine($"GenerateListBox: Creating ListBox with {data?.Length ?? 0} items");
            var listbox = new ListBox
            {
                ItemsSource = data,
                Focusable = false,
                IsTabStop = false
            };

            listbox.SelectionChanged += e;

            return listbox;
        }

        private void RecommendationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("RecommendationList_SelectionChanged: Enter");
            var listBox = sender as ListBox;
            
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

            if (listBox != null && listBox.SelectedItem != null)
            {
                string selectedOption = listBox.SelectedItem.ToString();
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
            Debug.WriteLine("RecommendationList_SelectionChanged: Exit");
        }

    }
}