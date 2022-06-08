
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;


namespace MSPlayground.Core
{
    /// <summary>
    /// Orient this gameobject to point towards the directional target
    /// </summary>
    public class DirectionalArrowOrientation : MonoBehaviour
    {
        [SerializeField]
        private float _lookatSpeed = 0.3f;   
        
        private Transform _cachedTransform;
        private DirectionalIndicator _indicator;

        private void Start()
        {
            _cachedTransform = transform;
            _indicator = GetComponentInParent<DirectionalIndicator>();
            if (_indicator == null)
            {
                Debug.LogError($"Unable to find component {nameof(DirectionalIndicator)}", gameObject);
                enabled = false;
            }
        }

        private void Update()
        {
            if (_indicator.DirectionalTarget != null)
            {
                var targetRotation = Quaternion.LookRotation(_indicator.DirectionalTarget.position - _cachedTransform.position);
                _cachedTransform.rotation = Quaternion.Slerp(_cachedTransform.rotation, targetRotation, _lookatSpeed * Time.deltaTime);
            }
        }
    }
}
