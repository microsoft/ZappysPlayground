// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Data;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// A data source that notifies child data consumers when themeable components
    /// should be updated with the current UIThemeProfile.
    /// </summary>
    /// <remarks>
    /// All themeable components must be a child of the GameObject where this data source is attached in
    /// order to be properly notified. Parent/ancestor level does not matter.
    /// </remarks>
    [Serializable]
    public class DataSourceUITheme : DataSourceGOBase
    {
        [SerializeField] protected UIThemeProfile[] _availableThemes;
        [SerializeField] protected int _currentTheme;
        public UIThemeProfile[] AvailableThemes => _availableThemes;

        /// The Reset() function is called whenever this script is first attached to a new GameObject
        /// or when manually clicked in the component settings menu.
        private void Reset()
        {
            dataSourceType = "theme";      // Make default "theme" to differentiate from "data" data source types
        }
        
        protected override void InitializeDataSource()
        {
            ChangeTheme(_currentTheme);
        }

        /// <summary>
        /// Set the UI theme by the index from the availableThemes array
        /// </summary>
        /// <param name="themeIndex"></param>
        public void ChangeTheme(int themeIndex)
        {
            if (_availableThemes != null && themeIndex >= 0 && themeIndex < _availableThemes.Length)
            {
                _currentTheme = themeIndex;

                DataSourceReflection dataSource = DataSource as DataSourceReflection;

                dataSource.SetDataSourceObject(_availableThemes[_currentTheme]);
                DataSource.NotifyAllChanged();
            }
        }
        
        public void NextTheme()
        {
            ChangeTheme((_currentTheme + 1) % _availableThemes.Length);
        }

        private void OnValidate()
        {
            ChangeTheme(_currentTheme);
        }

        public override IDataSource AllocateDataSource()
        {
            if (_availableThemes != null && _availableThemes.Length > 0)
            {
                return new DataSourceReflection(_availableThemes[_currentTheme]);
            }
            return null;
        }
    }
}
