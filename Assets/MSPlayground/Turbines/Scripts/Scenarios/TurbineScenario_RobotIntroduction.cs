// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using MSPlayground.Core;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines;
using MSPlayground.Turbines.Events;
using UnityEngine;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// Turbine scenario script to introduce the player to robot helper
    /// </summary>
    public class TurbineScenario_RobotIntroduction : TurbineScenario_Base
    {
        const string LOW_BATTERY_UI_ID = "LowBattery";
        const string ROBOT_NEEDS_HELP_DIALOG_ID = "robot_needs_help_dialog";

        public override void EnterState()
        {
            base.EnterState();
            StartCoroutine(SequenceRoutine());
        }

        public override void SkipState()
        {
            _scenarioResources.FocusOnObject(null);
            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }

        IEnumerator SequenceRoutine()
        {
            // Setup state for player to approach the robot
            _scenarioResources.Robot.SetActive(true);
            _scenarioResources.FocusOnObject(_scenarioResources.Robot.transform);
            yield return new WaitForSeconds(2f);
            
            // Wait for player to close dialog box
            GlobalEventSystem.Fire<RobotSFXEvent>(new RobotSFXEvent() {SfxType = SFXType.Glitch});
            GlobalEventSystem.Fire<RobotAnimationEvent>(new RobotAnimationEvent() {PowerState = RobotController.PowerState.Low});
            yield return WaitForDialogClosed(_scenarioResources.ShowRobotDialog(ROBOT_NEEDS_HELP_DIALOG_ID));

            _scenarioResources.FocusOnObject(null);
            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }
    }
}