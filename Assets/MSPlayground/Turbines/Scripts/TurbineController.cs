using System;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using MSPlayground.Core.Utils;
using MSPlayground.Common;
using MSPlayground.Scenarios.Turbines;
using MSPlayground.Turbines.Events;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

namespace MSPlayground.Turbines
{
    /// <summary>
    /// Manage the inputs, outputs and states of the Wind Turbine
    /// </summary>
    public class TurbineController : MonoBehaviour, IPowerSource
    {
        private const float MAX_POWER = 10.0f;
        
        [SerializeField]
        [Tooltip("How fast the turbine settles onto the dock, once released.")]
        float _dockingSpeed = 5f;

        [SerializeField]
        [Tooltip("The speed a rotor will spin when perfectly aligned with the wind")]
        float _maxRotorSpeed = 100f;

        [SerializeField]
        [Tooltip("How long it takes the rotor to reach target speed")]
        float _rotorRampUpSpeed = 3f;

        [SerializeField]
        [Tooltip("The hinged rigidbody gameobject connected to this rigidbody.")]
        Transform _mass;

        [SerializeField]
        [Tooltip("The window object from which this turbine receives wind direction and strength.")]
        WindowController _window;

        [Tooltip("Particle effects system for damaged state.")]
        [SerializeField] private ParticleSystem _damageParticles;

        [SerializeField]
        [Tooltip("The tall upright stand of the wind turbine.")]
        Transform _tower;
        [SerializeField] 
        [Tooltip("The box containing all the turbine hardware.")]
        Transform _nacelle;
        [SerializeField] 
        [Tooltip("The large spinning part that captures the wind.")]
        Transform _rotor;
        
        [SerializeField] 
        TurbineMenu _turbineMenu;

        [SerializeField]
        [Tooltip("The colour a turbine part is tinted in its broken state.")]
        Color _damagedColor = Color.red;
        [SerializeField]
        [Tooltip("The colour a turbine part is tinted in its working state.")]
        Color _workingColor = Color.white;

        public event Action<float> OnPowerUpdatedEvent;

        Transform _nearDock;
        TurbineDock _activeDock;
        Rigidbody _rigidTurbine;

        bool _towerBroken = false;
        bool _nacelleBroken = false;
        bool _rotorBroken = false;
        bool _isDocked = false;
        bool _docking = false;
        bool _picked = false;
        bool _released = false;

        float _power = 0f;

        float _rotorSpeed;

        ObjectManipulator _objectManip;
        BoundsControl _boundsControl;
        BoundingBoxVisuals _boundsVisuals;

        Renderer _towerRenderer;
        Renderer _nacelleRenderer;
        Renderer _rotorRenderer;

        Transform _cachedTransform;

        [SerializeField][Tooltip("The distance from the docking point the turbine must be to be considered docked.")]
        float _minDockingDistance = 0.001f;
        [SerializeField][Tooltip("The angle from upright the turbine must be to be considered standing.")]
        float _minDockingAngle = 1f;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSourceBase = null;
        [SerializeField] private AudioClip[] _sfxTurbineDrops = null;
        [SerializeField] private AudioSource _audioSourceRotorBlades = null;
        [SerializeField] private AudioClip[] _sfxFixSuccess = null;
        [SerializeField] private AudioClip _sfxFixError = null;
        [SerializeField] private AudioClip[] _sfxOnBreaks = null;
        [SerializeField] private AudioClip _sfxMaintenanceAlert = null;
        [SerializeField] private AudioClip _sfxDocked = null;
        [SerializeField] private AudioClip _sfxPowerUp = null;


        /// <summary>
        /// Called from MRTK Object Manipulator
        /// </summary>
        public bool Picked { set => _picked = true; }

        /// <summary>
        /// Called from MRTK Object Manipulator
        /// </summary>
        public bool Released { set => _released = true; }

        /// <summary>
        /// The power output of this turbine as determined by its orientation relative to the wind.
        /// </summary>
        public float Power => _power;

