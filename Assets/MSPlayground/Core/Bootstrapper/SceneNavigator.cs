
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MSPlayground.Common;
using MSPlayground.Core.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MSPlayground.Core
{
    /// <summary>
    /// Add this to an object in the Bootstrap scene.  It autoloads into _mainSceneName
    /// and handles scene navigation.
    /// </summary>
    public class SceneNavigator : MonoBehaviour
    {
        static SceneNavigator _instance;
        /// <summary>
        /// Scenes that will user will not be able to navigate to via the debug menu
        /// </summary>
        static readonly string[] HIDDEN_SCENES =
        {
            "BootstrapScene"
        };
        
        /// <summary>
        /// Scene to load after bootstrap
        /// </summary>
        [SerializeField] string _mainSceneName = "Hub";
        /// <summary>
        /// After critical setup steps are complete in main scene, bypass flow to go to this scene instead.
        /// If null or empty then there is no bypass.
        /// </summary>
        [SerializeField] string _autoloadSceneAfterSetup = null;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                GameObject.Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        private void Start()
        {
            SceneManager.LoadScene(_mainSceneName, LoadSceneMode.Single);
            
            SetupDebugMenuItems();
        }

        /// <summary>
        /// Add scene nav items to debug menu
        /// </summary>
        void SetupDebugMenuItems()
        {
            for (int i=0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                // Only add debug button if it is not a hidden scene
                if (!HIDDEN_SCENES.Contains(sceneName))
                {
                    DebugMenu.AddButton($"Go to Scene/{sceneName}", () => DoGoToScene(sceneName));
                }
            }
        }
        
        /// <summary>
        /// Go to scene
        /// </summary>
        /// <param name="sceneName"></param>
        void DoGoToScene(string sceneName)
        {
            GlobalEventSystem.Fire<WillLoadNewSceneEvent>(new WillLoadNewSceneEvent()
            {
                SceneToLoad = sceneName
            });

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Go to the autoload scene after critical setup steps are complete.
        /// Clear reference to autoload scene so users can still come back to main scene after.
        /// </summary>
        void DoAutoLoadSceneAfterSetup()
        {
            GoToScene(_autoloadSceneAfterSetup);
            _autoloadSceneAfterSetup = null;
        }
        
        #region Static Interface
        /// <summary>
        /// Load scene
        /// </summary>
        public static void GoToScene(string sceneName) => _instance.DoGoToScene(sceneName);

        /// <summary>
        /// Autoload the set scene after critical setup is complete
        /// </summary> 
        public static void AutoLoadSceneAfterSetup() => _instance.DoAutoLoadSceneAfterSetup();

        /// <summary>
        /// True if we are bypassing the main scene flow except for critical anchor setup steps.
        /// </summary>
        public static bool BypassSetupFlow => !string.IsNullOrEmpty(_instance._autoloadSceneAfterSetup);

#if VRBUILD
        public static bool LoadVREnv => true;
#else
        public static bool LoadVREnv => false;
#endif

#if VRBUILD
        public static bool BypassScanningAndAnchoring => true;
#else
        public static bool BypassScanningAndAnchoring => false;
#endif

#endregion
    }
}
