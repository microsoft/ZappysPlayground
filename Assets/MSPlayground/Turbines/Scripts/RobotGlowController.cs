
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using UnityEngine;

namespace MSPlayground.Turbines
{
    public class RobotGlowController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Renderer[] _targetRenderers;
        [SerializeField] private int _materialIndex = 1;
        [SerializeField] private string _colorProperty = "_EmissiveColor";

        [Header("State Colors")]
        [SerializeField] private Color _lowPowerColor = default;
        [SerializeField] private Color _fullPowerColor = default;
        [SerializeField] private Color _poweredUpColor = default;

        [Header("Low Power Settings")]
        [Tooltip("The amount of time each pulse takes in seconds.")]
        [SerializeField] private float _pulseDuration = 4.0f;
        [Tooltip("The animation curve of the pulse.")]
        [SerializeField] private AnimationCurve _pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Power Up Settings")]
        [Tooltip("The amount of time it takes to ease into the power up state in seconds.")]
        [SerializeField] private float _powerUpDuration = 1.0f;
        [Tooltip("The animation curve of the pulse.")]
        [SerializeField] private AnimationCurve _powerUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public enum States
        {
            Off,
            LowPower,
            PoweredUp,
        }

        private States _currentStateRoutine;
        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _currentRoutine;
        private Color _currentEmissionColor;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            SetBatteryLevel(false);
        }

        private void OnDestroy()
        {
            if (_currentRoutine != null)
            {
                StopCoroutine(_currentRoutine);
            }
        }

        /// <summary>
        /// Set current state
        /// </summary>
        public void SetState(States state, bool forceChange = false)
        {
            if (_currentStateRoutine != state || forceChange)
            {
                _currentStateRoutine = state;

                if (_currentRoutine != null)
                {
                    StopCoroutine(_currentRoutine);
                }

                switch (state)
                {
                    case States.Off:
                        _currentRoutine = StartCoroutine(OffStateRoutine());
                        break;
                    case States.LowPower:
                        _currentRoutine = StartCoroutine(LowPowerStateRoutine());
                        break;
                    case States.PoweredUp:
                        _currentRoutine = StartCoroutine(PoweredUpStateRoutine());
                        break;
                }
            }
        }

        /// <summary>
        /// Update battery level
        /// </summary>
        public void SetBatteryLevel(bool isBatteryFull)
        {
            _currentEmissionColor = isBatteryFull ? _fullPowerColor : _lowPowerColor;
        }


        private IEnumerator OffStateRoutine()
        {
            UpdateEmission(0);

            yield break;
        }

        /// <summary>
        /// Routine that controls the lower power state pulse animation
        /// </summary>
        private IEnumerator LowPowerStateRoutine()
        {
            float timer = 0;

            while (true)
            {
                timer += Time.deltaTime;
                float ratio = Mathf.Clamp01(timer / _pulseDuration);
                UpdateEmission(_pulseCurve.Evaluate(ratio));

                if (ratio >= 1)
                {
                    timer = 0;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Routine that controls the powered up state
        /// </summary>
        private IEnumerator PoweredUpStateRoutine()
        {
            _currentEmissionColor = _poweredUpColor;

            float timer = 0;

            while (true)
            {
                _currentEmissionColor = _poweredUpColor;

                timer += Time.deltaTime;
                float ratio = Mathf.Clamp01(timer / _powerUpDuration);
                UpdateEmission(_powerUpCurve.Evaluate(ratio));

                if (ratio >= 1)
                {
                    yield break;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Update all target renderers
        /// </summary>
        private void UpdateEmission(float emissionLevel)
        {
            //Calculate HSV, set V to emission level, and convert back to RGB
            Color.RGBToHSV(_currentEmissionColor, out float h, out float s, out _);
            float v = emissionLevel;

            Color rgb = Color.HSVToRGB(h, s, v);
            _propertyBlock.SetColor(_colorProperty, rgb);

            //Apply setting to target renderers
            for (int i = 0; i < _targetRenderers.Length; i++)
            {
                _targetRenderers[i].SetPropertyBlock(_propertyBlock, _materialIndex);
            }
        }
    }
}
