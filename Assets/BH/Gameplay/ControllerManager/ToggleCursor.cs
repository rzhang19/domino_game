using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// Controls the state of the mouse cursor.
    /// </summary>
    public static class CursorController
    {
        /// <summary>
        /// Hides the cursor and locks it to the center of the screen.
        /// </summary>
        public static void HideCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Shows the cursor and frees its movement.
        /// </summary>
        public static void ShowCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // public static ChangeCursor(Texture2D texture) {...}
    }
}
