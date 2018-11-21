using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    public class GhostSelectable : MonoBehaviour
    {
        [SerializeField] Animator _anim;

        void Awake()
        {
            if (!_anim)
            {
                _anim = GetComponentInChildren<Animator>();
            }
        }

        public void AnimateFadeIn()
        {
            _anim.Play("FadeIn");
        }
    }
}
