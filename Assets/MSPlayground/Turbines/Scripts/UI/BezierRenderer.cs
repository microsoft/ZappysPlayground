// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;

namespace MSPlayground.Common.UI
{
    /// <summary>
    /// Updates a LineRenderer component to display as a bezier line
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class BezierRenderer : MonoBehaviour
    {
        [SerializeField] private float _widthModifier = 1.0f;
        [SerializeField] private int _positionCount = 10;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _start;
        [SerializeField] private Transform _controlPoint1;
        [SerializeField] private Transform _controlPoint2;
        [SerializeField] private Transform _end;
        [SerializeField] private bool _executeOnUpdate = true;
        [SerializeField] private Material _vrLineMaterial;

        private void Start()
        {
#if VRBUILD
            if (_vrLineMaterial!=null)
            {
                _lineRenderer.material = _vrLineMaterial;
            }
#endif
        }

        /// <summary>
        /// Starting point transform
        /// </summary>
        public Transform StartTransform
        {
            get { return _start; }
            set { _start = value; }
        }

        /// <summary>
        /// End point transform
        /// </summary>
        public Transform End
        {
            get { return _end; }
            set { _end = value; }
        }

        private void Reset()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }
        }

        private void OnValidate()
        {
            _positionCount = Mathf.Max(_positionCount, 2);
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                if (_start != null)
                {
                    Destroy(_start.gameObject);   
                }

                if (_end != null)
                {
             
                    Destroy(_end.gameObject);   
                }

                if (_controlPoint1 != null)
                {
                    Destroy(_controlPoint1.gameObject);   
                }

                if (_controlPoint2 != null)
                {
                    Destroy(_controlPoint2.gameObject);   
                }   
            }
        }

        private void OnEnable()
        {
            Execute();
        }

        private void Update()
        {
            if (_executeOnUpdate || Application.isPlaying == false)
            {
                Execute();
            }
        }

        [ContextMenu(nameof(Execute))]
        private void Execute()
        {
            if (_start == null || _end == null)
            {
                return;
            }

            _lineRenderer.widthMultiplier = _widthModifier;

            if (_controlPoint1 != null)
            {
                if (_controlPoint2 != null)
                {
                    _lineRenderer.positionCount = _positionCount;
                    DrawCubicBezierCurve(StartTransform.position, _controlPoint1.position, _controlPoint2.position,
                        End.position);
                }
                else
                {
                    _lineRenderer.positionCount = _positionCount;
                    DrawQuadraticBezierCurve(StartTransform.position, _controlPoint1.position, End.position);
                }
            }
            else
            {
                _lineRenderer.positionCount = 2;
                _lineRenderer.SetPositions(new[] {StartTransform.position, End.position});
            }
        }

        private void DrawQuadraticBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2)
        {
            float t = 0f;
            Vector3 B = new Vector3(0, 0, 0);
            for (int i = 0; i < _lineRenderer.positionCount; i++)
            {
                B = (1 - t) * (1 - t) * point0 + 2 * (1 - t) * t * point1 + t * t * point2;
                _lineRenderer.SetPosition(i, B);
                t += (1 / (float) _lineRenderer.positionCount);
            }
        }

        private void DrawCubicBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2, Vector3 point3)
        {
            float t = 0f;
            Vector3 B = new Vector3(0, 0, 0);
            for (int i = 0; i < _lineRenderer.positionCount; i++)
            {
                B = (1 - t) * (1 - t) * (1 - t) * point0 + 3 * (1 - t) * (1 - t) *
                    t * point1 + 3 * (1 - t) * t * t * point2 + t * t * t * point3;

                _lineRenderer.SetPosition(i, B);
                t += (1 / (float) _lineRenderer.positionCount);
            }
            // When there are multiple control points ensure that the very last line renderer is
            // still positioned to the configured end point.
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, point3);
        }
    }
}