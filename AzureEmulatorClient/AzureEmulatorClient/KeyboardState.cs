using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureEmulatorClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using WindowsInput;
    using WindowsInput.Native;

    [Serializable]
    public class KeyboardState
    {
        private HashSet<Keys> _pressedKeys;
        public KeyboardState()
        {
            this._pressedKeys = new HashSet<Keys>();
        }

        public void Press(Keys key)
        {
            if (!_pressedKeys.Contains(key))
            {
                Console.WriteLine("Pressed " + key);
                _pressedKeys.Add(key);
            }
        }

        public void Release(Keys key)
        {
            if (_pressedKeys.Contains(key))
            {
                Console.WriteLine("Released " + key);
                _pressedKeys.Remove(key);
            }
            else
            {
                Console.WriteLine("Unable to release " + key);
            }
        }

        public void SimulateDif(KeyboardState oldstate)
        {
            HashSet<Keys> oldkeys = new HashSet<Keys>(oldstate._pressedKeys);
            HashSet<Keys> newkeys = new HashSet<Keys>(_pressedKeys);

            newkeys.ExceptWith(oldstate._pressedKeys);
            oldkeys.ExceptWith(_pressedKeys);

            InputSimulator iss = new InputSimulator();
            foreach (Keys key in oldkeys)
            {
                iss.Keyboard.KeyUp((VirtualKeyCode)(int)key);
            }

            foreach (Keys key in newkeys)
            {
                iss.Keyboard.KeyDown((VirtualKeyCode)(int)key);
            }
        }

        public string State
        {
            get
            {
                return "Keyboard: " + String.Join(",", _pressedKeys.ToArray());
            }
        }
    }

}
