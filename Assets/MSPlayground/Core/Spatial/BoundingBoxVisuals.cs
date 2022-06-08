// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Unity.Profiling;
using UnityEngine;

namespace MSPlayground.Common
{
    /// <summary>
    /// Helper to change the material of bounding box visuals between green and default color
    /// </summary>
    public class BoundingBoxVisuals : MonoBehaviour
    {
        [SerializeField] private Material _handleMaterialDefault = null;
        [SerializeField] private Material _handleMaterialGreen = null;
        [SerializeField] private Material _boxMaterialDefault = null;
        [SerializeField] private Material _boxMaterialGreen = null;
        [SerializeField] private MeshRenderer[] _handleRenderers = null;
        [SerializeField] private MeshRenderer _boxRenderer = null;

        private void Reset()
        {
            if (_handleRenderers == null || _handleRenderers.Length == 0)
            {
                _handleRenderers = GetComponentsInChildren<MeshRenderer>();
            }
        }

        public void SetMaterial(bool isGreen)
        {
            foreach (var renderer in _handleRenderers)
            {
                renderer.material = isGreen ? _handleMaterialGreen : _handleMaterialDefault;
            }
            _boxRenderer.material = isGreen ? _boxMaterialGreen : _boxMaterialDefault;
        }
    }
}