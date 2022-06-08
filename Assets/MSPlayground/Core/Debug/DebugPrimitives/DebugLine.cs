
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core
{
    public class DebugLine : MonoBehaviour
    {
        const string PREFAB_NAME = "DebugLine";
        static Object _prefab = null;

        LineRenderer _lineRenderer;

        static public DebugLine Create(Transform parent, Vector3 startPos, Vector3 direction, float distance)
        {
            Vector3 endPos = startPos + direction * distance;

            return Create(parent, startPos, endPos);
        }

        static public DebugLine Create(Transform parent, Vector3 startPos, Vector3 endPos)
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

            DebugLine debugLine = go.GetComponent<DebugLine>();
            debugLine._lineRenderer = go.GetComponent<LineRenderer>();
            debugLine.SetLine(startPos, endPos);

            return debugLine;
        }

        public void SetLine(Vector3 startPos, Vector3 direction, float distance)
        {
            SetLine(startPos, startPos + direction * distance);
        }

        public void SetLine(Vector3 startPos, Vector3 endPos)
        {
            _lineRenderer.SetPosition(0, startPos);
            _lineRenderer.SetPosition(1, endPos);
        }
    }
}