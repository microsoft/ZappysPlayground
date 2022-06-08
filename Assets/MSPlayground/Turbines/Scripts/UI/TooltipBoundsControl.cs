
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using MSPlayground.Core.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace MSPlayground.Turbines
{
    /// <summary>
    /// The bounds control tooltip has special behaviour so this script manages that instead
    /// of cluttering the generic TooltipStandalone script.
    /// </summary>
    public class TooltipBoundsControl : TooltipBase
    {
        private const string ANIM_TRIGGER_CAN_ANIMATE_IN = "CanAnimateIn";

        [Header("Animation Configuration")]
        [Tooltip("Bounds controls to keep track of for animating in conditions. One tooltip is shared among these")]
        [SerializeField] private BoundsControl[] _boundsControls = null;
        [Tooltip("Position offset of the tooltip when it is translated over to the last interacted object")]
        [SerializeField] private Vector3 _positionOffset;
        [Tooltip("Position offset of the tooltip when it is translated over to the last interacted object")]
        [SerializeField] private Vector3 _vrPositionOffset;
        [Tooltip("Audio source to play on animate in")]
        [SerializeField] private AudioSource _audioSource = null;

        /// <summary>
        /// Has the tooltip animated in at least one time
        /// </summary>
        private bool _hasAnimatedInOnce = false;
        
        /// <summary>
        /// The bounds control gameObject that the tooltip is currently highlighting
        /// </summary>
        private GameObject _trackedObject = null;
        
        private void Start()
        {
            if (_boundsControls != null && _boundsControls.Length != 0)
            {
                foreach (var bounds in _boundsControls)
                {
                    bounds.ManipulationStarted.AddListener(OnSharedTooltipInteract);
                }
            }
        }
        
        private void OnDestroy()
        {
            RemoveEventListeners();
        }
        
        /// <summary>
        /// Event callback when the boundsControl rotate has started.
        /// </summary>
        /// <param name="target">GameObject of the interacted object</param>
        private void OnSharedTooltipInteract(SelectEnterEventArgs eventArgs)
        {
            _animator.SetTrigger(ANIM_TRIGGER_CAN_ANIMATE_IN);
            
            // If the latest interaction was from a different object, then play the flashIn animation.
            // Get the parent of the interactable, as we do not care about the specific rotation handle,
            // so much as the object to which this handle belongs.
            GameObject target = eventArgs.interactableObject.transform.parent.gameObject;

            // If the tooltip has already been expanded then don't flash in again, just move the position
            if (target != _trackedObject && !_hasExpanded)
            {
                _animator.Play("FlashIn");
            }

            _trackedObject = target;

            // Move position of the tooltip to above the last interacted object
#if VRBUILD
            transform.position = target.transform.position + _vrPositionOffset;
#else
            transform.position = target.transform.position + _positionOffset;
#endif
        }

#region Animation Events
        /// <summary>
        /// Invoked on fade out animation complete
        /// </summary>
        public void OnAnimateOutComplete()
        {
            // Destroy after animate out if it has already been expanded once
            if (_hasExpanded)
            {
                Destroy(gameObject);
            }
            else
            {
                // Reset animation state for reuse
                _animator.ResetTrigger(ANIM_TRIGGER_CAN_ANIMATE_IN);
                _animator.Play("HideUntilInView");
            }
        }
        
                
        /// <summary>
        /// Invoked on animate in
        /// </summary>
        public void OnAnimateIn()
        {
            // Only play the animate in SFX the first time to avoid being spammy
            if (_audioSource && !_hasAnimatedInOnce)
            {
                _audioSource.Play();
                _hasAnimatedInOnce = true;
            }
        }
#endregion

        private void RemoveEventListeners()
        {
            if (_boundsControls != null && _boundsControls.Length != 0)
            {
                foreach (var bounds in _boundsControls)
                {
                    bounds.ManipulationStarted.RemoveListener(OnSharedTooltipInteract);
                }
            }
        }
    }
}
