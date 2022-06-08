// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core;
using MSPlayground.Core.Scenario;
using UnityEngine;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// Base turbine scenario class to inherit from when creating more turbine steps
    /// </summary>
    public class TurbineScenario_Base : Scenario
    {
        [SerializeField] protected TurbineScenarioResources _scenarioResources;

        protected override void Reset()
        {
            base.Reset();
            if (_scenarioResources == null)
            {
                _scenarioResources = this.GetComponentInParent<TurbineScenarioResources>();
            }
        }
    }
}