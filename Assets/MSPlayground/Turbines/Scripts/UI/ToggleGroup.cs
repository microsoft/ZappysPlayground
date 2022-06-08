// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// Enforces a group of PressableButtons to only have one button toggled on
    /// </summary>
    public class ToggleGroup : MonoBehaviour
    {
        [SerializeField] private GameObject _toggleButtonPrefab;
        [SerializeField] private List<PressableButton> toggleButtonInstanceInstances;

        public List<PressableButton> ButtonInstances => toggleButtonInstanceInstances;

        private void Reset()
        {
            if (toggleButtonInstanceInstances == null || toggleButtonInstanceInstances.Count == 0)
            {
                FindButtonsInChildren();
            }
        }

        [ContextMenu(nameof(FindButtonsInChildren))]
        private void FindButtonsInChildren()
        {
            toggleButtonInstanceInstances = this.GetComponentsInChildren<PressableButton>(true).ToList();
        }

        private void Start()
        {
            if (_toggleButtonPrefab != null)
            {
                _toggleButtonPrefab.gameObject.SetActive(false);   
            }
            
            foreach (var button in toggleButtonInstanceInstances)
            {
                RegisterButtonToGroup(button);
            }
        }

        /// <summary>
        /// Creates a number of buttons
        /// </summary>
        /// <param name="count"></param>
        /// <param name="onCreationAction"></param>
        public void InstantiateButtons(int count, System.Action<int, PressableButton> onCreationAction)
        {
            for (int i = 0; i < count; ++i)
            {
                GameObject instance = GameObject.Instantiate(_toggleButtonPrefab, _toggleButtonPrefab.transform.parent);
                PressableButton pressableButton = instance.GetComponentInChildren<PressableButton>();
                toggleButtonInstanceInstances.Add(pressableButton);
                RegisterButtonToGroup(pressableButton);
                instance.SetActive(true);
                onCreationAction?.Invoke(i, pressableButton);
            }
        }

        private void RegisterButtonToGroup(PressableButton sourceButton)
        {
            sourceButton.OnClicked.AddListener(() =>
            {
                SetButtonToggled(sourceButton);
            });
        }

        /// <summary>
        /// Disables all buttons other than the source button to be toggled on
        /// </summary>
        /// <param name="sourceButton"></param>
        public void SetButtonToggled(PressableButton sourceButton)
        {
            foreach (var button in toggleButtonInstanceInstances)
            {
                if (button == sourceButton)
                {
                    continue;
                }

                button.ForceSetToggled(false);
            }
        }

        /// <summary>
        /// Disables all buttons other than the button at index in collection
        /// </summary>
        /// <param name="index"></param>
        public void SetButtonToggled(int index)
        {
            for (int i = 0; i < toggleButtonInstanceInstances.Count; ++i)
            {
                if (i == index)
                {
                    continue;
                }
                
                toggleButtonInstanceInstances[i].ForceSetToggled(false);
            }
        }

        /// <summary>
        /// Sets the state of all the toggle buttons to be false
        /// </summary>
        /// <param name="state"></param>
        public void ClearAllButtonToggleState()
        {
            foreach (var button in toggleButtonInstanceInstances)
            {
                button.ForceSetToggled(false);
            }
        }
    }   
}