// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit.Data;
using UnityEngine;
using UnityEngine.UI;

namespace MSPlayground.Core.Data
{
    /// <summary>
    /// A data consumer to help theme the materials in maskable graphics (Unity UI/Canvas).
    /// Some examples of maskable graphics include:
    ///     - UnityEngine.UI.Text
    ///     - UnityEngine.UI.Image
    ///     - UnityEngine.UI.RawImage
    ///     - Microsoft.MixedReality.GraphicsTools.CanvasElementRoundedRect
    ///     - Microsoft.MixedReality.GraphicsTools.CanvasElementBeveledRect
    /// 
    /// This is based off of {Microsoft.MixedReality.Toolkit.Data.DataConsumerMaterial} which
    /// themes the materials in renderers.
    /// </summary>
    public class DataConsumerMaskableGraphicMaterial : DataConsumerThemableBase<Material>
    {
        [Serializable]
        private struct ValueToMaterial
        {
            /// <summary>
            /// Value from the data source to be mapped to an object.
            /// </summary>
            public string Value;

            /// <summary>
            /// Material that this value maps to.
            /// </summary>
            public Material Material;
        }

        [Tooltip("(Optional) List of <key,Material> mappings where a list index or a string key can be used to identify the desired Material to use. Note: The key can be left blank or used as a description if only a list index will be used.")]
        [SerializeField]
        private ValueToMaterial[] _materialLookup;

        [Tooltip("(Optional) Explicit list of MaskableGraphic components that should be modified. If none specified, then all renderers found are considered.")]
        [SerializeField]
        private MaskableGraphic[] _maskableGraphicsToModify;

        
        private void Awake()
        {
            ConfigureFromBindingProfile(DataSourceTypes, DataBindingProfiles);
        }

        /// </inheritdoc/>
        protected override Type[] GetComponentTypes()
        {
            Type[] types = { typeof(MaskableGraphic) };
            return types;
        }

        /// <inheritdoc/>
        protected override Material GetObjectByIndex(int n)
        {
            if (n < _materialLookup.Length)
            {
                return _materialLookup[n].Material;
            }
            else
            {
                return null;
            }
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
        protected override Material GetObjectByKey(string keyValue)
        {
            foreach (ValueToMaterial valueToMaterial in _materialLookup)
            {
                if (keyValue == valueToMaterial.Value)
                {
                    return valueToMaterial.Material;
                }
            }

            return null;
        }

        /// </inheritdoc/>
        protected override void SetObject(Component component, object inValue, Material colorToSet)
        {
            MaskableGraphic graphic = component as MaskableGraphic;
            if (graphic)
            {
                graphic.material = colorToSet;
            }
        }
    }
}
