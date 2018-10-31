using UnityEngine;

namespace BH
{
    [RequireComponent(typeof(CharacterController))]
    public class FreeFlyController : TakesInput
    {
        // Input state
        Vector2 _inputVec;  // Horizontal movement input
        float _xRot;
        float _yRot;

        // Inconstant member variables
        Vector3 _moveVec;   // Vector3 used to move the character controller
        float _moveSpeed;
        float _friction;

        // Constant member variables
        CharacterController _charController;
        [SerializeField] float _normalFriction = 5f;
        [SerializeField] float _normalSpeed = 5f;
        [SerializeField] float _controlRatio = 0.1f;
        [SerializeField] float _minimumX = -89f;
        [SerializeField] float _maximumX = 89f;
        [SerializeField] float _sensitivity = 1f;

        void GetInput()
        {
            _inputVec = Vector2.zero;

            if (_locks.Count > 0)
                return;

            float up = InputManager.GetKey("Strafe Up") ? 1 : 0,
            left = InputManager.GetKey("Strafe Left") ? -1 : 0,
            down = InputManager.GetKey("Strafe Down") ? -1 : 0,
            right = InputManager.GetKey("Strafe Right") ? 1 : 0;

            _inputVec = new Vector2(left + right, up + down);

            // Retrieve mouse input
            _xRot -= Input.GetAxis("Mouse Y") * _sensitivity;
            _yRot += Input.GetAxis("Mouse X") * _sensitivity;
        }

        void Awake()
        {
            _charController = GetComponent<CharacterController>();
            _inputVec = Vector2.zero;
            _moveVec = Vector3.zero;
            _friction = _normalFriction;
            _moveSpeed = _normalSpeed;
            _xRot = transform.localRotation.eulerAngles.x;
            _yRot = transform.localRotation.eulerAngles.y;
        }

        void Update()
        {
            GetInput();
            
            // Normalizing input movement vector
            if (_inputVec.magnitude > 1)
                _inputVec = _inputVec.normalized;
            
            Move();

            _charController.Move(_moveVec * Time.deltaTime);
            
            SetRotation();
        }

        void Move()
        {
            Vector3 wishVel = transform.forward * _inputVec.y + transform.right * _inputVec.x;
            Vector3 wishDir = wishVel.normalized;
            Vector3 prevMove = _moveVec;

            // Calculate movement vector to add
            Vector3 addMove;
            if (_controlRatio * prevMove.magnitude > 1f)
                addMove = wishDir * _controlRatio * prevMove.magnitude;
            else
                addMove = wishDir * _controlRatio * _moveSpeed;

            // Apply friction to previous move
            float prevSpeed = prevMove.magnitude;
            if (prevSpeed != 0) // To avoid divide by zero errors
            {
                float drop = prevSpeed * _friction * Time.deltaTime;
                float newSpeed = prevSpeed - drop;
                if (newSpeed < 0)
                    newSpeed = 0;
                else if (newSpeed != prevSpeed)
                {
                    newSpeed /= prevSpeed;
                    prevMove = prevMove * newSpeed;
                }
            }

            // The next move is the previous move plus an input-based movement vector
            Vector3 nextMove = prevMove + addMove;
            if (nextMove.magnitude > _moveSpeed)
                nextMove = nextMove.normalized * _moveSpeed;

            _moveVec = nextMove;
        }

        // Called during CharacterController.Move()
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // If this frame's attempted move was blocked by a collider, project the movement vector onto the collider's surface
            Vector3 surfaceNormal = hit.normal;
            _moveVec = Vector3.ProjectOnPlane(_moveVec, surfaceNormal);
        }

        // Sets rotation based on current values of xRot and yRot.
        void SetRotation()
        {
            // Keep values within 180 of 0.
            _xRot = Normalize(_xRot);
            _yRot = Normalize(_yRot);

            // Bound x-axis camera rotation.
            _xRot = Mathf.Clamp(_xRot, _minimumX, _maximumX);

            // Set rotation.
            transform.localRotation = Quaternion.Euler(_xRot, _yRot, transform.localRotation.eulerAngles.z);
        }

        // Returns an equivalent rotation float value in (-180, 180].
        // This is an idempotent function.
        float Normalize(float f)
        {
            if (f >= 180f)
                return f - 360f;
            else if (f < -180f)
                return f + 360f;
            else
                return f;
        }
    }
}
