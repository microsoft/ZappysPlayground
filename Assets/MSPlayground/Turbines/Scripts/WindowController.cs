using System;
using System.Collections;
using System.Collections.Generic;
using MSPlayground.Core.Spatial;
using MSPlayground.Core.Utils;
using MSPlayground.Common;
using MSPlayground.Common.Helper;
using UnityEngine;
using Microsoft.MixedReality.GraphicsTools;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using MSPlayground.Turbines.Events;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// Global event for when the window is opened or closed
    /// </summary>
    public class OpenWindowEvent : BaseEvent
    {
        public bool IsWindowOpen;
        public Vector3 WindDirection;
    }

    /// <summary>
    /// Manage the state of the Window which affects the wind direction and speed.
    /// </summary>
    public class WindowController : MonoBehaviour
    {
        [Header("Control")]

        [Tooltip("The slider controling the window iris.")]
        [SerializeField] private Slider _irisControl;
        
        [Tooltip("The button controling the sliding window.")]
        [SerializeField] private PressableButton _paneControl;

        [SerializeField] private Color _controlLightEnabledColor = Color.green;
        [SerializeField] private Color _controlLightDisabledColor = Color.blue;

        [Header("Iris")]

        [Tooltip("The list of all the fins of the iris window.")]
        [SerializeField] private Transform[] irisFinTransforms;

        [Tooltip("Helps handle iris renderer functions")]
        [SerializeField] private RendererCollectionHelper _irisRendererHelper;

        [Tooltip("The indicator light on the slider")]
        [SerializeField] private RendererCollectionHelper _irisControlStatusLight;

        [Tooltip("Controls the curve of the iris fin movement")]
        [SerializeField] private AnimationCurve _irisFinMovementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Tooltip("How much the fins can rotate open (in degrees)")]
        [SerializeField] private float _irisFinRotationRange = 52f;

        [Tooltip("A buffer allowed on each end of the slider to deem the iris open or closed (normalized)")]
        [SerializeField] private float _irisFinStateThreshold = 0.05f;

        [Header("Panes")]

        [Tooltip("The left glass pane")]
        [SerializeField] private Transform _leftPaneTransform = default;

        [Tooltip("The right glass pane")]
        [SerializeField] private Transform _rightPaneTransform = default;

        [Tooltip("Helps handle glass renderer functions")]
        [SerializeField] private RendererCollectionHelper _paneRendererHelper;

        [Tooltip("The indicator light on the button")]
        [SerializeField] private RendererCollectionHelper _paneControlStatusLight;

        [Tooltip("Helps handle outside view renderer functions")]
        [SerializeField] private RendererCollectionHelper _outsideRendererHelper;

        [Tooltip("The distance that the panes travel when opening and closing")]
        [SerializeField] private Vector3 _paneMovementRange = new Vector3(0.561f, 0.0f, 0.0f);

        [Tooltip("Controls the curve of the opening and closing animation")]
        [SerializeField] private AnimationCurve _paneMovementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Tooltip("How long the opening and closing animations take (in seconds)")]
        [SerializeField] private float _paneMovementDuration = 0.5f;

        [Header("Wind")]
        
        [Tooltip("The strength of the wind - affects the power output of the turbines.")]
        [SerializeField] private float _windPower = 10f;

        [Tooltip("Wind Zone used for particle effect reactions.")]
        [SerializeField] private WindZone _windZone;

        [Tooltip("Wind visualization particle system.")]
        [SerializeField] private ParticleSystem _windParticles;

        [Header("Occlusion")]
        [Tooltip("Used to cut a hole in the wall when placed")]
        [SerializeField] private ClippingPrimitive _clippingPrimitive;
        [SerializeField] private GameObject _occlusionBox;

        [Header("References")]
	    [SerializeField]
	    private SurfaceMagnetism _surfaceMagnetism;
        [SerializeField] 
	    private MRTKBaseInteractable _surfaceMagnetismInteractable;
        
        [Header("Audio")]
        [SerializeField] private AudioSource _windowButtonAudioSource = null;
        [SerializeField] private AudioClip _windowButtonActiveSFX = null;

        private bool _irisIsOpen;
        private bool _paneIsOpen;

        private Tween<Vector3> _leftPaneTween;
        private Tween<Vector3> _rightPaneTween;

        private Vector3 _leftPaneReferencePosition;
        private Vector3 _rightPaneReferencePosition;

        private Coroutine _leftPaneTweenRoutine;
        private Coroutine _rightPaneTweenRoutine;

        private Vector2 _wind;
        /// <summary>
        /// The direction (x) and intensity (y) of the wind
        /// </summary>
        public Vector2 Wind => _wind;

	    /// <summary>
	    /// Is the the window fully opened
	    /// </summary>
	    public bool IsWindowOpen => _paneIsOpen && _irisIsOpen;

        private void Start()
        {
            _irisIsOpen = false;
            _irisControl.enabled = true;
            _irisControlStatusLight.SetColor(_controlLightEnabledColor);
            _irisRendererHelper.SetRenderingEnabled(true);

            _paneIsOpen = false;
            _paneControl.enabled = false;
            _paneControlStatusLight.SetColor(_controlLightDisabledColor);
            _paneRendererHelper.SetRenderingEnabled(false);
            _outsideRendererHelper.SetRenderingEnabled(false);

            _windZone.gameObject.SetActive(false);
            _windParticles.Stop(true);

            _leftPaneReferencePosition = _leftPaneTransform.localPosition;
            _rightPaneReferencePosition = _rightPaneTransform.localPosition;
            _leftPaneTween = new Tween<Vector3>() { Duration = _paneMovementDuration, Curve = _paneMovementCurve };
            _rightPaneTween = new Tween<Vector3>() { Duration = _paneMovementDuration, Curve = _paneMovementCurve };

#if VRBUILD
            _occlusionBox.SetActive(false);
#else
            _occlusionBox.SetActive(true);
#endif
        }

        /// <summary>
        /// Update wind direction
        /// </summary>
        private void Update()
        {
            _wind.x = transform.rotation.eulerAngles.y;
            _wind.y = _paneIsOpen ? _windPower : 0f;
        }

        /// <summary>
        /// Open/close the window and start/stop the wind
        /// </summary>
        public void OpenOrCloseWindow(bool openTheWindow)
        {
            OpenOrCloseWindow(openTheWindow, false);
        }

        /// <summary>
        /// Open/close the window and start/stop the wind
        /// </summary>
        public void OpenOrCloseWindow(bool openTheWindow, bool forceOpen = false)
        {
            // if iris window is not open, user cannot open the sliding window
            if (_irisIsOpen || forceOpen)
            {
                SetPanesState(openTheWindow);

                _irisControl.enabled = !_paneIsOpen;
                _irisControlStatusLight.SetColor(_paneIsOpen ? _controlLightDisabledColor : _controlLightEnabledColor);
                _windZone.gameObject.SetActive(_paneIsOpen);

                _windParticles.SetEmissionEnabled(_paneIsOpen);
                if (_paneIsOpen)
                {
                    _windParticles.Play(true);
                }

                GlobalEventSystem.Fire(new OpenWindowEvent() { IsWindowOpen = _paneIsOpen, WindDirection = _wind });
            }
        }

        /// <summary>
        /// Handle the animations for when the sliding panes are opened or closed
        /// </summary>
        /// <param name="isOpen"></param>
        private void SetPanesState(bool isOpen)
        {
            if (_paneIsOpen != isOpen)
            {
                _paneIsOpen = isOpen;
                _paneRendererHelper.SetRenderingEnabled(true);

                if (_leftPaneTweenRoutine != null)
                {
                    StopCoroutine(_leftPaneTweenRoutine);
                }

                if (_rightPaneTweenRoutine != null)
                {
                    StopCoroutine(_rightPaneTweenRoutine);
                }

                _leftPaneTween.From = _leftPaneTransform.localPosition;
                _leftPaneTween.To = isOpen ? _leftPaneReferencePosition + _paneMovementRange : _leftPaneReferencePosition;
                _leftPaneTweenRoutine = StartCoroutine(_leftPaneTransform.TweenPosition(_leftPaneTween, Space.Self, ()=> OnRoutineCompleted()));

                _rightPaneTween.From = _rightPaneTransform.localPosition;
                _rightPaneTween.To = isOpen ? _rightPaneReferencePosition - _paneMovementRange : _rightPaneReferencePosition;
                _rightPaneTweenRoutine = StartCoroutine(_rightPaneTransform.TweenPosition(_rightPaneTween, Space.Self));

                void OnRoutineCompleted()
                {
                    if (isOpen)
                    {
                        //Disables rendering the panes when fully open
                        _paneRendererHelper.SetRenderingEnabled(false);
                    }
                }
            }
        }

        /// <summary>
        /// Set the rotation of the iris fins
        /// </summary>
        /// <param name="openValue"></param>
        private void SetIris(float openValue = 0f)
        {
            float angle = _irisFinMovementCurve.Evaluate(openValue) * _irisFinRotationRange;
            Quaternion finRot = Quaternion.Euler(0f, angle, 0f);
            for (int i = 0; i < irisFinTransforms.Length; i++)
            {
                irisFinTransforms[i].localRotation = finRot;
            }
        }

        /// <summary>
        /// Called when the slider control is updated
        /// </summary>
        /// <param name="eventData"></param>
        public void OnSliderUpdated(SliderEventData eventData)
        {
            if (!_paneIsOpen)
            {
                float openAmount = eventData.NewValue;

                SetIris(openAmount);

                //Whether at either end of the slider
                bool closed = openAmount < _irisFinStateThreshold;
                _irisIsOpen = openAmount > 1 - _irisFinStateThreshold;

                //Disable pane components when closed
                _paneRendererHelper.SetRenderingEnabled(!closed);
                _outsideRendererHelper.SetRenderingEnabled(!closed);

                //Disable iris components when fully opened
                _irisRendererHelper.SetRenderingEnabled(!_irisIsOpen);

                //Play newly activated window button sfx
                if (_irisIsOpen && !_paneControl.enabled)
                {
                    _windowButtonAudioSource.PlayOneShot(_windowButtonActiveSFX);
                }
                
                //Set control status
                _paneControl.enabled = _irisIsOpen;
                _paneControlStatusLight.SetColor(_irisIsOpen ? _controlLightEnabledColor : _controlLightDisabledColor);
            }
        }

        /// <summary>
        /// Add or remove a wall from the Clipping Primitive's tracking list.
        /// </summary>
        /// <param name="wall">The wall to affect</param>
        /// <param name="addToList">Whether to add or remove from the list</param>
        public void SetWallClipping(Renderer wallRenderer, bool addToList)
        {
            if (addToList)
            {
                _clippingPrimitive.AddRenderer(wallRenderer);
            }
            else
            {
                _clippingPrimitive.RemoveRenderer(wallRenderer);
            }
        }
       
        /// <summary>
        /// Enable objects for the context of Surface Magnetism
        /// </summary>
        /// <param name="enable"></param>
        public void EnablePlacement(bool enable)
        {
            _surfaceMagnetismInteractable.gameObject.SetActive(enable);
        }

        public void OnWindowPickedUp()
        {
            GlobalEventSystem.Fire(new WindowManipulationEvent(){PickedUp = true});
        }

        /// <summary>
        /// Inspector callback from MRTKInteractable for Surface Magnetism placement
        /// </summary>
        public void OnWindowSurfaceMagnetismPlaced()
        {
            GlobalEventSystem.Fire(new WindowManipulationEvent(){PickedUp = false});
        }
    }
}
