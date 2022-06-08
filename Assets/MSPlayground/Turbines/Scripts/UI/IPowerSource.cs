
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace MSPlayground.Turbines
{
    /// <summary>
    /// Power source interface to allow objects to collect power from multiple different types of sources
    /// </summary>
    public interface IPowerSource
    {
        /// <summary>
        /// Current value from a range of 0 to 1
        /// </summary>
        float PowerSourceOutput { get; }

        /// <summary>
        /// Event raised when the Power value changes
        /// </summary>
        event System.Action<float> OnPowerUpdatedEvent;
    }
}