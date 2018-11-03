using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;

namespace BH
{
    public class Domino : PooledMonobehaviour, ISelectable
    {
        bool _isSelected = false;

        [SerializeField] Material _defaultMaterial;
        [SerializeField] Material _selectedMaterial;

        MeshRenderer _renderer;

        void Awake()
        {
            if (!_defaultMaterial)
                Debug.LogError("Default material is not initialized.");
            
            if (!_selectedMaterial)
                Debug.LogError("Selected material is not initialized.");

            _renderer = GetComponentInChildren<MeshRenderer>();
            _renderer.material = _defaultMaterial;
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
    }
}
