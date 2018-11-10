using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    [RequireComponent(typeof(LineRenderer))]
    public class SpectatorModeController : TakesInput
    {
        // Input state
        bool _clickDown = false;
        bool _clickUp = false;

        Camera _cam;
        float _distance = float.MaxValue;
        public LayerMask _interactableMask;
        
        bool _waitingForRelease = false;
        Rigidbody _holding = null;
        Vector3 _holdingOrigin;

        LineRenderer _lineRenderer;

        public AnimationCurve _velocityCurve;
        public float _minVelocityDistance = 0.1f;
        public float _maxVelocityDistance = 10f;
        public float _maxVelocity = 10f;

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

            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = 0;
        }

        void Update()
        {
            GetInput();
            
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (_waitingForRelease)
            {
                Plane plane = new Plane(Vector3.up, _holdingOrigin);
                float enter = 0f;

                // hitPoint may not be set correctly if the ray is perfectly
                // parallel to the ground. Hopefully this won't be an actual issue!
                Vector3 hitPoint = Vector3.zero;
                if (plane.Raycast(ray, out enter))
                    hitPoint = ray.GetPoint(enter);

                if (_clickUp)
                {
                    Debug.Log("Released " + _holding.name);

                    Vector3 diff = hitPoint - _holdingOrigin;
                    _holding.velocity = diff.normalized * _velocityCurve.Evaluate((diff.magnitude - _minVelocityDistance) / _maxVelocityDistance) * _maxVelocity;

                    _waitingForRelease = false;
                    _holding = null;
                    _holdingOrigin = Vector3.zero;
                    _lineRenderer.positionCount = 0;
                }
                else
                {
                    _lineRenderer.SetPosition(1, hitPoint);
                }
            }

            if (Physics.Raycast(ray, out hitInfo, _distance, _interactableMask))
            {
                Interactable i = hitInfo.collider.GetComponentInChildren<Interactable>();
                if (_clickDown && !_waitingForRelease && i && i._canBePushed)
                {
                    _waitingForRelease = true;
                    _holding = hitInfo.collider.GetComponent<Rigidbody>();
                    if (_holding)
                    {
                        _holdingOrigin = hitInfo.point;
                        _lineRenderer.positionCount = 2;
                        _lineRenderer.SetPosition(0, hitInfo.point);
                        _lineRenderer.SetPosition(1, hitInfo.point);
                    }
                    else
                        _waitingForRelease = false;

                    Debug.Log("Holding " + hitInfo.collider.name);
                }
            }
        }
    }
}
