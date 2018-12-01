using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    [RequireComponent(typeof(Animator))]
    public class BackgroundDomino : MonoBehaviour
    {
        Animator _anim;

        void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        public void ToggleLowHigh()
        {
            if (_anim.GetBool("isHigh"))
                Low();
            else
                High();
        }

        public void Low()
        {
            _anim.SetBool("isHigh", false);
        }

        public void High()
        {
            _anim.SetBool("isHigh", true);
        }
    }
}
