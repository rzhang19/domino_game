using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    public class BezierCurve
    {
        Vector3 p0;
        Vector3 p1;
        Vector3 p2;
        Vector3 p3;

        public BezierCurve(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            p0 = point1;
            p1 = point2;
            p2 = point3;
            p3 = point4;
        }

        public Vector3 ValueAt(float t)
        {
            Vector3 val = Mathf.Pow(1 - t, 3) * p0
                + 3 * Mathf.Pow(1 - t, 2) * t * p1
                + 3 * (1 - t) * Mathf.Pow(t, 2) * p2
                + Mathf.Pow(t, 3) * p3;
            return val;
        }

        public Vector3 TangentAt(float t)
        {
            float oneMinusT = 1 - t;
            Vector3 tangent = 3f * oneMinusT * oneMinusT * (p1 - p0) +
                6f * oneMinusT * t * (p2 - p1) +
                3f * t * t * (p3 - p2);
            return tangent;
        }
    }
}
