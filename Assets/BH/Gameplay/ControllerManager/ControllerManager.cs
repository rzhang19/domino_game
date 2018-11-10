using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;

// There are some pretty repetitive lines of code in here. Probably should refactor soon...

namespace BH
{
    public class ControllerManager : Singleton<ControllerManager>
    {
        [SerializeField] TakesInput[] _freeFloatInputs;
        [SerializeField] TakesInput[] _pickupInputs;
        [SerializeField] TakesInput[] _selectInputs;
        [SerializeField] TakesInput[] _spectatorModeInputs;
        [SerializeField] Canvas _freeFloatCanvas;
        [SerializeField] Canvas _pickupCanvas;
        [SerializeField] Canvas _selectCanvas;
        [SerializeField] Canvas _spectatorModeCanvas;

        enum Controller
        {
            FreeFloat,
            Pickup,
            Select,
            SpectatorMode
        }
        Controller _controller;

        void Start()
        {
            Select();
        }

        void Update()
        {
            if (InputManager.GetPauseKeyDown())
            {
                switch (_controller)
                {
                    case Controller.FreeFloat:
                        Pickup();
                        break;
                    case Controller.Pickup:
                        Select();
                        break;
                    case Controller.Select:
                        SpectatorMode();
                        break;
                    case Controller.SpectatorMode:
                        FreeFloat();
                        break;
                    default:
                        break;
                }
            }
        }

        void FreeFloat()
        {
            _controller = Controller.FreeFloat;
            TakesInput.EnableInputs(_freeFloatInputs, this);
            TakesInput.DisableInputs(_pickupInputs, this);
            TakesInput.DisableInputs(_selectInputs, this);
            ToggleCursor.HideCursor();
            _freeFloatCanvas.enabled = true;
            _pickupCanvas.enabled = false;
            _selectCanvas.enabled = false;
            _spectatorModeCanvas.enabled = false;
        }

        void Pickup()
        {
            _controller = Controller.Pickup;
            TakesInput.DisableInputs(_freeFloatInputs, this);
            TakesInput.EnableInputs(_pickupInputs, this);
            TakesInput.DisableInputs(_selectInputs, this);
            TakesInput.DisableInputs(_spectatorModeInputs, this);
            ToggleCursor.ShowCursor();
            _freeFloatCanvas.enabled = false;
            _pickupCanvas.enabled = true;
            _selectCanvas.enabled = false;
            _spectatorModeCanvas.enabled = false;
        }

        void Select()
        {
            _controller = Controller.Select;
            TakesInput.DisableInputs(_freeFloatInputs, this);
            TakesInput.DisableInputs(_pickupInputs, this);
            TakesInput.EnableInputs(_selectInputs, this);
            TakesInput.DisableInputs(_spectatorModeInputs, this);
            ToggleCursor.ShowCursor();
            _freeFloatCanvas.enabled = false;
            _pickupCanvas.enabled = false;
            _selectCanvas.enabled = true;
            _spectatorModeCanvas.enabled = false;
        }

        void SpectatorMode()
        {
            _controller = Controller.SpectatorMode;
            TakesInput.DisableInputs(_freeFloatInputs, this);
            TakesInput.DisableInputs(_pickupInputs, this);
            TakesInput.DisableInputs(_selectInputs, this);
            TakesInput.EnableInputs(_spectatorModeInputs, this);
            ToggleCursor.ShowCursor();
            _freeFloatCanvas.enabled = false;
            _pickupCanvas.enabled = false;
            _selectCanvas.enabled = false;
            _spectatorModeCanvas.enabled = true;
        }
    }
}
