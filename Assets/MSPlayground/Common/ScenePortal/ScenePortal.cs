
using Microsoft.MixedReality.Toolkit;
using MSPlayground.Common;
using MSPlayground.Core.Utils;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Data;
using MSPlayground.Core;
using TMPro;
using UnityEngine;

namespace MSPlayground.Common
{
    public class ScenePortal : MonoBehaviour
    {
        /// <summary>
        /// Localization asset constants
        /// </summary>
        private const string LOC_TABLE = "ScenePortals";
        private const string LOC_KEY_PREFIX_TOOLTIP_TITLE = "portal_title_";
        private const string LOC_KEY_PREFIX_TOOLTIP_FEATURES = "portal_features_";
        private const string LOC_KEY_MOFIDIER_VR = "vr_";
        private const string KEYPATH_TITLE = "title";
        private const string KEYPATH_FEATURES = "features_list";
        
        [SerializeField] string _targetScene;

        [Header("Effects")]
        [SerializeField] ParticleSystem _idleParticleSystem;
        [SerializeField] ParticleSystem _selectedParticleSystem;
        [SerializeField] Tweener _shellActivationTweener;
        [SerializeField] float _loadDelay = 1.0f;

        [Header("SFX")]
        [SerializeField] AudioSource _audioSource;
        [SerializeField] AudioClip _orbSpawnSFX;
        [SerializeField] AudioClip _orbSelectSFX;

        /// <summary>
        /// Tooltip has child MRTKBaseInteractables with different interaction
        /// behaviours and colliders, so to avoid conflicts and squashed events
        /// we track gaze on all of them to determine whether tooltip should be shown.
        /// </summary>
        [Header("Tooltip")]
        [SerializeField] MRTKBaseInteractable[] _compositeTooltipInteractables;
        [SerializeField] GameObject _toolTipContainer;
        [SerializeField] DataSourceGODictionary _locDataSource;
        [SerializeField] RectTransform _scrollContentRect = null;
        [Tooltip("Time duration to scroll the tooltip description")] [Min(0f)]
        [SerializeField] float _scrollDescDuration = 3f;

        bool _transitionInProgress;
        /// <summary>
        /// Number of tooltip interactables that user is currently gazing at
        /// </summary>
        /// <remarks>
        /// There can be race conditions if different callbacks set bools at the same time, so integer
        /// incrementing/decrementing can help instead
        /// </remarks>
        int _tooltipGazeCount = 0;
        Tween<Vector3> _descScrollTween;
        /// <summary>
        /// The tooltip scrollrect is only auto-scrolled the very first time the tooltip
        /// is shown.
        /// </summary>
        private bool _hasAutoScrolled = false;

        private void Start()
        {
            PopulateLocalizationData();
            RegisterInteractableListeners();

            Debug.Log($"Start {_audioSource} {GetComponent<AudioSource>()}");
            PlayAudioClip(_orbSpawnSFX);

            HideTooltip();

            ToggleSelectedState(false);
           _descScrollTween = new Tween<Vector3>() {Duration = _scrollDescDuration};
        }

        private void OnDestroy()
        {
            UnregisterInteractableListeners();
        }

        /// <summary>
        /// Register the gaze and ray event listeners so that we can track
        /// whether the tooltip should be shown or not. The scene portal is
        /// a special case where we want to keep the whole tooltip visible when viewing
        /// different subcomponents that have different interactions (e.g. draggable
        /// scrollview, portal sphere button, gazeable hitbox rect that expands).
        /// </summary>
        void RegisterInteractableListeners()
        {
            foreach (var interactable in _compositeTooltipInteractables)
            {
                interactable.IsGazeHovered.OnEntered.AddListener(OnEnterGaze);
                interactable.IsGazeHovered.OnExited.AddListener(OnExitGaze);
                interactable.IsRayHovered.OnEntered.AddListener(OnEnterGaze);
                interactable.IsRayHovered.OnExited.AddListener(OnExitGaze);
            }
        }
        
        /// <summary>
        /// Unregister the gaze and event listeners for the tooltip interactables
        /// </summary>
        void UnregisterInteractableListeners()
        {
            foreach (var interactable in _compositeTooltipInteractables)
            {
                interactable.IsGazeHovered.OnEntered.RemoveListener(OnEnterGaze);
                interactable.IsGazeHovered.OnExited.RemoveListener(OnExitGaze);
                interactable.IsRayHovered.OnEntered.RemoveListener(OnEnterGaze);
                interactable.IsRayHovered.OnExited.RemoveListener(OnExitGaze);
            }
        }
        
        /// <summary>
        /// Play an audio clip
        /// </summary>
        /// <param name="audioClip">the audio clip to play</param>
        void PlayAudioClip(AudioClip audioClip)
        {
            _audioSource.clip = audioClip;
            _audioSource.Play();
        }

