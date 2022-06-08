// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using MSPlayground.Common;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines.Events;
using UnityEngine;

namespace MSPlayground.Turbines
{
    /// <summary>
    /// Menu to manage the displaying repair modules for a turbine and handling of the user interaction
    /// </summary>
    public class GearboxMenu : BaseHandSubMenu
    {
        /// <summary>
        /// Data structure to cache commonly used components or initialization variables
        /// </summary>
        [Serializable]
        public struct GearboxItem
        {
            public readonly GameObject Instance;
            public readonly TurbineModuleType ModuleType;
            public readonly Transform Transform;
            public readonly Vector3 InitialPosition;
            public readonly Vector3 InitialScale;
            public readonly Quaternion InitialRotation;
            public readonly Transform InitialParent;
            public readonly ObjectManipulator Interactable;
            public readonly Rigidbody Rigidbody;
            public readonly Collider Collider;

            public GearboxItem(GameObject instance, TurbineModuleType moduleType)
            {
                Instance = instance;
                ModuleType = moduleType;
                Transform = instance.transform;
                InitialPosition = Transform.localPosition;
                InitialScale = Transform.localScale;
                InitialRotation = Transform.localRotation;
                InitialParent = Transform.parent;
                Interactable = instance.GetComponent<ObjectManipulator>();
                Rigidbody = instance.GetComponent<Rigidbody>();
                Rigidbody.isKinematic = true;
                Collider = instance.GetComponent<Collider>();
            }
        }

        [SerializeField] private GameObject _rotorGameObject;
        [SerializeField] private GameObject _nacelleGameObject;
        [SerializeField] private GameObject _towerGameObject;

#pragma warning disable 0414
        [SerializeField] private float _vrObjectRadius = 15f;
#pragma warning restore 0414
        [SerializeField] private float _onManipulationScaleMod = 1.3f;
        [SerializeField] private TweenData _onManipulationScaleTween;
        [SerializeField] private TweenData _introScaleTween;
        [SerializeField] private TweenData _outroScaleTween;

        private GearboxItem _rotorGearBoxItem;
        private GearboxItem _nacelleGearBoxItem;
        private GearboxItem _towerGearBoxItem;

        private void Awake()
        {
            _rotorGearBoxItem = new GearboxItem(_rotorGameObject, TurbineModuleType.Rotor);
            _nacelleGearBoxItem = new GearboxItem(_nacelleGameObject, TurbineModuleType.Nacelle);
            _towerGearBoxItem = new GearboxItem(_towerGameObject, TurbineModuleType.Tower);
        }

        void Start()
        {
#if VRBUILD
            // update physics sizes in VR because they're harder to touch with controller rays
            _rotorGameObject.GetComponent<SphereCollider>().radius = _vrObjectRadius;
            _nacelleGameObject.GetComponent<SphereCollider>().radius = _vrObjectRadius;
            _towerGameObject.GetComponent<SphereCollider>().radius = _vrObjectRadius;
#endif
        }

        private void OnEnable()
        {
            StopAllCoroutines();
            _toggledOnGUI.SetActive(true);
        }
        
        private void OnDisable()
        {
            _toggledOnGUI.SetActive(false);
        }

        public void OnRepairObjectManipulatedStarted(GameObject repairObject)
        {
            GearboxItem gearboxItem = GetGearboxItem(repairObject);

            gearboxItem.Interactable.AllowedManipulations = TransformFlags.Move | TransformFlags.Rotate;
            gearboxItem.Collider.enabled = false;

            // Item needs to be unparented as nested ObjectManipulators tend cause problems
            gearboxItem.Transform.parent = null;
            StartCoroutine(gearboxItem.Transform.TweenScale(
                gearboxItem.Transform.localScale,
                gearboxItem.Transform.localScale * _onManipulationScaleMod,
                _onManipulationScaleTween.Duration,
                _onManipulationScaleTween.Curve)
            );
        }

        public void OnRepairObjectManipulatedEnded(GameObject repairObject)
        {
            GearboxItem gearboxItem = GetGearboxItem(repairObject);
            GlobalEventSystem.Fire(new DroppedRepairModuleEvent()
            {
                ModuleObject = repairObject, ModuleType = gearboxItem.ModuleType,
            });

            gearboxItem.Interactable.AllowedManipulations = 0;

            // Start the coroutine on the _handMenu MonoBehaviour so that it can still
            // complete even when GearboxMenu is deactivated.
            _handMenu.StartCoroutine(gearboxItem.Transform.TweenScale(
                gearboxItem.Transform.localScale,
                Vector3.zero,
                _outroScaleTween.Duration, _outroScaleTween.Curve, () =>
                {
                    if (isActiveAndEnabled)
                    {
                        ResetGearboxItemState(gearboxItem, false);

                        _handMenu.StartCoroutine(gearboxItem.Transform.TweenScale(
                            Vector3.zero,
                            gearboxItem.InitialScale,
                            _introScaleTween.Duration, _introScaleTween.Curve)
                        );
                    }
                    else
                    {
                        ResetGearboxItemState(gearboxItem);
                    }
                }));
        }

        private void ResetGearboxItemState(GearboxItem gearboxItem, bool resetScale = true)
        {
            gearboxItem.Transform.parent = gearboxItem.InitialParent;
            gearboxItem.Transform.localPosition = gearboxItem.InitialPosition;
            gearboxItem.Transform.localRotation = gearboxItem.InitialRotation;
            gearboxItem.Interactable.AllowedManipulations = TransformFlags.Move | TransformFlags.Rotate | TransformFlags.Scale;
            gearboxItem.Collider.enabled = true;
            if (resetScale)
            {
                gearboxItem.Transform.localScale = gearboxItem.InitialScale;   
            }
        }

        private GearboxItem GetGearboxItem(GameObject instance)
        {
            if (instance == _rotorGameObject)
            {
                return _rotorGearBoxItem;
            }
            if (instance == _nacelleGameObject)
            {
                return _nacelleGearBoxItem;
            }
            if (instance == _towerGameObject)
            {
                return _towerGearBoxItem;
            }

            Debug.LogError($"Unable to determine {nameof(GearboxItem)} for {instance.name}", instance);
            return default;
        }
    }
}