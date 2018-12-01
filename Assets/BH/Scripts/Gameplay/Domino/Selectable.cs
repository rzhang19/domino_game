using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;

namespace BH
{
    /// <summary>
    /// Contains the functionality of being interacted with by the player.
    /// This script must be attached to a game object if it is to be interacted with by the player in any way.
    /// </summary>
    /// <seealso cref="BH.DesignPatterns.PooledMonobehaviour" />
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Selectable : GameObj
    {
        bool _isSelected = false;

        [SerializeField] Material _selectedMaterial;

        public Rigidbody _rigidbody { get; private set; }
        Collider _collider;

        AudioSource _audioSource;
        [SerializeField] AudioClip _playOnCollision;
        //public Material[] materials;
        //int matNumber = 0;

        public bool _canBePickedUp = true;
        public bool _canBePushed = true;

        void Awake()
        {
            if (!_defaultMaterial)
                Debug.LogError("Default material is not initialized.");

            if (!_selectedMaterial)
                Debug.LogError("Selected material is not initialized.");

            _renderer = GetComponentInChildren<MeshRenderer>();
            _renderer.material = _defaultMaterial;

            _rigidbody = GetComponent<Rigidbody>();
           
            _collider = GetComponent<Collider>();

            //_renderer.sharedMaterial = materials[0];
            _originalColor = _renderer.material.GetColor("_AlbedoColor");
        }

        void OnEnable()
        {
            Deselect();
            ResetColor();
            ResetVelocities();
        }

        protected override void OnDisable()
        {
            Deselect();
            base.OnDisable();
        }

        /// <summary>
        /// Plays collision audio upon collisions.
        /// </summary>
        void OnCollisionEnter(Collision other)
        {
            //Rigidbody otherRB = other.gameObject.GetComponent<Rigidbody>();
            //if (otherRB == null)
            //    otherRB = _rigidbody;
            AudioSource.PlayClipAtPoint(_playOnCollision, other.contacts[0].point);
        }

        /// <summary>
        /// Determines whether this instance is selected.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSelected()
        {
            return _isSelected;
        }

        /// <summary>
        /// Selects this instance.
        /// </summary>
        public void Select()
        {
            if (_isSelected)
                return;

            _isSelected = true;
            _renderer.material = _selectedMaterial;
            RefreshColor();
        }

        /// <summary>
        /// Deselects this instance.
        /// </summary>
        public void Deselect()
        {
            if (!_isSelected)
                return;

            _isSelected = false;
            _renderer.material = _defaultMaterial;
            RefreshColor();
        }

        /// <summary>
        /// Toggles select on this instance.
        /// </summary>
        public void ToggleSelect()
        {
            if (_isSelected)
                Deselect();
            else
                Select();
        }

        public void RotateX(float deg)
        {
            RotateAround(GetColliderCenter(), transform.right, deg);
        }

        Vector3 GetColliderCenter()
        {
            return _collider.bounds.center;
        }

        /// <summary>
        /// Freezes the rotation of the attached rigidbody.
        /// </summary>
        public void FreezeRotation()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation | _rigidbody.constraints;
        }

        /// <summary>
        /// Unfreezes the rotation of the attached rigidbody.
        /// </summary>
        public void UnfreezeRotation()
        {
            _rigidbody.constraints = ~RigidbodyConstraints.FreezeRotation & _rigidbody.constraints;
        }

        /// <summary>
        /// Freezes the position of the attached rigidbody.
        /// </summary>
        public void FreezePosition()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezePosition | _rigidbody.constraints;
        }

        /// <summary>
        /// Unfreezes the position of the attached rigidbody.
        /// </summary>
        public void UnfreezePosition()
        {
            _rigidbody.constraints = ~RigidbodyConstraints.FreezePosition & _rigidbody.constraints;
        }

        /// <summary>
        /// Sets the velocity of the attached rigidbody.
        /// </summary>
        public void SetVelocity(Vector3 vel)
        {
            _rigidbody.velocity = vel;
        }

        /// <summary>
        /// Sets the angular velocity of the attached rigidbody.
        /// </summary>
        public void SetAngularVelocity(Vector3 vel)
        {
            _rigidbody.angularVelocity = vel;
        }

        /// <summary>
        /// Resets velocity and angular velocity of the attached rigidbody.
        /// </summary>
        public void ResetVelocities()
        {
            SetVelocity(Vector3.zero);
            SetAngularVelocity(Vector3.zero);
        }

        /// <summary>
        /// Sets the material of the domino
        /// </summary>
        public void SetMaterial()
        {
            //matNumber++;
            //Debug.Log(materials.Length);
            //Debug.Log(Mathf.Abs(matNumber) % materials.Length);
            //_renderer.sharedMaterial = materials[Mathf.Abs(matNumber) % materials.Length];
        }
    }
}
