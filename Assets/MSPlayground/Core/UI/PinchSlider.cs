
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;

namespace MSPlayground.Core.UI
{
    public class PinchSlider : Slider
    {
        protected override void Start()
        {
            base.Start();
            UpdateUI();
            OnValueUpdated.AddListener(UpdateUI);
        }

        private void UpdateUI(SliderEventData arg0)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            HandleTransform.position = Vector3.Lerp(SliderStart.position, SliderEnd.position, SliderValue);
        }
    }
}