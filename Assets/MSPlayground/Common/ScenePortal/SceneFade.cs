// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.GraphicsTools;
using MSPlayground.Core.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MSPlayground.Common
{
    public class SceneFade : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private float _transitionTime = 1.0f;
        [SerializeField] private float _preSceneChangeDelay = 2.0f;

        private Material _material;
        private MaterialPropertyBlock _propertyBlock;
        private float _currentOpacity;
        private Coroutine _fadeRoutine;

        private void Awake()
        {
            _material = _renderer.EnsureComponent<MaterialInstance>().Material;
            _propertyBlock = new MaterialPropertyBlock();
            _renderer.enabled = false;
        }

        public void Reset()
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }
            UpdateOpacity(0);
        }

        private void Start()
        {
            //Register events
            GlobalEventSystem.Register<TransitionStartEvent>(TransitionStartEventHandler);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            //Deregister events
            GlobalEventSystem.Unregister<TransitionStartEvent>(TransitionStartEventHandler);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Event called to fade out scene when transition begins.
        /// </summary>
        private void TransitionStartEventHandler(TransitionStartEvent obj)
        {
            FadeOut(_preSceneChangeDelay);
        }

        /// <summary>
        /// Fade in to scene when a new one is loaded.
        /// </summary>
        /// <param name="level"></param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FadeIn();
        }

        /// <summary>
        /// Update the opacity of the fade material instance.
        /// </summary>
        private void UpdateOpacity(float opacity)
        {
            Color c = Color.black;
            c.a = opacity;

            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_Color", c);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        /// <summary>
        /// Fade in from black to the scene.
        /// </summary>
        public void FadeIn(float delay = 0)
        {
            Fade(fadeIn: true, delay);
        }

        /// <summary>
        /// Fade out the scene to black.
        /// </summary>
        public void FadeOut(float delay = 0)
        {
            Fade(fadeIn: false, delay);
        }

        /// <summary>
        /// Start a fade in or fade out coroutine, canceling any already in progress.
        /// </summary>
        private void Fade(bool fadeIn, float delay = 0)
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }
            
            UpdateOpacity(fadeIn ? 1.0f : 0.0f);
            _fadeRoutine = StartCoroutine(FadeRoutine(fadeIn, delay));
        }

        /// <summary>
        /// Change the opacity of the fade panel over a length of time.
        /// </summary>
        private IEnumerator FadeRoutine(bool fadeIn, float delay = 0)
        {
            _renderer.enabled = true;

            float timer = -delay;

            while (timer <= _transitionTime)
            {
                timer += Time.deltaTime;
                float ratio = Mathf.Clamp01(timer / _transitionTime);

                _currentOpacity = fadeIn ? 1 - ratio : ratio;
                UpdateOpacity(_currentOpacity);

                yield return null;
            }

            if (fadeIn)
            {
                _renderer.enabled = false;
            }

            _fadeRoutine = null;
        }
    }
}
