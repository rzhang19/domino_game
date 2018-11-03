using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    public class PickupController : TakesInput
    {
        // Input state
        bool _clickDown = false;
        bool _clickUp = false;

        Camera _cam;
        float _distance = float.MaxValue;
        public LayerMask _interactableMask;

        public Vector3 _pickUpOffset = Vector3.up;
        bool _waitingForRelease = false;
        Vector3 _offset;
        ISelectable _selected = null;
        Rigidbody _pickedUp = null;
        ClosestColliderBelow _closestColliderBelow = null;
        public AnimationCurve _velocityCurve;
        public float _maxVelocityDistance = 10f;
        public float _maxVelocity = 50f;
        public float _bufferDistance = 2f;

        public LayerMask _interactSurfaceMask;

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
            
            if (_waitingForRelease && _clickUp)
            {
                _waitingForRelease = false;
                if (_pickedUp.velocity.y > 0)
                    _pickedUp.velocity = new Vector3(_pickedUp.velocity.x, 0f, _pickedUp.velocity.z);
                _pickedUp.useGravity = true;
                _pickedUp.freezeRotation = false;
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
            if (Physics.Raycast(ray, out hitInfo, _distance, _interactSurfaceMask))
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

            if (Physics.Raycast(ray, out hitInfo, _distance, _interactableMask))
            {
                Interactable i = hitInfo.collider.GetComponentInChildren<Interactable>();
                if (_clickDown && !_waitingForRelease && i && i._canBePickedUp)
                {
                    _waitingForRelease = true;
                    _pickedUp = hitInfo.collider.GetComponent<Rigidbody>();
                    if (_pickedUp)
                    {
                        _pickedUp.useGravity = false;
                        _pickedUp.freezeRotation = true;
                    }
                    else
                        _waitingForRelease = false;
                    _offset = _pickedUp.position - offsetBase + _pickUpOffset;

                    _closestColliderBelow = hitInfo.collider.GetComponent<ClosestColliderBelow>();
                    if (_closestColliderBelow)
                        _closestColliderBelow.enabled = true;

                    Debug.Log("Picked up " + hitInfo.collider.name + ". With offset " + _offset);
                }
            }
        }
    }
}
