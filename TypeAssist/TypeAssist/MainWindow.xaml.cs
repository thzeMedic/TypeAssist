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

        List<char> buffer = new List<char>();
        // Test data
        string[] testOption = { "Option1", "Option2", "Option3", "Option4" };
        private static List<string> processes = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            InputListenerService.Subscribe(buffer, processes, testblock, testPopup, testOption, RecommendationList_SelectionChanged);
        }

        private void RecommendationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
                testblock.Text = selectedOption;
                SetForegroundWindow(handleEmulation);
                CompletionService.EmulateKeys(selectedOption, buffer);
                buffer.Add(' ');
                testPopup.IsOpen = false;
            }
        }

    }
}