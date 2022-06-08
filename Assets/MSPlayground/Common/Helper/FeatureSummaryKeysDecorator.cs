
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using MSPlayground.Core.UI;
using MSPlayground.Scenarios.Turbines;
using UnityEngine;

namespace MSPlayground.Common.Helper
{
    /// <summary>
    /// A decorator to set the feature set of a FeatureSummary from public static references
    /// </summary>
    public class FeatureSummaryKeysDecorator : MonoBehaviour
    {
        [SerializeField] private FeatureSummary _featureSummary = null;

        private void Reset()
        {
            if (!_featureSummary)
            {
                _featureSummary = GetComponent<FeatureSummary>();
            }
        }

        private void Start()
        {
            if (_featureSummary)
            {
                // TODO: Right now we just get the platform-specific feature set for
                // the Hub and Turbines experiences (combined). If in the future this project contains
                // more experiences/scenes, we may want to consider changing the visible feature set
                // depending on the currently active scene.
                _featureSummary.Initialize(TurbineScenarioResources.FEATURE_KEYS);
            }
        }
    }
}
