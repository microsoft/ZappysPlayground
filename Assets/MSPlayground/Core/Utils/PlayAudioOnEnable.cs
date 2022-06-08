
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Utils
{
    /// <summary>
    /// Helper class to play audio on gameObject enable
    /// </summary>
    public class PlayAudioOnEnable : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource = null;

        private void Reset()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnEnable()
        {
            _audioSource.Play();
        }
    }
}
