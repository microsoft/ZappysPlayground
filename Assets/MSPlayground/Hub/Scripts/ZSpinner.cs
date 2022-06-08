
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Hub
{
    /// <summary>
    /// Script to constantly rotate an object around the z-axis.
    /// </summary>
    public class ZSpinner : MonoBehaviour
    {
        [SerializeField] private Transform _transform = null;
        [SerializeField] private float _degreesRotationPerSecond = 50f;

        private void Reset()
        {
            if (!_transform)
            {
                _transform = GetComponent<Transform>();
            }
        }

        private void Update ()
        {
            //rotates 50 degrees per second around z axis
            _transform.Rotate (0,0,_degreesRotationPerSecond*Time.deltaTime); 
        }
    }
}
