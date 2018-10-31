using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;

namespace BH
{
    public class ModeManager : Singleton<ModeManager>
    {
        [SerializeField] TakesInput[] _freeModeInputs;
        [SerializeField] TakesInput[] _buildModeInputs;
        [SerializeField] Canvas _buildCanvas;

        enum Mode
        {
            Build,
            Free
        }
        Mode mode = Mode.Build;

        void Awake()
        {
            if (_freeModeInputs.Length <= 0)
                Debug.LogError("Player controller is not initialized.");

            if (!_buildCanvas)
                Debug.LogError("Build canvas is not initialized.");
        }

        void Start()
        {
            BuildModeOn();
        }

        void Update()
        {
            if (InputManager.GetPauseKeyDown())
            {
                switch (mode)
                {
                    case Mode.Build:
                        FreeModeOn();
                        break;
                    case Mode.Free:
                        BuildModeOn();
                        break;
                    default:
                        break;
                }
            }
        }

        void FreeModeOn()
        {
            mode = Mode.Free;
            TakesInput.EnableInputs(_freeModeInputs, this);
            TakesInput.DisableInputs(_buildModeInputs, this);
            ToggleCursor.HideCursor();
            _buildCanvas.enabled = false;
        }

        void BuildModeOn()
        {
            mode = Mode.Build;
            TakesInput.DisableInputs(_freeModeInputs, this);
            TakesInput.EnableInputs(_buildModeInputs, this);
            ToggleCursor.ShowCursor();
            _buildCanvas.enabled = true;
        }
    }
}
