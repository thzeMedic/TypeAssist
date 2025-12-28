using System.Windows;
using System.Windows.Input;

namespace TypeAssist
{
    
    public partial class MainWindow : Window
    {
        List<char> buffer = new List<char>();
        public MainWindow()
        {
            InitializeComponent();
            InputListenerService.Subscribe(buffer);

        }
    }
}