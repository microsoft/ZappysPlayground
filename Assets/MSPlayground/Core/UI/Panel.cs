// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using MSPlayground.Common.Tweens;
using UnityEngine;

namespace MSPlayground.Core
{
    /// <summary>
    /// Basic UI script to display to the user with a potential continue button to close the popup
    /// </summary>
    public class Panel : MonoBehaviour
    {
        [SerializeField] private TweenerGroup _tweenerGroup;
        
        public event Action<string> OnButtonPressedEvent;
        public event Action OnPanelClosed;

        private void Reset()
        {
            if (_tweenerGroup == null)
            {
                _tweenerGroup = this.GetComponent<TweenerGroup>();
            }
        }

        protected virtual void OnEnable()
        {
            // Always orient towards the user so they may read the panel
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            PlayIntro();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// Event handler for the continue button
        /// </summary>
        public void OnContinueButtonPressed()
        {
            ClosePanel();
        }

        /// <summary>
        /// Opens the panel by setting it active.
        /// </summary>
        public void ShowPanel()
        {
            if (_tweenerGroup != null)
            {
                _tweenerGroup.ResetToBeginning();
                _tweenerGroup.PlayForward();
            }
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Closes panel by playing outro aesthetics before disabling gameobject
        /// </summary>
        public void ClosePanel()
        {
            ClosePanel(null);
        }

        /// <summary>
        /// Closes panel by playing outro aesthetics before disabling gameobject
        /// </summary>
        public void ClosePanel(System.Action closeTransitionCompleteCallback)
        {
            if (isActiveAndEnabled)
            {
                if (_tweenerGroup == null)
                {
                    gameObject.SetActive(false);
                    OnPanelClosed?.Invoke();
                }
                else
                {
                    void tweenGroupCompleteCallback()
                    {
                        gameObject.SetActive(false);
                        closeTransitionCompleteCallback?.Invoke();
                        _tweenerGroup.OnGroupCompleteEvent -= tweenGroupCompleteCallback;
                        OnPanelClosed?.Invoke();
                    }

                    _tweenerGroup.OnGroupCompleteEvent += tweenGroupCompleteCallback;
                    _tweenerGroup.PlayReverse();
                }
            }
        }

        /// <summary>
        /// Play intro tween
        /// </summary>
        [ContextMenu(nameof(PlayIntro))]
        public void PlayIntro()
        {
            if (_tweenerGroup != null)
            {
                _tweenerGroup.PlayForward();   
            }
        }

        /// <summary>
        /// Play outro tween
        /// </summary>
        [ContextMenu(nameof(PlayOutro))]
        public void PlayOutro()
        {
            _tweenerGroup.PlayReverse();
        }

        /// <summary>
        /// Button callback to raise button IDs as event parameters for external objects to listen for
        /// </summary>
        /// <param name="buttonID"></param>
        public void OnButtonPressed(string buttonID)
        {
            OnButtonPressedEvent?.Invoke(buttonID);            
        }
    }
}
