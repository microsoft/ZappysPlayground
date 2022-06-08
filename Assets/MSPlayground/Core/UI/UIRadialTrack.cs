// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// Slimmed down version of RadialPinchSlider so that we may mimick the visuals of a circular slider
    /// </summary>
    public class UIRadialTrack : MonoBehaviour
    {
        [SerializeField] private GameObject _target;
        [SerializeField] private float _radius = 1.0f;
        [SerializeField] private float _startDegree = 0.0f;

        [SerializeField] private float _deltaDegree = 45.0f;

        // Percentage value between start and delta degrees
        [Range(0.0f, 1.0f)] [SerializeField] private float _sliderValue = 0.5f;

        public float Radius => _radius;

        public float StartDegree
        {
            get { return _startDegree; }
            set { _startDegree = RadialHelpers.NicifyDegreeValue(value); }
        }

        public float DeltaDegree
        {
            get { return _deltaDegree; }
            set { _deltaDegree = RadialHelpers.NicifyDegreeValue(value); }
        }

        public float SliderValue
        {
            get { return _sliderValue; }
            set
            {
                if (_sliderValue != value)
                {
                    _sliderValue = value;
                    UpdateUI();
                }
            }
        }

        private void OnValidate()
        {
            _radius = Mathf.Max(_radius, 0.0f);

            // Try and keep start/end degrees within a clean value of 360
            _startDegree = RadialHelpers.NicifyDegreeValue(_startDegree);
            _deltaDegree = RadialHelpers.NicifyDegreeValue(_deltaDegree);

            UpdateUI();
        }

        protected virtual void Start()
        {
            if (_target == null)
            {
                throw new Exception(
                    $"Slider thumb on gameObject {gameObject.name} is not specified. Did you forget to set it?");
            }

            UpdateUI();
        }

        [ContextMenu(nameof(UpdateUI))]
        private void UpdateUI()
        {
            if (_target != null)
            {
                _target.transform.position = RadialHelpers.CalculatePosition(
                    _sliderValue,
                    _startDegree,
                    _deltaDegree,
                    transform,
                    _radius
                );
                _target.transform.LookAt(this.transform.position);
            }
        }
    }
}