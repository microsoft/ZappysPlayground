
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Utils
{
    /// <summary>
    /// A helper class used to send the PlayOneShotEvent to a listening
    /// shared audio source.
    /// </summary>
    /// <remarks>Useful for playing sound effects on object destroy</remarks>
    public class PlayOneShotEventSender : MonoBehaviour
    {
        public void PlayOneShot(AudioClip audioClip)
        {
            GlobalEventSystem.Fire(new PlayOneShotEvent()
            {
                AudioClip = audioClip
            });   
        }
    }
}