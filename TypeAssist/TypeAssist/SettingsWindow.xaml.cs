using System.Configuration;
using System.Windows;

namespace TypeAssist
{
    public partial class SettingsWindow : Window
    {
        string[] modes = { "Wörter", "Silben", "Buchstaben" };
        string[] suggestionPositions = { "Maus", "rechts", "links", "oben", "unten", "mitte" };
        public SettingsWindow()
        {
            InitializeComponent();

            modeSelection.ItemsSource = modes;
            positionSelection.ItemsSource = suggestionPositions;

            var modeSettingSection = SettingService.getSettings();

            this.DataContext = modeSettingSection;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SettingService.SaveAndReload();
            this.Close();
        }
    }
}
