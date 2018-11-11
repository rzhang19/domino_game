using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;

namespace BH
{
    [RequireComponent(typeof(Rigidbody))]
    public class Selectable : PooledMonobehaviour//, ISelectable
    {
        bool _isSelected = false;

        [SerializeField] Material _defaultMaterial;
        [SerializeField] Material _selectedMaterial;

        MeshRenderer _renderer;

        Rigidbody _rigidBody;

        void Awake()
        {
            if (!_defaultMaterial)
                Debug.LogError("Default material is not initialized.");
            
            if (!_selectedMaterial)
                Debug.LogError("Selected material is not initialized.");

            _renderer = GetComponentInChildren<MeshRenderer>();
            _renderer.material = _defaultMaterial;

            _rigidBody = GetComponent<Rigidbody>();
        }
        
        void OnEnable()
        {
            Deselect();
        }

        protected override void OnDisable()
        {
            Deselect();
            base.OnDisable();
        }

        public bool IsSelected()
        {
            return _isSelected;
        }

        public void Select()
        {
            if (_isSelected)
                return;

            _isSelected = true;
            _renderer.material = _selectedMaterial;
        }

        public void Deselect()
        {
            if (!_isSelected)
                return;
            
            _isSelected = false;
            _renderer.material = _defaultMaterial;
        }

        public void Toggle()
        {
            if (_isSelected)
                Deselect();
            else
                Select();
        }

        public void Delete()
        {
            Debug.Log("Deleted " + name);
            gameObject.SetActive(false);
        }

        public void Rotate(Vector3 point, Vector3 axis, float deg)
        {
            transform.RotateAround(point, axis, deg);
        }

        public void FreezeRotation()
        {
            _rigidBody.freezeRotation = true;
        }

        public void UnfreezeRotation()
        {
            _rigidBody.freezeRotation = false;
        }

        public void SetVelocity(Vector3 vel)
        {
            _rigidBody.velocity = vel;
        }

        public void SetAngularVelocity(Vector3 vel)
        {
            _rigidBody.angularVelocity = vel;
        }
    }
}