        public float PowerSourceOutput => IsBroken ? 0.0f : Power / MAX_POWER;
        /// <summary>
        /// Does this turbine have wind power maxed with a 0.1 allowance
        /// </summary>
        public bool IsWindPowerMaxed => Math.Abs(_power - MAX_POWER) <= 0.1f;

        /// <summary>
        /// Is this turbine picked up
        /// </summary>
        public bool IsPicked => _picked;

        /// <summary>
        /// Is this turbine docked
        /// </summary>
        public bool IsDocked => _isDocked;

        public bool IsBroken => _nacelleBroken || _rotorBroken || _towerBroken;
        public bool IsNacelleBroken => _nacelleBroken;
        public bool IsRotorBroken => _rotorBroken;
        public bool IsTowerBroken => _towerBroken;
        public Transform DockTransform => _nearDock;
        public TurbineMenu TurbineMenu => _turbineMenu;
        public Transform Tower => _tower;
        public Transform Nacelle => _nacelle;
        public Transform Rotor => _rotor;

        private void Start()
        {
            _cachedTransform = transform;
            
            _turbineMenu.gameObject.SetActive(false);

            _towerRenderer = _tower.GetComponentInChildren<Renderer>();
            _nacelleRenderer = _nacelle.GetComponentInChildren<Renderer>();
            _rotorRenderer = _rotor.GetComponentInChildren<Renderer>();

            ShowTurbine(false);

            _rigidTurbine = GetComponent<Rigidbody>();
            if (_mass != null)
            {
                _mass.transform.position = _cachedTransform.position;   
            }

            _objectManip = GetComponent<ObjectManipulator>();
            _objectManip.AllowedManipulations = 0;
            _objectManip.AllowedManipulations = Microsoft.MixedReality.Toolkit.TransformFlags.Move;
            _objectManip.OnClicked.AddListener(OnTurbineClicked);

            _boundsControl = GetComponent<BoundsControl>();
            _boundsVisuals = GetComponentInChildren<BoundingBoxVisuals>();
            EnableBoundsControl(false);
            
            GlobalEventSystem.Register<PowerEngagedEvent>(OnPowerEngagedEventHandler);
        }

        private void OnEnable()
        {
            EnableMass(true);
        }

        private void OnDisable()
        {
            EnableMass(false);
        }

        public void EnableMass(bool enable)
        {
            if (_mass != null)
            {
                if (enable)
                {
                    _mass.transform.position = transform.position;
                    _mass.gameObject.SetActive(true);   
                }
                else
                {
                    _mass.gameObject.SetActive(false);   
                }
            }
        }

        public void ShowTurbine(bool enabled)
        {
            _towerRenderer.enabled = enabled;
            _rotorRenderer.enabled = enabled;
            _nacelleRenderer.enabled = enabled;
        }

        private void OnDestroy()
        {
            _objectManip.OnClicked.RemoveListener(OnTurbineClicked);
            GlobalEventSystem.Unregister<PowerEngagedEvent>(OnPowerEngagedEventHandler);
        }

        private void Update()
        {
            if (_docking)
            {
                SlideIntoDock();
            }

            if (_isDocked && !_docking)
            {
                var prevPower = _power;
                _power = GetPower(_window.Wind);
                if (prevPower != _power)
                {
                    OnPowerUpdatedEvent?.Invoke(PowerSourceOutput);
                    SetSFXPitchBlades();
                }
            }

            SpinRotor(_power);

            if (_picked)
            {
                if (_isDocked)
                {
                    UnDock();
                }
                _picked = false;
            }

            if (_released)
            {
                if (_nearDock != null)
                {
                    if (!_isDocked)
                    {
                        Dock();
                    }
                }
                else
                {
                    _rigidTurbine.isKinematic = false;
                    _docking = false;
                }
                _released = false;
            }
        }

