// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit.Data;
using MSPlayground.Scenarios.Turbines;
using UnityEngine;
using UnityEngine.UI;

namespace MSPlayground.Turbines
{
    /// <summary>
    /// UI Control for the power allocation for the turbines scenario
    /// </summary>
    public class PowerPanelModule : DataSourceGODictionary
    {
        // DataSource keys
        private string POWER_KEY = "Power";
        private string AVAILABLE_POWER_KEY = "AvailablePower";
        private string POWER_STATUS_TEXT_KEY = "PowerStatusText";
        private string POWER_STATUS_COLOR_KEY = "PowerStatusColor";
        private string POWER_STATUS_GLOW_COLOR_KEY = "PowerStatusGlowColor";
        private string AVAILABLE_POWER_STATUS_TEXT_KEY = "AvailablePowerStatusText";
        private string AVAILABLE_POWER_STATUS_COLOR = "AvailablePowerStatusColor";
        private string AVAILABLE_POWER_STATUS_GLOW_COLOR = "AvailablePowerStatusGlowColor";
        
        [Serializable]
        public struct State
        {
            public string LocId;
            [Range(0.0f, 1.0f)] public float Threshold;
            public Color Color;
            public Color GlowColor;
            [Range(0.0f, 1.0f)] public float GroupAlpha;
            public float PowerOutputModifier;
            public GameObject[] Activate;
            public GameObject[] Deactivate;
            public bool IsRequiredState;
        }

        [SerializeField] protected TurbineScenarioResources _turbineScenarioResources;
        [SerializeField] protected PowerPanel _powerPanel = null;
        [SerializeField] protected Image _availablePowerFill;
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected Image _controlledPowerFill;
        [SerializeField] protected State _offState = new State();
        [SerializeField] protected State[] _availablePowerStates = new State[0];
        [SerializeField] protected State[] _controlledOutputStates = new State[0];
        [SerializeField] protected Image[] _hexSprites = new Image[0];
        [SerializeField] protected Sprite _emptyHexSprite = null;
        [SerializeField] protected Sprite _filledHexSprite = null;

        protected IPowerSource _powerSource;
        protected float _controlledPowerOutput;
        protected bool _pendingUpdateState = false;

        /// <summary>
        /// Event for when the power value has been updated for this module
        /// </summary>
        public Action<float> OnPowerUpdatedEvent;

        /// <summary>
        /// Current connected power source
        /// </summary>
        public IPowerSource PowerSource => _powerSource;

        protected virtual bool ShouldShowOffState => AvailablePowerLevelPercentage == 0.0f;

        /// <summary>
        /// Power that is being given by the power source
        /// </summary>
        public float AvailablePowerLevelPercentage => _powerSource?.PowerSourceOutput ?? 0.0f;

        /// <summary>
        /// Returns the true value of power that is being outputted
        /// Only output what is available between the lowest of the user's setting or the actual power output from the power source
        /// </summary>
        /// <value></value>
        public float PowerOutputLevelPercentage => Mathf.Min(_controlledPowerOutput, AvailablePowerLevelPercentage);

        /// <summary>
        /// Returns the true value of power that is being outputted
        /// </summary>
        /// <value></value>
        public float PowerOutputLevelPercentageModified
        {
            get
            {
                // Only output what is available between the lowest of the user's setting or the actual power output from the power source
                float power = PowerOutputLevelPercentage;

                // Based on the state that the module is in, the power can be modified
                // Ex. a High power state would output less than Good state because High is inefficient
                if (DetermineState(power, ref _controlledOutputStates, out State controlledState))
                {
                    power *= controlledState.PowerOutputModifier;
                }

                return power;
            }
        }

        /// <summary>
        /// Returns true if the current state has the IsRequiredState flag
        /// </summary>
        public bool IsRequiredControlledState { get; protected set; }
        
        protected virtual void Reset()
        {
            if (_powerPanel == null)
            {
                _powerPanel = GetComponentInParent<PowerPanel>();
            }

            if (_turbineScenarioResources == null)
            {
                _turbineScenarioResources = GetComponentInParent<TurbineScenarioResources>();
            }
        }

        private void OnValidate()
        {
            ValidateStateCollection(ref _controlledOutputStates);
            ValidateStateCollection(ref _availablePowerStates);
        }

        private void ValidateStateCollection(ref State[] states)
        {
            // Ensure threshold values are valid and there are no overlap
            for (int i = 1; i < states.Length; ++i)
            {
                states[i].Threshold = Mathf.Max(states[i].Threshold, states[i - 1].Threshold);
            }
        }

