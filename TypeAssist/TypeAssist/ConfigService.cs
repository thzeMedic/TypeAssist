using System.Configuration;
using System.Diagnostics;
using System.Windows.Controls.Primitives;

namespace TypeAssist
{
    class ConfigService
    {
        public static Configuration Config { get; private set; } = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private static ConfigProperties _settings = Config.GetSection("ModeSettings") as ConfigProperties ?? new ConfigProperties();
        public static event Action? SettingsUpdated;

        public static ConfigProperties GetSettings()
        {
            return _settings;
        }

        /// <summary>
        /// Ensures that the "ModeSettings" configuration section exists, creating and saving it if necessary.
        /// </summary>
        /// <remarks>Call this method before accessing the "ModeSettings" section to guarantee it is
        /// present in the configuration. If the section does not exist, it will be added and the configuration will be
        /// saved.</remarks>
        public static void CheckConfigSection()
        {
            if (Config.Sections["ModeSettings"] is null)
            {
                Config.Sections.Add("ModeSettings", new ConfigProperties());
                Config.Save();
            }
        }

        public static void SaveAndReload()
        {
            try
            {
                Config.Save();

                ConfigurationManager.RefreshSection("ModeSettings");

                _settings = Config.GetSection("ModeSettings") as ConfigProperties ?? new ConfigProperties();

                SettingsUpdated?.Invoke();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error saving config file {e.Message}");
            }
        }

        public static string ApplyMode(string context)
        {
            string? prompt;
            switch (_settings.Mode)
            {
                case "Silben":
                    prompt = "return always this single word: Alex";
                    break;
                case "Buchstaben":
                    prompt = "return always this single word: Amer";
                    break;
                default:
                    prompt = $@"
                    You are a very smart autocomplete program. 
                    Rules:
                    - Provide 3 suggestions that complete the last word.
                    - Each suggestion must be ONE WORD only.
                    - Format: word1|word2|word3
                    - No explanations, no punctuation, no extra text.

                    Examples:
                    - Input: 'I like appl' -> Output: 'apple|apples|applied'
                    - Input: 'The weather is be' -> Output: 'beautiful|better|best'
                    - Input: 'Wait for the applic' -> Output: 'application|applicant|applicable'
                    
                    Task: Complete the last unfinished word of the UserInput.
                    UserInput: '{context}' -> Output:";
                    break;
            }
            return prompt;
        }
    }
}
