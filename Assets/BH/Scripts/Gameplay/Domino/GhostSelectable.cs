using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// A game object that can be "ghost-animated" into display.
    /// </summary>
    public class GhostSelectable : GameObj
    {
        [SerializeField] Animator _anim;

        void Awake()
        {
            if (!_anim)
            {
                _anim = GetComponentInChildren<Animator>();
            }

            if (!_defaultMaterial)
                Debug.LogError("Default material is not initialized.");

            _renderer = GetComponentInChildren<MeshRenderer>();
            _renderer.material = _defaultMaterial;
            _originalColor = _renderer.material.GetColor("_AlbedoColor");
        }
        
        void OnEnable()
        {
            ResetColor();
        }

        public void AnimateFadeIn()
        {
            _anim.Play("FadeIn");
        }
    }
}
