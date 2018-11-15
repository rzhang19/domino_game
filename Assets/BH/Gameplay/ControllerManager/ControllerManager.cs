using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;

namespace BH
{
    /// <summary>
    /// Manages the "mode" of the player during gameplay.
    /// The player can either be in "build mode" or "spectator mode".
    /// From either of those modes, the player can also toggle between "free-fly" controls or "fixed-camera" controls.
    /// <para>
    /// A client can make calls through ControllerManager to change the mode and controls of the player.
    /// </para>
    /// </summary>
    /// <seealso cref="BH.DesignPatterns.Singleton{BH.ControllerManager}" />
    public class ControllerManager : Singleton<ControllerManager>
    {
        [SerializeField] TakesInput[] _freeFlyInputs;
        [SerializeField] TakesInput[] _buildModeInputs;
        [SerializeField] TakesInput[] _spectatorModeInputs;
        [SerializeField] Canvas _freeFlyCanvas;
        [SerializeField] Canvas _buildModeCanvas;
        [SerializeField] Canvas _spectatorModeCanvas;
        
        enum Controller
        {
            BuildMode,
            BuildModeFreeFly,
            SpectatorMode,
            SpectatorModeFreeFly
        }
        Controller _controller;

        // Maintain action history as the user switches modes.
        Stack<ActionClass> actions = new Stack<ActionClass>();

        void Start()
        {
            BuildMode();
        }

        void Update()
        {
            if (InputManager.GetKeyDown("Toggle Free-fly"))
            {
                ToggleFreeFly();
            }
            else if (InputManager.GetKeyDown("Toggle Build/Spectate"))
            {
                ToggleMode();
            }
        }
        
        /// <summary>
        /// Toggles between build mode and spectator mode.
        /// </summary>
        public void ToggleMode() // I feel like this function got butchered (or was from the beginning) somehow... "Fixed" it. - Brandon
        {
            switch (_controller)
            {
                case Controller.BuildMode:
                    SaveActions();
                    SelectableManager.Instance.SaveLayout();
                    SpectatorMode();
                    break;
                //case Controller.BuildModeFreeFly:
                //    BuildMode();
                //    break;
                case Controller.SpectatorMode:
                    ((BuildModeController)_buildModeInputs[0]).SetActions(this.actions);
                    SelectableManager.Instance.ResetLayout();
                    BuildMode();
                    break;
                //case Controller.SpectatorModeFreeFly:
                //    SpectatorMode();
                //    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Toggles between free-fly controls and fixed-camera controls.
        /// </summary>
        public void ToggleFreeFly() // "Fixed" this also. - Brandon
        {
            switch (_controller)
            {
                case Controller.BuildMode:
                    //SaveActions();
                    //SelectableManager.Instance.SaveLayout();
                    //SpectatorMode();
                    BuildModeFreeFly();
                    break;
                case Controller.BuildModeFreeFly:
                    //SpectatorMode();
                    BuildMode();
                    break;
                case Controller.SpectatorMode:
                    //BuildMode();
                    SpectatorModeFreeFly();
                    break;
                case Controller.SpectatorModeFreeFly:
                    //BuildMode();
                    SpectatorMode();
                    break;
                default:
                    break;
            }
        }

        void BuildModeFreeFly()
        {
            _controller = Controller.BuildModeFreeFly;
            TakesInput.UnlockInputs(_freeFlyInputs, this);
            TakesInput.LockInputs(_buildModeInputs, this);
            TakesInput.LockInputs(_spectatorModeInputs, this);
            CursorController.HideCursor();
            _freeFlyCanvas.enabled = true;
            _buildModeCanvas.enabled = false;
            _spectatorModeCanvas.enabled = false;
            
            SelectableManager.Instance.FreezeRotation();
        }

        void SpectatorModeFreeFly()
        {
            _controller = Controller.SpectatorModeFreeFly;
            TakesInput.UnlockInputs(_freeFlyInputs, this);
            TakesInput.LockInputs(_buildModeInputs, this);
            TakesInput.LockInputs(_spectatorModeInputs, this);
            CursorController.HideCursor();
            _freeFlyCanvas.enabled = true;
            _buildModeCanvas.enabled = false;
            _spectatorModeCanvas.enabled = false;
            
            SelectableManager.Instance.FreezeRotation();
        }

        void BuildMode()
        {
            _controller = Controller.BuildMode;
            TakesInput.LockInputs(_freeFlyInputs, this);
            TakesInput.UnlockInputs(_buildModeInputs, this);
            TakesInput.LockInputs(_spectatorModeInputs, this);
            CursorController.ShowCursor();
            _freeFlyCanvas.enabled = false;
            _buildModeCanvas.enabled = true;
            _spectatorModeCanvas.enabled = false;
            
            ////////// Moved these calls to (what I think is) a better place in ToggleMode().
            //SelectableManager.Instance.ResetLayout(); // Remove any messes from spectator mode
            //((BuildModeController)_buildModeInputs[0]).SetActions(this.actions);

            SelectableManager.Instance.FreezeRotation();
        }

        void SpectatorMode()
        {
            _controller = Controller.SpectatorMode;
            TakesInput.LockInputs(_freeFlyInputs, this);
            TakesInput.LockInputs(_buildModeInputs, this);
            TakesInput.UnlockInputs(_spectatorModeInputs, this);
            CursorController.ShowCursor();
            _freeFlyCanvas.enabled = false;
            _buildModeCanvas.enabled = false;
            _spectatorModeCanvas.enabled = true;
            
            SelectableManager.Instance.UnfreezeRotation();
        }

        void SaveActions()
        {
            if (_controller != Controller.BuildMode) return;
            this.actions = ((BuildModeController)_buildModeInputs[0]).GetActions();
        }
    }
}
