
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace MSPlayground.Core
{
    /// <summary>
    /// Component to disable hand visualization on device
    /// </summary>
    public class HandVisualizerDisabler : MonoBehaviour
    {
#if !UNITY_EDITOR
        /// <summary>
        /// Disable the hand visualizer on device
        /// </summary>
        void Start()
        {
            GetComponent<HandVisualizer>().enabled = false;
        }
#endif
    }
}
