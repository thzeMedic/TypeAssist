using System.Configuration;
using System.Diagnostics;
using System.Windows.Controls.Primitives;

namespace TypeAssist
{
    class SettingService
    {
        public static Configuration Config { get; private set; } = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private static ModeSettings _settings = Config.GetSection("ModeSettings") as ModeSettings ?? new ModeSettings();
        public static event Action SettingsUpdated;

        public static ModeSettings getSettings()
        {
            return _settings;
        }

        public static void SaveAndReload()
        {
            try
            {
                Config.Save();

                ConfigurationManager.RefreshSection("ModeSettings");

                _settings = Config.GetSection("ModeSettings") as ModeSettings ?? new ModeSettings();

                SettingsUpdated?.Invoke();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error saving config file {e.Message}");
            }
        }

        public static void ApplySuggestionPosition(Popup popup)
        {
            switch (_settings.SuggestionPosition)
            {
                case "rechts":
                    popup.Placement = PlacementMode.Right;
                    break;
                case "links":
                    popup.Placement = PlacementMode.Left;
                    break;
                case "oben":
                    popup.Placement = PlacementMode.Top;
                    break;
                case "unten":
                    popup.Placement = PlacementMode.Bottom;
                    break;
                case "mitte":
                    popup.Placement = PlacementMode.Center;
                    break;
                default:
                    popup.Placement = PlacementMode.MousePoint;
                    break;
            }
        }

        public static string ApplyMode(string context)
        {
            string? prompt = null;
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
