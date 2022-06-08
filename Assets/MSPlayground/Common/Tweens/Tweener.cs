
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace MSPlayground.Common
{
    /// <summary>
    /// Base class tweener script to act as an independent MonoBehaviour component and run tweens
    /// </summary>
    public abstract class Tweener : MonoBehaviour
    {
        public enum PlayMode
        {
            PlayOnce,
            Loop,
            PingPong,
        }
        
        [SerializeField] protected float _duration = 1.0f;
        [SerializeField] protected float _delay = 0.0f;
        [SerializeField] protected AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] protected bool _playOnEnable = true;
        [SerializeField] protected PlayMode _playMode = PlayMode.PlayOnce;
        
        public UnityEvent OnCompleteTween;

        protected float _timer = 0.0f;
        protected float _timeDirectionModifier = 1.0f;
        protected bool _isPlaying = false;

        
        /// <summary>
        /// Multiplier to the speed that the tween plays at
        /// </summary>
        public float SpeedModifier { get; set; } = 1.0f;

        /// <summary>
        /// Is this tweener currently updating
        /// </summary>
        public bool IsPlaying => _isPlaying;

        private void Start()
        {
            if (_playOnEnable)
            {
                ResetToBeginning();
            }
        }

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                PlayForward();
            }
        }

        [ContextMenu(nameof(PlayForward))]
        public void PlayForward()
        {
            this.enabled = true;
            _isPlaying = true;
            _timer = 0.0f;
            _timeDirectionModifier = 1.0f;
        }

        [ContextMenu(nameof(PlayReverse))]
        public void PlayReverse()
        {
            this.enabled = true;
            _isPlaying = true;
            _timer = _duration;
            _timeDirectionModifier = -1.0f;
        }

        private void Update()
        {
            if (_isPlaying)
            {
                _timer += (Time.deltaTime * _timeDirectionModifier * SpeedModifier);

                bool playingForward = _timeDirectionModifier == 1.0f;

                float t = _timer;
                if (playingForward)
                {
                    // Only apply the delay when playing forward
                    t -= (_delay * _timeDirectionModifier);   
                }
                t = Mathf.Clamp(t, 0.0f, _duration); 
                t /= _duration;
                
                UpdateValue(t);

                if (_timer >= _duration + _delay || _timer <= 0.0f)
                {
                    OnCompleteTween?.Invoke();
                    switch (_playMode)
                    {
                        case PlayMode.PlayOnce:
                            _isPlaying = false;
                            this.enabled = false;
                            _timer = playingForward ? 0.0f : _duration;
                            break;
                        case PlayMode.Loop:
                            _timer = playingForward ? 0.0f : _duration;
                            break;
                        case PlayMode.PingPong:
                            _timeDirectionModifier = -_timeDirectionModifier;
                            _timer = playingForward ? _duration : 0.0f;
                            break;
                    }
                }
            }
        }

        [ContextMenu(nameof(ResetToBeginning))]
        public void ResetToBeginning()
        {
            if (_timeDirectionModifier == 1.0f)
            {
                // Reset to the beginning
                UpdateValue(0.0f);   
            }
            else
            {
                // Reset to the beginning
                UpdateValue(1.0f);
            }
        }

        protected abstract void UpdateValue(float t);

        protected void SetDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
        }
    }

    /// <summary>
    /// Tweener script with assumed From and To values to be the same type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Tweener<T> : Tweener
    {
        [SerializeField] protected T _from;
        [SerializeField] protected T _to;
    }
}