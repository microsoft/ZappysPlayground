// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Linq;
using MSPlayground.Common.Helper;
using MSPlayground.Common.UI;
using MSPlayground.Core;
using MSPlayground.Core.UI;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines;
using MSPlayground.Turbines.Events;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// Turbine scenario script to display the feature summary upon experience completion
    /// </summary>
    public class TurbineScenario_EndSummary : TurbineScenario_Base
    {
        private const string PANEL_ID_FEATURE_SUMMARY = "feature_summary_end";

        enum ExitBehaviour
        {
            None,
            RestartExperience,
            ReturnToHub
        }

        private ExitBehaviour _exitBehaviour = ExitBehaviour.None;

        public override void EnterState()
        {
            base.EnterState();

            StartCoroutine(SequenceRoutine());
        }

        private IEnumerator SequenceRoutine()
        {
            UISystem.DespawnAllActivePanels();
            GameObject panelObject = UISystem.SpawnComplexPanel(PANEL_ID_FEATURE_SUMMARY);
            Transform mainCamTransform = Camera.main.transform;
            panelObject.transform.position = MathHelpers.Vector3AtYPos(mainCamTransform.position + mainCamTransform.forward * 1.0f, mainCamTransform.position.y);

            FeatureSummary featureSummary = panelObject.GetComponent<FeatureSummary>();
            featureSummary.Initialize(TurbineScenarioResources.FEATURE_KEYS);
            featureSummary.OnButtonPressedEvent += OnFeatureSummaryButtonsPressed;

            _exitBehaviour = ExitBehaviour.None;
            yield return new WaitUntil(()=>_exitBehaviour != ExitBehaviour.None);
            UISystem.DespawnAllActivePanels();

            GlobalEventSystem.Fire<RobotAnimationEvent>(new RobotAnimationEvent() { PowerState = RobotController.PowerState.Overloading });
            _scenarioResources.PowerPanel.StartEndSequence();
            yield return new WaitForSeconds(4f);
            GlobalEventSystem.Fire<RobotAnimationEvent>(new RobotAnimationEvent() { PowerState = RobotController.PowerState.Overloaded });
            _scenarioResources.KnockOffTurbines();
            yield return new WaitForSeconds(4f);

            if (_exitBehaviour == ExitBehaviour.RestartExperience)
            {
                SceneNavigator.GoToScene("Turbines");
            }
            else
            {
                SceneNavigator.GoToScene("Hub");
            }

            GoToNextState();
        }

        private void OnFeatureSummaryButtonsPressed(string obj)
        {
            switch (obj)
            {
                case "restart_experience":
                    _exitBehaviour = ExitBehaviour.RestartExperience;
                    break;
                case "return_to_hub":
                    _exitBehaviour = ExitBehaviour.ReturnToHub;
                    break;
            }
        }
    }
}