        /// <summary>
        /// Move and orient the turbine into its docked position
        /// </summary>
        private void SlideIntoDock()
        {
            if (_nearDock != null)
            {

                Vector3 dockingPoint = _activeDock.DockingPoint;
                float distanceRemaining = Vector3.Distance(transform.position, dockingPoint);
                bool reachedPoint = false;

                Quaternion target = Quaternion.Euler(0f, _cachedTransform.eulerAngles.y, 0f);
                float angleRemaining = Quaternion.Angle(_cachedTransform.rotation, target);
                bool reachedAngle = false;

                if (distanceRemaining > _minDockingDistance)
                {
                    _cachedTransform.position = Vector3.MoveTowards(transform.position, dockingPoint, distanceRemaining * _dockingSpeed * Time.deltaTime);
                }
                else
                {
                    _cachedTransform.position = _activeDock.DockingPoint;
                    reachedPoint = true;
                }

                if (angleRemaining > _minDockingAngle)
                {
                    _cachedTransform.rotation = Quaternion.Lerp(_cachedTransform.rotation, target, _dockingSpeed * Time.deltaTime);
                }
                else
                {
                    _cachedTransform.rotation = Quaternion.Euler(0f, _cachedTransform.eulerAngles.y, 0f);
                    reachedAngle = true;
                }

                _docking = (reachedPoint && reachedAngle) ? false : true;
            }
        }


        /// <summary>
        /// Compare the orientation of this turbine with the wind direction
        /// and return the amount of power being generated.
        /// </summary>
        private float GetPower(Vector2 wind)
        {
            Vector3 windDir = Quaternion.AngleAxis(wind.x, Vector3.up) * Vector3.forward;
            return Mathf.Clamp01(Vector3.Dot(_cachedTransform.forward, windDir)) * wind.y;
        }

        private void SpinRotor(float windEnergy)
        {
            float targetSpeed = (_power > 0 && _isDocked && !IsBroken) ? windEnergy * _maxRotorSpeed : 0;
            _rotorSpeed = Mathf.Lerp(_rotorSpeed, targetSpeed, _rotorRampUpSpeed * Time.deltaTime);
            _rotor.RotateAround(_rotor.position, _rotor.forward, _rotorSpeed * Time.deltaTime);
        }

        private void UnDock()
        {
            _power = 0f;
            _rotorSpeed = 0f;
            _isDocked = false;
            _mass.gameObject.SetActive(true);
            _rigidTurbine.isKinematic = false;
            EnableBoundsControl(false);
            _activeDock.ConnectToDock(false, transform);
            _audioSourceRotorBlades.Stop();
            _audioSourceRotorBlades.pitch = 0f;
            
            _objectManip.AllowedManipulations = TransformFlags.Move;
            _objectManip.AllowedInteractionTypes = InteractionFlags.Near | InteractionFlags.Gaze | InteractionFlags.Ray | InteractionFlags.Generic;

            GlobalEventSystem.Fire(new TurbinePlatformEvent { Turbine = this, Docked = false });
        }

        private void Dock()
        {
            _rigidTurbine.isKinematic = true;
            _mass.gameObject.SetActive(false);
            _activeDock.ConnectToDock(true, transform);
            _docking = true;
            _isDocked = true;
            _audioSourceRotorBlades.Play();
            SetSFXPitchBlades();
            
            _objectManip.AllowedManipulations = TransformFlags.Rotate;
            _objectManip.AllowedInteractionTypes = InteractionFlags.Gaze;
            
            GlobalEventSystem.Fire(new TurbinePlatformEvent {Turbine = this, Docked = true});
            _audioSourceBase.PlayOneShot(_sfxDocked);
        }

        /// <summary>
        /// Sets BoundsControl component active
        /// </summary>
        /// <param name="enable"></param>
        public void EnableBoundsControl(bool enable)
        {
            if (enable != _boundsControl.enabled)
            {
                if (_boundsControl.BoundsOverride != null)
                {
                    _boundsControl.BoundsOverride.gameObject.SetActive(enable);
                }

                _boundsControl.enabled = enable;
                _boundsControl.HandlesActive = enable;
            }
        }

