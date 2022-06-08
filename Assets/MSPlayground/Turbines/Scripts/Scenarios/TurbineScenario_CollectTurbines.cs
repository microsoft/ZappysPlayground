// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using Microsoft.MixedReality.Toolkit.Data;
using MSPlayground.Core;
using MSPlayground.Core.Data;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines;
using MSPlayground.Turbines.Events;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// Turbine scenario script for player to gather turbines and place them on the docking platform
    /// </summary>
    public class TurbineScenario_CollectTurbines : TurbineScenario_Base
    {
        const string FIND_AND_REPAIR_TUTORIAL_ID = "tutorial_dialog_find_and_repair";
        const string FOUR_REMAINING_TURBINES_DIALOG_ID = "robot_dialog_good_job_four_turbines_remaining";

        [SerializeField] AudioClip _sfxAllSet;
        
        int _numTurbinesDocked = 0;
        DataSourceLocalizationVariables _locVariablesDataSource;

        private IntVariable _total;
        private IntVariable _num;
        private IntVariable _remaining;
        
        private int TotalNumTurbines => _scenarioResources.Turbines.Length;


        public override void ResetState()
        {
            base.ResetState();
            _numTurbinesDocked = 0;
        }

        public override void EnterState()
        {
            base.EnterState();
            _numTurbinesDocked = 0;
            foreach (var turbine in _scenarioResources.Turbines)
            {
                if (turbine.IsDocked)
                {
                    _numTurbinesDocked++;
                }
            }

            if (_numTurbinesDocked >= _scenarioResources.Turbines.Length)
            {
                // Skip this step
                GoToNextState();
            }
            else
            {
                _total = new IntVariable(){Value = TotalNumTurbines};
                _num = new IntVariable(){Value = _numTurbinesDocked};
                _remaining = new IntVariable(){Value = TotalNumTurbines - _numTurbinesDocked};

                GlobalEventSystem.Register<TurbinePlatformEvent>(TurbinePlatformEventHandler);
                StartCoroutine(SequenceRoutine());   
            }
        }

        public override void ExitState()
        {
            GlobalEventSystem.Unregister<TurbinePlatformEvent>(TurbinePlatformEventHandler);
            base.ExitState();
        }

        public override void SkipState()
        {
            _scenarioResources.SetupTurbineLocations();
            _scenarioResources.MuteTurbines(false);
            _scenarioResources.ShowTurbines(true);
            DockAllTurbines();

            _scenarioResources.FocusOnObject(null);
            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }

        private void FocusTurbine()
        {
            Transform focusTarget = null;
            foreach (var turbine in _scenarioResources.Turbines)
            {
                if (turbine.IsDocked)
                {
                    // No need to bring attention to a turbine that is already docked
                    continue;
                }
                else if (turbine.IsPicked)
                {
                    // Focus target onto the platform
                    focusTarget = _scenarioResources.PlatformGameObject.transform;
                    break;
                }

                focusTarget = turbine.transform;
            }

            _scenarioResources.FocusOnObject(focusTarget);
        }

        IEnumerator SequenceRoutine()
        {
            UISystem.SpawnComplexPanel(FIND_AND_REPAIR_TUTORIAL_ID, out GameObject findAndRepairDialogInstance, out _);
            _locVariablesDataSource = findAndRepairDialogInstance.GetComponent<DataSourceLocalizationVariables>();
            SetupLocalizationVariables(_locVariablesDataSource);

            FocusTurbine();
            _scenarioResources.MuteTurbines(false);
            _scenarioResources.ShowTurbines(true);
            _scenarioResources.ShowPlatform(true);
            
            // Wait for at least one turbine being docked to update the robot dialog
            while (_numTurbinesDocked <= 0)
            {
                yield return null;
            }
            GameObject robotTurbineCounterDialog = _scenarioResources.ShowRobotDialog(FOUR_REMAINING_TURBINES_DIALOG_ID);
            SetupLocalizationVariables(robotTurbineCounterDialog.AddComponent<DataSourceLocalizationVariables>());
            
            GlobalEventSystem.Fire<RobotSFXEvent>(new RobotSFXEvent() {SfxType = SFXType.Glitch});
            // Wait for all turbines to be docked
            while (_numTurbinesDocked < _scenarioResources.Turbines.Length)
            {
                yield return null;
            }
            _scenarioResources.PlatformAudioSource.PlayOneShot(_sfxAllSet);

            _scenarioResources.FocusOnObject(null);
            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }

        void TurbinePlatformEventHandler(TurbinePlatformEvent obj)
        {
            _numTurbinesDocked += obj.Docked ? 1 : -1;

            _num.Value = _numTurbinesDocked;
            _remaining.Value = TotalNumTurbines - _numTurbinesDocked;
            
            FocusTurbine();
        }

        [ContextMenu(nameof(DockAllTurbines))]
        private void DockAllTurbines()
        {
            for (int i = 0; i < _scenarioResources.Turbines.Length; ++i)
            {
                TurbineController turbine = _scenarioResources.Turbines[i];
                GameObject dock = _scenarioResources.TurbineDocks[i];

                Transform turbineTransform = turbine.transform;
                turbineTransform.position = dock.transform.position + (Vector3.up * 0.25f);

                Vector3 rotation = turbineTransform.eulerAngles;
                rotation = Vector3.RotateTowards(rotation, Vector3.right, Random.Range(-360, 360), 1.0f);
                turbine.transform.eulerAngles = rotation;
                
                turbine.ForceOntoDock(dock.transform);
            }
        }
        
        /// <summary>
        /// Helper to test localization counters as turbines are docked one by one
        /// </summary>
        [ContextMenu(nameof(Dock1MoreTurbine))]
        private void Dock1MoreTurbine()
        {
            for (int i = 0; i < _scenarioResources.Turbines.Length; ++i)
            {
                TurbineController turbine = _scenarioResources.Turbines[i];
                if (!turbine.IsDocked)
                {
                    GameObject dock = _scenarioResources.TurbineDocks[i];

                    Transform turbineTransform = turbine.transform;
                    turbineTransform.position = dock.transform.position + (Vector3.up * 0.25f);

                    Vector3 rotation = turbineTransform.eulerAngles;
                    rotation = Vector3.RotateTowards(rotation, Vector3.right, Random.Range(-360, 360), 1.0f);
                    turbine.transform.eulerAngles = rotation;

                    turbine.ForceOntoDock(dock.transform);
                    break;
                }
            }
        }

        /// <summary>
        /// Set the input data source with the shared counter IVariables
        /// </summary>
        /// <param name="dataSource"></param>
        void SetupLocalizationVariables(DataSourceLocalizationVariables dataSource)
        {
            if (dataSource != null)
            {
                dataSource.DataChangeSetBegin();
                dataSource.LocalVariablesByID["total"] = _total;
                dataSource.LocalVariablesByID["num"] = _num;
                dataSource.LocalVariablesByID["remaining_num"] = _remaining;
                dataSource.DataChangeSetEnd();
            }
        }
    }
}