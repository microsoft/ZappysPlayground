// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.MixedReality.Toolkit.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

namespace MSPlayground.Core.Data
{
    /// <summary>
    /// A data consumer that can embed localized data into text components. This
    /// depends on the Unity Localization package. This script is a variant of the MRTK
    /// package's Microsoft.MixedReality.Toolkit.Data.DataConsumerText.
    ///
    /// Currently supported are:
    ///     TextMeshPro (via TextMeshProUGUI)
    ///     TextMesh (via UnityEngine.UI.Text)
    ///     UnityEngine.UI.Text
    ///
    /// One of these data consumer components can manage any number
    /// of text components so long as they are being populated by
    /// the same data source.
    ///
    /// Note that a single text message can support any number of variable phrases. To make this
    /// more efficient, all data changes within a single data set are cached and then applied
    /// at once at the DataSetEnd() method call.
    ///
    /// This consumer also supports "local variables" from the Unity Localization package, if there is a
    /// valid {DataSourceLocalizationVariables} in the object hierarchy.
    /// </summary>
    public class DataConsumerLocalizedText : DataConsumerGOBase
    {
        protected class ComponentInformation
        {
            private class TextVariableInformation
            {
                public string DataBindVariable { get; set; }
                public string ResolvedKeyPath { get; set; }
                public string LocalKeyPath { get; set; }
                public object CurrentValue { get; set; } // Current Value => loc path
            }

            private TextMeshProUGUI _textMeshProUGUIComponent = null;
            private UnityEngine.UI.Text _textComponent = null;
            private TextMeshPro _textMeshProComponent = null;
            private string _originalTemplateValue;
            private bool _hasChanged;
            private LocalizedString _stringReference = new LocalizedString();

            /* Used to find all Components which are affected by a change in a specific keypath */
            private Dictionary<string, TextVariableInformation> _keyPathToVariableInformation = new Dictionary<string, TextVariableInformation>();

            public ComponentInformation(Component theComponent, string templateValue = null, DataSourceLocalizationVariables locVariablesDataSource = null)
            {
                switch (theComponent)
                {
                    case TextMeshProUGUI tmpUGUIComponent:
                        _textMeshProUGUIComponent = tmpUGUIComponent;
                        break;
                    case UnityEngine.UI.Text unityUITextComponent:
                        _textComponent = unityUITextComponent;
                        break;
                    case TextMeshPro tmpComponent:
                        _textMeshProComponent = tmpComponent;
                        break;
                }

                _originalTemplateValue = templateValue ?? GetValue();
                // Add any local variables to the string reference.
                if (locVariablesDataSource != null)
                {
                    foreach (var locVariable in locVariablesDataSource.LocalVariablesByID)
                    {
                        _stringReference.Add(locVariable);
                    }
                }

                RegisterLocalizationChangeHandler();
            }

            public string GetTemplate()
            {
                return _originalTemplateValue;
            }

