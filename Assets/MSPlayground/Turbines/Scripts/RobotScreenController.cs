
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using UnityEngine;

namespace MSPlayground.Turbines
{
    public class RobotScreenController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Renderer _renderer;
        [Tooltip("The joint(transform) controlling the eye animation.")]
        [SerializeField] Transform _eyeControlJoint;

        [Header("Material")]
        [SerializeField] private string _frameProperty = "_Frame";
        [SerializeField] private string _offsetXProperty = "_OffsetX";
        [SerializeField] private string _offsetYProperty = "_OffsetY";

        [Header("Frame Index")]
        [SerializeField] private int _offFrame = 0;
        [SerializeField] private int _blinkFrame = 0;
        [SerializeField] private int _lowPowerFrame = 0;
        [SerializeField] private int _powerUpSequenceFrame = 0;
        [SerializeField] private int _fullPowerFrame = 0;
        [SerializeField] private int _lowBatteryFrame = 0;
        [SerializeField] private int _fullBatteryFrame = 0;

        [Header("State Colors")]
        [SerializeField] private Color _lowPowerColor = default;
        [SerializeField] private Color _fullPowerColor = default;
        [SerializeField] private Color _poweredUpColor = default;
        [SerializeField] private Color _overloadedColor = default;

        [Header("Blink Settings")]
        [Tooltip("The minimum amount of time between each blink.")]
        [SerializeField] private float _blinkFrequencyMin = 4.0f;
        [Tooltip("The maximum amount of time between each blink.")]
        [SerializeField] private float _blinkFrequencyMax = 8.0f;
        [Tooltip("The length of each blink in seconds.")]
        [SerializeField] private float _blinkDuration = 0.1f;

        [Header("Low Battery Settings")]
        [Tooltip("How often the low battery override is displayed in seconds.")]
        [SerializeField] private float _lowBatteryDisplayFrequency = 5.0f;
        [Tooltip("The length of each battery blink in seconds.")]
        [SerializeField] private float _lowBatteryFlashDuration = 0.5f;
        [Tooltip("The length of each battery blink in seconds.")]
        [SerializeField] private int _lowBatteryFlashCount = 3;

        [Header("Power Up Sequence Settings")]
        [Tooltip("Delay before changing to the power up sequence frame.")]
        [SerializeField] private float _powerUpSequenceDelay = 1.0f;
        [Tooltip("The length of time to display the power up sequence frame.")]
        [SerializeField] private float _powerUpSequenceDuration = 2.0f;

        public enum States
        {
            Off,
            LowPower,
            PowerUpSequence,
            PoweredUp,
            Overloaded
        }

        public enum OverrideStates
        {
            None,
            Blink,
            LowBattery,
            FullBattery,
            ShowNothing,
        }

        private States _currentState;
        private OverrideStates _currentOverride;
        private MaterialPropertyBlock _propertyBlock;

