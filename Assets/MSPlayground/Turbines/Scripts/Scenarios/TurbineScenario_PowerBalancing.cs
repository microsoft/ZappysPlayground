// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MSPlayground.Common;
using MSPlayground.Common.Helper;
using MSPlayground.Common.UI;
using MSPlayground.Core;
using MSPlayground.Core.UI;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines;
using MSPlayground.Turbines.Events;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.XR.Interaction.Toolkit;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// Turbine scenario script for player to balance the power between turbines
    /// </summary>
    public class TurbineScenario_PowerBalancing : TurbineScenario_Base
    {
        private const string ADJUST_DIALS_UX_CONTROL_DIALOG_ID = "robot_dialog_adjust_sliders_to_manage_power_supply";
        private const string TURBINES_NOT_PRODUCING_ENOUGH_POWER_DIALOG_ID = "robot_dialog_turbines_not_producing_enough_power";
        private const string WE_HAVE_ENOUGH_POWER_DIALOG_ID = "robot_dialog_turbines_are_producing_enough_power";
        private const string EXPERIENCE_SUMMARY_DIALOG_ID = "robot_dialog_experience_summary";

        [SerializeField] private AudioClip _sfxPowerUp;
        [SerializeField] private AudioClip _sfxRotateSet;
        
        private PowerUpdatedEvent _powerUpdatedEvent;
        private PowerEngagedEvent _powerEngagedEvent;
        private GameObject _adjustDialsDialogGO;
        private GameObject _boundsControlDialogGO;
        private GameObject _pressPowerDialogGO;
        private IntVariable _numRemainingVariable = new IntVariable();
        private int _numRemainingCount = 5;

        public override void EnterState()
        {
            base.EnterState();
            
            _powerUpdatedEvent = null;
            _powerEngagedEvent = null;
            _boundsControlDialogGO = null;
            _adjustDialsDialogGO = null;
        
            GlobalEventSystem.Register<PowerEngagedEvent>(OnPowerEngagedEventHandler);
            GlobalEventSystem.Register<PowerUpdatedEvent>(OnPowerUpdatedEventHandler);
            GlobalEventSystem.Register<TurbinePowerModuleAdjustedEvent>(OnTurbinePowerModuleAdjustedEvent);
            RegisterBoundsControlManipulationStartedListeners();
            
            _scenarioResources.FocusOnObject(null);
            StartCoroutine(SequenceRoutine());
        }

        public override void ExitState()
        {
            _scenarioResources.HideRobotDialog(_adjustDialsDialogGO);
            _scenarioResources.HideRobotDialog(_boundsControlDialogGO);
            
            GlobalEventSystem.Unregister<TurbinePowerModuleAdjustedEvent>(OnTurbinePowerModuleAdjustedEvent);
            GlobalEventSystem.Unregister<PowerUpdatedEvent>(OnPowerUpdatedEventHandler);
            GlobalEventSystem.Unregister<PowerEngagedEvent>(OnPowerEngagedEventHandler);
            UnregisterBoundsControlManipulationStartedListeners();
            _scenarioResources.FocusOnObject(null);

            base.ExitState();
        }

        public override void SkipState()
        {
            // Set successful environment
            OrientTurbines();
            SetDialsMax();

            // Fire happy robot SFX and happy animation state even if scenario is skipped
            _scenarioResources.PlatformAudioSource.PlayOneShot(_sfxPowerUp);
            GlobalEventSystem.Fire<RobotSFXEvent>(new RobotSFXEvent() {SfxType = SFXType.Positive});
            GlobalEventSystem.Fire<RobotAnimationEvent>(new RobotAnimationEvent() {PowerState = RobotController.PowerState.Full});

            _scenarioResources.FocusOnObject(null);
            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }

        private IEnumerator SequenceRoutine()
        {
            // Enable the bounds control of turbines that do not have wind power maxed
            SetBoundsControlWindPowerMaxed(playSFX: false);

            _numRemainingVariable.Value = _numRemainingCount;
            // Spawn initial bounds control instruction
            _boundsControlDialogGO = ShowPointerDialog(GetFirstTurbineNotOriented(), TURBINES_NOT_PRODUCING_ENOUGH_POWER_DIALOG_ID, 
                new Dictionary<string, IVariable>() {{"num_remaining",_numRemainingVariable}});
            
            yield return new WaitUntil(() => _powerEngagedEvent?.Success == true);
            _scenarioResources.FocusOnObject(null);

            UISystem.DespawnAllActivePanels();
            _scenarioResources.PlatformAudioSource.PlayOneShot(_sfxPowerUp);
            GlobalEventSystem.Fire<RobotSFXEvent>(new RobotSFXEvent() {SfxType = SFXType.Positive});
            GlobalEventSystem.Fire<RobotAnimationEvent>(new RobotAnimationEvent() {PowerState = RobotController.PowerState.Full});

            yield return WaitForDialogClosed(_scenarioResources.ShowRobotDialog(EXPERIENCE_SUMMARY_DIALOG_ID));
            
            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }

        private void OnPowerUpdatedEventHandler(PowerUpdatedEvent eventData)
        {
            _powerUpdatedEvent = eventData;

            // Disable the bounds controls of wind turbines that have maxed available power
            SetBoundsControlWindPowerMaxed();

            // Case where output power decreases after controlled power was already achieved
            if (_pressPowerDialogGO && !eventData.IsOutputPowerEnough)
            {
                CloseDialog(_pressPowerDialogGO);
                _pressPowerDialogGO = null;
                SpawnAdjustDialsDialog();
                _scenarioResources.FocusOnObject(null);
            }
            // Available power is obtained from orientation of turbines
            else if (eventData.IsAvailablePowerEnough && !_adjustDialsDialogGO && !eventData.IsOutputPowerEnough)
            {
                CloseDialog(_boundsControlDialogGO);
                _boundsControlDialogGO = null;
                SpawnAdjustDialsDialog();
            }
            // Output power represents the controlled power from the sliders
            else if (!_pressPowerDialogGO && eventData.IsOutputPowerEnough)
            {
                // We've reached the end, tell the user to continue to the next step
                CloseDialog(_adjustDialsDialogGO);
                _adjustDialsDialogGO = null;
                GlobalEventSystem.Fire<RobotSFXEvent>(new RobotSFXEvent() {SfxType = SFXType.General});
                _pressPowerDialogGO = _scenarioResources.ShowRobotDialog(WE_HAVE_ENOUGH_POWER_DIALOG_ID);
                _scenarioResources.FocusOnObject(_scenarioResources.PowerPanel.MainDial.transform);
            }
        }

        private void SpawnAdjustDialsDialog()
        {
            if (_adjustDialsDialogGO)
            {
                CloseDialog(_adjustDialsDialogGO);
                _adjustDialsDialogGO = null;
            }

            // Point to the top left turbine dock
            PowerPanelModuleTurbine[] modules = _scenarioResources.PowerPanel.Modules;
            PowerPanelModuleTurbine module = modules.Length >= 2 ? modules[1] : modules.FirstOrDefault();
            _adjustDialsDialogGO = ShowPointerDialog(module.radialSlider.HandleTransform, ADJUST_DIALS_UX_CONTROL_DIALOG_ID);
        }

        private void OnPowerEngagedEventHandler(PowerEngagedEvent eventData)
        {
            _powerEngagedEvent = eventData;
        }

        private void CloseDialog(GameObject dialog, float closeRobotDialogDelay = 0.0f)
        {
            if (dialog != null && dialog.activeInHierarchy)
            {
                if (closeRobotDialogDelay > 0.0f)
                {
                    _scenarioResources.HideRobotDialog(dialog);
                }
                else
                {
                    StartCoroutine(Coroutines.WaitAfterSeconds(closeRobotDialogDelay, () =>
                    {
                        _scenarioResources.HideRobotDialog(dialog);
                    }));   
                }
            }
        }

        // Spawn panel anchored to the platform with a bezier line
        private GameObject ShowPointerDialog(Transform pointTo, string panelID, Dictionary<string, IVariable> localVariables = null)
        {
            GameObject dialog = _scenarioResources.ShowRobotDialog(panelID, null, localVariables);

            // Anchor to bone
            Transform dialogTransform = dialog.transform;
            dialogTransform.SetParent(_scenarioResources.ControlDialogBone);
            dialogTransform.localPosition = Vector3.zero;
            dialogTransform.localEulerAngles = Vector3.zero;

            var bezierRenderer = dialog.GetComponentInChildren<BezierRenderer>();
            if (bezierRenderer)
            {
                bezierRenderer.End.SetParent(null);
                bezierRenderer.End.position = pointTo.position;
            }

            return dialog;
        }

        /// <summary>
        /// Deactivate the line from the pointer panel
        /// </summary>
        /// <param name="panelObject"></param>
        private void HidePointerLine(GameObject panelObject)
        {
            var bezierRenderer = panelObject.GetComponentInChildren<BezierRenderer>();
            if (bezierRenderer)
            {
                bezierRenderer.gameObject.SetActive(false);
            }
        }

        [ContextMenu(nameof(OrientTurbines))]
        private void OrientTurbines()
        {
            foreach( var turbine in _scenarioResources.Turbines)
            {
                turbine.transform.rotation = Quaternion.identity;
            }
        }

        [ContextMenu(nameof(SetDialsMax))]
        private void SetDialsMax()
        {
            float powerNeeded = _scenarioResources.PowerPanel.PowerNeeded;
            foreach (var module in _scenarioResources.PowerPanel.Modules)
            {
                module.OnSliderValueUpdated(new RadialSlider.SliderEventData()
                {
                    OldValue = 0.0f,
                    NewValue = powerNeeded,
                });   
            }
        }

        private void OnTurbinePowerModuleAdjustedEvent(TurbinePowerModuleAdjustedEvent obj)
        {
            // Only hide pointer line from the adjust-dials panel if it hasn't already been hidden.
            if (_adjustDialsDialogGO != null && _adjustDialsDialogGO.activeInHierarchy)
            {
                HidePointerLine(_adjustDialsDialogGO);
                GlobalEventSystem.Unregister<TurbinePowerModuleAdjustedEvent>(OnTurbinePowerModuleAdjustedEvent);
            }
        }

        private void EnableBoundsControl(bool isEnabled)
        {
            foreach (var turbine in _scenarioResources.Turbines)
            {
                turbine.EnableBoundsControl(isEnabled);
            }
        }

        private void RegisterBoundsControlManipulationStartedListeners()
        {
            foreach (var turbine in _scenarioResources.Turbines)
            {
                turbine.RegisterBoundsControlListener_ManipulationStarted(OnBoundsControlManipulationStarted);
            }
        }
        
        private void UnregisterBoundsControlManipulationStartedListeners()
        {
            foreach (var turbine in _scenarioResources.Turbines)
            {
                turbine.UnregisterBoundsControlListener_ManipulationStarted(OnBoundsControlManipulationStarted);
            }
        }

        /// <summary>
        /// Invoked when any of the turbines' bounds controls have started manipulation by user
        /// </summary>
        /// <param name="arguments"></param>
        private void OnBoundsControlManipulationStarted(SelectEnterEventArgs arguments)
        {
            // Once user has moved one bounds control, despawn the instruction panel anchored to the platform and respawn
            // it at the robot model to keep the platform uncluttered, but still provide some guidance if needed.
            if (_boundsControlDialogGO != null)
            {
                HidePointerLine(_boundsControlDialogGO);
                UnregisterBoundsControlManipulationStartedListeners();
            }
        }
        
        /// <summary>
        /// Get the first rotor transform of the turbine that does not have wind power maxed yet.
        /// If they are all maxed then return the first rotor transform.
        /// </summary>
        private Transform GetFirstTurbineNotOriented()
        {
            foreach (var turbine in _scenarioResources.Turbines)
            {
                if (!turbine.IsWindPowerMaxed)
                {
                    return turbine.Rotor;
                }
            }
            return _scenarioResources.Turbines[0].Rotor;
        }

        private void SetBoundsControlWindPowerMaxed(bool playSFX = true)
        {
            int turbinesToMax = 5;
            foreach (var turbine in _scenarioResources.Turbines)
            {
                turbine.EnableBoundsControl(!turbine.IsWindPowerMaxed);
                turbine.SetBoundingBoxColor(turbine.IsWindPowerMaxed);
                turbinesToMax -= turbine.IsWindPowerMaxed ? 1 : 0;
            }

            if (playSFX && _numRemainingCount != turbinesToMax)
            {
                _scenarioResources.PlatformAudioSource.PlayOneShot(_sfxRotateSet);
            }

            _numRemainingVariable.Value = _numRemainingCount = turbinesToMax;
        }
    }
}