using DeftSharp.Windows.Input.Keyboard;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using TypeAssist.Services;

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
        public static bool IgnoreInput { get; set; } = false;
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
        private static bool clearBuffer = false;
        private static CancellationTokenSource _cts;
        private static Key _lastKeyPressed = Key.None;
        private static Dictionary<Key, DateTime> _lastKeyPressTimes = new Dictionary<Key, DateTime>();
        private static int _travelDistanceTilLastKey = -1;

        public static void SubscribeSettingsHotkey()
        {
            _keyboardListener.SubscribeCombination([Key.LeftCtrl, Key.F1], () =>
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Show();
            });
        }

        public static void SubscribeClearBufferHotkey()
        {
            _keyboardListener.SubscribeCombination([Key.LeftCtrl, Key.F2], () =>
            {
                clearBuffer = true;
            });
        }

        /// <summary>
        /// Subscribes to keyboard events and updates the provided character buffer and process list in response to key
        /// presses. Invokes a callback when the buffer changes.
        /// </summary>
        /// <param name="buffer">The list of characters representing the current input buffer. This list is modified in place as keys are
        /// pressed.</param>
        /// <param name="processes">The list to which the names of foreground processes are added when relevant key events occur. This list is
        /// updated in place.</param>
        /// <param name="onBufferChanged">A callback that is invoked with the current buffer contents as a string whenever the buffer is modified. Can
        /// be null if no notification is needed.</param>
        public static void Subscribe(List<char> buffer, List<string> processes, Action<string> onBufferChanged)
        {
            foreach (var keyEnum in _subscribedKeys)
            {

                _keyboardListener.Subscribe(keyEnum, pressedKey =>
                {
                    if (IgnoreInput) return;

                    if (IsDebounced(pressedKey)) return;

                    var process = GetForegroundProcessName();

                    if (pressedKey == Key.Back)
                    {
                        HandleBackspace(buffer, processes, process);
                    }

                    if (pressedKey == Key.Tab && process == "TypeAssist") return;

                    char? charToAdd = MapKeyToChar(pressedKey);

                    if (charToAdd.HasValue)
                    {
                        ProcessInput(buffer, processes, process, charToAdd.Value, onBufferChanged);
                    }

                    string charString = charToAdd.HasValue ? charToAdd.Value.ToString().ToUpper() : "";
                    string lastKeyString = _lastKeyPressed.ToString().ToUpper();

                    if (!string.IsNullOrEmpty(charString))
                    {
                        _travelDistanceTilLastKey = Traveldistance.CalcTravelDistance(lastKeyString, charString);
                    }

                    Debug.WriteLine($"TravelDistance : {_travelDistanceTilLastKey}");

                    _lastKeyPressed = pressedKey;
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

        /// <summary>
        /// Determines whether the specified key press should be ignored due to debounce timing.
        /// </summary>
        /// <remarks>This method is typically used to prevent processing rapid, repeated key presses by
        /// ignoring inputs that occur within a short time frame. The debounce interval is 100 milliseconds.</remarks>
        /// <param name="key">The key to evaluate for debounce.</param>
        /// <returns>true if the key press occurred within the debounce interval and should be ignored; otherwise, false.</returns>
        private static bool IsDebounced(Key key)
        {
            if (_lastKeyPressTimes.ContainsKey(key))
            {
                var timeSinceLast = DateTime.Now - _lastKeyPressTimes[key];
                if (timeSinceLast.TotalMilliseconds < 100) // 100ms Debounce
                {
                    return true;
                }
            }
            _lastKeyPressTimes[key] = DateTime.Now;
            return false;
        }

        /// <summary>
        /// Maps a specified key value to its corresponding character representation, if available.
        /// </summary>
        /// <remarks>This method provides character mappings for select keys. For keys not explicitly
        /// handled, an attempt is made to convert the key to a string representation, which may return null if no
        /// mapping is possible.</remarks>
        /// <param name="key">The key to convert to a character. Common keys such as Space, Enter, Tab, OemComma, and OemPeriod are mapped
        /// to their respective characters.</param>
        /// <returns>A character representing the specified key if a mapping exists; otherwise, null.</returns>
        private static char? MapKeyToChar(Key key)
        {
            return key switch
            {
                Key.Space => ' ',
                Key.Enter => '\n',
                Key.Tab => '\t',
                Key.OemComma => ',',
                Key.OemPeriod => '.',
                _ => TryConvertToString(key)
            };
        }

        /// <summary>
        /// Attempts to convert the specified key to its corresponding character representation.
        /// </summary>
        /// <param name="key">The key to convert to a character.</param>
        /// <returns>A character representing the key if the conversion is successful; otherwise, null.</returns>
        private static char? TryConvertToString(Key key)
        {
            string? keyString = _keyconverter.ConvertToString(key);
            if (char.TryParse(keyString, out char keyChar))
            {
                return keyChar;
            }
            return null;
        }

        /// <summary>
        /// Handles a backspace operation by removing the last character from the input buffer and updating the process
        /// list as needed.
        /// </summary>
        /// <remarks>If the process is "TypeAssist", additional completion-related actions are performed
        /// after the backspace is handled.</remarks>
        /// <param name="buffer">The buffer containing the current sequence of input characters. The last character will be removed if the
        /// buffer is not empty.</param>
        /// <param name="processes">The list of process names to which the current process may be added if applicable.</param>
        /// <param name="process">The name of the process associated with the backspace event. If not null, it may be added to the process
        /// list and may trigger additional actions if it equals "TypeAssist".</param>
        private static void HandleBackspace(List<char> buffer, List<string> processes, string process)
        {

            lock (buffer)
            {
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
                }
            }
        }

        /// <summary>
        /// Processes the specified input character, updates the buffer and process list, and notifies listeners of
        /// buffer changes.
        /// </summary>
        /// <param name="buffer">The list of characters representing the current input buffer. The method adds the specified character to
        /// this buffer.</param>
        /// <param name="processes">The collection of process names that tracks the sequence of input processing steps. The method may add the
        /// current process to this list.</param>
        /// <param name="process">The name of the process to apply for the input character. If not null, it is added to the process list.</param>
        /// <param name="charToAdd">The character to be added to the input buffer and processed according to the specified process.</param>
        /// <param name="onBufferChanged">An action delegate that is invoked with the updated buffer text after the input character is processed. Can
        /// be null if no notification is required.</param>
        private static void ProcessInput(List<char> buffer, List<string> processes, string process, char charToAdd, Action<string> onBufferChanged)
        {
            // dont even add typeassist to the process list we dont want
            // to focous it again
            if (process != null && process != "TypeAssist")
            {
                processes.Add(process);
                if (processes.Count > 50) processes.RemoveAt(0);
            }

            if (process == "TypeAssist")
            {
                NoCompletion(processes);
                CompletionService.EmulateSingleKey(charToAdd);
                lock (buffer)
                {
                    buffer.Add(charToAdd);
                    onBufferChanged?.Invoke(new string(buffer.ToArray()));
                }
                return;
            }

            string currentText;

            lock (buffer)
            {
                buffer.Add(charToAdd);

                try
                {
                    var settings = ConfigService.GetSettings();
                    if (charToAdd == ' ' && (settings.Mode == "Silben" || settings.Mode == "Buchstaben"))
                    {
                        buffer.Clear();
                        Debug.WriteLine($"[InputListener] Buffer cleared because of Space in Mode '{settings.Mode}'");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[InputListener] Error checking settings: {ex.Message}");
                }

                if (clearBuffer)
                {
                    buffer.Clear();
                    Debug.WriteLine("Buffer has been cleared.");
                    clearBuffer = false;
                }

                currentText = new string(buffer.ToArray());
            }

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            _cts = new CancellationTokenSource();

            onBufferChanged?.Invoke(currentText);
        }


        /// <summary>
        /// If the user does not select a completion, this method brings the previously active window back to the foreground 
        /// and emulates the pressed key.
        /// </summary>
        /// <param name="processes">a list of processes to get the last window before a recommendation was given.</param>
        private static void NoCompletion(List<string> processes)
        {
            // change: take the last process that is not TypeAssist
            string targetProcessName = processes.LastOrDefault(p => p != "TypeAssist");

            if (string.IsNullOrEmpty(targetProcessName)) return; 

            IntPtr handleEmulation = IntPtr.Zero;

            Process[] targetProcesses = Process.GetProcessesByName(targetProcessName);

            if (targetProcesses.Length > 0)
            {
                handleEmulation = targetProcesses[0].MainWindowHandle;

                if (handleEmulation != IntPtr.Zero)
                {
                    SetForegroundWindow(handleEmulation);
                }
            }
        }
    }
}
