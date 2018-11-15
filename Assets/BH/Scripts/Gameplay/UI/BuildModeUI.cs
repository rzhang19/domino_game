using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// Implements an interface for UI objects to interact with a BuildModeController.
    /// This class is an adapter for BuildModeController with some additional functionalities.
    /// This class exists because Unity's UI objects are limited to calling no-argument and one-argument functions.
    /// </summary>
    /// <seealso cref="BH.BuildModeController" />
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class BuildModeUI : MonoBehaviour
    {
        [SerializeField] BuildModeController _buildModeController;

        Color _col = Color.black;

        void Awake()
        {
            if (!_buildModeController)
            {
                _buildModeController = GetComponentInChildren<BuildModeController>();
                if (!_buildModeController)
                    Debug.LogError("Build mode controller is not initialized.");
            }
        }

        /// <summary>
        /// Triggers the current BuildModeController to change selectables to BuildModeUI's saved color.
        /// </summary>
        public void ChangeColor()
        {
            _buildModeController.ChangeColor(_col);
        }

        /// <summary>
        /// Triggers the current BuildModeController to reset selectables to their default colors.
        /// </summary>
        public void ResetColor()
        {
            _buildModeController.ResetColor();
        }

        /// <summary>
        /// Updates the red value of BuildModeUI's saved color.
        /// </summary>
        public void SetRedValue(float val)
        {
            _col = new Color(val / 255f, _col.g, _col.b);
        }

        /// <summary>
        /// Updates the green value of BuildModeUI's saved color.
        /// </summary>
        public void SetGreenValue(float val)
        {
            _col = new Color(_col.r, val / 255f, _col.b);
        }

        /// <summary>
        /// Updates the blue value of BuildModeUI's saved color.
        /// </summary>
        public void SetBlueValue(float val)
        {
            _col = new Color(_col.r, _col.g, val / 255f);
        }
    }
}
