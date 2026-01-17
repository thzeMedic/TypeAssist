using System.Configuration;
using System.Windows;

namespace TypeAssist
{
    public partial class SettingsWindow : Window
    {
        readonly string[] modes = ["Wörter", "Silben", "Buchstaben"];
        readonly string[] suggestionPositions = ["Maus", "rechts", "links", "oben", "unten", "mitte"];

        public SettingsWindow()
        {
            InitializeComponent();

            modeSelection.ItemsSource = modes;
            positionSelection.ItemsSource = suggestionPositions;

            var modeSettingSection = ConfigService.GetSettings();

            this.DataContext = modeSettingSection;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ConfigService.SaveAndReload();
            this.Close();
        }
    }
}
