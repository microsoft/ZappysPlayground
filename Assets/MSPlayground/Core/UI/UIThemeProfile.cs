// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// The UI theme profile allows users to customize their MSPlayground experience
    /// with different predetermined materials.
    ///
    /// This could also be extended to have customized sprites, fonts, and more.
    /// </summary>
    [CreateAssetMenu(fileName = "MRTK_UI_Theme", menuName = "MRTK/UI Theme")]
    public class UIThemeProfile : ScriptableObject
    {
        #region Embedded Types
        [Serializable]
        public struct CanvasMaterials
        {
            public Material InnerPlateMaterial;
        }

        [Serializable]
        public struct NonCanvasMaterials
        {
            public Material BackPlateMaterial;
        }
        #endregion
        
        [Header("Materials")]
        public CanvasMaterials MatsCanvas;
        public NonCanvasMaterials MatsNonCanvas;

        [Header("Colors")]
        #region Colors
        public Color ButtonColor_Yes;
        public Color ButtonColor_No;
        public Color InputFieldColor;
        public Color ToggleQuadColor;
        #endregion
    }
}
