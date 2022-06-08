
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable 0414

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MSPlayground.Core
{
    /// <summary>
    /// Add this to any scene to force loading of the bootstrap scene in editor.
    /// If the Bootstrapper tag is not found (which exists only in the bootstrap scene), it loads into the bootstrap scene.
    /// If it is found, then we've already bootstrapped and nothing to do.
    /// </summary>
    public class EditorBootstrapHelper : MonoBehaviour
    {
        [SerializeField] string _bootstrapTag = "Bootstrapper";
        [SerializeField] string _bootstrapSceneName = "BootstrapScene";

        /// <summary>
        /// Check for the bootstrapper tag.  If its not found, we need to load the bootstrapper scene first.
        /// </summary>
        private void Awake()
        {
#if UNITY_EDITOR
            GameObject bootstrapper = GameObject.FindGameObjectWithTag(_bootstrapTag);
            if (bootstrapper == null)
            {
                // We need to destroy all gameobjects in this scene before reloading, otherwise their Awake will still
                //  get called, even though the new scene has been loaded, which can throw a bunch of exceptions.
                DestroyAllGameObjectsInScene();

                // load bootstrapper scene
                SceneManager.LoadScene(_bootstrapSceneName, LoadSceneMode.Single);
            }
            else
            {
                GameObject.Destroy(gameObject);
            }
#else
            GameObject.Destroy(gameObject);
#endif
        }

        /// <summary>
        /// Destroy all gameobjects in the active scene
        /// </summary>
#if UNITY_EDITOR
        private void DestroyAllGameObjectsInScene()
        {
            List<GameObject> allGameObjects = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(allGameObjects);

            foreach (GameObject go in allGameObjects)
            {
                GameObject.DestroyImmediate(go);
            }
        }
#endif
    }
}
