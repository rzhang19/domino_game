using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TWM.UI;

namespace BH
{
    public class LoginMenu : MonoBehaviour
    {
        string _inputUsername = "";
        string _inputPassword = "";

        [SerializeField] TextMeshProUGUI _loginStatusText;
        UIElementAnimator _uiElementAnimator;

        void Awake()
        {
            if (!_loginStatusText)
                Debug.LogError("Login status text is not initialized.");

            _uiElementAnimator = _loginStatusText.GetComponent<UIElementAnimator>();
        }

        public void SetUsername(string username)
        {
            _inputUsername = username;
        }

        public void SetPassword(string password)
        {
            _inputPassword = password;
        }

        public void SignIn()
        {
            DataManager.Instance.GetData(_inputUsername, _inputPassword, (data, err) =>
            {
                switch (err)
                {
                    case DataManagerStatusCodes.SUCCESS:
                        SetLoginStatusText("Successfully signed in as " + _inputUsername + "!");
                        break;
                    case DataManagerStatusCodes.DATABASE_ERROR:
                        SetLoginStatusText("Database error occured!");
                        break;
                    case DataManagerStatusCodes.WRONG_CREDENTIALS:
                        SetLoginStatusText("Wrong credentials!");
                        break;
                    default:
                        SetLoginStatusText("Unknown error occured!");
                        break;
                }

                if (_uiElementAnimator)
                    _uiElementAnimator.ScalePop();

                Debug.Log(data);
            });
        }

        public void Register()
        {
            DataManager.Instance.RegisterUser(_inputUsername, _inputPassword, (err) =>
            {
                switch (err)
                {
                    case DataManagerStatusCodes.SUCCESS:
                        SetLoginStatusText("Successfully registered new user " + _inputUsername + "!");
                        break;
                    case DataManagerStatusCodes.DATABASE_ERROR:
                        SetLoginStatusText("Database error occured!");
                        break;
                    case DataManagerStatusCodes.USERNAME_TAKEN:
                        SetLoginStatusText("Username already taken!");
                        break;
                    default:
                        SetLoginStatusText("Unknown error occured!");
                        break;
                }

                if (_uiElementAnimator)
                    _uiElementAnimator.ScalePop();
            });
        }

        void SetLoginStatusText(string text)
        {
            _loginStatusText.SetText(text);
        }
    }
}
