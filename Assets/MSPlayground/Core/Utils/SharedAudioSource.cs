
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Utils
{
    #region Global Events
    /// <summary>
    /// Global event used to play one shot audio from a shared audio source.
    /// </summary>
    /// <remarks>Used when audio does not need to be spatialized, and especially for handling SFX audio
    /// in between despawning panels. This way the audio can play out even while UI objects are being destroyed.
    /// </remarks>
    public class PlayOneShotEvent : BaseEvent
    {
        /// <summary>
        /// AudioClip to play
        /// </summary>
        public AudioClip AudioClip;
    }
    #endregion
    
    /// <summary>
    /// A non-spatialized shared audio source used for managing sound effects in UI flows where sounds must be triggered
    /// while panels are being destroyed.
    /// </summary>
    public class SharedAudioSource : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource = null;
        [Tooltip("Default volume to play one shot audio")]
        [Range(0f, 1f)] [SerializeField] private float _defaultVolume = 1f;

        private void Start()
        {
            GlobalEventSystem.Register<PlayOneShotEvent>(OnPlayOneShotEvent);
        }

        private void Reset()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnPlayOneShotEvent(PlayOneShotEvent eventData)
        {
            _audioSource.PlayOneShot(eventData.AudioClip, _defaultVolume);
        }
    }
}
