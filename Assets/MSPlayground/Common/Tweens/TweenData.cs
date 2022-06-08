// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace MSPlayground.Common
{
    /// <summary>
    /// Basic tween data of time and an animation curve
    /// </summary>
    [System.Serializable]
    public class TweenData
    {
        public float Duration = 1.0f;
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}