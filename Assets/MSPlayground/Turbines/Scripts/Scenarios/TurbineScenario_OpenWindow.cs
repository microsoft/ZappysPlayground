using System.Collections;
using MSPlayground.Core;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines;
using MSPlayground.Turbines.Events;
using UnityEngine;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// Turbine scenario script for player to open window
    /// </summary>
    public class TurbineScenario_OpenWindow : TurbineScenario_Base
    {
        private const string OPEN_WINDOW_DIALOG_ID = "robot_dialog_lets_open_the_window";
        public override void EnterState()
        {
            base.EnterState();
            
            if (_scenarioResources.WindowController.IsWindowOpen)
            {
                // The user has already opened this window, no need to wait for the event
                GoToNextState();
            }
            else
            {
                StartCoroutine(SequenceRoutine());
            }
        }

        public override void SkipState()
        {
            _scenarioResources.WindowController.OpenOrCloseWindow(true, true);
            _scenarioResources.FocusOnObject(null);
            UISystem.DespawnAllActivePanels();
            GoToNextState();
        }

        protected IEnumerator SequenceRoutine()
        {
            _scenarioResources.FocusOnObject(_scenarioResources.WindowController.transform);
            GlobalEventSystem.Fire<RobotSFXEvent>(new RobotSFXEvent() {SfxType = SFXType.Glitch});
            GameObject robotDialog = _scenarioResources.ShowRobotDialog(OPEN_WINDOW_DIALOG_ID);
            yield return WaitForGlobalEvent<OpenWindowEvent>();
            _scenarioResources.HideRobotDialog(robotDialog);
            _scenarioResources.FocusOnObject(null);
            
            GoToNextState();
        }
    }
}
