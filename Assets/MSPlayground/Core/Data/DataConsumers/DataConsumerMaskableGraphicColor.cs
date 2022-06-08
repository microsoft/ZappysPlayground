// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit.Data;
using UnityEngine;
using UnityEngine.UI;

namespace MSPlayground.Core.Data
{
    /// <summary>
    /// A data consumer to help theme the colors of maskable graphics (Unity UI/Canvas).
    /// Some examples of maskable graphics include:
    ///     - UnityEngine.UI.Text
    ///     - UnityEngine.UI.Image
    ///     - UnityEngine.UI.RawImage
    ///     - Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect
    ///     - Microsoft.MixedReality.GraphicsTools.CanvasElementBeveledRect
    /// 
    /// This is based off of {Microsoft.MixedReality.Toolkit.Data.DataConsumerMaterial} which
    /// themes the materials in renderers. We use the extension class {DataConsumerThemableBaseStruct<T>}
    /// which supports structs like Color.
    /// </summary>
    public class DataConsumerMaskableGraphicColor : DataConsumerThemableBaseStruct<Color>
    {
        [Serializable]
        private struct ValueToColor
        {
            /// <summary>
            /// Value from the data source to be mapped to an object.
            /// </summary>
            public string Value;

            /// <summary>
            /// Color that this value maps to.
            /// </summary>
            public Color Color;
        }

        [Tooltip("(Optional) List of <key,Color> mappings where a list index or a string key can be used to identify the desired Material to use. Note: The key can be left blank or used as a description if only a list index will be used.")]
        [SerializeField]
        private ValueToColor[] _colorLookup;

        [Tooltip("(Optional) Explicit list of MaskableGraphic components that should be modified. If none specified, then all renderers found are considered.")]
        [SerializeField]
        private MaskableGraphic[] _maskableGraphicsToModify;

        /// </inheritdoc/>
        protected override Type[] GetComponentTypes()
        {
            Type[] types = { typeof(MaskableGraphic) };
            return types;
        }

        /// <inheritdoc/>
        protected override Color GetObjectByIndex(int n)
        {
            if (n < _colorLookup.Length)
            {
                return _colorLookup[n].Color;
            }

            return default;
        }

        /// </inheritdoc/>
        protected override bool DoesManageSpecificComponents()
        {
            return _maskableGraphicsToModify.Length > 0;
        }

        /// </inheritdoc/>
        protected override void AttachDataConsumer()
        {
            foreach (Component component in _maskableGraphicsToModify)
            {
                if (ComponentMeetsAllQualifications(component))
                {
                    _componentsToManage.Add(component);
                }
            }
        }

        /// </inheritdoc/>
        protected override Color GetObjectByKey(string keyValue)
        {
            foreach (ValueToColor valueToColor in _colorLookup)
            {
                if (keyValue == valueToColor.Value)
                {
                    return valueToColor.Color;
                }
            }

            return default;
        }

        /// </inheritdoc/>
        protected override void SetObject(Component component, object inValue, Color colorToSet)
        {
            MaskableGraphic graphic = component as MaskableGraphic;

            if (graphic)
            {
                graphic.color = colorToSet;
            }
        }
    }
}
