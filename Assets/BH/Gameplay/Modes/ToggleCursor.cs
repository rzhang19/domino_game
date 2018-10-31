using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    public static class ToggleCursor
    {
        public static void HideCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public static void ShowCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
