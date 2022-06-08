// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Data;

namespace MSPlayground.Core.Data
{
    /// <summary>
    /// Given a value from a data source, use that value to look up the correct Sprite
    /// specified in the Unity inspector list. That Sprite is then associated
    /// with an image being managed by this object.
    ///
    /// This is based off of {Microsoft.MixedReality.Toolkit.Data.DataConsumerSpriteLookup} which
    /// supports sprite lookup in sprite renderers rather than UGUI images.
    /// </summary>
    ///
    [Serializable]
    public class DataConsumerImageSpriteLookup : DataConsumerGOBase
    {
        [Serializable]
        internal struct ValueToSpriteInfo
        {
            /// <summary>
            /// Value from the data source to be mapped to a sprite.
            /// </summary>
            public string Value;

            /// <summary>
            /// Sprite to map to for this value.
            /// </summary>
            public Sprite Sprite;
        }

        [Tooltip("Manage sprites in child game objects as well as this one.")]
        [SerializeField]
        private bool _manageChildren = true;

        [Tooltip("Key path within the data source for the value used for sprite lookup.")]
        [SerializeField]
        private string _keyPath;

        [Tooltip("List of value-to-sprite mappings.")]
        [SerializeField]
        private ValueToSpriteInfo[] _valueToSpriteLookup;

        protected Image _image;

        /// </inheritdoc/>
        protected override Type[] GetComponentTypes()
        {
            Type[] types = { typeof(Image) };
            return types;
        }

        /// </inheritdoc/>
        protected override bool ManageChildren()
        {
            return _manageChildren;
        }

        /// </inheritdoc/>
        protected override void AddVariableKeyPathsForComponent(Component component)
        {
            _image = component as Image;
            AddKeyPathListener(_keyPath);
        }

        /// </inheritdoc/>
        protected override void ProcessDataChanged(IDataSource dataSource, string resolvedKeyPath, string localKeyPath, object value, DataChangeType dataChangeType)
        {
            if (localKeyPath == _keyPath && value != null)
            {
                string strValue = value.ToString();

                foreach (ValueToSpriteInfo v2si in _valueToSpriteLookup)
                {
                    if (strValue == v2si.Value)
                    {
                        _image.sprite = v2si.Sprite;
                        break;
                    }
                }
            }
        }
    }
}
