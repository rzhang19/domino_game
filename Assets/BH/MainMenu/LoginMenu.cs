using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    public class LoginMenu : MonoBehaviour
    {
        string _inputUsername = "";
        string _inputPassword = "";

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
                Debug.Log(err);
                Debug.Log(data);
            });
        }

        public void Register()
        {
            DataManager.Instance.RegisterUser(_inputUsername, _inputPassword, (err) =>
            {
                Debug.Log(err);
            });
        }
    }
}
