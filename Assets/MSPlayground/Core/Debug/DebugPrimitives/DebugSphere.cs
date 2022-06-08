
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace MSPlayground.Core
{
    public class DebugSphere : DebugObjectBase
    {
        static Object _prefab = null;
        const string PREFAB_NAME = "DebugSphere";

        static public DebugSphere Create(Transform parent, Vector3 position, float scale = 0.1f, Color? color = null)
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

            DebugSphere debugSphere = go.GetComponent<DebugSphere>();
            if (color.HasValue)
            {
                debugSphere.SetColor(color.Value);
            }

            return debugSphere;
        }
    }
}
