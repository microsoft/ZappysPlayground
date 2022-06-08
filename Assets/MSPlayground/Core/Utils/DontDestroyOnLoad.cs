
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core
{
    /// <summary>
    /// Set the DontDestroyOnLoad flag on the game object
    /// </summary>
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }
}
