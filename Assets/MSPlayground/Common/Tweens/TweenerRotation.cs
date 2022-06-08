// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace MSPlayground.Common
{
    /// <summary>
    /// Rotation tweening MonoBehaviour with the ability to test
    /// </summary>
    public class TweenerRotation : Tweener<Vector3>
    {
        [SerializeField] private Transform _target;

        private void Reset()
        {
            if (_target == null)
            {
                _target = transform;   
            }
            SetFromAsCurrent();
            SetToAsCurrent();
        }

        [ContextMenu(nameof(SetFromAsCurrent))]
        public void SetFromAsCurrent()
        {
            _from = _target.localEulerAngles;
            SetDirty();
        }

        [ContextMenu(nameof(SetToAsCurrent))]
        public void SetToAsCurrent()
        {
            _to = _target.localEulerAngles;
            SetDirty();
        }

        [ContextMenu(nameof(SetToBeginning))]
        public void SetToBeginning()
        {
            _target.localEulerAngles = _from;
            SetDirty();
        }

        [ContextMenu(nameof(SetToEnd))]
        public void SetToEnd()
        {
            _target.localEulerAngles = _to;
            SetDirty();
        }

        protected override void UpdateValue(float t)
        {
            _target.localEulerAngles = Vector3.Lerp(_from, _to, _curve.Evaluate(t));
        }
    }
}