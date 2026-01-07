using DeftSharp.Windows.Input.Keyboard;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace TypeAssist
{
    static class InputListenerService
    {
        // Source - https://stackoverflow.com/a
        // Posted by Ozgur Ozcitak, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-01-06, License - CC BY-SA 3.0
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        private static KeyboardListener _keyboardListener = new KeyboardListener();
        private static readonly HashSet<Key> _subscribedKeys = new HashSet<Key>()
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
        

        public static void Subscribe(List<char> buffer, List<string> processes, TextBlock textblock, Popup popup, string[] data, SelectionChangedEventHandler e)
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
                                // next 3 lines for debugging
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

                        var process = GetForegroundProcessName();

                        if (process != null)
                        {
                            processes.Add(process);
                        }

                        if (process == "TypeAssist")
                        {
                            return; // Ignore key presses from TypeAssist itself
                        }

                        buffer.Add(charToAdd.Value);

                        //next 4 lines for debugging
                        Debug.WriteLine($"Key Pressed: {charToAdd.Value}");

                        string currentBuffer = new string(buffer.ToArray());
                        Debug.WriteLine($"Current Buffer: {currentBuffer}");

                        textblock.Text = process; 

                        if (!popup.IsOpen)
                        {
                            popup.IsOpen = true;
                        }

                        popup.Child = GenerateListBox(data, e);

                        ProcessHandle();
                    }
                });
            }
        }

        private static ListBox GenerateListBox(string[] data, SelectionChangedEventHandler e)
        {
            var listbox = new ListBox
            {
                ItemsSource = data
            };

            listbox.SelectionChanged += e;

            return listbox;
        }

        private static void ProcessHandle() 
        {
            Process[] typeAssistProcess = Process.GetProcessesByName("TypeAssist");
            IntPtr typeAssistHandle = typeAssistProcess[0].MainWindowHandle;

            SetForegroundWindow(typeAssistHandle);
        }

        private static string GetForegroundProcessName()
        {
            IntPtr hwnd = GetForegroundWindow();

            // The foreground window can be NULL in certain circumstances, 
            // such as when a window is losing activation.
            if (hwnd == null)
                return "Unknown";

            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);

            foreach (Process p in Process.GetProcesses())
            {
                if (p.Id == pid)
                    return p.ProcessName;
            }

            return "Unknown";
        }
    }
}
