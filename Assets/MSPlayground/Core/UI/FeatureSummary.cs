
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Data;
using Microsoft.MixedReality.Toolkit.UX;
using MSPlayground.Common.Helper;
using MSPlayground.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// An auto-populated feature summary with a draggable scroll rect.
    /// </summary>
    public class FeatureSummary : MonoBehaviour
    {
        private const string KEYPATH_ICON = "tooltip_icon";
        private const string KEYPATH_TITLE = "tooltip_title";
        private const string KEYPATH_DESC = "tooltip_desc";

        private const string LOC_PATH_PREFIX_TITLE = "Tooltips/title_";
        private const string LOC_PATH_PREFIX_DESC = "Tooltips/desc_";

        [SerializeField] private GameObject _cellContainer = null;
        [SerializeField] private GameObject _cellPrefab = null;
        [SerializeField] private string[] _featureKeys = null;
        [SerializeField] private ScrollRect _scrollRect = null;

        [Tooltip("Used to show a fade on the side where user can scroll for more content (left/right)")]
        [SerializeField]
        private RectMask2D _scrollRectMask2D;

        private bool _initialized = false;

        /// <summary>
        /// Has the user started moving the scroll rect (using MRTK object manipulator
        /// restricted to move logic only)
        /// </summary>
        private bool _isManipulatingScroll = false;

        public event System.Action<string> OnButtonPressedEvent;

        private void Start()
        {
            if (_initialized == false && _featureKeys != null && _featureKeys.Length != 0)
            {
                Initialize(_featureKeys);
            }
        }

        /// <summary>
        /// Called from MRTK Object Manipulator
        /// </summary>
        public void SetIsManipulatingScroll(bool isManipulatingScroll)
        {
            _isManipulatingScroll = isManipulatingScroll;
        }

        /// <summary>
        /// Initialize UI with the restart callback and the featureKey set.
        /// </summary>
        /// <param name="featureKeys">String array of the features used</param>
        public void Initialize(string[] featureKeys)
        {
            _initialized = true;
            _featureKeys = featureKeys;
            PopulateCells();
            _scrollRect.horizontalNormalizedPosition = 0;
        }

        /// <summary>
        /// Populate the cell container with content of the configured features
        /// </summary>
        void PopulateCells()
        {
            foreach (string featureKey in _featureKeys)
            {
                GameObject cell = Instantiate(_cellPrefab, _cellContainer.transform);
                DataSourceGODictionary dataSource = cell.GetComponent<DataSourceGODictionary>();
                if (dataSource != null)
                {
                    dataSource.DataChangeSetBegin();
                    dataSource.SetValue(KEYPATH_ICON, featureKey);
                    dataSource.SetValue(KEYPATH_TITLE, $"{LOC_PATH_PREFIX_TITLE}{featureKey}");
                    dataSource.SetValue(KEYPATH_DESC, $"{LOC_PATH_PREFIX_DESC}{featureKey}");
                    dataSource.DataChangeSetEnd();
                }
            }

            this.StartCoroutine(Coroutines.WaitFrames(1, ()=> { _scrollRect.horizontalNormalizedPosition = 0; }));
            this.StartCoroutine(Coroutines.WaitFrames(2, UpdateScrollRectMaskFade));
        }

        /// <summary>
        /// Use late update to determine where the scroll rect should be faded along the edges.
        /// Need to wait at the end of update so that the horizontal normalized position is accurate.
        /// </summary>
        void LateUpdate()
        {
            if (_isManipulatingScroll)
            {
                UpdateScrollRectMaskFade();
            }
        }

        /// <summary>
        /// Fade the left side if it the scroll rect is not all the way to the left, otherwise
        /// fade to right side if it's not all the way to the right.
        /// If the rect is in the middle and there is more content on both sides then the fade is applied
        /// to both ends.
        /// Opportunity for visual polish here.
        /// </summary>
        void UpdateScrollRectMaskFade()
        {
            var normalizedX = _scrollRect.horizontalNormalizedPosition;
            Vector4 padding = Vector4.zero;
            if (normalizedX <= Mathf.Epsilon)
            {
                padding.x = -_scrollRectMask2D.softness.x;
            }
            else if (normalizedX >= (1f - Mathf.Epsilon))
            {
                padding.z = -_scrollRectMask2D.softness.x;
            }

            if (_scrollRectMask2D.padding != padding)
            {
                _scrollRectMask2D.padding = padding;
            }
        }

        /// <summary>
        /// Called by PressableButton.OnClicked unity event
        /// </summary>
        /// <param name="buttonID"></param>
        public void OnButtonPressed(string buttonID)
        {
            OnButtonPressedEvent?.Invoke(buttonID);
        }
    }
}
