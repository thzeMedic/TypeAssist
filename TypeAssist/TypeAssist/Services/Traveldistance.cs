using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TypeAssist.Services
{
    internal class Traveldistance
    {
        public const int TRAVELDISTANCE_THRESHOLD = 3;

        private static Dictionary<string, (int x, int y)> keyCoordinatesQWERTZ = new Dictionary<string, (int x, int y)>
        {
            
            {"Q", (0, 0)}, {"W", (1, 0)}, {"E", (2, 0)}, {"R", (3, 0)}, {"T", (4, 0)},
            {"Z", (5, 0)}, {"U", (6, 0)}, {"I", (7, 0)}, {"O", (8, 0)}, {"P", (9, 0)},
            {"Ü", (10, 0)}, {"ß", (11, -1)},
            {"A", (0, 1)}, {"S", (1, 1)}, {"D", (2, 1)}, {"F", (3, 1)}, {"G", (4, 1)},
            {"H", (5, 1)}, {"J", (6, 1)}, {"K", (7, 1)}, {"L", (8, 1)}, {"Ö", (9, 1)}, {"Ä", (10, 1)},
         
            {"Y", (1, 2)}, {"X", (2, 2)}, {"C", (3, 2)}, {"V", (4, 2)}, {"B", (5, 2)},
            {"N", (6, 2)}, {"M", (7, 2)}
        };

       
        public static List<string> GetTravelDistanceAdjustedSuggestions(string currentInput, List<string> suggestions)
        {
          
            if (string.IsNullOrEmpty(currentInput)) return suggestions;

            var filteredSuggestions = new List<string>();

           
            string lastChar = currentInput.Substring(currentInput.Length - 1, 1).ToUpper();

            Debug.WriteLine($"--- TravelDistance Check (Threshold: {TRAVELDISTANCE_THRESHOLD}) ---");
            Debug.WriteLine($"Start Key: {lastChar}");

            foreach (var suggestion in suggestions)
            {
               
                if (!suggestion.StartsWith(currentInput, StringComparison.OrdinalIgnoreCase)) continue;

               
                if (suggestion.Length <= currentInput.Length) continue;

               
                string nextChar = suggestion.Substring(currentInput.Length, 1).ToUpper();

                
                int distance = CalcTravelDistance(lastChar, nextChar);

                
                if (distance >= TRAVELDISTANCE_THRESHOLD)
                {
                    Debug.WriteLine($"[KEEP] '{suggestion}' (Next: {nextChar}). Dist: {distance}");
                    filteredSuggestions.Add(suggestion);
                }
                else
                {
                    Debug.WriteLine($"[DROP] '{suggestion}' (Next: {nextChar}). Dist: {distance} (Too easy)");
                }
            }

            return filteredSuggestions;
        }
        public static int CalcTravelDistance(string fromKey, string toKey)
        {

            if (!keyCoordinatesQWERTZ.ContainsKey(fromKey) || !keyCoordinatesQWERTZ.ContainsKey(toKey))
            {
                return 99;
            }

            var start = keyCoordinatesQWERTZ[fromKey];
            var end = keyCoordinatesQWERTZ[toKey];

            // Manhattan Distance: |x1 - x2| + |y1 - y2|
            int distance = Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y);

            return distance;
        }
    }
}
