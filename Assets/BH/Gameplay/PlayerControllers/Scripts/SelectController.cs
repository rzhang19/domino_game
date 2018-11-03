using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    public class SelectController : TakesInput
    {
        // Input state
        bool _clickDown = false;
        bool _clickUp = false;

        Camera _cam;
        float _distance = float.MaxValue;
        public LayerMask _interactableMask;
        
        List<ISelectable> _selected = new List<ISelectable>();

        void GetInput()
        {
            _clickDown = false;
            _clickUp = true;

            if (_locks.Count > 0)
                return;

            _clickDown = InputManager.GetKeyDown("Attack1");
            _clickUp = InputManager.GetKeyUp("Attack1");
        }

        void Awake()
        {
            if (!_cam)
            {
                _cam = Camera.main;
                if (!_cam)
                    Debug.LogError("Camera is not initialized.");
            }
        }

        void Update()
        {
            GetInput();
            
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, _distance, _interactableMask))
            {
                if (_clickDown)
                {
                    ISelectable selectable = hitInfo.collider.GetComponentInChildren<ISelectable>();
                    if (_selected != null)
                    {
                        if (selectable.IsSelected())
                        {
                            _selected.Remove(selectable);
                            selectable.Deselect();
                        }
                        else
                        {
                            _selected.Add(selectable);
                            selectable.Select();
                        }
                    }
                }
            }
        }

        public void Delete()
        {
            foreach (ISelectable selectable in _selected)
            {
                selectable.Delete();
            }

            _selected.RemoveAll(selectable => true);
        }
    }
}