        protected virtual void OnEnable()
        {
            UpdateState();
        }

        /// <summary>
        /// Attach a power source to this module
        /// </summary>
        /// <param name="powerSource"></param>
        public void SetPowerSource(IPowerSource powerSource)
        {
            if (_powerSource != powerSource)
            {
                if (_powerSource != null)
                {
                    _powerSource.OnPowerUpdatedEvent -= OnPowerSourceUpdated;
                }

                _powerSource = powerSource;
                if (_powerSource != null)
                {
                    _powerSource.OnPowerUpdatedEvent += OnPowerSourceUpdated;
                    OnPowerSourceUpdated(_powerSource.PowerSourceOutput);
                }

                _pendingUpdateState = true;
            }
        }

        protected virtual void OnPowerSourceUpdated(float power)
        {
            OnPowerUpdatedEvent?.Invoke(power);
            _pendingUpdateState = true;
        }

        private void LateUpdate()
        {
            if (_pendingUpdateState)
            {
                _pendingUpdateState = false;
                UpdateState();
            }
        }

        [ContextMenu(nameof(UpdateState))]
        protected void UpdateState()
        {
            DataChangeSetBegin();

            float controlledPower = PowerOutputLevelPercentage;
            UpdatePowerOutputUI(controlledPower);
            SetValue(POWER_KEY, controlledPower);

            float availablePower = AvailablePowerLevelPercentage;
            UpdateAvailablePowerUI(availablePower);
            SetValue(AVAILABLE_POWER_KEY, availablePower);
            IsRequiredControlledState = false;

            if (ShouldShowOffState)
            {
                ExecuteState(_offState);
                SetValue(POWER_STATUS_TEXT_KEY, _offState.LocId);
                SetValue(POWER_STATUS_COLOR_KEY, _offState.Color);
                SetValue(POWER_STATUS_GLOW_COLOR_KEY, _offState.GlowColor);
                SetValue(AVAILABLE_POWER_STATUS_TEXT_KEY, _offState.LocId);
                SetValue(AVAILABLE_POWER_STATUS_COLOR, _offState.Color);
                SetValue(AVAILABLE_POWER_STATUS_GLOW_COLOR, _offState.GlowColor);
            }
            else
            {
                if (DetermineState(availablePower, ref _availablePowerStates, out State availableState))
                {
                    ExecuteState(availableState);

                    SetValue(AVAILABLE_POWER_STATUS_TEXT_KEY, availableState.LocId);
                    SetValue(AVAILABLE_POWER_STATUS_COLOR, availableState.Color);
                    SetValue(AVAILABLE_POWER_STATUS_GLOW_COLOR, availableState.GlowColor);
                }

                if (DetermineState(controlledPower, ref _controlledOutputStates, out State controlledState))
                {
                    ExecuteState(controlledState);

                    SetValue(POWER_STATUS_TEXT_KEY, controlledState.LocId);
                    SetValue(POWER_STATUS_COLOR_KEY, controlledState.Color);
                    SetValue(POWER_STATUS_GLOW_COLOR_KEY, controlledState.GlowColor);
                    IsRequiredControlledState = controlledState.IsRequiredState;
                }
            }

            DataChangeSetEnd();
        }

        private bool DetermineState(float t, ref State[] states, out State result)
        {
            result = default;
            for (int i = states.Length - 1; i >= 0; --i)
            {
                State state = states[i];
                if (t >= state.Threshold)
                {
                    result = state;
                    return true;
                }
            }

            return false;
        }

        private void ExecuteState(State state)
        {
            if (state.Activate != null)
            {
                foreach (var go in state.Activate)
                {
                    go.SetActive(true);
                }
            }

            if (state.Deactivate != null)
            {
                foreach (var go in state.Deactivate)
                {
                    go.SetActive(false);
                }
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = state.GroupAlpha;
            }
        }

        protected virtual void UpdatePowerOutputUI(float percentage)
        {
            if (_controlledPowerFill != null)
            {
                _controlledPowerFill.fillAmount = percentage;
            }
        }

        protected virtual void UpdateAvailablePowerUI(float percentage)
        {
            if (_availablePowerFill != null)
            {
                // Currently using UGUI Image fill support to display fill for available power
                // The problem is that the UGUI image uses the full 360 degrees as a fill where as
                // we only want just a portion of the a radial to fill as per the png
                const float FILL_TO = 0.9f;
                const float FILL_FROM = 0.1f;
                float t = FILL_FROM + ((FILL_TO - FILL_FROM) * percentage);
                _availablePowerFill.fillAmount = t;
            }
        }
    }
}