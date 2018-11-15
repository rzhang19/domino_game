using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    public struct CustomTransform
    {
        public Vector3 position;
        public Quaternion rotation;

        public CustomTransform(Transform t) //(Vector3 p, Quaternion r)
        {
            /*
            position = p;
            rotation = r;
            */
            position = t.position;
            rotation = t.rotation;
        }
    }
}
