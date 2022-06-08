// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// Common utility methods used for by Radial UI interactions
    /// </summary>
    public static class RadialHelpers
    {
        /// <summary>
        /// Calculates a position on a radial
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="startDegree"></param>
        /// <param name="deltaDegree"></param>
        /// <param name="transform"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector3 CalculatePosition(float percentage, float startDegree, float deltaDegree, Transform transform, float radius)
        {
            Vector3 dir = Quaternion.AngleAxis(startDegree + deltaDegree * percentage, -transform.forward) * transform.up;
            return ProjectOnCircumference(transform.position + dir, transform, radius);
        }

        
        /// <summary>
        /// Takes a position and normalizes it within the radius of the radial
        /// </summary>
        /// <param name="position"></param>
        /// <param name="transform"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector3 ProjectOnCircumference(Vector3 position, Transform transform, float radius)
        {
            return transform.position + ((position - transform.position).normalized * radius);
        }

        public static float NicifyDegreeValue(float value)
        {
            const float FULL_CIRCLE = 360;
            if (value > FULL_CIRCLE || value < -FULL_CIRCLE)
            {
                value = value % FULL_CIRCLE;
            }

            return value;
        }
    }
}