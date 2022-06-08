// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// Partial copy of PinchSlider. RadialSlider allows the user to move a slider in a circular motion.
    /// Pinch limitation is done by DisabledInteractorTypes in the inspector
    /// </summary>
    public class RadialSlider : StatefulInteractable
    {
        public struct SliderEventData
        {
            public float OldValue;
            public float NewValue;
            public RadialSlider Slider;
        }

        [SerializeField] private float _radius = 1.0f;
        [SerializeField] private float _startDegree = 0.0f;
        [SerializeField] private float _deltaDegree = 45.0f;
        [SerializeField] private GameObject _handle;
        [Range(0.0f, 1.0f)] [SerializeField] private float _sliderValue = 0.5f;

        public UnityEvent<SliderEventData> OnValueUpdated = new UnityEvent<SliderEventData>();

        // The interactor that currently holds control over the slider
        private IXRInteractor sliderControlInteractor;

        public float Radius => _radius;

        /// <summary>
        /// The starting point of the slider
        /// </summary>
        public float StartDegree
        {
            get { return _startDegree; }
            set { _startDegree = RadialHelpers.NicifyDegreeValue(value); }
        }

        /// <summary>
        /// The distance of the slider, add onto to StartDegree to get the end point
        /// </summary>
        public float DeltaDegree
        {
            get { return _deltaDegree; }
            set { _deltaDegree = RadialHelpers.NicifyDegreeValue(value); }
        }

        /// <summary>
        /// Value of the slider
        /// </summary>
        public float SliderValue
        {
            get { return _sliderValue; }
            set
            {
                if (_sliderValue != value)
                {
                    float oldSliderValue = _sliderValue;
                    _sliderValue = value;
                    UpdateUI();
                    OnValueUpdated.Invoke(new SliderEventData()
                    {
                        OldValue = oldSliderValue, NewValue = _sliderValue, Slider = this
                    });
                }
            }
        }
        
        /// <summary>
        /// The transform of the handle
        /// </summary>
        public Transform HandleTransform
        {
            get { return _handle.transform; }
        }

        /// <summary>
        /// Starting interaction point, in world space.
        /// Computed by <see cref="GetInteractionPoint"> in <see cref="SetupForInteraction">
        /// </summary>
        protected Vector3 StartInteractionPoint { get; private set; }

        /// <summary>
        /// Float value that holds the starting value of the slider.
        /// </summary>
        protected float StartSliderValue { get; private set; }

        #region Unity methods

        private void OnValidate()
        {
            _radius = Mathf.Max(_radius, 0.0f);

            // Try and keep start/end degrees within a clean value of 360
            _startDegree = RadialHelpers.NicifyDegreeValue(_startDegree);
            _deltaDegree = RadialHelpers.NicifyDegreeValue(_deltaDegree);

            UpdateUI();
        }

        protected override void Awake()
        {
            base.Awake();
            ApplyRequiredSettings();
        }

        protected override void Reset()
        {
            base.Reset();
            ApplyRequiredSettings();
        }

        protected virtual void Start()
        {
            if (_handle == null)
            {
                throw new Exception(
                    $"Slider thumb on gameObject {gameObject.name} is not specified. Did you forget to set it?");
            }

            InitializeSliderThumb();

            OnValueUpdated?.Invoke(new SliderEventData()
            {
                OldValue = _sliderValue, NewValue = _sliderValue, Slider = this
            });

            selectEntered.AddListener((args) => SetupForInteraction());
        }

        #endregion

        #region Private Methods

        protected virtual void ApplyRequiredSettings()
        {
            // Sliders use InteractableSelectMode.Single to ignore
            // incoming interactors after a first/valid interactor has
            // been acquired.
            selectMode = InteractableSelectMode.Single;
        }

        protected virtual void SetupForInteraction()
        {
            StartInteractionPoint = GetInteractionPoint();
            StartSliderValue = _sliderValue;
        }

        /// <summary>
        /// Fetches the interaction point from any/all valid interactors.
        /// Should only be called if a selecting interactor exists
        /// </summary>
        protected virtual Vector3 GetInteractionPoint()
        {
            Vector3 interactionPoint = Vector3.zero;
            IXRInteractor currentSliderInteractor = null;

            if (isSelected)
            {
                interactionPoint = firstInteractorSelecting.GetAttachTransform(this).position;
                interactionPoint = RadialHelpers.ProjectOnCircumference(
                    interactionPoint,
                    transform,
                    _radius
                );
                currentSliderInteractor = firstInteractorSelecting;
            }


            if (sliderControlInteractor != currentSliderInteractor)
            {
                StartInteractionPoint = interactionPoint;
                StartSliderValue = _sliderValue;
                sliderControlInteractor = currentSliderInteractor;
            }

            return interactionPoint;
        }

        private void InitializeSliderThumb()
        {
            UpdateUI();
        }

        [ContextMenu(nameof(UpdateUI))]
        private void UpdateUI()
        {
            if (_handle != null)
            {
                _handle.transform.position = RadialHelpers.CalculatePosition(
                    _sliderValue,
                    _startDegree,
                    _deltaDegree,
                    transform,
                    _radius
                );
                _handle.transform.LookAt(this.transform.position, this.transform.forward);
            }
        }

        private void UpdateSliderValue()
        {
            Vector3 interactionPoint = GetInteractionPoint();
            // The StartInteractionPoint is considered the position where the user starts interacting with the knob
            // It is better to have the interaction delta based off of the center of the radial circle as the user
            // will be rotating around that point and not the knob point
            //Vector3 interactorDelta = interactionPoint - StartInteractionPoint;
            Vector3 interactorDelta = interactionPoint - this.transform.position;

            Vector3 forward = -transform.forward;
            Vector3 position = transform.position;

            Vector3 sliderPosition = RadialHelpers.CalculatePosition(
                _sliderValue,
                _startDegree,
                _deltaDegree,
                transform,
                _radius
            );
            Vector3 sliderDir = sliderPosition - position;
            Vector3 sliderTrackDirection = Vector3.Cross(forward, sliderPosition - position).normalized;
            float handDirectionScaler = Vector3.Dot(sliderTrackDirection, interactorDelta);

            Vector3 interactionDir = Vector3.ProjectOnPlane(interactionPoint - position, forward);
            float deltaAngle = Vector3.SignedAngle(interactionDir, sliderDir, forward);
            float deltaSliderValue = (Mathf.Abs(deltaAngle) / _deltaDegree) * handDirectionScaler;

            SliderValue = Mathf.Clamp01(SliderValue + deltaSliderValue);
        }

        #endregion

        #region XRI methods

        ///<inheritdoc />
        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && isSelected)
            {
                UpdateSliderValue();
            }
        }

        #endregion
    }
}