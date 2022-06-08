
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using MSPlayground.Core;
using UnityEngine;

namespace MSPlayground.Scenarios.Hub
{
    public class HubScenario_Intro : HubScenario_Base
    {
        private const string PANEL_ID_WELCOME_NEW_USER = "hub_welcome_new";
        [SerializeField] HubScenario_Base _nextScenarioFirstTimeUser;
        [SerializeField] HubScenario_Base _nextScenarioReturningUser;

        private GameObject _panel = null;
        public override void EnterState()
        {
            base.EnterState();
            StartCoroutine(SequenceRoutine());
        }

        IEnumerator SequenceRoutine()
        {
            yield return null;
            if (_scenarioResources.AreGamestatesEmpty)
            {
                _panel = _scenarioResources.ShowDialog(PANEL_ID_WELCOME_NEW_USER);
                yield return WaitForDialogClosed(_panel);
                GoToCustomState(_nextScenarioFirstTimeUser);
            }
            else
            {
                GoToCustomState(_nextScenarioReturningUser);
            }
        }

        public override void SkipState()
        {
            // The reference is for the same scenario, both cases are handled in HubScenario_GenerateRoom
            GoToCustomState(_nextScenarioFirstTimeUser);
        }

        public override void ExitState()
        {
            if (_panel)
            {
                UISystem.DespawnPanel(_panel);
                _panel = null;
            }
            base.ExitState();
        }
    }
}
