using WindowsInput;

namespace TypeAssist
{
    internal class CompletionService
    {
        public static void EmulateKeys (string recommendation, List<char> buffer)
        {
            if (buffer.Count == 0 || recommendation == null) return; // Nothing to complete

            char[] recommendationArray = recommendation.ToCharArray();

            string currentBuffer = new string(buffer.ToArray());
            char[] lastWord = currentBuffer.Split(' ').Last().ToCharArray();

            InputSimulator isim = new InputSimulator();

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
    }
}
