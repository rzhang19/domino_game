using System.Collections;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// Provides first-person camera control through player mouse movements.
    /// </summary>
    /// <seealso cref="BH.TakesInput" />
    public class FirstPersonCamera : TakesInput
    {
        // Input state
        float _xRot;
        float _yRot;

        // Inconstant member variables
        [SerializeField] Transform _playerTransform; // Transform used to rotate around y-axis
        CameraControl _camCtrl = CameraControl.lockNone;
        enum CameraControl
        {
            lockNone,
            lockXY,
            lockX,
            lockY
        }

        float _oldXRot;
        float _oldYRot;
        bool _cachedOldRots = false;
        public bool _isBeingControlled = false;

        // Constant member variables
        [SerializeField] Camera _cam; // Camera used to change FOV
        [SerializeField] float _minimumX = -90f;
        [SerializeField] float _maximumX = 90f;
        [SerializeField] float _sensitivity = 1f;
        
        void Awake()
        {
            if (!_playerTransform)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player)
                    _playerTransform = player.transform;
                else
                    Debug.LogError("Player transform is not initialized.");
            }
            else
            {
                _xRot = transform.eulerAngles.x;
                _yRot = _playerTransform.localRotation.eulerAngles.y;
            }

            if (!_cam)
            {
                _cam = GetComponent<Camera>();
                if (!_cam)
                Debug.LogError("Camera is not initialized.");
            }
        }
        
        void GetInput()
        {
            if (_locks.Count > 0)
                return;

            // Retrieve mouse input
            float deltaYRot = (_camCtrl == CameraControl.lockXY || _camCtrl == CameraControl.lockX) ? 0f: Input.GetAxis("Mouse Y");
            float deltaXRot = (_camCtrl == CameraControl.lockXY || _camCtrl == CameraControl.lockY) ? 0f: Input.GetAxis("Mouse X");

            if (deltaXRot != 0f || deltaYRot != 0f)
            {
                //StopAutomatedRotation();
                _xRot -= deltaYRot * _sensitivity;
                _yRot += deltaXRot * _sensitivity;
            }
        }

        void Update()
        {
            //if (_camCtrl != CameraControl.scriptControlledStrong)
            //{
            //    GetInput();
            //    SetRotation();
            //}
            //// else there exists a running coroutine that is setting the rotation during this frame.
            
            GetInput();
            SetRotation();
        }

        // Sets rotation based on current values of xRot and yRot.
        void SetRotation()
        {
            // Keep values within 180 of 0
            _xRot = Normalize(_xRot);
            _yRot = Normalize(_yRot);

            // Bound x-axis camera rotation
            _xRot = Mathf.Clamp(_xRot, _minimumX, _maximumX);

            // Set player rotation around the y-axis, camera rotation around the x-axis
            _playerTransform.localRotation = Quaternion.Euler(0f, _yRot, 0f);
            transform.localRotation = Quaternion.Euler(_xRot, 0f, transform.localRotation.eulerAngles.z);
        }

        public void LookRegular()
        {
            if (!_cachedOldRots)
                return;

            _cachedOldRots = false;
            
            StartCoroutine(AsyncRotateTowards(_oldXRot, _oldYRot, CameraControl.lockNone, 0.2f));
        }

        public void LookDown()
        {
            _oldXRot = _xRot;
            _oldYRot = _yRot;
            _cachedOldRots = true;
            
            StartCoroutine(AsyncRotateTowards(90f, _yRot, CameraControl.lockX, 0.2f));
        }
    
        IEnumerator AsyncRotateTowards(float xRot, float yRot, CameraControl cameraControl, float time, AnimationCurve motionCurve = null)
        {
            if (motionCurve == null)
                motionCurve = AnimationCurve.Linear(0, 0, 1, 1);
            
            _camCtrl = CameraControl.lockXY;
            yield return StartCoroutine(AsyncRotateTowardsHelper(xRot, yRot, time, motionCurve));
            _camCtrl = cameraControl;
        }

        IEnumerator AsyncRotateTowardsHelper(float xRot, float yRot, float time, AnimationCurve motionCurve = null)
        {
            _isBeingControlled = true;
            if (motionCurve == null)
                motionCurve = AnimationCurve.Linear(0, 0, 1, 1);

            float speed = 1 / time;

            float interpVal = 0f;
            float startTime = Time.time;
            float startXRot = Normalize(transform.localRotation.eulerAngles.x);
            float startYRot = Normalize(_playerTransform.localRotation.eulerAngles.y);

            while (interpVal < 1f)
            {
                yield return null;
                
                float endXRot = Normalize(xRot);
                float endYRot = Normalize(yRot);

                // Forces the interpolation to take the shortest path.
                FixRotation(out endXRot, out startXRot, endXRot, startXRot);
                FixRotation(out endYRot, out startYRot, endYRot, startYRot);

                interpVal = motionCurve.Evaluate((Time.time - startTime) * speed);
                _xRot = Mathf.Lerp(startXRot, endXRot, interpVal);
                SetRotation();
            }
            
            _isBeingControlled = false;
        }

        //public void SetRotation(float newXRot, float newYRot)
        //{
        //    _xRot = newXRot;
        //    _yRot = newYRot;
        //}

        //// Interpolates the rotation values to look at a transform "tf" over "time" seconds.
        //// Note: To call this (and similar coroutines), we must use "StartCoroutine".
        //// Example usage: "StartCoroutine(LookAt(thingToLookAt.transform, 5f, AnimationCurve.EaseInOut(0, 0, 1, 1)))" would rotate the
        ////      camera to look at the game object "thingToLookAt", taking 5 seconds to do so.
        //public IEnumerator LookAt(Transform tf, float time, AnimationCurve motionCurve = null)
        //{
        //    if (motionCurve == null)
        //        motionCurve = AnimationCurve.Linear(0, 0, 1, 1);

        //    if (_camCtrl == CameraControl.playerControlled)
        //    {
        //        _camCtrl = CameraControl.scriptControlledStrong;
        //        yield return StartCoroutine(LookAtCoro(tf, time, motionCurve));
        //        _camCtrl = CameraControl.playerControlled;
        //    }
        //}

        //// Sets the rotation values to track a transform "tf" for "time" seconds.
        //public IEnumerator Track(Transform tf, float time)
        //{
        //    if (_camCtrl == CameraControl.playerControlled)
        //    {
        //        _camCtrl = CameraControl.scriptControlledStrong;
        //        yield return StartCoroutine(TrackCoro(tf, time));
        //        _camCtrl = CameraControl.playerControlled;
        //    }
        //}

        //// Should be self-explanatory after looking at the comments of LookAt() and Track().
        //public IEnumerator LookAtAndTrack(Transform tf, float timeToLookAt, float timeToTrack, AnimationCurve motionCurve = null)
        //{
        //    if (motionCurve == null)
        //        motionCurve = AnimationCurve.Linear(0, 0, 1, 1);

        //    if (_camCtrl == CameraControl.playerControlled)
        //    {
        //        _camCtrl = CameraControl.scriptControlledStrong;
        //        yield return StartCoroutine(LookAtCoro(tf, timeToLookAt, motionCurve));
        //        yield return StartCoroutine(TrackCoro(tf, timeToTrack));
        //        _camCtrl = CameraControl.playerControlled;
        //    }
        //}

        //// Preemptable LookAt.
        //public IEnumerator LookAtWeak(Transform tf, float time, AnimationCurve motionCurve = null)
        //{
        //    if (motionCurve == null)
        //        motionCurve = AnimationCurve.Linear(0, 0, 1, 1);

        //    if (_camCtrl == CameraControl.playerControlled)
        //    {
        //        _camCtrl = CameraControl.scriptControlledWeak;
        //        yield return StartCoroutine(LookAtCoro(tf, time, motionCurve));
        //        _camCtrl = CameraControl.playerControlled;
        //    }
        //}

        ////----------------------------------------------------------------------------------//
        ////  For the sake of abstraction, it's preferable to call the above coroutines       //
        ////  through StartCoroutine rather than the lower-level detailed coroutines below.   //
        ////  If the above ones don't satisfy your needs, you can create a new one by         //
        ////  concatenating the coroutines below (as is done in the above coroutines), or     //
        ////  you can just tell me what you need! --Brandon                                   //
        ////----------------------------------------------------------------------------------//

        //// Coroutine for smoothly interpolating toward facing a given transform's position.
        //// Note: Transforms are pass-by-reference, so this allows for dynamic positions.
        //IEnumerator LookAtCoro(Transform tf, float time, AnimationCurve motionCurve = null)
        //{
        //    if (motionCurve == null)
        //        motionCurve = AnimationCurve.Linear(0, 0, 1, 1);

        //    float speed = 1 / time;

        //    float interpVal = 0f;
        //    float startTime = Time.time;
        //    float startXRot = Normalize(transform.localRotation.eulerAngles.x);
        //    float startYRot = Normalize(_playerTransform.localRotation.eulerAngles.y);

        //    while (interpVal < 1f)
        //    {
        //        yield return null;

        //        Vector3 relativePos = tf.position - transform.position;
        //        Quaternion quat = Quaternion.LookRotation(relativePos);
        //        float endXRot = Normalize(quat.eulerAngles.x);
        //        float endYRot = Normalize(quat.eulerAngles.y);

        //        // Forces the interpolation to take the shortest path.
        //        FixRotation(out endXRot, out startXRot, endXRot, startXRot);
        //        FixRotation(out endYRot, out startYRot, endYRot, startYRot);

        //        interpVal = motionCurve.Evaluate((Time.time - startTime) * speed);
        //        _xRot = Mathf.Lerp(startXRot, endXRot, interpVal);
        //        _yRot = Mathf.Lerp(startYRot, endYRot, interpVal);
        //        SetRotation();
        //    }
        //}

        //// Coroutine for tracking a given transform's position for a certain amount of time.
        //IEnumerator TrackCoro(Transform tf, float time)
        //{
        //    float startTime = Time.time;

        //    if (time > 0f)
        //    {
        //        while (Time.time - startTime < time)
        //        {
        //            yield return null;
        //            Vector3 relativePos = tf.position - transform.position;
        //            Quaternion quat = Quaternion.LookRotation(relativePos);
        //            _xRot = Normalize(quat.eulerAngles.x);
        //            _yRot = Normalize(quat.eulerAngles.y);
        //            SetRotation();
        //        }
        //    }
        //    else
        //    {
        //        while (true)
        //        {
        //            yield return null;
        //            Vector3 relativePos = tf.position - transform.position;
        //            Quaternion quat = Quaternion.LookRotation(relativePos);
        //            _xRot = Normalize(quat.eulerAngles.x);
        //            _yRot = Normalize(quat.eulerAngles.y);
        //            SetRotation();
        //        }
        //    }
        //}

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

        // Checks if the start and end values differ in sign.
        // If it would be a shorter path, change the negative value to its positive equivalent.
        void FixRotation(out float newEndRot, out float newStartRot, float endRot, float startRot)
        {
            newEndRot = endRot;
            newStartRot = startRot;

            if (endRot < 0f && startRot > 0f)
            {
                if (Mathf.Abs(endRot - startRot) > Mathf.Abs(startRot - (endRot + 360f)))
                    newEndRot = endRot + 360f;
            }
            else if (startRot < 0f && endRot > 0f)
            {
                if (Mathf.Abs(endRot - startRot) > Mathf.Abs((startRot + 360f) - endRot))
                    newStartRot = startRot + 360f;
            }
        }

        //// Stops all coroutines running on this script, i.e. stops the script's control of the camera.
        //public void StopAutomatedRotation()
        //{
        //    if (_camCtrl == CameraControl.playerControlled)
        //        return;

        //    StopAllCoroutines();
        //    _camCtrl = CameraControl.playerControlled;
        //}

        public void SetSensitivity(float sens)
        {
            _sensitivity = sens;
        }

        //public void SetFOV(float fov)
        //{
        //    _cam.fieldOfView = fov;
        //}
    }
}
