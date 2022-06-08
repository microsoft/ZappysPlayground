using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Data;
using MSPlayground.Core.Data;
using MSPlayground.Core.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace MSPlayground.Core
{
    /// <summary>
    /// UI system that spawns panel instances from set prefabs.
    /// Can spawn:
    ///   - complex panels with multiple localization keys
    ///     used within one prefab. The combinations are stored
    ///     as a JSON within Resources folder
    ///   - simple panels that only need a singular
    ///     localization key
    ///
    /// This system uses MRTK Data binding in conjunction with
    /// the Unity Localization package.
    /// </summary>
    public class UISystem : MonoBehaviour
    {
        static UISystem _instance;

        const string RESOURCES_PATH_COMPLEX_PANELS = "JSON/ComplexPanels";
        const string RESOURCES_PATH_PANEL_PREFABS = "Prefabs/UI Panels/";
        const string PANEL_PREFAB_DEFAULT = "panel_c_simple";

        // Contains localization keys and prefab to use for complex panels.
        // If no prefab is defined, will use the default simple panel.
        Dictionary<string, PanelModel> _panelModels = new Dictionary<string, PanelModel>(StringComparer.Ordinal);
        private List<GameObject> _activePanels = new List<GameObject>();

        /// <summary>
        /// True when complex panel configs have been loaded to the system
        /// </summary>
        private bool _isReady = false;
        /// <summary>
        /// Key identifier for overriding localization texts on the current platform.
        /// </summary>
        private string _platformOverride = null;

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
            // Set platform overrides depending on compiler flag
#if VRBUILD
            _platformOverride = "vr";
#endif
            
            LoadPanelModels();
        }

        /// <summary>
        /// Load UI panel configs from the resource json
        /// </summary>
        private void LoadPanelModels()
        {
            // Load UI panel configs
            string panelConfigsJson = Resources.Load<TextAsset>(RESOURCES_PATH_COMPLEX_PANELS).text;
            
            // Deserialize first as a JObject to retain the arbitrarily-named properties, then process each to
            // become a panel model.
            var jObjects = JsonConvert.DeserializeObject<JObject[]>(panelConfigsJson);
            foreach (var jObject in jObjects)
            {
                PanelModel model = new PanelModel(jObject);
                _panelModels[model.ID] = model;
            }

            _isReady = true;
        }

        private void Update()
        {
            // Deactivate the latest
            if (Keyboard.current[Key.Escape].wasPressedThisFrame
                && _activePanels.Count > 0)
            {
                DespawnPanel(_activePanels.LastOrDefault());
            }
        }

        /// <summary>
        /// Used to spawn a simple default panel that only has one text box for one localization key.
        /// </summary>
        /// <param name="localizationKey"></param>
        /// <param name="panelPrefabID">Prefab ID as named in Resources (without extension)</param>
        /// <param name="localVariables">Dictionary of dynamic {UnityEngine.Localization.SmartFormat.PersistentVariables}
        ///     to keep track of in localization</param>
        /// <returns>Instantiated panel game object</returns>
        private GameObject DoSpawnSimplePanel(string localizationKey, string panelPrefabID = PANEL_PREFAB_DEFAULT, Dictionary<string, IVariable> localVariables = null)
        {
            GameObject panelObject = Instantiate(Resources.Load($"{RESOURCES_PATH_PANEL_PREFABS}{panelPrefabID}")) as GameObject;
            
            if (panelObject != null)
            {
                // Localization keypaths
                if (panelObject.GetComponent<DataSourceGODictionary>() is { } dataDict)
                {
                    dataDict.SetValue($"text", localizationKey, true);
                }
                
                if (localVariables != null)
                {
                    // Add a data source for local variables if they are provided
                    DataSourceLocalizationVariables variablesSource = panelObject.AddComponent<DataSourceLocalizationVariables>();
                    variablesSource.Initialize(localVariables);
                }
                
                if (panelObject.GetComponent<Panel>() is {} panel)
                {
                    panel.ShowPanel();
                }
                
                _activePanels.Add(panelObject);
            }

            return panelObject;
        }
        
        /// <summary>
        /// Spawn a complex panel with an arbitrary amount of texts to localize. 
        /// </summary>
        /// <param name="panelID">ID as defined in the complex panels JSON</param>
        /// <param name="localVariables">Dictionary of dynamic {UnityEngine.Localization.SmartFormat.PersistentVariables}
        ///     to keep track of in localization</param>
        /// <returns>Instantiated panel game object</returns>
        private GameObject DoSpawnComplexPanel(string panelID, Dictionary<string, IVariable> localVariables = null)
        {
            DoSpawnComplexPanel(panelID, out var panelObject, out var _, localVariables);
            return panelObject;
        }

        /// <summary>
        /// Spawn a complex panel with an arbitrary amount of texts to localize.
        /// </summary>
        /// <param name="panelID">ID as defined in the complex panels JSON</param>
        /// <param name="panelObject">The instantiated gameobject instance</param>
        /// <param name="dataSource">DataSource component if there exists one</param>
        /// <param name="localVariables">Dictionary of dynamic {UnityEngine.Localization.SmartFormat.PersistentVariables}
        ///     to keep track of in localization</param>
        /// <returns>Success</returns>
        private bool DoSpawnComplexPanel(string panelID,
            out GameObject panelObject,
            out DataSourceGODictionary dataSource,
            Dictionary<string, IVariable> localVariables = null)
        {
            panelObject = null;
            dataSource = null;
            if (_panelModels.ContainsKey(panelID))
            {
                PanelModel model = _panelModels[panelID];
                string prefabID = model.PrefabPath ?? PANEL_PREFAB_DEFAULT;

                // Spawn desired prefab or default
                panelObject = Instantiate(Resources.Load($"{RESOURCES_PATH_PANEL_PREFABS}{prefabID}")) as GameObject;

                if (panelObject != null)
                {
                    dataSource = panelObject.GetComponent<DataSourceGODictionary>();
                    if (dataSource != null)
                    {
                        dataSource.DataChangeSetBegin();
                        // Get each property in the config json and add it to the GODIctionary (MRTK data binding)
                        foreach (var property in model.LocalizationKeys)
                        {
                            dataSource.SetValue(property.Key, property.Value);
                        }
    
                        // Apply platform-specific overrides
                        if (!string.IsNullOrEmpty(_platformOverride)
                            && model.PlatformOverrideKeys != null
                            && model.PlatformOverrideKeys.ContainsKey(_platformOverride)) 
                        {
                            foreach (var overrideProperty in model.PlatformOverrideKeys[_platformOverride])
                            {
                                dataSource.SetValue(overrideProperty.Key, overrideProperty.Value);
                            }
                        }

                        dataSource.DataChangeSetEnd();
                    }
                    
                    if (localVariables != null)
                    {
                        // Add a data source for local variables if they are provided
                        DataSourceLocalizationVariables variablesSource = panelObject.AddComponent<DataSourceLocalizationVariables>();
                        variablesSource.Initialize(localVariables);
                    }
                    
                    if (panelObject.GetComponent<Panel>() is {} panel)
                    {
                        panel.ShowPanel();
                    }
                    
                    _activePanels.Add(panelObject);
                    return true;
                }
            }
            Debug.LogError($"[{this.GetType().ToString()}] No panel configuration found with id: {panelID}");
            return false;
        }

        /// <summary>
        /// Display a simple dialog with a Yes option and callback
        /// </summary>
        /// <param name="dialogId"></param>
        /// <param name="onYesButtonPressedCallback"></param>
        /// <returns></returns>
        public GameObject DoDialogPrompt(string dialogId, Dictionary<string, System.Action> buttonCallbacksByID, Dictionary<string, IVariable> localVariables = null)
        {
            GameObject dialog = UISystem.SpawnComplexPanel(dialogId, localVariables);
            if (dialog == null)
            {
                Debug.LogError($"No dialog instance created");
            }

            var panel = dialog.GetComponent<Panel>();
            Action<string> buttonHandler = null;
            buttonHandler = (buttonId) =>
            {
                if (buttonCallbacksByID.TryGetValue(buttonId, out Action buttonCallback))
                {
                    buttonCallback?.Invoke();
                }

                panel.OnButtonPressedEvent -= buttonHandler;
                panel.ClosePanel();
            };
            panel.OnButtonPressedEvent += buttonHandler;
            return dialog;
        }

        #region Static Interface

        /// <summary>
        /// True when complex panel configs have been loaded to the system
        /// </summary>
        public static bool IsReady => _instance._isReady;

        /// <summary>
        /// Spawn a complex panel with an arbitrary amount of texts to localize. 
        /// </summary>
        /// <param name="panelID">ID as defined in the complex panels JSON</param>
        /// <param name="localVariables">Dictionary of dynamic {UnityEngine.Localization.SmartFormat.PersistentVariables}
        ///     to keep track of in localization</param>
        /// <returns>Success</returns>
        public static GameObject SpawnComplexPanel(string panelID, Dictionary<string, IVariable> localVariables = null)
        {
            return _instance.DoSpawnComplexPanel(panelID, localVariables);
        }

        /// <summary>
        /// Spawn a complex panel with an arbitrary amount of texts to localize. 
        /// </summary>
        /// <param name="panelID">ID as defined in the complex panels JSON</param>
        /// <param name="panelObject">The instantiated gameobject instance</param>
        /// <param name="dataSource">DataSource component if there exists one</param>
        /// <param name="localVariables">Dictionary of dynamic {UnityEngine.Localization.SmartFormat.PersistentVariables}
        ///     to keep track of in localization</param>
        /// <returns>Instantiated panel game object</returns>
        public static bool SpawnComplexPanel(string panelID,
            out GameObject panelObject,
            out DataSourceGODictionary dataSource,
            Dictionary<string, IVariable> localVariables = null
            )
        {
            return _instance.DoSpawnComplexPanel(panelID, out panelObject, out dataSource, localVariables);
        }

        /// <summary>
        /// Used to spawn a simple default panel that only has one text box for one localization key.
        /// </summary>
        /// <param name="localizationKey"></param>
        /// <param name="panelPrefabID">Prefab ID as named in Resources (without extension)</param>
        /// <param name="localVariables">Dictionary of dynamic {UnityEngine.Localization.SmartFormat.PersistentVariables}
        ///     to keep track of in localization</param>
        /// <returns>Instantiated panel game object</returns>
        public static GameObject SpawnSimplePanel(string localizationKey, string panelPrefabID = PANEL_PREFAB_DEFAULT,
            Dictionary<string, IVariable> localVariables = null)
        {
            return _instance.DoSpawnSimplePanel(localizationKey, panelPrefabID, localVariables);
        }

        /// <summary>
        /// Destroys a panel
        /// </summary>
        /// <param name="panelObject"></param>
        public static void DespawnPanel(GameObject panelObject)
        {
            System.Action cleanupAction = () =>
            {
                _instance._activePanels.Remove(panelObject);
                GameObject.Destroy(panelObject);
            };
            if (panelObject != null)
            {
                if (panelObject.activeInHierarchy == false)
                {
                    cleanupAction.Invoke();
                }
                else
                {
                    Panel panel = panelObject.GetComponent<Panel>();
                    if (panel == null)
                    {
                        cleanupAction.Invoke();
                    }
                    else
                    {
                        panel.ClosePanel(cleanupAction);
                    }
                }
            }
        }

        /// <summary>
        /// Destroys all active panels that the system is tracking
        /// </summary>
        public static void DespawnAllActivePanels()
        {
            for (int i = _instance._activePanels.Count - 1; i >= 0; --i)
            {
                DespawnPanel(_instance._activePanels[i]);
            }
        }

        /// <summary>
        /// Used to spawn a panel with an assumed Yes button ID to be raised as an event
        /// </summary>
        /// <param name="dialogId"></param>
        /// <param name="onYesButtonPressedCallback"></param>
        public static GameObject SpawnDialogPrompt(string dialogId, System.Action onYesButtonPressedCallback, Dictionary<string, IVariable> localVariables = null)
        {
            return _instance.DoDialogPrompt(dialogId, new Dictionary<string, Action>() {{"Yes", onYesButtonPressedCallback}}, localVariables);
        }

        /// <summary>
        /// Used to spawn a panel with assumed Yes and No button ID to be raised as an event
        /// </summary>
        /// <param name="dialogId"></param>
        /// <param name="onYesButtonPressedCallback"></param>
        /// /// <param name="onNoButtonPressedCallback"></param>
        public static GameObject SpawnDialogPrompt(string dialogId, System.Action onYesButtonPressedCallback, System.Action onNoButtonPressedCallback, Dictionary<string, IVariable> localVariables = null)
        {
            return _instance.DoDialogPrompt(dialogId, new Dictionary<string, Action>() { { "Yes", onYesButtonPressedCallback }, { "No", onNoButtonPressedCallback } }, localVariables);
        }

        /// <summary>
        /// Used to spawn a panel with an assumed button IDs to be raised as events
        /// </summary>
        /// <param name="dialogId"></param>
        /// <param name="onYesButtonPressedCallback"></param>
        public static GameObject SpawnDialogPrompt(string dialogId,
            Dictionary<string, System.Action> buttonCallbacksByID)
        {
            return _instance.DoDialogPrompt(dialogId, buttonCallbacksByID);
        }

        #endregion
    }
}