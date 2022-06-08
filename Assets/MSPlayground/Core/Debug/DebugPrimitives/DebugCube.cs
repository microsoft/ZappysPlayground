
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core
{
    public class DebugCube : DebugObjectBase
    {
        const string PREFAB_NAME = "DebugCube";
        static Object _prefab = null;

        static public DebugCube Create(Transform parent, Vector3 position, float scale = 0.08f, Color? color = null)
        {
            if (_prefab == null)
            {
                _prefab = Resources.Load(PREFAB_NAME);

                if (_prefab == null)
                {
                    Debug.LogError($"Failed to load {PREFAB_NAME}");
                    return null;
                }
            }

            GameObject go = GameObject.Instantiate(_prefab) as GameObject;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale, scale, scale);

            DebugCube debugCube = go.GetComponent<DebugCube>();
            if (color.HasValue)
            {
                debugCube.SetColor(color.Value);
            }

            return debugCube;
        }
    }
}
