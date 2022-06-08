
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Common.Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Turbines
{
    public class TurbineDock : MonoBehaviour
    {
        /// <summary>
        /// The power this dock is receiving from the turbine
        /// </summary>
        public float Power => _isOccupied ? _dockedTurbine.PowerSourceOutput : 0f;

        /// <summary>
        /// The occupied state of this dock
        /// </summary>
        public bool Occupied => _isOccupied;

        [SerializeField]
        [Tooltip("The colour of the dock when a turbine is nearby.")]
        Color _closebyColor;
        [SerializeField]
        [Tooltip("The colour of the dock when no turbine is in the dock.")]
        Color _emptyColor;
        [SerializeField]
        [Tooltip("The colour of the dock when a turbine is docked.")]
        Color _occupiedColor;

        [SerializeField]
        [Tooltip("The transform defining the turbine docked position.")]
        Transform _dockingPoint;

        [SerializeField]
        [Tooltip("The object that lights up to indicate the state of the dock.")]
        RendererCollectionHelper _activationRingRendererHelper;
        [SerializeField]
        [Tooltip("The glow that lights up once the turbine has docked.")]
        Renderer _activationGlow;

        [Header("Cable")]
        [SerializeField] TurbineCable _turbineCable;

        /// <summary>
        /// The world space position for docking the turbine
        /// </summary>
        public Vector3 DockingPoint => _dockingPoint.position;

        TurbineController _dockedTurbine;
        bool _isOccupied;

        private void Start()
        {
            _isOccupied = false;
            _activationGlow.enabled = false;
            _activationRingRendererHelper.SetColor(_emptyColor);
        }

        private void Update()
        {
            if (_isOccupied)
            {
                UpdateCableMaterial();
            }
        }

        /// <summary>
        /// Update the speed of the pulse animation on the connected cable
        /// </summary>
        private void UpdateCableMaterial()
        {
            _turbineCable.SetPower(_isOccupied ? Power : 0);
        }

        /// <summary>
        /// Connect the turbine controller and set up this dock as occupied
        /// </summary>
        public void ConnectToDock(bool connect, Transform turbine)
        {
            _isOccupied = connect;
            _activationRingRendererHelper.SetColor(_isOccupied ? _occupiedColor : _emptyColor);
            _activationGlow.enabled = _isOccupied;
            _dockedTurbine = _isOccupied ? turbine.GetComponent<TurbineController>() : null;
            UpdateCableMaterial();
        }

        /// <summary>
        /// Light up the docking indicator when a turbine is close enough to dock
        /// </summary>
        /// <param name="isNear"></param>
        public void ShowNearDock(bool isNear)
        {
            _activationRingRendererHelper.SetColor(isNear ? _closebyColor : _emptyColor);
        }
    }
}