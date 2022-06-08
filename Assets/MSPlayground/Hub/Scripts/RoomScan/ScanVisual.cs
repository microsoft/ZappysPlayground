// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSPlayground.Common;
using Microsoft.MixedReality.GraphicsTools;

namespace MSPlayground.Scenarios.Hub
{
    public class ScanVisual : MonoBehaviour
    {
        [Header("Mesh")]
        [SerializeField] private Transform _fillTransform;
        [SerializeField] private MeshRenderer _innerMeshRenderer;
        [SerializeField] private Color _fillColor = Color.white;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] [Min(0.01f)] private float _fillChangeTime = 0.2f;
        [SerializeField] private string _completeAnimationTrigger = "Complete";

        [Header("Effects")]
        [SerializeField] private ParticleSystem _completeParticles;

        private Material _fillMaterial;
        
        private float _scanProgress;
        private float _displayedProgress;

        private Coroutine _fillRoutine;
        private float _fillRoutineTimer;
        private float _fillRoutineStartValue;

        private void Awake()
        {
            //Create material instance
            _fillMaterial = _innerMeshRenderer.EnsureComponent<MaterialInstance>().Material;
            _fillMaterial.color = _fillColor;

            Reset();
        }

        public void Reset()
        {
            if (_fillRoutine != null)
            {
                StopCoroutine(_fillRoutine);
            }

            _fillTransform.localScale = Vector3.zero;
        }

        public void SetScanProgress(float scanProgress)
        {
            if (gameObject.activeInHierarchy)
            {
                scanProgress = Mathf.Clamp01(scanProgress);

                if (_scanProgress != scanProgress)
                {
                    _scanProgress = scanProgress;

                    _fillRoutineTimer = 0.0f;
                    _fillRoutineStartValue = _displayedProgress;

                    if (_fillRoutine == null)
                    {
                        _fillRoutine = StartCoroutine(FillRoutine());
                    }
                }
            }
        }

        private IEnumerator FillRoutine()
        {
            //Change the scale of the inner mesh over time to correspond with scan progress
            while (_fillRoutineTimer <= _fillChangeTime)
            {
                _fillRoutineTimer += Time.deltaTime;
                float ratio = Mathf.Clamp01(_fillRoutineTimer / _fillChangeTime);

                _displayedProgress = Mathf.Lerp(_fillRoutineStartValue, _scanProgress, ratio);
                _fillTransform.localScale = Vector3.Lerp(_fillRoutineStartValue * Vector3.one, _scanProgress * Vector3.one, ratio);

                yield return null;
            }

            _fillRoutine = null;

            //If animation is done and completely filled, activate effects and animation
            if (_displayedProgress == 1)
            {
                _completeParticles.Play();
                _animator.SetTrigger(_completeAnimationTrigger);
            }
        }
    }
}
