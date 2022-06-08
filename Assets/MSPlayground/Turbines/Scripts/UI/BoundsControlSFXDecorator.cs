
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// A decorator to play a tick sound effect every X degrees rotation of a manipulated
    /// bounds control object.
    /// </summary>
    public class BoundsControlSFXDecorator : MonoBehaviour
    {
        /// <summary>
        /// Angle (degrees) to rotate before playing the tick sound effect.
        /// </summary>
        private const float ANGLE_TICK = 15f;
        
        [SerializeField] private BoundsControl _boundsControl = null;
        [SerializeField] private AudioSource _audioSource = null;
        [SerializeField] private AudioClip _sfxManipulateTick = null;

        Quaternion _lastTickQuaternion = Quaternion.identity;
        
        private void Reset()
        {
            if (_boundsControl == null)
            {
                _boundsControl = GetComponent<BoundsControl>();
            }
        }

        private void Update()
        {
            // If the bounds control is currently being manipulated, then play the tick sound effect
            // every X degrees rotation (where X is ANGLE_TICK).
            if (_boundsControl.enabled && _boundsControl.IsManipulated)
            {
                Single rotationAngle = Quaternion.Angle(_boundsControl.transform.rotation, _lastTickQuaternion);
                if (rotationAngle >= ANGLE_TICK)
                {
                    _audioSource.PlayOneShot(_sfxManipulateTick, 0.7f);
                    _lastTickQuaternion = _boundsControl.transform.rotation;
                }
            }
        }
    }
}
