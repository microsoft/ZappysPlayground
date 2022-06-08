
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace MSPlayground.Common.Tweens
{
    /// <summary>
    /// Component to manage a group of tweener instances
    /// </summary>
    public class TweenerGroup : MonoBehaviour
    {
        [SerializeField] private Tweener[] _tweeners = new Tweener[0];

        [SerializeField] private bool _playTweensOnEnable = false;

        private int _pendingTweenerCount = 0;

        public System.Action OnGroupCompleteEvent;

        public bool IsPlaying => _pendingTweenerCount > 0;


        private void Reset()
        {
            if (_tweeners == null || _tweeners.Length == 0)
            {
                FindTweensInChildren();
            }
        }

        [ContextMenu(nameof(FindTweensInChildren))]
        private void FindTweensInChildren()
        {
            _tweeners = this.GetComponentsInChildren<Tweener>(true);
        }

        private void OnEnable()
        {
            for (int i = 0; i < _tweeners.Length; ++i)
            {
                _tweeners[i].OnCompleteTween.AddListener(OnTweenerComplete);
            }

            if (_playTweensOnEnable)
            {
                for (int i = 0; i < _tweeners.Length; ++i)
                {
                    _tweeners[i].PlayForward();
                }   
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _tweeners.Length; ++i)
            {
                _tweeners[i].OnCompleteTween.RemoveListener(OnTweenerComplete);
            }
        }

        public void PlayForward()
        {
            _pendingTweenerCount = _tweeners.Length;
            for (int i = 0; i < _tweeners.Length; ++i)
            {
                _tweeners[i].ResetToBeginning();
                _tweeners[i].PlayForward();
            }
        }

        public void PlayReverse()
        {
            _pendingTweenerCount = _tweeners.Length;
            for (int i = 0; i < _tweeners.Length; ++i)
            {
                _tweeners[i].PlayReverse();
            }
        }

        private void OnTweenerComplete()
        {
            _pendingTweenerCount--;
            if (_pendingTweenerCount == 0)
            {
                OnGroupCompleteEvent?.Invoke();
            }
        }

        public void ResetToBeginning()
        {
            for (int i = 0; i < _tweeners.Length; ++i)
            {
                _tweeners[i].ResetToBeginning();
            }
        }
    }
}