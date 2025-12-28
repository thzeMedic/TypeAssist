using DeftSharp.Windows.Input.Keyboard;
using System.Diagnostics;
using System.Windows.Input;

namespace TypeAssist
{
    static class InputListenerService
    {
        private static KeyboardListener _keyboardListener = new KeyboardListener();
        private static HashSet<Key> _subscribedKeys = new HashSet<Key>()
        {
            Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G,
            Key.H, Key.I, Key.J, Key.K, Key.L, Key.M, Key.N,
            Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T, Key.U,
            Key.V, Key.W, Key.X, Key.Y, Key.Z,
            Key.D0, Key.D1, Key.D2, Key.D3, Key.D4,
            Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, 
            Key.Space, Key.Enter, Key.Back, Key.Tab, Key.OemComma, Key.OemPeriod
        };
        private static KeyConverter _keyconverter = new KeyConverter();

        public static void Subscribe(List<char> buffer)
        {
            foreach (var keyEnum in _subscribedKeys)
            {
                _keyboardListener.Subscribe(keyEnum, pressedKey => {
                    char? charToAdd = null;

                    switch (pressedKey)
                    {
                        case Key.Space:
                            charToAdd = ' ';
                            break;
                        case Key.Enter:
                            charToAdd = '\n';
                            break;
                        case Key.Tab:
                            charToAdd = '\t';
                            break;
                        case Key.Back:
                            if (buffer.Count > 0)
                            {
                                buffer.RemoveAt(buffer.Count - 1);
                                Debug.WriteLine("Backspace Pressed. Removed last character.");

                                string currentBuffer = new string(buffer.ToArray());
                                Debug.WriteLine($"Current Buffer: {currentBuffer}");
                            }
                            return; // Exit early to avoid adding a character
                        case Key.OemComma:
                            charToAdd = ',';
                            break;
                        case Key.OemPeriod:
                            charToAdd = '.';
                            break;
                        default:
                            string? keyString = _keyconverter.ConvertToString(pressedKey);
                            if (char.TryParse(keyString, out char keyChar))
                            {

                                charToAdd = keyChar;

                            }
                            break;
                    }
 
                    if (charToAdd.HasValue)
                    {
                        buffer.Add(charToAdd.Value);
                        Debug.WriteLine($"Key Pressed: {charToAdd.Value}");

                        string currentBuffer = new string(buffer.ToArray());
                        Debug.WriteLine($"Current Buffer: {currentBuffer}");
                    }
                });
            }
        }

        public static void Unsubscribe()
        {
            _keyboardListener.Unsubscribe(Key.A);
        }
    }
}
