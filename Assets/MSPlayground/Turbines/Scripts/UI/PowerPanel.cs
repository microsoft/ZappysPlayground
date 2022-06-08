// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit.Data;
using Microsoft.MixedReality.Toolkit.UX;
using MSPlayground.Core.UI;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines.Events;
using UnityEngine;

namespace MSPlayground.Turbines
{
    /// <summary>
    /// Component that helps manage the entirety of the Power Balancing panel. Relies on external PowerPanelModule components
    /// </summary>
    public class PowerPanel : DataSourceGODictionary, IPowerSource
    {
        [SerializeField] [Range(0.0f, 1.0f)] private float _powerNeeded = 0.8f;
        [SerializeField] private PressableButton _powerButton;
        [SerializeField] private PowerPanelModuleMain _mainDial;
        [SerializeField] private PowerPanelModuleTurbine[] _modules;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _sparksSfx;
        [SerializeField] private AudioClip _explosionSfx;

        [Header("Robot Cable")]
        [SerializeField] private TurbineCable _cableToRobotAR;
        [SerializeField] private TurbineCable _cableToRobotVR;
        [SerializeField] private Color _overheatingPulseColor = Color.yellow;
        TurbineCable _cableToRobot;

        private float _lastKnownOutputPowerCheck = 0.0f;
        private float _lastKnownAvailablePowerCheck = 0.0f;
        private PowerUpdatedEvent _powerUpdatedEvent = new PowerUpdatedEvent();

        public event Action<float> OnPowerUpdatedEvent;

        
        public float PowerSourceOutput { get; private set;}

        /// <summary>
        /// Necessary power needed to be in a good state
        /// </summary>
        public float PowerNeeded => _powerNeeded;

        /// <summary>
        /// Collection of turbine modules
        /// </summary>
        public PowerPanelModuleTurbine[] Modules => _modules;

        /// <summary>
        /// Reference to the power button
        /// </summary>
        public PowerPanelModuleMain MainDial => _mainDial;
        
        
        private void Reset()
        {
            if (_modules == null || _modules.Length == 0)
            {
                InitializeModulesFromChildren();
            }
        }

        [ContextMenu(nameof(InitializeModulesFromChildren))]
        private void InitializeModulesFromChildren()
        {
            _modules = this.GetComponentsInChildren<PowerPanelModuleTurbine>(true);
        }

        private void Start()
        {
#if VRBUILD
            _cableToRobot = _cableToRobotVR;
#else
            _cableToRobot = _cableToRobotAR;
#endif

            _cableToRobotAR.gameObject.SetActive(false);
            _cableToRobotVR.gameObject.SetActive(false);

            for (int i = 0; i < _modules.Length; ++i)
            {
                _modules[i].SetIndex(i);
            }
            
            _mainDial.SetPowerSource(this);
            UpdateState(false);

        }

        private void OnEnable()
        {
            for (int i = 0; i < _modules.Length; ++i)
            {
                // When the user slides the slider we need to update overall power
                _modules[i].radialSlider.OnValueUpdated.AddListener(OnTurbinePowerUpdated);

                // When the power source changes in power we need to update overall power
                _modules[i].OnPowerUpdatedEvent += OnTurbinePowerUpdated;
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _modules.Length; ++i)
            {
                _modules[i].OnPowerUpdatedEvent -= OnTurbinePowerUpdated;
                if (_modules[i].radialSlider != null)
                {
                    _modules[i].radialSlider.OnValueUpdated.RemoveListener(OnTurbinePowerUpdated);   
                }
            }
        }

        private void OnTurbinePowerUpdated(float power)
        {
            UpdateState();
        }

        private void OnTurbinePowerUpdated(RadialSlider.SliderEventData eventData)
        {
            UpdateState();
        }
        
        /// <summary>
        /// Updates the state of the PowerPanel by collecting power information from PowerPanelModules
        /// </summary>
        private void UpdateState(bool sendEvent = true)
        {
            GetCurrentPowerPercentage(out float current, out float available, out float total, out bool isControlledPowerAllGreen);
            float averageOutput = current / total;
            float averageAvailable = available / total;
            // Require all dials to be in green as well for power output to be sufficient
            bool powerOutputAchieved = averageOutput >= _powerNeeded && isControlledPowerAllGreen;
            bool powerAvailableAchieved = averageAvailable >= _powerNeeded;
            
            if (_powerButton != null)
            {
                _powerButton.enabled = powerOutputAchieved;   
            }
            
            _mainDial.SetControlledPowerOutput(current / total);
            PowerSourceOutput = averageAvailable;
            OnPowerUpdatedEvent?.Invoke(averageAvailable);

            _powerUpdatedEvent.UpdateValues(
                _lastKnownOutputPowerCheck,
                _lastKnownAvailablePowerCheck,
                averageOutput,
                averageAvailable,
                powerOutputAchieved,
                powerAvailableAchieved
            );

            _cableToRobot?.SetPower(averageOutput);

            if (sendEvent)
            {
                GlobalEventSystem.Fire(_powerUpdatedEvent);   
            }

            _lastKnownOutputPowerCheck = averageOutput;
            _lastKnownAvailablePowerCheck = averageAvailable;
        }

        /// <summary>
        /// Power button callback. Fires a global event with the success status
        /// </summary>
        [ContextMenu(nameof(PowerButtonPressed))]
        public void PowerButtonPressed()
        {
            GetCurrentPowerPercentage(out float current, out float available, out float total, out bool isControlledPowerAllGreen);
            float average = current / (float) _modules.Length;
            GlobalEventSystem.Fire(new PowerEngagedEvent() {Success = average >= _powerNeeded && isControlledPowerAllGreen});
        }

        private void GetCurrentPowerPercentage(out float currentOutput, out float availableOutput, out float total, out bool isControlledPowerAllGreen)
        {
            currentOutput = availableOutput = total = 0.0f;
            bool areDialsAllGreen = true;
            foreach (var controller in _modules)
            {
                currentOutput += controller.PowerOutputLevelPercentageModified;
                availableOutput += controller.AvailablePowerLevelPercentage;
                total += 1.0f;
                areDialsAllGreen = areDialsAllGreen && controller.IsRequiredControlledState;
            }

            isControlledPowerAllGreen = areDialsAllGreen;
        }

        /// <summary>
        /// Attempts to return the index of a PowerPanelModule from our managed array
        /// </summary>
        /// <param name="instance">PowerPanelModule assumed to be managed</param>
        /// <param name="index">Out parameter that represents the index that the instance is managed at</param>
        /// <returns>Success</returns>
        public bool TryGetIndexOf(PowerPanelModuleTurbine instance, out int index)
        {
            index = 0;
            for (int i = 0; i < _modules.Length; ++i)
            {
                if (_modules[i] == instance)
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Start the end sequence
        /// </summary>
        public void StartEndSequence()
        {
            _audioSource.clip = _sparksSfx;
            _audioSource.loop = true;
            _audioSource.Play();

            _cableToRobot.SetForward(false);
            _cableToRobot.SetColor(_overheatingPulseColor);
            _cableToRobot.SetPower(3f);
            _cableToRobot.SetMaxSpeed(2f);
        }

        /// <summary>
        /// Called when the turbines are knocked off
        /// </summary>
        public void KnockOffTurbines()
        {
            _audioSource.Stop();
            _audioSource.PlayOneShot(_explosionSfx);

            _cableToRobot.SetPower(0);
        }

        /// <summary>
        /// Called when the platform has been placed to activate the robot cable
        /// </summary>
        public void PlatformPlaced()
        {
            _cableToRobot.gameObject.SetActive(true);
        }
    }
}