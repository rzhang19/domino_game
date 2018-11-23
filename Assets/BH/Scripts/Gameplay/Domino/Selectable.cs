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
    public class Selectable : PooledMonobehaviour
    {
        bool _isSelected = false;

        [SerializeField] Material _defaultMaterial;
        [SerializeField] Material _selectedMaterial;

        MeshRenderer _renderer;
        Rigidbody _rigidBody;
        Collider _collider;
        Color _color = Color.white;
        Color _originalColor;

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

            _rigidBody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();

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

        /// <summary>
        /// Disables the game object that the instance is attached to.
        /// By disabling the object, this makes the game object eligible for pool collection.
        /// </summary>
        public void Delete()
        {
            Debug.Log("Deleted " + name);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Setter for transform. Input is our custom SerializableTransform to bypass Unity's Transform restrictions.
        /// <param name="newT">The SerializableTransform to copy from.</param>
        /// </summary>
        public void SetTransform(SerializableTransform newT)
        {
            transform.position = newT._position;
            transform.rotation = newT._rotation;
            transform.localScale = newT._scale;
            //ResetVelocities(); ? maybe useful here
        }

        /// <summary>
        /// Rotates the attached transform about axis passing through point in world coordinates by deg degrees.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="deg">The deg.</param>
        public void RotateAround(Vector3 point, Vector3 axis, float deg)
        {
            transform.RotateAround(point, axis, deg);
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
            _rigidBody.freezeRotation = true;
        }

        /// <summary>
        /// Unfreezes the rotation of the attached rigidbody.
        /// </summary>
        public void UnfreezeRotation()
        {
            _rigidBody.freezeRotation = false;
        }

        /// <summary>
        /// Sets the velocity of the attached rigidbody.
        /// </summary>
        public void SetVelocity(Vector3 vel)
        {
            _rigidBody.velocity = vel;
        }

        /// <summary>
        /// Sets the angular velocity of the attached rigidbody.
        /// </summary>
        public void SetAngularVelocity(Vector3 vel)
        {
            _rigidBody.angularVelocity = vel;
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
        /// Sets the material color.
        /// </summary>
        /// <param name="color">The color.</param>
        public void SetColor(Color color)
        {
            // Changes the color of the material
            _color = color;
            RefreshColor();
        }

        /// <summary>
        /// Resets the material color to its first, original color.
        /// </summary>
        public void ResetColor()
        {
            _color = _originalColor;
            RefreshColor();
        }

        /// <summary>
        /// Getter for the material's current color.
        /// </summary>
        /// <returns>
        ///   An object of type <c>Color</c> representing the current color.
        /// </returns>
        public Color GetColor()
        {
            return _color;
        }

        /// <summary>
        /// Completely re-initialize the material's color.
        /// </summary>
        void RefreshColor()
        {
            _renderer.material.SetColor("_AlbedoColor", _color);
        }
    }
}
