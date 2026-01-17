using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;


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
            _currentCts?.Cancel();
            _currentCts = new CancellationTokenSource();
            var token = _currentCts.Token;

            try
            {
                var sw = Stopwatch.StartNew();

                var rawSuggestion = await App.LlmService.GetNextWordAsync(currentText, token);

                sw.Stop();

                if (!token.IsCancellationRequested && !string.IsNullOrEmpty(rawSuggestion))
                {
                    var suggestions = rawSuggestion.Split('|', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => s.Trim())
                                           .ToArray();

                    Dispatcher.Invoke(() =>
                    {
                        ListBox listBox = GenerateListBox(suggestions);
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
        private void ConfirmSelection(string selectedOption)
        {
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
            CompletionService.EmulateKeys(selectedOption, buffer);
            buffer.Add(' ');
            recommendations.IsOpen = false;
        }

        private static void TypeAssistForeground()
        {
            Process[] typeAssistProcess = Process.GetProcessesByName("TypeAssist");
            IntPtr typeAssistHandle = typeAssistProcess[0].MainWindowHandle;

            SetForegroundWindow(typeAssistHandle);
        }
    }
}