using WindowsInput;
using WindowsInput.Native;

namespace TypeAssist
{
    internal class CompletionService
    {
        static InputSimulator isim = new InputSimulator();

        /// <summary>
        /// Emulates key presses to complete the current word in the buffer using the provided recommendation.
        /// </summary>
        /// <remarks>This method compares the current word in the buffer with the recommendation and
        /// emulates key presses only for the remaining characters needed to complete the word. A space key press is
        /// sent after the completion. If the recommendation is shorter than or equal to the current word, no keys are
        /// emulated.</remarks>
        /// <param name="recommendation">The recommended word or phrase to use for completing the current word. Cannot be null.</param>
        /// <param name="buffer">The buffer containing the current sequence of typed characters. Must not be empty.</param>
        public static void EmulateKeys(string recommendation, List<char> buffer)
        {
            if (buffer.Count == 0 || recommendation == null) return;

            char[] recommendationArray = recommendation.ToLower().ToCharArray();
            string currentBuffer = new string(buffer.ToArray());
            char[] lastWord = currentBuffer.Split(' ').Last().ToLower().ToCharArray();

            if (recommendationArray.Length <= lastWord.Length) return;

            for (int i = 0; i < recommendationArray.Length; i++)
            {
                if (i < lastWord.Count())
                {
                    if (recommendationArray[i] == lastWord[i]) continue;
                    else break;
                }
                else
                {
                    isim.Keyboard.KeyPress(VirtualKeyCode.VK_A + (char.ToUpper(recommendationArray[i]) - 'A'));
                }
            }

            if (!(ConfigService.GetSettings().Mode == "Silben") && !(ConfigService.GetSettings().Mode == "Buchstaben"))
            {
                isim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            }
        }

        /// <summary>
        /// Simulates a single key press corresponding to the specified character.
        /// </summary>
        /// <remarks>This method emulates only a single key press for the specified character. Modifier
        /// keys (such as Shift) are not applied, so only unmodified key presses are supported. Characters that do not
        /// correspond to a supported key are ignored.</remarks>
        /// <param name="key">The character representing the key to emulate. Supported values include letters (A-Z, a-z), digits (0-9),
        /// space, tab, and newline characters.</param>
        public static void EmulateSingleKey(char key)
        {
            if (char.IsLetter(key))
            {
                isim.Keyboard.KeyPress(VirtualKeyCode.VK_A + (char.ToUpper(key) - 'A'));
            }
            else if (char.IsDigit(key))
            {
                isim.Keyboard.KeyPress(VirtualKeyCode.VK_0 + (key - '0'));
            }
            else
            {
                switch (key)
                {
                    case ' ':
                        isim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                        break;
                    case '\n':
                        isim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                        break;
                    case '\t':
                        isim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        break;
                }
            }
        }

        public static void EmulateBackspace()
        {
            isim.Keyboard.KeyPress(VirtualKeyCode.BACK);
        }
    }
}
