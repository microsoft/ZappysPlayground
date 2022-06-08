
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Data;
using MSPlayground.Core;
using MSPlayground.Core.Gamestate;
using MSPlayground.Core.Spatial;
using MSPlayground.Core.Utils;
using System.Collections;
using System.Collections.Generic;
using MSPlayground.Common;
using MSPlayground.Core.Scenario;
using MSPlayground.Core.UI;
using MSPlayground.Hub;
using MSPlayground.Turbines;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace MSPlayground.Scenarios.Hub
{
    /// <summary>
    /// Shared resources and code for the Hub scene
    /// </summary>
    public class HubScenarioResources : MonoBehaviour
    {
        private const string DEBUG_RESET_SPATIAL_ANCHORS = "debug_reset_spatial_anchors";
        private const string DEBUG_DEVELOPER_OPTIONS = "debug_developer_options";
        private const string QUIT_OUT_OF_APP = "quit_app";

        [SerializeField] private ScenarioManager _scenarioManager;
        [SerializeField] string _vrEnvironmentScene;
        [SerializeField] private HubScenario_Base _rescanScenario;
        [SerializeField] private GameObject _turbinePortal;
        AudioSource _asaAudioSource;
        
        private GameObject _quitAppPopup = null;
        private HubVRRoom _vrRoom;

        private VirtualRoom _virtualRoom;
        private VirtualRoom VirtualRoom
        {
            get
            {
                if (_virtualRoom == null)
                {
                    _virtualRoom = GameObject.FindObjectOfType<RoomGenerator>().VirtualRoom;
                }
                return _virtualRoom;
            }
        }

        public RoomGenerator RoomGenerator { get; private set; }
        public GamestateManager GamestateManager { get; private set; }
        public string[] GamestateIds { get; private set; }
        public bool FailedToFindAnchors { get; set; } = false;

        public bool AreGamestatesEmpty {  get { return GamestateIds == null || GamestateIds.Length == 0; } }
        
        IEnumerator Start()
        {
            if (SceneNavigator.LoadVREnv)
            {
                SceneManager.LoadScene(_vrEnvironmentScene, LoadSceneMode.Additive);
                yield return null;
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(_vrEnvironmentScene));
            }

            _asaAudioSource = GetComponent<AudioSource>();
            RoomGenerator = GameObject.FindObjectOfType<RoomGenerator>();
            GamestateManager = GameObject.FindObjectOfType<GamestateManager>();

            HandMenu handMenu = HandMenu.Instance;
            handMenu.SettingsPanel.ThemeGroup.SetActive(true);
            handMenu.SettingsPanel.LanguageThemeGroup.SetActive(false);

            DynamicHandSubMenu debugMenu = handMenu.DebugMenu;
            debugMenu.CreateButton(DEBUG_RESET_SPATIAL_ANCHORS, "UI/rescan_space", PromptResetSpatialAnchors);
            debugMenu.CreateButton(DEBUG_DEVELOPER_OPTIONS, "UI/developer_options", ShowDeveloperDebugOptions);
            
            DynamicHandSubMenu doorMenu = handMenu.DoorMenu;
            doorMenu.CreateButton(QUIT_OUT_OF_APP, "UI/quit_app", PromptQuitApp);

            GamestateIds = GamestateManager.EnumerateGameStates();

            PropagateUsernameToLoc();
        }

        private void OnDestroy()
        {
            DynamicHandSubMenu debugMenu = HandMenu.Instance.DebugMenu;
            debugMenu.DestroyButton(DEBUG_RESET_SPATIAL_ANCHORS);
            debugMenu.DestroyButton(DEBUG_DEVELOPER_OPTIONS);
            
            DynamicHandSubMenu doorMenu = HandMenu.Instance.DoorMenu;
            doorMenu.DestroyButton(QUIT_OUT_OF_APP);
            
            DebugMenu.RemoveButton("ASA/Reset Anchor");

            if (_quitAppPopup)
            {
                UISystem.DespawnPanel(_quitAppPopup);
                _quitAppPopup = null;
            }
        }

        public void InitializeEnvironment()
        {
#if VRBUILD
            GameObject env = GameObject.Find("HubEnvironment");
            HubVRRoom vrRoom = env.GetComponent<HubVRRoom>();
            InitializeEnvironment_VR(vrRoom);
#else
            // place portal along longest wall
            Transform longestWall = VirtualRoom.Walls[0].transform;
            _turbinePortal.transform.SetPositionAndRotation(
                longestWall.transform.position - longestWall.transform.right * 0.5f - longestWall.transform.forward * 0.25f,
                longestWall.transform.rotation);
#endif
        }

        void InitializeEnvironment_VR(HubVRRoom vrRoom)
        {
            _turbinePortal.transform.SetPositionAndRotation(vrRoom.TurbinesPortal.transform.position, vrRoom.TurbinesPortal.transform.rotation);
        }

        public GameObject ShowDialog(string panelId, bool rotateToCamera = false)
        {
            if (UISystem.SpawnComplexPanel(panelId, out GameObject dialogGameObject, out DataSourceGODictionary dataSource))
            {
                Transform dialogTransform = dialogGameObject.transform;
                Transform cameraTransform = Camera.main.transform;
                dialogTransform.position = MathHelpers.Vector3AtYPos(cameraTransform.position + cameraTransform.forward * 1.0f, cameraTransform.position.y);
                if (rotateToCamera)
                {
                    Vector3 lookAtVector = dialogTransform.position - cameraTransform.position;
                    if (lookAtVector != Vector3.zero)
                    {
                        dialogTransform.rotation = Quaternion.LookRotation(lookAtVector);
                    }
                }
            }
            return dialogGameObject;
        }

        /// <summary>
        /// Propagates the username from player settings to the localization global variable
        /// </summary>
        private void PropagateUsernameToLoc()
        {
            if (PlayerPrefs.HasKey(NameEntryController.PLAYER_PREF_USERNAME))
            {
                var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
                StringVariable nameVariable = source["global"]["username"] as StringVariable;
                nameVariable.Value = PlayerPrefs.GetString(NameEntryController.PLAYER_PREF_USERNAME);
            }
        }
        
        /// <summary>
        /// Reset the username in localization settings to default, and clear the PlayerPref.
        /// AG Note: This is currently not in use, just putting it here in case we want this functionality
        /// without the name entry UI panel. If we end up using this, it might be worth caching the name string variable
        /// to HubScenarioResources so we don't have to get it again.
        /// </summary>
        public void ResetUsername()
        {
            PlayerPrefs.DeleteKey(NameEntryController.PLAYER_PREF_USERNAME);
            PlayerPrefs.Save();
            
            // Update the localization value
            var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
            StringVariable nameVariable = source["global"]["username"] as StringVariable;
            nameVariable.Value = NameEntryController.DEFAULT_USERNAME;
        }

        private void PromptResetSpatialAnchors()
        {
            UISystem.SpawnDialogPrompt("dialog_reset_spatial_anchors_confirmation", () =>
            {
                var gamestateManager = GameObject.FindObjectOfType<GamestateManager>();
                gamestateManager.DeleteActiveGamestate();
                
                _scenarioManager.ChangeScenario(_rescanScenario);
            });
        }
        
        private void ShowDeveloperDebugOptions()
        {
            HandMenu.Instance.ToggleDeveloperMenuPanel();
        }

        private void PromptQuitApp()
        {
            if (!_quitAppPopup)
            {
                _quitAppPopup = UISystem.SpawnDialogPrompt("are_you_sure_you_want_to_quit", Application.Quit);
                // Destroy panel object on close so we only spawn the popup as needed
                Panel panelComponent = _quitAppPopup.GetComponent<Panel>();
                if (panelComponent)
                {
                    panelComponent.OnPanelClosed += () =>
                    {
                        UISystem.DespawnPanel(_quitAppPopup);
                        _quitAppPopup = null;
                    };
                }
            }
        }

        /// <summary>
        /// Play SFX for ASA events
        /// </summary>
        public void PlayASASfx()
        {
            _asaAudioSource?.Play();
        }
    }
}
