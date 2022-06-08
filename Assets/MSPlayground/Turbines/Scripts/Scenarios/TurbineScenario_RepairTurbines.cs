// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Data;
using MSPlayground.Common.UI;
using MSPlayground.Core;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines;
using MSPlayground.Turbines.Events;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// Turbine scenario script for the player to select the turbines and add repairs
    /// </summary>
    public class TurbineScenario_RepairTurbines : TurbineScenario_Base
    {
        private const string USE_TOOLBOX_WRITE_MENU_DIALOG_ID = "robot_dialog_theres_an_issue_with_gearbox";

        [SerializeField]
        private int _desiredNumBrokenTurbines = 3;
        private int _numBrokenTurbines = 0;
        private TurbineController _selectedTurbine;
        private GameObject _selectedTurbinePanel;
        private GameObject _maintenanceDialog;


        public override void ResetState()
        {
            base.ResetState();
            _numBrokenTurbines = _scenarioResources.Turbines.Count(t => t.IsBroken);
        }

        public override void EnterState()
        {
            base.EnterState();
            
            List<TurbineController> turbines = _scenarioResources.Turbines.ToList();
            Shuffle(turbines);
            
            // Ensure we don't try and target more turbines to be broken than what we have available
            _desiredNumBrokenTurbines = Mathf.Min(_desiredNumBrokenTurbines, turbines.Count);
            for (int i = 0; i < _desiredNumBrokenTurbines; ++i)
            {
                var turbine = turbines[i];
                turbine.GenerateBrokenState();
                if (turbine.IsBroken)
                {
                    _numBrokenTurbines++;
                    // Delay this to give the user time to adjust to the events
                    //turbine.EnableTurbineMaintenanceMenu(true);
                    if (_numBrokenTurbines >= _desiredNumBrokenTurbines)
                    {
                        break;
                    }
                }
            }
            
            GlobalEventSystem.Register<TurbineModuleRepairedEvent>(OnTurbineModuleRepairedEvent);
            StartCoroutine(SequenceRoutine());
        }

        public override void ExitState()
        {
            GlobalEventSystem.Unregister<TurbineModuleRepairedEvent>(OnTurbineModuleRepairedEvent);

            base.ExitState();
        }

        public override void SkipState()
        {
            _numBrokenTurbines = 0;
            for( int i = 0 ; i < _scenarioResources.Turbines.Length; ++i )
            {
                TurbineController turbine = _scenarioResources.Turbines[i];
                turbine.TryRepairTurbine(TurbineModuleType.Nacelle);
                turbine.TryRepairTurbine(TurbineModuleType.Rotor);
                turbine.TryRepairTurbine(TurbineModuleType.Tower);
                turbine.EnableTurbineMaintenanceMenu(false);
            }
            _scenarioResources.FocusOnObject(null);
            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }

        IEnumerator SequenceRoutine()
        {
            // Skip this scenario step altogether if there are no turbines to repair
            if (_numBrokenTurbines == 0)
            {
                GoToNextState();
                yield break;
            }
            
            _scenarioResources.FocusOnObject(_scenarioResources.PlatformGameObject.transform);
            GlobalEventSystem.Fire<RobotSFXEvent>(new RobotSFXEvent() {SfxType = SFXType.Glitch});
            // Point to the first broken turbine
            ShowMaintenanceDialog(_scenarioResources.Turbines.First(t => t.IsBroken));
            
            yield return new WaitForSeconds(1.0f);
            foreach (var turbine in _scenarioResources.Turbines)
            {
                if (turbine.IsBroken)
                {
                    turbine.EnableTurbineMaintenanceMenu(true);
                }
            }
            
            // Wait for user to repair the turbines via UI menus
            while (_numBrokenTurbines > 0)
            {
                yield return null;
            }

            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }
        
        private void ShowMaintenanceDialog(TurbineController turbine)
        {
            _maintenanceDialog = _scenarioResources.ShowRobotDialog(USE_TOOLBOX_WRITE_MENU_DIALOG_ID, null);
            
            Transform target = turbine.Nacelle;
            var bezierRenderer = _maintenanceDialog.GetComponentInChildren<BezierRenderer>();
            if (bezierRenderer != null)
            {
                bezierRenderer.End.SetParent(null);;
                bezierRenderer.End.position = target.position;   
            }
            
            // Anchor to bone
            Transform dialogTransform = _maintenanceDialog.transform;
            dialogTransform.SetParent(_scenarioResources.ControlDialogBone);
            dialogTransform.localPosition = Vector3.zero;
            dialogTransform.localEulerAngles = Vector3.zero;
        }

        private void OnTurbineModuleRepairedEvent(TurbineModuleRepairedEvent eventData)
        {
            _scenarioResources.HideRobotDialog(_maintenanceDialog);
            if (eventData.Turbine.IsBroken == false)
            {
                _numBrokenTurbines--;
            }
        }
        
        private void Shuffle(List<TurbineController> turbines)
        {
            for (int index = 0; index < turbines.Count; index++ )
            {
                TurbineController temp = turbines[index];
                int randomIndex = Random.Range(index, turbines.Count);
                turbines[index] = turbines[randomIndex];
                turbines[randomIndex] = temp;
            }
        }
    }
}
