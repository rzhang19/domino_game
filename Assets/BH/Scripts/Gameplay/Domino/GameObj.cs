using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;

namespace BH
{
    /// <summary>
    /// Represents a generic game object with a changeable color and transform. Part of our object pool.
    /// </summary>
    /// <seealso cref="BH.DesignPatterns.PooledMonobehaviour" />
    public class GameObj : PooledMonobehaviour
    {
        [SerializeField] protected Material _defaultMaterial;
        protected MeshRenderer _renderer;
        protected Color _color = Color.white;
        protected Color _originalColor;

        void Awake()
        {
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
        protected void RefreshColor()
        {
            _renderer.material.SetColor("_AlbedoColor", _color);
        }
    }
}
