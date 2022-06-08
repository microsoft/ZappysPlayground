// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.UI;

namespace MSPlayground.Common
{
    /// <summary>
    /// Tween a target UGUI Graphic or CanvasGroup alpha value 
    /// </summary>
    public class TweenerAlphaCanvas : Tweener<float>
    {
        [SerializeField] private Graphic _graphic;
        [SerializeField] private CanvasGroup _group;

        private void UpdateAlpha(float alpha)
        {
            if (_graphic != null)
            {
                Color color = _graphic.color;
                color.a = alpha;
                _graphic.color = color;
            }

            if (_group != null)
            {
                _group.alpha = alpha;
            }
        }

        private void Reset()
        {
            if (_graphic == null)
            {
                _graphic = this.GetComponent<Graphic>();   
            }

            if (_group == null)
            {
                _group = this.GetComponent<CanvasGroup>();   
            }
            
            SetFromAsCurrent();
            SetToAsCurrent();
        }

        [ContextMenu(nameof(SetFromAsCurrent))]
        public void SetFromAsCurrent()
        {
            _from = _graphic != null ? _graphic.color.a : _group != null ? _group.alpha : default;
            SetDirty();
        }

        [ContextMenu(nameof(SetToAsCurrent))]
        public void SetToAsCurrent()
        {
            _to = _graphic != null ? _graphic.color.a : _group != null ? _group.alpha : default;
            SetDirty();
        }

        [ContextMenu(nameof(SetToBeginning))]
        public void SetToBeginning()
        {
            UpdateAlpha(_from);
            SetDirty();
        }

        [ContextMenu(nameof(SetToEnd))]
        public void SetToEnd()
        {
            UpdateAlpha(_to);
            SetDirty();
        }

        protected override void UpdateValue(float t)
        {
            UpdateAlpha(Mathf.Lerp(_from, _to, _curve.Evaluate(t)));
        }
    }
}