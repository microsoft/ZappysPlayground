// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace MSPlayground.Common
{
    /// <summary>
    /// Extension functions to help control particle systems.
    /// </summary>

    public static class ParticleSystemExtensions
    {
        /// <summary>
        /// Extension function to set whether or not the emission of a particle system is enabled.
        /// This allows particles to activate and deactivate more gracefully than through enabling or disabling GameObjects.
        /// </summary>
        public static void SetEmissionEnabled(this ParticleSystem ps, bool enabled, bool includeChildren = true)
        {
            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = enabled;
            if (includeChildren)
            {
                foreach (ParticleSystem c in ps.GetComponentsInChildren<ParticleSystem>())
                {
                    if (c != ps)
                    {
                        c.SetEmissionEnabled(enabled);
                    }
                }
            }
        }
    }
}
