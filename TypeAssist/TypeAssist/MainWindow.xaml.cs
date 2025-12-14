using DeftSharp.Windows.Input.Keyboard;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace TypeAssist
{
    
    public partial class MainWindow : Window
    {

        private static KeyboardListener _keyboardListener = new KeyboardListener();

        public MainWindow()
        {
            InitializeComponent();
            InputListenerService.Subscribe(testblock);

        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Key pressed: {e.Key}");
            testblock.Text = $"Key pressed: {e.Key}";
        }
    }
}