        public void SetBoundingBoxColor(bool isGreen)
        {
            if (_boundsVisuals)
            {
                _boundsVisuals.SetMaterial(isGreen);
            }
        }

        /// <summary>
        /// Registers a listener for the bounds control on manipulation started
        /// </summary>
        /// <param name="action"></param>
        public void RegisterBoundsControlListener_ManipulationStarted(UnityAction<SelectEnterEventArgs> action)
        {
            _boundsControl.ManipulationStarted.AddListener(action);
        }
        
        /// <summary>
        /// Unregisters a listener for the bounds control on manipulation started
        /// </summary>
        public void UnregisterBoundsControlListener_ManipulationStarted(UnityAction<SelectEnterEventArgs> action)
        {
            _boundsControl.ManipulationStarted.RemoveListener(action);
        }

        /// <summary>
        /// Updates the materials and effects of the turbine to match the broken state
        /// </summary>
        public void SetupBrokenVisuals()
        {
            if (_towerBroken)
            {
                _towerRenderer.material.color = _damagedColor;
            }

            if (_nacelleBroken)
            {
                _nacelleRenderer.material.color = _damagedColor;
            }

            if (_rotorBroken)
            {
                _rotorRenderer.material.color = _damagedColor;
            }

            _damageParticles.SetEmissionEnabled(_towerBroken || _nacelleBroken || _rotorBroken);
            _damageParticles.Play(true);
        }


        /// <summary>
        /// Dock turbine instance to target
        /// </summary>
        /// <param name="dock"></param>
        public void ForceOntoDock(Transform dock)
        {
            _nearDock = dock;
            _activeDock = _nearDock.GetComponent<TurbineDock>();
            Dock();
        }

        public void ForceUndock()
        {
            _power = 0f;
            _rotorSpeed = 0f;
            _isDocked = false;
            _mass.gameObject.SetActive(true);
            _rigidTurbine.isKinematic = false;
            EnableBoundsControl(false);

            _objectManip.AllowedManipulations = 0;
            _objectManip.AllowedManipulations = Microsoft.MixedReality.Toolkit.TransformFlags.Move;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("TurbineDock") && _isDocked == false)
            {
                var turbineDock = other.transform.GetComponent<TurbineDock>();
                if (turbineDock.Occupied == false)
                {
                    _nearDock = other.transform;
                    _activeDock = turbineDock;
                    _activeDock.ShowNearDock(true);   
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("TurbineDock") && _isDocked == false)
            {
                if (_activeDock != null)
                {
                    _activeDock.ShowNearDock(false);   
                }
                _activeDock = null;
                _nearDock = null;
                _docking = false;
            }
        }

        private void OnTurbineClicked()
        {
            GlobalEventSystem.Fire(new TurbineSelectedEvent() { Turbine = this });
        }

        /// <summary>
        /// Sets the tower to be no longer broken
        /// </summary>
        public void RepairTower()
        {
            _towerBroken = false;
            _towerRenderer.material.color = _workingColor;
            if (IsBroken == false)
            {
                _damageParticles.Stop(true);
            }
        }

        /// <summary>
        /// Sets the nacelle to be no longer broken
        /// </summary>
        public void RepairNacelle()
        {
            _nacelleBroken = false;
            _nacelleRenderer.material.color = _workingColor;
            if (IsBroken == false)
            {
                _damageParticles.Stop(true);
            }
        }

        /// <summary>
        /// Sets the rotor to be no longer broken
        /// </summary>
        public void RepairRotor()
        {
            _rotorBroken = false;
            _rotorRenderer.material.color = _workingColor;
            if (IsBroken == false)
            {
                _damageParticles.Stop(true);
            }
        }

