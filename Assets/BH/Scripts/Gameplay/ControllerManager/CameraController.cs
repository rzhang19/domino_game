using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    [RequireComponent(typeof(FirstPersonCamera))]
    public class CameraController : TakesInput
    {
        // Input state
        bool _toggleProjectionDown = false;

        [SerializeField] Camera _camera;
        
        bool _orthographic = false;
        Matrix4x4 _orthoMatrix, _perspectiveMatrix;
        [SerializeField] float _orthoToPerspectiveDuration = 1f;
        [SerializeField] float _perspectiveToOrthoDuration = 1f;
        [SerializeField] AnimationCurve _orthoToPerspectiveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] AnimationCurve _perspectiveToOrthoCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        FirstPersonCamera _firstPersonCamera;

        void Awake()
        {
            if (!_camera)
            {
                _camera = GetComponent<Camera>();
                if (!_camera)
                    Debug.LogError("Camera is not initialized.");
            }

            _orthoMatrix = Matrix4x4.Ortho(-_camera.orthographicSize * _camera.aspect, _camera.orthographicSize * _camera.aspect, -_camera.orthographicSize, _camera.orthographicSize, _camera.nearClipPlane, _camera.farClipPlane);
            _perspectiveMatrix = Matrix4x4.Perspective(_camera.fieldOfView, _camera.aspect, _camera.nearClipPlane, _camera.farClipPlane);

            _orthographic = _camera.orthographic;

            _firstPersonCamera = GetComponent<FirstPersonCamera>();
        }

        void GetInput()
        {
            if (_locks.Count > 0)
                _toggleProjectionDown = false;
            else
                _toggleProjectionDown = InputManager.GetKeyDown("Toggle Projection");
        }

        void Update()
        {
            GetInput();

            if (_toggleProjectionDown)
                ToggleProjection();
        }

        public void ToggleProjection()
        {
            if (_orthographic)
                SetProjectionToPerspective();
            else
                SetProjectionToOrthographic();
        }

        public void SetProjectionToOrthographic()
        {
            if (_orthographic)
                return;

            _orthographic = true;
            BlendToMatrix(_orthoMatrix, _perspectiveToOrthoDuration, _perspectiveToOrthoCurve);

            _firstPersonCamera.LookDown();
        }

        public void SetProjectionToPerspective()
        {
            if (!_orthographic)
                return;

            _orthographic = false;
            BlendToMatrix(_perspectiveMatrix, _orthoToPerspectiveDuration, _orthoToPerspectiveCurve);
        }

        // Source: https://forum.unity.com/threads/smooth-transition-between-perspective-and-orthographic-modes.32765/
        Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float time)
        {
            Matrix4x4 ret = new Matrix4x4();
            for (int i = 0; i < 16; i++)
                ret[i] = Mathf.Lerp(from[i], to[i], time);
            return ret;
        }

        IEnumerator AsyncLerpFromTo(Matrix4x4 src, Matrix4x4 dest, float duration, AnimationCurve animationCurve)
        {
            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                _camera.projectionMatrix = MatrixLerp(src, dest, animationCurve.Evaluate((Time.time - startTime) / duration));
                yield return 1;
            }
            _camera.projectionMatrix = dest;
        }

        Coroutine BlendToMatrix(Matrix4x4 targetMatrix, float duration, AnimationCurve animationCurve)
        {
            StopAllCoroutines();
            return StartCoroutine(AsyncLerpFromTo(_camera.projectionMatrix, targetMatrix, duration, animationCurve));
        }
    }
}
