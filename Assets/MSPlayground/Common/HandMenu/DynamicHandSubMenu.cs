// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Data;
using Microsoft.MixedReality.Toolkit.UX;
using MSPlayground.Core;
using MSPlayground.Scenarios.Turbines;
using UnityEngine;
using UnityEngine.Events;

namespace MSPlayground.Common
{
    /// <summary>
    /// Dynamic sub menu found within the hand menu. This sub menu can have buttons added and removed dynamically
    /// </summary>
    public class DynamicHandSubMenu : BaseHandSubMenu
    {
        [SerializeField] private GameObject _buttonTemplate;
        private List<GameObject> _buttons = new List<GameObject>();

        private void Start()
        {
            _buttonTemplate.SetActive(false);
        }

        /// <summary>
        /// Toggles the state of this gameobject active or not
        /// If there are no buttons registered to the sub menu, the menu will stay deactivated
        /// </summary>
        public override void ToggleActive()
        {
            if (gameObject.activeSelf == false && _buttons.Count == 0)
            {
                Debug.LogError("Cannot toggle on the hand sub menu when there are no buttons registered", this.gameObject);
                return;
            }
            
            base.ToggleActive();
        }

        /// <summary>
        /// Creates a new gameobject instance with button configurations
        /// </summary>
        /// <param name="buttonName"></param>
        /// <param name="locId"></param>
        /// <param name="onPressCallback"></param>
        /// <returns></returns>
        public GameObject CreateButton(string buttonName, string locId, UnityAction onPressCallback)
        {
            GameObject instance = GameObject.Instantiate(_buttonTemplate, _buttonTemplate.transform.parent);
            instance.name = buttonName;

            var dataSource = instance.GetComponent<DataSourceGODictionary>();
            dataSource.DataChangeSetBegin();
            dataSource.SetValue("text", locId);
            dataSource.DataChangeSetEnd();

            var button = instance.GetComponentInChildren<PressableButton>();
            button.OnClicked.AddListener(onPressCallback);

            _buttons.Add(instance);
            instance.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Removes button given the name that was added using CreateButton()
        /// </summary>
        /// <param name="buttonName"></param>
        public void DestroyButton(string buttonName)
        {
            foreach (var button in _buttons)
            {
                if (button != null && button.name.Equals(buttonName, StringComparison.Ordinal))
                {
                    GameObject.Destroy(button);
                    break;
                }   
            }

            if (_buttons.Count == 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}