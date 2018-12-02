using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TWM.UI
{
    public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler, IPointerEnterHandler
    {
        [SerializeField] UnityEvent _onButtonDown;
        [SerializeField] UnityEvent _onButtonUp;
        [SerializeField] UnityEvent _onButtonEnter;
        [SerializeField] UnityEvent _onButtonExit;
        
        [SerializeField] UnityEvent _onToggleOn;
        [SerializeField] UnityEvent _onToggleOff;

        [SerializeField] bool _isToggleable = false;
        bool _toggleState = false;
        [SerializeField] Sprite _toggleOnImage;
        [SerializeField] Sprite _toggleOffImage;
        [SerializeField] Image _toggleImage;

        bool _isPressed = false;
        

        bool _enabled = true;

        UIElementAnimator _imageAnimator;

        void Awake()
        {
            _imageAnimator = GetComponentInChildren<UIElementAnimator>();

            if (_isToggleable)
            {
                if (!_toggleImage)
                    Debug.LogError("Button is toggleable, but there's no toggle image.");

                _toggleState = false;
                _toggleImage.sprite = _toggleOffImage;
                _onToggleOff.Invoke();
            }
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

                ToggleState();
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

        void ToggleState()
        {
            if (!_isToggleable)
                return;

            if (_toggleState)
            {
                _toggleState = false;
                _toggleImage.sprite = _toggleOffImage;
                _onToggleOff.Invoke();
            }
            else
            {
                _toggleState = true;
                _toggleImage.sprite = _toggleOnImage;
                _onToggleOn.Invoke();
            }
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

        // Invoke functions for tests.
        public void OnButtonDownInvoke()
        {
            _onButtonDown.Invoke();
        }

        public void OnButtonUpInvoke()
        {
            _onButtonUp.Invoke();
        }

        public void OnButtonEnterInvoke()
        {
            _onButtonEnter.Invoke();
        }

        public void OnButtonExitInvoke()
        {
            _onButtonExit.Invoke();
        }

        public void OnToggleOnInvoke()
        {
            _onToggleOn.Invoke();
        }

        public void OnToggleOffInvoke()
        {
            _onToggleOff.Invoke();
        }
    }
}
