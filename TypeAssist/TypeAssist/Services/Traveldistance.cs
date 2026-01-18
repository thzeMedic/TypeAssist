using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TypeAssist.Services
{
    internal class Traveldistance
    {
        public const int TRAVELDISTANCE_THRESHOLD = 3;

        private static Dictionary<string, (int x, int y)> keyCoordinatesQWERTZ = new Dictionary<string, (int x, int y)>
        {
            {"Q", (0, 0)}, {"W", (1, 0)}, {"E", (2, 0)}, {"R", (3, 0)}, {"T", (4, 0)},
            {"Z", (5, 0)}, {"U", (6, 0)}, {"I", (7, 0)}, {"O", (8, 0)}, {"P", (9, 0)},
            {"Ü", (10, 0)}, {"ß", (11, -1)}, // ß etwas versetzt
            
            {"A", (0, 1)}, {"S", (1, 1)}, {"D", (2, 1)}, {"F", (3, 1)}, {"G", (4, 1)},
            {"H", (5, 1)}, {"J", (6, 1)}, {"K", (7, 1)}, {"L", (8, 1)}, {"Ö", (9, 1)}, {"Ä", (10, 1)},
            
            {"Y", (1, 2)}, {"X", (2, 2)}, {"C", (3, 2)}, {"V", (4, 2)}, {"B", (5, 2)},
            {"N", (6, 2)}, {"M", (7, 2)},

            {" ", (4, 3)} // Space simulieren wir mittig unter der Tastatur
        };

        public static List<string> GetTravelDistanceAdjustedSuggestions(string currentInput, List<string> suggestions)
        {
            string cleanInput = currentInput.Replace("\n", "").Replace("\r", "").Replace("\t", "");

            if (string.IsNullOrWhiteSpace(cleanInput))
            {
                Debug.WriteLine("[TD] Input is empty -> Returning all suggestions.");
                return suggestions;
            }

            var filteredSuggestions = new List<string>();

            string lastChar = currentInput.Substring(currentInput.Length - 1, 1).ToUpper();

            Debug.WriteLine($"[TD] Start Travel Check. Threshold: {TRAVELDISTANCE_THRESHOLD}");
            Debug.WriteLine($"[TD] From Key (Last Input): '{lastChar}'");

            foreach (var suggestion in suggestions)
            {
                if (!suggestion.StartsWith(cleanInput, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"[TD] REJECT: '{suggestion}' -> Does not start with '{currentInput}'");
                    continue;
                }

                if (suggestion.Length <= currentInput.Length)
                {
                    Debug.WriteLine($"[TD] REJECT: '{suggestion}' -> Too short/same length (S: {suggestion.Length} <= I: {currentInput.Length})");
                    continue;
                }

                string nextChar = suggestion.Substring(currentInput.Length, 1).ToUpper();

                int distance = CalcTravelDistance(lastChar, nextChar);

                if (distance >= TRAVELDISTANCE_THRESHOLD)
                {
                    Debug.WriteLine($"[TD] KEEP  : '{suggestion}' (Next: '{nextChar}'). Distance: {distance} >= {TRAVELDISTANCE_THRESHOLD}");
                    filteredSuggestions.Add(suggestion);
                }
                else
                {
                    Debug.WriteLine($"[TD] DROP  : '{suggestion}' (Next: '{nextChar}'). Distance: {distance} < {TRAVELDISTANCE_THRESHOLD} (Too close)");
                }
            }

            return filteredSuggestions;
        }

        public static int CalcTravelDistance(string fromKey, string toKey)
        {
            // Debugging für fehlende Keys
            if (!keyCoordinatesQWERTZ.ContainsKey(fromKey))
            {
                Debug.WriteLine($"[TD] WARN: Key '{fromKey}' not in map. Returning 99.");
                return 99;
            }
            if (!keyCoordinatesQWERTZ.ContainsKey(toKey))
            {
                Debug.WriteLine($"[TD] WARN: Key '{toKey}' not in map. Returning 99.");
                return 99;
            }

            var start = keyCoordinatesQWERTZ[fromKey];
            var end = keyCoordinatesQWERTZ[toKey];

            int distance = Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y);
            return distance;
        }
    }
}