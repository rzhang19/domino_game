using UnityEngine;

namespace BH
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : TakesInput
    {
        // Input state
        Vector2 _inputVec;                  // Horizontal movement input
        bool _jump;                         // Whether the jump key is inputted
        bool _sprintKeyDown = false;
        bool _sprintKeyUp = false;

        // Inconstant member variables
        Vector3 _moveVec;                   // Vector3 used to move the character controller
        float _moveSpeed;
        float _friction;
        bool _isJumping = false;            // Player has jumped and not been grounded yet
        bool _groundedLastFrame = false;    // Player was grounded during the last frame
        bool _isSprinting = false;

        // Constant member variables
        CharacterController _charController;

        [SerializeField] float _gravityMultiplier = 1f;
        [SerializeField] float _stickToGroundForce = 3f;
        [SerializeField] float _walkFriction = 5f;

        public bool _canJump = false;
        [SerializeField] float _jumpSpeed = 5f; // Initial upwards speed of the jump

        [SerializeField] float _walkSpeed = 5f;
        [SerializeField] float _minAirSpeed = 3f;

        [SerializeField] bool _canSprint = false;
        [SerializeField] float _sprintSpeed = 7f;

        [SerializeField] float _airControlRatio = 0.02f;
        [SerializeField] float _groundControlRatio = 0.1f;

        [SerializeField] float _surfAngleThreshold = 20f;

        void GetInput()
        {
            _inputVec = Vector2.zero;
            _sprintKeyUp = true;

            if (_locks.Count > 0)
                return;

            float up = InputManager.GetKey("Strafe Up") ? 1 : 0,
            left = InputManager.GetKey("Strafe Left") ? -1 : 0,
            down = InputManager.GetKey("Strafe Down") ? -1 : 0,
            right = InputManager.GetKey("Strafe Right") ? 1 : 0;

            _inputVec = new Vector2(left + right, up + down);

            // Auto-hop
            if (_canJump && !_jump)
                _jump = InputManager.GetKey("Jump");

            //// No auto-hop
            //if (!jump)
            //    jump = InputManager.GetKeyDown("Jump");
            
            // Hold to sprint
            _sprintKeyDown = _canSprint && ((InputManager.GetKeyDown("Sprint") || InputManager.GetKeyDown("Strafe Up"))
                && (InputManager.GetKey("Sprint") && InputManager.GetKey("Strafe Up")));
            _sprintKeyUp = _canSprint && (InputManager.GetKeyUp("Sprint") || InputManager.GetKeyUp("Strafe Up"));

            //// Toggle sprint
            //sprintKeyDown = InputManager.GetKeyDown("Sprint") && InputManager.GetKey("Strafe Up");
            //sprintKeyUp = InputManager.GetKeyDown("Sprint") || InputManager.GetKeyUp("Strafe Up");
        }

        void Awake()
        {
            _charController = GetComponent<CharacterController>();

            _inputVec = Vector2.zero;
            _jump = false;
            _sprintKeyDown = false;
            _sprintKeyDown = false;
            _moveVec = Vector3.zero;
            _friction = _walkFriction;
            _isJumping = false;
            _groundedLastFrame = false;
            _isSprinting = false;
            _moveSpeed = _walkSpeed;
        }

        void Update()
        {
            GetInput();

            if (_sprintKeyDown && !_isSprinting)
            {
                _isSprinting = true;
                _moveSpeed = _sprintSpeed;
            }
            else if (_sprintKeyUp && _isSprinting)
            {
                _isSprinting = false;
                _moveSpeed = _walkSpeed;
            }

            // Jump
            if (!_groundedLastFrame && _charController.isGrounded)
            {
                _moveVec.y = 0f;
                _isJumping = false;
            }

            // This code is not be needed if charController.isGrounded is set during charController.Move().
            // TODO: try to find this piece of info in Unity's documentation
            if (!_charController.isGrounded && !_isJumping && _groundedLastFrame)
                _moveVec.y = 0f;

            // Normalizing input movement vector
            if (_inputVec.magnitude > 1)
                _inputVec = _inputVec.normalized;

            if (_charController.isGrounded)
                GroundMove();
            else
                AirMove();

            _charController.Move(_moveVec * Time.deltaTime);
            _jump = false;
            _groundedLastFrame = _charController.isGrounded;
        }

        void GroundMove()
        {
            Vector3 wishVel = transform.forward * _inputVec.y + transform.right * _inputVec.x;
            Vector3 wishDir = wishVel.normalized;
            Vector3 prevMove = new Vector3(_moveVec.x, 0f, _moveVec.z);

            // Calculate movement vector to add
            Vector3 addMove;
            if (_groundControlRatio * prevMove.magnitude > 1f)
                addMove = wishDir * _groundControlRatio * prevMove.magnitude;
            else
                addMove = wishDir * _groundControlRatio * _moveSpeed;

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

            // y-component calculated separately
            nextMove.y = -_stickToGroundForce;
            if (_jump)
            {
                nextMove.y = _jumpSpeed;
                _jump = false;
                _isJumping = true;
            }

            _moveVec = nextMove;
        }

        void AirMove()
        {
            Vector3 wishVel = transform.forward * _inputVec.y + transform.right * _inputVec.x;
            Vector3 wishDir = wishVel.normalized;
            Vector3 prevMove = new Vector3(_moveVec.x, 0f, _moveVec.z);

            // Calculate movement vector to add
            Vector3 addMove;
            if (_airControlRatio * prevMove.magnitude > _airControlRatio * _minAirSpeed)
                addMove = wishDir * _airControlRatio * prevMove.magnitude;
            else
                addMove = wishDir * _airControlRatio * _minAirSpeed;

            // The next move is the previous move plus an input-based movement vector
            Vector3 nextMove = prevMove + addMove;
            if (nextMove.magnitude > _moveSpeed)
                nextMove = nextMove.normalized * _moveSpeed;

            // y-component calculated separately
            nextMove.y = _moveVec.y;
            nextMove += Physics.gravity * _gravityMultiplier * Time.deltaTime;

            _moveVec = nextMove;
        }

        // Called during CharacterController.Move()
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // If this frame's attempted move was blocked by a collider, project the movement vector onto the collider's surface
            Vector3 surfaceNormal = hit.normal;
            float projY = Vector3.Project(surfaceNormal, Vector3.up).y;
            if (Vector3.Dot(surfaceNormal, _moveVec) < 0 && (projY <= Mathf.Sin(_surfAngleThreshold * Mathf.PI / 180)))
                _moveVec = Vector3.ProjectOnPlane(_moveVec, surfaceNormal);
        }
    }
}
