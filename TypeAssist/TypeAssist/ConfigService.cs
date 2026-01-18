using OpenAI.Chat;
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



        public static (string, string) ApplyMode(string context)
        {
            string? prompt;
            string? systemPrompt;
            string baseInstructions = "Your task: complete the user's input exactly 5 suggestions, where each suggestion is separated by this delimiter: '|' " +
                                      "Rules: No explanations. No punctuation. No polite conversation. Only the suggestions in the specified format.";

            switch (_settings.Mode)
            {
                case "Silben":

                    systemPrompt = "You are a smart text completer. " +
                                    baseInstructions +
                                    " Mode: SYLLABLES/SEGMENTS. " +
                                    "Format: input+extension1|input+extension2|... " +
                                    "Instruction: Complete the current word with the next logical syllable or character sequence. " +
                                    "Rules: 1. If the word is almost complete, finish it. 2. If it is the start of a long word, add one syllable. " +
                                    "Example 1: Input 'Com' -> Suggestion 'Compu'. " +
                                    "Example 2: Input 'Sno' -> Suggestion 'Snow' (finishes the word).";

                    prompt = $"Input: {context}";
                    break;

                case "Buchstaben":
                    systemPrompt = "You are a precise character predictor. " +
                                   baseInstructions +
                                   " Mode: CHARACTERS. " +
                                   "Format: input+nextChar1|input+nextChar2|... " +
                                   "Instruction: Extend the user's input by exactly ONE character. " +
                                   "Always use lowercase for the appended character." +
                                   "Example: Input 'Hous' -> Suggestion 'House' (if 'e' is likely) or 'Housi' (if 'i' is likely).";

                    prompt = $"Input: {context}";
                    break;

                default: 
                    systemPrompt = "You are a precise autocomplete engine. " +
                                   baseInstructions +
                                   " Mode: FULL WORDS. " +
                                   "Format: word1|word2|word3|word4|word5 " +
                                   "Instruction: Complete the user's input to a full meaningful word.";

                    prompt = $"Input: {context}";
                    break;
            }
            return (systemPrompt, prompt);
        }
    }
}
