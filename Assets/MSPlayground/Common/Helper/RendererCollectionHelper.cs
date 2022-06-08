
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Common.Helper
{
    public class RendererCollectionHelper : MonoBehaviour
    {
        [SerializeField] private bool _startEnabled = true;
        [SerializeField] private Renderer[] _renderers;

        private MaterialPropertyBlock _propertyBlock;

        public bool RenderingEnabled { get; private set; }

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            SetRenderingEnabled(_startEnabled, forceChange: true);
        }

        /// <summary>
        /// Enable or disable all targeted renderers
        /// </summary>
        public void SetRenderingEnabled(bool enabled, bool forceChange = false)
        {
            if (RenderingEnabled != enabled || forceChange)
            {
                RenderingEnabled = enabled;
                for (int i = 0; i < _renderers.Length; i++)
                {
                    _renderers[i].enabled = enabled;
                }
            }
        }

        /// <summary>
        /// Change the color of all targeted renderers, using a Material Property Block
        /// </summary>
        public void SetColor(Color color)
        {
            SetColor("_Color", color);
        }

        /// <summary>
        /// Change the color of all targeted renderers, using a Material Property Block
        /// </summary>
        public void SetColor(string property, Color color)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(property, color);
                _renderers[i].SetPropertyBlock(_propertyBlock);
            }
        }

        /// <summary>
        /// Change the value of a float property on all targeted renderers, using a Material Property Block
        /// </summary>
        public void SetFloat(string property, float value)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat(property, value);
                _renderers[i].SetPropertyBlock(_propertyBlock);
            }
        }
    }
}