
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using MSPlayground.Core;
using UnityEngine;

namespace MSPlayground.Scenarios.Hub
{
    /// <summary>
    /// Scenario where user can set their name
    /// </summary>
    public class HubScenario_EnterName : HubScenario_Base
    {
        private const string PANEL_ID = "hub_enter_name";
        private GameObject _panel = null;
        
        public override void EnterState()
        {
            base.EnterState();

            if (SceneNavigator.BypassSetupFlow)
            {
                GoToNextState();
            }
            else
            {
                StartCoroutine(SequenceRoutine());
            }
        }

        IEnumerator SequenceRoutine()
        {
            _panel = _scenarioResources.ShowDialog(PANEL_ID, rotateToCamera: true);
            yield return WaitForDialogClosed(_panel);

            GoToNextState();
        }
        
        public override void SkipState()
        {
            UISystem.DespawnPanel(_panel);
            GoToNextState();
        }
    }
}
