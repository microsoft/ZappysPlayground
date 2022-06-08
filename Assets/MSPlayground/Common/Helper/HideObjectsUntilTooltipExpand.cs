
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using MSPlayground.Core.UI;
using UnityEngine;

namespace MSPlayground.Common
{
    /// <summary>
    /// Helper to hide gameobjects until the specified tooltip has already been expanded.
    /// </summary>
    public class HideObjectsUntilTooltipExpand : MonoBehaviour
    {
        [SerializeField] private TooltipBase _tooltip = null;
        [Min(0)]
        [Tooltip("How long to wait after the tooltip has been expanded before activating the objects")]
        [SerializeField] private float _delayTimeAfterExpanded = 0f;

        [SerializeField] private GameObject[] _objectsToActivate = null;

        private void Start()
        {
            // Deactivate in case they are still active
            if (_objectsToActivate != null)
            {
                foreach (var obj in _objectsToActivate)
                {
                    obj.SetActive(false);
                }
            }

            _tooltip.OnTooltipExpanded += ActivateAfterExpand;
        }

        private void OnDestroy()
        {
            _tooltip.OnTooltipExpanded -= ActivateAfterExpand;
        }

        private void ActivateAfterExpand() => StartCoroutine(ActivateAfterExpandCoroutine());

        private IEnumerator ActivateAfterExpandCoroutine()
        {
            yield return new WaitForSeconds(_delayTimeAfterExpanded);
            foreach (var obj in _objectsToActivate)
            {
                obj.SetActive(true);
            }
        }
    }
}
