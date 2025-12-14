using DeftSharp.Windows.Input.Keyboard;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
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
            Key.D5, Key.D6, Key.D7, Key.D8, Key.D9
        };

        public static void Subscribe(TextBlock block)
        {
            foreach (var key in _subscribedKeys)
            {
                _keyboardListener.Subscribe(key, key => block.Text = $"Key pressed: {key}");
            }
        }

        public static void Unsubscribe()
        {
            _keyboardListener.Unsubscribe(System.Windows.Input.Key.A);
        }
    }
}
