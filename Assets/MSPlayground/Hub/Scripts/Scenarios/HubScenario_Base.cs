
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core.Scenario;
using UnityEngine;

namespace MSPlayground.Scenarios.Hub
{
    public class HubScenario_Base : Scenario
    {
        [SerializeField] protected HubScenarioResources _scenarioResources;

        protected override void Reset()
        {
            base.Reset();
            if (_scenarioResources == null)
            {
                _scenarioResources = this.GetComponentInParent<HubScenarioResources>();
            }
        }
    }
}
