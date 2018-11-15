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
    [RequireComponent(typeof(Rigidbody))]
    public class Selectable : PooledMonobehaviour
    {
        bool _isSelected = false;

        [SerializeField] Material _defaultMaterial;
        [SerializeField] Material _selectedMaterial;

        MeshRenderer _renderer;
        Rigidbody _rigidBody;
<<<<<<< HEAD
        Color _color = Color.white;
        Color _originalColor;

=======
        
>>>>>>> c1d8484c23117d42372cef2a4ed4486fe19f5978
        public bool _canBePickedUp = true;
        public bool _canBePushed = true;

        void Awake()
        {
            if (!_defaultMaterial)
                Debug.LogError("Default material is not initialized.");
<<<<<<< HEAD

=======
            
>>>>>>> c1d8484c23117d42372cef2a4ed4486fe19f5978
            if (!_selectedMaterial)
                Debug.LogError("Selected material is not initialized.");

            _renderer = GetComponentInChildren<MeshRenderer>();
            _renderer.material = _defaultMaterial;

            _rigidBody = GetComponent<Rigidbody>();
<<<<<<< HEAD

            _originalColor = _renderer.material.GetColor("_AlbedoColor");
        }

        void OnEnable()
        {
            Deselect();
            ResetColor();
            SetVelocity(Vector3.zero); // Hmm. Is it smart to do this here? Or should the client handle this?
            SetAngularVelocity(Vector3.zero); // Hmm. Is it smart to do this here? Or should the client handle this?
=======
        }
        
        void OnEnable()
        {
            Deselect();
>>>>>>> c1d8484c23117d42372cef2a4ed4486fe19f5978
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
<<<<<<< HEAD
            RefreshColor();
=======
>>>>>>> c1d8484c23117d42372cef2a4ed4486fe19f5978
        }

        /// <summary>
        /// Deselects this instance.
        /// </summary>
        public void Deselect()
        {
            if (!_isSelected)
                return;
<<<<<<< HEAD

            _isSelected = false;
            _renderer.material = _defaultMaterial;
            RefreshColor();
=======
            
            _isSelected = false;
            _renderer.material = _defaultMaterial;
>>>>>>> c1d8484c23117d42372cef2a4ed4486fe19f5978
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
<<<<<<< HEAD
        /// Setter for transform. Input is our custom SerializableTransform to bypass Unity's Transform restrictions.
        /// <param name="newT">The SerializableTransform to copy from.</param>
        /// </summary>
        public void SetTransform(SerializableTransform newT)
        {
            transform.position = newT._position;
            transform.rotation = newT._rotation;
            transform.localScale = newT._scale;
        }

        /// <summary>
=======
>>>>>>> c1d8484c23117d42372cef2a4ed4486fe19f5978
        /// Rotates the attached transform about axis passing through point in world coordinates by deg degrees.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="deg">The deg.</param>
        public void Rotate(Vector3 point, Vector3 axis, float deg)
        {
            transform.RotateAround(point, axis, deg);
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
<<<<<<< HEAD

        /// <summary>
        /// Sets the material color.
        /// </summary>
        /// <param name="color">The color.</param>
        public void ChangeColor(Color color)
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
=======
>>>>>>> c1d8484c23117d42372cef2a4ed4486fe19f5978
    }
}
