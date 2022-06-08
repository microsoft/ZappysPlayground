
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using MSPlayground.Core;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines.Events;
using UnityEngine;

namespace MSPlayground.Turbines
{
    public class WindfarmController : MonoBehaviour
    {
        [SerializeField]
        private MRTKBaseInteractable _surfaceMagnetismInteractable;

        [SerializeField]
        private GameObject _floatingCables;

        [SerializeField]
        private GameObject _flatCables;

        private void Start()
        {
            bool flatSurface = SceneNavigator.LoadVREnv;
            _floatingCables.SetActive(!flatSurface);
            _flatCables.SetActive(flatSurface);
        }

        /// <summary>
        /// Enables world placement
        /// </summary>
        /// <param name="enable"></param>
        public void EnablePlacement(bool enable)
        {
            _surfaceMagnetismInteractable.gameObject.SetActive(enable);
        }

        /// <summary>
        /// Called when the windfarm is picked up during initial placement
        /// </summary>
        public void OnWindfarmPickedUp()
        {
            GlobalEventSystem.Fire(new PlatformManipulationEvent(){PickedUp = true});
        }

        /// <summary>
        /// Inspector callback 
        /// </summary>
        public void OnWindfarmPlaced()
        {
            GlobalEventSystem.Fire(new PlatformManipulationEvent(){PickedUp = false});
        }
    }
}