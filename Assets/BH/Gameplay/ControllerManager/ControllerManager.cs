using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;

// There are some pretty repetitive lines of code in here. Probably should refactor soon...

namespace BH
{
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

        void Start()
        {
            BuildMode();
        }

        void Update()
        {
            if (InputManager.GetKeyDown("Toggle Free-fly"))
            {
                switch (_controller)
                {
                    case Controller.BuildMode:
                        BuildModeFreeFly();
                        break;
                    case Controller.BuildModeFreeFly:
                        BuildMode();
                        break;
                    case Controller.SpectatorMode:
                        SpectatorModeFreeFly();
                        break;
                    case Controller.SpectatorModeFreeFly:
                        SpectatorMode();
                        break;
                    default:
                        break;
                }
            }
            else if (InputManager.GetKeyDown("Toggle Build/Spectate"))
            {
                switch (_controller)
                {
                    case Controller.BuildMode:
                        SpectatorMode();
                        break;
                    case Controller.BuildModeFreeFly:
                        SpectatorMode();
                        break;
                    case Controller.SpectatorMode:
                        BuildMode();
                        break;
                    case Controller.SpectatorModeFreeFly:
                        BuildMode();
                        break;
                    default:
                        break;
                }
            }
        }

        void BuildModeFreeFly()
        {
            _controller = Controller.BuildModeFreeFly;
            TakesInput.EnableInputs(_freeFlyInputs, this);
            TakesInput.DisableInputs(_buildModeInputs, this);
            TakesInput.DisableInputs(_spectatorModeInputs, this);
            ToggleCursor.HideCursor();
            _freeFlyCanvas.enabled = true;
            _buildModeCanvas.enabled = false;
            _spectatorModeCanvas.enabled = false;
            
            DominoManager.Instance.FreezeRotation();
        }

        void SpectatorModeFreeFly()
        {
            _controller = Controller.SpectatorModeFreeFly;
            TakesInput.EnableInputs(_freeFlyInputs, this);
            TakesInput.DisableInputs(_buildModeInputs, this);
            TakesInput.DisableInputs(_spectatorModeInputs, this);
            ToggleCursor.HideCursor();
            _freeFlyCanvas.enabled = true;
            _buildModeCanvas.enabled = false;
            _spectatorModeCanvas.enabled = false;
            
            DominoManager.Instance.FreezeRotation();
        }

        void BuildMode()
        {
            _controller = Controller.BuildMode;
            TakesInput.DisableInputs(_freeFlyInputs, this);
            TakesInput.EnableInputs(_buildModeInputs, this);
            TakesInput.DisableInputs(_spectatorModeInputs, this);
            ToggleCursor.ShowCursor();
            _freeFlyCanvas.enabled = false;
            _buildModeCanvas.enabled = true;
            _spectatorModeCanvas.enabled = false;
            
            DominoManager.Instance.FreezeRotation();
        }

        void SpectatorMode()
        {
            _controller = Controller.SpectatorMode;
            TakesInput.DisableInputs(_freeFlyInputs, this);
            TakesInput.DisableInputs(_buildModeInputs, this);
            TakesInput.EnableInputs(_spectatorModeInputs, this);
            ToggleCursor.ShowCursor();
            _freeFlyCanvas.enabled = false;
            _buildModeCanvas.enabled = false;
            _spectatorModeCanvas.enabled = true;
            
            DominoManager.Instance.UnfreezeRotation();
        }
    }
}