        private Coroutine _currentStateRoutine;
        private Coroutine _blinkRoutine;
        private bool _isBatteryFull = false;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _blinkRoutine = StartCoroutine(BlinkRoutine());
            SetBatteryLevel(false);
        }

        private void OnDestroy()
        {
            Reset();
        }

        private void Reset()
        {
            if (_currentStateRoutine != null)
            {
                StopCoroutine(_currentStateRoutine);
            }

            if (_blinkRoutine != null)
            {
                StopCoroutine(_blinkRoutine);
            }
        }

        /// <summary>
        /// Set current state
        /// </summary>
        public void SetState(States state, bool forceChange = false)
        {
            if (_currentState != state || forceChange)
            {
                _currentState = state;

                if (_currentState == States.Overloaded)
                {
                    SetMaterialColor(_overloadedColor);
                }
                else
                {
                    SyncMaterialToState();

                    if (_currentStateRoutine != null)
                    {
                        StopCoroutine(_currentStateRoutine);
                    }

                    if (_currentOverride != OverrideStates.Blink)
                    {
                        SetOverride(OverrideStates.None);
                    }

                    switch (state)
                    {
                        case States.Off:
                            _currentStateRoutine = StartCoroutine(OffStateRoutine());
                            break;
                        case States.LowPower:
                            _currentStateRoutine = StartCoroutine(LowPowerStateRoutine());
                            break;
                        case States.PowerUpSequence:
                            _currentStateRoutine = StartCoroutine(PowerUpSequenceRoutine());
                            break;
                        case States.PoweredUp:
                            _currentStateRoutine = StartCoroutine(PoweredStateRoutine());
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Set current override state
        /// </summary>
        private void SetOverride(OverrideStates overrideState, bool forceChange = false)
        {
            if (_currentOverride != overrideState || forceChange)
            {
                _currentOverride = overrideState;
                SyncMaterialToState();
            }
        }

        /// <summary>
        /// Update the frame of the screen shader by the current state and override
        /// </summary>
        private void SyncMaterialToState()
        {
            if (_currentOverride == OverrideStates.None)
            {
                switch (_currentState)
                {
                    case States.Off:
                        SetMaterialFrame(_offFrame);
                        break;
                    case States.LowPower:
                        SetMaterialFrame(_lowPowerFrame);
                        break;
                    case States.PowerUpSequence:
                        SetMaterialFrame(_powerUpSequenceFrame);
                        break;
                    case States.PoweredUp:
                        SetMaterialFrame(_fullPowerFrame);
                        break;
                }
            }
            else
            {
                switch (_currentOverride)
                {
                    case OverrideStates.Blink:
                        SetMaterialFrame(_blinkFrame);
                        break;
                    case OverrideStates.LowBattery:
                        SetMaterialFrame(_lowBatteryFrame);
                        break;
                    case OverrideStates.FullBattery:
                        SetMaterialFrame(_fullBatteryFrame);
                        break;
                    case OverrideStates.ShowNothing:
                        SetMaterialFrame(_offFrame);
                        break;
                }
            }
        }

        /// <summary>
        /// Update battery level.
        /// </summary>
        public void SetBatteryLevel(bool isBatteryFull)
        {
            if (_currentState != States.PoweredUp)
            {
                _isBatteryFull = isBatteryFull;
                SetMaterialColor(_isBatteryFull ? _fullPowerColor : _lowPowerColor);
            }
        }

        /// <summary>
        /// Routine that runs during the Blank state.
        /// </summary>
        private IEnumerator OffStateRoutine()
        {
            SetMaterialOffset(0, 0);

            yield break;
        }

        /// <summary>
        /// Routine that runs during the Low Power state.
        /// </summary>
        private IEnumerator LowPowerStateRoutine()
        {
            float timer = 0;
            int flashCount = 0;
            bool flashing = true;

            // Displays battery sequence at start
            SetOverride(_isBatteryFull ? OverrideStates.FullBattery : OverrideStates.LowBattery);

            while (true)
            {
                SyncOffsetToBone();

                timer += Time.deltaTime;

                if (flashing)
                {
                    // Toggle between showing the battery and blank screen at an interval
                    if (timer >= _lowBatteryFlashDuration)
                    {
                        if (_currentOverride == OverrideStates.FullBattery || _currentOverride == OverrideStates.LowBattery)
                        {
                            SetOverride(OverrideStates.ShowNothing);
                        }
                        else
                        {
                            SetOverride(_isBatteryFull ? OverrideStates.FullBattery : OverrideStates.LowBattery);
                            flashCount++;
                        }
                        timer = 0;
                    }

                    // After the set number of flashes, reset the override and timer
                    if (flashCount >= _lowBatteryFlashCount)
                    {
                        SetOverride(OverrideStates.None);
                        flashing = false;
                        flashCount = 0;
                    }
                }
                else
                {
                    // Start showing the flashing sequence after an interval
                    if (timer >= _lowBatteryDisplayFrequency)
                    {
                        SetOverride(_isBatteryFull ? OverrideStates.FullBattery : OverrideStates.LowBattery);
                        flashing = true;
                        timer = 0;
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Routine that runs during the Power Up Sequence state.
        /// </summary>
        private IEnumerator PowerUpSequenceRoutine()
        {
            SetMaterialOffset(0, 0);
            SetOverride(OverrideStates.FullBattery);

            yield return new WaitForSeconds(_powerUpSequenceDelay);

            SetMaterialColor(_poweredUpColor);
            SetOverride(OverrideStates.None);

            yield return new WaitForSeconds(_powerUpSequenceDuration);

            SetState(States.PoweredUp);

            yield break;
        }

        /// <summary>
        /// Routine that runs during the Full Power state.
        /// </summary>
        private IEnumerator PoweredStateRoutine()
        {
            while (true)
            {
                SyncOffsetToBone();

                yield return null;
            }
        }

        /// <summary>
        /// Routine that runs alongside the state routine that controls blinking.
        /// </summary>
        private IEnumerator BlinkRoutine()
        {
            float timer = 0;
            float blinkTime = Random.Range(_blinkFrequencyMin, _blinkFrequencyMax);

            while (true)
            {
                if (_currentState == States.LowPower || _currentState == States.PoweredUp)
                {
                    if (_currentOverride == OverrideStates.None || _currentOverride == OverrideStates.Blink)
                    {
                        timer += Time.deltaTime;

                        //Reset blink override
                        if (_currentOverride == OverrideStates.Blink && timer >= 0)
                        {
                            SetOverride(OverrideStates.None);
                        }

                        //Activate blink and set up next one
                        if (timer >= blinkTime)
                        {
                            SetOverride(OverrideStates.Blink);
                            blinkTime = Random.Range(_blinkFrequencyMin, _blinkFrequencyMax);
                            timer = -_blinkDuration;
                        }
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Syncs the offset of the screen shader to the angle of the eye bone
        /// </summary>
        private void SyncOffsetToBone()
        {
            Vector3 eyeJoint = _eyeControlJoint.localEulerAngles;

            float x = eyeJoint.x;
            float y = eyeJoint.y;

            x = (x > 180) ? x - 360f : x;
            y = (y > 180) ? y - 360f : y;

            SetMaterialOffset(x, y);
        }

        /// <summary>
        /// Set color of screen material
        /// </summary>
        private void SetMaterialColor(Color color)
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_Color", color);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        /// <summary>
        /// Set frame index of screen shader
        /// </summary>
        private void SetMaterialFrame(int frame)
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(_frameProperty, frame);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        /// <summary>
        /// Set offset of screen shader
        /// </summary>
        private void SetMaterialOffset(float xOffset, float yOffset)
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(_offsetXProperty, xOffset);
            _propertyBlock.SetFloat(_offsetYProperty, yOffset);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