        /// <summary>
        /// Called when the ScenePortal is selected, initiates loading of _targetScene.
        /// </summary>
        public void OnSelected()
        {
            if (string.IsNullOrEmpty(_targetScene))
            {
                Debug.LogError($"Target scene {_targetScene} does not exist");
            }
            else
            {
                if (!_transitionInProgress)
                {
                    StartCoroutine(TransitionSceneCR());
                }
            }
        }

        /// <summary>
        /// Transition to the new scene with VFX and a short delay to wait for VFX
        /// </summary>
        /// <returns></returns>
        IEnumerator TransitionSceneCR()
        {
            ToggleSelectedState(true);
            HideTooltip();

            PlayAudioClip(_orbSelectSFX);

            GlobalEventSystem.Fire<TransitionStartEvent>(new TransitionStartEvent());

            yield return new WaitForSeconds(_loadDelay);

            SceneNavigator.GoToScene(_targetScene);
        }

        /// <summary>
        /// Invoked by MRTKBaseInteractable when Gaze or Ray is hovered
        /// on one of the composite interactables
        /// </summary>
        /// <param name="_"></param>
        void OnEnterGaze(float _)
        {
            _tooltipGazeCount++;
        }

        /// <summary>
        /// Invoked by MRTKBaseInteractable when Gaze or Ray stops hovering
        /// on one of the composite interactables
        /// </summary>
        void OnExitGaze(float _)
        {
            _tooltipGazeCount--;
        }

        void ShowTooltip()
        {
            _toolTipContainer.SetActive(true);
            if (!_hasAutoScrolled)
            {
                StartCoroutine(ScrollFeaturesDesc());
            }
        }

        void HideTooltip()
        {
            _toolTipContainer.SetActive(false);
        }

        /// <summary>
        /// Todo: This updates tooltips, which should be available through the MRTK StatefulInteractable but don't work right now.
        /// Update to use MRTK if this is fixed.
        /// 
        /// </summary>
        void LateUpdate()
        {
            // AG: Don't make any updates if the tooltip is paused. This is to ease scene transition OnSelect
            // but cannot reuse {_transitionInProgress} bool since there is a slight delay for scene load
            // where we still want tooltip paused.
            if (!_transitionInProgress)
            {
                if (_tooltipGazeCount > 0 && !_toolTipContainer.activeInHierarchy)
                {
                    ShowTooltip();
                }
                else if (_tooltipGazeCount == 0 && _toolTipContainer.activeInHierarchy)
                {
                    HideTooltip();
                }
            }
        }

        /// <summary>
        /// Populate the localization data source with automated data based on the target scene name
        /// </summary>
        private void PopulateLocalizationData()
        {
            string locKeyTitle = $"{LOC_KEY_PREFIX_TOOLTIP_TITLE}{_targetScene.ToLower()}";
            #if VRBUILD
                string locKeyFeatures = $"{LOC_KEY_PREFIX_TOOLTIP_FEATURES}{LOC_KEY_MOFIDIER_VR}{_targetScene.ToLower()}";
            #else
                string locKeyFeatures = $"{LOC_KEY_PREFIX_TOOLTIP_FEATURES}{_targetScene.ToLower()}";
            #endif
            _locDataSource.DataChangeSetBegin();
            _locDataSource.SetValue(KEYPATH_TITLE,$"{LOC_TABLE}/{locKeyTitle}");
            _locDataSource.SetValue(KEYPATH_FEATURES,$"{LOC_TABLE}/{locKeyFeatures}");
            _locDataSource.DataChangeSetEnd();
        }

        /// <summary>
        /// Toggle visual state between idle and selected
        /// </summary>
        private void ToggleSelectedState(bool selected)
        {
            if (selected)
            {
                _shellActivationTweener.PlayForward();
            }
            else
            {
                _shellActivationTweener.ResetToBeginning();
            }
            _transitionInProgress = selected;

            if (selected)
            {
                _idleParticleSystem.Stop(true);
                _selectedParticleSystem.Play(true);
            }
            else
            {
                _selectedParticleSystem.Stop(true);
                _idleParticleSystem.Play(true);
            }
        }
        
        /// <summary>
        /// Slow scroll from bottom to top of description in tooltip
        /// </summary>
        /// <returns></returns>
        IEnumerator ScrollFeaturesDesc()
        {
            if (_scrollContentRect)
            {
                yield return new WaitForEndOfFrame(); // Wait for end of frame in case there are layout changes
                _descScrollTween.From = new Vector3(0, _scrollContentRect.sizeDelta.y, 0);
                _descScrollTween.To = Vector3.zero;
                yield return StartCoroutine(_scrollContentRect.TweenPosition(_descScrollTween, Space.Self));
                _hasAutoScrolled = true;
            }
        }
    }
}
