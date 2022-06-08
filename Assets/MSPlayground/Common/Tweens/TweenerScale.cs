
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace MSPlayground.Common
{
    /// <summary>
    /// Scale tweening MonoBehaviour with the ability to test
    /// </summary>
    public class TweenerScale : Tweener<Vector3>
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
            _from = _target.localScale;
            SetDirty();
        }

        [ContextMenu(nameof(SetToAsCurrent))]
        public void SetToAsCurrent()
        {
            _to = _target.localScale;
            SetDirty();
        }

        [ContextMenu(nameof(SetToBeginning))]
        public void SetToBeginning()
        {
            _target.localScale = _from;
            SetDirty();
        }

        [ContextMenu(nameof(SetToEnd))]
        public void SetToEnd()
        {
            _target.localScale = _to;
            SetDirty();
        }

        protected override void UpdateValue(float t)
        {
            _target.localScale = Vector3.Lerp(_from, _to, _curve.Evaluate(t));
        }
    }
}