            public string GetValue()
            {
                if (_textMeshProUGUIComponent != null)
                {
                    return _textMeshProUGUIComponent.text;
                }
                else if (_textComponent != null)
                {
                    return _textComponent.text;
                }
                else if (_textMeshProComponent != null)
                {
                    return _textMeshProComponent.text;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Get the localized string value of the given loc path and set the value 
            /// </summary>
            /// <param name="newValue">Localization path in format $TABLE_ID/$ENTRY_ID</param>
            public void LocalizeValue(string newValue)
            {
                string value = newValue;

                if (!string.IsNullOrEmpty(value) && value != _originalTemplateValue)
                {
                    // First component is the Table Reference, second component is the Entry reference
                    string[] locPathComponents = newValue.Split('/');
                    if (locPathComponents.Length != 2)
                    {
                        Debug.LogError($"[{this.GetType().ToString()}] Localization path needs to be formatted as" +
                                       "{$TABLE_ID/$ENTRY_ID}.\n" +
                                       $"Instead, the received localization path was: {newValue}");
                        return;
                    }

                    _stringReference.TableReference = locPathComponents[0];
                    _stringReference.TableEntryReference = locPathComponents[1];
                    _stringReference.RefreshString();

                    value = _stringReference.GetLocalizedString();
                }

                SetValue(value);
            }

            public void SetValue(string newValue)
            {
                if (_textMeshProUGUIComponent != null)
                {
                    _textMeshProUGUIComponent.text = newValue;
                }
                else if (_textComponent != null)
                {
                    _textComponent.text = newValue;
                }
                else if (_textMeshProComponent != null)
                {
                    _textMeshProComponent.text = newValue;
                }
            }

            public void ProcessDataChanged(IDataSource dataSource, string resolvedKeyPath, string localKeyPath, object value, DataChangeType dataChangeType)
            {
                if (_keyPathToVariableInformation.ContainsKey(resolvedKeyPath))
                {
                    _keyPathToVariableInformation[resolvedKeyPath].CurrentValue = value;
                    _hasChanged = true;
                }
            }

            public void ApplyAllChanges()
            {
                if (_hasChanged)
                {
                    string textToChange = _originalTemplateValue;

                    foreach (TextVariableInformation tvi in _keyPathToVariableInformation.Values)
                    {
                        if (tvi.CurrentValue != null)
                        {
                            textToChange = textToChange.Replace(tvi.DataBindVariable, tvi.CurrentValue.ToString());
                        }
                    }

                    LocalizeValue(textToChange);

                    _hasChanged = false;
                }
            }

            public void Detach()
            {
                SetValue(_originalTemplateValue);

                // clear old keypaths when object is going back to re-use pool.
                _keyPathToVariableInformation.Clear();
                ClearLocalizationChangeHandler();
            }

            public bool AddKeyPathListener(string resolvedKeyPath, string localKeyPath, string entireVariable)
            {
                if (!_keyPathToVariableInformation.ContainsKey(resolvedKeyPath))
                {
                    TextVariableInformation textVariableInfo = new TextVariableInformation
                    {
                        DataBindVariable = entireVariable,
                        CurrentValue = localKeyPath,
                        ResolvedKeyPath = resolvedKeyPath,
                        LocalKeyPath = localKeyPath
                    };

                    _keyPathToVariableInformation[resolvedKeyPath] = textVariableInfo;
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public void RegisterLocalizationChangeHandler()
            {
                _stringReference.StringChanged += OnLocalizationUpdate;
            }

            private void ClearLocalizationChangeHandler()
            {
                _stringReference.StringChanged -= OnLocalizationUpdate;
            }

            private void OnLocalizationUpdate(string value)
            {
                SetValue(value);
            }
        } /* End of protected class ComponentInformation */

        [FormerlySerializedAs("manageChildren")]
        [Tooltip("Manage sprites in child game objects as well as this one.")]
        [SerializeField]
        private bool _manageChildren = true;

        protected const string _dataBindSpecifierBegin = @"{{";
        protected const string _dataBindSpecifierEnd = @"}}";
        protected Regex _variableRegex = new Regex(_dataBindSpecifierBegin + @"\s*([a-zA-Z0-9\[\]\-._]+)\s*" + _dataBindSpecifierEnd);

        /* Used to find all keypaths that influence a specific component to make sure all variable data is updated when any one element changes */
        protected Dictionary<Component, ComponentInformation> _componentInfoLookup = new Dictionary<Component, ComponentInformation>();
        private HashSet<string> _localKeypaths = new HashSet<string>();

        /// </inheritdoc/>
        protected override Type[] GetComponentTypes()
        {
            Type[] types = { typeof(TextMeshProUGUI), typeof(UnityEngine.UI.Text), typeof(TextMeshPro) };
            return types;
        }

        /// </inheritdoc/>
        protected override bool ManageChildren()
        {
            return _manageChildren;
        }

        /// </inheritdoc/>
        public override void DataChangeSetEnd(IDataSource dataSource)
        {
            foreach (ComponentInformation componentInfo in _componentInfoLookup.Values)
            {
                componentInfo.ApplyAllChanges();
            }
        }

        /// </inheritdoc/>
        protected override void ProcessDataChanged(IDataSource dataSource, string resolvedKeyPath, string localKeyPath, object value, DataChangeType dataChangeType)
        {
            foreach (ComponentInformation componentInfo in _componentInfoLookup.Values)
            {
                componentInfo.ProcessDataChanged(dataSource, resolvedKeyPath, localKeyPath, value, dataChangeType);
            }
        }

        /// </inheritdoc/>
        protected override void DetachDataConsumer()
        {
            foreach (ComponentInformation ci in _componentInfoLookup.Values)
            {
                ci.Detach();
            }
            _componentInfoLookup.Clear();
            _localKeypaths.Clear();
        }

        /// </inheritdoc/>
        protected override void AddVariableKeyPathsForComponent(Component component)
        {
            if (!_componentInfoLookup.ContainsKey(component))
            {
                // Check if the template matches the variable regex before initializing a new ComponentInformation
                string template = GetTemplate(component);
                MatchCollection matches = GetVariableMatchingRegex().Matches(template);
                if (matches.Count > 0)
                {
                    // Check for a local variables data source (can be null)
                    DataSourceLocalizationVariables localVariablesDataSource = GetBestDataSource(DataSourceLocalizationVariables.DATA_SOURCE_TYPE) as DataSourceLocalizationVariables;

                    ComponentInformation componentInfo = new ComponentInformation(component, templateValue: template, localVariablesDataSource);
                    _componentInfoLookup[component] = componentInfo;

                    foreach (Match match in matches)
                    {
                        string localKeyPath = match.Groups[1].Value;

                        IDataSource dataSource = GetBestDataSourceForKeyPath(ResolvedKeyPathPrefix, localKeyPath, out string resolvedKeyPath);
                        if (dataSource != null)
                        {
                            componentInfo.AddKeyPathListener(resolvedKeyPath, localKeyPath, match.Value);

                            if (_localKeypaths.Add(localKeyPath))
                            {
                                // if first occurrence, then add keypath listener on data source
                                AddKeyPathListener(localKeyPath);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("No data source found for key path: " + localKeyPath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the template that is baked into the text component
        /// </summary>
        private static string GetTemplate(Component textComponent)
        {
            switch (textComponent)
            {
                case TextMeshProUGUI tmpUGUIComponent:
                    return tmpUGUIComponent.text;
                case UnityEngine.UI.Text unityUITextComponent:
                    return unityUITextComponent.text;
                case TextMeshPro tmpComponent:
                    return tmpComponent.text;
            }
            return String.Empty;
        }

        /// <summary>
        /// Returns a preallocated regex object to use for identifying variable key paths in textual strings.
        /// </summary>
        ///
        /// <remarks>
        /// This is provided to reduce the number of identical regex objects when searching for data embedded variables.</remarks>
        /// <returns>The instantiated Regex for matching variables.</returns>
        protected Regex GetVariableMatchingRegex()
        {
            return _variableRegex;
        }
    }
}
