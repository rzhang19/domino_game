using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BH
{
    /// <summary>
    /// An interface for Unity's input system.
    /// This class creates a mapping of strings to KeyCodes to aid in the readability of inputs.
    /// </summary>
    public static class InputManager
    {
        public static KeyCode _pauseKey = (Application.isEditor ? KeyCode.T : KeyCode.Escape);
        // saves the last programmatically simulated key. Stays null/false for normal gaming; only used for testing
        static bool _isSimulatingKeyDown = false;
        static string _simulatedKeyDown = null;
        static bool _isSimulatingKeyUp = false;
        static string _simulatedKeyUp = null; 
        static bool _isSimulatingCursor = false;
        static Vector3 _simulatedCursorPos;
        static bool _isSimulatingScroll = false;
        static float _simulatedScroll;
        static bool _simulatePointerOverGameObject = false;
        
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
                    else if (_isSimulatingKeyDown && _simulatedKeyDown == key)
                    {
                        return true;
                    }
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
                    else if (_isSimulatingKeyDown && _simulatedKeyDown == key)
                    {
                        return true;
                    }
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
                    else if (_isSimulatingKeyUp && _simulatedKeyUp == key)
                    {
                        return true;
                    }
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
        /// Returns the current cursor position.
        /// If the position is simulated, the simulation is wiped after.
        /// </summary>
        public static Vector3 GetCursorPos()
        {
            Vector3 pos;
            if (_isSimulatingCursor)
            {
                pos = _simulatedCursorPos;
            }
            else
            {
                pos = Input.mousePosition;
            }
            return pos;
        }

        /// <summary>
        /// Returns the current scroll position.
        /// If the scroll is simulated, the simulation is wiped after.
        /// </summary>
        public static float GetScroll()
        {
            return _isSimulatingScroll? _simulatedScroll : Input.GetAxisRaw("Mouse ScrollWheel");
        }

        /// <summary>
        /// Returns true if the pointer is over a game object.
        /// If the pointer is simulated, it always returns false.
        /// </summary>
        public static bool IsPointerOverGameObject()
        {
            return _simulatePointerOverGameObject? false : EventSystem.current.IsPointerOverGameObject();
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

        /// <summary>
        /// Programmatically simulate a down keypress. 
        /// </summary>
        public static void SimulateKeyDown(string key)
        {
            _isSimulatingKeyDown = true;
            _simulatedKeyDown = key;
            _simulatedKeyUp = null;
        }

        /// <summary>
        /// Programmatically simulate a down keypress. 
        /// </summary>
        public static void SimulateKeyUp(string key)
        {
            _isSimulatingKeyUp = true;
            _simulatedKeyUp = key;
            _simulatedKeyDown = null;
        }

        /// <summary>
        /// Programmatically turns on cursor simulation and simulates cursor movement. 
        /// Once simulated, the movement will stay until it has been retrieved by GetCursorPos
        /// </summary>
        public static void SimulateCursorMoveTo(Vector3 pos)
        {
            _isSimulatingCursor = true;
            _simulatedCursorPos = pos;
        }

        /// <summary>
        /// Programmatically turns on cursor simulation and simulates cursor movement. 
        /// Once simulated, the movement will stay until it has been retrieved by GetCursorPos
        /// </summary>
        public static void SimulateScrollTo(float newScroll)
        {
            _isSimulatingScroll = true;
            _simulatedScroll = newScroll;
        }

        /// <summary>
        /// Programmatically turns on pointer over game object simulation
        /// </summary>
        public static void SimulatePointerOverGameObject()
        {
            _simulatePointerOverGameObject = true;
        }  

        /// <summary>
        /// Programmatically turns off cursor simulation.
        /// </summary>
        public static void DisableCursorSimulation()
        {
            _isSimulatingCursor = false;
        }

        /// <summary>
        /// Programmatically turns off keypress simulation
        /// </summary>
        public static void DisableKeypressSimulation()
        {
            _isSimulatingKeyDown = false;
            _isSimulatingKeyUp = false;
        }

        /// <summary>
        /// Programmatically turns off scrolling simulation
        /// </summary>
        public static void DisableScrollSimulation()
        {
            _isSimulatingScroll = false;
        }

        /// <summary>
        /// Programmatically turns off pointer over game object detection
        /// </summary>
        public static void DisableSimulatePointerOverGameObject()
        {
            _simulatePointerOverGameObject = false;
        }  

    }
}
