using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// A simple imitation of Unity's built-in Transform. Exists because Unity severely restricts Transform instantiations.
    /// </summary>
    public struct CustomTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;

        /// <summary>
        /// Creates a CustomTransform with the same position, rotation, and localScale properties as the input.
        /// </summary>
        /// <param name='t'>Transform to copy from.</param>
        public CustomTransform(Transform t)
        {
            position = t.position;
            rotation = t.rotation;
            localScale = t.localScale;
        }
    }
}
