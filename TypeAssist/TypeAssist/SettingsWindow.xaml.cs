using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TypeAssist
{
    public partial class SettingsWindow : Window
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        string[] modes = new string[] { "Wörter", "Silben", "Buchstaben" };
        public SettingsWindow()
        {
            InitializeComponent();

            modeSelection.ItemsSource = modes;

            if (config.Sections["ModeSettings"] is null)
            {
                config.Sections.Add("ModeSettings", new ModeSettings());
            }

            var modeSettingSection = config.GetSection("ModeSettings");

            this.DataContext = modeSettingSection;
        }
    }
}
