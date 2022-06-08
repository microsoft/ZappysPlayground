
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Turbines
{
    public class TurbinesVRRoom : MonoBehaviour
    {
        [SerializeField] Transform _turbine0;
        public Transform Turbine0 => _turbine0;

        [SerializeField] Transform _turbine1;
        public Transform Turbine1 => _turbine1;

        [SerializeField] Transform _turbine2;
        public Transform Turbine2 => _turbine2;

        [SerializeField] Transform _turbine3;
        public Transform Turbine3 => _turbine3;

        [SerializeField] Transform _turbine4;
        public Transform Turbine4 => _turbine4;

        [SerializeField] Transform _robot;
        public Transform Robot => _robot;

        [SerializeField] Transform _window;
        public Transform Window => _window;

        [SerializeField] Transform _windowGuide;
        public Transform WindowGuide => _windowGuide;

        [SerializeField] Transform _windfarm;
        public Transform Windfarm => _windfarm;

        [SerializeField] Transform _windfarmGuide;
        public Transform WindfarmGuide => _windfarmGuide;

        [SerializeField] List<Renderer> _wallRenderers = new List<Renderer>();
        public List<Renderer> WallRenderers => _wallRenderers;
    }
}
