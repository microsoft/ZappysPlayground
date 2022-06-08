
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Data;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines.Events;
using Random = UnityEngine.Random;

namespace MSPlayground.Turbines
{
    /// <summary>
    /// Manage the state of the Robot
    /// </summary>
    public class RobotController : DataSourceThemeProvider
    {
        internal enum PowerState
        {
            Off,
            Low,
            Full,
            Overloading,
            Overloaded
        }

        [Header("Animation")]
        [Tooltip("Controls bone animations")]
        [SerializeField] private Animator _robotAnimator;
        [Tooltip("Power level parameter of the animator")]
        [SerializeField] private string _animatorPowerLevelParameter = "PowerLevel";

        [Tooltip("Controls screen animations and effects")]
        [SerializeField] private RobotScreenController _screenController;
        [Tooltip("Controls glow animations and effects")]
        [SerializeField] private RobotGlowController _glowController;

        [Header("Audio")]
        [SerializeField]
        [Tooltip("The audio source for the robot")]
        AudioSource _audioSource = null;
        [Tooltip("The audio source for the hovering state of the robot (lower volume than main)")]
        [SerializeField] AudioSource _audioSourceHover = null;
        [SerializeField] AudioClip _sfxPowerUp = null;
        [SerializeField] AudioClip _sfxPowerDown = null;
        [SerializeField] AudioClip _sfxOverload = null;
        [SerializeField] AudioClip _sfxOverloaded = null;
        [SerializeField] AudioSource _musicAudioSource = null;


        const string SFX_THEME_CATEGORY = "robot";
        private PowerState _powerState;

        private void Start()
        {
            // Register event handler to trigger robot sound effects externally
            GlobalEventSystem.Register<RobotSFXEvent>(RobotSfxEventHandler);
            // Register event handler to trigger robot animation state externally
            GlobalEventSystem.Register<RobotAnimationEvent>(RobotAnimationEventHandler);
            // Register event handler to display available power level
            GlobalEventSystem.Register<PowerUpdatedEvent>(OnPowerUpdatedEventHandler);
        }

        private void OnDestroy()
        {
            GlobalEventSystem.Unregister<RobotSFXEvent>(RobotSfxEventHandler);
            GlobalEventSystem.Unregister<RobotAnimationEvent>(RobotAnimationEventHandler);
            GlobalEventSystem.Unregister<PowerUpdatedEvent>(OnPowerUpdatedEventHandler);
        }

        /// <summary>
        /// Set current power state
        /// </summary>
        private void SetPowerState(PowerState state, bool forceChange = false)
        {
            if (_powerState != state || forceChange)
            {
                _powerState = state;
                _robotAnimator.SetInteger(_animatorPowerLevelParameter, (int)_powerState);

                switch (_powerState)
                {
                    case PowerState.Off:
                        _screenController.SetState(RobotScreenController.States.Off);
                        _glowController.SetState(RobotGlowController.States.Off);
                        _audioSourceHover.Stop(); // stop looped hover sound
                        break;
                    case PowerState.Low:
                        _screenController.SetState(RobotScreenController.States.LowPower);
                        _glowController.SetState(RobotGlowController.States.LowPower);
                        _audioSourceHover.Stop(); // stop looped hover sound
                        _audioSource.PlayOneShot(_sfxPowerDown);
                        break;
                    case PowerState.Full:
                        _screenController.SetState(RobotScreenController.States.PowerUpSequence);
                        _glowController.SetState(RobotGlowController.States.PoweredUp);
                        _audioSourceHover.Play(); // looped hover sound
                        _audioSource.PlayOneShot(_sfxPowerUp);
                        _musicAudioSource.Play();
                        break;
                    case PowerState.Overloading:
                        _screenController.SetState(RobotScreenController.States.Overloaded);
                        _glowController.SetBatteryLevel(false);
                        _glowController.SetState(RobotGlowController.States.LowPower);
                        _audioSource.PlayOneShot(_sfxOverload);
                        break;
                    case PowerState.Overloaded:
                        _audioSource.PlayOneShot(_sfxOverloaded);
                        _screenController.SetState(RobotScreenController.States.Off);
                        _glowController.SetState(RobotGlowController.States.Off);
                        _audioSourceHover.Stop();
                        _musicAudioSource.Stop();
                        SetPowerState(0);
                        break;
                }
            }
        }

        /// <summary>
        /// Increment the robot's state to preview the state animations for testing purposes.
        /// </summary>
        public void TestRobotPowerState()
        {
            SetPowerState(_powerState == PowerState.Full ? PowerState.Off : _powerState + 1);
            Debug.Log("Robot state test: Power now " + _powerState);
        }

        [ContextMenu(nameof(TestSFX))]
        public void TestSFX()
        {
            GlobalEventSystem.Fire<RobotSFXEvent>(new RobotSFXEvent() {SfxType = SFXType.Positive});
        }
        
        /// <summary>
        /// Handle the RobotSFXEvent
        /// </summary>
        /// <param name="eventData"></param>
        void RobotSfxEventHandler(RobotSFXEvent eventData)
        {
            if (ThemeProfile != null)
            {
                RobotSFXTheme _currentSFXTheme = ThemeProfile as RobotSFXTheme;
                var clipArray = _currentSFXTheme.SfxByType[eventData.SfxType];
                int randomIndex = Random.Range(0, clipArray.Length);
                _audioSource.PlayOneShot(clipArray[randomIndex]);
            }
        }
        
        /// <summary>
        /// Handle the RobotAnimationEvent
        /// </summary>
        /// <param name="eventData"></param>
        void RobotAnimationEventHandler(RobotAnimationEvent eventData)
        {
            SetPowerState(eventData.PowerState);
        }

        /// <summary>
        /// Handle the PowerUpdatedEvent
        /// </summary>
        private void OnPowerUpdatedEventHandler(PowerUpdatedEvent eventData)
        {
            _screenController.SetBatteryLevel(eventData.IsOutputPowerEnough);
            _glowController.SetBatteryLevel(eventData.IsOutputPowerEnough);
        }
    }
}