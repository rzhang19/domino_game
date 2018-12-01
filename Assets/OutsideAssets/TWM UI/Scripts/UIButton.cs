using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TWM.UI
{
    public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler, IPointerEnterHandler
    {
        [SerializeField] UnityEvent _onButtonDown;
        [SerializeField] UnityEvent _onButtonUp;
        [SerializeField] UnityEvent _onButtonEnter;
        [SerializeField] UnityEvent _onButtonExit;

        bool _isPressed = false;

        bool _enabled = true;

        UIElementAnimator _imageAnimator;

        void Awake()
        {
            _imageAnimator = GetComponentInChildren<UIElementAnimator>();
        }

        void OnDisable()
        {
            if (_isPressed)
                Unpress();
        }

        public void OnPointerEnter(PointerEventData data)
        {
            if (!_enabled)
                return;

            if (!_isPressed)
            {
                Press(data.pointerId);

                if (_onButtonEnter != null)
                    _onButtonEnter.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (!_enabled)
                return;
            
            if (_onButtonDown != null)
                _onButtonDown.Invoke();
        }

        public void OnPointerExit(PointerEventData data)
        {
            if (_isPressed)
            {
                Unpress();
                if (_onButtonExit != null)
                    _onButtonExit.Invoke();
            }
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (_isPressed)
            {
                //Unpress();
                if (_onButtonUp != null)
                    _onButtonUp.Invoke();
            }
        }

        void Press(int pointerId)
        {
            _isPressed = true;
            if (_imageAnimator)
                _imageAnimator.PushOut();
        }

        void Unpress()
        {
            _isPressed = false;
            if (_imageAnimator)
                _imageAnimator.PushIn();
        }

        public void Enable()
        {
            _enabled = true;
        }

        public void Disable()
        {
            if (_isPressed)
                Unpress();

            _enabled = false;
        }

        public void Toggle()
        {
            if (_enabled)
                Disable();
            else
                Enable();
        }
    }
}
