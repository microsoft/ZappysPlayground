
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using MSPlayground.Scenarios.Hub;
using System.Collections;
using UnityEngine;

namespace MSPlayground.Core.Spatial
{
    public class RoomScanTargetController : MonoBehaviour
    {
        [SerializeField] GameObject[] _visualizerPrefabs;
        [SerializeField] DirectionalIndicator _directionalIndicator;
        [SerializeField] float _hoverDuration = 1.0f;
        [SerializeField] float _completionDelay = 0.5f;

        [Header("Audio")]
        [SerializeField] AudioClip _sfxSpawn;
        [SerializeField] AudioClip _sfxSonar;
        [SerializeField] AudioClip _sfxComplete;
        [SerializeField] float _sonarFrequency = 1.0f;
        float _sonarTimer = 0;
        AudioSource _audioSource;

        ScanVisual _targetVisual;
        StatefulInteractable _interactable;
        State _currentState = State.None;
        float _gazeHoverTime = -1f;
        int _visualSpawnIndex = 0;

        enum State
        {
            None,
            Active,
            Hovered,
            Completing,
            Complete
        }

        public bool IsComplete { get { return _currentState == State.Complete; } }
        bool IsCompletingOrComplete {  get { return _currentState == State.Completing || _currentState == State.Complete; } }

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();

            _interactable = GetComponent<StatefulInteractable>();
            _interactable.IsGazeHovered.OnEntered.AddListener(IsGazeHoveredEnter);
            _interactable.IsGazeHovered.OnExited.AddListener(IsGazeHoveredExit);
            _interactable.IsRayHovered.OnEntered.AddListener(IsGazeHoveredEnter);
            _interactable.IsRayHovered.OnExited.AddListener(IsGazeHoveredExit);

            _directionalIndicator.DirectionalTarget = transform;
            _directionalIndicator.gameObject.SetActive(false);

        }

        public void Activate(Vector3 targetPos)
        {
            _directionalIndicator.gameObject.SetActive(true);

            transform.position = targetPos;
            _currentState = State.Active;

            gameObject.SetActive(true);
            SpawnVisuals();

            PlaySFX(_sfxSpawn);

            _sonarTimer = 0;
            _gazeHoverTime = 0;
        }

        void PlaySFX(AudioClip clip)
        {
            if (clip != null)
            {
                _audioSource.Stop();
                _audioSource.clip = clip;
                _audioSource.Play();
            }
        }

        void SpawnVisuals()
        {
            _visualSpawnIndex = _visualSpawnIndex < _visualizerPrefabs.Length - 1 ? _visualSpawnIndex + 1 : 0;
            GameObject prefab = _visualizerPrefabs[_visualSpawnIndex];
            _targetVisual = GameObject.Instantiate(prefab).GetComponent<ScanVisual>();
            _targetVisual.transform.SetParent(transform, false);
            _targetVisual.transform.localPosition = Vector3.zero;
        }

        IEnumerator SetCompleteCR(float delay)
        {
            PlaySFX(_sfxComplete);

            _targetVisual.SetScanProgress(1.0f);

            _currentState = State.Completing;
            yield return new WaitForSeconds(_completionDelay);
            _currentState = State.Complete;
        }

        public void IsGazeHoveredEnter(float param)
        {
            if (!IsCompletingOrComplete)
            {
                _gazeHoverTime = 0;
                _currentState = State.Hovered;
            }
        }

        public void IsGazeHoveredExit(float param)
        {
            _gazeHoverTime = -1f;

            if (!IsCompletingOrComplete)
            {
                _currentState = State.Active;
                _targetVisual.SetScanProgress(0);
            }
        }

        private void Update()
        {
            if (_currentState == State.Active)
            {
                _sonarTimer += Time.deltaTime;
                if (_sonarTimer > _sonarFrequency)
                {
                    _sonarTimer = 0;
                    PlaySFX(_sfxSonar);
                }
            }
            else
            {
                _sonarTimer = 0;
            }

            if (_currentState==State.Hovered)
            {
                _gazeHoverTime += Time.deltaTime;
                float scanProgress = Mathf.Clamp(_gazeHoverTime / _hoverDuration, 0, 1f);

                _targetVisual.SetScanProgress(scanProgress);

                if (scanProgress >= 1.0f)
                {
                    StartCoroutine(SetCompleteCR(_completionDelay));
                }
            }
        }

        public void UpdateTarget(Vector3 targetPos)
        {
            transform.position = targetPos;
        }

        public void Deactivate()
        {
            _directionalIndicator.gameObject.SetActive(false);

            _currentState = State.None;

            gameObject.SetActive(false);

            if (_targetVisual != null)
            {
                GameObject.Destroy(_targetVisual.gameObject);
            }
        }
    }
}
