// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace MSPlayground.Turbines
{
    /// <summary>
    /// Sound effect types for the robot
    /// </summary>
    public enum SFXType
    {
        /// <summary>
        /// Neutral variations
        /// </summary>
        General = 0,
        /// <summary>
        /// Negative connotation variations
        /// </summary>
        Glitch,
        /// <summary>
        /// Positive connotation variations
        /// </summary>
        Positive
    }
    
    /// <summary>
    /// The RobotSFXTheme profile allows users to customize their Turbines experience
    /// with different robot sound effect presets.
    /// </summary>
    [CreateAssetMenu(fileName = "Robot_SFX_Theme", menuName = "MRTK/Robot SFX Theme")]
    public class RobotSFXTheme : ScriptableObject
    {
        /// <summary>
        /// Audio clips organized by sound effect type
        /// </summary>
        /// <remarks>
        /// We leverage the Microsoft.MixedReality.Toolkit.SerializableDictionary so we can set these
        /// clips in the Unity Inspector as a dictionary.
        /// </remarks>
        public SerializableDictionary<SFXType, AudioClip[]> SfxByType;

        /// <summary>
        /// The serializable dictionary can have issues with adding new entries since duplicate keys are not allowed.
        /// Reset() will automatically populate the dictionary with all sfx types.
        /// </summary>
        private void Reset()
        {
            if (SfxByType == null || SfxByType.Count == 0)
            {
                SfxByType = new SerializableDictionary<SFXType, AudioClip[]>();
                foreach (SFXType sfxType in Enum.GetValues(typeof(SFXType)))
                {
                    SfxByType[sfxType] = null;
                }
            }
        }
    }
}