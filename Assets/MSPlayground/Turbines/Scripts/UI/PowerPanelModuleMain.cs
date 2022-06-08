// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace MSPlayground.Turbines
{
    /// <summary>
    /// UI Control for the main power allocation of the power panel
    /// </summary>
    public class PowerPanelModuleMain : PowerPanelModule
    {
        /// <summary>
        /// Directly controlled from the PowerPanel how much controlled power we have
        /// </summary>
        /// <param name="percentage"></param>
        public void SetControlledPowerOutput(float percentage)
        {
            _controlledPowerOutput = percentage;
            _pendingUpdateState = true;
        }
    }
}