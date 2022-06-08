
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Utils
{
    public static class MathHelpers
    {
        /// <summary>
        /// Convenience function to get a vector3 at a specific y position
        /// </summary>
        /// <param name="v3In">input vector</param>
        /// <param name="yPos">y position</param>
        /// <returns>vector at yPos</returns>
        static public Vector3 Vector3AtYPos(Vector3 v3In, float yPos)
        {
            Vector3 v3Out = new Vector3(v3In.x, yPos, v3In.z);
            return v3Out;
        }

        /// <summary>
        /// Rotate a Vector3 in the horizontal axis only
        /// </summary>
        /// <param name="v3In"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        public static Vector3 RotateYAxis(Vector3 v3In, float rads)
        {
            float sin = Mathf.Sin(rads);
            float cos = Mathf.Cos(rads);

            Vector3 v3Out = new Vector3(
                v3In.x * Mathf.Cos(rads) + v3In.z * Mathf.Sin(rads),
                0,
                v3In.z * Mathf.Cos(rads) - v3In.x * Mathf.Sin(rads)
            );

            return v3Out;
        }

        /// <summary>
        /// Convenience function to get the horizontal magnitude of a vector
        /// </summary>
        /// <param name="v3In">input vector</param>
        /// <returns>distance</returns>
        static public float HorizontalMagnitude(Vector3 v3In)
        {
            return Vector3AtYPos(v3In, 0).magnitude;
        }

        /// <summary>
        /// Convenience function to get the horizontal sqrmagnitude of a vector
        /// </summary>
        /// <param name="v3In">input vector</param>
        /// <returns>distance</returns>
        static public float HorizontalSqrMagnitude(Vector3 v3In)
        {
            return Vector3AtYPos(v3In, 0).sqrMagnitude;
        }

        const float DOT_UP = 0.9f;
        /// <summary>
        /// Check if a vector is pointing up using dot product vs Vector3.up
        /// </summary>
        /// <param name="v3In">the input vector</param>
        /// <returns></returns>
        public static bool IsVectorPointingUp(Vector3 v3In)
        {
            float dot = Vector3.Dot(v3In, Vector3.up);
            if (dot >= DOT_UP)
            {
                return true;
            }
            return false;
        }
    }
}
