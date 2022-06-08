
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// Handles the animation states of the tooltips.
    /// </summary>
    public class TooltipBase : MonoBehaviour
    {
        protected const string ANIM_BOOL_IS_GAZE_HOVERED = "IsGazeHovered";
        
        [SerializeField] protected Animator _animator = null;

#pragma warning disable 0414
        [Tooltip("Skip on VR build")]
        [SerializeField] protected bool _skipOnVR = false;
#pragma warning restore 0414

        /// <summary>
        /// Has tooltip been expanded
        /// </summary>
        protected bool _hasExpanded = false;

        public event Action OnTooltipExpanded;

        private void Start()
        {
#if VRBUILD
            if (_skipOnVR)
            {
                gameObject.SetActive(false);
                return;
            }
#endif
        }

        public virtual void Reset()
        {
            if (!_animator)
            {
                _animator = GetComponent<Animator>();
            }
        }

        /// <summary>
        ///  Invoked as a callback from the MRTK IsGazeHovered.OnEntered event
        /// </summary>
        public virtual void StartGaze()
        {
            _animator.SetBool(ANIM_BOOL_IS_GAZE_HOVERED, true);
        }

        /// <summary>
        ///  Invoked as a callback from the MRTK IsGazeHovered.OnExited event
        /// </summary>
        public virtual void StopGaze()
        {
            _animator.SetBool(ANIM_BOOL_IS_GAZE_HOVERED, false);
        }
        
        /// <summary>
        ///  Invoked on anim state expand
        /// </summary>
        public virtual void OnExpand()
        {
            _hasExpanded = true;
            OnTooltipExpanded?.Invoke();
        }
    }
}
