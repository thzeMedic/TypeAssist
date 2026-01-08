using WindowsInput;

namespace TypeAssist
{
    internal class CompletionService
    {
        public static void EmulateKeys (string recommendation, List<char> buffer)
        {
            InputSimulator isim = new InputSimulator();
            if (buffer.Count == 0 || recommendation == null) return; // Nothing to complete

            char[] recommendationArray = recommendation.ToCharArray();

            if(buffer.Last() != ' ')
            {
                // If the last character in the buffer is not a space, we add a space to separate with the recommendation
                buffer.Add(' ');
                isim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.SPACE);
            }
            string currentBuffer = new string(buffer.ToArray());
            char[] lastWord = currentBuffer.Split(' ').Last().ToCharArray();

            

            if (recommendationArray.Length <= lastWord.Length) return; // No additional characters to complete

            for (int i = 0; i < recommendationArray.Length; i++)
            {
                if (i < lastWord.Count())
                {
                    if (recommendationArray[i] == lastWord[i]) continue; // Skip already typed characters
                    else break; // Mismatch found, stop processing further
                }
                else
                {
                    isim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.VK_A + (char.ToUpper(recommendationArray[i]) - 'A'));
                }
            }
            isim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.SPACE);
        }

        public static void EmulateSingleKey (char key)
        {
            InputSimulator isim = new InputSimulator();
            if (char.IsLetter(key))
            {
                isim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.VK_A + (char.ToUpper(key) - 'A'));
            }
            else if (char.IsDigit(key))
            {
                isim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.VK_0 + (key - '0'));
            }
            else
            {
                switch (key)
                {
                    case ' ':
                        isim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.SPACE);
                        break;
                    case '\n':
                        isim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
                        break;
                    case '\t':
                        isim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.TAB);
                        break;
                    // Add more special characters as needed
                }
            }
        }

        public static void EmulateBackspace()
        {
            InputSimulator isim = new InputSimulator();
            isim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.BACK);
        }
    }
}
