
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// Handles the animation states of standalone tooltips.
    /// Unlike the base tooltip, standalone tips only trigger the animator to fade in when
    /// the entire rect is in full view for the first time.
    /// </summary>
    public class TooltipStandalone : TooltipBase
    {
        private enum AnimateInCondition
        {
            /// <summary>
            /// When rect transform is in full view for the first time
            /// </summary>
            InFullView = 0,
            /// <summary>
            ///  XR event trigger on firstSelectEntered
            /// </summary>
            OnSelectEnter,
            /// <summary>
            ///  When the referenced stateful interactable has been clicked
            /// </summary>
            OnClick
        }

        /// <summary>
        /// Condition to destroy the gameobject
        /// </summary>
        private enum DestroyCondition
        {
            /// <summary>
            /// On animate out (fade)
            /// </summary>
            OnAnimateOut = 0,
            /// <summary>
            /// On animate out after the first time user expands the tooltip
            /// </summary>
            OnAnimateOutAfterExpand,
            /// <summary>
            /// Allow gameobject to be active, but still alpha 0
            /// </summary>
            DontDestroy
        }
        
        private const string ANIM_TRIGGER_CAN_ANIMATE_IN = "CanAnimateIn";
        private const string ANIM_STATE_HIDDEN = "HideUntilInView";
        
        [Header("Animate In")]
        [Tooltip("Conditions to animate the tooltip in")]
        [SerializeField] private AnimateInCondition _animateInCondition = 0;
        [Tooltip("Wait until this rect transform is in full view of the camera before animating in")]
        [SerializeField] private RectTransform _rectTransform = null;
        [Tooltip("Stateful interactable to keep track of for animating in conditions")]
        [SerializeField] private StatefulInteractable _statefulInteractable = null;
        [Tooltip("Audio source to play on animate in")]
        [SerializeField] private AudioSource _audioSource = null;

        [Header("Destroy Condition")]
        [Tooltip("Condition to destroy the tooltip")]
        [SerializeField] private DestroyCondition _destroyCondition = 0;
        
        [Header("UI")]
        [SerializeField] private Image _onGazeSlicedImage = null;
        
        /// <summary>
        /// Has the tooltip animated in at least one time
        /// </summary>
        private bool _hasAnimatedInOnce = false;

        public void Start()
        {
#if VRBUILD
            if (_skipOnVR)
            {
                gameObject.SetActive(false);
                return;
            }
#endif

            // Add persistent listeners for animate in condition
            switch (_animateInCondition)
            {
                case AnimateInCondition.OnSelectEnter:
                    if (_statefulInteractable)
                    {
                        _statefulInteractable.firstSelectEntered.AddListener(OnSelectEnter);
                    }
                    break;
                case AnimateInCondition.OnClick:
                    if (_statefulInteractable)
                    {
                        _statefulInteractable.OnClicked.AddListener(OnStatefulGeneralInteract);
                    }
                    break;
            }
        }
        
        public void OnEnable()
        {
            // Wait for animate in condition
            switch (_animateInCondition)
            {
                case AnimateInCondition.InFullView:
                    if (_rectTransform)
                    {
                        StartCoroutine(WaitUntilInFullView());
                    }
                    break;
            }
        }

        /// Will happen externally or if parent objects are disabled
        /// Destroy tooltip as necessary in this case
        public void OnDisable()
        {
            if (_destroyCondition == DestroyCondition.OnAnimateOutAfterExpand &&
                _hasExpanded)
            {
                Destroy(gameObject);
            }
            else
            {
                ResetAnimationState();
            }
        }

        public override void Reset()
        {
            base.Reset();
            if (!_rectTransform)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
        }
        
        private void OnDestroy()
        {
            RemoveStatefulEventListener();
        }

#region Animation Events
        /// <summary>
        /// Invoked on fade out animation complete
        /// </summary>
        public void OnAnimateOutComplete()
        {
            switch (_destroyCondition)
            {
                case DestroyCondition.OnAnimateOut:
                    Destroy(gameObject);
                    break;
                case DestroyCondition.OnAnimateOutAfterExpand:
                    if (_hasExpanded)
                    {
                        Destroy(gameObject);
                    }
                    break;
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
        
#region Event Listeners
        private void OnSelectEnter(SelectEnterEventArgs args)
        {
            OnStatefulGeneralInteract();
        }
        
        private void OnStatefulGeneralInteract()
        {
            _animator.SetTrigger(ANIM_TRIGGER_CAN_ANIMATE_IN);
            RemoveStatefulEventListener();
        }
#endregion

        /// <summary>
        /// Only animate in when the entire rect is in full view within the viewframe
        /// </summary>
        private IEnumerator WaitUntilInFullView()
        {
            if (!_rectTransform)
            {
                yield break;
            }
            _animator.Play(ANIM_STATE_HIDDEN); // Force reset animation to hidden state
            while (!_rectTransform.IsFullyVisibleFrom(Camera.main))
            {
                yield return null;
            }

            _animator.SetTrigger(ANIM_TRIGGER_CAN_ANIMATE_IN);
        }

        private void RemoveStatefulEventListener()
        {
            if (_statefulInteractable)
            {
                switch (_animateInCondition)
                {
                    case AnimateInCondition.OnClick:
                        _statefulInteractable.OnClicked.RemoveListener(OnStatefulGeneralInteract);
                        break;
                    case AnimateInCondition.OnSelectEnter:
                        _statefulInteractable.firstSelectEntered.RemoveListener(OnSelectEnter);
                        break;
                }
            }
        }

        /// <summary>
        /// Reset animation state of the tooltip
        /// </summary>
        private void ResetAnimationState()
        {
            _animator.ResetTrigger(ANIM_TRIGGER_CAN_ANIMATE_IN);
            base.StopGaze();
            _onGazeSlicedImage.fillAmount = 0;
        }
    }
}
