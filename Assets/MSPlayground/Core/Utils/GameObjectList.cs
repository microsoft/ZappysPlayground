
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MSPlayground.Core.Utils
{
    /// <summary>
    /// Simple list of objects that can be assigned in the inspector for easy iteration
    /// </summary>
    [System.Serializable]
    public class GameObjectList
    {
        [SerializeField] List<GameObject> _gameObjects;

        /// <summary>
        /// Set active state of all objects
        /// </summary>
        /// <param name="active">desired state</param>
        public void SetActive(bool active)
        {
            Iterate((GameObject gameObject) => gameObject.SetActive(active));
        }

        /// <summary>
        /// Iterate all objects in the list
        /// </summary>
        /// <param name="callback">action called for each object</param>
        public void Iterate(UnityAction<GameObject> callback)
        {
            if (_gameObjects != null)
            {
                foreach (GameObject gameObject in _gameObjects)
                {
                    callback.Invoke(gameObject);
                }
            }
        }
    }
}
