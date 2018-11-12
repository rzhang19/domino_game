using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// Manages the controls in build mode.
    /// This class reads and responds to player inputs.
    /// Most notably, this class provides the logic for game object selection and pickup.
    /// </summary>
    /// <seealso cref="BH.TakesInput" />
    public class BuildModeController : TakesInput
    {
        // Input state
        bool _selectDown = false;
        bool _selectUp = false;
        bool _pickupDown = false;
        bool _pickupUp = false;
        float _scrollWheel = 0f;

        Camera _cam;
        float _distance = float.MaxValue;
        [SerializeField] LayerMask _selectableMask;
        
        // For selection functionality
        List<Selectable> _selected = new List<Selectable>();
        List<Transform> _selectedTransforms = new List<Transform>();

        // For pickup functionality
        [SerializeField] Vector3 _pickUpOffset = Vector3.up;
        bool _waitingForRelease = false;
        Vector3 _offset;
        Rigidbody _pickedUp = null;
        ClosestColliderBelow _closestColliderBelow = null;
        [SerializeField] AnimationCurve _velocityCurve;
        [SerializeField] float _maxVelocityDistance = 10f;
        [SerializeField] float _maxVelocity = 50f;
        [SerializeField] float _bufferDistance = 2f;

        [SerializeField] LayerMask _selectableSurfaceMask;

        void GetInput()
        {
            if (_locks.Count > 0)
            {
                _selectDown = false;
                _selectUp = true;
                _pickupDown = false;
                _pickupUp = true;
                _scrollWheel = 0f;

                // Unselect everything upon input lock.
                foreach (Selectable selectable in _selected)
                    selectable.Deselect();
                _selected.RemoveAll(selected => true);
                _selectedTransforms.RemoveAll(selected => true);

                return;
            }

            _selectDown = InputManager.GetKeyDown("Attack2");
            _selectUp = InputManager.GetKeyUp("Attack2");
            _pickupDown = InputManager.GetKeyDown("Attack1");
            _pickupUp = InputManager.GetKeyUp("Attack1");
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

            // Pickup release
            if (_waitingForRelease && _pickupUp)
            {
                _waitingForRelease = false;
                if (_pickedUp.velocity.y > 0)
                    _pickedUp.velocity = new Vector3(_pickedUp.velocity.x, 0f, _pickedUp.velocity.z);
                _pickedUp.useGravity = true;
                _pickedUp = null;
                _offset = Vector3.zero;

                if (_closestColliderBelow)
                {
                    _closestColliderBelow.enabled = false;
                    _closestColliderBelow = null;
                }
            }
            
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            Vector3 offsetBase = Vector3.zero;
            if (Physics.Raycast(ray, out hitInfo, _distance, _selectableSurfaceMask))
            {
                offsetBase = hitInfo.point;
                if (_pickedUp)
                {
                    float closestColliderY = float.MinValue;
                    if (_closestColliderBelow && _closestColliderBelow._closestTransform)
                        closestColliderY = _closestColliderBelow._closestTransform.position.y;

                    Vector3 desiredPos = offsetBase + _offset;
                    desiredPos.y = Mathf.Max(desiredPos.y, closestColliderY + _bufferDistance);
                    Vector3 diff = desiredPos - _pickedUp.position;
                    _pickedUp.velocity = diff.normalized * _velocityCurve.Evaluate(diff.magnitude / _maxVelocityDistance) * _maxVelocity;
                }
            }

            if (Physics.Raycast(ray, out hitInfo, _distance, _selectableMask))
            {
                Selectable sel = hitInfo.collider.GetComponentInChildren<Selectable>();
                if (sel)
                {
                    // Pickup
                    if (_pickupDown && !_waitingForRelease && sel._canBePickedUp)
                    {
                        _waitingForRelease = true;
                        _pickedUp = hitInfo.collider.GetComponent<Rigidbody>();
                        if (_pickedUp)
                            _pickedUp.useGravity = false;
                        else
                            _waitingForRelease = false;
                        _offset = _pickedUp.position - offsetBase + _pickUpOffset;

                        _closestColliderBelow = hitInfo.collider.GetComponent<ClosestColliderBelow>();
                        if (_closestColliderBelow)
                            _closestColliderBelow.enabled = true;

                        //Debug.Log("Picked up " + hitInfo.collider.name + ". With offset " + _offset);
                    }

                    // Select
                    if (_selectDown)
                    {
                        Selectable selectable = hitInfo.collider.GetComponentInChildren<Selectable>();
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

            if (_scrollWheel != 0f)
                RotateSelected(_scrollWheel * 10f);
        }

        /// <summary>
        /// Despawns the selected game objects.
        /// </summary>
        public void DespawnSelected()
        {
            foreach (Selectable selectable in _selected)
            {
                SelectableManager.Instance.DespawnSelectable(selectable);
            }

            _selected.RemoveAll(selected => true);
            _selectedTransforms.RemoveAll(selected => true);
        }
        
        /// <summary>
        /// Rotates the selected game objects at a constant speed.
        /// </summary>
        public void RotateSelected()
        {
            Vector3 center = FindCenter(_selectedTransforms.ToArray());

            foreach (Selectable selectable in _selected)
            {
                selectable.Rotate(center, Vector3.up, 30f * Time.deltaTime);
            }
        }

        /// <summary>
        /// Rotates the selected game objects a specified amount.
        /// </summary>
        /// <param name="deg">The rotation in degrees.</param>
        public void RotateSelected(float deg)
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
