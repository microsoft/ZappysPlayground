
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core;
using Microsoft.MixedReality.Toolkit.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core
{
    /// <summary>
    /// Helper class for VisialProfiler
    /// - parent the window before it is created
    /// - we need to do this otherwise it has no parent and gets unloaded on a scene reload
    /// </summary>
    public class VisualProfilerHelper : MonoBehaviour
    {
        HackedVisualProfiler _visualProfiler;

        [SerializeField] bool _enableOnStart = false;

        /// <summary>
        /// Called when the component starts
        /// </summary>
        void Start()
        {
            _visualProfiler = GetComponent<HackedVisualProfiler>();
            _visualProfiler.WindowParent = transform;
            _visualProfiler.IsVisible = _enableOnStart;

            DebugMenu.AddButton("Toggle FPS", () => 
            {
                _visualProfiler.IsVisible = !_visualProfiler.IsVisible;
                // force an update otherwise the window will stay visible when we set it not visible
                _visualProfiler.LateUpdate();
                _visualProfiler.enabled = _visualProfiler.IsVisible;
            });
        }
    }
}
