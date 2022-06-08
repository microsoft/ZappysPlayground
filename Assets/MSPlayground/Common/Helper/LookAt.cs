
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace MSPlayground.Common
{
    /// <summary>
    /// Helper component to always orient this transform towards a target
    /// </summary>
    public class LookAt : MonoBehaviour
    {
        [SerializeField] private bool _targetIsMainCamera = true;
        [SerializeField] private Transform _target;
        private Transform _cachedTransform;

        private void Start()
        {
            _cachedTransform = transform;
            if (_targetIsMainCamera)
            {
                _target = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            Vector3 lookAtVector = _cachedTransform.position - _target.position;
            if (lookAtVector != Vector3.zero)
            {
                _cachedTransform.rotation = Quaternion.LookRotation(lookAtVector);
            }
        }
    }
}