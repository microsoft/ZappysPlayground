// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEditor;
using UnityEngine;

namespace MSPlayground.Common
{
    /// <summary>
    /// Tween an alpha value from a renderer 
    /// </summary>
    public class TweenerAlphaRenderer : Tweener<float>
    {
        [SerializeField] private Renderer _target;
        [SerializeField] private string _colorProperty = "_Color";
        [SerializeField] private bool _useMaterialPropertyBlock;

        private MaterialPropertyBlock _materialPropertyBlock;

        private void Awake()
        {
            if (_useMaterialPropertyBlock)
            {
                _materialPropertyBlock = new MaterialPropertyBlock();
            }
        }

        public float Alpha
        {
            get
            {
                float alpha;

                if (_useMaterialPropertyBlock)
                {
                    _target.GetPropertyBlock(_materialPropertyBlock);
                    alpha = _materialPropertyBlock.GetColor(_colorProperty).a;
                }
                else
                {
                    var material = Application.isPlaying ? _target.material : _target.sharedMaterial;
                    alpha = material.GetColor(_colorProperty).a;
                }
                return alpha;
            }
            set
            {
                if (_useMaterialPropertyBlock)
                {
                    _target.GetPropertyBlock(_materialPropertyBlock);
                    Color color = _materialPropertyBlock.GetColor(_colorProperty);
                    color.a = value;
                    _materialPropertyBlock.SetColor(_colorProperty, color);
                    _target.SetPropertyBlock(_materialPropertyBlock);
                }
                else
                {
                    var material = Application.isPlaying ? _target.material : _target.sharedMaterial;

                    if (material != null)
                    {
                        Color color = material.color;
                        color.a = value;
                        material.SetColor(_colorProperty, color);
                    }
                }
            }
        }

        private void UpdateAlpha(float alpha)
        {
            Alpha = alpha;
        }

        private void Reset()
        {
            if (_target == null)
            {
                _target = this.GetComponent<Renderer>();   
            }
            
            SetFromAsCurrent();
            SetToAsCurrent();
        }

        [ContextMenu(nameof(SetFromAsCurrent))]
        public void SetFromAsCurrent()
        {
            _from = _target != null ? Alpha : default;
            SetDirty();
        }

        [ContextMenu(nameof(SetToAsCurrent))]
        public void SetToAsCurrent()
        {
            _to = _target != null ? Alpha : default;
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