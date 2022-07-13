// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using MSPlayground.Core;
using MSPlayground.Core.UI;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines;
using MSPlayground.Turbines.Events;
using UnityEngine;

namespace MSPlayground.Scenarios.Turbines
{
    public class TurbineScenario_ExperienceIntroduction : TurbineScenario_Base
    {
        const string TURBINE_EXPERIENCE_INTRODUCTION_DIALOG_ID = "tutorial_dialog_turbine_experience_intro";
        private const string PANEL_ID_FEATURE_SUMMARY = "feature_summary_intro";

        private Panel _experienceIntroPanel;        
        private Panel _featureSummaryPanel;


        
        public override void EnterState()
        {
            base.EnterState();

            _scenarioResources.ResetEnvironmentToBeginning();

            _experienceIntroPanel = UISystem.SpawnComplexPanel(TURBINE_EXPERIENCE_INTRODUCTION_DIALOG_ID).GetComponent<Panel>();
            _experienceIntroPanel.OnButtonPressedEvent += OnButtonPressed;
            _experienceIntroPanel.OnPanelClosed += ExperienceIntroPanelClosed;
            
            GlobalEventSystem.Fire<RobotAnimationEvent>(new RobotAnimationEvent() {PowerState = RobotController.PowerState.Off});
        }

        public override void SkipState()
        {
            EndState();
        }
        

        private void OnButtonPressed(string obj)
        {
            switch (obj)
            {
                case "view_features":
                    if (_featureSummaryPanel == null)
                    {
                        var featureSummary = UISystem.SpawnComplexPanel(PANEL_ID_FEATURE_SUMMARY).GetComponent<FeatureSummary>();
                        featureSummary.Initialize(TurbineScenarioResources.FEATURE_KEYS);
                        featureSummary.OnButtonPressedEvent += OnFeatureSummaryButtonsPressed;
                        Transform mainCamTransform = Camera.main.transform;
                        Transform featureSummaryTransform = featureSummary.transform;
                        Vector3 cameraPosition = mainCamTransform.position;
                        featureSummaryTransform.position = MathHelpers.Vector3AtYPos(cameraPosition + mainCamTransform.forward, cameraPosition.y);
                        _featureSummaryPanel = featureSummary.GetComponent<Panel>();
                    }

                    _experienceIntroPanel.ClosePanel();
                    _featureSummaryPanel.gameObject.SetActive(true);
                    break;
                case "start_experience":
                    _experienceIntroPanel.ClosePanel();
                    break;
            }
        }

        private void OnFeatureSummaryButtonsPressed(string obj)
        {
            switch (obj)
            {
                case "experience_description":
                    _featureSummaryPanel.ClosePanel();
                    _experienceIntroPanel.gameObject.SetActive(true);
                    break;
                case "start_experience":
                    EndState();
                    break;
            }
        }

        private void EndState()
        {
            _scenarioResources.FocusOnObject(null);
            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }

        private void ExperienceIntroPanelClosed()
        {
            if (_featureSummaryPanel == null || _featureSummaryPanel.isActiveAndEnabled == false)
            {
                EndState();
            }
        }
    }
}