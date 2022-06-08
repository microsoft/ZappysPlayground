// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Data;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using MSPlayground.Common;
using MSPlayground.Common.Helper;
using MSPlayground.Core;
using MSPlayground.Core.Gamestate;
using MSPlayground.Core.Scenario;
using MSPlayground.Core.Spatial;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines;
using MSPlayground.Turbines.Events;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// Manages the turbine scenario and common references
    /// </summary>
    public class TurbineScenarioResources : MonoBehaviour
    {
        const string HUB_SCENE_NAME = "Hub";

        const string DEBUG_RESET_SCENARIO = "Scenario/Reset To Beginning";
        const string DEBUG_RESTART_EXPERIENCE = "debug_restart_experience";
        const string DEBUG_DEVELOPER_OPTIONS = "debug_developer_options";
        const string QUIT_RESET_SPATIAL_ANCHORS = "quit_to_reset_spatial_anchors";
        const string QUIT_RESCAN_SPACE = "quit_to_rescan_space";
        const string QUIT_LEAVE_EXPERIENCE = "quit_to_leave_experience";

        #region Feature List
#if !VRBUILD
        public static string[] FEATURE_KEYS = new[]
        {
            "data_binding",
            "hand_constraint",
            "directional_arrow",
            "gaze",
            "scene_understanding",
            "surface_magnetism",
            "spatial_audio",
            "spatial_anchors",
            "follow",
            "near_and_far",
            "ux_controls",
            "bounds_controls"
        };
#else
        public static string[] FEATURE_KEYS = new[]
        {
            "data_binding",
            "directional_arrow",
            "gaze",
            "surface_magnetism",
            "spatial_audio",
            "follow",
            "near_and_far",
            "ux_controls",
            "bounds_controls"
        };
#endif
        #endregion

        [SerializeField] ScenarioManager _scenarioManager;
        [SerializeField] string _vrEnvironmentScene;
        [SerializeField] GameObject _robot;
        [SerializeField] WindfarmController _windfarmController;

        [SerializeField] TurbineController[] _turbines;
        [SerializeField] GameObject[] _turbineDocks;
        [SerializeField] WindowController _windowController;
        [SerializeField] Transform _robotDialogBone;
        [SerializeField] Transform _controlDialogBone;
        [SerializeField] DirectionalIndicator _directionalIndicator;
        [SerializeField] PowerPanel _powerPanel;
        [SerializeField] GameObject _scenePortal;
        [SerializeField] GameObject _windowPlacementGuide;
        [SerializeField] GameObject _platformPlacementGuide;
        [SerializeField] AudioSource _platformAudioSource;
        [SerializeField] Vector2 _knockoffForce = new Vector2(5f, 15f);

        GamestateManager _gamestateManager;
        Vector3[] _turbineInitalPositions;
        GameObject _player;
        GameObject _pendingDebugDialogPrompt;
        TurbinesVRRoom _vrRoom;

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

        public GameObject Player => _player;
        public GameObject Robot => _robot;
        public GameObject PlatformGameObject => _windfarmController.gameObject;
        public TurbineController[] Turbines => _turbines;
        public GameObject[] TurbineDocks => _turbineDocks;
        public WindowController WindowController => _windowController;
        public PowerPanel PowerPanel => _powerPanel;
        public WindfarmController WindfarmController => _windfarmController;
        public GameObject WindowPlacementGuide => _windowPlacementGuide;
        public GameObject PlatformPlacementGuide => _platformPlacementGuide;
        public AudioSource PlatformAudioSource => _platformAudioSource;
        public Transform ControlDialogBone => _controlDialogBone;
        public float FloorHeight { get; private set; }

        IEnumerator Start()
        {
            _player = Camera.main.gameObject;

            Debug.Assert(_turbineDocks.Length <= _turbines.Length,
                "There are not enough turbines for the number of docks");

            if (SceneNavigator.LoadVREnv)
            {
                SceneManager.LoadScene(_vrEnvironmentScene, LoadSceneMode.Additive);
                yield return null;
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(_vrEnvironmentScene));
                GameObject envRoot = GameObject.Find("TurbinesEnvironment");
                _vrRoom = envRoot.GetComponent<TurbinesVRRoom>();
                CalculateInitialTurbinePositions_VR();
            }
            else
            {
                CalculateInitialTurbinePositions_AR();
            }

            _gamestateManager = GameObject.FindObjectOfType<GamestateManager>();

            GlobalEventSystem.Register<DroppedRepairModuleEvent>(OnDroppedRepairModule);

            HandMenu handMenu = HandMenu.Instance;
            handMenu.SettingsPanel.ThemeGroup.SetActive(true);
            handMenu.SettingsPanel.LanguageThemeGroup.SetActive(true);

            DynamicHandSubMenu debugMenu = handMenu.DebugMenu;
            debugMenu.CreateButton(DEBUG_RESTART_EXPERIENCE, "UI/restart_experience", PromptRestartExperience);
            debugMenu.CreateButton(DEBUG_DEVELOPER_OPTIONS, "UI/developer_options", ShowDeveloperDebugOptions);

            DynamicHandSubMenu doorMenu = handMenu.DoorMenu;
            doorMenu.CreateButton(QUIT_RESCAN_SPACE, "UI/rescan_space", PromptRescanSpace);
            doorMenu.CreateButton(QUIT_LEAVE_EXPERIENCE, "UI/return_to_hub", PromptReturnToHub);

            DebugMenu.AddButton(DEBUG_RESET_SCENARIO, ResetToBeginning);
        }

        private void CalculateInitialTurbinePositions_VR()
        {
            _turbineInitalPositions = new Vector3[_turbines.Length];

            _turbineInitalPositions[0] = _vrRoom.Turbine0.position;
            _turbineInitalPositions[1] = _vrRoom.Turbine1.position;
            _turbineInitalPositions[2] = _vrRoom.Turbine2.position;
            _turbineInitalPositions[3] = _vrRoom.Turbine3.position;
            _turbineInitalPositions[4] = _vrRoom.Turbine4.position;
        }

        private void CalculateInitialTurbinePositions_AR()
        {
            Vector3 center = VirtualRoom.RoomCenter.position;
            Vector3 bounds = VirtualRoom.CalculateRoomSize();
            float radius = Mathf.Min(bounds.x, bounds.z);
            radius *= 0.5f;

            VirtualRoom.EnablePhysics(true, true, true, false);
            Physics.SyncTransforms();

            // Track initial turbine positions
            _turbineInitalPositions = new Vector3[_turbines.Length];
            for (int i = 0; i < _turbines.Length; ++i)
            {
                Vector3 turbinePos = MathHelpers.Vector3AtYPos(center + new Vector3(
                    UnityEngine.Random.Range(-radius, radius),
                    0,
                    UnityEngine.Random.Range(-radius, radius)), VirtualRoom.Floor.transform.position.y + 1.5f);

                // stay inside the room
                if (VirtualRoom.RaycastAgainstRoom(center, (turbinePos - center).normalized,
                        (turbinePos - center).magnitude + 0.5f, out RaycastHit hit))
                {
                    i--;
                }
                else
                {
                    _turbineInitalPositions[i] = turbinePos;
                }
            }

            VirtualRoom.EnablePhysics(true, false, false, true);
            Physics.SyncTransforms();
        }

        /// <summary>
        /// Generates random positions for the turbines to fall from and places them there
        /// </summary>
        [ContextMenu(nameof(SetupTurbineLocations))]
        public void SetupTurbineLocations()
        {
            if (SceneNavigator.LoadVREnv)
            {
                CalculateInitialTurbinePositions_VR();
            }
            else
            {
                CalculateInitialTurbinePositions_AR();
            }

            for (int i = 0; i < _turbineInitalPositions.Length; ++i)
            {
                TurbineController turbine = _turbines[i];
                // The mass or HingeJoint needs to be reset as well with the new position
                turbine.EnableMass(false);
                turbine.transform.position = _turbineInitalPositions[i];
                turbine.EnableMass(true);
            }
        }

        private void OnDestroy()
        {
            DynamicHandSubMenu debugMenu = HandMenu.Instance.DebugMenu;
            debugMenu.DestroyButton(DEBUG_RESTART_EXPERIENCE);
            debugMenu.DestroyButton(DEBUG_DEVELOPER_OPTIONS);

            DynamicHandSubMenu doorMenu = HandMenu.Instance.DoorMenu;
            doorMenu.DestroyButton(QUIT_RESET_SPATIAL_ANCHORS);
            doorMenu.DestroyButton(QUIT_RESCAN_SPACE);
            doorMenu.DestroyButton(QUIT_LEAVE_EXPERIENCE);

            GlobalEventSystem.Unregister<DroppedRepairModuleEvent>(OnDroppedRepairModule);

            DebugMenu.RemoveButton(DEBUG_RESET_SCENARIO);
        }

        void InitializeEnvironment()
        {
            if (SceneNavigator.LoadVREnv)
            {
                InitializeEnvironment_VR();
            }
            else
            {
                InitializeEnvironment_AR();
            }
        }

        void InitializeEnvironment_AR()
        {
            Transform roomCenter = VirtualRoom.RoomCenter;

            float floorHeight = VirtualRoom.Floor.transform.position.y;
            FloorHeight = floorHeight;

            // enable wall physics only
            VirtualRoom.EnablePhysics(true, false, false, false);
            Physics.SyncTransforms();

            Transform platformPlacementGuideTransform = _platformPlacementGuide.transform;
            Transform windowPlacementGuideTransform = _windowPlacementGuide.transform;

            // platform offset based off of how far off the ground it floats
            float platformYPos = VirtualRoom.Floor.transform.position.y + 0.85f;
            Vector3 platformHalfExtents = (platformPlacementGuideTransform.GetComponent<BoxCollider>().size * 0.5f);

            // place the window position at center of longest wall
            Transform longestWall = VirtualRoom.Walls[0].transform;
            windowPlacementGuideTransform.position =
                MathHelpers.Vector3AtYPos(longestWall.position, floorHeight + 1.35f);
            windowPlacementGuideTransform.rotation = longestWall.rotation;
            platformPlacementGuideTransform.rotation = Quaternion.LookRotation(-windowPlacementGuideTransform.forward);
            platformPlacementGuideTransform.position = MathHelpers.Vector3AtYPos(longestWall.position, platformYPos) +
                                                       platformPlacementGuideTransform.forward * (platformHalfExtents.z + 0.2f) +
                                                       Vector3.up * platformHalfExtents.y;

            // robot to right side of the window
            _robot.transform.position = MathHelpers.Vector3AtYPos(platformPlacementGuideTransform.transform.position +
                platformPlacementGuideTransform.transform.right * 0.9f +
                platformPlacementGuideTransform.transform.forward * 0.2f,
                floorHeight);
            _robot.transform.eulerAngles = new Vector3(0, platformPlacementGuideTransform.eulerAngles.y -45f, 0);

            // initial placement of the window to the left
            bool rayHit = VirtualRoom.RaycastAgainstRoom(roomCenter.position, -roomCenter.right, 20f,
                out RaycastHit raycastHit);
            Debug.Assert(rayHit, "Raycast missed VirtualRoom");
            if (rayHit)
            {
                _windowController.transform.position =
                    MathHelpers.Vector3AtYPos(raycastHit.transform.position, floorHeight + 1.35f);
                _windowController.transform.rotation = Quaternion.LookRotation(-raycastHit.normal, Vector3.up);
            }

            // initial placement windfarm of to the left
            rayHit = VirtualRoom.BoxcastAgainstRoom(roomCenter.position, platformHalfExtents, -roomCenter.right,
                Quaternion.LookRotation(-roomCenter.right), 20f, out raycastHit);
            if (rayHit)
            {
                Vector3 collisionPt = roomCenter.position - roomCenter.right * raycastHit.distance;
                _windfarmController.transform.position = MathHelpers.Vector3AtYPos(collisionPt, platformYPos);
                _windfarmController.transform.rotation = Quaternion.LookRotation(roomCenter.right, Vector3.up);
            }

            // disable physics except floor and platforms
            VirtualRoom.EnablePhysics(false, true, false, true);
            Physics.SyncTransforms();

            // Set walls to interact with the Window's Clipping Primitive
            foreach (VirtualWall virtualWall in VirtualRoom.Walls)
            {
                _windowController.SetWallClipping(virtualWall.Renderer, addToList: true);
            }
        }

        void InitializeEnvironment_VR()
        {
            // set all object transforms from the vrRoom
            _windfarmController.transform.SetPositionAndRotation(_vrRoom.Windfarm.position, _vrRoom.Windfarm.rotation);
            _platformPlacementGuide.transform.SetPositionAndRotation(_vrRoom.WindfarmGuide.position, _vrRoom.WindfarmGuide.rotation);
            _windowController.transform.SetPositionAndRotation(_vrRoom.Window.position, _vrRoom.Window.rotation);
            _windowPlacementGuide.transform.SetPositionAndRotation(_vrRoom.WindowGuide.position, _vrRoom.WindowGuide.rotation);
            _robot.transform.SetPositionAndRotation(_vrRoom.Robot.position, _vrRoom.Robot.rotation);

            // Set walls to interact with the Window's Clipping Primitive
            foreach (Renderer wallRenderer in _vrRoom.WallRenderers)
            {
                _windowController.SetWallClipping(wallRenderer, addToList: true);
            }
        }

        /// <summary>
        /// Setup the environment state to be ready for the scenario experience from the beginning
        /// </summary>
        public void ResetEnvironmentToBeginning()
        {
            InitializeEnvironment();

            WindowController.gameObject.SetActive(false);
            WindfarmController.gameObject.SetActive(false);
            WindowPlacementGuide.SetActive(false);
            PlatformPlacementGuide.SetActive(false);

            UISystem.DespawnAllActivePanels();
            _robot.SetActive(true);
            PlatformGameObject.SetActive(false);

            for (int i = 0; i < _turbineInitalPositions.Length; ++i)
            {
                var turbine = _turbines[i];
                turbine.ForceUndock();
                turbine.RepairNacelle();
                turbine.RepairRotor();
                turbine.RepairTower();
                turbine.EnableTurbineMaintenanceMenu(false);
                turbine.transform.position = _turbineInitalPositions[i];
                turbine.EnableBoundsControl(false);
                turbine.gameObject.SetActive(false);
            }

            FocusOnObject(null);
        }

        /// <summary>
        /// Show or hide all turbine instances
        /// </summary>
        /// <param name="show">Show state</param>
        public void ShowTurbines(bool show)
        {
            foreach (var turbine in Turbines)
            {
                turbine.gameObject.SetActive(show);
                turbine.ShowTurbine(show);
            }
        }
        
        /// <summary>
        /// Mute or unmute all turbine instances
        /// </summary>
        /// <param name="show">Show state</param>
        public void MuteTurbines(bool isMuted)
        {
            foreach (var turbine in Turbines)
            {
                turbine.MuteAudioSource(isMuted);
            }
        }

        /// <summary>
        /// Show or hide the docking platform
        /// </summary>
        /// <param name="show">Show state</param>
        public void ShowPlatform(bool show)
        {
            PlatformGameObject.SetActive(show);
        }

        /// <summary>
        /// Displays UI adjusted to fit the robot's dialog bone
        /// </summary>
        /// <param name="panelId"></param>
        /// <returns></returns>
        public GameObject ShowRobotDialog(string panelId)
        {
            return ShowRobotDialog(panelId, _robotDialogBone);
        }

        /// <summary>
        /// Displays UI adjusted to fit a given bone
        /// </summary>
        /// <param name="panelId"></param>
        /// <returns></returns>
        public GameObject ShowRobotDialog(string panelId, Transform bone, Dictionary<string, IVariable> localVariables = null)
        {
            if (UISystem.SpawnComplexPanel(panelId, out GameObject dialogGameObject,
                    out DataSourceGODictionary dataSource, localVariables))
            {
                Transform dialogTransform = dialogGameObject.transform;
                dialogTransform.SetParent(bone);
                dialogTransform.localPosition = Vector3.zero;
                dialogTransform.localEulerAngles = Vector3.zero;
            }

            return dialogGameObject;
        }

        /// <summary>
        /// Hide instance of robot UI dialog that is being displayed
        /// </summary>
        /// <param name="dialog"></param>
        public void HideRobotDialog(GameObject dialog)
        {
            if (dialog != null)
            {
                UISystem.DespawnPanel(dialog);
            }
        }

        /// <summary>
        /// Resets the entire scenario and the environment
        /// </summary>
        [ContextMenu(nameof(ResetToBeginning))]
        public void ResetToBeginning()
        {
            SceneNavigator.GoToScene("Turbines");
        }

        /// <summary>
        /// Sets the user to focus on the target object
        /// </summary>
        /// <param name="target">Transform to set focus on</param>
        public void FocusOnObject(Transform target)
        {
            _directionalIndicator.DirectionalTarget = target;
            _directionalIndicator.gameObject.SetActive(target != null);
        }

        /// <summary>
        /// Attempts to retrieve a dock gameobject from our managed array of dock instances
        /// </summary>
        /// <param name="index">Index into the array</param>
        /// <param name="dock">Out parameter of dock instance</param>
        /// <returns>Success</returns>
        public bool TryGetDockAtIndex(int index, out GameObject dock)
        {
            dock = null;
            if (index < _turbineDocks.Length)
            {
                dock = _turbineDocks[index];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to return the turbine instance that is docked to the target 
        /// </summary>
        /// <param name="target">Dock object</param>
        /// <param name="turbineInstance">Out parameter of turbine teturned</param>
        /// <returns>Success</returns>
        public bool TryGetTurbineOnDock(GameObject target, out TurbineController turbineInstance)
        {
            turbineInstance = null;
            Transform dockTransform = target.transform;
            for (int i = 0; i < _turbines.Length; ++i)
            {
                if (_turbines[i].DockTransform == dockTransform)
                {
                    turbineInstance = _turbines[i];
                    return true;
                }
            }

            return false;
        }

        private void OnDroppedRepairModule(DroppedRepairModuleEvent eventData)
        {
            const float TURBINE_DROP_RADIUS_SQR = 0.3f * 0.3f;
            Transform moduleObjectTransform = eventData.ModuleObject.transform;
            foreach (var turbine in _turbines)
            {
                if (turbine.IsDocked == false)
                {
                    continue;
                }

                if (turbine.IsBroken == false)
                {
                    continue;
                }

                // Dropped onto a turbine
                if (Vector3.SqrMagnitude(turbine.transform.position - moduleObjectTransform.position) <
                    TURBINE_DROP_RADIUS_SQR)
                {
                    if (turbine.TryRepairTurbine(eventData.ModuleType))
                    {
                        break;
                    }
                }
            }
        }

        private bool TryShowPrompt(string dialogPromptId, System.Action onYesButtonPressed)
        {
            if (_pendingDebugDialogPrompt != null && _pendingDebugDialogPrompt.activeInHierarchy)
            {
                // Cannot display more than one prompt at a time
                return false;
            }

            _pendingDebugDialogPrompt = UISystem.SpawnDialogPrompt(dialogPromptId, () =>
            {
                onYesButtonPressed?.Invoke();
                _pendingDebugDialogPrompt = null;
            });
            return true;
        }

        private void PromptRestartExperience()
        {
            TryShowPrompt("dialog_restart_experience_confirmation", () =>
            {
                SceneNavigator.GoToScene("Turbines");
                HandMenu.Instance.DebugMenu.gameObject.SetActive(false);
            });
        }

        private void ShowDeveloperDebugOptions()
        {
            HandMenu.Instance.ToggleDeveloperMenuPanel();
        }

        private void PromptRescanSpace()
        {
            TryShowPrompt("dialog_rescan_your_space", () =>
            {
                var gamestateManager = GameObject.FindObjectOfType<GamestateManager>();
                gamestateManager.ClearAllGamestates();
                PlayerPrefs.DeleteKey("AnchorId");
                PlayerPrefs.Save();
                GlobalEventSystem.Fire(new WillLoadNewSceneEvent() {SceneToLoad = HUB_SCENE_NAME});
                SceneManager.LoadScene(HUB_SCENE_NAME, LoadSceneMode.Single);
            });
        }

        private void PromptReturnToHub()
        {
            TryShowPrompt("are_you_sure_you_want_to_leave", () =>
            {
                GlobalEventSystem.Fire(new WillLoadNewSceneEvent() {SceneToLoad = HUB_SCENE_NAME});
                SceneManager.LoadScene(HUB_SCENE_NAME, LoadSceneMode.Single);
            });
        }

        /// <summary>
        /// Knock the turbines off the platform
        /// </summary>
        public void KnockOffTurbines()
        {
            _powerPanel.KnockOffTurbines();

            Vector3 force = MathHelpers.Vector3AtYPos(_windfarmController.transform.forward * _knockoffForce.x, _knockoffForce.y);

            foreach (TurbineController turbineController in Turbines)
            {
                turbineController.KnockOffWindfarm(force);
            }
        }
    }
}