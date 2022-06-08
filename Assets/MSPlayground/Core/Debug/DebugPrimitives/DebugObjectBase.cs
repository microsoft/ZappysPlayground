
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace MSPlayground.Core
{
    public class DebugObjectBase : MonoBehaviour
    {
        Renderer _renderer;

        public void SetColor(Color color)
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<MeshRenderer>();
            }

            _renderer.material.SetColor("_Color", color);
        }
    }
}
