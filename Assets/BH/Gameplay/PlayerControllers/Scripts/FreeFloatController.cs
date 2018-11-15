using UnityEngine;

namespace BH
{
    /// <summary>
    /// The current free-fly controller.
    /// This class reads and responds to player inputs.
    /// Most notably, this class reads WASD inputs and moves the player appropriately.
    /// </summary>
    /// <seealso cref="BH.TakesInput" />
    [RequireComponent(typeof(CharacterController))]
    public class FreeFloatController : TakesInput
    {
        // Input state
        Vector2 _horizontalInputVec;    // Horizontal movement input
        float _verticalInput;           // Vertical movement input

        // Inconstant member variables
        Vector3 _moveVec;   // Vector3 used to move the character controller
        float _moveSpeed;
        float _friction;
        float _verticalMoveSpeed;

        // Constant member variables
        CharacterController _charController;
        [SerializeField] float _normalFriction = 5f;
        [SerializeField] float _normalSpeed = 5f;
        [SerializeField] float _controlRatio = 0.1f;
        [SerializeField] float _normalVerticalSpeed = 2f;

        void GetInput()
        {
            _horizontalInputVec = Vector2.zero;
            _verticalInput = 0f;

            if (_locks.Count > 0)
                return;

            float horizontalUp = InputManager.GetKey("Strafe Up") ? 1f : 0f,
            horizontalLeft = InputManager.GetKey("Strafe Left") ? -1f : 0f,
            horizontalDown = InputManager.GetKey("Strafe Down") ? -1f : 0f,
            horizontalRight = InputManager.GetKey("Strafe Right") ? 1f : 0f;

            float verticalUp = InputManager.GetKey("Float") ? 1f : 0f,
            verticalDown = InputManager.GetKey("Sink") ? -1f : 0f;

            _horizontalInputVec = new Vector2(horizontalLeft + horizontalRight, horizontalUp + horizontalDown);
            _verticalInput = verticalUp + verticalDown;
        }

        void Awake()
        {
            _charController = GetComponent<CharacterController>();
            _horizontalInputVec = Vector2.zero;
            _verticalInput = 0f;
            _moveVec = Vector3.zero;
            _friction = _normalFriction;
            _moveSpeed = _normalSpeed;
            _verticalMoveSpeed = _normalVerticalSpeed;
        }

        void Update()
        {
            GetInput();
            
            // Normalizing horizontal input movement vector
            if (_horizontalInputVec.magnitude > 1)
                _horizontalInputVec = _horizontalInputVec.normalized;

            // Normalizing vertical input
            if (_verticalInput > 0)
                _verticalInput = 1;
            else if (_verticalInput < 0)
                _verticalInput = -1;
            
            Move();

            _charController.Move(_moveVec * Time.deltaTime);
        }

        void Move()
        {
            Vector3 wishVel = transform.forward * _horizontalInputVec.y + transform.right * _horizontalInputVec.x;
            Vector3 wishDir = wishVel.normalized;
            Vector3 prevMove = new Vector3(_moveVec.x, 0f, _moveVec.z);

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

            // Vertical movement
            nextMove.y = _verticalInput * _verticalMoveSpeed;

            _moveVec = nextMove;
        }

        // Called during CharacterController.Move()
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // If this frame's attempted move was blocked by a collider, project the movement vector onto the collider's surface
            Vector3 surfaceNormal = hit.normal;
            _moveVec = Vector3.ProjectOnPlane(_moveVec, surfaceNormal);
        }
    }
}
