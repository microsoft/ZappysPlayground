// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Data;
using MSPlayground.Common;
using MSPlayground.Core.Utils;
using UnityEngine;
using UnityEngine.Localization;

namespace MSPlayground.Turbines
{
    /// <summary>
    /// UI Panel display and for managing the turbine menu
    /// </summary>
    public class TurbineMenu : DataSourceGODictionary
    {
        private const string TURBINE_LOCALIZATION_TABLE_ID = "TurbinesScenario";
        
        [SerializeField] private TurbineController _turbineController;
        [SerializeField] private StatefulInteractable _statefulInteractable;

        private void Reset()
        {
            if (_turbineController == null)
            {
                _turbineController = GetComponentInParent<TurbineController>();
            }
            
            if (_turbineController != null && _statefulInteractable != null)
            {
                _statefulInteractable = _turbineController.GetComponent<StatefulInteractable>();   
            }
        }

        private void OnEnable()
        {
            _statefulInteractable.IsGazeHovered.OnEntered.AddListener(OnEnterGaze);
            _statefulInteractable.IsGazeHovered.OnExited.AddListener(OnExitGaze);

            UpdateUI();

            GlobalEventSystem.Register<TurbineModuleRepairedEvent>(OnTurbineRepaired);
        }

        public void UpdateUI()
        {
            TurbineModuleType brokenModule = TurbineModuleType.None;
            if (_turbineController.IsRotorBroken)
            {
                brokenModule = TurbineModuleType.Rotor;
            }
            else if (_turbineController.IsNacelleBroken)
            {
                brokenModule = TurbineModuleType.Nacelle;
            }
            else if (_turbineController.IsTowerBroken)
            {
                brokenModule = TurbineModuleType.Tower;
            }

            if (brokenModule != TurbineModuleType.None)
            {
                SetValue("broken_module_name", $"{TURBINE_LOCALIZATION_TABLE_ID}/{brokenModule.ToString().ToLower()}_issues", true);
            }
        }

        private void OnDisable()
        {
            GlobalEventSystem.Unregister<TurbineModuleRepairedEvent>(OnTurbineRepaired);
            _statefulInteractable.IsGazeHovered.OnEntered.RemoveListener(OnEnterGaze);
            _statefulInteractable.IsGazeHovered.OnExited.RemoveListener(OnExitGaze);
        }

        private void OnTurbineRepaired(TurbineModuleRepairedEvent obj)
        {
            if (obj.Turbine == _turbineController)
            {
                if (_turbineController.IsBroken == false)
                {
                    this.gameObject.SetActive(false);
                }
            }
        }

        private void OnEnterGaze(float arg0)
        {
        }

        private void OnExitGaze(float arg0)
        {
        }
    }
}