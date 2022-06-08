
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core
{
    public class DebugAxis : MonoBehaviour
    {
        const string PREFAB_NAME = "DebugAxis";
        static Object _prefab = null;

        static public DebugAxis Create(Transform parent)
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
            go.transform.position = parent.transform.position;
            go.transform.rotation = parent.transform.rotation;
            go.transform.SetParent(parent, true);

            return go.GetComponent<DebugAxis>();
        }
    }
}
