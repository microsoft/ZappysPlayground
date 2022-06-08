// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MSPlayground.Common
{
    /// <summary>
    /// Manage lighting across all scenes.
    /// </summary>
    public class LightingManager : MonoBehaviour
    {
        [SerializeField] private Light _directionalLight;
        [SerializeField] private Material _skyboxMaterial;
        [SerializeField] private ReflectionProbe _reflectionProbe;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

            #if VRBUILD
                _directionalLight.enabled = false;
                _reflectionProbe.enabled = false;
                //Destroy all extra directional lights
                Light[] allLights = Light.GetLights(LightType.Directional, 0);
                for (int i = 0; i < allLights.Length; i++)
                {
                    if (allLights[i] != _directionalLight)
                    {
                        Destroy(allLights[i].gameObject);
                    }
                }

            #else
                //Destroy all extra directional lights
                Light[] allLights = Light.GetLights(LightType.Directional, 0);
                for (int i = 0; i < allLights.Length; i++)
                {
                    if (allLights[i] != _directionalLight)
                    {
                        Destroy(allLights[i].gameObject);
                    }
                }

                //Set new scene's skybox and sun
                RenderSettings.skybox = _skyboxMaterial;
                RenderSettings.sun = _directionalLight;
                DynamicGI.UpdateEnvironment();

                // Refresh reflection probe
                _reflectionProbe.RenderProbe();
            #endif
        }
    }
}
