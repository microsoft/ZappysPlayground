
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Data;
using MSPlayground.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace MSPlayground.Hub
{
    /// <summary>
    /// Handle the UI panel with the name entry input field
    /// </summary>
    public class NameEntryController : MonoBehaviour
    {
        public const string PLAYER_PREF_USERNAME = "username";
        [Tooltip("Need to save default so we can revert the loc variable reference as well")]
        public const string DEFAULT_USERNAME = "User";

        [SerializeField] DataSourceGODictionary _locDataSource = null;
        [SerializeField] TMP_InputField _inputField = null;
        [SerializeField] GameObject _resetButtonObject = null;

        private StringVariable _nameVariable = null;
        private string _nameOnSpawn = null;
        private bool _isReturningUser => PlayerPrefs.HasKey(PLAYER_PREF_USERNAME);
            
        void Start()
        {
            // Get username variable in loc settings
            var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();
            _nameVariable = source["global"]["username"] as StringVariable;
            
            SetupUI();
        }

        /// <summary>
        /// There are some different UI states depending on if the user is returning or new.
        /// Setup initial state of the UI
        /// </summary>
        private void SetupUI()
        {
            _locDataSource.DataChangeSetBegin();
            if (_isReturningUser)
            {
                _locDataSource.SetValue("text", "HubScenario/name_entry_prompt_returning");
                _locDataSource.SetValue("placeholder_text", "UI/username");
                _inputField.text = _nameVariable.Value;
            }
            else
            {
                _locDataSource.SetValue("text", "HubScenario/name_entry_prompt");
                _locDataSource.SetValue("placeholder_text", "HubScenario/name_entry_placeholder");
            }
            ChangeCallToAction(false);
            _locDataSource.DataChangeSetEnd();
            
            _nameOnSpawn = _inputField.text;
            _resetButtonObject.SetActive(false);
            _inputField.onValueChanged.AddListener(OnInputChange);
        }

        // On input field value change
        private void OnInputChange(string value)
        {
            // Once there has been a single input value change, change the call to action
            ChangeCallToAction(true, true);
            _resetButtonObject.SetActive(true);

            _inputField.onValueChanged.RemoveListener(OnInputChange);
        }

        /// <summary>
        /// Invoked when the submit button of the panel is clicked
        /// </summary>
        public void OnSubmitButtonClick()
        {
            // only update the username variable if there has been a change
            if (!string.IsNullOrEmpty(_inputField.text) && _nameVariable.Value != _inputField.text)
            {
                _nameVariable.Value = _inputField.text;
                PlayerPrefs.SetString(PLAYER_PREF_USERNAME, _inputField.text);
                PlayerPrefs.Save();
            }
            UISystem.DespawnPanel(gameObject);
        }

        /// <summary>
        /// Invoked when the reset button of the panel is clicked ("x")
        /// </summary>
        public void OnResetButtonClick()
        {
            _inputField.text = _nameOnSpawn;
            ChangeCallToAction(false, true);
            _resetButtonObject.SetActive(false);

            _inputField.onValueChanged.AddListener(OnInputChange);
        }

        /// <summary>
        /// Change the call to action text
        /// </summary>
        /// <param name="isInputModified">Has the input been modified from cached value</param>
        /// <param name="isAtomicChange">Update immediately?</param>
        private void ChangeCallToAction(bool isInputModified, bool isAtomicChange = false)
        {
            string ctaLocKey = "HubScenario/name_entry_cta_next";
            if (!isInputModified && _isReturningUser)
            {
                ctaLocKey = "HubScenario/name_entry_cta_continue";
            }
            else if (!isInputModified && !_isReturningUser)
            {
                ctaLocKey = "HubScenario/name_entry_cta_skip";
            }

            _locDataSource.DataChangeSetBegin();
            _locDataSource.SetValue("call_to_action", ctaLocKey, isAtomicChange);
            _locDataSource.DataChangeSetEnd();
        }
        
        private void OnDestroy()
        {
            _inputField.onValueChanged.RemoveListener(OnInputChange);
        }
    }
}