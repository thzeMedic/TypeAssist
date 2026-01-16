using WindowsInput;
using WindowsInput.Native;

namespace TypeAssist
{
    internal class CompletionService
    {
        public static void EmulateKeys (string recommendation, List<char> buffer)
        {
            InputSimulator isim = new InputSimulator();
            if (buffer.Count == 0 || recommendation == null) return; // Nothing to complete

            char[] recommendationArray = recommendation.ToLower().ToCharArray();

            //if(buffer.Last() != ' ')
            //{
            //    // If the last character in the buffer is not a space, we add a space to separate with the recommendation
            //    buffer.Add(' ');
            //    isim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            //}
            string currentBuffer = new string(buffer.ToArray());
            char[] lastWord = currentBuffer.Split(' ').Last().ToLower().ToCharArray();

            

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
                   isim.Keyboard.KeyPress(VirtualKeyCode.VK_A + (char.ToUpper(recommendationArray[i]) - 'A'));
                }
            }
             isim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
        }

        public static void EmulateSingleKey (char key)
        {
            InputSimulator isim = new InputSimulator();
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
                    // Add more special characters as needed
                }
            }
        }

        public static void EmulateBackspace()
        {
            InputSimulator isim = new InputSimulator();
            isim.Keyboard.KeyPress(VirtualKeyCode.BACK);
        }
    }
}
