using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// An interface for Unity's input system.
    /// This class creates a mapping of strings to KeyCodes to aid in the readability of inputs.
    /// </summary>
    public static class InputManager
    {
        public static KeyCode _pauseKey = (Application.isEditor ? KeyCode.T : KeyCode.Escape);
        
        public static Dictionary<string, KeyCode[]> _keyDict = new Dictionary<string, KeyCode[]>()
        {
            {"Strafe Up",                   new KeyCode[] {KeyCode.W, KeyCode.None}},
            {"Strafe Left",                 new KeyCode[] {KeyCode.A, KeyCode.None}},
            {"Strafe Down",                 new KeyCode[] {KeyCode.S, KeyCode.None}},
            {"Strafe Right",                new KeyCode[] {KeyCode.D, KeyCode.None}},
            
            {"Float",                       new KeyCode[] {KeyCode.Space, KeyCode.None}},
            {"Sink",                        new KeyCode[] {KeyCode.LeftShift, KeyCode.None}},

            {"Attack1",                     new KeyCode[] {KeyCode.Mouse0, KeyCode.None}},
            {"Attack2",                     new KeyCode[] {KeyCode.Mouse1, KeyCode.None}},
            
            {"Toggle Build/Spectate",       new KeyCode[] {KeyCode.T, KeyCode.None}},
            {"Toggle Free-fly",             new KeyCode[] {KeyCode.F, KeyCode.None}},
            
            {"Toggle Rotation Axis",        new KeyCode[] {KeyCode.R, KeyCode.None}},
            {"Undo",                        new KeyCode[] {KeyCode.Z, KeyCode.None}},

            {"Toggle drag mouse",           new KeyCode[] {KeyCode.M, KeyCode.None}},

            {"Toggle Projection",           new KeyCode[] {KeyCode.G, KeyCode.None}}, //was V, but Paste took V

            {"Copy",                        new KeyCode[] {KeyCode.C, KeyCode.None}},
            {"Paste",                        new KeyCode[] {KeyCode.V, KeyCode.None}}
        };

        /// <summary>
        /// Gets the keypress status.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the keypress-down status.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the keypress-up status.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the first keycode corresponding to key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static KeyCode GetFirstKeyCode(string key)
        {
            KeyCode[] keyCodes;
            _keyDict.TryGetValue(key, out keyCodes);
            if (keyCodes != null)
                return keyCodes[0];

            return KeyCode.None;
        }

        /// <summary>
        /// Gets the pause-keypress-down status.
        /// </summary>
        /// <returns></returns>
        public static bool GetPauseKeyDown()
        {
            return Input.GetKeyDown(_pauseKey);
        }

        /// <summary>
        /// Overwrites the keybind for specified key, val, and index.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        /// <param name="index">The index.</param>
        public static void OverwriteKeybind(string key, KeyCode val, int index)
        {
            if (!_keyDict.ContainsKey(key) || (index < 0 && index >= 1))
                return;

            _keyDict[key][index] = val;
        }
    }
}