        /// <summary>
        /// Sets turbine modules to be broken by chance
        /// </summary>
        [ContextMenu(nameof(GenerateBrokenState))]
        public void GenerateBrokenState()
        {
            if (IsBroken)
            {
                // Turbine is already broken
                return;
            }

            switch ((TurbineModuleType)Random.Range((int)TurbineModuleType.None+1, (int)TurbineModuleType.Tower+1))
            {
                case TurbineModuleType.Rotor:
                    _rotorBroken = true;
                    break;
                case TurbineModuleType.Nacelle:
                    _nacelleBroken = true;
                    break;
                case TurbineModuleType.Tower:
                    _towerBroken = true;
                    break;
                
                case TurbineModuleType.None:
                default:
                    // This is impossible but cover anyways in case we add more and forget to include it in this switch statement
                    _rotorBroken = true;
                    break;
            }
            
            
            _audioSourceBase.PlayOneShot(_sfxOnBreaks[Random.Range(0, _sfxOnBreaks.Length)]);
            
            SetupBrokenVisuals();
        }

        /// <summary>
        /// Actiaves the turbine menu object
        /// </summary>
        /// <param name="show"></param>
        public void EnableTurbineMaintenanceMenu(bool show)
        {
            _turbineMenu.gameObject.SetActive(show);
            _audioSourceBase.PlayOneShot(_sfxMaintenanceAlert);
        }

        /// <summary>
        /// Attempts to repair a broken turbine part
        /// </summary>
        /// <param name="moduleType">Type of part to repair</param>
        /// <returns>Success</returns>
        public bool TryRepairTurbine(TurbineModuleType moduleType)
        {
            bool success = false;
            switch (moduleType)
            {
                case TurbineModuleType.Nacelle:
                    success = IsNacelleBroken;
                    if (success)
                    {
                        RepairNacelle();
                    }
                    break;
                case TurbineModuleType.Rotor:
                    success = IsRotorBroken;
                    if (success)
                    {
                        RepairRotor();
                    }
                    break;
                case TurbineModuleType.Tower:
                    success = IsTowerBroken;
                    if (success)
                    {
                        RepairTower();
                    }
                    break;
            }

            if (success)
            {
                OnPowerUpdatedEvent?.Invoke(PowerSourceOutput);
                GlobalEventSystem.Fire(new TurbineModuleRepairedEvent() {Turbine = this, ModuleType = moduleType,});
            }

            if (success)
            {
                AudioClip sfxFixSuccess = _sfxFixSuccess[Random.Range(0, _sfxFixSuccess.Length)];
                _audioSourceBase.PlayOneShot(sfxFixSuccess, volumeScale: 0.5f);   
            }
            else
            {
                _audioSourceBase.PlayOneShot(_sfxFixError, volumeScale: 0.5f);
            }
            
            return success;
        }

        /// <summary>
        /// On collisions play a random turbine drop sound effect
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            int randomSFXIndex = Random.Range(0, _sfxTurbineDrops.Length);
            _audioSourceBase.PlayOneShot(_sfxTurbineDrops[randomSFXIndex], 0.7f);
        }
        
        /// <summary>
        /// Play turbine power up sound effect when the entire power panel has been successfully been engaged
        /// </summary>
        /// <param name="eventData"></param>
        private void OnPowerEngagedEventHandler(PowerEngagedEvent eventData)
        {
            if (eventData.Success)
            {
                _audioSourceBase.PlayOneShot(_sfxPowerUp, 0.6f);
            }
        }

        /// <summary>
        /// Update blades spinning SFX pitch relative to power.
        /// </summary>
        private void SetSFXPitchBlades()
        {
            // 3f is the Pitch max in AudioSource
            _audioSourceRotorBlades.pitch = PowerSourceOutput * 3f;
        }

        /// <summary>
        /// Knock the turbine off the windfarm
        /// </summary>
        public void KnockOffWindfarm(Vector3 force)
        {
            if (_activeDock != null)
            {
                UnDock();
            }

            _rigidTurbine.mass = 10f;
            _rigidTurbine.AddForce(force);
        }

        /// <summary>
        /// Mutes the base audio source (NOT the rotor audio source)
        /// </summary>
        /// <param name="isMuted"></param>
        public void MuteAudioSource(bool isMuted)
        {
            _audioSourceBase.mute = isMuted;
        }
    }
}