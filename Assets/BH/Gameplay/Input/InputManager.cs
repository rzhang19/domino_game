using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    public static class InputManager
    {
        public static KeyCode _pauseKey = (Application.isEditor ? KeyCode.T : KeyCode.Escape);
        
        public static Dictionary<string, KeyCode[]> _keyDict = new Dictionary<string, KeyCode[]>()
        {
            {"Strafe Up",       new KeyCode[] {KeyCode.W, KeyCode.None}},
            {"Strafe Left",     new KeyCode[] {KeyCode.A, KeyCode.None}},
            {"Strafe Down",     new KeyCode[] {KeyCode.S, KeyCode.None}},
            {"Strafe Right",    new KeyCode[] {KeyCode.D, KeyCode.None}},
            
            {"Float",           new KeyCode[] {KeyCode.Space, KeyCode.None}},
            {"Sink",            new KeyCode[] {KeyCode.LeftShift, KeyCode.None}},

            {"Attack1",         new KeyCode[] {KeyCode.Mouse0, KeyCode.None}},
            {"Attack2",         new KeyCode[] {KeyCode.Mouse1, KeyCode.None}}
        };

        public static bool GetKey(string key)
        {
            KeyCode[] keyCodes;
            _keyDict.TryGetValue(key, out keyCodes);
            if (keyCodes != null)
            {
                foreach (KeyCode val in keyCodes)
                {
                    if (Input.GetKey(val))
                        return true;
                }
            }
            return false;
        }

        public static bool GetKeyDown(string key)
        {
            KeyCode[] keyCodes;
            _keyDict.TryGetValue(key, out keyCodes);
            if (keyCodes != null)
            {
                foreach (KeyCode val in keyCodes)
                {
                    if (Input.GetKeyDown(val))
                        return true;
                }
            }
            return false;
        }

        public static bool GetKeyUp(string key)
        {
            KeyCode[] keyCodes;
            _keyDict.TryGetValue(key, out keyCodes);
            if (keyCodes != null)
            {
                foreach (KeyCode val in keyCodes)
                {
                    if (Input.GetKeyUp(val))
                        return true;
                }
            }
            return false;
        }

        public static KeyCode GetFirstKeyCode(string key)
        {
            KeyCode[] keyCodes;
            _keyDict.TryGetValue(key, out keyCodes);
            if (keyCodes != null)
                return keyCodes[0];

            return KeyCode.None;
        }

        public static bool GetPauseKeyDown()
        {
            return Input.GetKeyDown(_pauseKey);
        }

        public static void OverwriteKeybind(string key, KeyCode val, int index)
        {
            if (!_keyDict.ContainsKey(key) || (index < 0 && index >= 1))
                return;

            _keyDict[key][index] = val;
        }
    }
}
