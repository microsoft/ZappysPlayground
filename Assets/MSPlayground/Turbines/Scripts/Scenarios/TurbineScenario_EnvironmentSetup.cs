// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using MSPlayground.Core;
using MSPlayground.Core.Spatial;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines;
using MSPlayground.Turbines.Events;
using UnityEngine;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// Turbine scenario script to help setup the environment for the player
    /// </summary>
    public class TurbineScenario_EnvironmentSetup : TurbineScenario_Base
    {
        const string DIALOG_ENVIRONMENT_SETUP_STEP_1 = "tutorial_dialog_environment_setup_1";
        const string DIALOG_ENVIRONMENT_SETUP_STEP_2 = "tutorial_dialog_environment_setup_2";
        
        [SerializeField] private float _delayBetweenSteps = 1.0f;
        [SerializeField] private Vector3 _displayTargetOffset = Vector3.forward;
        [Header("Audio")]
        [SerializeField] private AudioSource _windowAudioSource = null;
        [SerializeField] private AudioClip _windowSetSuccessSFX = null;
        [SerializeField] private AudioClip _platformSetSuccessSFX = null;

        private bool _windowPlaced = false;
        private bool _platformPlaced = false;
        [Tooltip("The index of the wall that user chose to place the window and windfarm on")]
        private int _windowWallIndex = 0;
        
        public override void EnterState()
        {
            base.EnterState();

            _windowPlaced = false;
            _platformPlaced = false;

            GlobalEventSystem.Register<PlatformManipulationEvent>(OnPlatformPlacedEvent);
            GlobalEventSystem.Register<WindowManipulationEvent>(OnWindowPlacedEvent);
            StartCoroutine(SequenceRoutine());
        }

        public override void ExitState()
        {
            GlobalEventSystem.Unregister<WindowManipulationEvent>(OnWindowPlacedEvent);
            GlobalEventSystem.Unregister<PlatformManipulationEvent>(OnPlatformPlacedEvent);
            base.ExitState();
        }

        public override void SkipState()
        {
            _scenarioResources.Robot.SetActive(true);
            _scenarioResources.WindowController.gameObject.SetActive(true);
            _scenarioResources.WindowController.EnablePlacement(false);
            _scenarioResources.ToggleWindowPlacementGuidesVisibility(false);
            // Default to the first window placement guide
            _scenarioResources.WindowController.transform.position =  _scenarioResources.WindowPlacementGuides[0].transform.position;
            _scenarioResources.WindowController.transform.rotation = _scenarioResources.WindowPlacementGuides[0].transform.rotation;

            _scenarioResources.WindfarmController.gameObject.SetActive(true);
            _scenarioResources.WindfarmController.EnablePlacement(false);
            _scenarioResources.PlatformPlacementGuide.SetActive(false);
#if !VRBUILD
            // Reorient the windfarm to the default window position
            // If in VR, this is not necessary as the object positions are fixed.
            _scenarioResources.PlaceWindfarmGuide(_scenarioResources.VirtualRoom.Walls[0].transform);
#endif
            _scenarioResources.WindfarmController.transform.position = _scenarioResources.PlatformPlacementGuide.transform.position;
            _scenarioResources.WindfarmController.transform.rotation = _scenarioResources.PlatformPlacementGuide.transform.rotation;

            _scenarioResources.FocusOnObject(null);
            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }

        IEnumerator SequenceRoutine()
        {
            VirtualRoom virtualRoom = GameObject.FindObjectOfType<RoomGenerator>().VirtualRoom;
            _scenarioResources.WindowController.gameObject.SetActive(false);
            _scenarioResources.WindfarmController.gameObject.SetActive(false);
            _scenarioResources.ToggleWindowPlacementGuidesVisibility(false);
            _scenarioResources.PlatformPlacementGuide.SetActive(false);
            
            virtualRoom?.EnablePhysics(true, true, true, true);

            yield return WaitForUserToPlaceWindow();

#if !VRBUILD
            PlaceObjectsRelativeToChosenWindowPlacement();
#endif            
            
            yield return WaitForUserToPlacePlatform();
            
            virtualRoom?.EnablePhysics(true, true, false, true);
            _scenarioResources.Robot.SetActive(true);

            _scenarioResources.FocusOnObject(null);
            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }

        private IEnumerator WaitForUserToPlaceWindow()
        {
            WindowController windowController = _scenarioResources.WindowController;
            GameObject windowControllerGO = windowController.gameObject;
            Transform windowControllerTransform = windowController.transform;
            
            GameObject dialog = UISystem.SpawnComplexPanel(DIALOG_ENVIRONMENT_SETUP_STEP_1);
            yield return new WaitForSeconds(_delayBetweenSteps);
            windowControllerGO.SetActive(true);
            windowController.EnablePlacement(true);
            _scenarioResources.FocusOnObject(windowControllerTransform);
                       
            _scenarioResources.ToggleWindowPlacementGuidesVisibility(true);
            do
            {
                // Wait for the user to place the window into the correct spot
                yield return null;
            } while (_windowPlaced == false);
            _scenarioResources.ToggleWindowPlacementGuidesVisibility(false);
            windowController.EnablePlacement(false);
            UISystem.DespawnPanel(dialog);
            _windowAudioSource.PlayOneShot(_windowSetSuccessSFX);
        }

        private IEnumerator WaitForUserToPlacePlatform()
        {
            WindfarmController windfarmController = _scenarioResources.WindfarmController;
            Transform windfarmControllerTransform = windfarmController.transform;

            GameObject dialog = UISystem.SpawnComplexPanel(DIALOG_ENVIRONMENT_SETUP_STEP_2);
            yield return new WaitForSeconds(_delayBetweenSteps);
            windfarmController.gameObject.SetActive(true);
            windfarmController.EnablePlacement(true);
            _scenarioResources.FocusOnObject(windfarmControllerTransform);
            
            _scenarioResources.PlatformPlacementGuide.SetActive(true);
            
            do
            {
                // Wait for the user to place the platform into the correct spot
                yield return null;
            } while (_platformPlaced == false);
            _scenarioResources.PlatformPlacementGuide.SetActive(false);
            _scenarioResources.PowerPanel.PlatformPlaced();
            windfarmController.EnablePlacement(false);
            UISystem.DespawnPanel(dialog);
            _scenarioResources.PlatformAudioSource.PlayOneShot(_platformSetSuccessSFX);
        }

        private void PlaceObjectInfrontOfUser(Transform obj)
        {
            Transform userTransform = Camera.main.transform;
            Vector3 position = userTransform.position;
            position += userTransform.right * _displayTargetOffset.x;
            position += userTransform.up * _displayTargetOffset.y;
            position += userTransform.forward * _displayTargetOffset.z;
            
            SurfaceMagnetism surfaceMagnetism = obj.GetComponent<SurfaceMagnetism>();
            position.y = _scenarioResources.FloorHeight;
            position.y += surfaceMagnetism != null ? surfaceMagnetism.SurfaceRayOffset : 0.0f;
            position.y += 0.5f;
            
            obj.transform.position = position;
        }

        private void OnWindowPlacedEvent(WindowManipulationEvent obj)
        {
            // IsWindowPlacementCorrect() returns a tuple where item 1 is a bool representing whether the placement is valid,
            // and item 2 is the index of the valid placement guide.
            (bool, int) windowPlacementCorrect = IsWindowPlacementCorrect();
            _windowPlaced = obj.PickedUp == false && windowPlacementCorrect.Item1;
            Transform target = _scenarioResources.WindowController.transform;
            Transform guide = _scenarioResources.WindowPlacementGuides[windowPlacementCorrect.Item2].transform;
            _windowWallIndex = windowPlacementCorrect.Item2;
            if (_windowPlaced)
            {
                // Snap to position
                target.position = guide.position;
#if VRBUILD
                // slight adjustment in VR to line up with the rendered wall
                target.localPosition = target.localPosition + new Vector3(0, 0, 0.03f);
#endif
            }
            else
            {
                _scenarioResources.FocusOnObject(obj.PickedUp ? guide : target);
            }
        }

        private void OnPlatformPlacedEvent(PlatformManipulationEvent obj)
        {
            _platformPlaced = obj.PickedUp == false && IsPlatformPlacementCorrect();
            Transform target = _scenarioResources.PlatformGameObject.transform;
            Transform guide = _scenarioResources.PlatformPlacementGuide.transform;
            if (_platformPlaced)
            {
                // Snap to position
                target.position = guide.position;
                target.rotation = guide.rotation;
            }
            else
            {
                _scenarioResources.FocusOnObject(obj.PickedUp ? guide : target);
            }
        }
        
        /// <summary>
        /// Checks whether the current window placement is within the bounds of one of the valid placement guides
        /// </summary>
        /// <returns>A tuple where:
        ///     - item 1: represents a bool of whether the placement is valid
        ///     - item 2: the index of the valid placement guide
        /// </returns>
        private (bool, int) IsWindowPlacementCorrect()
        {
            Transform window = _scenarioResources.WindowController.transform;

            for (int i=0; i < _scenarioResources.WindowPlacementGuides.Length; i++)
            {
                if (_scenarioResources.WindowPlacementGuides[i])
                {
                    Transform guideTransform = _scenarioResources.WindowPlacementGuides[i].transform;
                    var collider = guideTransform.GetComponent<BoxCollider>();

                    bool isValidPlacement = collider.bounds.Contains(window.transform.position);
                    if (isValidPlacement)
                    {
                        return (isValidPlacement, i);
                    }
                }
            }

            // Window placement is invalid
            return (false, 0);
        }

        private bool IsPlatformPlacementCorrect()
        {
            Transform platform = _scenarioResources.PlatformGameObject.transform;
            Transform guide = _scenarioResources.PlatformPlacementGuide.transform;
            var collider = guide.GetComponent<BoxCollider>();
            return collider.bounds.Contains(platform.transform.position);
        }

        private void PlaceObjectsRelativeToChosenWindowPlacement()
        {
            // Place windfarm guide right in front of the window
            _scenarioResources.PlaceWindfarmGuide(_scenarioResources.VirtualRoom.Walls[_windowWallIndex].transform);

            // Place robot to the left of the windfarm
            Transform platformGuideTransform = _scenarioResources.PlatformPlacementGuide.transform;
            _scenarioResources.Robot.transform.position = MathHelpers.Vector3AtYPos(platformGuideTransform.position +
                platformGuideTransform.right * 0.9f +
                platformGuideTransform.forward * 0.2f,
                _scenarioResources.FloorHeight);
            _scenarioResources.Robot.transform.eulerAngles = new Vector3(0, platformGuideTransform.eulerAngles.y - 45f, 0);
        }
    }
}