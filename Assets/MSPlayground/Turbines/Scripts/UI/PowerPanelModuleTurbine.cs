// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using MSPlayground.Common;
using MSPlayground.Core.UI;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines.Events;
using UnityEngine;

namespace MSPlayground.Turbines
{
    /// <summary>
    /// UI Control for the power allocation for the turbines
    /// </summary>
    public class PowerPanelModuleTurbine : PowerPanelModule
    {
        /// <summary>
        /// The amount of steps in a full rotation of the radial track.
        /// This is only used for SFX.
        /// </summary>
        const int SLIDER_TICK_DIVISIONS = 15;
        [SerializeField] private RadialSlider _radialSlider;
        [SerializeField] private Tweener _outputPowerTweener;
        [SerializeField] private UIRadialTrack _availablePowerSlider;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _sfxSliderTick;
        
        [Tooltip("The step when the slider tick SFX was last played")]
        private int _sliderLastTickStep = 0;
        
        /// <summary>
        /// User controlled slider
        /// </summary>
        public RadialSlider radialSlider => _radialSlider;

        protected override bool ShouldShowOffState => _powerSource == null;

        protected override void Reset()
        {
            if (_radialSlider == null)
            {
                _radialSlider = GetComponentInChildren<RadialSlider>(true);
            }
        }

        protected override void OnEnable()
        {
            _radialSlider.SliderValue = 0.0f;
            TryInitializeWithExistingTurbines();
            UpdateState();
            GlobalEventSystem.Register<TurbinePlatformEvent>(OnTurbinePlatformEvent);
        }

        private void OnDisable()
        {
            GlobalEventSystem.Unregister<TurbinePlatformEvent>(OnTurbinePlatformEvent);
        }

        private void TryInitializeWithExistingTurbines()
        {
            if (_turbineScenarioResources != null)
            {
                if (_powerPanel.TryGetIndexOf(this, out int index) == false)
                {
                    Debug.LogError("Unable to retrieve PowerPanelModule index from PowerPanel");
                }

                var turbine = _turbineScenarioResources.Turbines[index];
                if (turbine.IsDocked)
                {
                    SetPowerSource(turbine);
                }
            }
        }

        private void OnTurbinePlatformEvent(TurbinePlatformEvent obj)
        {
            // Undocked
            if (obj.Docked == false)
            {
                if (obj.Turbine == (TurbineController) _powerSource)
                {
                    SetPowerSource(null);
                }
            }
            else
            {
                if (_powerPanel.TryGetIndexOf(this, out int index) == false)
                {
                    Debug.LogError("Unable to retrieve PowerPanelModule index from PowerPanel");
                }

                if (_turbineScenarioResources.TryGetDockAtIndex(index, out GameObject associatedDock) == false)
                {
                    Debug.LogError($"Unable to retrieve associated dock at index {index}");
                }

                // The global event sent out will be received by all modules, we need to check if our relevant dock was affected
                if (obj.Turbine.DockTransform == associatedDock.transform)
                {
                    SetPowerSource(obj.Turbine);
                }
            }
        }

        protected override void OnPowerSourceUpdated(float power)
        {
            OnPowerUpdatedEvent?.Invoke(power);

            // The user controlled power cannot be more than the available power
            if (power < _radialSlider.SliderValue)
            {
                _radialSlider.SliderValue = power;
            }

            _pendingUpdateState = true;
        }

        /// <summary>
        /// Called by the pinch slider in the inspector
        /// </summary>
        /// <param name="sliderEventData"></param>
        public void OnSliderValueUpdated(RadialSlider.SliderEventData sliderEventData)
        {
            // The user cannot set the power level higher than what the turbine is able to give
            float availablePower = AvailablePowerLevelPercentage;
            if (_radialSlider.SliderValue > availablePower)
            {
                _radialSlider.SliderValue = availablePower;
            }

            _controlledPowerOutput = sliderEventData.NewValue;
            GlobalEventSystem.Fire(new TurbinePowerModuleAdjustedEvent()
            {
                Module = this,
            });
            
            // Calculate the floored decimal of the new slider value relative to {SLIDER_TICK_DIVISIONS}.
            // If the value is different to the decimal value when we last played the tick SFX, then play the SFX again!
            int flooredTickValue = (int) Math.Floor(sliderEventData.NewValue * SLIDER_TICK_DIVISIONS); // Get the dividend step
            if (flooredTickValue != _sliderLastTickStep)
            {
                _audioSource.PlayOneShot(_sfxSliderTick);
                _sliderLastTickStep = flooredTickValue;
            }
            
            _pendingUpdateState = true;
        }

        protected override void UpdateAvailablePowerUI(float percentage)
        {
            base.UpdateAvailablePowerUI(percentage);
            if (_availablePowerSlider != null)
            {
                _availablePowerSlider.SliderValue = percentage;
            }
            if (_outputPowerTweener != null)
            {
                _outputPowerTweener.SpeedModifier = percentage;
            }
        }

        public void SetIndex(int index)
        {
            for (int i = 0; i < _hexSprites.Length; ++i)
            {
                _hexSprites[i].sprite = i == index ? _filledHexSprite : _emptyHexSprite;
            }
        }
        
        protected override void UpdatePowerOutputUI(float percentage)
        {
            if (_controlledPowerFill != null)
            {
                // Towards the [0,1] bounds edges, the radial fill becomes more different
                // from our internal RadialSlider. This adjusts the fill to look more visually accurate.
                float fillAmount = percentage;
                if (percentage <= 0.3f)
                {
                    fillAmount = (float) Math.Pow(percentage, 0.8);
                }
                else if (0.7f <= percentage)
                {
                    fillAmount = (float) Math.Pow(percentage, 1.35);
                }

                _controlledPowerFill.fillAmount = Mathf.Clamp01(fillAmount);
            }
        }
    }
}