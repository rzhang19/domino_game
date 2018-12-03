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

        /// <summary>
        /// Initializes a new instance of the <see cref="BezierCurve"/> class.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <param name="point3">The third point.</param>
        /// <param name="point4">The fourth point.</param>
        public BezierCurve(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            p0 = point1;
            p1 = point2;
            p2 = point3;
            p3 = point4;
        }

        /// <summary>
        /// Returns the value of the Bezier curve evaluated at t.
        /// </summary>
        /// <param name="t">The value to evaluate at.</param>
        /// <returns></returns>
        public Vector3 ValueAt(float t)
        {
            Vector3 val = Mathf.Pow(1 - t, 3) * p0
                + 3 * Mathf.Pow(1 - t, 2) * t * p1
                + 3 * (1 - t) * Mathf.Pow(t, 2) * p2
                + Mathf.Pow(t, 3) * p3;
            return val;
        }

        /// <summary>
        /// Returns the tangent vector of the Bezier curve evaluated at t.
        /// </summary>
        /// <param name="t">The value to evaluate at.</param>
        /// <returns></returns>
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
