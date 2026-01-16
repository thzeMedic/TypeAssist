using DeftSharp.Windows.Input.Keyboard;
using System.Diagnostics;
using System.Runtime.InteropServices;
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

        private static CancellationTokenSource _cts;

        private static Dictionary<Key, DateTime> _lastKeyPressTimes = new Dictionary<Key, DateTime>();

        public static void SubscribeSetting()
        {
            _keyboardListener.SubscribeCombination([Key.LeftCtrl, Key.F1], () =>
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Show();
            });
        }

        public static void Subscribe(List<char> buffer, List<string> processes, Action<string> onBufferChanged)
        {
            foreach (var keyEnum in _subscribedKeys)
            {

                _keyboardListener.Subscribe(keyEnum, pressedKey => 
                {
                    var process = GetForegroundProcessName();

                    if (_lastKeyPressTimes.ContainsKey(pressedKey))
                    {
                        var timeSinceLast = DateTime.Now - _lastKeyPressTimes[pressedKey];
                        if (timeSinceLast.TotalMilliseconds < 100) // 100ms Debounce
                        {
                            return; 
                        }
                    }
                    _lastKeyPressTimes[pressedKey] = DateTime.Now;

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
                            if (process == "TypeAssist")
                            {
                                return;
                            }
                            charToAdd = '\t';
                            break;
                        case Key.Back:
                            if (buffer.Count > 0)
                            {
                                buffer.RemoveAt(buffer.Count - 1);
                                Debug.WriteLine("Backspace Pressed. Removed last character.");

                                if (process != null)
                                {
                                    processes.Add(process);
                                }

                                if (process == "TypeAssist")
                                {
                                    NoCompletion(processes);
                                    CompletionService.EmulateBackspace();
                                }
                                string currentBuffer = new string(buffer.ToArray());
                                Debug.WriteLine($"Current Buffer: {currentBuffer}");
                            }
                            return; 
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
                        if (process != null)
                        {
                            processes.Add(process);
                        }

                        if (process == "TypeAssist")
                        {
                            NoCompletion(processes);
                            CompletionService.EmulateSingleKey(charToAdd.Value);
                            buffer.Add(charToAdd.Value);
                            return;
                        }

                        buffer.Add(charToAdd.Value);

                        if (_cts != null)
                        {
                            _cts.Cancel();
                            _cts.Dispose();
                        }

                        _cts = new CancellationTokenSource();

                        string currentText = new string(buffer.ToArray());

                        onBufferChanged?.Invoke(currentText);
                    }
                });
            }
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

        private static void NoCompletion(List<string> processes)
        {
            IntPtr handleEmulation;

            if (processes.Count == 1)
            {
                Process[] notTypeAssist = Process.GetProcessesByName(processes[0]);
                handleEmulation = notTypeAssist[0].MainWindowHandle;
            }
            else
            {
                Process[] notTypeAssist = Process.GetProcessesByName(processes[processes.Count - 2]);
                handleEmulation = notTypeAssist[0].MainWindowHandle;
            }
            SetForegroundWindow(handleEmulation);
        }
    }
}
