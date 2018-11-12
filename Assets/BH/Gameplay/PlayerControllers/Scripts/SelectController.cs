using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// DEPRECATED. Use "BuildModeController" instead.
    /// </summary>
    /// <seealso cref="BH.TakesInput" />
    public class SelectController : TakesInput
    {
        // Input state
        bool _clickDown = false;
        bool _clickUp = false;
        bool _rotate = false;
        float _scrollWheel = 0f;

        Camera _cam;
        float _distance = float.MaxValue;
        public LayerMask _interactableMask;
        
        List<Selectable> _selected = new List<Selectable>();
        List<Transform> _selectedTransforms = new List<Transform>();

        void GetInput()
        {
            _clickDown = false;
            _clickUp = true;
            _rotate = false;
            _scrollWheel = 0f;

            if (_locks.Count > 0)
            {
                // Unselect everything upon input lock.
                foreach (Selectable selectable in _selected)
                    selectable.Deselect();
                _selected.RemoveAll(selected => true);
                _selectedTransforms.RemoveAll(selected => true);

                return;
            }

            _clickDown = InputManager.GetKeyDown("Attack1");
            _clickUp = InputManager.GetKeyUp("Attack1");
            _rotate = InputManager.GetKey("Rotate");
            _scrollWheel = Input.GetAxisRaw("Mouse ScrollWheel") * 10f;
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
                    Selectable selectable = hitInfo.collider.GetComponentInChildren<Selectable>();
                    if (_selected != null)
                    {
                        if (selectable.IsSelected())
                        {
                            _selected.Remove(selectable);
                            _selectedTransforms.Remove(selectable.transform);
                            selectable.Deselect();
                        }
                        else
                        {
                            _selected.Add(selectable);
                            _selectedTransforms.Add(selectable.transform);
                            selectable.Select();
                        }
                    }
                }
            }

            //if (_rotate)
            //    Rotate();

            if (_scrollWheel != 0f)
                Rotate(_scrollWheel * 10f);
        }
        
        public void Delete()
        {
            foreach (Selectable selectable in _selected)
            {
                SelectableManager.Instance.DespawnSelectable(selectable);
            }

            _selected.RemoveAll(selected => true);
            _selectedTransforms.RemoveAll(selected => true);
        }

        public void Rotate()
        {
            Vector3 center = FindCenter(_selectedTransforms.ToArray());

            foreach (Selectable selectable in _selected)
            {
                selectable.Rotate(center, Vector3.up, 30f * Time.deltaTime);
            }
        }

        public void Rotate(float deg)
        {
            Vector3 center = FindCenter(_selectedTransforms.ToArray());

            foreach (Selectable selectable in _selected)
            {
                selectable.Rotate(center, Vector3.up, deg);
            }
        }

        Vector3 FindCenter(Transform[] tfs)
        {
            if (tfs.Length <= 0)
                return Vector3.zero;

            if (tfs.Length == 1)
                return tfs[0].position;

            Bounds bounds = new Bounds();
            foreach (Transform tf in tfs)
            {
                bounds.Encapsulate(tf.position);
            }

            return bounds.center;
        }
    }
}
